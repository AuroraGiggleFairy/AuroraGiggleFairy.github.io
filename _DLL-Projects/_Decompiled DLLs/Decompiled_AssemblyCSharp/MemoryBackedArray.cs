using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class MemoryBackedArray<T> : IBackedArray<T>, IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class MemoryBackedArrayHandle : IBackedArrayHandle, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;

		public BackedArrayHandleMode Mode => m_mode;

		public MemoryBackedArrayHandle(BackedArrayHandleMode mode)
		{
			m_mode = mode;
		}

		public void Dispose()
		{
		}

		public void Flush()
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class MemoryBackedArrayUnsafeHandle : IBackedArrayHandle, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public GCHandle m_gcHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;

		public BackedArrayHandleMode Mode => m_mode;

		public MemoryBackedArrayUnsafeHandle(GCHandle gcHandle, BackedArrayHandleMode mode)
		{
			m_gcHandle = gcHandle;
			m_mode = mode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("MemoryBackedArrayHandle is being finalized, it should be disposed properly.");
			}
			else if (m_gcHandle != default(GCHandle))
			{
				m_gcHandle.Free();
				m_gcHandle = default(GCHandle);
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~MemoryBackedArrayUnsafeHandle()
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
				throw new ObjectDisposedException("MemoryBackedArrayUnsafeHandle has already been disposed.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsDisposed()
		{
			return m_gcHandle == default(GCHandle);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfCannotWrite()
		{
			if (!m_mode.CanWrite())
			{
				throw new NotSupportedException("This MemoryBackedArrayUnsafeHandle is not writable.");
			}
		}

		public void Flush()
		{
		}
	}

	public sealed class MemoryBackedArrayView : IBackedArrayView<T>, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public T[] m_array;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;

		public int Length => m_array.Length;

		public BackedArrayHandleMode Mode => m_mode;

		public T this[int i]
		{
			get
			{
				return m_array[i];
			}
			set
			{
				m_array[i] = value;
			}
		}

		public MemoryBackedArrayView(MemoryBackedArray<T> array, BackedArrayHandleMode mode)
		{
			m_array = array.m_array;
			m_mode = mode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("MemoryBackedArrayView is being finalized, it should be disposed properly.");
			}
			else
			{
				m_array = null;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~MemoryBackedArrayView()
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
				throw new ObjectDisposedException("MemoryBackedArrayView has already been disposed.");
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
				throw new NotSupportedException("This MemoryBackedArrayView is not writable.");
			}
		}

		public void Flush()
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly MemoryBackedArrayHandle s_handleReadWrite = new MemoryBackedArrayHandle(BackedArrayHandleMode.ReadWrite);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly MemoryBackedArrayHandle s_handleReadOnly = new MemoryBackedArrayHandle(BackedArrayHandleMode.ReadOnly);

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] m_array;

	public int Length => m_array.Length;

	public MemoryBackedArray(int length)
	{
		m_array = new T[length];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("MemoryBackedArray<T> is being finalized, it should be disposed properly.");
		}
		else
		{
			m_array = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~MemoryBackedArray()
	{
		Dispose(disposing: false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (m_array == null)
		{
			throw new ObjectDisposedException("MemoryBackedArray has already been disposed.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryBackedArrayHandle GetStaticHandle(BackedArrayHandleMode mode)
	{
		return mode switch
		{
			BackedArrayHandleMode.ReadOnly => s_handleReadOnly, 
			BackedArrayHandleMode.ReadWrite => s_handleReadWrite, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle GetMemoryInternal(int start, int length, out Memory<T> memory, BackedArrayHandleMode mode)
	{
		memory = m_array.AsMemory(start, length);
		return GetStaticHandle(mode);
	}

	public IBackedArrayHandle GetMemory(int start, int length, out Memory<T> memory)
	{
		return GetMemoryInternal(start, length, out memory, BackedArrayHandleMode.ReadWrite);
	}

	public IBackedArrayHandle GetReadOnlyMemory(int start, int length, out ReadOnlyMemory<T> memory)
	{
		Memory<T> memory2;
		IBackedArrayHandle memoryInternal = GetMemoryInternal(start, length, out memory2, BackedArrayHandleMode.ReadOnly);
		memory = memory2;
		return memoryInternal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe IBackedArrayHandle GetMemoryUnsafeInternal(int start, int length, out T* arrayPtr, BackedArrayHandleMode mode)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException($"Expected length to be non-negative but was {length}.");
		}
		if (start < 0 || start + length > m_array.Length)
		{
			throw new ArgumentOutOfRangeException($"Expected requested memory range [{start}, {start + length}) to be a subset of [0, {m_array.Length}).");
		}
		GCHandle gcHandle = GCHandle.Alloc(m_array, GCHandleType.Pinned);
		arrayPtr = (T*)(void*)gcHandle.AddrOfPinnedObject();
		arrayPtr += start;
		return new MemoryBackedArrayUnsafeHandle(gcHandle, mode);
	}

	public unsafe IBackedArrayHandle GetMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		return GetMemoryUnsafeInternal(start, length, out arrayPtr, BackedArrayHandleMode.ReadWrite);
	}

	public unsafe IBackedArrayHandle GetReadOnlyMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		return GetMemoryUnsafeInternal(start, length, out arrayPtr, BackedArrayHandleMode.ReadOnly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle GetSpanInternal(int start, int length, out Span<T> span, BackedArrayHandleMode mode)
	{
		span = m_array.AsSpan(start, length);
		return GetStaticHandle(mode);
	}

	public IBackedArrayHandle GetSpan(int start, int length, out Span<T> span)
	{
		return GetSpanInternal(start, length, out span, BackedArrayHandleMode.ReadWrite);
	}

	public IBackedArrayHandle GetReadOnlySpan(int start, int length, out ReadOnlySpan<T> span)
	{
		Span<T> span2;
		IBackedArrayHandle spanInternal = GetSpanInternal(start, length, out span2, BackedArrayHandleMode.ReadOnly);
		span = span2;
		return spanInternal;
	}
}
