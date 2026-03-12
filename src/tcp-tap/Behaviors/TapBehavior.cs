using System.Text;

namespace tcp_tap.Behaviors;

public class TapBehavior : IForwardingBehavior
{
    private const int BytesPerLine = 8;
    private const string DefaultTappedChunkCaption = "Tapped chunk";
    
    private readonly string _tappedChungCaption;
    public string Name => nameof(TapBehavior);
    
    public TapBehavior(string tappedChungCaption = "")
    {
        if (string.IsNullOrWhiteSpace(tappedChungCaption))
        {
            _tappedChungCaption = DefaultTappedChunkCaption;
        }
        else
        {
            _tappedChungCaption = tappedChungCaption;
        }
    }
    
    public Task<ReadOnlyMemory<byte>> ProcessAsync(ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken)
    {
        var chunkSpan = chunk.Span;
        var output = new StringBuilder();
        
        output.AppendLine($"{_tappedChungCaption}:");
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
}