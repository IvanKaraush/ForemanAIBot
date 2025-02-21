namespace ForemanAIBot.Options;

public sealed record AIConfiguration
{
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int MaxTokens { get; init; } = 100;
    public Dictionary<string, string> Prompts { get; init; } = new();
}