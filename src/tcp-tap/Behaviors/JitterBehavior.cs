namespace tcp_tap.Behaviors;

public class JitterBehavior : IForwardingBehavior
{
    private readonly Random _random = new Random();
    private readonly int _minDelayMs;
    private readonly int _maxDelayMs;
    
    public string Name => nameof(JitterBehavior);
    
    public JitterBehavior(int minDelayMs, int maxDelayMs)
    {
        _minDelayMs = minDelayMs;
        _maxDelayMs = maxDelayMs;
    }
    
    public async Task<ReadOnlyMemory<byte>> ProcessAsync(ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken)
    {
        var delayMs = _random.Next(_minDelayMs, _maxDelayMs + 1);
        await Task.Delay(delayMs, cancellationToken);
        return chunk;
    }
}