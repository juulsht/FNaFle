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
                ViewBag.Message = "🎉 Correct! Come back tomorrow! :D";               ViewBag.JustWon = true;
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

        private VoiceLine GetTodayVoiceLine()
        {
            var today = DateTime.UtcNow.Date;
            var dailyEntry = _context.DailyVoiceLineGames.FirstOrDefault(x => x.Date == today);

            if (dailyEntry == null)
            {
                var randomVL = _context.VoiceLines.Include(v => v.Character).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (randomVL == null) return null;

                dailyEntry = new DailyVoiceLineGame
                {
                    VoiceLineId = randomVL.Id,
                    Date = today
                };

                _context.DailyVoiceLineGames.Add(dailyEntry);
                _context.SaveChanges();
            }

            return _context.VoiceLines.Include(v => v.Character).First(x => x.Id == dailyEntry.VoiceLineId);
        }

        [HttpGet]
        public IActionResult PlayVoiceLines()
        {
            var voiceLine = GetTodayVoiceLine();
            
            var sessionData = HttpContext.Session.GetString("VoiceLineGuessHistory");
            List<Character> history = string.IsNullOrEmpty(sessionData)
                ? new List<Character>()
                : JsonSerializer.Deserialize<List<Character>>(sessionData);

            ViewBag.History = history;
            ViewBag.VoiceLine = voiceLine;
            
            bool guessedCorrectlyToday = false;
            var sessionWon = HttpContext.Session.GetString("VoiceLineWonToday_" + DateTime.UtcNow.Date.ToString("yyyyMMdd"));
            if (!string.IsNullOrEmpty(sessionWon) && sessionWon == "true")
            {
                guessedCorrectlyToday = true;
            }

            ViewBag.GuessedCorrectlyToday = guessedCorrectlyToday;
            ViewBag.AllCharacters = _context.Characters.OrderBy(c => c.Name).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult PlayVoiceLines(string guess)
        {
            var voiceLine = GetTodayVoiceLine();
            if (voiceLine == null)
            {
                ViewBag.Error = "No voice lines available in the database.";
                ViewBag.AllCharacters = _context.Characters.OrderBy(c => c.Name).ToList();
                return View();
            }

            bool guessedCorrectlyToday = false;
            var sessionWonKey = "VoiceLineWonToday_" + DateTime.UtcNow.Date.ToString("yyyyMMdd");
            var sessionWon = HttpContext.Session.GetString(sessionWonKey);
            if (!string.IsNullOrEmpty(sessionWon) && sessionWon == "true")
            {
                guessedCorrectlyToday = true;
                ViewBag.Message = "You already guessed correctly today!";
                ViewBag.VoiceLine = voiceLine;
                ViewBag.GuessedCorrectlyToday = guessedCorrectlyToday;
                var histDataStr = HttpContext.Session.GetString("VoiceLineGuessHistory");
                ViewBag.History = string.IsNullOrEmpty(histDataStr) ? new List<Character>() : JsonSerializer.Deserialize<List<Character>>(histDataStr);
                ViewBag.AllCharacters = _context.Characters.OrderBy(c => c.Name).ToList();
                return View();
            }

            var guessedCharacter = _context.Characters.FirstOrDefault(x => x.Name.ToLower() == (guess ?? "").ToLower());

            var sessionKey = "VoiceLineGuessHistory";
            var sessionData = HttpContext.Session.GetString(sessionKey);
            List<Character> history = string.IsNullOrEmpty(sessionData)
                ? new List<Character>()
                : JsonSerializer.Deserialize<List<Character>>(sessionData);

            if (guessedCharacter == null)
            {
                ViewBag.Error = "Character not found!";
            }
            else
            {
                if (!history.Any(c => c.Id == guessedCharacter.Id))
                {
                    history.Insert(0, guessedCharacter);
                    HttpContext.Session.SetString(sessionKey, JsonSerializer.Serialize(history));
                }

                if (string.Equals(guess, voiceLine.Character.Name, StringComparison.OrdinalIgnoreCase))
                {
                    guessedCorrectlyToday = true;
                    HttpContext.Session.SetString(sessionWonKey, "true");
                    ViewBag.Message = "🎉 Correct! Come back tomorrow! :D";               ViewBag.JustWon = true;
                }
                else
                {
                    ViewBag.Message = "Wrong, try again?";
                }
            }

            ViewBag.VoiceLine = voiceLine;
            ViewBag.GuessedCorrectlyToday = guessedCorrectlyToday;
            ViewBag.History = history;
            ViewBag.AllCharacters = _context.Characters.OrderBy(c => c.Name).ToList();

            return View();
        }
    }
}
