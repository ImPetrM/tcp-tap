using System.Text;

namespace tcp_tap.Sinks;

public class ConsoleRecordSink : IRecordSink
{
    private const int BytesPerLine = 8;
    private const string SourceToDestinationCaption = "Source -> Destination";
    private const string DestinationToSourceCaption = "Destination -> Source";
    
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
        
        Console.WriteLine(output.ToString());

        return Task.CompletedTask;
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

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}