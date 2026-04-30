using System.Collections.Generic;
using System.Text;
using GamePath;
using UnityEngine;

public class MemoryPools
{
	public const int VMLPoolSize = 1000;

	public static MemoryPooledObject<Chunk> PoolChunks = new MemoryPooledObject<Chunk>(0);

	public static MemoryPooledObject<ChunkBlockLayer> poolCBL = new MemoryPooledObject<ChunkBlockLayer>(0);

	public static MemoryPooledObject<VoxelMeshLayer> poolVML = new MemoryPooledObject<VoxelMeshLayer>(0);

	public static MemoryPooledObject<ChunkGameObjectLayer> poolCGOL = new MemoryPooledObject<ChunkGameObjectLayer>(0);

	public static MemoryPooledObject<PooledMemoryStream> poolMS = new MemoryPooledObject<PooledMemoryStream>(40);

	public static MemoryPooledObject<CBCLayer> poolCBC = new MemoryPooledObject<CBCLayer>(0);

	public static MemoryPooledArray<Vector3> poolVector3 = new MemoryPooledArray<Vector3>();

	public static MemoryPooledArray<Vector4> poolVector4 = new MemoryPooledArray<Vector4>();

	public static MemoryPooledArray<Vector2> poolVector2 = new MemoryPooledArray<Vector2>();

	public static MemoryPooledArray<int> poolInt = new MemoryPooledArray<int>();

	public static MemoryPooledArray<ushort> poolUInt16 = new MemoryPooledArray<ushort>();

	public static MemoryPooledArray<float> poolFloat = new MemoryPooledArray<float>();

	public static MemoryPooledArray<Color> poolColor = new MemoryPooledArray<Color>();

	public static MemoryPooledArray<byte> poolByte = new MemoryPooledArray<byte>();

	public static MemoryPooledObject<PooledExpandableMemoryStream> poolMemoryStream = new MemoryPooledObject<PooledExpandableMemoryStream>(100);

	public static MemoryPooledObject<PooledBinaryReader> poolBinaryReader = new MemoryPooledObject<PooledBinaryReader>(100);

	public static MemoryPooledObject<PooledBinaryWriter> poolBinaryWriter = new MemoryPooledObject<PooledBinaryWriter>(100);

	public static MemoryPooledObject<NameIdMapping> poolNameIdMapping = new MemoryPooledObject<NameIdMapping>(10);

	public static List<byte[]> poolCBLUpper24BitArrCache = new List<byte[]>();

	public static List<byte[]> poolCBLLower8BitArrCache = new List<byte[]>();

	public static DynamicObjectPool<PathPoint> s_pool = new DynamicObjectPool<PathPoint>(64);

	public static void InitStatic(bool _usePools)
	{
		_usePools = true;
		PoolChunks = new MemoryPooledObject<Chunk>(_usePools ? 1000 : 0);
		poolCBL = new MemoryPooledObject<ChunkBlockLayer>(_usePools ? 50000 : 0);
		poolVML = new MemoryPooledObject<VoxelMeshLayer>(_usePools ? 1000 : 0);
		poolCGOL = new MemoryPooledObject<ChunkGameObjectLayer>(_usePools ? 1000 : 0);
		poolMS = new MemoryPooledObject<PooledMemoryStream>(_usePools ? 40 : 0);
		poolCBC = new MemoryPooledObject<CBCLayer>(_usePools ? 50000 : 0);
	}

	public static void Cleanup()
	{
		PoolChunks.Cleanup();
		poolCBL.Cleanup();
		poolVML.Cleanup();
		poolCGOL.Cleanup();
		poolMS.Cleanup();
		poolCBC.Cleanup();
		poolVector3.FreeAll();
		poolVector4.FreeAll();
		poolVector2.FreeAll();
		poolInt.FreeAll();
		poolColor.FreeAll();
		poolByte.FreeAll();
		lock (poolCBLUpper24BitArrCache)
		{
			poolCBLUpper24BitArrCache.Clear();
		}
		lock (poolCBLLower8BitArrCache)
		{
			poolCBLLower8BitArrCache.Clear();
		}
	}

	public static string GetDebugInfo()
	{
		return $"Chunks:{Chunk.InstanceCount}/{PoolChunks.GetPoolSize()} CBL:{ChunkBlockLayer.InstanceCount}/{poolCBL.GetPoolSize()} CBC:{CBCLayer.InstanceCount}/{poolCBC.GetPoolSize()} CGL:{ChunkGameObjectLayer.InstanceCount}/{poolCGOL.GetPoolSize()} VML:{VoxelMeshLayer.InstanceCount}/{poolVML.GetPoolSize()} MS:{PooledMemoryStream.InstanceCount}/{poolMS.GetPoolSize()}";
	}

	public static string GetDebugInfoEx()
	{
		int count = poolVector2.GetCount();
		int count2 = poolVector3.GetCount();
		int count3 = poolVector4.GetCount();
		int count4 = poolInt.GetCount();
		int count5 = poolUInt16.GetCount();
		int count6 = poolFloat.GetCount();
		int count7 = poolColor.GetCount();
		int count8 = poolByte.GetCount();
		int count9 = poolCBLUpper24BitArrCache.Count;
		int count10 = poolCBLLower8BitArrCache.Count;
		long bytes = poolVector2.GetElementsCount() * MemoryTracker.GetSize<Vector2>() + poolVector3.GetElementsCount() * MemoryTracker.GetSize<Vector3>() + poolVector4.GetElementsCount() * MemoryTracker.GetSize<Vector4>() + poolInt.GetElementsCount() * MemoryTracker.GetSize<int>() + poolUInt16.GetElementsCount() * MemoryTracker.GetSize<ushort>() + poolFloat.GetElementsCount() * MemoryTracker.GetSize<float>() + poolColor.GetElementsCount() * MemoryTracker.GetSize<Color>() + poolByte.GetElementsCount() + count9 * 1024 * 3 + count10 * 1024;
		int num = PoolChunks.GetPoolSize() + poolCBL.GetPoolSize() + poolCBC.GetPoolSize() + poolCGOL.GetPoolSize() + poolVML.GetPoolSize() + poolMS.GetPoolSize();
		int num2 = Chunk.InstanceCount + ChunkBlockLayer.InstanceCount + CBCLayer.InstanceCount + ChunkGameObjectLayer.InstanceCount + VoxelMeshLayer.InstanceCount + PooledMemoryStream.InstanceCount;
		return $"V2/V3/V4/C:{count}/{count2}/{count3}/{count7} I/UI/F/B:{count4}/{count5}/{count6}/{count8} 24/8:{count9}/{count10} pools={num} inst={num2} pooled arrays mem={MetricConversion.ToShortestBytesString(bytes)}";
	}

	public static string GetDebugInfoArrays()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendDebugInfoArray(stringBuilder, poolVector2);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolVector3);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolVector4);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolColor);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolInt);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolByte);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolUInt16);
		stringBuilder.AppendLine();
		AppendDebugInfoArray(stringBuilder, poolFloat);
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AppendDebugInfoArray<T>(StringBuilder _builder, MemoryPooledArray<T> _pool) where T : new()
	{
		_builder.AppendLine(typeof(T).Name);
		long num = 0L;
		long num2 = MemoryTracker.GetSize<T>();
		for (int i = 0; i < MemoryPooledArraySizes.poolElements.Length; i++)
		{
			int count = _pool.GetCount(i);
			_builder.Append(MetricConversion.ToShortestBytesString(MemoryPooledArraySizes.poolElements[i] * num2));
			_builder.Append(": ");
			_builder.Append(count);
			if (i < MemoryPooledArraySizes.poolElements.Length - 1)
			{
				_builder.Append(", ");
			}
			num += count * MemoryPooledArraySizes.poolElements[i];
		}
		_builder.AppendLine();
		_builder.Append("Total: ");
		_builder.AppendLine(MetricConversion.ToShortestBytesString(num * num2));
	}
}
