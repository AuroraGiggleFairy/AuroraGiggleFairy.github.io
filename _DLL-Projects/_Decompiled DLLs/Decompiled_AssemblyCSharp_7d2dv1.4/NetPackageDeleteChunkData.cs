using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDeleteChunkData : NetPackage
{
	public List<long> chunkKeys = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageDeleteChunkData Setup(ICollection<long> _chunkKeys)
	{
		chunkKeys.Clear();
		chunkKeys.AddRange(_chunkKeys);
		length = 4 + 8 * chunkKeys.Count;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		int num = _br.ReadInt32();
		chunkKeys = new List<long>();
		for (int i = 0; i < num; i++)
		{
			chunkKeys.Add(_br.ReadInt64());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(chunkKeys.Count);
		for (int i = 0; i < chunkKeys.Count; i++)
		{
			_bw.Write(chunkKeys[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			DynamicMeshUnity.DeleteDynamicMeshData(chunkKeys);
			WaterSimulationNative.Instance.changeApplier.DiscardChangesForChunks(chunkKeys);
			MultiBlockManager.Instance.CullChunklessDataOnClient(chunkKeys);
		}
	}

	public override int GetLength()
	{
		return length;
	}
}
