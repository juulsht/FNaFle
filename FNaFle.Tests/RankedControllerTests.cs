using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace FNaFle.Tests
{
    public class RankedControllerTests
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

        private Mock<UserManager<IdentityUser>> GetMockUserManager(IdentityUser user)
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            return userManagerMock;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithCharacterAndHistory()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Id = 1, Name = "Freddy", Gender = "Male", Generation = "Gen 1", Species = "Bear", Location = "Pizzeria", Status = "Active" });
            await db.SaveChangesAsync();

            var user = new IdentityUser { Id = "user1", UserName = "test" };
            var userManagerMock = GetMockUserManager(user);

            var controller = new RankedController(db, userManagerMock.Object);
            
            var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                        var result = await controller.Index();

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Character);
            Assert.NotNull(controller.ViewBag.History);
        }

        [Fact]
        public async Task Play_CorrectGuess_UpdatesScore()
        {
                        var db = GetDatabaseContext();
            var character = new Character { Id = 1, Name = "Freddy", Gender = "Male", Generation = "Gen 1", Species = "Bear", Location = "Pizzeria", Status = "Active" };
            db.Characters.Add(character);
            await db.SaveChangesAsync();

            var user = new IdentityUser { Id = "user1", UserName = "test" };
            var userManagerMock = GetMockUserManager(user);

            var controller = new RankedController(db, userManagerMock.Object);

            var sessionMock = new Mock<ISession>();
            byte[] sessionValue = null;
            sessionMock.Setup(s => s.TryGetValue("RankedHistory", out sessionValue)).Returns(false);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            
                        var result = await controller.Play("Freddy");

                        var viewResult = Assert.IsType<ViewResult>(result);
            var score = await db.RankedScores.FirstOrDefaultAsync(s => s.Username == "user1");
            Assert.NotNull(score);
            Assert.True(score.TotalPoints > 0);
            Assert.Equal(DateTime.Today, score.LastPlayedDate);
        }
        [Fact]
        public async Task Index_UserNotAuthenticated_Redirects()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Name = "X", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" });
            db.SaveChanges();
            var user = new IdentityUser { Id = "u1" };
            var userManagerMock = GetMockUserManager(user);
            var controller = new RankedController(db, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = await controller.Index();

                        Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Play_HttpPost_NoGuess_ReturnsError()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Name = "X", Gender = "M", Generation = "1", Species = "B", Location = "P", Status = "A" });
            db.SaveChanges();
            var user = new IdentityUser { Id = "u1" };
            var userManagerMock = GetMockUserManager(user);
            var controller = new RankedController(db, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { Session = new Mock<ISession>().Object } };

                        var result = await controller.Play("");

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.Error);
        }
    }
}
