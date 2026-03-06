using System;
using System.Text;
using Steamworks;

namespace Platform.Steam;

public class Api : IPlatformApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Action clientApiInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(_bStart: false);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EApiStatus ClientApiStatus
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = EApiStatus.Uninitialized;

	public event Action ClientApiInitialized
	{
		add
		{
			lock (this)
			{
				clientApiInitialized = (Action)Delegate.Combine(clientApiInitialized, value);
				if (ClientApiStatus == EApiStatus.Ok)
				{
					value();
				}
			}
		}
		remove
		{
			lock (this)
			{
				clientApiInitialized = (Action)Delegate.Remove(clientApiInitialized, value);
			}
		}
	}

	public void Init(IPlatform _owner)
	{
	}

	public bool InitClientApis()
	{
		if (!Packsize.Test())
		{
			Log.Out("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		if (!DllCheck.Test())
		{
			Log.Out("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		try
		{
			if (!SteamAPI.Init())
			{
				Log.Out("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
				ClientApiStatus = EApiStatus.TemporaryError;
				return false;
			}
		}
		catch (DllNotFoundException ex)
		{
			Log.Out("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		Log.Out("[Steamworks.NET] SteamAPI_Init() ok");
		SteamClient.SetWarningMessageHook(ExceptionThrown);
		SteamUtils.SetOverlayNotificationPosition(ENotificationPosition.k_EPositionTopRight);
		ClientApiStatus = EApiStatus.Ok;
		clientApiInitialized?.Invoke();
		return true;
	}

	public bool InitServerApis()
	{
		return true;
	}

	public void ServerApiLoaded()
	{
		if (ClientApiStatus != EApiStatus.Ok)
		{
			clientApiInitialized?.Invoke();
		}
	}

	public void Update()
	{
		if (ClientApiStatus == EApiStatus.Ok)
		{
			tickDurationStopwatch.Restart();
			SteamAPI.RunCallbacks();
			long num = tickDurationStopwatch.ElapsedMicroseconds / 1000;
			if (num > 25)
			{
				Log.Warning($"[Steam] Tick took exceptionally long: {num} ms");
			}
		}
	}

	public void Destroy()
	{
		if (ClientApiStatus == EApiStatus.Ok)
		{
			SteamAPI.Shutdown();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExceptionThrown(int _severity, StringBuilder _message)
	{
		Log.Error("[Steamworks.NET] " + ((_severity == 0) ? "Info: " : "Warning: ") + ": " + _message);
	}

	public float GetScreenBoundsValueFromSystem()
	{
		return 1f;
	}
}
