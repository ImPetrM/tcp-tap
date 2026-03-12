using Microsoft.Extensions.Logging;

namespace tcp_tap.Behaviors;

public class ForwardingBehaviorChainFactory : IForwardingBehaviorChainFactory
{
    private readonly ILogger<ForwardingBehaviorChainFactory> _logger;

    public ForwardingBehaviorChainFactory(ILogger<ForwardingBehaviorChainFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public IReadOnlyList<IForwardingBehavior> GetForwardingBehaviorChain(TcpTapOptions options)
    {
        var behaviors = new List<IForwardingBehavior>();
        behaviors.Add(new TapBehavior());
        
        if (IsDelayRequested(options))
        {
            behaviors.Add(new DelayBehavior(options.DelayMs.Value));
        }
        
        if(IsJitterRequested(options))
        {
            behaviors.Add(new JitterBehavior(options.JitterMinMs.Value, options.JitterMaxMs.Value));
        }
        
        _logger.LogInformation("Constructed forwarding behavior chain with following behaviors: {Behaviors}",
            string.Join(',', behaviors.Select(b => b.Name)));
        
        return behaviors;
    }

    private bool IsDelayRequested(TcpTapOptions options)
    {
        return options.DelayMs.HasValue && options.DelayMs > 0;
    }
    
    private bool IsJitterRequested(TcpTapOptions options)
    {
        return options.JitterMinMs.HasValue && 
               options.JitterMaxMs.HasValue && 
               options.JitterMinMs >= 0 
               && options.JitterMaxMs > options.JitterMinMs;
    }
}