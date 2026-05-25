using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FNaFle.Tests
{
    public class HomeControllerTests
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
        public async Task Index_UserNotAuthenticated_ReturnsZeroStreak()
        {
                        var context = GetDatabaseContext();
            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);
            
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

                        var result = await controller.Index();

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(0, controller.ViewBag.Streak);
        }

        [Fact]
        public async Task Leaderboard_ReturnsViewResult_WithStreakAndRankedLeaders()
        {
                        var context = GetDatabaseContext();
            var user = new IdentityUser { Id = "u1", UserName = "P1" };
            context.Users.Add(user);
            context.UserProgress.Add(new UserProgress { UserId = "u1", HighestStreak = 10 });
            context.RankedScores.Add(new RankedScore { Username = "u1", TotalPoints = 100, LastPlayedDate = System.DateTime.Today });
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);

                        var result = await controller.Leaderboard();

                        var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.StreakLeaders);
            Assert.NotNull(controller.ViewBag.RankedLeaders);
        }

        [Fact]
        public async Task PublicProfile_InvalidUsername_RedirectsToLeaderboard()
        {
                        var context = GetDatabaseContext();
            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            userManagerMock.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);

                        var result = await controller.PublicProfile("nonexistent");

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Leaderboard", redirectResult.ActionName);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
                        var context = GetDatabaseContext();
            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);

                        var result = controller.Privacy();

                        Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
                        var context = GetDatabaseContext();
            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

                        var result = controller.Error();

                        var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.ViewData.Model);
            Assert.NotNull(model.RequestId);
        }
    }
}
