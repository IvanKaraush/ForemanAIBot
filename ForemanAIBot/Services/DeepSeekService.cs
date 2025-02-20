using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ForemanAIBot.DTOs;
using ForemanAIBot.Primitives;
using Microsoft.Extensions.Configuration;

namespace ForemanAIBot.Services;

public class DeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;

    public DeepSeekService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _configuration = configuration;
        _apiKey = _configuration["DeepSeekConfiguration:ApiKey"];
    }

    public async Task<string> AskAIAsync(Specialization role, string userMessage)
    {
        var roleKey = role.ToString();

        // Получаем промпт из конфигурации в зависимости от роли
        var prompt = _configuration[$"Prompts:{roleKey}"];
        if (string.IsNullOrEmpty(prompt))
        {
            throw new ArgumentException($"Промпт для роли '{roleKey}' не найден в конфигурации.");
        }

        // Создаем объект настроек запроса
        var requestBody = new
        {
            model = "deepseek-v3",
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = userMessage }
            },
            max_tokens = 150
        };
        
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        var response = await _httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Ошибка при запросе к DeepSeek: {response.StatusCode}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);

        return responseObject?.ResponseMessage ?? "Не удалось получить ответ от ИИ.";
    }
}