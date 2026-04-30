using System.Net;

namespace Platform.LAN;

public static class LANServerSearchConfig
{
	public static readonly IPAddress MulticastGroupIp = IPAddress.Parse("239.192.0.1");

	public const int DefaultPort = 11000;
}
