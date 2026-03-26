using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using tcp_tap.Behaviors;
using tcp_tap.Sinks;

namespace tcp_tap;

public class TcpTapApp
{
    private readonly IForwardingBehaviorChainFactory _forwardingBehaviorChainFactory;
    private readonly IRecordPublisher _recordPublisher;
    private readonly TcpTapOptions _options;
    private readonly ILogger<TcpTapApp> _logger;
    
    private Task _currentClientTask = Task.CompletedTask;

    public TcpTapApp(IForwardingBehaviorChainFactory forwardingBehaviorChainFactory, IRecordPublisher recordPublisher, 
        TcpTapOptions options, ILogger<TcpTapApp> logger)
    {
        _forwardingBehaviorChainFactory = forwardingBehaviorChainFactory ?? throw new ArgumentNullException(nameof(forwardingBehaviorChainFactory));
        _recordPublisher = recordPublisher ?? throw new ArgumentNullException(nameof(recordPublisher));
        
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task RunAppAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting TCP tap application with options: {@Options}", _options);
        
        var listener = new TcpListener(IPAddress.Any, _options.ListenPort);
        listener.Start();
        
        Console.WriteLine($"Waiting for incoming connections on port {_options.ListenPort}...");
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);
                if (_currentClientTask.IsCompleted)
                {
                    Console.WriteLine($"Accepted connection from {tcpClient.Client.RemoteEndPoint}");
                    _currentClientTask = HandleClientAsync(tcpClient, cancellationToken);
                    _currentClientTask.ContinueWith(HandleFinishedTask);
                }
                else
                {
                    _logger.LogWarning("A client attempted to connect while another client is still being handled. Rejecting connection.");
                    tcpClient.Close();
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("TCP tap application is shutting down.");
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        TcpClient forwardConnection = null;
        
        try
        {
            var forwardingBehaviorsChain = _forwardingBehaviorChainFactory.GetForwardingBehaviorChain(_options);
            
            forwardConnection = new TcpClient(_options.ForwardHost, _options.ForwardPort);
        
            var handlingTasks = new List<Task>
            {
                ForwardDataAsync(tcpClient.GetStream(), forwardConnection.GetStream(), FlowDirection.SourceToDestination, forwardingBehaviorsChain, cancellationToken),
                ForwardDataAsync(forwardConnection.GetStream(), tcpClient.GetStream(), FlowDirection.DestinationToSource, forwardingBehaviorsChain, cancellationToken)
            };
        
            await Task.WhenAny(handlingTasks);
        }
        finally
        {
            tcpClient.Close();
            forwardConnection?.Close();
        }
    }

    private async Task ForwardDataAsync(NetworkStream source, NetworkStream destination, FlowDirection direction, IReadOnlyList<IForwardingBehavior> forwardingBehaviors, CancellationToken cancellationToken)
    {
        var sourceReader = PipeReader.Create(source);
        var destinationWriter = PipeWriter.Create(destination);
        
        while (true)
        {
            var result = await sourceReader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;
            
            if (buffer.IsEmpty && result.IsCompleted)
            {
                _logger.LogInformation("Source stream completed.");
                break;
            }
            
            foreach (var chunk in buffer)
            {
                var context = new ForwardingContext(DateTime.UtcNow, direction);
                var chunkBuffer = chunk;
                foreach (var forwardingBehavior in forwardingBehaviors)
                {
                    chunkBuffer = await forwardingBehavior.ProcessAsync(chunkBuffer, context, cancellationToken);
                }
                
                context.SendAt = DateTime.UtcNow;
                await RecordChunkAsync(context, chunkBuffer, cancellationToken);
                
                await destinationWriter.WriteAsync(chunkBuffer, cancellationToken);
                await destinationWriter.FlushAsync(cancellationToken);
            }
            
            sourceReader.AdvanceTo(buffer.End);
        }
    }
    
    private void HandleFinishedTask(Task previousTask)
    {
        if (previousTask.IsFaulted)
        {
            _logger.LogError(previousTask.Exception, "An error occurred while handling a client connection.");
        }
        else
        {
            _logger.LogInformation("Finished handling client connection.");
            
            Console.WriteLine("Finished handling client connection.");
            Console.WriteLine($"Waiting for incoming connections on port {_options.ListenPort}...");
        }
    }

    private Task RecordChunkAsync(ForwardingContext context, ReadOnlyMemory<byte> chunk, CancellationToken cancellationToken)
    {
        var chunkRecord = new ChunkRecord
        {
            CapturedAt = context.CapturedAt,
            SendAt = context.SendAt,
            FlowDirection = context.Direction,
            Chunk = chunk.ToArray()
        };
        
        return _recordPublisher.PublishAsync(chunkRecord, cancellationToken);
    }
}