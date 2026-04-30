using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[NativeContainer]
public struct NativeSafeHandle<T> : IDisposable where T : struct, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public T target;

	public T Target => target;

	public NativeSafeHandle(ref T _target, Allocator allocator)
	{
		target = _target;
	}

	public void Dispose()
	{
		target.Dispose();
	}
}
