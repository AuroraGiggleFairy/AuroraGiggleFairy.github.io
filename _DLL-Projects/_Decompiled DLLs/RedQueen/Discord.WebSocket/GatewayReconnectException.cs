using System;

namespace Discord.WebSocket;

internal class GatewayReconnectException : Exception
{
	public GatewayReconnectException(string message)
		: base(message)
	{
	}
}
