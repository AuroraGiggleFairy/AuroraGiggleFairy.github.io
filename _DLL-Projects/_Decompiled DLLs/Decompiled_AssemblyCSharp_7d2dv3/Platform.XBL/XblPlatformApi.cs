using System;
using System.Threading;
using Unity.Profiling;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;
using UnityEngine;
using UnityEngine.Profiling;

namespace Platform.XBL;

public abstract class XblPlatformApi : IPlatformApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_TaskQueueDispatchMarker = new ProfilerMarker("XTaskQueueDispatch");

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread m_dispatchGXDKTaskQueueThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dispatchGXDKTaskQueueRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action m_clientApiInitialized;

	public abstract string SCID { get; }

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
				m_clientApiInitialized = (Action)Delegate.Combine(m_clientApiInitialized, value);
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
				m_clientApiInitialized = (Action)Delegate.Remove(m_clientApiInitialized, value);
			}
		}
	}

	public abstract void Init(IPlatform owner);

	public bool InitClientApis()
	{
		if (ClientApiStatus == EApiStatus.Ok)
		{
			return true;
		}
		if (Application.isEditor)
		{
			ClientApiStatus = EApiStatus.Ok;
			m_clientApiInitialized?.Invoke();
			return true;
		}
		int hr = SDK.XGameRuntimeInitialize();
		XblHelpers.LogHR(hr, "Initialize Gaming Runtime.");
		if (Unity.XGamingRuntime.Interop.HR.FAILED(hr))
		{
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		if (Unity.XGamingRuntime.Interop.HR.FAILED(SDK.CreateDefaultTaskQueue()))
		{
			ClientApiStatus = EApiStatus.PermanentError;
			return false;
		}
		m_dispatchGXDKTaskQueueRunning = true;
		m_dispatchGXDKTaskQueueThread = new Thread([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Log.Out("[XBL] Started Thread: " + Thread.CurrentThread.Name);
			try
			{
				while (m_dispatchGXDKTaskQueueRunning)
				{
					try
					{
						SDK.XTaskQueueDispatch(32u);
					}
					catch (ThreadInterruptedException)
					{
						break;
					}
					catch (Exception e)
					{
						Log.Exception(e);
					}
					finally
					{
					}
				}
			}
			finally
			{
				Profiler.EndThreadProfiling();
				Log.Out("[XBL] Stopped Thread: " + Thread.CurrentThread.Name);
			}
		})
		{
			Name = "GXDK Task Queue Dispatch Completion",
			IsBackground = true
		};
		m_dispatchGXDKTaskQueueThread.Start();
		int hr2 = SDK.XBL.XblInitialize(SCID);
		XblHelpers.LogHR(hr2, "Initialize Xbox Live.");
		if (Unity.XGamingRuntime.Interop.HR.FAILED(hr2))
		{
			return false;
		}
		Log.Out("[XBL] API loaded.");
		ClientApiStatus = EApiStatus.Ok;
		m_clientApiInitialized?.Invoke();
		return true;
	}

	public abstract bool InitServerApis();

	public abstract void ServerApiLoaded();

	public void Update()
	{
	}

	public void Destroy()
	{
		if (!Application.isEditor)
		{
			m_dispatchGXDKTaskQueueRunning = false;
			SDK.CloseDefaultXTaskQueue();
			SDK.XBL.XblCleanup([PublicizedFrom(EAccessModifier.Internal)] (int hr) =>
			{
				XblHelpers.LogHR(hr, "Uninitialize Xbox Live.");
			});
			m_dispatchGXDKTaskQueueThread?.Interrupt();
			m_dispatchGXDKTaskQueueThread?.Join();
			m_dispatchGXDKTaskQueueThread = null;
			SDK.XGameRuntimeUninitialize();
			XblHelpers.LogHR(0, "Uninitialize Gaming Runtime.");
			Log.Out("[XBL] API Destroyed.");
		}
	}

	public float GetScreenBoundsValueFromSystem()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XblPlatformApi()
	{
	}
}
