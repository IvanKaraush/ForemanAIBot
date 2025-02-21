using ForemanAIBot.DTOs;
using ForemanAIBot.Interfaces;
using ForemanAIBot.Options;
using ForemanAIBot.Primitives;
using Microsoft.Extensions.Options;

namespace ForemanAIBot.Services;

public class DeepSeekService : IAIService
{
    private readonly ApiClient _apiClient;
    private readonly AIConfiguration _config;

    public DeepSeekService(ApiClient apiClient, IOptions<AIConfiguration> options)
    {
        _apiClient = apiClient;
        _config = options.Value;
    }

    public async Task<AIResponse> AskAIAsync(AIRequest request)
    {
        var roleKey = request.Role.ToString();

        // Получаем промпт из конфигурации
        var prompt = _config.Prompts?.GetValueOrDefault(roleKey);
        if (string.IsNullOrEmpty(prompt))
        {
            throw new ArgumentException($"Промпт для роли '{roleKey}' не найден в конфигурации.");
        }
        
        var requestBody = new
        {
            model = _config.Model,
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = request.UserMessage }
            },
            max_tokens = _config.MaxTokens
        };

        // Выполняем запрос через ApiClient
        var response = await _apiClient.PostAsync<object, DeepSeekResponse>(
            _config.BaseUrl,
            requestBody,
            _config.ApiKey
        );

        return new AIResponse(response?.ResponseMessage ?? "Не удалось получить ответ от ИИ.");
    }
}