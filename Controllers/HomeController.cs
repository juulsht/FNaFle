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

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
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

        // --- NEW LEADERBOARD METHOD ---
        public async Task<IActionResult> Leaderboard()
        {
            var leaderboardData = await _context.UserProgress
                .Join(_context.Users,
                    progress => progress.UserId,
                    user => user.Id,
                    (progress, user) => new LeaderboardUserViewModel
                    {
                        Username = user.UserName ?? "Unknown",
                        Streak = progress.Streak
                    })
                .OrderByDescending(u => u.Streak)
                .Take(100) // Shows top 100 players
                .ToListAsync();

            return View(leaderboardData);
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