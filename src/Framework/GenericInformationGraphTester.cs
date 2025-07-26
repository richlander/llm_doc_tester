using DotNetReleaseDocTester.Models;
using DotNetReleaseDocTester.Services;

namespace DotNetReleaseDocTester.Framework;

public class GenericInformationGraphTester : IInformationGraphTester
{
    private readonly InformationGraphConfig _config;

    public GenericInformationGraphTester(InformationGraphConfig config)
    {
        _config = config;
    }

    public string DomainName => _config.DomainName;
    public string BaseUrl => _config.BaseUrl;

    public List<TestCase> LoadTestCases()
    {
        // Load from markdown files in tests/{domain-name} directory
        var testsDir = $"tests/{_config.DomainName.ToLowerInvariant().Replace(" ", "-")}";
        return MarkdownTestLoader.LoadFromDirectory(testsDir);
    }

    public string GetSystemPrompt()
    {
        return _config.SystemPromptTemplate;
    }

    public bool IsValidUrl(string url)
    {
        return url.StartsWith(_config.BaseUrl);
    }
}