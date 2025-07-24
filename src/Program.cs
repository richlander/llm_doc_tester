using DotNetReleaseDocTester.Services;

namespace DotNetReleaseDocTester;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("Error: OPENROUTER_API_KEY environment variable not set");
                Console.WriteLine("Please set your OpenRouter API key:");
                Console.WriteLine("export OPENROUTER_API_KEY=your_key_here");
                return 1;
            }

            var includeAllModels = args.Contains("--all-models");
            var outputPath = GetOutputPath(args);

            Console.WriteLine(".NET Release Documentation Tester");
            Console.WriteLine("===================================");
            Console.WriteLine();

            using var httpClient = new HttpClient();
            var framework = new DotNetReleaseTestFramework(httpClient, apiKey);
            
            Console.WriteLine("Starting test suite...");
            var results = await framework.RunTestSuiteAsync(includeAllModels);
            
            Console.WriteLine();
            Console.WriteLine("Test Summary:");
            Console.WriteLine($"Total Tests: {results.TotalTests}");
            Console.WriteLine($"Successful Tests: {results.SuccessfulTests.Count}");
            Console.WriteLine($"Failed Tests: {results.FailedTests.Count}");
            Console.WriteLine($"Average Score: {results.AverageScore:F2}");
            Console.WriteLine();

            // Generate report
            ReportGenerator.GenerateMarkdownReport(results, outputPath);
            
            Console.WriteLine($"Detailed report saved to: {outputPath}");
            
            return results.FailedTests.Count > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private static string GetOutputPath(string[] args)
    {
        var outputFlag = Array.FindIndex(args, arg => arg == "--output" || arg == "-o");
        if (outputFlag >= 0 && outputFlag + 1 < args.Length)
        {
            return args[outputFlag + 1];
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmm");
        return $"test-results-{timestamp}.md";
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Usage: dotnet run [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --all-models          Test with all configured models (default: Claude only)");
        Console.WriteLine("  --output, -o <file>   Output report file (default: test-results-{timestamp}.md)");
        Console.WriteLine("  --help, -h            Show this help");
        Console.WriteLine();
        Console.WriteLine("Environment Variables:");
        Console.WriteLine("  OPENROUTER_API_KEY    Required: Your OpenRouter API key");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run");
        Console.WriteLine("  dotnet run --all-models");
        Console.WriteLine("  dotnet run --output my-report.md");
    }
}