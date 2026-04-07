using System.Text;

namespace tcp_tap.Sinks;

public class ConsoleRecordSink : IRecordSink
{
    private readonly ITextChunkFormatter _formatter;

    public ConsoleRecordSink(ITextChunkFormatter formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }
    
    public Task WriteAsync(ChunkRecord chunkRecord, CancellationToken cancellationToken)
    {
        return WriteAsync([chunkRecord], cancellationToken);
    }

    public Task WriteAsync(IReadOnlyCollection<ChunkRecord> chunkRecords, CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        
        foreach (var chunkRecord in chunkRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();
            output.AppendLine(_formatter.FormatChunk(chunkRecord));
        }
        
        Console.WriteLine(output.ToString());

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}