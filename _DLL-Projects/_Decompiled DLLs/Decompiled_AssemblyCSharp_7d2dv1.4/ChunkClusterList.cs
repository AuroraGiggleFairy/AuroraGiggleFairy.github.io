using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ChunkClusterList
{
	public ChunkCluster Cluster0;

	public List<Dictionary<string, int>> LayerMappingTable = new List<Dictionary<string, int>>();

	public ChunkCluster this[int _idx]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Cluster0;
		}
	}

	public int Count
	{
		get
		{
			if (Cluster0 == null)
			{
				return 0;
			}
			return 1;
		}
	}

	public ChunkClusterList()
	{
		AddLayerMappingTable(0, new Dictionary<string, int>
		{
			{ "terraincollision", 16 },
			{ "nocollision", 14 },
			{ "grass", 18 },
			{ "Glass", 30 },
			{ "water", 4 },
			{ "terrain", 28 }
		});
	}

	public void AddFixed(ChunkCluster _cc, int _index)
	{
		Cluster0 = _cc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLayerMappingTable(int _id, Dictionary<string, int> _table)
	{
		for (int i = LayerMappingTable.Count - 1; i < _id; i++)
		{
			LayerMappingTable.Add(null);
		}
		LayerMappingTable[_id] = _table;
	}

	public void Cleanup()
	{
		if (Cluster0 != null)
		{
			Cluster0.Cleanup();
			Cluster0 = null;
		}
	}
}
