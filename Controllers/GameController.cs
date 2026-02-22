using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FNaFle.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Reset time for session/guesses if needed
        private readonly TimeSpan resetTime = new TimeSpan(8, 0, 0);

        public GameController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Character GetTodayCharacter()
        {
            var today = DateTime.UtcNow.Date;
            var dailyEntry = _context.DailyGames.FirstOrDefault(x => x.Date == today);

            if (dailyEntry == null)
            {
                var randomCharacter = _context.Characters.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (randomCharacter == null) return null;

                dailyEntry = new DailyGame
                {
                    CharacterId = randomCharacter.Id,
                    Date = today
                };

                _context.DailyGames.Add(dailyEntry);
                _context.SaveChanges();
            }

            return _context.Characters.First(x => x.Id == dailyEntry.CharacterId);
        }

        [HttpGet]
        public async Task<IActionResult> Play()
        {
            var character = GetTodayCharacter();
            var user = await _userManager.GetUserAsync(User);
            UserProgress progress = null;

            if (user != null)
            {
                progress = await _context.UserProgress.FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (progress == null)
                {
                    progress = new UserProgress
                    {
                        UserId = user.Id,
                        LastGuessDate = DateTime.UtcNow.Date.AddDays(-1),
                        Streak = 0,
                        HasGuessedCorrectlyToday = false
                    };
                    _context.UserProgress.Add(progress);
                    await _context.SaveChangesAsync();
                }

                if (progress.LastGuessDate < DateTime.UtcNow.Date)
                {
                    if (progress.LastGuessDate < DateTime.UtcNow.Date.AddDays(-1))
                    {
                        progress.Streak = 0;
                    }

                    progress.HasGuessedCorrectlyToday = false;
                    _context.Update(progress);
                    await _context.SaveChangesAsync();
                }
            }

            // Load existing history from session if it exists
            var sessionData = HttpContext.Session.GetString("GuessHistory");
            if (!string.IsNullOrEmpty(sessionData))
            {
                ViewBag.History = JsonSerializer.Deserialize<List<Character>>(sessionData);
            }

            ViewBag.Character = character;
            ViewBag.Progress = progress;
            ViewBag.GuessedCorrectlyToday = progress?.HasGuessedCorrectlyToday ?? false;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Play(string guess)
        {
            var character = GetTodayCharacter();
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ViewBag.Error = "You must be logged in to play.";
                ViewBag.Character = character;
                return View();
            }

            var progress = await _context.UserProgress.FirstAsync(x => x.UserId == user.Id);

            if (progress.HasGuessedCorrectlyToday)
            {
                ViewBag.Message = "Tonight's challenge is already complete! Come back tomorrow.";
                ViewBag.Character = character;
                ViewBag.Progress = progress;
                ViewBag.GuessedCorrectlyToday = true;
                return View();
            }

            var guessedCharacter = _context.Characters
                .FirstOrDefault(x => x.Name.ToLower() == (guess ?? "").ToLower());

            if (guessedCharacter == null)
            {
                ViewBag.Error = "Character not found!";
                ViewBag.Character = character;
                ViewBag.Progress = progress;
                return View();
            }

            // --- HISTORY LOGIC ---
            var sessionKey = "GuessHistory";
            var sessionData = HttpContext.Session.GetString(sessionKey);
            List<Character> history = string.IsNullOrEmpty(sessionData)
                ? new List<Character>()
                : JsonSerializer.Deserialize<List<Character>>(sessionData);

            if (!history.Any(c => c.Id == guessedCharacter.Id))
            {
                history.Insert(0, guessedCharacter);
                HttpContext.Session.SetString(sessionKey, JsonSerializer.Serialize(history));
            }

            if (string.Equals(guess, character.Name, StringComparison.OrdinalIgnoreCase))
            {
                progress.HasGuessedCorrectlyToday = true;
                progress.Streak++;
                progress.LastGuessDate = DateTime.UtcNow.Date;
                ViewBag.Message = "🎉 Correct! Come back tomorrow! :D";
            }
            else
            {
                ViewBag.Message = "Wrong, try again?";
            }

            _context.Update(progress);
            await _context.SaveChangesAsync();

            ViewBag.Character = character;
            ViewBag.Progress = progress;
            ViewBag.GuessedCorrectlyToday = progress.HasGuessedCorrectlyToday;
            ViewBag.History = history;

            return View();
        }
    }
}