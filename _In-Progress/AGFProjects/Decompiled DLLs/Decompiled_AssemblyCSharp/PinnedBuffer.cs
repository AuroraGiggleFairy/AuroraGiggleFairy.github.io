using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public abstract class PinnedBuffer : IDisposable
{
	public static PinnedBuffer<T> Create<T>(ArrayListMP<T> list, bool copy) where T : unmanaged
	{
		if (list == null || list.Count <= 0)
		{
			return new PinnedBuffer<T>();
		}
		if (!copy)
		{
			return new PinnedBuffer<T>(list.Items, list.Count);
		}
		PinnedBuffer<T> pinnedBuffer = new PinnedBuffer<T>(list.pool, list.Count);
		list.Items.AsSpan(0, list.Count).CopyTo(pinnedBuffer.AsSpan());
		return pinnedBuffer;
	}

	public abstract void Dispose();

	[PublicizedFrom(EAccessModifier.Protected)]
	public PinnedBuffer()
	{
	}
}
public sealed class PinnedBuffer<T> : PinnedBuffer where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledArray<T> m_pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] m_poolArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* m_ptr;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong m_gcHandle;

	public unsafe T* Ptr => m_ptr;

	public int Length => m_length;

	public PinnedBuffer(MemoryPooledArray<T> pool, int length)
	{
		if (length <= 0)
		{
			InitBuffer(Array.Empty<T>(), 0);
			return;
		}
		m_pool = pool;
		m_poolArray = m_pool.Alloc(length);
		InitBuffer(m_poolArray, length);
	}

	public PinnedBuffer()
	{
		InitBuffer(Array.Empty<T>(), 0);
	}

	public PinnedBuffer(int length)
	{
		if (length <= 0)
		{
			InitBuffer(Array.Empty<T>(), 0);
		}
		else
		{
			InitBuffer(new T[length], length);
		}
	}

	public PinnedBuffer(T[] buffer, int length)
	{
		if (length <= 0)
		{
			InitBuffer(Array.Empty<T>(), 0);
		}
		else
		{
			InitBuffer(buffer, length);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void InitBuffer(T[] buffer, int length)
	{
		m_ptr = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(buffer, out m_gcHandle);
		m_length = length;
	}

	public unsafe override void Dispose()
	{
		m_length = 0;
		m_ptr = null;
		if (m_gcHandle != 0)
		{
			UnsafeUtility.ReleaseGCObject(m_gcHandle);
			m_gcHandle = 0uL;
		}
		if (m_pool != null)
		{
			m_pool.Free(m_poolArray);
			m_pool = null;
		}
		m_poolArray = null;
	}

	public unsafe PinnedBufferRef<T> AsRef()
	{
		return new PinnedBufferRef<T>(m_ptr, m_length);
	}

	public static implicit operator PinnedBufferRef<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsRef();
	}

	public PinnedBufferRef<byte> AsBytes()
	{
		return AsRef().AsBytes();
	}

	public static implicit operator PinnedBufferRef<byte>(PinnedBuffer<T> buffer)
	{
		return buffer.AsBytes();
	}

	public unsafe Span<T> AsSpan()
	{
		return new Span<T>(m_ptr, m_length);
	}

	public static implicit operator Span<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsSpan();
	}

	public unsafe ReadOnlySpan<T> AsReadOnlySpan()
	{
		return new ReadOnlySpan<T>(m_ptr, m_length);
	}

	public static implicit operator ReadOnlySpan<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsReadOnlySpan();
	}

	public unsafe AtomicSafeHandleScope CreateNativeArray(out NativeArray<T> array)
	{
		return MeshDataUtils.CreateNativeArray(m_ptr, m_length, out array);
	}
}
