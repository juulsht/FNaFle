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

        // --- UPDATED LEADERBOARD: HANDLES BOTH MODES ---
        public async Task<IActionResult> Leaderboard()
        {
            // 1. Fetch Classic Streak Data
            var streakLeaders = await _context.UserProgress
                .Join(_context.Users,
                    progress => progress.UserId,
                    user => user.Id,
                    (progress, user) => new LeaderboardUserViewModel
                    {
                        Username = user.UserName ?? "Unknown Player",
                        Streak = progress.Streak
                    })
                .OrderByDescending(u => u.Streak)
                .Take(100)
                .ToListAsync();

            // 2. Fetch Ranked Points Data
            // Note: We join with Users table to get the actual display name (UserName) 
            // instead of just the Id stored in RankedScores.Username
            var rankedLeaders = await _context.RankedScores
                .Join(_context.Users,
                    ranked => ranked.Username, // This holds the User.Id
                    user => user.Id,
                    (ranked, user) => new
                    {
                        Username = user.UserName ?? "Unknown Player",
                        Points = ranked.TotalPoints
                    })
                .OrderByDescending(u => u.Points)
                .Take(100)
                .ToListAsync();

            ViewBag.StreakLeaders = streakLeaders;
            ViewBag.RankedLeaders = rankedLeaders;

            return View();
        }

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

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            var existingUser = await _userManager.FindByNameAsync(model.NewUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("NewUsername", "That username is already claimed!");
                model.CurrentUsername = user.UserName;
                return View(model);
            }

            user.UserName = model.NewUsername;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
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