using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Hosting;
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
        public async Task Index_UserAuthenticated_ReturnsStreakInViewBag()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new IdentityUser { Id = "user-1", UserName = "Freddy" };
            var progress = new UserProgress { UserId = "user-1", Streak = 5 };
            context.UserProgress.Add(progress);
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<HomeController>>();
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(userManagerMock.Object, new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new HomeController(loggerMock.Object, context, userManagerMock.Object, signInManagerMock.Object, webHostEnvironmentMock.Object);
            
            // Mock authentication
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Freddy") }, "mock"));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = userPrincipal }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(5, controller.ViewBag.Streak);
        }
    }
}
