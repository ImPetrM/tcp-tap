namespace tcp_tap.Behaviors;

/// <summary>
/// Defines a factory for creating chains of forwarding behaviors.
/// </summary>
public interface IForwardingBehaviorChainFactory
{
    /// <summary>
    /// Retrieves a chain of forwarding behaviors based on the specified chain name and options.
    /// </summary>
    /// <param name="chainName">The name of the forwarding behavior chain to retrieve.</param>
    /// <param name="options">The options to configure the forwarding behavior chain.</param>
    /// <returns>A read-only list of forwarding behaviors that make up the chain.</returns>
    IReadOnlyList<IForwardingBehavior> GetForwardingBehaviorChain(string chainName, TcpTapOptions options);
}