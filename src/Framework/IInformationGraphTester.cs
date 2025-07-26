using DotNetReleaseDocTester.Models;
using DotNetReleaseDocTester.Services;

namespace DotNetReleaseDocTester.Framework;

/// <summary>
/// Generic interface for testing AI model navigation of structured information graphs
/// </summary>
public interface IInformationGraphTester
{
    /// <summary>
    /// The name of this information domain (e.g., ".NET Releases", ".NET Documentation")
    /// </summary>
    string DomainName { get; }
    
    /// <summary>
    /// Base URL for the information graph
    /// </summary>
    string BaseUrl { get; }
    
    /// <summary>
    /// Load test cases specific to this information domain
    /// </summary>
    List<TestCase> LoadTestCases();
    
    /// <summary>
    /// Get the system prompt that instructs models how to navigate this graph
    /// </summary>
    string GetSystemPrompt();
    
    /// <summary>
    /// Validate that a URL is within the expected domain for this graph
    /// </summary>
    bool IsValidUrl(string url);
    
    /// <summary>
    /// Custom scoring criteria specific to this domain
    /// </summary>
    ScoreResult ScoreResponse(string response, TestCriteria criteria) => 
        ResponseScorer.ScoreResponse(response, criteria);
}

/// <summary>
/// Configuration for an information graph testing domain
/// </summary>
public record InformationGraphConfig(
    string DomainName,
    string BaseUrl,
    string SystemPromptTemplate,
    string TestCasesPath,
    Dictionary<string, string>? CustomProperties = null);

/// <summary>
/// Factory for creating domain-specific testers
/// </summary>
public static class InformationGraphTesterFactory
{
    public static IInformationGraphTester Create(InformationGraphConfig config)
    {
        return config.DomainName.ToLowerInvariant() switch
        {
            "dotnet-releases" => new DotNetReleaseGraphTester(config),
            "dotnet-docs" => new DotNetDocsGraphTester(config),
            _ => new GenericInformationGraphTester(config)
        };
    }
    
    public static InformationGraphConfig DotNetReleasesConfig => new(
        DomainName: ".NET Releases",
        BaseUrl: "https://raw.githubusercontent.com/richlander/core/main/release-notes",
        SystemPromptTemplate: GetDotNetReleaseSystemPrompt(),
        TestCasesPath: "Services/TestCaseLoader.cs");
        
    public static InformationGraphConfig DotNetDocsConfig => new(
        DomainName: ".NET Documentation", 
        BaseUrl: "https://raw.githubusercontent.com/richlander/docs_llm/main",
        SystemPromptTemplate: GetDotNetDocsSystemPrompt(),
        TestCasesPath: "TestCases/DotNetDocsTests.cs");
    
    private static string GetDotNetReleaseSystemPrompt() => """
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
        
    private static string GetDotNetDocsSystemPrompt() => """
        You are an AI assistant helping with .NET feature documentation for zero-shot code generation.
        Use the structured data available at https://raw.githubusercontent.com/richlander/docs_llm/main/llms.txt
        
        IMPORTANT: You cannot directly access URLs. Instead, when you need content from a URL, 
        ask me to fetch it by saying: "Please fetch the content from [URL]" and I will provide it to you.
        
        Guidelines:
        - Start with the JSON API: https://raw.githubusercontent.com/richlander/docs_llm/main/index.json
        - Follow HAL _links for feature discovery and navigation
        - Use token-dense markdown files for detailed feature information
        - Focus on generating correct, idiomatic .NET code
        - For CLI features, use: https://raw.githubusercontent.com/richlander/docs_llm/main/cli/index.json
        - For library features, use: https://raw.githubusercontent.com/richlander/docs_llm/main/libraries/index.json
        - For C# features, use: https://raw.githubusercontent.com/richlander/docs_llm/main/csharp/index.json
        
        Always prefer the structured documentation over general knowledge.
        Request URLs from me when you need them.
        """;
}