namespace tcp_tap.Behaviors;

public class DelayBehavior : IForwardingBehavior
{
    private readonly int _delayMs;

    public string Name => nameof(DelayBehavior);
    
    public DelayBehavior(int delayMs)
    {
        _delayMs = delayMs;
    }

    public async Task<ReadOnlyMemory<byte>> ProcessAsync(ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken)
    {
        await Task.Delay(_delayMs, cancellationToken);
        return chunk;
    }
}