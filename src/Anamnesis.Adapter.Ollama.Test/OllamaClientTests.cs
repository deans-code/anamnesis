using System.Net;
using System.Text;
using System.Text.Json;
using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.Domain;
using Microsoft.Extensions.Options;
using Xunit;

namespace Anamnesis.Adapter.Ollama.Test;

public class OllamaClientTests
{
    private static OllamaClient CreateClient(HttpMessageHandler handler, OllamaSettings? settings = null)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") };
        var options = Options.Create(settings ?? new OllamaSettings());
        return new OllamaClient(httpClient, options);
    }

    [Fact]
    public async Task ChatAsync_ReturnsAssistantContent_WhenOllamaRespondsSuccessfully()
    {
        var responsePayload = new
        {
            message = new { role = "assistant", content = "How long have you had this headache?" }
        };
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(responsePayload), Encoding.UTF8, "application/json")
            });

        var client = CreateClient(handler);
        var messages = new[] { new ConversationMessage("user", "I have a headache.") };

        var result = await client.ChatAsync(messages);

        Assert.Equal("How long have you had this headache?", result);
    }

    [Fact]
    public async Task ChatAsync_UsesConfiguredModel_InRequest()
    {
        string? capturedBody = null;
        var responsePayload = new { message = new { role = "assistant", content = "ok" } };
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(responsePayload), Encoding.UTF8, "application/json")
            },
            body => capturedBody = body);

        var settings = new OllamaSettings { Model = "medgemma", BaseUrl = "http://localhost:11434" };
        var client = CreateClient(handler, settings);
        var messages = new[] { new ConversationMessage("user", "test") };

        await client.ChatAsync(messages);

        Assert.NotNull(capturedBody);
        Assert.Contains("medgemma", capturedBody);
    }

    [Fact]
    public async Task ChatAsync_ThrowsOllamaUnavailableException_OnHttpRequestException()
    {
        var handler = new FailingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var client = CreateClient(handler);
        var messages = new[] { new ConversationMessage("user", "test") };

        await Assert.ThrowsAsync<OllamaUnavailableException>(() => client.ChatAsync(messages));
    }

    [Fact]
    public async Task ChatAsync_ThrowsOllamaUnavailableException_OnTimeout()
    {
        var handler = new FailingHttpMessageHandler(new TaskCanceledException("Timeout"));
        var client = CreateClient(handler);
        var messages = new[] { new ConversationMessage("user", "test") };

        await Assert.ThrowsAsync<OllamaUnavailableException>(() => client.ChatAsync(messages));
    }
}

internal class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response);
}

internal class CapturingHttpMessageHandler(HttpResponseMessage response, Action<string> captureBody) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        captureBody(body);
        return response;
    }
}

internal class FailingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}
