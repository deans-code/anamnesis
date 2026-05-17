using System.Collections.Concurrent;
using Anamnesis.UseCase.Conversation.Contract;
using Microsoft.Extensions.Logging;

namespace Anamnesis.Interface.Website;

public class NhsIndexService : INhsIndexService
{
    private static readonly Dictionary<NhsIndexType, string> _indexUrls = new()
    {
        [NhsIndexType.Conditions] = "https://www.nhs.uk/health-a-to-z/conditions/",
        [NhsIndexType.Symptoms]   = "https://www.nhs.uk/symptoms/",
    };

    private static readonly Dictionary<NhsIndexType, string> _linkPrefixes = new()
    {
        [NhsIndexType.Conditions] = "/conditions/",
        [NhsIndexType.Symptoms]   = "/symptoms/",
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<NhsIndexService> _logger;
    private readonly ConcurrentDictionary<NhsIndexType, Dictionary<string, string>> _cache = new();
    private readonly SemaphoreSlim _fetchLock = new(1, 1);

    public NhsIndexService(HttpClient httpClient, ILogger<NhsIndexService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetNamesAsync(NhsIndexType indexType)
    {
        var index = await GetIndexAsync(indexType);
        return [.. index.Keys];
    }

    public string? GetUrl(string exactName, NhsIndexType indexType)
    {
        if (!_cache.TryGetValue(indexType, out var index))
            return null;
        return index.TryGetValue(exactName.Trim(), out var url) ? url : null;
    }

    private async Task<Dictionary<string, string>> GetIndexAsync(NhsIndexType indexType)
    {
        if (_cache.TryGetValue(indexType, out var cached))
            return cached;

        await _fetchLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(indexType, out cached))
                return cached;

            var index = await FetchIndexAsync(indexType);
            _cache[indexType] = index;
            return index;
        }
        finally
        {
            _fetchLock.Release();
        }
    }

    private async Task<Dictionary<string, string>> FetchIndexAsync(NhsIndexType indexType)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(_indexUrls[indexType]);
            var index = ParseIndex(html, _linkPrefixes[indexType]);

            if (index.Count == 0)
                _logger.LogWarning("NHS index page for {IndexType} returned no entries after parsing.", indexType);

            return index;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch NHS index page for {IndexType}. Links will not be resolved.", indexType);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, string> ParseIndex(string html, string hrefPrefix)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Find all <a href="/conditions/..."> or <a href="/symptoms/..."> tags
        var pos = 0;
        while (pos < html.Length)
        {
            var aStart = html.IndexOf("<a ", pos, StringComparison.OrdinalIgnoreCase);
            if (aStart < 0) break;

            var aEnd = html.IndexOf('>', aStart);
            if (aEnd < 0) break;

            var tag = html[aStart..(aEnd + 1)];

            var hrefIdx = tag.IndexOf("href=", StringComparison.OrdinalIgnoreCase);
            if (hrefIdx >= 0)
            {
                var quoteChar = tag[hrefIdx + 5];
                if (quoteChar == '"' || quoteChar == '\'')
                {
                    var hrefStart = hrefIdx + 6;
                    var hrefEnd = tag.IndexOf(quoteChar, hrefStart);
                    if (hrefEnd > hrefStart)
                    {
                        var href = tag[hrefStart..hrefEnd];
                        if (href.StartsWith(hrefPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var closeTag = html.IndexOf("</a>", aEnd, StringComparison.OrdinalIgnoreCase);
                            if (closeTag > aEnd)
                            {
                                var linkText = StripTags(html[(aEnd + 1)..closeTag]).Trim();
                                if (!string.IsNullOrWhiteSpace(linkText))
                                {
                                    var absoluteUrl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                        ? href
                                        : "https://www.nhs.uk" + href;

                                    index.TryAdd(linkText, absoluteUrl);
                                }
                            }
                        }
                    }
                }
            }

            pos = aEnd + 1;
        }

        return index;
    }

    private static string StripTags(string html)
    {
        var result = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(result);
    }
}
