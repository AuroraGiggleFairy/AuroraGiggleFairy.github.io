using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetBlock : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> blockChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs persistentPlayerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localPlayerThatChanged;

	public NetPackageSetBlock Setup(PersistentPlayerData _persistentPlayerData, List<BlockChangeInfo> _blockChanges, int _localPlayerThatChanged)
	{
		persistentPlayerId = _persistentPlayerData?.PrimaryId;
		blockChanges = _blockChanges;
		localPlayerThatChanged = _localPlayerThatChanged;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		persistentPlayerId = PlatformUserIdentifierAbs.FromStream(_br);
		int num = _br.ReadInt16();
		blockChanges = new List<BlockChangeInfo>();
		for (int i = 0; i < num; i++)
		{
			BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
			blockChangeInfo.Read(_br);
			blockChanges.Add(blockChangeInfo);
		}
		localPlayerThatChanged = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		persistentPlayerId.ToStream(_bw);
		int count = blockChanges.Count;
		_bw.Write((short)count);
		for (int i = 0; i < count; i++)
		{
			blockChanges[i].Write(_bw);
		}
		_bw.Write(localPlayerThatChanged);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!ValidUserIdForSender(persistentPlayerId) || !ValidEntityIdForSender(localPlayerThatChanged))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_callbacks.SetBlocksOnClients(localPlayerThatChanged, this);
		}
		if (_world == null || _world.ChunkClusters[0] == null)
		{
			return;
		}
		if (DynamicMeshManager.CONTENT_ENABLED)
		{
			foreach (BlockChangeInfo blockChange in blockChanges)
			{
				DynamicMeshManager.ChunkChanged(blockChange.pos, -1, blockChange.blockValue.type);
			}
		}
		_callbacks.ChangeBlocks(persistentPlayerId, blockChanges);
	}

	public override int GetLength()
	{
		return blockChanges.Count * 16;
	}
}
