using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class GameController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string DailyCharacterKey = "DailyCharacterId";
    private const string PreviousGuessesKey = "PreviousGuesses";

    public GameController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Game/Play
    public IActionResult Play()
    {
        // Pick a random daily character if none exists
        int? dailyId = HttpContext.Session.GetInt32(DailyCharacterKey);
        Character dailyCharacter = null;

        if (dailyId == null)
        {
            dailyCharacter = _context.Characters
                .OrderBy(c => Guid.NewGuid())
                .FirstOrDefault();

            if (dailyCharacter != null)
            {
                HttpContext.Session.SetInt32(DailyCharacterKey, dailyCharacter.Id);
            }
        }
        else
        {
            dailyCharacter = _context.Characters.FirstOrDefault(c => c.Id == dailyId.Value);
        }

        if (dailyCharacter == null)
        {
            ViewBag.Error = "No characters in the database!";
            return View();
        }

        // Load previous guesses
        var previousGuessesJson = HttpContext.Session.GetString(PreviousGuessesKey);
        var previousGuesses = !string.IsNullOrEmpty(previousGuessesJson)
            ? JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(previousGuessesJson)
            : new List<Dictionary<string, string>>();

        ViewBag.PreviousGuesses = previousGuesses;
        ViewBag.DailyCharacter = dailyCharacter;

        return View();
    }

    // POST: /Game/Play
    [HttpPost]
    public IActionResult Play(string guessName)
    {
        if (string.IsNullOrEmpty(guessName))
        {
            ViewBag.Error = "Please enter a character name!";
            return Play(); // reuse GET to reload daily character and previous guesses
        }

        int? dailyId = HttpContext.Session.GetInt32(DailyCharacterKey);
        if (dailyId == null)
            return RedirectToAction("Play");

        var dailyCharacter = _context.Characters.FirstOrDefault(c => c.Id == dailyId.Value);
        if (dailyCharacter == null)
        {
            ViewBag.Error = "Daily character not found!";
            return Play();
        }

        var guessedCharacter = _context.Characters
            .FirstOrDefault(c => c.Name.ToLower() == guessName.ToLower());

        if (guessedCharacter == null)
        {
            ViewBag.Error = "Character not found!";
            return Play();
        }

        // Store guessed values
        var results = new Dictionary<string, string>
        {
            { "Gender", guessedCharacter.Gender },
            { "Generation", guessedCharacter.Generation },
            { "Species", guessedCharacter.Species },
            { "Location", guessedCharacter.Location },
            { "Status", guessedCharacter.Status }
        };

        ViewBag.GuessName = guessedCharacter.Name;
        ViewBag.Results = results;
        ViewBag.DailyCharacter = dailyCharacter;

        // Previous guesses
        var previousGuessesJson = HttpContext.Session.GetString(PreviousGuessesKey);
        var previousGuesses = !string.IsNullOrEmpty(previousGuessesJson)
            ? JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(previousGuessesJson)
            : new List<Dictionary<string, string>>();

        previousGuesses.Add(results);

        // Save back to session
        HttpContext.Session.SetString(PreviousGuessesKey, JsonConvert.SerializeObject(previousGuesses));

        // Exclude last guess from previous guesses in the display
        ViewBag.PreviousGuesses = previousGuesses.Take(previousGuesses.Count - 1).ToList();

        return View();
    }
}
