using System;

namespace Discord.Net;

internal class WebSocketClosedException : Exception
{
	public int CloseCode { get; }

	public string Reason { get; }

	public WebSocketClosedException(int closeCode, string reason = null)
		: base(string.Format("The server sent close {0}{1}", closeCode, (reason != null) ? (": \"" + reason + "\"") : ""))
	{
		CloseCode = closeCode;
		Reason = reason;
	}
}
