using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public class DotNetReleaseTestFramework
{
    private readonly OpenRouterClient _client;
    private readonly string[] _models;
    private readonly string? _testsPath;

    public DotNetReleaseTestFramework(HttpClient httpClient, string apiKey, string[]? models = null, string? testsPath = null)
    {
        _client = new OpenRouterClient(httpClient, apiKey);
        _testsPath = testsPath;
        _models = models ?? new[]
        {
            "anthropic/claude-3.5-sonnet",
            "anthropic/claude-3.5-haiku", // Often free on OpenRouter
            "openai/gpt-4o", // GPT-4o latest
            "openai/gpt-4o-mini", // Let's try the exact model ID
            "deepseek/deepseek-chat", // Free model - excellent performance!
            "moonshotai/kimi-k2:free", // Kimi K2 reasoning model (free, excellent performance)
            "google/gemini-pro-1.5"
        };
    }

    public async Task<TestResults> RunTestSuiteAsync(bool includeAllModels = false)
    {
        var testCases = LoadTestCases();
        var results = new List<TestResult>();
        var modelsToTest = includeAllModels ? _models : new[] { _models[0] }; // Default to just Claude for faster testing

        Console.WriteLine($"Running {testCases.Count} test cases across {modelsToTest.Length} model(s)...");
        Console.WriteLine($"Models: {string.Join(", ", modelsToTest)}");
        Console.WriteLine();

        foreach (var model in modelsToTest)
        {
            Console.WriteLine($"Testing model: {model}");
            
            foreach (var testCase in testCases)
            {
                try
                {
                    Console.Write($"  Running {testCase.Id} ({testCase.Category})... ");
                    
                    var result = await RunSingleTestAsync(model, testCase);
                    results.Add(result);
                    
                    Console.WriteLine($"Score: {result.Scores.Overall:F2} ({result.Duration.TotalMilliseconds:F0}ms)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                    
                    // Add failed result
                    results.Add(new TestResult
                    {
                        Model = model,
                        TestCaseId = testCase.Id,
                        Prompt = testCase.Prompt,
                        Response = $"ERROR: {ex.Message}",
                        Scores = new ScoreResult(),
                        Duration = TimeSpan.Zero,
                        Timestamp = DateTime.UtcNow
                    });
                }
                
                // Small delay to respect rate limits
                await Task.Delay(500);
            }
            
            Console.WriteLine();
        }

        return new TestResults(results);
    }

    private List<TestCase> LoadTestCases()
    {
        if (!string.IsNullOrEmpty(_testsPath))
        {
            // Load from external path
            Console.WriteLine($"Loading tests from: {_testsPath}");
            return MarkdownTestLoader.LoadFromDirectory(_testsPath);
        }
        
        // Load from default location
        var markdownTests = MarkdownTestLoader.LoadFromDirectory("tests/dotnet-releases");
        if (markdownTests.Count > 0)
        {
            return markdownTests;
        }
        
        // If no markdown tests found, return empty list (no fallback to embedded tests)
        Console.WriteLine("Warning: No test cases found. Use --tests to specify test directory.");
        Console.WriteLine("Exiting with no tests to run.");
        Environment.Exit(0);
        return new List<TestCase>(); // Never reached
    }

    private async Task<TestResult> RunSingleTestAsync(string model, TestCase testCase)
    {
        var messages = new List<ChatMessage>
        {
            new("system", GetSystemPrompt()),
            new("user", testCase.Prompt)
        };

        var startTime = DateTime.UtcNow;
        var finalResponse = "";
        var conversationLog = new List<string>();
        var maxTurns = 10; // Prevent infinite loops
        var turnCount = 0;

        // Multi-turn conversation loop
        while (turnCount < maxTurns)
        {
            turnCount++;
            Console.WriteLine($"[TURN {turnCount}] Making API call...");
            var response = await _client.ChatAsync(model, messages);
            Console.WriteLine($"[TURN {turnCount}] Response: {response.Substring(0, Math.Min(200, response.Length))}...");
            conversationLog.Add($"Turn {turnCount}: {response}");
            
            // Check if this response contains URL requests
            var requestedUrls = UrlFetcher.ExtractUrlRequests(response);
            Console.WriteLine($"[TURN {turnCount}] Found {requestedUrls.Count} URL requests: {string.Join(", ", requestedUrls)}");
            
            if (requestedUrls.Count == 0)
            {
                // No more URL requests - this is the final answer
                Console.WriteLine($"[TURN {turnCount}] Final answer received");
                finalResponse = response;
                break;
            }
            
            // Fetch all requested URLs and provide content back
            var urlContents = new List<string>();
            foreach (var url in requestedUrls)
            {
                var content = await UrlFetcher.FetchUrlContent(url);
                urlContents.Add($"Content from {url}:\n{content}");
            }
            
            // Add assistant's request and our response to conversation
            messages.Add(new("assistant", response));
            messages.Add(new("user", string.Join("\n\n", urlContents)));
            Console.WriteLine($"[TURN {turnCount}] Provided {urlContents.Count} URL contents, continuing conversation...");
        }
        
        var duration = DateTime.UtcNow - startTime;
        
        // If we hit max turns without a final answer, use the last response
        if (string.IsNullOrEmpty(finalResponse))
        {
            finalResponse = conversationLog.LastOrDefault() ?? "No response received";
        }

        var scores = ResponseScorer.ScoreResponse(finalResponse, testCase.ExpectedCriteria);

        return new TestResult
        {
            Model = model,
            TestCaseId = testCase.Id,
            Prompt = testCase.Prompt,
            Response = finalResponse,
            Scores = scores,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetSystemPrompt()
    {
        // Try to load custom system prompt from test config
        if (!string.IsNullOrEmpty(_testsPath))
        {
            var customPrompt = MarkdownTestLoader.LoadSystemPromptFromConfig(_testsPath);
            if (!string.IsNullOrEmpty(customPrompt))
            {
                Console.WriteLine("Using custom system prompt from test-config.md");
                return customPrompt;
            }
        }
        
        // Default to release-notes system prompt
        return GetDefaultSystemPrompt();
    }
    
    private static string GetDefaultSystemPrompt() => """
        You are an AI assistant helping with .NET release information. 
        Use the structured data available at https://raw.githubusercontent.com/richlander/core/main/llms.txt
        
        IMPORTANT: You cannot directly access URLs. Instead, when you need content from a URL, 
        ask me to fetch it by saying: "Please fetch the content from [URL]" and I will provide it to you.
        
        Guidelines:
        - Start with the JSON API: https://raw.githubusercontent.com/richlander/core/main/release-notes/index.json
        - Follow HAL _links for discovery and traversal
        - Use JSON files primarily; markdown as fallback
        - Cite specific JSON endpoints in your responses
        - For CVE data, use: https://raw.githubusercontent.com/richlander/core/main/release-notes/archives/index.json
        - For version-specific data, use: https://raw.githubusercontent.com/richlander/core/main/release-notes/{version}/index.json
        
        Always prefer the structured JSON data over web searches or general knowledge.
        Request URLs from me when you need them.
        """;

}