using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                if (randomCharacter == null) return null; // Handle empty database

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

                // 1. If progress doesn't exist, create it properly
                if (progress == null)
                {
                    progress = new UserProgress
                    {
                        UserId = user.Id,
                        LastGuessDate = DateTime.UtcNow.Date.AddDays(-1), // Set to yesterday so they can play today
                        Streak = 0,
                        HasGuessedCorrectlyToday = false
                    };
                    _context.UserProgress.Add(progress);
                    await _context.SaveChangesAsync();
                }

                // 2. CHECK: If it's a new day, unlock the ability to guess
                if (progress.LastGuessDate < DateTime.UtcNow.Date)
                {
                    // If they missed more than 1 day, reset the streak
                    if (progress.LastGuessDate < DateTime.UtcNow.Date.AddDays(-1))
                    {
                        progress.Streak = 0;
                    }

                    progress.HasGuessedCorrectlyToday = false;
                    _context.Update(progress);
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.Character = character;
            ViewBag.Progress = progress;
            ViewBag.GuessedCorrectlyToday = progress?.HasGuessedCorrectlyToday ?? false;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Play(string guess)
        {
            // Reset stored guesses after 08:00
            if (DateTime.Now.TimeOfDay >= resetTime)
                HttpContext.Session.Remove("PreviousGuesses");

            var character = GetTodayCharacter();
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ViewBag.Error = "You must be logged in to play.";
                ViewBag.Character = character;
                return View();
            }

            var progress = await _context.UserProgress.FirstAsync(x => x.UserId == user.Id);

            // 3. FIX: Stop the streak from going up if they've already won today
            if (progress.HasGuessedCorrectlyToday)
            {
                ViewBag.Message = "Tonight's challenge is already complete! Come back tomorrow.";
                ViewBag.Character = character;
                ViewBag.Progress = progress;
                ViewBag.GuessedCorrectlyToday = true;
                return View();
            }

            // FIND THE GUESSED CHARACTER
            var guessedCharacter = _context.Characters
                .FirstOrDefault(x => x.Name.ToLower() == (guess ?? "").ToLower());

            if (guessedCharacter == null)
            {
                ViewBag.Error = "Character not found!";
                ViewBag.Character = character;
                ViewBag.Progress = progress;
                return View();
            }

            // BUILD RESULT BOXES
            var results = new Dictionary<string, string>
            {
                { "Gender", guessedCharacter.Gender },
                { "Generation", guessedCharacter.Generation },
                { "Species", guessedCharacter.Species },
                { "Location", guessedCharacter.Location },
                { "Status", guessedCharacter.Status }
            };

            ViewBag.Results = results;
            ViewBag.GuessName = guessedCharacter.Name;

            // 4. CHECK WIN CONDITION
            if (string.Equals(guess, character.Name, StringComparison.OrdinalIgnoreCase))
            {
                progress.HasGuessedCorrectlyToday = true;
                progress.Streak++; // Increment ONLY on the first win of the day
                progress.LastGuessDate = DateTime.UtcNow.Date; // Record the win date

                ViewBag.Message = "🎉 Correct! Come back tomorrow after 08:00. :D";
            }
            else
            {
                ViewBag.Message = "Wrong, try again?";
            }

            // Save the win and the date to the DB
            _context.Update(progress);
            await _context.SaveChangesAsync();

            ViewBag.Character = character;
            ViewBag.Progress = progress;
            ViewBag.GuessedCorrectlyToday = progress.HasGuessedCorrectlyToday;

            return View();
        }
    }
}