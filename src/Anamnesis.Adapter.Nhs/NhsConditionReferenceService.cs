using System.Collections.Concurrent;
using Anamnesis.Domain;
using Anamnesis.Adapter.MedicalData.Contract;
using Microsoft.Extensions.Logging;

namespace Anamnesis.Adapter.Nhs;

public class NhsConditionReferenceService : IMedicalReferenceService
{
    private const string ConditionsIndexUrl = "https://www.nhs.uk/health-a-to-z/conditions/";
    private const string SymptomsIndexUrl = "https://www.nhs.uk/symptoms/";
    private const string ConditionsLinkPrefix = "/conditions/";
    private const string SymptomsLinkPrefix = "/symptoms/";

    private readonly HttpClient _httpClient;
    private readonly ILogger<NhsConditionReferenceService> _logger;
    private readonly ConcurrentDictionary<string, IReadOnlyList<RelatedCondition>> _cache = new();
    private readonly SemaphoreSlim _fetchLock = new(1, 1);

    public NhsConditionReferenceService(HttpClient httpClient, ILogger<NhsConditionReferenceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<IReadOnlyList<RelatedCondition>> GetConditionsAsync() =>
        GetCachedAsync(ConditionsIndexUrl, ConditionsLinkPrefix, "conditions");

    public Task<IReadOnlyList<RelatedCondition>> GetSymptomsAsync() =>
        GetCachedAsync(SymptomsIndexUrl, SymptomsLinkPrefix, "symptoms");

    private async Task<IReadOnlyList<RelatedCondition>> GetCachedAsync(
        string indexUrl, string linkPrefix, string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        await _fetchLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(cacheKey, out cached))
                return cached;

            var result = await FetchAsync(indexUrl, linkPrefix, cacheKey);
            _cache[cacheKey] = result;
            return result;
        }
        finally
        {
            _fetchLock.Release();
        }
    }

    private async Task<IReadOnlyList<RelatedCondition>> FetchAsync(
        string indexUrl, string linkPrefix, string cacheKey)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(indexUrl);
            var entries = ParseIndex(html, linkPrefix);

            if (entries.Count == 0)
                _logger.LogWarning("NHS index page for {CacheKey} returned no entries after parsing.", cacheKey);

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch NHS index page for {CacheKey}. Links will not be resolved.", cacheKey);
            return [];
        }
    }

    private static IReadOnlyList<RelatedCondition> ParseIndex(string html, string hrefPrefix)
    {
        var results = new List<RelatedCondition>();
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

                                    if (!results.Any(r => r.Name.Equals(linkText, StringComparison.OrdinalIgnoreCase)))
                                        results.Add(new RelatedCondition(linkText, absoluteUrl));
                                }
                            }
                        }
                    }
                }
            }

            pos = aEnd + 1;
        }

        return results;
    }

    private static string StripTags(string html)
    {
        var result = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(result);
    }
}
