using System.Text;

namespace tcp_tap.Sinks;

public class FileRecordSink : IRecordSink
{
    private readonly string _filePath;
    private readonly ITextChunkFormatter _formatter;
    private StreamWriter? _writer;

    public FileRecordSink(string filePath, ITextChunkFormatter formatter)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public Task WriteAsync(ChunkRecord chunkRecord, CancellationToken cancellationToken)
    {
        return WriteAsync([chunkRecord], cancellationToken);
    }

    public async Task WriteAsync(IReadOnlyCollection<ChunkRecord> chunkRecords, CancellationToken cancellationToken)
    {
        var writer = GetWriter();
        
        var output = new StringBuilder();
        foreach (var chunkRecord in chunkRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();
            output.AppendLine(_formatter.FormatChunk(chunkRecord));
        }
        
        await writer.WriteAsync(output.ToString());
        await writer.FlushAsync(cancellationToken);
    }
    
    private StreamWriter GetWriter()
    {
        return _writer ??= CreateWriter();
    }

    private StreamWriter CreateWriter()
    {
        var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        return new StreamWriter(stream) { AutoFlush = true };
    }

    public async ValueTask DisposeAsync()
    {
        if (_writer != null) await _writer.DisposeAsync();
    }
}