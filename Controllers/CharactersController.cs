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

        
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Characters.OrderBy(c => c.Name).ToListAsync();
            return View(list);
        }

        
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var c = await _db.Characters.Include(x => x.VoiceLines).FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVoiceLine(int characterId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return RedirectToAction(nameof(Details), new { id = characterId });
            var vl = new VoiceLine { CharacterId = characterId, Text = text };
            _db.VoiceLines.Add(vl);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = characterId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVoiceLine(int id)
        {
            var vl = await _db.VoiceLines.FindAsync(id);
            if (vl != null)
            {
                var charId = vl.CharacterId;
                _db.VoiceLines.Remove(vl);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = charId });
            }
            return RedirectToAction(nameof(Index));
        }

        
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
