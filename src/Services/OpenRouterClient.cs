using System.Text;
using System.Text.Json;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public class OpenRouterClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenRouterClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _httpClient.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/richlander/core");
        _httpClient.DefaultRequestHeaders.Add("X-Title", ".NET Release Documentation Tester");
    }

    public async Task<string> ChatAsync(string model, List<ChatMessage> messages, double temperature = 0.1)
    {
        var request = new
        {
            model = model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = temperature,
            max_tokens = 2000
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChatResponse>(responseJson);
            
            return result?.Choices?[0]?.Message?.Content ?? "No response received";
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"API request failed: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse API response: {ex.Message}", ex);
        }
    }

}