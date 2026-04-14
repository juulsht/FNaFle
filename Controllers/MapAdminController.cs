using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FNaFle.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MapAdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MapAdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.MapLocations.OrderBy(m => m.GameName).ThenBy(m => m.CameraName).ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new MapLocation());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MapLocation model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.MapLocations.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.MapLocations.FindAsync(id);
            if (m == null) return NotFound();
            return View(m);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var m = await _db.MapLocations.FindAsync(id);
            if (m != null)
            {
                _db.MapLocations.Remove(m);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
