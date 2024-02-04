using Microsoft.Extensions.Options;

namespace DotnetHelp.DevTools.Web.Application;

internal class WebSocketOptions
{
	public string? BaseAddress { get; set; }
}

internal interface IWebSocket : IAsyncDisposable
{
	WebSocketState State { get; }
	event EventHandler<WebSocketMessage>? OnMessage;
	Task ConnectAsync(CancellationToken cancellationToken);
	Task SendAsync(string message, CancellationToken cancellationToken);
}

internal class WebSocket(IStateManagement state, IOptions<WebSocketOptions> options, ILogger<WebSocket> logger) : IWebSocket
{
	private readonly ClientWebSocket _wss = new();
	private CancellationTokenSource? _cancellationToken;
	private Task? _receiveLoopTask;

	public event EventHandler<WebSocketMessage>? OnMessage;

	public WebSocketState State => _wss.State;

	public async Task ConnectAsync(CancellationToken cancellationToken)
	{
		_cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var bucket = await state.GetIdAsync(_cancellationToken.Token);
		await _wss.ConnectAsync(new Uri($"{options.Value.BaseAddress}?bucket={bucket}"), cancellationToken);
		_receiveLoopTask = ReceiveLoop(_cancellationToken.Token);
	}

	public async Task SendAsync(string message, CancellationToken cancellationToken)
	{
		if (_wss.State != WebSocketState.Open)
		{
			throw new InvalidOperationException("WebSocket is not open");
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
			catch (OperationCanceledException)
			{
				// Ignore
			}
			catch (Exception e)
			{
				logger.ErrorReceivingMessage(e);
			}
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_cancellationToken is not null)
		{
			if (_cancellationToken.Token.CanBeCanceled)
			{
				await _cancellationToken.CancelAsync();
			}
		}
		
		if (_wss.State == WebSocketState.Open)
		{
			await _wss.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
		}

		_wss.Dispose();
		_cancellationToken?.Dispose();
	}
}

internal static partial class WebSocketLogger
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Received: {Message}")]
	public static partial void ReceivedMessage(this ILogger logger, string message);

	[LoggerMessage(Level = LogLevel.Error, Message = "Error receiving message")]
	public static partial void ErrorReceivingMessage(this ILogger logger, Exception exception);
}