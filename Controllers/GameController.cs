using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Mvc;
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

    public IActionResult Play()
    {
        int? dailyId = HttpContext.Session.GetInt32(DailyCharacterKey);
        Character dailyCharacter = null;

        if (dailyId == null)
        {
            dailyCharacter = _context.Characters
                .OrderBy(c => Guid.NewGuid())
                .FirstOrDefault();

            if (dailyCharacter != null)
                HttpContext.Session.SetInt32(DailyCharacterKey, dailyCharacter.Id);
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

        var previousGuessesJson = HttpContext.Session.GetString(PreviousGuessesKey);
        var previousGuesses = !string.IsNullOrEmpty(previousGuessesJson)
            ? JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(previousGuessesJson)
            : new List<Dictionary<string, string>>();

        ViewBag.PreviousGuesses = previousGuesses;
        ViewBag.DailyCharacter = dailyCharacter;

        return View();
    }

    [HttpPost]
    public IActionResult Play(string guessName)
    {
        int? dailyId = HttpContext.Session.GetInt32(DailyCharacterKey);
        if (dailyId == null)
        {
            ViewBag.Error = "No daily character set!";
            return View();
        }

        var dailyCharacter = _context.Characters.FirstOrDefault(c => c.Id == dailyId.Value);
        if (dailyCharacter == null)
        {
            ViewBag.Error = "Daily character not found!";
            return View();
        }

        if (string.IsNullOrEmpty(guessName))
        {
            ViewBag.Error = "Please enter a character name!";
            ViewBag.DailyCharacter = dailyCharacter;
            return View();
        }

        var guessedCharacter = _context.Characters
            .FirstOrDefault(c => c.Name.ToLower() == guessName.ToLower());

        if (guessedCharacter == null)
        {
            ViewBag.Error = "Character not found!";
            var prevGuessesJson = HttpContext.Session.GetString(PreviousGuessesKey);
            ViewBag.PreviousGuesses = !string.IsNullOrEmpty(prevGuessesJson)
                ? JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(prevGuessesJson)
                : new List<Dictionary<string, string>>();
            ViewBag.DailyCharacter = dailyCharacter;
            return View();
        }

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
        ViewBag.IsCorrectGuess = guessedCharacter.Id == dailyCharacter.Id;

        if (ViewBag.IsCorrectGuess)
        {
            // Clear previous guesses if player guessed correctly
            HttpContext.Session.SetString(PreviousGuessesKey, JsonConvert.SerializeObject(new List<Dictionary<string, string>>()));
            ViewBag.PreviousGuesses = new List<Dictionary<string, string>>();
        }
        else
        {
            // Add to previous guesses if incorrect
            var prevGuessesJson2 = HttpContext.Session.GetString(PreviousGuessesKey);
            var previousGuesses = !string.IsNullOrEmpty(prevGuessesJson2)
                ? JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(prevGuessesJson2)
                : new List<Dictionary<string, string>>();

            previousGuesses.Add(results);
            HttpContext.Session.SetString(PreviousGuessesKey, JsonConvert.SerializeObject(previousGuesses));

            // Exclude last guess in display
            ViewBag.PreviousGuesses = previousGuesses.Take(previousGuesses.Count - 1).ToList();
        }

        return View();
    }
}
