using System.Text.Json;
using Anamnesis.UseCase.Conversation.Contract;
using Microsoft.Extensions.Options;
using Xunit;

namespace Anamnesis.UseCase.Conversation.Test;

public class FileAuditLoggerTests : IDisposable
{
    private readonly string _logFile;
    private readonly FileAuditLogger _sut;

    public FileAuditLoggerTests()
    {
        _logFile = Path.Combine(Path.GetTempPath(), $"audit_test_{Guid.NewGuid():N}.log");
        var options = Options.Create(new AuditLoggingSettings { FilePath = _logFile });
        _sut = new FileAuditLogger(options);
    }

    [Fact]
    public async Task LogAsync_WritesEntryWithAllRequiredFields()
    {
        var entry = new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: "abc123",
            EventType: "user_message",
            Detail: "I have a headache.");

        await _sut.LogAsync(entry);

        var lines = await File.ReadAllLinesAsync(_logFile);
        Assert.Single(lines.Where(l => !string.IsNullOrWhiteSpace(l)));

        var written = JsonSerializer.Deserialize<AuditEntry>(lines[0]);
        Assert.NotNull(written);
        Assert.Equal("abc123", written.SessionId);
        Assert.Equal("user_message", written.EventType);
        Assert.Equal("I have a headache.", written.Detail);
    }

    [Fact]
    public async Task LogAsync_TruncatesDetail_At512Characters()
    {
        var longDetail = new string('x', 600);
        var entry = new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: "sess1",
            EventType: "test",
            Detail: longDetail);

        await _sut.LogAsync(entry);

        var lines = await File.ReadAllLinesAsync(_logFile);
        var written = JsonSerializer.Deserialize<AuditEntry>(lines[0]);
        Assert.NotNull(written);
        Assert.Equal(512, written.Detail.Length);
        Assert.Equal(new string('x', 512), written.Detail);
    }

    [Fact]
    public async Task LogAsync_AppendsToExistingFile()
    {
        var entry1 = new AuditEntry(DateTimeOffset.UtcNow, "s1", "event_a", "first");
        var entry2 = new AuditEntry(DateTimeOffset.UtcNow, "s1", "event_b", "second");

        await _sut.LogAsync(entry1);
        await _sut.LogAsync(entry2);

        var lines = (await File.ReadAllLinesAsync(_logFile))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        Assert.Equal(2, lines.Length);

        var written1 = JsonSerializer.Deserialize<AuditEntry>(lines[0]);
        var written2 = JsonSerializer.Deserialize<AuditEntry>(lines[1]);
        Assert.Equal("first", written1?.Detail);
        Assert.Equal("second", written2?.Detail);
    }

    public void Dispose()
    {
        if (File.Exists(_logFile))
            File.Delete(_logFile);
    }
}
