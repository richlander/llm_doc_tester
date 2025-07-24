using System.Text.Json.Serialization;

namespace DotNetReleaseDocTester.Models;

public record TestCase
{
    public string Id { get; init; } = "";
    public string Category { get; init; } = "";
    public string Prompt { get; init; } = "";
    public TestCriteria ExpectedCriteria { get; init; } = new();
    public string Description { get; init; } = "";
}

public record TestCriteria
{
    public List<string> RequiredKeywords { get; init; } = new();
    public List<string> RequiredUrls { get; init; } = new();
    public bool ShouldUseHalNavigation { get; init; } = true;
    public bool ShouldCiteJsonSources { get; init; } = true;
    public int MinResponseLength { get; init; } = 50;
    public int MaxResponseLength { get; init; } = 2000;
}

public record TestResult
{
    public string Model { get; init; } = "";
    public string TestCaseId { get; init; } = "";
    public string Prompt { get; init; } = "";
    public string Response { get; init; } = "";
    public ScoreResult Scores { get; init; } = new();
    public TimeSpan Duration { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ScoreResult
{
    public double Accuracy { get; init; }
    public double Completeness { get; init; }
    public double HalUsage { get; init; }
    public double SourceCitation { get; init; }
    public double Format { get; init; }
    public double Overall => (Accuracy + Completeness + HalUsage + SourceCitation + Format) / 5;
}

public record TestResults(List<TestResult> Results)
{
    public double AverageScore => Results.Count > 0 ? Results.Average(r => r.Scores.Overall) : 0.0;
    public int TotalTests => Results.Count;
    public List<TestResult> SuccessfulTests => Results.Where(r => r.Scores.Overall >= 0.7).ToList();
    public List<TestResult> FailedTests => Results.Where(r => r.Scores.Overall < 0.7).ToList();
}

public record ChatMessage(
    [property: JsonPropertyName("role")] string Role, 
    [property: JsonPropertyName("content")] string Content);

public record ChatResponse(
    [property: JsonPropertyName("choices")] ChatChoice[] Choices);

public record ChatChoice(
    [property: JsonPropertyName("message")] ChatMessage Message);