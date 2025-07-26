using DotNetReleaseDocTester.Models;
using DotNetReleaseDocTester.Services;

namespace DotNetReleaseDocTester.Framework;

public class DotNetDocsGraphTester : IInformationGraphTester
{
    private readonly InformationGraphConfig _config;

    public DotNetDocsGraphTester(InformationGraphConfig config)
    {
        _config = config;
    }

    public string DomainName => _config.DomainName;
    public string BaseUrl => _config.BaseUrl;

    public List<TestCase> LoadTestCases()
    {
        // Load from markdown files
        return MarkdownTestLoader.LoadFromDirectory("tests/dotnet-docs");
    }

    public string GetSystemPrompt()
    {
        return _config.SystemPromptTemplate;
    }

    public bool IsValidUrl(string url)
    {
        return url.StartsWith(_config.BaseUrl) || 
               url.Contains("richlander/docs_llm/main") ||
               url.Contains("docs_llm/main/llms.txt");
    }
}