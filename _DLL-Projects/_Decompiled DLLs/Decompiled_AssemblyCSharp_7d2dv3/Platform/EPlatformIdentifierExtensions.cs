using System;

namespace Platform;

public static class EPlatformIdentifierExtensions
{
	public static bool IsNative(this EPlatformIdentifier platformIdentifier)
	{
		return platformIdentifier switch
		{
			EPlatformIdentifier.None => false, 
			EPlatformIdentifier.Local => true, 
			EPlatformIdentifier.EOS => false, 
			EPlatformIdentifier.Steam => true, 
			EPlatformIdentifier.XBL => true, 
			EPlatformIdentifier.PSN => true, 
			EPlatformIdentifier.EGS => true, 
			EPlatformIdentifier.LAN => false, 
			EPlatformIdentifier.Count => false, 
			_ => throw new ArgumentOutOfRangeException("platformIdentifier", platformIdentifier, null), 
		};
	}

	public static bool IsCross(this EPlatformIdentifier platformIdentifier)
	{
		return platformIdentifier switch
		{
			EPlatformIdentifier.None => true, 
			EPlatformIdentifier.Local => false, 
			EPlatformIdentifier.EOS => true, 
			EPlatformIdentifier.Steam => false, 
			EPlatformIdentifier.XBL => false, 
			EPlatformIdentifier.PSN => false, 
			EPlatformIdentifier.EGS => false, 
			EPlatformIdentifier.LAN => false, 
			EPlatformIdentifier.Count => false, 
			_ => throw new ArgumentOutOfRangeException("platformIdentifier", platformIdentifier, null), 
		};
	}

	public static bool IsServer(this EPlatformIdentifier platformIdentifier)
	{
		return platformIdentifier switch
		{
			EPlatformIdentifier.None => false, 
			EPlatformIdentifier.Local => false, 
			EPlatformIdentifier.EOS => false, 
			EPlatformIdentifier.Steam => true, 
			EPlatformIdentifier.XBL => true, 
			EPlatformIdentifier.PSN => true, 
			EPlatformIdentifier.EGS => true, 
			EPlatformIdentifier.LAN => true, 
			EPlatformIdentifier.Count => false, 
			_ => throw new ArgumentOutOfRangeException("platformIdentifier", platformIdentifier, null), 
		};
	}

	public static bool IsServerValid(this EPlatformIdentifier serverPlatform, EPlatformIdentifier nativePlatform, EPlatformIdentifier crossPlatform)
	{
		bool flag = serverPlatform.IsServer();
		if (flag)
		{
			bool flag2;
			switch (serverPlatform)
			{
			case EPlatformIdentifier.Steam:
				flag2 = true;
				break;
			case EPlatformIdentifier.LAN:
				flag2 = true;
				break;
			case EPlatformIdentifier.XBL:
			case EPlatformIdentifier.PSN:
			case EPlatformIdentifier.EGS:
				flag2 = crossPlatform == EPlatformIdentifier.EOS;
				break;
			default:
				throw new ArgumentOutOfRangeException("serverPlatform", serverPlatform, null);
			}
			flag = flag2;
		}
		return flag;
	}
}
