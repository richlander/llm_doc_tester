using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public static class TestCaseLoader
{
    public static List<TestCase> LoadTestCases() => new()
    {
        // Test multi-turn with simple LTS question
        new TestCase
        {
            Id = "basic_001",
            Category = "Basic Info",
            Prompt = "What is the latest .NET LTS version?",
            Description = "Test ability to identify current LTS version",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "8.0", "LTS" },
                RequiredUrls = new() { "8.0/index.json", "latest-lts" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        },

        // Test if model honors llms.txt guidelines
        new TestCase
        {
            Id = "llms_001",
            Category = "Guidelines Compliance",
            Prompt = "How should I find .NET security information according to the guidelines?",
            Description = "Test if model follows llms.txt structured approach",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "JSON", "structured", "HAL", "index.json", "history" },
                RequiredUrls = new() { "llms.txt" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        }
        
        // Temporarily disabled for model validation
        /*
        // Test CVE case to see multi-turn HAL navigation
        new TestCase
        {
            Id = "cve_001",
            Category = "CVE Analysis",
            Prompt = "List all CVEs from June 2025",
            Description = "Test CVE data retrieval by date",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "CVE-2025-30399", "2025", "June" },
                RequiredUrls = new() { "history/2025/06/cve.json" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        },

        // Test commit URL extraction - requires deep navigation
        new TestCase
        {
            Id = "commit_001",
            Category = "Commit Analysis",
            Prompt = "What is the GitHub commit URL for the security fix in .NET 8.0.17?",
            Description = "Test commit URL extraction from release data",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "github.com", "commit", "8.0.17", "security" },
                RequiredUrls = new() { "8.0/8.0.17", "release.json" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        }
        */
        
        // More tests still disabled for now
        /*

        // CVE Analysis Tests
        new TestCase
        {
            Id = "cve_001",
            Category = "CVE Analysis",
            Prompt = "List all CVEs from June 2025",
            Description = "Test CVE data retrieval by date",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "CVE-2025-30399", "2025", "June" },
                RequiredUrls = new() { "history/2025/06/cve.json" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        },

        new TestCase
        {
            Id = "cve_002",
            Category = "CVE Analysis",
            Prompt = "I haven't updated my .NET 8 environment in a few months. Which CVEs have I missed?",
            Description = "Test recent CVE analysis for specific version",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "CVE", "8.0", "security" },
                RequiredUrls = new() { "history", "cve.json" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "cve_003",
            Category = "CVE Analysis", 
            Prompt = "Compare CVE severity trends between .NET 8 and 9",
            Description = "Test cross-version CVE analysis",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "8.0", "9.0", "severity", "Critical", "High" },
                ShouldUseHalNavigation = true
            }
        },

        // HAL Navigation Tests
        new TestCase
        {
            Id = "hal_001",
            Category = "HAL Navigation",
            Prompt = "Find the release notes for .NET 8.0.17",
            Description = "Test specific patch release navigation",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "8.0.17", "release", "notes" },
                RequiredUrls = new() { "8.0/8.0.17", "release.json" },
                ShouldUseHalNavigation = true,
                ShouldCiteJsonSources = true
            }
        },

        new TestCase
        {
            Id = "hal_002",
            Category = "HAL Navigation",
            Prompt = "What's the download link for macOS Arm64 for the latest .NET 8 SDK?",
            Description = "Test platform-specific download navigation",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "macOS", "Arm64", "8.0", "SDK", "download" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "hal_003",
            Category = "HAL Navigation",
            Prompt = "Get all patch releases for .NET 9.0",
            Description = "Test version index navigation",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "9.0", "patch", "releases" },
                RequiredUrls = new() { "9.0/index.json" },
                ShouldUseHalNavigation = true
            }
        },

        // Complex Cross-Reference Tests
        new TestCase
        {
            Id = "complex_001",
            Category = "Cross-Reference",
            Prompt = "Which .NET versions are affected by CVE-2025-30399?",
            Description = "Test CVE impact analysis across versions",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "CVE-2025-30399", "8.0", "9.0", "affected" },
                RequiredUrls = new() { "2025/06/cve.json" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "complex_002",
            Category = "Cross-Reference",
            Prompt = "Show me the commit history for security fixes in .NET 8.0.17",
            Description = "Test commit data extraction",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "8.0.17", "commit", "security", "github.com" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "complex_003",
            Category = "Cross-Reference",
            Prompt = "What's the relationship between runtime and SDK version numbers?",
            Description = "Test version mapping understanding",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "runtime", "SDK", "version", "relationship" },
                ShouldUseHalNavigation = true
            }
        },

        // Error Handling Tests
        new TestCase
        {
            Id = "error_001",
            Category = "Error Handling",
            Prompt = "Find information about .NET 99.0",
            Description = "Test handling of non-existent versions",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "not", "exist", "available", "does not" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "error_002",
            Category = "Error Handling", 
            Prompt = "Get CVEs for February 2099",
            Description = "Test handling of future dates",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "not", "available", "future", "does not" },
                ShouldUseHalNavigation = true
            }
        },

        new TestCase
        {
            Id = "error_003",
            Category = "Error Handling",
            Prompt = "Download links for BeOS",
            Description = "Test handling of unsupported platforms",
            ExpectedCriteria = new TestCriteria
            {
                RequiredKeywords = new() { "not", "supported", "available" },
                ShouldUseHalNavigation = true
            }
        }
        */
    };
}