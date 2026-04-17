using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FNaFle.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<MapLocation> GetTodayMapLocation()
        {
            var today = DateTime.UtcNow.Date;
            var dailyEntry = await _context.DailyMapGames.FirstOrDefaultAsync(x => x.Date == today);

            if (dailyEntry == null)
            {
                var randomMap = await _context.MapLocations.OrderBy(x => Guid.NewGuid()).FirstOrDefaultAsync();

                if (randomMap == null) return null;

                dailyEntry = new DailyMapGame
                {
                    MapLocationId = randomMap.Id,
                    Date = today
                };

                _context.DailyMapGames.Add(dailyEntry);
                await _context.SaveChangesAsync();
            }

            return await _context.MapLocations.FirstOrDefaultAsync(x => x.Id == dailyEntry.MapLocationId);
        }

        public async Task<IActionResult> Index()
        {
            var location = await GetTodayMapLocation();

            
            ViewBag.AllGames = new List<string> { "FNaF 1", "FNaF 2", "FNaF 3", "FFPS", "FNaF SL" };

            
            bool guessedCorrectlyToday = false;
            var userName = User.Identity.Name;

            var sessionWonKey = $"MapWonToday_{DateTime.UtcNow.Date:yyyyMMdd}_{userName}";
            var sessionWon = HttpContext.Session.GetString(sessionWonKey);
            if (!string.IsNullOrEmpty(sessionWon) && sessionWon == "true")
            {
                guessedCorrectlyToday = true;
            }
            ViewBag.GuessedCorrectlyToday = guessedCorrectlyToday;

            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> CheckVisualGuess(int id, string gameName, string cameraName)
        {
            var userName = User.Identity.Name;

            var sessionWonKey = $"MapWonToday_{DateTime.UtcNow.Date:yyyyMMdd}_{userName}";
            var sessionWon = HttpContext.Session.GetString(sessionWonKey);
            if (!string.IsNullOrEmpty(sessionWon) && sessionWon == "true")
            {
                return Json(new { success = false, message = "You already guessed correctly today!" });
            }

            var actual = await _context.MapLocations.FindAsync(id);
            if (actual == null) return Json(new { success = false, message = "Error: Signal Lost" });

            bool gameCorrect = string.Equals(actual.GameName?.Trim(), gameName?.Trim(), StringComparison.OrdinalIgnoreCase);
            bool cameraCorrect = string.Equals(actual.CameraName?.Trim(), cameraName?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (gameCorrect && cameraCorrect)
            {
                HttpContext.Session.SetString(sessionWonKey, "true");
                return Json(new { success = true, message = "Correct! Come back tomorrow!" });
            }

            return Json(new { success = false, message = "wrong try again :(" });
        }
    }
}