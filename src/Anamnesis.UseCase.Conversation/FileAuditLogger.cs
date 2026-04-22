using System.Text.Json;
using Anamnesis.UseCase.Conversation.Contract;
using Microsoft.Extensions.Options;

namespace Anamnesis.UseCase.Conversation;

public class FileAuditLogger : IAuditLogger
{
    private const int MaxDetailLength = 512;

    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileAuditLogger(IOptions<AuditLoggingSettings> options)
    {
        _filePath = !string.IsNullOrWhiteSpace(options.Value.FilePath)
            ? options.Value.FilePath
            : Path.Combine(AppContext.BaseDirectory, "logs", "audit.log");
    }

    public async Task LogAsync(AuditEntry entry)
    {
        var truncatedDetail = entry.Detail.Length > MaxDetailLength
            ? entry.Detail[..MaxDetailLength]
            : entry.Detail;

        var sanitised = entry with { Detail = truncatedDetail };

        var line = JsonSerializer.Serialize(sanitised);

        await _lock.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine);
        }
        finally
        {
            _lock.Release();
        }
    }
}
