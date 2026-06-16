using System;
using System.Diagnostics;
using System.Threading;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save;

public sealed class SizeTracker : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int DefaultMaxSaveDataBytes = 1073741824;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly long m_maxBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SizeTrackerGetRemainingQuotaAsync m_getRemainingQuotaAsync;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_shouldUpdateSizesOnEstimate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_disposed;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_refreshCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_refreshCountExpected;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSizes m_sizes;

	public SaveDataSizes Sizes
	{
		get
		{
			lock (m_lock)
			{
				return m_sizes;
			}
		}
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string text)
	{
		Log.Out("[XBL: SizeTracker] " + text);
	}

	public SizeTracker(long max, long used, SizeTrackerGetRemainingQuotaAsync getRemainingQuotaAsync, bool shouldUpdateSizesOnEstimate)
	{
		m_maxBytes = max;
		m_getRemainingQuotaAsync = getRemainingQuotaAsync;
		m_shouldUpdateSizesOnEstimate = shouldUpdateSizesOnEstimate;
		m_sizes = new SaveDataSizes(max, max - used);
	}

	public void Dispose()
	{
		lock (m_lock)
		{
			m_disposed = true;
		}
	}

	public void UpdateUsedEstimate(long deltaUsed)
	{
		lock (m_lock)
		{
			if (m_disposed)
			{
				return;
			}
			m_sizes = new SaveDataSizes(m_sizes.Total, Math.Max(m_sizes.Remaining - deltaUsed, 0L));
		}
		if (m_shouldUpdateSizesOnEstimate)
		{
			RefreshAsync();
		}
	}

	public void RefreshSync()
	{
		if (!m_disposed)
		{
			long num = Interlocked.Increment(ref m_refreshCountExpected);
			m_getRemainingQuotaAsync(XGameSaveGetRemainingQuotaAsyncCompleted);
			while (Interlocked.Read(ref m_refreshCount) < num)
			{
				Thread.Sleep(16);
			}
		}
	}

	public void RefreshAsync()
	{
		lock (m_lock)
		{
			if (m_disposed)
			{
				return;
			}
		}
		long num = Interlocked.Read(ref m_refreshCountExpected);
		if (Interlocked.Read(ref m_refreshCount) < num && Interlocked.CompareExchange(ref m_refreshCountExpected, num + 1, num) == num)
		{
			m_getRemainingQuotaAsync(XGameSaveGetRemainingQuotaAsyncCompleted);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XGameSaveGetRemainingQuotaAsyncCompleted(int hrXGameSaveGetRemainingQuota, long remaining)
	{
		if (HR.FAILED(hrXGameSaveGetRemainingQuota))
		{
			GameCoreSaveHelpers.NonTraceLogHR(hrXGameSaveGetRemainingQuota, "SizeTracker#XGameSaveGetRemainingQuotaAsync");
			Interlocked.Increment(ref m_refreshCount);
			return;
		}
		lock (m_lock)
		{
			if (m_disposed)
			{
				Interlocked.Increment(ref m_refreshCount);
				return;
			}
		}
		_ = m_sizes.Total;
		long total = m_sizes.Total;
		lock (m_lock)
		{
			if (m_disposed)
			{
				Interlocked.Increment(ref m_refreshCount);
				return;
			}
			m_sizes = new SaveDataSizes(total, remaining);
		}
		Interlocked.Increment(ref m_refreshCount);
	}
}
