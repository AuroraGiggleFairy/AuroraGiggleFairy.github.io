using Steamworks;

namespace Platform.Steam;

public static class NetworkCommonSteam
{
	public enum ESteamNetChannels : byte
	{
		NetpackageChannel0 = 0,
		NetpackageChannel1 = 1,
		Authentication = 50,
		Ping = 60
	}

	public readonly struct SendInfo(CSteamID _recipient, ArrayListMP<byte> _data)
	{
		public readonly CSteamID Recipient = _recipient;

		public readonly ArrayListMP<byte> Data = _data;
	}
}
