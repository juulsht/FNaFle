using FNaFle.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FNaFle.Models;
using System.IO;

namespace FNaFle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
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
            var streakLeaders = await (from progress in _context.UserProgress
                                       join user in _context.Users on progress.UserId equals user.Id
                                       join up in _context.UserProfiles on user.Id equals up.UserId into upJoin
                                       from up in upJoin.DefaultIfEmpty()
                                       orderby progress.HighestStreak descending
                                       select new LeaderboardUserViewModel
                                       {
                                           Username = user.UserName ?? "Unknown Player",
                                           Streak = progress.HighestStreak,
                                           ProfilePicturePath = up.ProfilePicturePath
                                       })
                                       .Take(100)
                                       .ToListAsync();

            // 2. Fetch Ranked Points Data
            var rankedLeaders = await (from ranked in _context.RankedScores
                                       join user in _context.Users on ranked.Username equals user.Id
                                       join up in _context.UserProfiles on user.Id equals up.UserId into upJoin
                                       from up in upJoin.DefaultIfEmpty()
                                       orderby ranked.TotalPoints descending
                                       select new 
                                       {
                                           Username = user.UserName ?? "Unknown Player",
                                           Points = ranked.TotalPoints,
                                           ProfilePicturePath = up.ProfilePicturePath
                                       })
                                       .Take(100)
                                       .ToListAsync();

            ViewBag.StreakLeaders = streakLeaders;
            ViewBag.RankedLeaders = rankedLeaders;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PublicProfile(string username)
        {
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Leaderboard");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return RedirectToAction("Leaderboard");

            var profile = await _context.UserProfiles
                .Include(up => up.FavChar1)
                .Include(up => up.FavChar2)
                .Include(up => up.FavChar3)
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            var progress = await _context.UserProgress.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var rankedScore = await _context.RankedScores.FirstOrDefaultAsync(r => r.Username == user.Id);

            if (progress != null && progress.Streak > progress.HighestStreak)
            {
                progress.HighestStreak = progress.Streak;
                _context.Update(progress);
                await _context.SaveChangesAsync();
            }

            ViewBag.Username = user.UserName;
            ViewBag.ProfilePicturePath = profile?.ProfilePicturePath;
            ViewBag.RankedPoints = rankedScore?.TotalPoints ?? 0;
            ViewBag.HighestStreak = progress?.HighestStreak ?? 0;
            
            var favCharacters = new List<Character>();
            if (profile?.FavChar1 != null) favCharacters.Add(profile.FavChar1);
            if (profile?.FavChar2 != null) favCharacters.Add(profile.FavChar2);
            if (profile?.FavChar3 != null) favCharacters.Add(profile.FavChar3);
            
            ViewBag.FavCharacters = favCharacters;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == user.Id);
            var characters = await _context.Characters.OrderBy(c => c.Name).ToListAsync();

            var model = new EditProfileViewModel
            {
                CurrentUsername = user.UserName ?? "",
                CurrentProfilePicturePath = userProfile?.ProfilePicturePath,
                FavChar1Id = userProfile?.FavChar1Id,
                FavChar2Id = userProfile?.FavChar2Id,
                FavChar3Id = userProfile?.FavChar3Id,
                AvailableCharacters = characters
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            var characters = await _context.Characters.OrderBy(c => c.Name).ToListAsync();
            model.AvailableCharacters = characters;

            if (string.IsNullOrWhiteSpace(model.NewUsername))
            {
                model.NewUsername = user.UserName;
            }

            var existingUser = await _userManager.FindByNameAsync(model.NewUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("NewUsername", "That username is already claimed!");
                model.CurrentUsername = user.UserName;
                return View(model);
            }

            user.UserName = model.NewUsername;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                model.CurrentUsername = user.UserName;
                return View(model);
            }

            // Handle Profile updates
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (userProfile == null)
            {
                userProfile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(userProfile);
            }

            userProfile.FavChar1Id = model.FavChar1Id;
            userProfile.FavChar2Id = model.FavChar2Id;
            userProfile.FavChar3Id = model.FavChar3Id;

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }

                userProfile.ProfilePicturePath = "/images/profiles/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Leaderboard");
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