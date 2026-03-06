using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRegionMetaData : DynamicMeshServerData
{
	public List<Vector2i> ChunksWithData = new List<Vector2i>();

	public override bool FlushQueue => true;

	public NetPackageRegionMetaData()
	{
	}

	public NetPackageRegionMetaData(DynamicMeshRegion region)
	{
		X = region.WorldPosition.x;
		Z = region.WorldPosition.z;
	}

	public override bool Prechecks()
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Sending region data: " + X + "," + Z + "  Items: " + ChunksWithData.Count + "   length: " + GetLength());
		}
		return true;
	}

	public override int GetLength()
	{
		return 12 + ChunksWithData.Count * 8;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED || DynamicMeshManager.Instance == null)
		{
			return;
		}
		DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(X, Z);
		if (DynamicMeshManager.DoLog)
		{
			Vector3i worldPosition = region.WorldPosition;
			DynamicMeshManager.LogMsg("Recieved Region meta data " + worldPosition.ToString() + " items: " + ChunksWithData.Count);
		}
		foreach (Vector2i chunksWithDatum in ChunksWithData)
		{
			DynamicMeshManager.Instance.AddChunk(DynamicMeshUnity.GetItemKey(chunksWithDatum.x, chunksWithDatum.y), addToThread: false, primary: false, null);
		}
	}

	public override void read(PooledBinaryReader reader)
	{
		X = reader.ReadInt32();
		Z = reader.ReadInt32();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ChunksWithData.Add(new Vector2i(reader.ReadInt32(), reader.ReadInt32()));
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(X);
		writer.Write(Z);
		writer.Write(ChunksWithData.Count);
		for (int i = 0; i < ChunksWithData.Count; i++)
		{
			writer.Write(ChunksWithData[i].x);
			writer.Write(ChunksWithData[i].y);
		}
	}
}
