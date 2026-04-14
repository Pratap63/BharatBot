using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChatBot.Services
{
    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public OpenAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GenerateAnswerAsync(string question)
        {
            var apiKey = _config["OpenAI:ApiKey"];

            var requestBody = new
            {
                model = "gpt-4o", // example
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful technical assistant, Answers should be short and crisp, it should be properly formatted." },
                    new { role = "user", content = question }
                }
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://models.github.ai/inference/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return "AI service is currently unavailable.";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No response from AI.";
        }
    }
}
