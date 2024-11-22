using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatGptService _chatGptService;

    public ChatController(ChatGptService chatGptService)
    {
        _chatGptService = chatGptService;
    }

    [HttpPost]
    [Route("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var response = await _chatGptService.GetChatGptResponse(request.Message);
        return Ok(new { response });
    }
}

public class ChatRequest
{
    public string? Message { get; set; }
}
