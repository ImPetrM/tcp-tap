namespace tcp_tap;

public class ForwardingContext
{
    public FlowDirection Direction { get; }
    
    public ForwardingContext(FlowDirection direction)
    {
        Direction = direction;
    }
}