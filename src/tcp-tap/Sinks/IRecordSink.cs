namespace tcp_tap.Sinks;

/// <summary>
/// Represents a sink for recording data chunks.
/// </summary>
public interface IRecordSink : IAsyncDisposable
{
    /// <summary>
    /// Asynchronously writes a chunk record to the sink.
    /// </summary>
    /// <param name="chunkRecord">The chunk record to write.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAsync(ChunkRecord chunkRecord, CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously writes a collection of chunk records to the sink.
    /// </summary>
    /// <param name="chunkRecords">The collection of chunk records to write.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAsync(IReadOnlyCollection<ChunkRecord> chunkRecords, CancellationToken cancellationToken);
}