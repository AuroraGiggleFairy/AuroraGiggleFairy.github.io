using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Rest;

internal class TypingNotifier : IDisposable
{
	private readonly CancellationTokenSource _cancelToken;

	private readonly IMessageChannel _channel;

	private readonly RequestOptions _options;

	public TypingNotifier(IMessageChannel channel, RequestOptions options)
	{
		_cancelToken = new CancellationTokenSource();
		_channel = channel;
		_options = options;
		RunAsync();
	}

	private async Task RunAsync()
	{
		_ = 1;
		try
		{
			CancellationToken token = _cancelToken.Token;
			while (!_cancelToken.IsCancellationRequested)
			{
				try
				{
					await _channel.TriggerTypingAsync(_options).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch
				{
				}
				await Task.Delay(9500, token).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void Dispose()
	{
		_cancelToken.Cancel();
	}
}
