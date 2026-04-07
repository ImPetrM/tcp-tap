using System.Diagnostics;
using tcp_tap.Behaviors;

namespace tcp_tap.UnitTests.Behaviors;

public class DelayBehaviorTests
{
    [Test]
    public async Task ProcessAsync_ConfiguredDelay_ReturnsAfterDelay()
    {
        // Arrange
        var behavior = new DelayBehavior(50);
        ReadOnlyMemory<byte> chunk = new byte[] { 1, 2, 3, 4 };
        var context = CreateForwardingContext();
    
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await behavior.ProcessAsync(chunk, context, CancellationToken.None);
        stopwatch.Stop();
    
        // Assert
        Assert.That(result.ToArray(), Is.EqualTo(chunk.ToArray()));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(35));
    }
    
    [Test]
    public async Task ProcessAsync_ConfigureZeroDelay_ReturnWithoutDelay()
    {
        // Arrange
        var behavior = new DelayBehavior(0);
        ReadOnlyMemory<byte> chunk = new byte[] { 9, 8, 7 };
        var context = CreateForwardingContext();
    
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await behavior.ProcessAsync(chunk, context, CancellationToken.None);
        stopwatch.Stop();
    
        // Assert
        Assert.That(result.ToArray(), Is.EqualTo(chunk.ToArray()));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(20));
    }
    
    [Test]
    public void ProcessAsync_CancelTokenActivated_ThrowsException()
    {
        // Arrange
        var behavior = new DelayBehavior(100);
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
        var behavior = new DelayBehavior(500);
        ReadOnlyMemory<byte> chunk = new byte[] { 1, 2 };
        var context = CreateForwardingContext();
        using var cts = new CancellationTokenSource(25);
    
        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () => await behavior.ProcessAsync(chunk, context, cts.Token));
    }
    
    private static ForwardingContext CreateForwardingContext()
    {
        return new ForwardingContext(DateTime.UtcNow, FlowDirection.SourceToDestination);
    }
}