using DotNetReleaseDocTester.Models;
using DotNetReleaseDocTester.Services;

namespace DotNetReleaseDocTester.Framework;

public class DotNetReleaseGraphTester : IInformationGraphTester
{
    private readonly InformationGraphConfig _config;

    public DotNetReleaseGraphTester(InformationGraphConfig config)
    {
        _config = config;
    }

    public string DomainName => _config.DomainName;
    public string BaseUrl => _config.BaseUrl;

    public List<TestCase> LoadTestCases()
    {
        // Load from markdown files only - no fallback to embedded tests
        return MarkdownTestLoader.LoadFromDirectory("tests/dotnet-releases");
    }

    public string GetSystemPrompt()
    {
        return _config.SystemPromptTemplate;
    }

    public bool IsValidUrl(string url)
    {
        return url.StartsWith(_config.BaseUrl) || 
               url.Contains("richlander/core/main/release-notes") ||
               url.Contains("richlander/core/main/llms.txt");
    }
}