using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // Arrange
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

            // Mock session
            var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.Play();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Character);
            Assert.Equal("Freddy", ((Character)controller.ViewBag.Character).Name);
        }

        [Fact]
        public async Task Play_HttpPost_CorrectGuess_UpdatesProgress()
        {
            // Arrange
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
            
            var user = new IdentityUser { Id = "test-user", UserName = "test" };
            var progress = new UserProgress { UserId = "test-user", LastGuessDate = DateTime.UtcNow.Date.AddDays(-1) };
            context.UserProgress.Add(progress);
            
            // Daily Game entry
            context.DailyGames.Add(new DailyGame { CharacterId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new GameController(context, userManagerMock.Object);

            // Mock session
            var sessionMock = new Mock<ISession>();
            byte[] sessionValue = null;
            sessionMock.Setup(s => s.TryGetValue("GuessHistory", out sessionValue)).Returns(false);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.Play("Freddy");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var updatedProgress = await context.UserProgress.FirstAsync(p => p.UserId == "test-user");
            Assert.True(updatedProgress.HasGuessedCorrectlyToday);
            Assert.Equal(1, updatedProgress.Streak);
        }

        [Fact]
        public void PlayVoiceLines_HttpGet_ReturnsViewResultWithVoiceLine()
        {
            // Arrange
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
            var voiceLine = new VoiceLine { Id = 1, Text = "I love pizza", CharacterId = 1, Character = character };
            context.Characters.Add(character);
            context.VoiceLines.Add(voiceLine);
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);

            // Mock session
            var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = controller.PlayVoiceLines();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.VoiceLine);
        }

        [Fact]
        public void PlayVoiceLines_HttpPost_CorrectGuess_ReturnsCorrectMessage()
        {
            // Arrange
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
            var voiceLine = new VoiceLine { Id = 1, Text = "I love pizza", CharacterId = 1, Character = character };
            context.Characters.Add(character);
            context.VoiceLines.Add(voiceLine);
            context.DailyVoiceLineGames.Add(new DailyVoiceLineGame { VoiceLineId = 1, Date = DateTime.UtcNow.Date });
            context.SaveChanges();

            var userManagerMock = GetMockUserManager();
            var controller = new GameController(context, userManagerMock.Object);

            // Mock session
            var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = controller.PlayVoiceLines("Freddy");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Contains("Correct!", (string)controller.ViewBag.Message);
            Assert.True((bool)controller.ViewBag.GuessedCorrectlyToday);
        }
    }
}
