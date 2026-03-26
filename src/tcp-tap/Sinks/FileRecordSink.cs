using System.Text;

namespace tcp_tap.Sinks;

public class FileRecordSink : IRecordSink
{
    private const int BytesPerLine = 8;
    private const string SourceToDestinationCaption = "Source -> Destination";
    private const string DestinationToSourceCaption = "Destination -> Source";
    
    private readonly string _filePath;
    private StreamWriter? _writer;

    public FileRecordSink(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
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
            
            output.AppendLine($"{GetCaption(chunkRecord.FlowDirection)}:");
            output.AppendLine($"Captured: {chunkRecord.CapturedAt:O} | Sent: {chunkRecord.SendAt:O}");
            for (var index = 0; index < chunkRecord.Chunk.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
            
                if(index % BytesPerLine == 0) 
                    output.Append($"{index:X4}: ");
            
                output.Append($"{chunkRecord.Chunk[index]:X2} ");

                if ((index + 1) % BytesPerLine == 0)
                    output.AppendLine();
            }
            output.AppendLine();
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
    
    private static string GetCaption(FlowDirection direction)
    {
        return direction switch
        {
            FlowDirection.SourceToDestination => SourceToDestinationCaption,
            FlowDirection.DestinationToSource => DestinationToSourceCaption,
            _ => throw new InvalidOperationException($"Unknown forwarding direction: {direction}")
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_writer != null) await _writer.DisposeAsync();
    }
}