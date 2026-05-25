using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FNaFle.Tests
{
    public class GameControllerTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Play_HttpGet_ReturnsViewResultWithCharacter()
        {
                        var context = GetDatabaseContext();
            var character = new Character 
            { 
                Id = 1, 
                Name = "Freddy",
                Gender = "Male",
                Generation = "Gen 1",
                Location = "Pizzeria",
                Species = "Bear",
                Status = "Active"
            };
            context.Characters.Add(character);
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);

                        var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                        var result = await controller.Play();

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Character);
            Assert.Equal("Freddy", ((Character)controller.ViewBag.Character).Name);
        }

        [Fact]
        public async Task Play_HttpPost_IncorrectGuess_AddsToHistory()
        {
                        var context = GetDatabaseContext();
            var user = new IdentityUser { Id = "u1", UserName = "T" };
            context.Users.Add(user);
            context.UserProgress.Add(new UserProgress { UserId = "u1" });
            
            var character = new Character { Id = 1, Name = "Freddy", Gender = "M", Generation = "1", Location = "P", Species = "B", Status = "A" };
            var other = new Character { Id = 2, Name = "Bonnie", Gender = "M", Generation = "1", Location = "P", Species = "R", Status = "A" };
            context.Characters.AddRange(character, other);
            
            context.DailyGames.Add(new DailyGame { CharacterId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var controller = new GameController(context, userManagerMock.Object);

            var sessionDataDict = new Dictionary<string, byte[]>();
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => sessionDataDict[k] = v);
            
            byte[] outValue;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out outValue))
                .Returns((string key, out byte[] value) => 
                {
                    bool found = sessionDataDict.TryGetValue(key, out value);
                    return found;
                });
            
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                        var result = await controller.Play("Bonnie");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.History);
            var history = (List<Character>)controller.ViewBag.History;
            Assert.Contains(history, c => c.Name == "Bonnie");
        }

        [Fact]
        public async Task Play_HttpPost_GuessedCorrectlyToday_ReturnsMessage()
        {
                        var context = GetDatabaseContext();
            var character = new Character { Id = 1, Name = "Freddy", Gender = "M", Generation = "1", Location = "P", Species = "B", Status = "A" };
            context.Characters.Add(character);
            var user = new IdentityUser { Id = "u1", UserName = "T" };
            context.Users.Add(user);
            context.UserProgress.Add(new UserProgress { UserId = "u1", HasGuessedCorrectlyToday = true });
            context.DailyGames.Add(new DailyGame { CharacterId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var controller = new GameController(context, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = await controller.Play("Freddy");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Contains("already complete", (string)controller.ViewBag.Message);
        }

        [Fact]
        public async Task Play_HttpPost_UserNull_ReturnsError()
        {
                        var context = GetDatabaseContext();
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser)null);
            var controller = new GameController(context, userManagerMock.Object);

                        var result = await controller.Play("Freddy");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("You must be logged in to play.", (string)controller.ViewBag.Error);
        }

        [Fact]
        public async Task Play_HttpPost_CharacterNotFound_ReturnsError()
        {
                        var context = GetDatabaseContext();
            var character = new Character { Id = 1, Name = "Freddy", Gender = "M", Generation = "1", Location = "P", Species = "B", Status = "A" };
            context.Characters.Add(character);
            var user = new IdentityUser { Id = "u1" };
            context.Users.Add(user);
            context.UserProgress.Add(new UserProgress { UserId = "u1" });
            context.DailyGames.Add(new DailyGame { CharacterId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var controller = new GameController(context, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = await controller.Play("Unknown");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Character not found!", (string)controller.ViewBag.Error);
        }
        [Fact]
        public void PlayVoiceLines_HttpGet_ReturnsViewResult_WhenDailyExists()
        {
                        var context = GetDatabaseContext();
            var ch = new Character { Name = "X", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" };
            context.Characters.Add(ch);
            var vl = new VoiceLine { Text = "Hi", Character = ch };
            context.VoiceLines.Add(vl);
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = vl.Id, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = controller.PlayVoiceLines();

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.VoiceLine);
        }

        [Fact]
        public void PlayVoiceLines_HttpPost_IncorrectGuess_AddsToHistory()
        {
                        var context = GetDatabaseContext();
            var ch = new Character { Name = "Freddy", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" };
            var other = new Character { Name = "Bonnie", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" };
            context.Characters.AddRange(ch, other);
            var vl = new VoiceLine { Text = "Hi", Character = ch };
            context.VoiceLines.Add(vl);
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = vl.Id, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = controller.PlayVoiceLines("Bonnie");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.History);
        }
        [Fact]
        public void PlayVoiceLines_HttpPost_UserWonToday_ReturnsAlreadyGuessedMessage()
        {
                        var context = GetDatabaseContext();
            var ch = new Character { Name = "Freddy", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" };
            context.Characters.Add(ch);
            var vl = new VoiceLine { Text = "Hi", Character = ch };
            context.VoiceLines.Add(vl);
            var todayStr = DateTime.UtcNow.Date.ToString("yyyyMMdd");
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = vl.Id, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);
            
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("VoiceLineWonToday_" + todayStr, out It.Ref<byte[]>.IsAny))
                .Returns((string k, out byte[] v) => {
                    v = Encoding.UTF8.GetBytes("true");
                    return true;
                });
            
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = sessionMock.Object } };

                        var result = controller.PlayVoiceLines("Freddy");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Contains("already guessed", (string)controller.ViewBag.Message);
        }

        [Fact]
        public void PlayVoiceLines_HttpPost_CorrectGuess_SetsSession()
        {
                        var context = GetDatabaseContext();
            var ch = new Character { Name = "Freddy", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" };
            context.Characters.Add(ch);
            var vl = new VoiceLine { Text = "Hi", Character = ch };
            context.VoiceLines.Add(vl);
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = vl.Id, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);
            
            var sessionMock = new Mock<ISession>();
            string savedKey = null;
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => { if (k.StartsWith("VoiceLineWonToday")) savedKey = k; });

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = sessionMock.Object } };

                        var result = controller.PlayVoiceLines("Freddy");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(savedKey);
        }
    }
}
