using tcp_tap.Sinks;

namespace tcp_tap.UnitTests.Sinks;

public class HexTextFormatterTests
{
    [Test]
    public void FormatChunk_SourceToDestinationChunk_ReturnsCaptionMetadataAndHexBytes()
    {
        // Arrange
        var formatter = new HexTextFormatter();
        var chunk = new ChunkRecord
        {
            FlowDirection = FlowDirection.SourceToDestination,
            CapturedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            SendAt = new DateTime(2026, 1, 2, 3, 4, 6, DateTimeKind.Utc),
            Chunk = [0x00, 0x01, 0xAB, 0xFF]
        };

        // Act
        var formatted = formatter.FormatChunk(chunk).Split(Environment.NewLine);
    
        // Assert
        Assert.That(formatted, Has.Length.GreaterThanOrEqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(formatted[0], Does.Contain("Source -> Destination:"));
            Assert.That(formatted[1], Does.Contain($"Captured: {chunk.CapturedAt:O} | Sent: {chunk.SendAt:O}"));
            Assert.That(formatted[2], Does.Contain("0000: 00 01 AB FF "));
        });
    }
    
    [Test]
    public void FormatChunk_DestinationToSourceChunk_UsesDestinationCaption()
    {
        // Arrange
        var formatter = new HexTextFormatter();
        var chunk = new ChunkRecord
        {
            FlowDirection = FlowDirection.DestinationToSource,
            CapturedAt = DateTime.UtcNow,
            SendAt = DateTime.UtcNow,
            Chunk = [0x42]
        };
    
        // Act
        var formatted = formatter.FormatChunk(chunk);
    
        // Assert
        Assert.That(formatted, Does.StartWith($"Destination -> Source:{Environment.NewLine}"));
    }
    
    [Test]
    public void FormatChunk_MoreThanOneLineOfBytes_StartsNewLineAtEightBytes()
    {
        // Arrange
        var formatter = new HexTextFormatter();
        var chunk = new ChunkRecord
        {
            FlowDirection = FlowDirection.SourceToDestination,
            CapturedAt = DateTime.UtcNow,
            SendAt = DateTime.UtcNow,
            Chunk = [0, 1, 2, 3, 4, 5, 6, 7, 8]
        };
    
        // Act
        var formatted = formatter.FormatChunk(chunk);
    
        // Assert
        Assert.That(formatted, Does.Contain($"0000: 00 01 02 03 04 05 06 07 {Environment.NewLine}"));
        Assert.That(formatted, Does.Contain("0008: 08 "));
    }
    
    [Test]
    public void FormatChunk_EmptyChunk_ReturnsHeaderWithoutHexLines()
    {
        // Arrange
        var formatter = new HexTextFormatter();
        var chunk = new ChunkRecord
        {
            FlowDirection = FlowDirection.SourceToDestination,
            CapturedAt = DateTime.UtcNow,
            SendAt = DateTime.UtcNow,
            Chunk = []
        };
    
        // Act
        var formatted = formatter.FormatChunk(chunk);
    
        // Assert
        Assert.That(formatted, Does.Contain("Source -> Destination:"));
        Assert.That(formatted, Does.Not.Contain("0000: "));
    }
    
    [Test]
    public void FormatChunk_UnknownFlowDirection_ThrowsInvalidOperationException()
    {
        // Arrange
        var formatter = new HexTextFormatter();
        var chunk = new ChunkRecord
        {
            FlowDirection = (FlowDirection)1337,
            CapturedAt = DateTime.UtcNow,
            SendAt = DateTime.UtcNow,
            Chunk = [0x42]
        };
    
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => formatter.FormatChunk(chunk));
    }
}