namespace tcp_tap;

/// <summary>
/// Defines a publisher for chunk records.
/// </summary>
public interface IRecordPublisher : IAsyncDisposable
{
    /// <summary>
    /// Asynchronously publishes a chunk record.
    /// </summary>
    /// <param name="chunkRecord">The chunk record to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishAsync(ChunkRecord chunkRecord, CancellationToken cancellationToken);
}