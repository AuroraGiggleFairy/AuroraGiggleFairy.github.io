using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBossEvent : NetPackage
{
	public enum BossEventTypes
	{
		RequestGroups,
		AddGroup,
		UpdateGroupType,
		RemoveGroup,
		RemoveMinion,
		RequestStats
	}

	public int bossGroupID;

	public int entityID;

	public List<int> minionIDs;

	public string bossIcon1;

	public BossGroup.BossGroupTypes bossGroupType;

	public BossEventTypes eventType;

	public NetPackageBossEvent Setup(BossEventTypes _eventType, int _bossGroupID)
	{
		bossGroupID = _bossGroupID;
		entityID = -1;
		minionIDs = null;
		eventType = _eventType;
		bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(BossEventTypes _eventType, int _bossGroupID, BossGroup.BossGroupTypes _bossGroupType)
	{
		bossGroupID = _bossGroupID;
		bossGroupType = _bossGroupType;
		entityID = -1;
		minionIDs = null;
		eventType = _eventType;
		bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(BossEventTypes _eventType, int _bossGroupID, int _entityID)
	{
		bossGroupID = _bossGroupID;
		entityID = _entityID;
		minionIDs = null;
		eventType = _eventType;
		bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(BossEventTypes _eventType, int _bossGroupID, BossGroup.BossGroupTypes _bossGroupType, int _bossID, List<int> _minionIDs, string _bossIcon1)
	{
		bossGroupID = _bossGroupID;
		bossGroupType = _bossGroupType;
		entityID = _bossID;
		minionIDs = _minionIDs;
		eventType = _eventType;
		bossIcon1 = _bossIcon1;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		bossGroupID = _reader.ReadInt32();
		eventType = (BossEventTypes)_reader.ReadByte();
		bossGroupType = (BossGroup.BossGroupTypes)_reader.ReadByte();
		entityID = _reader.ReadInt32();
		bossIcon1 = _reader.ReadString();
		if (eventType == BossEventTypes.AddGroup)
		{
			int num = _reader.ReadInt32();
			minionIDs = new List<int>();
			for (int i = 0; i < num; i++)
			{
				minionIDs.Add(_reader.ReadInt32());
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(bossGroupID);
		_writer.Write((byte)eventType);
		_writer.Write((byte)bossGroupType);
		_writer.Write(entityID);
		_writer.Write(bossIcon1);
		if (eventType == BossEventTypes.AddGroup)
		{
			_writer.Write(minionIDs.Count);
			for (int i = 0; i < minionIDs.Count; i++)
			{
				_writer.Write(minionIDs[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			switch (eventType)
			{
			case BossEventTypes.RequestGroups:
				GameEventManager.Current.SendBossGroups(base.Sender.entityId);
				break;
			case BossEventTypes.AddGroup:
				GameEventManager.Current.SetupClientBossGroup(bossGroupID, bossGroupType, entityID, minionIDs, bossIcon1);
				break;
			case BossEventTypes.UpdateGroupType:
				GameEventManager.Current.UpdateBossGroupType(bossGroupID, bossGroupType);
				break;
			case BossEventTypes.RemoveGroup:
				GameEventManager.Current.RemoveClientBossGroup(bossGroupID);
				break;
			case BossEventTypes.RemoveMinion:
				GameEventManager.Current.RemoveEntityFromBossGroup(bossGroupID, entityID);
				break;
			case BossEventTypes.RequestStats:
				GameEventManager.Current.RequestBossGroupStatRefresh(bossGroupID, base.Sender.entityId);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
