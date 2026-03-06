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

	[PublicizedFrom(EAccessModifier.Private)]
	public byte channel;

	public NetPackageSetBlockTexture Setup(Vector3i _blockPos, BlockFace _blockFace, int _idx, int _playerIdThatChanged, byte _channel)
	{
		blockPos = _blockPos;
		blockFace = _blockFace;
		idx = (byte)_idx;
		playerIdThatChanged = _playerIdThatChanged;
		channel = _channel;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		blockPos = StreamUtils.ReadVector3i(_br);
		blockFace = (BlockFace)_br.ReadByte();
		idx = _br.ReadByte();
		playerIdThatChanged = _br.ReadInt32();
		channel = _br.ReadByte();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, blockPos);
		_bw.Write((byte)blockFace);
		_bw.Write(idx);
		_bw.Write(playerIdThatChanged);
		_bw.Write(channel);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.ChunkClusters[0] != null)
		{
			GameManager.Instance.SetBlockTextureClient(blockPos, blockFace, idx, channel);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageSetBlockTexture package = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(blockPos, blockFace, idx, playerIdThatChanged, channel);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, -1, playerIdThatChanged);
		}
	}

	public override int GetLength()
	{
		return 19;
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
