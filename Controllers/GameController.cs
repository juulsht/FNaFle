using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class GameController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    private readonly TimeSpan resetTime = new TimeSpan(8, 0, 0); // Reset at 08:00

    public GameController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private Character GetTodayCharacter()
    {
        var today = DateTime.UtcNow.Date;
        var dailyEntry = _context.DailyGames.FirstOrDefault(x => x.Date == today);

        if (dailyEntry == null)
        {
            var randomCharacter = _context.Characters.OrderBy(x => Guid.NewGuid()).First();

            dailyEntry = new DailyGame
            {
                CharacterId = randomCharacter.Id,
                Date = today
            };

            _context.DailyGames.Add(dailyEntry);
            _context.SaveChanges();
        }

        return _context.Characters.First(x => x.Id == dailyEntry.CharacterId);
    }

    public async Task<IActionResult> Play()
    {
        var character = GetTodayCharacter();
        var user = await _userManager.GetUserAsync(User);

        UserProgress progress = null;

        if (user != null)
        {
            progress = await _context.UserProgress.FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = user.Id,
                    LastGuessDate = DateTime.UtcNow.Date,
                    Streak = 0,
                    HasGuessedCorrectlyToday = false
                };

                _context.UserProgress.Add(progress);
                _context.SaveChanges();
            }

            // Reset streak if user skipped yesterday
            if (progress.LastGuessDate < DateTime.UtcNow.Date.AddDays(-1))
            {
                progress.Streak = 0;
                progress.HasGuessedCorrectlyToday = false;
                _context.SaveChanges();
            }
        }

        ViewBag.Character = character;
        ViewBag.Progress = progress;
        ViewBag.GuessedCorrectlyToday = progress?.HasGuessedCorrectlyToday ?? false;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Play(string guess)
    {
        // Reset stored guesses after 08:00
        if (DateTime.Now.TimeOfDay >= resetTime)
            HttpContext.Session.Remove("PreviousGuesses");

        var character = GetTodayCharacter();
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            ViewBag.Error = "You must be logged in to play.";
            ViewBag.Character = character;
            return View();
        }

        var progress = await _context.UserProgress.FirstAsync(x => x.UserId == user.Id);
        progress.LastGuessDate = DateTime.UtcNow.Date;

        // FIND THE GUESSED CHARACTER
        var guessedCharacter = _context.Characters
            .FirstOrDefault(x => x.Name.ToLower() == guess.ToLower());

        if (guessedCharacter == null)
        {
            ViewBag.Error = "Character not found!";
            ViewBag.Character = character;
            ViewBag.Progress = progress;
            return View();
        }

        // BUILD RESULT BOXES USING THE GUESSED CHARACTER
        var results = new Dictionary<string, string>
        {
            { "Gender", guessedCharacter.Gender },
            { "Generation", guessedCharacter.Generation },
            { "Species", guessedCharacter.Species },
            { "Location", guessedCharacter.Location },
            { "Status", guessedCharacter.Status }
        };

        ViewBag.Results = results;
        ViewBag.GuessName = guessedCharacter.Name;

        // CHECK WIN CONDITION
        if (string.Equals(guess, character.Name, StringComparison.OrdinalIgnoreCase))
        {
            progress.HasGuessedCorrectlyToday = true;
            progress.Streak++;
            _context.SaveChanges();

            ViewBag.Message = "🎉 Correct! Come back tomorrow after 08:00. :D";
        }
        else
        {
            ViewBag.Message = "Wrong, try again?";
        }

        _context.SaveChanges();

        ViewBag.Character = character;
        ViewBag.Progress = progress;
        ViewBag.GuessedCorrectlyToday = progress.HasGuessedCorrectlyToday;

        return View();
    }
}
