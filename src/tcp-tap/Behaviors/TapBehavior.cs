using System.Text;

namespace tcp_tap.Behaviors;

public class TapBehavior : IForwardingBehavior
{
    private const int BytesPerLine = 8;
    private const string SourceToDestinationCaption = "Source -> Destination";
    private const string DestinationToSourceCaption = "Destination -> Source";
    
    public string Name => nameof(TapBehavior);
    
    public TapBehavior()
    {
    }
    
    public Task<ReadOnlyMemory<byte>> ProcessAsync(ReadOnlyMemory<byte> chunk, ForwardingContext context, CancellationToken cancellationToken)
    {
        var chunkSpan = chunk.Span;
        var output = new StringBuilder();
        
        output.AppendLine($"{GetCaption(context.Direction)}:");
        for (var index = 0; index < chunkSpan.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if(index % BytesPerLine == 0) 
                output.Append($"{index:X4}: ");
            
            output.Append($"{chunkSpan[index]:X2} ");

            if ((index + 1) % BytesPerLine == 0)
                output.AppendLine();
        }
        output.AppendLine();
        
        Console.WriteLine(output.ToString());

        return Task.FromResult(chunk);
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
}