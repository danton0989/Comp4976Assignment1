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

    public AIAssistantController(HttpClient httpClient, ILogger<AIAssistantController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
            var apiUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2";
            
            var prompt = $"Provide a brief, factual summary of how {request.PersonName} died, including the date, location, and cause of death. Keep it concise and respectful.";

            var requestBody = new
            {
                inputs = prompt,
                parameters = new
                {
                    max_new_tokens = 200,
                    temperature = 0.7,
                    return_full_text = false
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Raw HuggingFace response: " + content);
                var result = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(content);
                if (result != null && result.Any())
                {
                    return Ok(new { response = result[0].generated_text?.Trim() });
                }
                return Ok(new { response = "No information available." });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return StatusCode(503, new { error = "AI model is loading. Please wait 20 seconds and try again." });
            }
            else
            {
                _logger.LogError($"HuggingFace API error: {response.StatusCode} - {content}");
                return StatusCode((int)response.StatusCode, new { error = "AI service error. Please try again." });
            }
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

    private class HuggingFaceResponse
    {
        public string? generated_text { get; set; }
    }
}