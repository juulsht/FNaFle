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

            // Fetch unique values for the dropdowns from your DB
            ViewBag.Games = await _context.MapLocations
                .Select(m => m.GameName)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.Cameras = await _context.MapLocations
                .Select(m => m.CameraName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckGuess(int id, string gameGuess, string cameraGuess)
        {
            var actual = await _context.MapLocations.FindAsync(id);
            if (actual == null) return RedirectToAction(nameof(Index));

            // Logic check against the database
            bool gameCorrect = string.Equals(actual.GameName?.Trim(), gameGuess?.Trim(), StringComparison.OrdinalIgnoreCase);
            bool cameraCorrect = string.Equals(actual.CameraName?.Trim(), cameraGuess?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (gameCorrect && cameraCorrect)
            {
                TempData["IsCorrect"] = true;
                TempData["Message"] = "Correct!";
            }
            else
            {
                TempData["IsCorrect"] = false;
                TempData["Message"] = "Try Again..";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}