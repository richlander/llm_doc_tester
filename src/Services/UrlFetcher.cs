using System.Text.RegularExpressions;

namespace DotNetReleaseDocTester.Services;

public static class UrlFetcher
{
    private static readonly HttpClient _httpClient = new();
    
    public static List<string> ExtractUrlRequests(string response)
    {
        // Look for patterns like "Please fetch the content from [URL]"
        var patterns = new[]
        {
            @"Please fetch (?:the )?content from (https?://[^\s\)\*\""\u201c\u201d]+)",
            @"Please fetch (?:the )?(?:data|information) from (https?://[^\s\)\*\""\u201c\u201d]+)",
            @"fetch (?:the )?content from:?\s*(https?://[^\s\)\*\""\u201c\u201d]+)",
            @"I need (?:the )?(?:data|content) from (https?://[^\s\)\*\""\u201c\u201d]+)",
            @"from (https?://[^\s\)\*\""\u201c\u201d]+)"
        };
        
        var urls = new List<string>();
        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(response, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var url = match.Groups[1].Value.TrimEnd('.', ',', ')', ']', '"', '*', '"', '"');
                if (!urls.Contains(url))
                {
                    urls.Add(url);
                }
            }
        }
        
        return urls;
    }
    
    public static async Task<string> FetchUrlContent(string url)
    {
        try
        {
            Console.WriteLine($"[FETCH] Fetching: {url}");
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[FETCH] Success: {content.Length} characters");
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FETCH] Failed: {ex.Message}");
            return $"Error fetching {url}: {ex.Message}";
        }
    }
    
    public static bool HasUrlRequests(string response)
    {
        return ExtractUrlRequests(response).Count > 0;
    }
}