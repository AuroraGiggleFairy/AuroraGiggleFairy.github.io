using System;

namespace Platform.EOS;

public static class AntiCheatCommon
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	public static bool NoEacCmdLine;

	public static bool DebugEacVerbose;

	public static readonly object LockObject = new object();

	public static void Init()
	{
		if (!initialized)
		{
			string launchArgument = GameUtils.GetLaunchArgument("debugeac");
			DebugEacVerbose = launchArgument != null && launchArgument == "verbose";
			NoEacCmdLine = GameUtils.GetLaunchArgument("noeac") != null;
			initialized = true;
		}
	}

	public static ClientInfo IntPtrToClientInfo(IntPtr _ptr, string _messageIfNull = null)
	{
		int num = _ptr.ToInt32();
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForClientNumber(num);
		if (clientInfo == null && _messageIfNull != null)
		{
			Log.Error(_messageIfNull, num);
		}
		return clientInfo;
	}

	public static IntPtr ClientInfoToIntPtr(ClientInfo _clientInfo)
	{
		return new IntPtr(_clientInfo.ClientNumber);
	}
}
