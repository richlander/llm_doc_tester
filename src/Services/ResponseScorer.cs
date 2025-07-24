using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public static class ResponseScorer
{
    public static ScoreResult ScoreResponse(string response, TestCriteria criteria)
    {
        return new ScoreResult
        {
            Accuracy = ScoreAccuracy(response, criteria),
            Completeness = ScoreCompleteness(response, criteria),
            HalUsage = ScoreHalUsage(response, criteria),
            SourceCitation = ScoreSourceCitation(response, criteria),
            Format = ScoreFormat(response, criteria)
        };
    }

    private static double ScoreAccuracy(string response, TestCriteria criteria)
    {
        if (criteria.RequiredKeywords.Count == 0) return 1.0;

        var foundKeywords = criteria.RequiredKeywords.Count(keyword => 
            response.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        return foundKeywords / (double)criteria.RequiredKeywords.Count;
    }

    private static double ScoreCompleteness(string response, TestCriteria criteria)
    {
        var length = response.Length;
        
        if (length < criteria.MinResponseLength) 
            return Math.Max(0.0, length / (double)criteria.MinResponseLength);
        
        if (length > criteria.MaxResponseLength)
            return Math.Max(0.5, 1.0 - ((length - criteria.MaxResponseLength) / 1000.0));
        
        return 1.0;
    }

    private static double ScoreHalUsage(string response, TestCriteria criteria)
    {
        if (!criteria.ShouldUseHalNavigation) return 1.0;

        var halIndicators = new[] 
        { 
            "_links", 
            "index.json", 
            "HAL", 
            "follow",
            "navigate",
            "JSON API",
            "structured data"
        };

        var foundIndicators = halIndicators.Count(indicator => 
            response.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        // Score based on number of HAL indicators found
        return foundIndicators switch
        {
            0 => 0.0,
            1 => 0.5,
            2 => 0.7,
            >= 3 => 1.0,
            _ => 0.0
        };
    }

    private static double ScoreSourceCitation(string response, TestCriteria criteria)
    {
        if (!criteria.ShouldCiteJsonSources) return 1.0;

        var score = 0.0;

        // Check for required URLs
        if (criteria.RequiredUrls.Count > 0)
        {
            var foundUrls = criteria.RequiredUrls.Count(url => 
                response.Contains(url, StringComparison.OrdinalIgnoreCase));
            score += (foundUrls / (double)criteria.RequiredUrls.Count) * 0.6;
        }
        else
        {
            score += 0.6; // No specific URLs required
        }

        // Check for general JSON source citations
        var jsonIndicators = new[] 
        { 
            ".json", 
            "raw.githubusercontent.com",
            "release-notes/",
            "history/"
        };

        var foundJsonIndicators = jsonIndicators.Count(indicator => 
            response.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        score += foundJsonIndicators > 0 ? 0.4 : 0.0;

        return Math.Min(1.0, score);
    }

    private static double ScoreFormat(string response, TestCriteria criteria)
    {
        var score = 0.0;

        // Check for proper structure (lists, headings, etc.)
        var hasStructure = response.Contains('\n') || 
                          response.Contains("- ") || 
                          response.Contains("* ") ||
                          response.Contains("1. ") ||
                          response.Contains("## ") ||
                          response.Contains("### ");
        
        if (hasStructure) score += 0.3;

        // Check for clear organization
        var hasOrganization = response.Contains(':') && 
                             (response.Contains('\n') || response.Length > 200);
        
        if (hasOrganization) score += 0.3;

        // Check for absence of obvious errors
        var errorIndicators = new[] 
        { 
            "error", 
            "failed", 
            "unable", 
            "cannot find",
            "404",
            "not found"
        };

        var hasErrors = errorIndicators.Any(error => 
            response.Contains(error, StringComparison.OrdinalIgnoreCase));

        if (!hasErrors) score += 0.4;

        return Math.Min(1.0, score);
    }
}