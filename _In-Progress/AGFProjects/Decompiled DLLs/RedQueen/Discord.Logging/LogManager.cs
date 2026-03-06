using System;
using System.Threading.Tasks;

namespace Discord.Logging;

internal class LogManager
{
	private readonly AsyncEvent<Func<LogMessage, Task>> _messageEvent = new AsyncEvent<Func<LogMessage, Task>>();

	public LogSeverity Level { get; }

	private Logger ClientLogger { get; }

	public event Func<LogMessage, Task> Message
	{
		add
		{
			_messageEvent.Add(value);
		}
		remove
		{
			_messageEvent.Remove(value);
		}
	}

	public LogManager(LogSeverity minSeverity)
	{
		Level = minSeverity;
		ClientLogger = new Logger(this, "Discord");
	}

	public async Task LogAsync(LogSeverity severity, string source, Exception ex)
	{
		try
		{
			if (severity <= Level)
			{
				await _messageEvent.InvokeAsync(new LogMessage(severity, source, null, ex)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch
		{
		}
	}

	public async Task LogAsync(LogSeverity severity, string source, string message, Exception ex = null)
	{
		try
		{
			if (severity <= Level)
			{
				await _messageEvent.InvokeAsync(new LogMessage(severity, source, message, ex)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch
		{
		}
	}

	public async Task LogAsync(LogSeverity severity, string source, FormattableString message, Exception ex = null)
	{
		try
		{
			if (severity <= Level)
			{
				await _messageEvent.InvokeAsync(new LogMessage(severity, source, message.ToString(), ex)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch
		{
		}
	}

	public Task ErrorAsync(string source, Exception ex)
	{
		return LogAsync(LogSeverity.Error, source, ex);
	}

	public Task ErrorAsync(string source, string message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Error, source, message, ex);
	}

	public Task ErrorAsync(string source, FormattableString message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Error, source, message, ex);
	}

	public Task WarningAsync(string source, Exception ex)
	{
		return LogAsync(LogSeverity.Warning, source, ex);
	}

	public Task WarningAsync(string source, string message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Warning, source, message, ex);
	}

	public Task WarningAsync(string source, FormattableString message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Warning, source, message, ex);
	}

	public Task InfoAsync(string source, Exception ex)
	{
		return LogAsync(LogSeverity.Info, source, ex);
	}

	public Task InfoAsync(string source, string message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Info, source, message, ex);
	}

	public Task InfoAsync(string source, FormattableString message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Info, source, message, ex);
	}

	public Task VerboseAsync(string source, Exception ex)
	{
		return LogAsync(LogSeverity.Verbose, source, ex);
	}

	public Task VerboseAsync(string source, string message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Verbose, source, message, ex);
	}

	public Task VerboseAsync(string source, FormattableString message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Verbose, source, message, ex);
	}

	public Task DebugAsync(string source, Exception ex)
	{
		return LogAsync(LogSeverity.Debug, source, ex);
	}

	public Task DebugAsync(string source, string message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Debug, source, message, ex);
	}

	public Task DebugAsync(string source, FormattableString message, Exception ex = null)
	{
		return LogAsync(LogSeverity.Debug, source, message, ex);
	}

	public Logger CreateLogger(string name)
	{
		return new Logger(this, name);
	}

	public async Task WriteInitialLog()
	{
		await ClientLogger.InfoAsync($"Discord.Net v{DiscordConfig.Version} (API v{10})").ConfigureAwait(continueOnCapturedContext: false);
	}
}
