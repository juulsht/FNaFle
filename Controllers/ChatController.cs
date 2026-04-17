using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FNaFle.Controllers
{
    public class ChatController : Controller
    {
        private readonly string _apiKey = "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";

        [HttpPost]
        public async Task<IActionResult> AskLore(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "SYSTEM ERROR: NULL_INPUT_DETECTED." });

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            client.DefaultRequestHeaders.Add("X-Title", "FNaFle");

            var payload = new
            {
                model = "openai/gpt-3.5-turbo",
                max_tokens = 500,
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "You are a helpful Fazbear Entertainment AI assistant. Provide clear and concise answers to questions about FNaF lore."
                    },
                    new {
                        role = "user",
                        content = message
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            var response = await client.PostAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { reply = "SYSTEM ERROR: The AI service is currently unavailable or the account limit has been reached." });
            }

            using var doc = JsonDocument.Parse(responseString);

            if (doc.RootElement.TryGetProperty("error", out var errorProp))
            {
                var errorMsg = errorProp.TryGetProperty("message", out var m) ? m.GetString() : "Unknown API Error";
                return Json(new { reply = $"API ERROR: {errorMsg}" });
            }

            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return Json(new { reply = "SYSTEM ERROR: NO_RESPONSE_FROM_AI." });
            }

            string aiReply = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Json(new { reply = aiReply });
        }
    }
}