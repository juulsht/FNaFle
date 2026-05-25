using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FNaFle.Tests
{
    public class MapControllerTests
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

        private ControllerContext GetMockControllerContext(string userName)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, userName)
            }, "mock"));

            var sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.Session = sessionMock.Object;

            return new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_AndCreatesDailyMapGameIfMissing()
        {
                        var db = GetDatabaseContext();
            db.MapLocations.Add(new MapLocation { Id = 1, GameName = "FNaF 1", CameraName = "CAM 1" });
            await db.SaveChangesAsync();

            var controller = new MapController(db);
            controller.ControllerContext = GetMockControllerContext("testuser");

                        var result = await controller.Index();

                        var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MapLocation>(viewResult.ViewData.Model);
            Assert.Equal(1, await db.DailyMapGames.CountAsync());
        }

        [Fact]
        public async Task CheckVisualGuess_CorrectGuess_ReturnsSuccess()
        {
                        var db = GetDatabaseContext();
            var location = new MapLocation { Id = 1, GameName = "FNaF 1", CameraName = "CAM 1" };
            db.MapLocations.Add(location);
            await db.SaveChangesAsync();

            var controller = new MapController(db);
            controller.ControllerContext = GetMockControllerContext("testuser");

                        var result = await controller.CheckVisualGuess(1, "FNaF 1", "CAM 1");

                        var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic data = jsonResult.Value;
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            Assert.Contains("true", json);
            Assert.Contains("Correct", json);
        }

        [Fact]
        public async Task CheckVisualGuess_IncorrectGuess_ReturnsFailure()
        {
                        var db = GetDatabaseContext();
            var location = new MapLocation { Id = 1, GameName = "FNaF 1", CameraName = "CAM 1" };
            db.MapLocations.Add(location);
            await db.SaveChangesAsync();

            var controller = new MapController(db);
            controller.ControllerContext = GetMockControllerContext("testuser");

                        var result = await controller.CheckVisualGuess(1, "Wrong Game", "CAM 1");

                        var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic data = jsonResult.Value;
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            Assert.Contains("false", json);
            Assert.Contains("wrong", json);
        }
    }
}
