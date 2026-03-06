using System;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class Api : IPlatformApi
{
	public const string SCID = "00000000-0000-0000-0000-0000680ee616";

	public const int TitleId = 1745806870;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action clientApiInitialized;

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
		if (ClientApiStatus == EApiStatus.Ok)
		{
			return true;
		}
		if (!XblHelpers.Succeeded(SDK.XGameRuntimeInitialize(), "Initialize gaming runtime"))
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[XBL] Failed to initialize GDK");
			return false;
		}
		if (!XblHelpers.Succeeded(SDK.CreateDefaultTaskQueue(), "Create default task queue"))
		{
			Log.Error("[XBL] Failed to create task queue");
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		if (!XblHelpers.Succeeded(SDK.XBL.XblInitialize("00000000-0000-0000-0000-0000680ee616"), "Initialize Xbox Live"))
		{
			ClientApiStatus = EApiStatus.PermanentError;
			Log.Error("[XBL] Failed to initialize Xbox Live");
			return false;
		}
		Log.Out("[XBL] API loaded");
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
	}

	public void Update()
	{
		if (ClientApiStatus == EApiStatus.Ok)
		{
			SDK.XTaskQueueDispatch(0u);
		}
	}

	public void Destroy()
	{
		SDK.CloseDefaultXTaskQueue();
		SDK.XBL.XblCleanup(null);
		SDK.XGameRuntimeUninitialize();
		ClientApiStatus = EApiStatus.Uninitialized;
	}

	public float GetScreenBoundsValueFromSystem()
	{
		return 1f;
	}
}
