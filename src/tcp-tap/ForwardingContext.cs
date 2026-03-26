namespace tcp_tap;

public class ForwardingContext
{
    public DateTime CapturedAt { get; }
    public DateTime SendAt { get; set; } = DateTime.MinValue;
    public FlowDirection Direction { get; }
    
    public ForwardingContext(DateTime capturedAt, FlowDirection direction)
    {
        CapturedAt = capturedAt;
        Direction = direction;
    }
}