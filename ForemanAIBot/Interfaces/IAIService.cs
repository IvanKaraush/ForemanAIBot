using ForemanAIBot.DTOs;

namespace ForemanAIBot.Interfaces;

public interface IAIService
{
    Task<AIResponse> AskAIAsync(AIRequest request);
}