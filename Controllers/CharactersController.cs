using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FNaFle.Controllers
{
    public class CharactersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CharactersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Everyone can view the list
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Characters.OrderBy(c => c.Name).ToListAsync();
            return View(list);
        }

        // Everyone can view details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var c = await _db.Characters.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        // Only Admin can create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Character());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Character model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.Characters.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Only Admin can edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _db.Characters.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Character model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // Only Admin can delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Characters.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var c = await _db.Characters.FindAsync(id);
            if (c != null)
            {
                _db.Characters.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
