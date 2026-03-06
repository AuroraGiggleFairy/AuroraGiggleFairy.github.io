using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public sealed class BackedArraySingleView<T> : IBackedArrayView<T>, IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class View : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArraySingleView<T> m_backedArraySingleView;

		public IBackedArrayHandle m_handle;

		public unsafe T* m_ptr;

		public int m_start;

		public int m_end;

		[PublicizedFrom(EAccessModifier.Private)]
		public object m_disposeLock = new object();

		public unsafe T this[int i]
		{
			get
			{
				Cache(i);
				return m_ptr[i - m_start];
			}
			set
			{
				Cache(i);
				m_ptr[i - m_start] = value;
			}
		}

		public View(BackedArraySingleView<T> backedArraySingleView)
		{
			m_backedArraySingleView = backedArraySingleView;
		}

		public unsafe void Cache(int offset)
		{
			if (m_start > offset || offset >= m_end)
			{
				IBackedArray<T> array = m_backedArraySingleView.m_array;
				int length = m_backedArraySingleView.m_length;
				int viewLength = m_backedArraySingleView.m_viewLength;
				BackedArrayHandleMode mode = m_backedArraySingleView.m_mode;
				if (offset < 0 || offset >= length)
				{
					throw new IndexOutOfRangeException($"Expected index {offset} to be in the range [0, {length}).");
				}
				if (offset + viewLength > length)
				{
					offset = length - viewLength;
				}
				m_handle?.Dispose();
				m_handle = mode switch
				{
					BackedArrayHandleMode.ReadOnly => array.GetReadOnlyMemoryUnsafe(offset, viewLength, out m_ptr), 
					BackedArrayHandleMode.ReadWrite => array.GetMemoryUnsafe(offset, viewLength, out m_ptr), 
					_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
				};
				m_start = offset;
				m_end = offset + viewLength;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~View()
		{
			Dispose(isDisposing: false);
		}

		public void Dispose()
		{
			Dispose(isDisposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe void Dispose(bool isDisposing)
		{
			lock (m_disposeLock)
			{
				if (!isDisposing)
				{
					Log.Warning("View<T> is being finalized, it should be disposed properly.");
				}
				if (m_handle != null)
				{
					m_handle.Dispose();
					m_handle = null;
					m_ptr = null;
					m_start = 0;
					m_end = 0;
					Interlocked.Decrement(ref m_backedArraySingleView.m_viewCount);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArray<T> m_array;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BackedArrayHandleMode m_mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_viewLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadLocal<View> m_views;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int VIEW_COUNT_WARNING_THRESHOLD = 16;

	public int Length => m_length;

	public BackedArrayHandleMode Mode => m_mode;

	public T this[int i]
	{
		get
		{
			return m_views.Value[i];
		}
		set
		{
			m_views.Value[i] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public View CreateView()
	{
		View result = new View(this);
		int num = Interlocked.Increment(ref m_viewCount);
		if (num > 16)
		{
			Log.Warning(string.Format("{0}<T> has opened a large amount of array views, this could indicate a memory issue. Count: {1}, View Length: {2}", "BackedArraySingleView", num, m_viewLength));
		}
		return result;
	}

	public BackedArraySingleView(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0)
	{
		m_array = array;
		m_length = array.Length;
		m_mode = mode;
		if (!mode.CanRead())
		{
			throw new ArgumentException("Expected a readable mode.", "mode");
		}
		if (viewLength <= 0)
		{
			viewLength = GetDefaultViewLength(array.Length);
		}
		m_viewLength = Math.Min(m_length, viewLength);
		m_views = new ThreadLocal<View>(CreateView, trackAllValues: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("BackedArraySingleView<T> is being finalized, it should be disposed properly.");
		}
		else
		{
			if (IsDisposed())
			{
				return;
			}
			foreach (View value in m_views.Values)
			{
				value.Dispose();
			}
			m_views.Dispose();
			m_views = null;
			m_array = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~BackedArraySingleView()
	{
		Dispose(disposing: false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (IsDisposed())
		{
			throw new ObjectDisposedException("BackedArraySingleView has already been disposed.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDisposed()
	{
		return m_array == null;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfCannotWrite()
	{
		if (!m_mode.CanWrite())
		{
			throw new NotSupportedException("This BackedArraySingleView is not writable.");
		}
	}

	public void Flush()
	{
		foreach (View value in m_views.Values)
		{
			value.m_handle.Flush();
		}
	}

	public static int GetDefaultViewLength(int length)
	{
		return Mathf.NextPowerOfTwo(4 * (int)Math.Sqrt(length));
	}
}
