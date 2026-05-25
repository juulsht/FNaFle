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
    public class CharactersControllerTests
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
        public async Task Index_ReturnsViewResult_WithListOfCharacters()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Name = "Bonnie", Gender = "Male", Generation = "Gen 1", Species = "Rabbit", Location = "Show Stage", Status = "Active" });
            db.Characters.Add(new Character { Name = "Chica", Gender = "Female", Generation = "Gen 1", Species = "Chicken", Location = "Show Stage", Status = "Active" });
            await db.SaveChangesAsync();
            var controller = new CharactersController(db);

                        var result = await controller.Index();

                        var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Character>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsInvalid()
        {
                        var db = GetDatabaseContext();
            var controller = new CharactersController(db);

                        var result = await controller.Details(99);

                        Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithCharacter()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Id = 1, Name = "Foxy", Gender = "Male", Generation = "Gen 1", Species = "Fox", Location = "Pirate Cove", Status = "Active" });
            await db.SaveChangesAsync();
            var controller = new CharactersController(db);

                        var result = await controller.Details(1);

                        var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Character>(viewResult.ViewData.Model);
            Assert.Equal("Foxy", model.Name);
        }

        [Fact]
        public async Task AddVoiceLine_RedirectsToDetails_WhenSuccessful()
        {
                        var db = GetDatabaseContext();
            var character = new Character { Name = "Foxy", Gender = "Male", Generation = "Gen 1", Species = "Fox", Location = "Pirate Cove", Status = "Active" };
            db.Characters.Add(character);
            await db.SaveChangesAsync();
            var controller = new CharactersController(db);

                        var result = await controller.AddVoiceLine(character.Id, "Never run");

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(character.Id, redirectResult.RouteValues["id"]);
            Assert.Equal(1, db.VoiceLines.Count());
        }

        [Fact]
        public async Task DeleteVoiceLine_RedirectsToDetails_AfterDeletion()
        {
                        var db = GetDatabaseContext();
            var character = new Character { Name = "Test", Gender = "Neutral", Generation = "Test", Species = "Test", Location = "Test", Status = "Test" };
            db.Characters.Add(character);
            await db.SaveChangesAsync();
            
            var vl = new VoiceLine { CharacterId = character.Id, Text = "Test" };
            db.VoiceLines.Add(vl);
            await db.SaveChangesAsync();
            var controller = new CharactersController(db);

                        var result = await controller.DeleteVoiceLine(vl.Id);

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Empty(db.VoiceLines);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
                        var db = GetDatabaseContext();
            var controller = new CharactersController(db);
            var character = new Character { Name = "New Character", Gender = "Male", Generation = "Gen 1", Species = "Bear", Location = "Pizzeria", Status = "Active" };

                        var result = await controller.Create(character);

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.Characters.Count());
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsInvalid()
        {
                        var db = GetDatabaseContext();
            var controller = new CharactersController(db);

                        var result = await controller.Edit(99);

                        Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsBadRequest_WhenIdsMismatch()
        {
                        var db = GetDatabaseContext();
            var controller = new CharactersController(db);
            var character = new Character { Id = 1, Name = "Test", Gender = "Male", Generation = "Gen 1", Species = "Bear", Location = "Pizzeria", Status = "Active" };

                        var result = await controller.Edit(2, character);

                        Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
                        var db = GetDatabaseContext();
            db.Characters.Add(new Character { Id = 1, Name = "To Delete", Gender = "Male", Generation = "Gen 1", Species = "Bear", Location = "Pizzeria", Status = "Active" });
            await db.SaveChangesAsync();
            var controller = new CharactersController(db);

                        var result = await controller.DeleteConfirmed(1);

                        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Empty(db.Characters);
        }
    }
}
