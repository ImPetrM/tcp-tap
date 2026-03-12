namespace tcp_tap.Behaviors;

/// <summary>
/// Represents a forwarding behavior that processes chunks of data asynchronously.
/// </summary>
public interface IForwardingBehavior
{
    /// <summary>
    /// Gets the name of the forwarding behavior.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Processes a chunk of data asynchronously.
    /// </summary>
    /// <param name="chunk">The chunk of data to process, represented as a read-only memory block.</param>
    /// <param name="context">The context containing metadata and state for the forwarding operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the processed data as a read-only memory block.</returns>
    Task<ReadOnlyMemory<byte>> ProcessAsync(ReadOnlyMemory<byte> chunk, ForwardingContext context, CancellationToken cancellationToken);
}