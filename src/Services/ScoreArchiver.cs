using System.Globalization;
using System.Text;
using DotNetReleaseDocTester.Models;

namespace DotNetReleaseDocTester.Services;

public static class ScoreArchiver
{
    public static async Task ArchiveResultsAsync(TestResults results, string? outputPath = null)
    {
        var timestamp = DateTime.UtcNow;
        var csvPath = outputPath ?? $"scores-{timestamp:yyyy-MM-dd_HHmm}.csv";
        
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Timestamp,Model,TestId,Category,Prompt,Overall,Accuracy,Completeness,HalUsage,SourceCitation,Format,Duration,Success");
        
        // Data rows
        foreach (var result in results.Results)
        {
            csv.AppendLine($"{timestamp:yyyy-MM-dd HH:mm:ss}," +
                          $"{EscapeCsv(result.Model)}," +
                          $"{EscapeCsv(result.TestCaseId)}," +
                          $"{EscapeCsv(GetCategory(result.TestCaseId))}," +
                          $"{EscapeCsv(result.Prompt)}," +
                          $"{result.Scores.Overall:F3}," +
                          $"{result.Scores.Accuracy:F3}," +
                          $"{result.Scores.Completeness:F3}," +
                          $"{result.Scores.HalUsage:F3}," +
                          $"{result.Scores.SourceCitation:F3}," +
                          $"{result.Scores.Format:F3}," +
                          $"{result.Duration.TotalMilliseconds:F0}," +
                          $"{(result.Scores.Overall >= 0.7).ToString().ToLower()}");
        }
        
        await File.WriteAllTextAsync(csvPath, csv.ToString());
        Console.WriteLine($"Scores archived to: {csvPath}");
    }
    
    public static async Task AppendToMasterCsvAsync(TestResults results, string masterCsvPath = "test-scores-master.csv")
    {
        var timestamp = DateTime.UtcNow;
        var fileExists = File.Exists(masterCsvPath);
        
        var csv = new StringBuilder();
        
        // Add header if new file
        if (!fileExists)
        {
            csv.AppendLine("Timestamp,Model,TestId,Category,Prompt,Overall,Accuracy,Completeness,HalUsage,SourceCitation,Format,Duration,Success");
        }
        
        // Append data rows
        foreach (var result in results.Results)
        {
            csv.AppendLine($"{timestamp:yyyy-MM-dd HH:mm:ss}," +
                          $"{EscapeCsv(result.Model)}," +
                          $"{EscapeCsv(result.TestCaseId)}," +
                          $"{EscapeCsv(GetCategory(result.TestCaseId))}," +
                          $"{EscapeCsv(result.Prompt)}," +
                          $"{result.Scores.Overall:F3}," +
                          $"{result.Scores.Accuracy:F3}," +
                          $"{result.Scores.Completeness:F3}," +
                          $"{result.Scores.HalUsage:F3}," +
                          $"{result.Scores.SourceCitation:F3}," +
                          $"{result.Scores.Format:F3}," +
                          $"{result.Duration.TotalMilliseconds:F0}," +
                          $"{(result.Scores.Overall >= 0.7).ToString().ToLower()}");
        }
        
        await File.AppendAllTextAsync(masterCsvPath, csv.ToString());
        Console.WriteLine($"Scores appended to master CSV: {masterCsvPath}");
    }
    
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        
        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        
        return value;
    }
    
    private static string GetCategory(string testId)
    {
        return testId.Split('_')[0] switch
        {
            "basic" => "Basic Info",
            "llms" => "Guidelines Compliance", 
            "sdk" => "SDK Downloads",
            "cve" => "CVE Analysis",
            "hal" => "HAL Navigation",
            "complex" => "Cross-Reference",
            "error" => "Error Handling",
            _ => "Unknown"
        };
    }
}