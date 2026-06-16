using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestObjectiveUpdate : NetPackage
{
	public enum QuestObjectiveEventTypes
	{
		TreasureRadiusBreak,
		TreasureComplete,
		BlockActivated
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestObjectiveEventTypes eventType;

	public NetPackageQuestObjectiveUpdate Setup(QuestObjectiveEventTypes _eventType, int _entityID, int _questCode)
	{
		senderEntityID = _entityID;
		questCode = _questCode;
		eventType = _eventType;
		blockPos = Vector3i.zero;
		return this;
	}

	public NetPackageQuestObjectiveUpdate Setup(QuestObjectiveEventTypes _eventType, int _entityID, int _questCode, Vector3i _blockPos)
	{
		senderEntityID = _entityID;
		questCode = _questCode;
		eventType = _eventType;
		blockPos = _blockPos;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		senderEntityID = _reader.ReadInt32();
		questCode = _reader.ReadInt32();
		eventType = (QuestObjectiveEventTypes)_reader.ReadByte();
		blockPos = StreamUtils.ReadVector3i(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(senderEntityID);
		_writer.Write(questCode);
		_writer.Write((byte)eventType);
		StreamUtils.Write(_writer, blockPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		switch (eventType)
		{
		case QuestObjectiveEventTypes.TreasureRadiusBreak:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer3 = _world.GetEntity(senderEntityID) as EntityPlayer;
				if (entityPlayer3 == null || entityPlayer3.Party == null)
				{
					break;
				}
				for (int j = 0; j < entityPlayer3.Party.MemberList.Count; j++)
				{
					EntityPlayer entityPlayer4 = entityPlayer3.Party.MemberList[j];
					if (entityPlayer4 != entityPlayer3)
					{
						if (entityPlayer4 is EntityPlayerLocal)
						{
							HandlePlayer(_world, entityPlayer4 as EntityPlayerLocal);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(eventType, senderEntityID, questCode), _onlyClientsAttachedToAnEntity: false, entityPlayer4.entityId);
						}
					}
				}
			}
			else
			{
				EntityPlayerLocal primaryPlayer2 = _world.GetPrimaryPlayer();
				HandlePlayer(_world, primaryPlayer2);
			}
			break;
		case QuestObjectiveEventTypes.TreasureComplete:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.FinishTreasureQuest(questCode, _world.GetEntity(senderEntityID) as EntityPlayer);
			}
			break;
		case QuestObjectiveEventTypes.BlockActivated:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer = _world.GetEntity(senderEntityID) as EntityPlayer;
				if (entityPlayer == null || entityPlayer.Party == null)
				{
					break;
				}
				for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
				{
					EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
					if (entityPlayer2 != entityPlayer)
					{
						if (entityPlayer2 is EntityPlayerLocal)
						{
							HandlePlayer(_world, entityPlayer2 as EntityPlayerLocal);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(eventType, senderEntityID, questCode, blockPos), _onlyClientsAttachedToAnEntity: false, entityPlayer2.entityId);
						}
					}
				}
			}
			else
			{
				EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
				HandlePlayer(_world, primaryPlayer);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePlayer(World _world, EntityPlayerLocal localPlayer)
	{
		if (localPlayer == null)
		{
			Log.Warning(string.Format("HandlePlayer {0} {1} with no active local player", "NetPackageQuestObjectiveUpdate", eventType));
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(senderEntityID) as EntityPlayer;
		Quest quest = localPlayer.QuestJournal.FindActiveQuest(questCode);
		if (quest == null || !(entityPlayer.GetDistance(localPlayer) < 15f))
		{
			return;
		}
		switch (eventType)
		{
		case QuestObjectiveEventTypes.TreasureRadiusBreak:
		{
			for (int j = 0; j < quest.Objectives.Count; j++)
			{
				if (!quest.Objectives[j].Complete && quest.Objectives[j] is ObjectiveTreasureChest)
				{
					(quest.Objectives[j] as ObjectiveTreasureChest).AddToDestroyCount();
					break;
				}
			}
			break;
		}
		case QuestObjectiveEventTypes.BlockActivated:
		{
			for (int i = 0; i < quest.Objectives.Count; i++)
			{
				if (!quest.Objectives[i].Complete && quest.Objectives[i] is ObjectivePOIBlockActivate)
				{
					(quest.Objectives[i] as ObjectivePOIBlockActivate).AddActivatedBlock(blockPos);
					break;
				}
			}
			break;
		}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
