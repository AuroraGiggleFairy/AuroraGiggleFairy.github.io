using System;
using System.Collections.Generic;

public class DynamicMeshServerUpdates
{
	public const int DataLayer = 255;

	public const byte EmptyLayer = 128;

	public const int EmptyBlock = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<DynamicMeshServerUpdates> Pool = new Queue<DynamicMeshServerUpdates>(20);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<uint, byte[]> BlockBytes = new Dictionary<uint, byte[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<long, byte[]> TexBytes = new Dictionary<long, byte[]>();

	public int EmptyLayerCount;

	public int DataLayerCount;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int ChunkX { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int ChunkZ { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int StartY { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int EndY { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int UpdateTime { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<byte> Bytes { get; set; }

	public static void AddToPool(DynamicMeshServerUpdates data)
	{
		if (Pool.Count > 40)
		{
			data.Bytes = null;
		}
		else
		{
			Pool.Enqueue(data);
		}
	}

	public static DynamicMeshServerUpdates GetFromPool()
	{
		if (Pool.Count > 0)
		{
			return Pool.Dequeue();
		}
		return new DynamicMeshServerUpdates();
	}

	public long GetKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundChunk(ChunkX)), World.toChunkXZ(DynamicMeshUnity.RoundChunk(ChunkZ)));
	}

	public DynamicMeshServerUpdates()
	{
		Bytes = new List<byte>();
	}

	public void WriteAir(List<byte> tempArray)
	{
		tempArray.Add(0);
	}

	public void WriteLayer(List<byte> tempArray)
	{
		DataLayerCount++;
		Bytes.Add(byte.MaxValue);
		Bytes.AddRange(tempArray);
	}

	public void WriteEmptyLayer()
	{
		EmptyLayerCount++;
		Bytes.Add(128);
	}

	public void WriteBinaryBlock(List<byte> tempArray, BlockValue b, sbyte dens, long tex)
	{
		if (b.isair)
		{
			tempArray.Add(0);
			return;
		}
		if (!BlockBytes.TryGetValue(b.rawData, out var value))
		{
			value = BitConverter.GetBytes(b.type);
			BlockBytes.Add(b.rawData, value);
		}
		tempArray.AddRange(value);
		if (!TexBytes.TryGetValue(tex, out value))
		{
			value = BitConverter.GetBytes(tex);
			TexBytes.Add(tex, value);
		}
		tempArray.AddRange(value);
		tempArray.Add((byte)dens);
	}
}
