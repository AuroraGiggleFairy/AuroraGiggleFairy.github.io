using System;

namespace Discord.Net.Udp;

internal static class DefaultUdpSocketProvider
{
	public static readonly UdpSocketProvider Instance = delegate
	{
		try
		{
			return new DefaultUdpSocket();
		}
		catch (PlatformNotSupportedException inner)
		{
			throw new PlatformNotSupportedException("The default UdpSocketProvider is not supported on this platform.", inner);
		}
	};
}
