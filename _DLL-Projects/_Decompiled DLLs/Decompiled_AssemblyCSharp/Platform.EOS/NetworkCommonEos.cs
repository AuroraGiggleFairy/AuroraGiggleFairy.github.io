namespace Platform.EOS;

public static class NetworkCommonEos
{
	public enum ESteamNetChannels : byte
	{
		NetpackageChannel0 = 0,
		NetpackageChannel1 = 1,
		Authentication = 50,
		Ping = 60
	}

	public readonly struct SendInfo(ClientInfo _clientInfo, ArrayListMP<byte> _data)
	{
		public readonly ClientInfo Recipient = _clientInfo;

		public readonly ArrayListMP<byte> Data = _data;
	}

	public const int MaxUsedPacketSize = 1120;
}
