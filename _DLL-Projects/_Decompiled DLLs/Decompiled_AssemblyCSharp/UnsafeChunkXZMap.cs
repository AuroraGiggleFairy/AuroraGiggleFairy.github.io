using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeChunkXZMap<T> : IDisposable where T : unmanaged
{
	public const int MAP_SIZE = 256;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* map;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;

	public unsafe bool IsCreated => map != null;

	public unsafe UnsafeChunkXZMap(AllocatorManager.AllocatorHandle _allocator)
	{
		allocator = _allocator;
		map = AllocatorManager.Allocate<T>(_allocator, 256);
	}

	public unsafe T Get(int _x, int _z)
	{
		return map[GetMapIndex(_x, _z)];
	}

	public unsafe void Set(int _x, int _z, T value)
	{
		map[GetMapIndex(_x, _z)] = value;
	}

	public unsafe void Clear()
	{
		if (map != null)
		{
			UnsafeUtility.MemClear(map, 256 * UnsafeUtility.SizeOf<T>());
		}
	}

	public unsafe void Dispose()
	{
		if (map != null)
		{
			AllocatorManager.Free(allocator, map, 256);
		}
	}

	public unsafe int CalculateOwnedBytes()
	{
		int num = UnsafeUtility.SizeOf<UnsafeChunkXZMap<T>>();
		if (map != null)
		{
			num += CollectionHelper.Align(256 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
		}
		return num;
	}

	public static int GetMapIndex(int _x, int _z)
	{
		return _x + 16 * _z;
	}

	public static void GetMapCoords(int _index, out int _x, out int _z)
	{
		_z = _index / 16;
		_x = _index % 16;
	}
}
