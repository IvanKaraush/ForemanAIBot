using AiAPIStub.Dtos;
using AiAPIStub.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiAPIStub.Controllers;

[ApiController]
[Route("v1/chat/completions")] 
public class ChatController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateChatCompletion([FromBody] ChatCompletionRequest request)
    {
        var response = new ChatCompletionResponse
        {
            Id = "chatcmpl-abc123",
            Object = "chat.completion",
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = request.Model,
            Usage = new Usage
            {
                PromptTokens = request.Messages.Sum(m => m.Content.Length) / 4, 
                CompletionTokens = 7, 
                TotalTokens = (request.Messages.Sum(m => m.Content.Length) / 4) + 7
            },
            Choices = new List<Choice>
            {
                new Choice
                {
                    Message = new Message
                    {
                        Role = "assistant",
                        Content = "This is a test!"
                    },
                    Logprobs = null,
                    FinishReason = "stop",
                    Index = 0
                }
            }
        };

        return Ok(response);
    }
}