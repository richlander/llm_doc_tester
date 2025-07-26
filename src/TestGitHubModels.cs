using DotNetReleaseDocTester.Services;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester;

public static class TestGitHubModels
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Testing GitHub Models Integration...");
        Console.WriteLine();

        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            Console.WriteLine("❌ GITHUB_TOKEN environment variable not set");
            Console.WriteLine("Please set your GitHub token:");
            Console.WriteLine("export GITHUB_TOKEN=your_token_here");
            return;
        }

        Console.WriteLine("✅ GitHub token found");
        
        // Test connection
        Console.WriteLine("🔍 Testing connection...");
        var connectionTest = await GitHubModelsClient.TestConnectionAsync(githubToken);
        if (!connectionTest)
        {
            Console.WriteLine("❌ Connection test failed");
            return;
        }
        Console.WriteLine("✅ Connection successful");

        // Test available models
        var modelsToTest = new[]
        {
            "gpt-4o",
            "gpt-4o-mini", 
            "Phi-3.5-MoE-instruct",
            "Meta-Llama-3.1-405B-Instruct",
            "Meta-Llama-3.1-70B-Instruct"
        };

        Console.WriteLine($"🧪 Testing {modelsToTest.Length} models...");
        Console.WriteLine();

        using var httpClient = new HttpClient();
        var client = new GitHubModelsClient(httpClient, githubToken);
        
        foreach (var model in modelsToTest)
        {
            Console.Write($"Testing {model}... ");
            try
            {
                var messages = new List<Models.ChatMessage>
                {
                    new("user", "What is 2+2? Respond with just the number.")
                };

                var startTime = DateTime.UtcNow;
                var response = await client.ChatAsync(model, messages);
                var duration = DateTime.UtcNow - startTime;

                Console.WriteLine($"✅ {response.Trim()} ({duration.TotalMilliseconds:F0}ms)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("GitHub Models test complete!");
    }
}