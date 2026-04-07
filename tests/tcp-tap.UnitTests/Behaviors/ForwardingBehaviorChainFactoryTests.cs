using Microsoft.Extensions.Logging.Abstractions;
using tcp_tap.Behaviors;

namespace tcp_tap.UnitTests.Behaviors;

public class ForwardingBehaviorChainFactoryTests
{
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _ = new ForwardingBehaviorChainFactory(null!));
    }
    
    [Test]
    public void GetForwardingBehaviorChain_DelayConfigured_ReturnsDelayBehavior()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions { DelayMs = 25 };
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors.Count, Is.EqualTo(1));
        Assert.That(behaviors[0], Is.TypeOf<DelayBehavior>());
    }
    
    [Test]
    public void GetForwardingBehaviorChain_ValidJitterConfigured_ReturnsJitterBehavior()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions { JitterMinMs = 5, JitterMaxMs = 10 };
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors.Count, Is.EqualTo(1));
        Assert.That(behaviors[0], Is.TypeOf<JitterBehavior>());
    }
    
    [Test]
    public void GetForwardingBehaviorChain_DelayAndJitterConfigured_ReturnsBehaviorsInConfiguredOrder()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions { DelayMs = 15, JitterMinMs = 1, JitterMaxMs = 3 };
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors.Count, Is.EqualTo(2));
        Assert.That(behaviors[0], Is.TypeOf<DelayBehavior>());
        Assert.That(behaviors[1], Is.TypeOf<JitterBehavior>());
    }
    
    [Test]
    public void GetForwardingBehaviorChain_NoBehaviorOptionsConfigured_ReturnsEmptyChain()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions();
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors, Is.Empty);
    }
    
    [Test]
    public void GetForwardingBehaviorChain_NonPositiveDelay_DoesNotIncludeDelayBehavior()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions { DelayMs = 0 };
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors, Is.Empty);
    }
    
    [Test]
    public void GetForwardingBehaviorChain_InvalidJitterRange_DoesNotIncludeJitterBehavior()
    {
        // Arrange
        var factory = CreateFactory();
        var options = new TcpTapOptions { JitterMinMs = 10, JitterMaxMs = 10 };
    
        // Act
        var behaviors = factory.GetForwardingBehaviorChain(options);
    
        // Assert
        Assert.That(behaviors, Is.Empty);
    }
    
    [Test]
    public void GetForwardingBehaviorChain_NullOptions_ThrowsNullReferenceException()
    {
        // Arrange
        var factory = CreateFactory();
    
        // Assert
        Assert.Throws<NullReferenceException>(() => factory.GetForwardingBehaviorChain(null!));
    }
    
    private static ForwardingBehaviorChainFactory CreateFactory()
    {
        return new ForwardingBehaviorChainFactory(NullLogger<ForwardingBehaviorChainFactory>.Instance);
    }
}