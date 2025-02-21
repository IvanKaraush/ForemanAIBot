using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ForemanAIBot.Primitives;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, string apiKey = null)
        where TRequest : class
        where TResponse : class
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            AppConstants.JsonMediaType
        );

        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        var response = await _httpClient.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Ошибка при запросе к API: {response.StatusCode}. Тело ответа: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<TResponse>(responseContent);
        
        return responseObject;
    }
}