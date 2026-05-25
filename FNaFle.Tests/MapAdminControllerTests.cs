using FNaFle.Controllers;
using FNaFle.Data;
using FNaFle.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FNaFle.Tests
{
    public class MapAdminControllerTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithMapLocations()
        {
                        var db = GetDatabaseContext();
            db.MapLocations.Add(new MapLocation { GameName = "A", CameraName = "1" });
            db.MapLocations.Add(new MapLocation { GameName = "B", CameraName = "2" });
            await db.SaveChangesAsync();
            var controller = new MapAdminController(db);

                        var result = await controller.Index();

                        var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<MapLocation>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenValid()
        {
                        var db = GetDatabaseContext();
            var controller = new MapAdminController(db);
            var location = new MapLocation { GameName = "Test", CameraName = "Test" };

                        var result = await controller.Create(location);

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.MapLocations.Count());
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdInvalid()
        {
                        var db = GetDatabaseContext();
            var controller = new MapAdminController(db);

                        var result = await controller.Delete(99);

                        Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesLocation()
        {
                        var db = GetDatabaseContext();
            var loc = new MapLocation { GameName = "A", CameraName = "1" };
            db.MapLocations.Add(loc);
            await db.SaveChangesAsync();
            var controller = new MapAdminController(db);

                        var result = await controller.DeleteConfirmed(loc.Id);

                        Assert.Equal(0, db.MapLocations.Count());
        }

        [Fact] public void Create_Get_ReturnsView() { var c = new MapAdminController(GetDatabaseContext()); Assert.IsType<ViewResult>(c.Create()); }
    }
}
