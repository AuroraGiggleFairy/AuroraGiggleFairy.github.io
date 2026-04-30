namespace Discord;

internal enum ConnectionState : byte
{
	Disconnected,
	Connecting,
	Connected,
	Disconnecting
}
