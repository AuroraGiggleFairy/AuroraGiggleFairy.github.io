using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeFixedBuffer<T> : IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Data
	{
		public unsafe T* buffer;

		public int count;
	}

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe Data* data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int capacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;

	public unsafe int Count => data->count;

	public unsafe bool IsCreated => data != null;

	public unsafe UnsafeFixedBuffer(int _capacity, AllocatorManager.AllocatorHandle _allocator)
	{
		data = AllocatorManager.Allocate<Data>(_allocator);
		data->buffer = AllocatorManager.Allocate<T>(_allocator, _capacity);
		data->count = 0;
		capacity = _capacity;
		allocator = _allocator;
	}

	public unsafe void AddThreadSafe(T item)
	{
		int count;
		int num;
		do
		{
			count = data->count;
			num = data->count + 1;
			if (num > capacity)
			{
				throw new IndexOutOfRangeException($"Index {count} is outside the UnsafeFixedBuffer capacity {capacity}");
			}
		}
		while (Interlocked.CompareExchange(ref data->count, num, count) != count);
		UnsafeUtility.WriteArrayElement(data->buffer, count, item);
	}

	public unsafe void Clear()
	{
		data->count = 0;
	}

	public unsafe NativeArray<T> AsNativeArray()
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data->buffer, data->count, Allocator.Invalid);
	}

	public unsafe void Dispose()
	{
		if (data != null)
		{
			AllocatorManager.Free(allocator, data->buffer, capacity);
			AllocatorManager.Free(allocator, data);
			data = null;
		}
	}

	public int CalculateOwnedBytes()
	{
		return UnsafeUtility.SizeOf<UnsafeFixedBuffer<T>>() + CollectionHelper.Align(capacity * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
	}
}
