namespace ForemanAIBot.DTOs;

public sealed record DeepSeekResponse
{
    public string ResponseMessage { get; init; } = string.Empty;
}