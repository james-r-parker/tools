namespace DotnetHelp.DevTools.Web.Application;

public interface IWebsocket
{
    Uri Uri { get; }
    WebSocketState State { get; }
    event EventHandler<WebSocketMessage>? OnMessage;
    Task ConnectAsync(Uri url, CancellationToken cancellationToken);
    Task SendAsync(string message, CancellationToken cancellationToken);
}

public class Websocket(ILogger<Websocket> logger) : IDisposable, IWebsocket
{
    private readonly ClientWebSocket _wss = new();
    private CancellationTokenSource? _cancellationToken;
    private Task? _receiveLoopTask;
    private Uri? _uri;

    public event EventHandler<WebSocketMessage>? OnMessage;

    public Uri Uri => _uri ?? throw new InvalidOperationException("Websocket is not connected");
    public WebSocketState State => _wss.State;

    public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (_wss.State == WebSocketState.Open || _wss.State == WebSocketState.Connecting)
        {
            return;
        }

        _uri = uri;
        _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await _wss.ConnectAsync(uri, cancellationToken);
        _receiveLoopTask = ReceiveLoop(_cancellationToken.Token);
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        if (_wss.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("Websocket is not open");
        }

        byte[] bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(new byte[4096]);
        for (var i = 0; i < bytes.Length; i += buffer.Count)
        {
            int count = Math.Min(buffer.Count, bytes.Length - i);
            Buffer.BlockCopy(bytes, i, buffer.Array!, 0, count);
            await _wss.SendAsync(buffer, WebSocketMessageType.Text, i + count >= bytes.Length, cancellationToken);
        }
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        while (!cancellationToken.IsCancellationRequested &&
               _wss.State == WebSocketState.Open)
        {
            try
            {
                MemoryStream bytes = new();
                bool endOfMessage;

                do
                {
                    var received = await _wss.ReceiveAsync(buffer, cancellationToken);
                    bytes.Write(buffer.Array!, 0, received.Count);
                    endOfMessage = received.EndOfMessage;
                } while (!endOfMessage);

                if (bytes.Length == 0)
                {
                    continue;
                }

                string receivedAsText = Encoding.UTF8.GetString(bytes.ToArray());

                if (string.IsNullOrWhiteSpace(receivedAsText))
                {
                    continue;
                }

                logger.ReceivedMessage(receivedAsText);
                WebSocketMessage? message = JsonSerializer.Deserialize<WebSocketMessage>(receivedAsText);

                if (message is null)
                {
                    continue;
                }

                OnMessage?.Invoke(this, message);
            }
            catch (Exception e)
            {
                logger.ErrorReceivingMessage(e);
            }
        }
    }

    public void Dispose()
    {
        _cancellationToken?.Cancel();
        _cancellationToken?.Dispose();
        _receiveLoopTask?.Dispose();
        _wss.Dispose();
    }
}

internal static partial class WebsocketLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Received: {Message}")]
    public static partial void ReceivedMessage(this ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error receiving message")]
    public static partial void ErrorReceivingMessage(this ILogger logger, Exception exception);
}