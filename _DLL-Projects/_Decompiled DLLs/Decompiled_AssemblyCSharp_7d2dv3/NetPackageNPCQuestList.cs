using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageNPCQuestList : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum NPCQuestEventTypes
	{
		FetchList,
		RemoveQuest,
		ResetQuests,
		AddUsedPOI,
		ClearUsedPOI
	}

	public struct QuestPacketEntry
	{
		public string QuestID;

		public Vector3 QuestLocation;

		public Vector3 QuestSize;

		public Vector3 TraderPos;

		public string POIName;

		public void read(BinaryReader _reader)
		{
			QuestID = _reader.ReadString();
			QuestLocation = StreamUtils.ReadVector3(_reader);
			QuestSize = StreamUtils.ReadVector3(_reader);
			POIName = _reader.ReadString();
			TraderPos = StreamUtils.ReadVector3(_reader);
		}

		public void write(BinaryWriter _writer)
		{
			_writer.Write(QuestID);
			StreamUtils.Write(_writer, QuestLocation);
			StreamUtils.Write(_writer, QuestSize);
			_writer.Write(POIName);
			StreamUtils.Write(_writer, TraderPos);
		}
	}

	public int npcEntityID;

	public int playerEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public NPCQuestEventTypes eventType;

	public QuestPacketEntry[] questPacketEntries;

	public int tierLevel = -1;

	public byte removeIndex;

	public Vector2 questGiverPos;

	public Vector2 prefabPos;

	public NetPackageNPCQuestList Setup(int _npcEntityID, int _playerEntityID)
	{
		npcEntityID = _npcEntityID;
		playerEntityID = _playerEntityID;
		eventType = NPCQuestEventTypes.ResetQuests;
		return this;
	}

	public NetPackageNPCQuestList Setup(int _playerEntityID, Vector2 _questGiverPos, int _tierLevel, Vector2 _prefabPos)
	{
		playerEntityID = _playerEntityID;
		tierLevel = _tierLevel;
		questGiverPos = _questGiverPos;
		prefabPos = _prefabPos;
		eventType = NPCQuestEventTypes.AddUsedPOI;
		return this;
	}

	public NetPackageNPCQuestList SetupClear(int _playerEntityID, Vector2 _questGiverPos, int _tierLevel)
	{
		playerEntityID = _playerEntityID;
		tierLevel = _tierLevel;
		questGiverPos = _questGiverPos;
		eventType = NPCQuestEventTypes.ClearUsedPOI;
		return this;
	}

	public NetPackageNPCQuestList Setup(int _npcEntityID, int _playerEntityID, int _tierLevel)
	{
		npcEntityID = _npcEntityID;
		playerEntityID = _playerEntityID;
		tierLevel = _tierLevel;
		eventType = NPCQuestEventTypes.FetchList;
		return this;
	}

	public NetPackageNPCQuestList Setup(int _npcEntityID, int _playerEntityID, int _tierLevel, byte _removeIndex)
	{
		npcEntityID = _npcEntityID;
		playerEntityID = _playerEntityID;
		tierLevel = _tierLevel;
		eventType = NPCQuestEventTypes.RemoveQuest;
		removeIndex = _removeIndex;
		return this;
	}

	public NetPackageNPCQuestList Setup(int _npcEntityID, int _playerEntityID, QuestPacketEntry[] _questPacketEntries)
	{
		npcEntityID = _npcEntityID;
		playerEntityID = _playerEntityID;
		questPacketEntries = _questPacketEntries;
		eventType = NPCQuestEventTypes.FetchList;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		npcEntityID = _reader.ReadInt32();
		playerEntityID = _reader.ReadInt32();
		eventType = (NPCQuestEventTypes)_reader.ReadByte();
		if (eventType == NPCQuestEventTypes.FetchList)
		{
			tierLevel = _reader.ReadInt32();
			int num = _reader.ReadInt32();
			if (num > 0)
			{
				questPacketEntries = new QuestPacketEntry[num];
				for (int i = 0; i < num; i++)
				{
					questPacketEntries[i].read(_reader);
				}
			}
			else
			{
				questPacketEntries = null;
			}
		}
		else if (eventType == NPCQuestEventTypes.RemoveQuest)
		{
			tierLevel = _reader.ReadInt32();
			removeIndex = _reader.ReadByte();
		}
		else if (eventType == NPCQuestEventTypes.AddUsedPOI)
		{
			tierLevel = _reader.ReadInt32();
			questGiverPos = StreamUtils.ReadVector2(_reader);
			prefabPos = StreamUtils.ReadVector2(_reader);
		}
		else if (eventType == NPCQuestEventTypes.ClearUsedPOI)
		{
			tierLevel = _reader.ReadInt32();
			questGiverPos = StreamUtils.ReadVector2(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(npcEntityID);
		_writer.Write(playerEntityID);
		_writer.Write((byte)eventType);
		if (eventType == NPCQuestEventTypes.FetchList)
		{
			_writer.Write(tierLevel);
			if (questPacketEntries != null)
			{
				_writer.Write(questPacketEntries.Length);
				for (int i = 0; i < questPacketEntries.Length; i++)
				{
					questPacketEntries[i].write(_writer);
				}
			}
			else
			{
				_writer.Write(0);
			}
		}
		else if (eventType == NPCQuestEventTypes.RemoveQuest)
		{
			_writer.Write(tierLevel);
			_writer.Write(removeIndex);
		}
		else if (eventType == NPCQuestEventTypes.AddUsedPOI)
		{
			_writer.Write(tierLevel);
			StreamUtils.Write(_writer, questGiverPos);
			StreamUtils.Write(_writer, prefabPos);
		}
		else if (eventType == NPCQuestEventTypes.ClearUsedPOI)
		{
			_writer.Write(tierLevel);
			StreamUtils.Write(_writer, questGiverPos);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityPlayer entityPlayer = _world.GetEntity(playerEntityID) as EntityPlayer;
			if (eventType == NPCQuestEventTypes.AddUsedPOI)
			{
				entityPlayer.QuestJournal.AddPOIToTraderData(tierLevel, questGiverPos, prefabPos);
				return;
			}
			EntityTrader entityTrader = _world.GetEntity(npcEntityID) as EntityTrader;
			entityTrader.activeQuests = QuestEventManager.Current.GetQuestList(_world, npcEntityID, playerEntityID);
			if (entityTrader.activeQuests == null)
			{
				entityTrader.activeQuests = entityTrader.PopulateActiveQuests(entityPlayer, tierLevel);
			}
			QuestEventManager.Current.SetupQuestList(entityTrader, playerEntityID, entityTrader.activeQuests);
			if (eventType == NPCQuestEventTypes.FetchList)
			{
				SendQuestPacketsToPlayer(entityTrader, playerEntityID);
			}
			else if (eventType == NPCQuestEventTypes.RemoveQuest)
			{
				List<Quest> questList = QuestEventManager.Current.GetQuestList(_world, npcEntityID, playerEntityID);
				int num = 0;
				for (int i = 0; i < questList.Count; i++)
				{
					if (questList[i].QuestClass.DifficultyTier == tierLevel)
					{
						if (num == removeIndex)
						{
							questList.RemoveAt(i);
							break;
						}
						num++;
					}
				}
				QuestEventManager.Current.SetupQuestList(entityTrader, playerEntityID, questList);
			}
			else
			{
				QuestEventManager.Current.ClearQuestList(npcEntityID);
				Log.Out("Quests Reset for NPC: " + npcEntityID + " by Player: " + playerEntityID + ".");
			}
		}
		else
		{
			EntityPlayer entityPlayer2 = _world.GetEntity(playerEntityID) as EntityPlayer;
			if (eventType == NPCQuestEventTypes.ClearUsedPOI)
			{
				entityPlayer2.QuestJournal.ClearTraderDataTier(tierLevel, questGiverPos);
			}
			else
			{
				(_world.GetEntity(npcEntityID) as EntityTrader).SetActiveQuests(entityPlayer2, questPacketEntries);
			}
		}
	}

	public static void SendQuestPacketsToPlayer(EntityTrader npc, int playerEntityID)
	{
		if (npc.activeQuests != null)
		{
			int count = npc.activeQuests.Count;
			QuestPacketEntry[] array = new QuestPacketEntry[count];
			for (int i = 0; i < count; i++)
			{
				Quest quest = npc.activeQuests[i];
				Vector3 traderPos = ((npc.traderArea != null) ? ((Vector3)npc.traderArea.Position) : npc.position);
				array[i].QuestID = quest.ID;
				array[i].QuestLocation = quest.GetLocation();
				array[i].QuestSize = quest.GetLocationSize();
				array[i].POIName = ((quest.QuestPrefab != null) ? quest.QuestPrefab.location.Name : "UNNAMED");
				array[i].TraderPos = traderPos;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(npc.entityId, playerEntityID, array), _onlyClientsAttachedToAnEntity: false, playerEntityID);
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
