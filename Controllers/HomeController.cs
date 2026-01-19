using FNaFle.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FNaFle.Models;

namespace FNaFle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            int streak = 0;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var progress = await _context.UserProgress
                        .FirstOrDefaultAsync(x => x.UserId == user.Id);

                    if (progress != null)
                        streak = progress.Streak;
                }
            }

            ViewBag.Streak = streak;
            return View();
        }

        // --- LEADERBOARD: JOINS PROGRESS WITH USERS TO GET USERNAME ---
        public async Task<IActionResult> Leaderboard()
        {
            var leaderboardData = await _context.UserProgress
                .Join(_context.Users,
                    progress => progress.UserId,
                    user => user.Id,
                    (progress, user) => new LeaderboardUserViewModel
                    {
                        // Pulls the Username column from the database
                        Username = user.UserName ?? "Unknown Player",
                        Streak = progress.Streak
                    })
                .OrderByDescending(u => u.Streak)
                .Take(100)
                .ToListAsync();

            return View(leaderboardData);
        }

        // --- PROFILE: GET PAGE ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            var model = new EditProfileViewModel
            {
                CurrentUsername = user.UserName ?? ""
            };
            return View(model);
        }

        // --- PROFILE: POST CHANGES ---
        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            // Check if new username is already taken by someone else
            var existingUser = await _userManager.FindByNameAsync(model.NewUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("NewUsername", "That username is already claimed!");
                model.CurrentUsername = user.UserName;
                return View(model);
            }

            // Update the IdentityUser UserName
            user.UserName = model.NewUsername;

            // Note: In some Identity setups, changing UserName also changes NormalizedUserName
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // This refreshes the user's cookies so the navbar shows the new name immediately
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Leaderboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            model.CurrentUsername = user.UserName;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}