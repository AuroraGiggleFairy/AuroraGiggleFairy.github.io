using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameEventResponse : NetPackage
{
	public enum ResponseTypes
	{
		Denied,
		Approved,
		TwitchPartyActionApproved,
		TwitchRefundNeeded,
		TwitchSetOwner,
		EntitySpawned,
		EntityDespawned,
		EntityKilled,
		BlocksAdded,
		BlocksRemoved,
		BlockRemoved,
		BlockDamaged,
		ClientSequenceAction,
		Completed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string eventName;

	[PublicizedFrom(EAccessModifier.Private)]
	public ResponseTypes responseType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entitySpawnedID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int targetEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string extraData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tag = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int index = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string actionKey = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDespawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> blockList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public NetPackageGameEventResponse Setup(string _event, int _targetEntityID, string _extraData, string _tag, ResponseTypes _responseType, int _entitySpawnedID = -1, int _index = -1, bool _isDespawn = false, string _actionKey = "")
	{
		eventName = _event;
		targetEntityID = _targetEntityID;
		extraData = _extraData;
		tag = _tag;
		responseType = _responseType;
		entitySpawnedID = _entitySpawnedID;
		index = _index;
		isDespawn = _isDespawn;
		actionKey = _actionKey;
		return this;
	}

	public NetPackageGameEventResponse Setup(ResponseTypes _responseType, int _entitySpawnedID = -1, int _index = -1, string _tag = "", bool _isDespawn = false)
	{
		eventName = "";
		targetEntityID = -1;
		extraData = "";
		tag = _tag;
		responseType = _responseType;
		entitySpawnedID = _entitySpawnedID;
		index = _index;
		isDespawn = _isDespawn;
		return this;
	}

	public NetPackageGameEventResponse Setup(ResponseTypes _responseType, string _event, int _blockGroupID, List<Vector3i> _blockList, string _tag = "", bool _isDespawn = false)
	{
		eventName = _event;
		targetEntityID = -1;
		extraData = "";
		tag = _tag;
		index = _blockGroupID;
		responseType = _responseType;
		blockList = _blockList;
		isDespawn = _isDespawn;
		return this;
	}

	public NetPackageGameEventResponse Setup(ResponseTypes _responseType, Vector3i _blockPos)
	{
		eventName = "";
		targetEntityID = -1;
		extraData = "";
		tag = "";
		responseType = _responseType;
		blockPos = _blockPos;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		eventName = _br.ReadString();
		targetEntityID = _br.ReadInt32();
		extraData = _br.ReadString();
		tag = _br.ReadString();
		responseType = (ResponseTypes)_br.ReadByte();
		entitySpawnedID = _br.ReadInt32();
		if (responseType == ResponseTypes.ClientSequenceAction)
		{
			actionKey = _br.ReadString();
		}
		else if (responseType == ResponseTypes.BlocksAdded)
		{
			index = _br.ReadInt32();
			int num = _br.ReadInt32();
			blockList = new List<Vector3i>();
			for (int i = 0; i < num; i++)
			{
				blockList.Add(StreamUtils.ReadVector3i(_br));
			}
		}
		else if (responseType == ResponseTypes.BlocksRemoved)
		{
			index = _br.ReadInt32();
			isDespawn = _br.ReadBoolean();
		}
		else if (responseType == ResponseTypes.BlocksRemoved || responseType == ResponseTypes.BlockDamaged)
		{
			blockPos = StreamUtils.ReadVector3i(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(eventName);
		_bw.Write(targetEntityID);
		_bw.Write(extraData);
		_bw.Write(tag);
		_bw.Write((byte)responseType);
		_bw.Write(entitySpawnedID);
		if (responseType == ResponseTypes.ClientSequenceAction)
		{
			_bw.Write(actionKey);
		}
		else if (responseType == ResponseTypes.BlocksAdded)
		{
			_bw.Write(index);
			if (blockList == null)
			{
				_bw.Write(0);
				return;
			}
			_bw.Write(blockList.Count);
			for (int i = 0; i < blockList.Count; i++)
			{
				StreamUtils.Write(_bw, blockList[i]);
			}
		}
		else if (responseType == ResponseTypes.BlocksRemoved)
		{
			_bw.Write(index);
			_bw.Write(isDespawn);
		}
		else if (responseType == ResponseTypes.BlocksRemoved || responseType == ResponseTypes.BlockDamaged)
		{
			StreamUtils.Write(_bw, blockPos);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			switch (responseType)
			{
			case ResponseTypes.Denied:
				GameEventManager.Current.HandleGameEventDenied(eventName, targetEntityID, extraData, tag);
				break;
			case ResponseTypes.Approved:
				GameEventManager.Current.HandleGameEventApproved(eventName, targetEntityID, extraData, tag);
				break;
			case ResponseTypes.TwitchPartyActionApproved:
				GameEventManager.Current.HandleTwitchPartyGameEventApproved(eventName, targetEntityID, extraData, tag);
				break;
			case ResponseTypes.TwitchRefundNeeded:
				GameEventManager.Current.HandleTwitchRefundNeeded(eventName, targetEntityID, extraData, tag);
				break;
			case ResponseTypes.TwitchSetOwner:
				GameEventManager.Current.HandleGameEntitySpawned(eventName, entitySpawnedID, tag);
				GameEventManager.Current.HandleTwitchSetOwner(targetEntityID, entitySpawnedID, extraData);
				break;
			case ResponseTypes.EntitySpawned:
				GameEventManager.Current.HandleGameEntitySpawned(eventName, entitySpawnedID, tag);
				break;
			case ResponseTypes.EntityDespawned:
				GameEventManager.Current.HandleGameEntityDespawned(entitySpawnedID);
				break;
			case ResponseTypes.EntityKilled:
				GameEventManager.Current.HandleGameEntityKilled(entitySpawnedID);
				break;
			case ResponseTypes.BlocksAdded:
				GameEventManager.Current.HandleGameBlocksAdded(eventName, index, blockList, tag);
				break;
			case ResponseTypes.BlocksRemoved:
				GameEventManager.Current.HandleGameBlocksRemoved(index, isDespawn);
				break;
			case ResponseTypes.BlockRemoved:
				GameEventManager.Current.HandleGameBlockRemoved(blockPos);
				break;
			case ResponseTypes.BlockDamaged:
				GameEventManager.Current.SendBlockDamageUpdate(blockPos);
				break;
			case ResponseTypes.ClientSequenceAction:
				GameEventManager.Current.HandleGameEventSequenceItemForClient(eventName, actionKey);
				break;
			case ResponseTypes.Completed:
				GameEventManager.Current.HandleGameEventCompleted(eventName, targetEntityID, extraData, tag);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
