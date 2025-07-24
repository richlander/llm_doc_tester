using System.Text;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public static class ReportGenerator
{
    public static void GenerateMarkdownReport(TestResults results, string outputPath)
    {
        var report = new StringBuilder();
        
        report.AppendLine("# .NET Release Documentation Testing Report");
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine($"Total Tests: {results.TotalTests}");
        report.AppendLine($"Overall Average Score: {results.AverageScore:F2}");
        report.AppendLine();

        // Executive Summary
        GenerateExecutiveSummary(report, results);

        // Model Performance Comparison
        GenerateModelComparison(report, results);

        // Category Breakdown
        GenerateCategoryBreakdown(report, results);

        // Detailed Results
        GenerateDetailedResults(report, results);

        // Recommendations
        GenerateRecommendations(report, results);

        File.WriteAllText(outputPath, report.ToString());
        Console.WriteLine($"Report generated: {outputPath}");
    }

    private static void GenerateExecutiveSummary(StringBuilder report, TestResults results)
    {
        report.AppendLine("## Executive Summary");
        
        var topScores = results.Results
            .Where(r => !r.Response.StartsWith("ERROR"))
            .OrderByDescending(r => r.Scores.Overall)
            .Take(3);

        var bottomScores = results.Results
            .Where(r => !r.Response.StartsWith("ERROR"))
            .OrderBy(r => r.Scores.Overall)
            .Take(3);

        report.AppendLine("### Best Performing Tests");
        foreach (var result in topScores)
        {
            report.AppendLine($"- **{result.TestCaseId}** ({result.Model}): {result.Scores.Overall:F2}");
            report.AppendLine($"  - *{GetTestDescription(result.TestCaseId)}*");
        }

        report.AppendLine();
        report.AppendLine("### Areas for Improvement");
        foreach (var result in bottomScores)
        {
            report.AppendLine($"- **{result.TestCaseId}** ({result.Model}): {result.Scores.Overall:F2}");
            report.AppendLine($"  - *{GetTestDescription(result.TestCaseId)}*");
        }
        report.AppendLine();
    }

    private static void GenerateModelComparison(StringBuilder report, TestResults results)
    {
        var modelStats = results.Results
            .Where(r => !r.Response.StartsWith("ERROR"))
            .GroupBy(r => r.Model)
            .Select(g => new
            {
                Model = g.Key,
                Average = g.Average(r => r.Scores.Overall),
                Count = g.Count(),
                AvgAccuracy = g.Average(r => r.Scores.Accuracy),
                AvgHalUsage = g.Average(r => r.Scores.HalUsage),
                AvgSourceCitation = g.Average(r => r.Scores.SourceCitation),
                AvgDuration = g.Average(r => r.Duration.TotalMilliseconds)
            })
            .OrderByDescending(x => x.Average);

        report.AppendLine("## Model Performance Comparison");
        report.AppendLine();
        report.AppendLine("| Model | Overall | Accuracy | HAL Usage | Source Citation | Avg Duration (ms) | Tests |");
        report.AppendLine("|-------|---------|----------|-----------|-----------------|-------------------|-------|");

        foreach (var model in modelStats)
        {
            report.AppendLine($"| {model.Model} | {model.Average:F2} | {model.AvgAccuracy:F2} | {model.AvgHalUsage:F2} | {model.AvgSourceCitation:F2} | {model.AvgDuration:F0} | {model.Count} |");
        }
        report.AppendLine();
    }

    private static void GenerateCategoryBreakdown(StringBuilder report, TestResults results)
    {
        var testCases = TestCaseLoader.LoadTestCases();
        var categoryStats = results.Results
            .Where(r => !r.Response.StartsWith("ERROR"))
            .Join(testCases, r => r.TestCaseId, t => t.Id, (r, t) => new { r.Scores.Overall, t.Category })
            .GroupBy(x => x.Category)
            .Select(g => new
            {
                Category = g.Key,
                Average = g.Average(x => x.Overall),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Average);

        report.AppendLine("## Performance by Test Category");
        report.AppendLine();
        report.AppendLine("| Category | Average Score | Test Count |");
        report.AppendLine("|----------|---------------|------------|");

        foreach (var category in categoryStats)
        {
            report.AppendLine($"| {category.Category} | {category.Average:F2} | {category.Count} |");
        }
        report.AppendLine();
    }

    private static void GenerateDetailedResults(StringBuilder report, TestResults results)
    {
        report.AppendLine("## Detailed Test Results");
        
        var testCases = TestCaseLoader.LoadTestCases();
        var groupedResults = results.Results
            .Join(testCases, r => r.TestCaseId, t => t.Id, (r, t) => new { Result = r, TestCase = t })
            .GroupBy(x => x.TestCase.Category)
            .OrderBy(g => g.Key);

        foreach (var categoryGroup in groupedResults)
        {
            report.AppendLine($"### {categoryGroup.Key}");
            report.AppendLine();

            foreach (var item in categoryGroup.OrderBy(x => x.Result.TestCaseId))
            {
                var result = item.Result;
                var testCase = item.TestCase;

                report.AppendLine($"#### {result.TestCaseId}: {testCase.Description}");
                report.AppendLine($"**Prompt:** {testCase.Prompt}");
                report.AppendLine($"**Model:** {result.Model}");
                report.AppendLine($"**Overall Score:** {result.Scores.Overall:F2}");
                report.AppendLine();
                
                report.AppendLine("**Score Breakdown:**");
                report.AppendLine($"- Accuracy: {result.Scores.Accuracy:F2}");
                report.AppendLine($"- Completeness: {result.Scores.Completeness:F2}");
                report.AppendLine($"- HAL Usage: {result.Scores.HalUsage:F2}");
                report.AppendLine($"- Source Citation: {result.Scores.SourceCitation:F2}");
                report.AppendLine($"- Format: {result.Scores.Format:F2}");
                report.AppendLine();

                if (result.Response.StartsWith("ERROR"))
                {
                    report.AppendLine($"**Error:** {result.Response}");
                }
                else
                {
                    report.AppendLine("**Response:**");
                    report.AppendLine("```");
                    report.AppendLine(result.Response.Length > 500 
                        ? result.Response[..500] + "..." 
                        : result.Response);
                    report.AppendLine("```");
                }
                report.AppendLine();
            }
        }
    }

    private static void GenerateRecommendations(StringBuilder report, TestResults results)
    {
        report.AppendLine("## Recommendations");
        
        var avgScores = new
        {
            Accuracy = results.Results.Where(r => !r.Response.StartsWith("ERROR")).Average(r => r.Scores.Accuracy),
            HalUsage = results.Results.Where(r => !r.Response.StartsWith("ERROR")).Average(r => r.Scores.HalUsage),
            SourceCitation = results.Results.Where(r => !r.Response.StartsWith("ERROR")).Average(r => r.Scores.SourceCitation),
            Format = results.Results.Where(r => !r.Response.StartsWith("ERROR")).Average(r => r.Scores.Format)
        };

        report.AppendLine("### Documentation Improvements");
        
        if (avgScores.HalUsage < 0.7)
        {
            report.AppendLine("- **Improve HAL Navigation Guidance**: Average HAL usage score is low. Consider:");
            report.AppendLine("  - Adding more explicit examples of HAL navigation in llms.txt");
            report.AppendLine("  - Providing step-by-step HAL traversal examples");
            report.AppendLine("  - Highlighting the _links structure more prominently");
        }

        if (avgScores.SourceCitation < 0.7)
        {
            report.AppendLine("- **Enhance Source Citation Examples**: Models aren't citing JSON sources effectively. Consider:");
            report.AppendLine("  - Adding specific URL templates in llms.txt");
            report.AppendLine("  - Providing more concrete examples of JSON endpoint usage");
            report.AppendLine("  - Emphasizing the importance of citing raw GitHub URLs");
        }

        if (avgScores.Accuracy < 0.8)
        {
            report.AppendLine("- **Improve Content Accuracy**: Consider:");
            report.AppendLine("  - Adding more specific examples in llms.txt");
            report.AppendLine("  - Providing clearer terminology definitions");
            report.AppendLine("  - Adding validation examples for common queries");
        }

        report.AppendLine();
        report.AppendLine("### Testing Improvements");
        report.AppendLine("- Add more edge case tests");
        report.AppendLine("- Include performance benchmarks");
        report.AppendLine("- Test with different prompt styles");
        report.AppendLine("- Add regression testing for documentation changes");
    }

    private static string GetTestDescription(string testId)
    {
        var testCase = TestCaseLoader.LoadTestCases().FirstOrDefault(t => t.Id == testId);
        return testCase?.Description ?? "Unknown test";
    }
}