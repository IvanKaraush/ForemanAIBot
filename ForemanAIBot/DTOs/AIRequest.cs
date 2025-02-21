using ForemanAIBot.Primitives;

namespace ForemanAIBot.DTOs;

public sealed record AIRequest(Specialization Role, string UserMessage);