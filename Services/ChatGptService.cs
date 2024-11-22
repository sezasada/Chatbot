using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

public class ChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatGptService> _logger;
    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly string _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    public ChatGptService(HttpClient httpClient, ILogger<ChatGptService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            _logger.LogError("Prompt cannot be null or empty.");
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogError("API Key not found in environment variables.");
            throw new InvalidOperationException("API Key is missing.");
        }

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = prompt } }
        };

        try
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(OpenAiApiUrl),
                Headers =
                {
                    { "Authorization", $"Bearer {_apiKey}" }
                },
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Rate limit exceeded.");
                throw new Exception("Rate limit exceeded. Please try again later.");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<ChatGptResponse>(responseContent);

            return jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling ChatGPT API.");
            throw;
        }
    }
}

public class ChatGptResponse
{
    public List<Choice>? Choices { get; set; }
}

public class Choice
{
    public Message? Message { get; set; }
}

public class Message
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}
