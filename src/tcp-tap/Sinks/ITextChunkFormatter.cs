namespace tcp_tap.Sinks;

public interface ITextChunkFormatter
{
    string FormatChunk(ChunkRecord chunk);
}