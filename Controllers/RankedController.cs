using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FNaFle.Controllers
{
    public class RankedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RankedController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Character GetDailyCharacter()
        {
            var all = _context.Characters.ToList();
            int seed = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
            var random = new Random(seed);
            return all[random.Next(all.Count)];
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var character = GetDailyCharacter();
            var user = await _userManager.GetUserAsync(User);

            bool hasPlayedToday = false;

            if (user != null)
            {
                // Check the database to see if a record exists for today
                hasPlayedToday = await _context.RankedScores
                    .AnyAsync(x => x.Username == user.Id && x.LastPlayedDate >= DateTime.Today);
            }

            // Load session history for display
            var sessionData = HttpContext.Session.GetString("RankedHistory");
            var history = string.IsNullOrEmpty(sessionData)
                ? new List<Character>()
                : JsonSerializer.Deserialize<List<Character>>(sessionData);

            ViewBag.Character = character;
            ViewBag.History = history;

            // If the DB says they played, they are locked out even if session is empty
            ViewBag.GuessedCorrectlyToday = hasPlayedToday;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Play(string guess)
        {
            var character = GetDailyCharacter();
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            // 1. HARD LOCK: Check DB before allowing a guess
            var existingScore = await _context.RankedScores
                .FirstOrDefaultAsync(x => x.Username == user.Id);

            if (existingScore != null && existingScore.LastPlayedDate >= DateTime.Today)
            {
                ViewBag.Message = "You already guessed the character. Come back tomorrow :)";
                ViewBag.GuessedCorrectlyToday = true;
                ViewBag.Character = character;
                return View("Index");
            }

            // 2. Get History from Session
            var sessionKey = "RankedHistory";
            var sessionData = HttpContext.Session.GetString(sessionKey);
            List<Character> history = string.IsNullOrEmpty(sessionData)
                ? new List<Character>()
                : JsonSerializer.Deserialize<List<Character>>(sessionData);

            // 3. Validate Guess
            var guessedCharacter = _context.Characters
                .FirstOrDefault(x => x.Name.ToLower() == (guess ?? "").ToLower());

            if (guessedCharacter == null)
            {
                ViewBag.Error = "Character not found!";
            }
            else if (history.Count < 3)
            {
                history.Insert(0, guessedCharacter);
                HttpContext.Session.SetString(sessionKey, JsonSerializer.Serialize(history));

                if (string.Equals(guess, character.Name, StringComparison.OrdinalIgnoreCase))
                {
                    int pointsEarned = 4 - history.Count;
                    await SaveRankedPoints(user.Id, pointsEarned);

                    ViewBag.GuessedCorrectlyToday = true;
                    ViewBag.Message = $"🎉 Correct! You earned {pointsEarned} points!";
                }
                else if (history.Count >= 3)
                {
                    // If they fail all 3 tries, we still mark them as "played" in the DB 
                    // so they can't restart the app to try again.
                    await SaveRankedPoints(user.Id, 0);
                    ViewBag.Message = "❌ Out of tries! Come back tomorrow.";
                    ViewBag.GuessedCorrectlyToday = true; // Lock the UI
                }
            }

            ViewBag.Character = character;
            ViewBag.History = history;
            return View("Index");
        }

        private async Task SaveRankedPoints(string userId, int points)
        {
            var score = await _context.RankedScores.FirstOrDefaultAsync(x => x.Username == userId);

            if (score == null)
            {
                score = new RankedScore
                {
                    Username = userId,
                    TotalPoints = 0,
                    CurrentStreak = 0
                };
                _context.RankedScores.Add(score);
            }

            score.TotalPoints += points;
            score.LastPlayedDate = DateTime.Today;

            await _context.SaveChangesAsync();
        }
    }
}