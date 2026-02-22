using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FNaFle.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var location = await _context.MapLocations
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (location == null) return View(null);

            // Get unique games for the top navigation buttons
            ViewBag.AllGames = await _context.MapLocations
                .Select(m => m.GameName)
                .Distinct()
                .ToListAsync();

            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> CheckVisualGuess(int id, string gameName, string cameraName)
        {
            var actual = await _context.MapLocations.FindAsync(id);
            if (actual == null) return Json(new { success = false, message = "Error: Signal Lost" });

            bool gameCorrect = string.Equals(actual.GameName?.Trim(), gameName?.Trim(), StringComparison.OrdinalIgnoreCase);
            bool cameraCorrect = string.Equals(actual.CameraName?.Trim(), cameraName?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (gameCorrect && cameraCorrect)
            {
                return Json(new { success = true, message = "great" });
            }

            return Json(new { success = false, message = "wrong try again :(" });
        }
    }
}