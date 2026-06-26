using System;
using System.Diagnostics;
using UnityEngine;

public sealed class BackedArraySingleView<T> : IBackedArrayView<T>, IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArray<T> m_array;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BackedArrayHandleMode m_mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* m_viewPtr;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle m_viewHandle;

	public int Length => m_length;

	public BackedArrayHandleMode Mode => m_mode;

	public unsafe T this[int i]
	{
		get
		{
			Cache(i);
			return m_viewPtr[i - m_viewStart];
		}
		set
		{
			Cache(i);
			m_viewPtr[i - m_viewStart] = value;
		}
	}

	public unsafe BackedArraySingleView(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0, int startingOffset = 0)
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
		m_viewStart = startingOffset;
		if (m_viewStart < 0 || m_viewStart >= m_length)
		{
			throw new IndexOutOfRangeException($"{m_viewStart} is not within length {m_length}.");
		}
		if (m_viewStart + m_viewLength > m_length)
		{
			m_viewStart = m_length - m_viewLength;
		}
		m_viewEnd = m_viewStart + m_viewLength;
		m_viewHandle = mode switch
		{
			BackedArrayHandleMode.ReadOnly => m_array.GetReadOnlyMemoryUnsafe(m_viewStart, m_viewLength, out m_viewPtr), 
			BackedArrayHandleMode.ReadWrite => m_array.GetMemoryUnsafe(m_viewStart, m_viewLength, out m_viewPtr), 
			_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("BackedArraySingleView<T> is being finalized, it should be disposed properly.");
		}
		else if (!IsDisposed())
		{
			m_viewHandle.Dispose();
			m_viewHandle = null;
			m_viewLength = 0;
			m_viewStart = 0;
			m_viewEnd = 0;
			m_viewPtr = null;
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
		return m_viewHandle == null;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void Cache(int i)
	{
		if (m_viewStart > i || i >= m_viewEnd)
		{
			if (i < 0 || i >= m_length)
			{
				throw new IndexOutOfRangeException($"Expected index {i} to be in the range [0, {m_length}).");
			}
			if (i + m_viewLength > m_length)
			{
				i = m_length - m_viewLength;
			}
			m_viewHandle.Dispose();
			m_viewHandle = m_mode switch
			{
				BackedArrayHandleMode.ReadOnly => m_array.GetReadOnlyMemoryUnsafe(i, m_viewLength, out m_viewPtr), 
				BackedArrayHandleMode.ReadWrite => m_array.GetMemoryUnsafe(i, m_viewLength, out m_viewPtr), 
				_ => throw new ArgumentOutOfRangeException("m_mode", m_mode, $"Unknown mode: {m_mode}"), 
			};
			m_viewStart = i;
			m_viewEnd = i + m_viewLength;
		}
	}

	public void Flush()
	{
		m_viewHandle.Flush();
	}

	public static int GetDefaultViewLength(int length)
	{
		return Mathf.NextPowerOfTwo(4 * (int)Math.Sqrt(length));
	}
}
