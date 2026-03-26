namespace tcp_tap;

public class ChunkRecord
{
    public FlowDirection FlowDirection { get; set; }
    public DateTime CapturedAt { get; set; }
    public DateTime SendAt { get; set; }

    public byte[] Chunk { get; set; } = [];
}