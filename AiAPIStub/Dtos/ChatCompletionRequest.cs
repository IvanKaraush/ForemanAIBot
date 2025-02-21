using AiAPIStub.Models;

namespace AiAPIStub.Dtos;

public class ChatCompletionRequest
{
    public string Model { get; set; }
    public List<Message> Messages { get; set; }
    public float Temperature { get; set; }
}