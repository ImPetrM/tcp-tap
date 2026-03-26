namespace tcp_tap;

public class TcpTapOptions
{
    public int ListenPort { get; init; }
    public string ForwardHost { get; init; } = "";
    public int ForwardPort { get; init; }
    
    public int? DelayMs { get; init; }
    public int? JitterMinMs { get; init; }
    public int? JitterMaxMs { get; init; }

    public bool ConsoleOutput { get; init; } = true;
    public bool LogToFile { get; init; } = false;
    public string FilePath { get; init; } = String.Empty;
}