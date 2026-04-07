using Serilog;
using tcp_tap.Sinks;

namespace tcp_tap;

public class RecordPublisher : IRecordPublisher
{
    private readonly IReadOnlyList<IRecordSink> _sinks;
    
    private bool IsMultiSink => _sinks.Count > 1;

    public RecordPublisher(TcpTapOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        _sinks = InitializeSinks(options);
        if(_sinks.Count == 0)
            Log.Warning("No sinks configured. Records will not be published.");
    }

    private List<IRecordSink> InitializeSinks(TcpTapOptions options)
    {
        var sinks = new List<IRecordSink>();
        var formatter = new HexTextFormatter(); 
        
        if(options.ConsoleOutput)
            sinks.Add(new ConsoleRecordSink(formatter));

        if(options.LogToFile)
            sinks.Add(new FileRecordSink(options.FilePath, formatter));
        
        // Add other sinks based on options here
        
        return sinks;
    }

    public async Task PublishAsync(ChunkRecord chunkRecord, CancellationToken cancellationToken)
    {
        try
        {
            if (IsMultiSink)
                await PublishToMultipleSinksAsync(_sinks, chunkRecord, cancellationToken);
            else 
                await PublishToSingleSinkAsync(_sinks[0], chunkRecord, cancellationToken); // We know there is one and only sink
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to publish record to sinks.");
        }
    }
    
    private Task PublishToMultipleSinksAsync(IReadOnlyList<IRecordSink> sinks, ChunkRecord chunkRecord, CancellationToken cancellationToken)
    {
        var tasks = sinks.Select(sink => sink.WriteAsync(chunkRecord, cancellationToken));
        return Task.WhenAll(tasks);
    }
    
    private Task PublishToSingleSinkAsync(IRecordSink sink, ChunkRecord chunkRecord, CancellationToken cancellationToken)
    {
        return sink.WriteAsync(chunkRecord, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sink in _sinks)
        {
            try
            {
                await sink.DisposeAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to dispose sink.");
            }
        }
    }
}