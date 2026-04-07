using System.Diagnostics;
using tcp_tap.Behaviors;

namespace tcp_tap.UnitTests.Behaviors;

public class JitterBehaviorTests
{
    [Test]
    public async Task ProcessAsync_FixedJitterRange_ReturnsAfterConfiguredDelay()
    {
        // Arrange
        var behavior = new JitterBehavior(40, 40);
        ReadOnlyMemory<byte> chunk = new byte[] { 1, 2, 3, 4 };
        var context = CreateForwardingContext();
    
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await behavior.ProcessAsync(chunk, context, CancellationToken.None);
        stopwatch.Stop();
    
        // Assert
        Assert.That(result.ToArray(), Is.EqualTo(chunk.ToArray()));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(30));
    }
    
    [Test]
    public async Task ProcessAsync_ValidJitterRange_ReturnsOriginalChunk()
    {
        // Arrange
        var behavior = new JitterBehavior(1, 5);
        ReadOnlyMemory<byte> chunk = new byte[] { 9, 8, 7 };
        var context = CreateForwardingContext();
    
        // Act
        var result = await behavior.ProcessAsync(chunk, context, CancellationToken.None);
    
        Assert.That(result.ToArray(), Is.EqualTo(chunk.ToArray()));
    }
    
    [Test]
    public void ProcessAsync_CancelTokenActivated_ThrowsException()
    {
        // Arrange
        var behavior = new JitterBehavior(100, 100);
        ReadOnlyMemory<byte> chunk = new byte[] { 1 };
        var context = CreateForwardingContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
    
        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () => await behavior.ProcessAsync(chunk, context, cts.Token));
    }
    
    [Test]
    public void ProcessAsync_CancellationDuringDelay_ThrowsException()
    {
        // Arrange
        var behavior = new JitterBehavior(300, 300);
        ReadOnlyMemory<byte> chunk = new byte[] { 1, 2 };
        var context = CreateForwardingContext();
        using var cts = new CancellationTokenSource(25);
    
        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () => await behavior.ProcessAsync(chunk, context, cts.Token));
    }
    
    [Test]
    public void ProcessAsync_MinDelayGreaterThanMaxDelay_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var behavior = new JitterBehavior(20, 10);
        ReadOnlyMemory<byte> chunk = new byte[] { 1, 2, 3 };
        var context = CreateForwardingContext();
    
        // Act & Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await behavior.ProcessAsync(chunk, context, CancellationToken.None));
    }
    
    private static ForwardingContext CreateForwardingContext()
    {
        return new ForwardingContext(DateTime.UtcNow, FlowDirection.SourceToDestination);
    }
}