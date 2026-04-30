using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe readonly struct PinnedBufferRef<T>(T* ptr, int length) where T : unmanaged
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe readonly T* Ptr = ptr;

	public readonly int Length = length;

	public unsafe Span<T> AsSpan()
	{
		return new Span<T>(Ptr, Length);
	}

	public static implicit operator Span<T>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsSpan();
	}

	public unsafe ReadOnlySpan<T> AsReadOnlySpan()
	{
		return new ReadOnlySpan<T>(Ptr, Length);
	}

	public static implicit operator ReadOnlySpan<T>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsReadOnlySpan();
	}

	public unsafe PinnedBufferRef<byte> AsBytes()
	{
		return new PinnedBufferRef<byte>((byte*)Ptr, Length * sizeof(T));
	}

	public static implicit operator PinnedBufferRef<byte>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsBytes();
	}

	public unsafe AtomicSafeHandleScope CreateNativeArray(out NativeArray<T> array)
	{
		return MeshDataUtils.CreateNativeArray(Ptr, Length, out array);
	}
}
