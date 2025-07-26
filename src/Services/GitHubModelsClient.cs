using System.Text;
using System.Text.Json;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public class GitHubModelsClient
{
    private readonly HttpClient _httpClient;
    private readonly string _githubToken;

    public GitHubModelsClient(HttpClient httpClient, string githubToken)
    {
        _httpClient = httpClient;
        _githubToken = githubToken;
    }

    public async Task<string> ChatAsync(string model, List<Models.ChatMessage> messages)
    {
        var requestBody = new
        {
            model = model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            max_tokens = 2000,
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_githubToken}");

        try
        {
            var response = await _httpClient.PostAsync("https://models.github.ai/inference/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GitHubChatResponse>(responseJson);
            
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response received";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"GitHub Models API request failed: {ex.Message}", ex);
        }
    }

    public static async Task<bool> TestConnectionAsync(string githubToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            var client = new GitHubModelsClient(httpClient, githubToken);
            var testMessages = new List<Models.ChatMessage>
            {
                new("user", "Hello, this is a test. Please respond with 'GitHub Models working!'")
            };
            
            var response = await client.ChatAsync("gpt-4o-mini", testMessages);
            return response.Contains("GitHub Models working", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}