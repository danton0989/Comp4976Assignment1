using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ObituaryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIAssistantController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIAssistantController> _logger;
    private readonly string _apiKey;

    public AIAssistantController(HttpClient httpClient, ILogger<AIAssistantController> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["GoogleAI:ApiKey"] ?? throw new InvalidOperationException("Google AI API key not configured");
    }

    [HttpPost("famous-death")]
    public async Task<IActionResult> GetFamousDeath([FromBody] AIRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PersonName))
        {
            return BadRequest(new { error = "Person name is required" });
        }

        try
        {
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            
            var prompt = $"Provide a brief, factual summary of how {request.PersonName} died, including the date, location, and cause of death. Keep it concise and respectful.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("Request body: " + jsonBody);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            
            // Add API key as header instead of query parameter
            httpRequest.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Response Status: {response.StatusCode}");
            _logger.LogInformation($"Response Content Length: {content?.Length ?? 0}");
            _logger.LogInformation($"Response Content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gemini API error: {response.StatusCode} - {content}");
                return StatusCode((int)response.StatusCode, new { error = $"AI service error: {content}" });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogError("Empty response from Gemini API");
                return StatusCode(502, new { error = "Empty response from AI service" });
            }

            var result = JsonSerializer.Deserialize<GeminiResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            if (result?.candidates != null && result.candidates.Any())
            {
                var text = result.candidates[0].content?.parts?[0].text;
                return Ok(new { response = text?.Trim() ?? "No information available." });
            }
            
            return Ok(new { response = "No information available." });
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON parsing error");
            return StatusCode(500, new { error = $"Failed to parse AI response: {jsonEx.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service");
            return StatusCode(500, new { error = $"Server error: {ex.Message}" });
        }
    }

    public class AIRequest
    {
        public string PersonName { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        public List<GeminiCandidate>? candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    private class GeminiContent
    {
        public List<GeminiPart>? parts { get; set; }
    }

    private class GeminiPart
    {
        public string? text { get; set; }
    }
}