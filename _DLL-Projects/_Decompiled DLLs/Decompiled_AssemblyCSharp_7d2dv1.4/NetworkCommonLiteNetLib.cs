using System.Text;
using LiteNetLib;

public static class NetworkCommonLiteNetLib
{
	public enum EAdditionalDisconnectCause : byte
	{
		InvalidPassword = 0,
		RateLimit = 1,
		PendingConnection = 2,
		ServerShutdown = 3,
		ClientSideDisconnect = 4,
		Other = byte.MaxValue
	}

	public const int PORT_OFFSET = 2;

	public const int MTU_OVERRIDE = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	static NetworkCommonLiteNetLib()
	{
	}

	public static bool InitConfig(NetManager _manager)
	{
		_manager.UnsyncedEvents = true;
		_manager.UnsyncedDeliveryEvent = true;
		_manager.UnsyncedReceiveEvent = true;
		_manager.AutoRecycle = true;
		_manager.DisconnectOnUnreachable = true;
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsLiteNetLibMtuOverride);
		if (flag)
		{
			Log.Out($"NET: LiteNetLib: MTU Override enabled ({1024})");
		}
		_manager.MtuOverride = (flag ? 1024 : 0);
		return true;
	}

	public static byte[] CreateRejectMessage(string _customText)
	{
		int byteCount = Encoding.UTF8.GetByteCount(_customText);
		byte[] array = new byte[2 + byteCount];
		array[0] = byte.MaxValue;
		array[1] = (byte)Encoding.UTF8.GetBytes(_customText, 0, _customText.Length, array, 2);
		return array;
	}
}
