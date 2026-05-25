using FNaFle.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace FNaFle.Tests
{
    public class ChatControllerTests
    {
        [Fact]
        public async Task AskLore_ReturnsSystemError_WhenMessageIsEmpty()
        {
                        var controller = new ChatController();

                        var result = await controller.AskLore("");

                        var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("NULL_INPUT_DETECTED", json);
        }
        
        [Fact]
        public async Task AskLore_ReturnsSystemError_WhenMessageIsWhitespace()
        {
                        var controller = new ChatController();

                        var result = await controller.AskLore("   ");

                        var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("NULL_INPUT_DETECTED", json);
        }
        [Fact]
        public async Task AskLore_ReturnsSystemError_WhenMessageIsTooLong()
        {
                        var controller = new ChatController();
            var longMessage = new string('a', 5001);

                        var result = await controller.AskLore(longMessage);

                        var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

    }
}
