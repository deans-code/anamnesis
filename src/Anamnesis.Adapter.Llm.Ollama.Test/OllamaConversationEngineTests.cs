using Anamnesis.Adapter.Llm.Contract;
using Anamnesis.Adapter.Llm.Ollama;
using Anamnesis.Domain;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Anamnesis.Adapter.Llm.Ollama.Test;

public class OllamaConversationEngineTests
{
    private readonly IOllamaClient _ollamaClient = Substitute.For<IOllamaClient>();
    private readonly OllamaConversationEngine _sut;

    public OllamaConversationEngineTests()
    {
        _sut = new OllamaConversationEngine(_ollamaClient);
    }

    [Fact]
    public async Task SendMessageAsync_PrependSystemPrompt_OnFirstCall()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First question?");

        await _sut.SendMessageAsync("I have a headache.");

        await _ollamaClient.Received(1).ChatAsync(
            Arg.Is<IEnumerable<ConversationMessage>>(
                msgs => msgs.First().Role == "system"));
    }

    [Fact]
    public async Task SendMessageAsync_DoesNotDuplicateSystemPrompt_OnSubsequentCalls()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First response", "Second response");

        await _sut.SendMessageAsync("First symptom.");
        await _sut.SendMessageAsync("Second answer.");

        await _ollamaClient.Received(2).ChatAsync(
            Arg.Is<IEnumerable<ConversationMessage>>(
                msgs => msgs.Count(m => m.Role == "system") == 1));
    }

    [Fact]
    public async Task CheckShouldContinueAsync_DoesNotAddToHistory()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First response", "CONTINUE", "Second response");

        await _sut.SendMessageAsync("Initial symptom.");
        await _sut.CheckShouldContinueAsync();
        await _sut.SendMessageAsync("Follow-up answer.");

        // Third ChatAsync call (second SendMessageAsync) should only see [system, user1, assistant1, user2]
        await _ollamaClient.Received(1).ChatAsync(
            Arg.Is<IEnumerable<ConversationMessage>>(msgs =>
                msgs.Count() == 4
                && msgs.ElementAt(0).Role == "system"
                && msgs.ElementAt(1).Role == "user"
                && msgs.ElementAt(2).Role == "assistant"
                && msgs.ElementAt(3).Role == "user"));
    }

    [Fact]
    public async Task SummariseAsync_DoesNotAddToHistory()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First response", "Summary text", "Second response");

        await _sut.SendMessageAsync("Initial symptom.");
        await _sut.SummariseAsync();
        await _sut.SendMessageAsync("Follow-up.");

        // The history for the second send should have 4 messages, not include the summary exchange
        await _ollamaClient.Received(1).ChatAsync(
            Arg.Is<IEnumerable<ConversationMessage>>(msgs =>
                msgs.Count() == 4));
    }

    [Fact]
    public async Task GetRelatedConditionsAsync_DoesNotAddToHistory()
    {
        var conditions = new List<RelatedCondition> { new("Asthma", "/conditions/asthma/") };
        var symptoms = new List<RelatedCondition> { new("Cough", "/symptoms/cough/") };

        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First response", """{"conditions":["Asthma"],"symptoms":["Cough"]}""", "Second response");

        await _sut.SendMessageAsync("I have a cough.");
        await _sut.GetRelatedConditionsAsync(conditions, symptoms);
        await _sut.SendMessageAsync("It started yesterday.");

        // History for second send should still only have 4 messages
        await _ollamaClient.Received(1).ChatAsync(
            Arg.Is<IEnumerable<ConversationMessage>>(msgs =>
                msgs.Count() == 4));
    }

    [Fact]
    public async Task CheckShouldContinueAsync_ReturnsTrue_WhenLlmSaysContinue()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("CONTINUE");

        var result = await _sut.CheckShouldContinueAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task CheckShouldContinueAsync_ReturnsFalse_WhenLlmSaysEnd()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("END");

        var result = await _sut.CheckShouldContinueAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task CheckShouldContinueAsync_ReturnsTrue_WhenLlmReturnsUnrecognisedValue()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("MAYBE");

        var result = await _sut.CheckShouldContinueAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task GetRelatedConditionsAsync_ReturnsEmptyResult_WhenLlmReturnsMalformedJson()
    {
        var conditions = new List<RelatedCondition> { new("Asthma", "/conditions/asthma/") };
        var symptoms = new List<RelatedCondition> { new("Cough", "/symptoms/cough/") };

        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("this is not json at all");

        var result = await _sut.GetRelatedConditionsAsync(conditions, symptoms);

        Assert.Empty(result.Conditions);
        Assert.Empty(result.Symptoms);
    }

    [Fact]
    public async Task GetRelatedConditionsAsync_ReturnsMatchedEntries_WithUrls()
    {
        var conditions = new List<RelatedCondition> { new("Asthma", "/conditions/asthma/") };
        var symptoms = new List<RelatedCondition> { new("Cough", "/symptoms/cough/") };

        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("""{"conditions":["Asthma"],"symptoms":["Cough"]}""");

        var result = await _sut.GetRelatedConditionsAsync(conditions, symptoms);

        Assert.Single(result.Conditions);
        Assert.Equal("Asthma", result.Conditions[0].Name);
        Assert.Equal("/conditions/asthma/", result.Conditions[0].Url);
        Assert.Single(result.Symptoms);
        Assert.Equal("Cough", result.Symptoms[0].Name);
    }

    [Fact]
    public async Task SendMessageAsync_OnRateLimit_ReturnsErrorMessage_WithoutLeavingHistoryDirty()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Throws(new RateLimitExceededException("rate limited", new Exception()));

        var result = await _sut.SendMessageAsync("I feel ill.");

        Assert.Contains("too many messages", result, StringComparison.OrdinalIgnoreCase);

        // Subsequent call should still work — history should not include the failed user message
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("What are your symptoms?");
        var result2 = await _sut.SendMessageAsync("I feel ill.");
        Assert.Equal("What are your symptoms?", result2);
    }
}
