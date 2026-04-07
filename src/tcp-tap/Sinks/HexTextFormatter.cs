using System.Text;

namespace tcp_tap.Sinks;

public class HexTextFormatter : ITextChunkFormatter
{
    private const int BytesPerLine = 8;
    private const string SourceToDestinationCaption = "Source -> Destination";
    private const string DestinationToSourceCaption = "Destination -> Source";
    
    public string FormatChunk(ChunkRecord chunk)
    {
        var output = new StringBuilder();
        
        output.AppendLine($"{GetCaption(chunk.FlowDirection)}:");
        output.AppendLine($"Captured: {chunk.CapturedAt:O} | Sent: {chunk.SendAt:O}");
        for (var index = 0; index < chunk.Chunk.Length; index++)
        {
            if(index % BytesPerLine == 0) 
                output.Append($"{index:X4}: ");
            
            output.Append($"{chunk.Chunk[index]:X2} ");

            if ((index + 1) % BytesPerLine == 0)
                output.AppendLine();
        }
        
        return output.ToString();
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