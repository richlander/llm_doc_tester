using System.Text.RegularExpressions;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public static class MarkdownTestLoader
{
    public static List<TestCase> LoadFromDirectory(string testsDirectory)
    {
        var testCases = new List<TestCase>();
        
        if (!Directory.Exists(testsDirectory))
        {
            Console.WriteLine($"Warning: Tests directory not found: {testsDirectory}");
            return testCases;
        }

        var markdownFiles = Directory.GetFiles(testsDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("test-config.md", StringComparison.OrdinalIgnoreCase));
        
        foreach (var file in markdownFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                var casesFromFile = ParseMarkdownTests(content);
                testCases.AddRange(casesFromFile);
                Console.WriteLine($"Loaded {casesFromFile.Count} test cases from {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load tests from {file}: {ex.Message}");
            }
        }
        
        return testCases;
    }
    
    public static List<TestCase> ParseMarkdownTests(string markdownContent)
    {
        var testCases = new List<TestCase>();
        
        // Split by ## headers (test case sections)
        var sections = Regex.Split(markdownContent, @"^## ", RegexOptions.Multiline)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Skip(1); // Skip content before first ##
        
        foreach (var section in sections)
        {
            try
            {
                var testCase = ParseTestCase(section);
                if (testCase != null)
                {
                    testCases.Add(testCase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to parse test case section: {ex.Message}");
            }
        }
        
        return testCases;
    }
    
    private static TestCase? ParseTestCase(string section)
    {
        // Extract test ID and title from first line
        var firstLine = section.Split('\n')[0];
        var match = Regex.Match(firstLine, @"^(\w+): (.+)$");
        if (!match.Success) return null;
        
        var testId = match.Groups[1].Value;
        var title = match.Groups[2].Value;
        
        // Extract fields using regex
        var category = ExtractField(section, "Category");
        var prompt = ExtractField(section, "Prompt");
        var description = ExtractField(section, "Description");
        
        // Extract expected results
        var keywords = ExtractList(section, "Keywords");
        var urls = ExtractList(section, "URLs"); 
        var halNavigation = ExtractBoolean(section, "HAL Navigation");
        var sourceCitation = ExtractBoolean(section, "Source Citation");
        
        return new TestCase
        {
            Id = testId,
            Category = category ?? "Unknown",
            Prompt = prompt ?? "",
            Description = description ?? title,
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = keywords,
                RequiredUrls = urls,
                ShouldUseHalNavigation = halNavigation,
                ShouldCiteJsonSources = sourceCitation
            }
        };
    }
    
    private static string? ExtractField(string section, string fieldName)
    {
        var pattern = $@"\*\*{fieldName}:\*\*\s*(.+)";
        var match = Regex.Match(section, pattern);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
    
    private static List<string> ExtractList(string section, string fieldName)
    {
        var pattern = $@"\*\*{fieldName}:\*\*\s*(.+)";
        var match = Regex.Match(section, pattern);
        if (!match.Success) return new List<string>();
        
        var value = match.Groups[1].Value;
        
        // Handle backtick-separated values: `value1`, `value2`, `value3`
        var backtickMatches = Regex.Matches(value, @"`([^`]+)`");
        if (backtickMatches.Count > 0)
        {
            return backtickMatches.Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();
        }
        
        // Fallback: comma-separated
        return value.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
    
    private static bool ExtractBoolean(string section, string fieldName)
    {
        var pattern = $@"\*\*{fieldName}:\*\*\s*(.+)";
        var match = Regex.Match(section, pattern);
        if (!match.Success) return false;
        
        var value = match.Groups[1].Value.Trim().ToLowerInvariant();
        return value == "required" || value == "true" || value == "yes";
    }
    
    public static string? LoadSystemPromptFromConfig(string testsDirectory)
    {
        var configPath = Path.Combine(testsDirectory, "test-config.md");
        if (!File.Exists(configPath))
        {
            return null;
        }
        
        try
        {
            var content = File.ReadAllText(configPath);
            
            // Extract system prompt section
            var systemPromptStart = content.IndexOf("## System Prompt");
            if (systemPromptStart == -1) return null;
            
            var nextSection = content.IndexOf("##", systemPromptStart + 1);
            var systemPromptSection = nextSection == -1 
                ? content.Substring(systemPromptStart) 
                : content.Substring(systemPromptStart, nextSection - systemPromptStart);
            
            // Remove the header and extract the prompt text
            var lines = systemPromptSection.Split('\n');
            var promptLines = lines.Skip(2).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            
            return string.Join("\n", promptLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load test config: {ex.Message}");
            return null;
        }
    }
}