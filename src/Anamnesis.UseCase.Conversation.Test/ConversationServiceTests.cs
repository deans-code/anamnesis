using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation.Contract;
using NSubstitute;
using Xunit;

namespace Anamnesis.UseCase.Conversation.Test;

public class ConversationServiceTests
{
    private readonly IOllamaClient _ollamaClient = Substitute.For<IOllamaClient>();
    private readonly IConversationService _sut;

    public ConversationServiceTests()
    {
        _sut = new ConversationService(_ollamaClient);
    }

    [Fact]
    public async Task SendAsync_ReturnsLlmResponse_AndAddsToHistory()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("How long have you had this headache?");

        var result = await _sut.SendAsync("I have a headache.");

        Assert.Equal("How long have you had this headache?", result);
    }

    [Fact]
    public async Task SendAsync_PrependSystemPrompt_OnFirstCall()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("What is the severity?");

        await _sut.SendAsync("I have chest pain.");

        await _ollamaClient.Received(1).ChatAsync(Arg.Is<IEnumerable<ConversationMessage>>(
            msgs => msgs.First().Role == "system"));
    }

    [Fact]
    public async Task SendAsync_DoesNotDuplicateSystemPrompt_OnSubsequentCalls()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("First question", "Second question");

        await _sut.SendAsync("Initial symptoms.");
        await _sut.SendAsync("Follow up answer.");

        await _ollamaClient.Received(2).ChatAsync(Arg.Is<IEnumerable<ConversationMessage>>(
            msgs => msgs.Count(m => m.Role == "system") == 1));
    }

    [Fact]
    public async Task CheckContinuationAsync_ReturnsTrue_WhenLlmSaysContinue()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("ok", "CONTINUE");

        await _sut.SendAsync("I feel unwell.");
        var result = await _sut.CheckContinuationAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task CheckContinuationAsync_ReturnsFalse_WhenLlmSaysEnd()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("ok", "END");

        await _sut.SendAsync("I feel unwell.");
        var result = await _sut.CheckContinuationAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task CheckContinuationAsync_DoesNotMutateConversationHistory()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("ok", "END", "another response");

        await _sut.SendAsync("Initial symptom.");
        await _sut.CheckContinuationAsync();

        var summaryResult = await _sut.RequestSummaryAsync();

        Assert.Equal("another response", summaryResult);
    }

    [Fact]
    public async Task RequestSummaryAsync_ReturnsLlmSummary()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("ok", "Summary: Patient reported headache for 2 days.");

        await _sut.SendAsync("Headache for 2 days.");
        var summary = await _sut.RequestSummaryAsync();

        Assert.Equal("Summary: Patient reported headache for 2 days.", summary);
    }

    [Fact]
    public void HasStarted_ReturnsFalse_BeforeFirstSend()
    {
        Assert.False(_sut.HasStarted);
    }

    [Fact]
    public async Task HasStarted_ReturnsTrue_AfterFirstSend()
    {
        _ollamaClient.ChatAsync(Arg.Any<IEnumerable<ConversationMessage>>())
            .Returns("question");

        await _sut.SendAsync("symptom");

        Assert.True(_sut.HasStarted);
    }
}
