using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeChunkData<T> : IDisposable where T : unmanaged, IEquatable<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct LayerData
	{
		public unsafe T* items;
	}

	public const int LAYER_SIZE = 1024;

	public const int NUM_LAYERS = 64;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe LayerData* layers;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* sameValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;

	public unsafe bool IsCreated => layers != null;

	public unsafe UnsafeChunkData(AllocatorManager.AllocatorHandle _allocator)
	{
		allocator = _allocator;
		layers = AllocatorManager.Allocate<LayerData>(allocator, 64);
		UnsafeUtility.MemClear(layers, UnsafeUtility.SizeOf<LayerData>() * 64);
		sameValues = AllocatorManager.Allocate<T>(allocator, 64);
		UnsafeUtility.MemClear(sameValues, UnsafeUtility.SizeOf<T>() * 64);
	}

	public unsafe T Get(int _x, int _y, int _z)
	{
		int num = _y / 4;
		LayerData* ptr = layers + num;
		if (ptr->items == null)
		{
			return sameValues[num];
		}
		int num2 = _x + 16 * _z;
		int num3 = _y % 4;
		int num4 = num2 + num3 * 256;
		return ptr->items[num4];
	}

	public unsafe T Get(int _chunkIndex)
	{
		int num = _chunkIndex / 256;
		int num2 = num / 4;
		LayerData* ptr = layers + num2;
		if (ptr->items == null)
		{
			return sameValues[num2];
		}
		int num3 = _chunkIndex % 256;
		int num4 = num % 4;
		int num5 = num3 + num4 * 256;
		return ptr->items[num5];
	}

	public void CheckSameValues()
	{
		for (int i = 0; i < 64; i++)
		{
			CheckSameValue(i);
		}
	}

	public unsafe void CheckSameValue(int _layerIndex)
	{
		LayerData* ptr = layers + _layerIndex;
		T* items = ptr->items;
		if (items == null || items == null)
		{
			return;
		}
		T val = *items;
		for (int i = 1; i < 1024; i++)
		{
			if (!val.Equals(items[i]))
			{
				return;
			}
		}
		sameValues[_layerIndex] = val;
		AllocatorManager.Free(allocator, ptr->items, 1024);
		ptr->items = null;
	}

	public unsafe void Set(int _chunkIndex, T _value)
	{
		int num = _chunkIndex / 256;
		int num2 = num / 4;
		LayerData* ptr = layers + num2;
		if (ptr->items == null)
		{
			T* ptr2 = AllocatorManager.Allocate<T>(allocator, 1024);
			T val = sameValues[num2];
			for (int i = 0; i < 1024; i++)
			{
				ptr2[i] = val;
			}
			ptr->items = ptr2;
		}
		int num3 = _chunkIndex % 256;
		int num4 = num % 4;
		int num5 = num3 + num4 * 256;
		ptr->items[num5] = _value;
	}

	public unsafe void Clear()
	{
		if (layers != null)
		{
			for (int i = 0; i < 64; i++)
			{
				LayerData* ptr = layers + i;
				if (ptr->items != null)
				{
					AllocatorManager.Free(allocator, ptr->items, 1024);
					ptr->items = null;
				}
			}
		}
		if (sameValues != null)
		{
			UnsafeUtility.MemClear(sameValues, UnsafeUtility.SizeOf<T>() * 64);
		}
	}

	public unsafe void Dispose()
	{
		Clear();
		if (layers != null)
		{
			AllocatorManager.Free(allocator, layers, 64);
			layers = null;
		}
		if (sameValues != null)
		{
			AllocatorManager.Free(allocator, sameValues, 64);
			sameValues = null;
		}
	}

	public unsafe int CalculateOwnedBytes()
	{
		int num = UnsafeUtility.SizeOf<UnsafeChunkData<T>>();
		if (layers != null)
		{
			num += CollectionHelper.Align(64 * UnsafeUtility.SizeOf<LayerData>(), UnsafeUtility.AlignOf<LayerData>());
			for (int i = 0; i < 64; i++)
			{
				if (layers[i].items != null)
				{
					num += CollectionHelper.Align(1024 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
				}
			}
		}
		if (sameValues != null)
		{
			num += CollectionHelper.Align(64 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
		}
		return num;
	}
}
