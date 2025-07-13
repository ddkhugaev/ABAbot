using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YandexGpt
{
    public class YandexGptClient
    {
        string _apiKey;
        readonly string _folderId;
        readonly HttpClient _httpClient;

        public YandexGptClient()
        {
            _folderId = Constants._idYandexFolder;
            _httpClient = new HttpClient();
            DotNetEnv.Env.Load();
            _apiKey = DotNetEnv.Env.GetString("YANDEX_API_KEY", "Variable not found");
        }

        public async Task<string> GetGptResponseAsync(string userMessage)
        {
            var request = new
            {
                modelUri = $"gpt://{_folderId}/yandexgpt",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.6,
                    maxTokens = "2000"
                },
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        text = "Ты бот помощник, который помогает студентам определиться с выбором направления для создания своего бизнес проекта. Генерируй по 5 гипотез. Не используй разметку Markdown!"
                    },
                    new
                    {
                        role = "user",
                        text = userMessage
                    }
                }
            };

            string url = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";
            var json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Api-Key {_apiKey}");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseContent);

            return jsonDocument.RootElement
                .GetProperty("result")
                .GetProperty("alternatives")[0]
                .GetProperty("message")
                .GetProperty("text")
                .GetString()!;
        }
    }
}
