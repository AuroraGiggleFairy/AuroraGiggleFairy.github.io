using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetBlockTexture : NetPackage, IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace blockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte idx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerIdThatChanged;

	public NetPackageSetBlockTexture Setup(Vector3i _blockPos, BlockFace _blockFace, int _idx, int _playerIdThatChanged)
	{
		blockPos = _blockPos;
		blockFace = _blockFace;
		idx = (byte)_idx;
		playerIdThatChanged = _playerIdThatChanged;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		blockPos = StreamUtils.ReadVector3i(_br);
		blockFace = (BlockFace)_br.ReadByte();
		idx = _br.ReadByte();
		playerIdThatChanged = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, blockPos);
		_bw.Write((byte)blockFace);
		_bw.Write(idx);
		_bw.Write(playerIdThatChanged);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.ChunkClusters[0] != null)
		{
			GameManager.Instance.SetBlockTextureClient(blockPos, blockFace, idx);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageSetBlockTexture package = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(blockPos, blockFace, idx, playerIdThatChanged);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, playerIdThatChanged);
		}
	}

	public override int GetLength()
	{
		return 18;
	}

	public void Reset()
	{
	}

	public void Cleanup()
	{
	}

	public static int GetPoolSize()
	{
		return 500;
	}
}
