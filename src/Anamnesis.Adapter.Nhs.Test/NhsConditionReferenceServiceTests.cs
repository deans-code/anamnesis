using System.Net;
using System.Text;
using Anamnesis.Adapter.Nhs;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Anamnesis.Adapter.Nhs.Test;

public class NhsConditionReferenceServiceTests
{
    private static NhsConditionReferenceService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.nhs.uk") };
        return new NhsConditionReferenceService(httpClient, NullLogger<NhsConditionReferenceService>.Instance);
    }

    [Fact]
    public async Task GetConditionsAsync_ReturnsEntries_WhenHtmlContainsNhsConditionLinks()
    {
        const string html = """
            <html><body>
            <a href="/conditions/diabetes/">Diabetes</a>
            <a href="/about/">About NHS</a>
            </body></html>
            """;

        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });

        var service = CreateService(handler);
        var result = await service.GetConditionsAsync();

        Assert.Single(result);
        Assert.Equal("Diabetes", result[0].Name);
        Assert.Equal("https://www.nhs.uk/conditions/diabetes/", result[0].Url);
    }

    [Fact]
    public async Task GetSymptomsAsync_ReturnsEntries_WhenHtmlContainsNhsSymptomsLinks()
    {
        const string html = """
            <html><body>
            <a href="/symptoms/headache/">Headache</a>
            <a href="/about/">About NHS</a>
            </body></html>
            """;

        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });

        var service = CreateService(handler);
        var result = await service.GetSymptomsAsync();

        Assert.Single(result);
        Assert.Equal("Headache", result[0].Name);
        Assert.Equal("https://www.nhs.uk/symptoms/headache/", result[0].Url);
    }

    [Fact]
    public async Task GetConditionsAsync_CachesResult_SecondCallMakesNoHttpRequest()
    {
        const string html = """
            <html><body><a href="/conditions/asthma/">Asthma</a></body></html>
            """;

        var handler = new CountingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });

        var service = CreateService(handler);
        await service.GetConditionsAsync();
        await service.GetConditionsAsync();

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetConditionsAsync_ReturnsEmptyList_WhenHttpRequestFails()
    {
        var handler = new FailingHttpMessageHandler(new HttpRequestException("Network error"));
        var service = CreateService(handler);

        var result = await service.GetConditionsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetConditionsAsync_ReturnsEmptyList_WhenHtmlContainsNoMatchingLinks()
    {
        const string html = """
            <html><body>
            <a href="/about/">About NHS</a>
            <a href="/symptoms/headache/">Headache</a>
            </body></html>
            """;

        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });

        var service = CreateService(handler);
        var result = await service.GetConditionsAsync();

        Assert.Empty(result);
    }
}

internal class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response);
}

internal class CountingHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        return Task.FromResult(response);
    }
}

internal class FailingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}
