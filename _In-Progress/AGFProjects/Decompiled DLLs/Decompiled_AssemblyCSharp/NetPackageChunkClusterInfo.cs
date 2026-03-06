using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunkClusterInfo : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cMinPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cMaxPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInfinite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pos;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageChunkClusterInfo Setup(ChunkCluster _chunkCluster)
	{
		index = _chunkCluster.ClusterIdx;
		name = _chunkCluster.Name;
		cMinPos = _chunkCluster.ChunkMinPos;
		cMaxPos = _chunkCluster.ChunkMaxPos;
		bInfinite = !_chunkCluster.IsFixedSize;
		pos = _chunkCluster.Position;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		index = _br.ReadUInt16();
		name = _br.ReadString();
		cMinPos = new Vector2i(_br.ReadInt32(), _br.ReadInt32());
		cMaxPos = new Vector2i(_br.ReadInt32(), _br.ReadInt32());
		bInfinite = _br.ReadBoolean();
		pos = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((ushort)index);
		_bw.Write(name);
		_bw.Write(cMinPos.x);
		_bw.Write(cMinPos.y);
		_bw.Write(cMaxPos.x);
		_bw.Write(cMaxPos.y);
		_bw.Write(bInfinite);
		StreamUtils.Write(_bw, pos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.ChunkClusterInfo(name, index, bInfinite, cMinPos, cMaxPos, pos);
	}

	public override int GetLength()
	{
		return 40;
	}
}
