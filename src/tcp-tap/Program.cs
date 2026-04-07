using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using tcp_tap.Behaviors;
using tcp_tap.Sinks;

namespace tcp_tap;

class Program
{
    private const string ListenPortArgumentName = "--listen-port";
    private const string ForwardHostArgumentName = "--forward-host";
    private const string ForwardPortArgumentName = "--forward-port";
    private const string DelayMsArgumentName = "--delay-ms";
    private const string JitterMinArgumentName = "--delay-min-ms";
    private const string JitterMaxArgumentName = "--delay-max-ms";
    private const string LogToFileArgumentName = "--log-to-file";
    
    static int Main(string[] args)
    {
        // Configure logging
        ConfigureSerilog(); 
        
        // Build command line parser
        var tapCommand = BuildTapCommand();
        tapCommand.SetAction(SetActionHandler);
        
        var parseResult = tapCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                $"logs/tcp-tap-{DateTime.Now:yyyy-M-d_hh-mm-ss}.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 5)
            .CreateLogger();
    }

    private static RootCommand BuildTapCommand()
    {
        var listenOption = new Option<int>(ListenPortArgumentName)
        {
            Description = "The local port on which the TCP tap will listen for incoming connections.",
            Required = true
        };

        var forwardHostOption = new Option<string>(ForwardHostArgumentName)
        {
            Description = "The hostname or IP address of the destination server to which the TCP tap will forward incoming connections.",
            Required = true
        };

        var forwardPortOption = new Option<int>(ForwardPortArgumentName)
        {
            Description = "The port number of the destination server to which the TCP tap will forward incoming connections.",
            Required = true
        };

        var delayMs = new Option<int?>(DelayMsArgumentName)
        {
            Description = "The fixed delay in milliseconds to introduce for each forwarded connection. If specified, this delay will be applied to all connections.",
            Required = false
        };

        var jitterMinOption = new Option<int?>(JitterMinArgumentName)
        {
            Description = "The minimum delay in milliseconds for jitter. If specified, the actual delay for each connection will be a random value between the minimum and maximum jitter values.",
            Required = false
        };
        
        var jitterMaxOption = new Option<int?>(JitterMaxArgumentName)
        {
            Description = "The maximum delay in milliseconds for jitter. If specified, the actual delay for each connection will be a random value between the minimum and maximum jitter values.",
            Required = false
        };
        
        var logToFileOption = new Option<string>(LogToFileArgumentName)
        {
            Description = "If specified, the path to a file where the TCP tap will log captured data. If not provided, logging to file will be disabled.",
            Required = false
        };
        
        return new RootCommand("A TCP tap tool that listens for incoming TCP connections on a specified local port displays data and forwards them to a specified destination server and port. " +
                               "The tool can introduce configurable delays and jitter to simulate network latency conditions.")
        {
            listenOption,
            forwardHostOption,
            forwardPortOption,
            delayMs,
            jitterMinOption,
            jitterMaxOption,
            logToFileOption
        };
    }
    
    private static Task SetActionHandler(ParseResult parseResults, CancellationToken cancellationToken)
    {
        var listenPort = parseResults.GetRequiredValue<int>(ListenPortArgumentName);
        var forwardHost = parseResults.GetRequiredValue<string>(ForwardHostArgumentName);
        var forwardPort = parseResults.GetRequiredValue<int>(ForwardPortArgumentName);
        var delayMs = parseResults.GetValue<int?>(DelayMsArgumentName);
        var jitterMinMs = parseResults.GetValue<int?>(JitterMinArgumentName);
        var jitterMaxMs = parseResults.GetValue<int?>(JitterMaxArgumentName);
        var logToFilePath = parseResults.GetValue<string?>(LogToFileArgumentName);

        var options = new TcpTapOptions
        {
            ListenPort = listenPort,
            ForwardHost = forwardHost,
            ForwardPort = forwardPort,
            DelayMs = delayMs,
            JitterMinMs = jitterMinMs,
            JitterMaxMs = jitterMaxMs,
            LogToFile = !string.IsNullOrEmpty(logToFilePath),
            FilePath = logToFilePath ?? string.Empty
        };
        
        return RunHost(options, cancellationToken);
    }

    private static async Task RunHost(TcpTapOptions options, CancellationToken cancellationToken)
    {
        var builder = Host.CreateApplicationBuilder();
        
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
        
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<TcpTapApp>();
        builder.Services.AddSingleton<IRecordPublisher, RecordPublisher>();
        builder.Services.AddSingleton<IForwardingBehaviorChainFactory, ForwardingBehaviorChainFactory>();
        builder.Services.AddSingleton<ITextChunkFormatter, HexTextFormatter>();

        using var host = builder.Build();

        var app = host.Services.GetRequiredService<TcpTapApp>();
        await app.RunAppAsync(cancellationToken);
    }
}