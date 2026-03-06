using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestEvent : NetPackage
{
	public enum QuestEventTypes
	{
		TryRallyMarker,
		ConfirmRallyMarker,
		RallyMarkerActivated,
		RallyMarkerLocked,
		RallyMarker_PlayerLocked,
		RallyMarker_BedrollLocked,
		RallyMarker_LandClaimLocked,
		LockPOI,
		UnlockPOI,
		ClearSleeper,
		ShowSleeperVolume,
		HideSleeperVolume,
		SetupFetch,
		SetupRestorePower,
		FinishManagedQuest,
		POILocked,
		ResetTraderQuests
	}

	public int entityID;

	public Vector3 prefabPos;

	public FastTags<TagGroup.Global> questTags;

	public QuestEventTypes eventType;

	public ObjectiveFetchFromContainer.FetchModeTypes FetchModeType;

	public bool SubscribeTo;

	public int PartyCount;

	public int questCode;

	public int factionPointOverride;

	public string blockIndex = "";

	public string eventName = "";

	public string questID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong extraData;

	public List<Vector3i> activateList;

	public int[] SharedWithList;

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID)
	{
		eventType = _eventType;
		entityID = _entityID;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, int _traderID, int _overrideFactionPoints)
	{
		eventType = _eventType;
		entityID = _entityID;
		questCode = _traderID;
		factionPointOverride = _overrideFactionPoints;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos, int _questCode)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		questCode = _questCode;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos, int _questCode, ulong _extraData)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		questCode = _questCode;
		extraData = _extraData;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, string _questID, FastTags<TagGroup.Global> _questTags, Vector3 _prefabPos, int[] _sharedWithList)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		questTags = _questTags;
		questID = _questID;
		SharedWithList = _sharedWithList;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos, ObjectiveFetchFromContainer.FetchModeTypes _fetchModeType)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		FetchModeType = _fetchModeType;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos, ObjectiveFetchFromContainer.FetchModeTypes _fetchModeType, int[] _sharedWithList)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		FetchModeType = _fetchModeType;
		SharedWithList = _sharedWithList;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, int _questCode, string _completeEvent, Vector3 _prefabPos, string _blockIndex, int[] _sharedWithList)
	{
		entityID = _entityID;
		questCode = _questCode;
		eventName = _completeEvent;
		prefabPos = _prefabPos;
		eventType = _eventType;
		blockIndex = _blockIndex;
		SharedWithList = _sharedWithList;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, Vector3 _prefabPos, bool _subscribeTo)
	{
		entityID = _entityID;
		prefabPos = _prefabPos;
		eventType = _eventType;
		SubscribeTo = _subscribeTo;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, int _questCode, string _completeEvent, Vector3 _prefabPos, List<Vector3i> _activateList)
	{
		entityID = _entityID;
		questCode = _questCode;
		eventName = _completeEvent;
		prefabPos = _prefabPos;
		eventType = _eventType;
		activateList = _activateList;
		return this;
	}

	public NetPackageQuestEvent Setup(QuestEventTypes _eventType, int _entityID, int _questCode, string _questID, Vector3 _prefabPos, int[] _sharedWithList)
	{
		entityID = _entityID;
		questCode = _questCode;
		prefabPos = _prefabPos;
		eventType = _eventType;
		questID = _questID;
		SharedWithList = _sharedWithList;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityID = _reader.ReadInt32();
		prefabPos = StreamUtils.ReadVector3(_reader);
		eventType = (QuestEventTypes)_reader.ReadByte();
		questTags = FastTags<TagGroup.Global>.Parse(_reader.ReadString());
		questCode = _reader.ReadInt32();
		switch (eventType)
		{
		case QuestEventTypes.ClearSleeper:
			SubscribeTo = _reader.ReadBoolean();
			break;
		case QuestEventTypes.SetupFetch:
		{
			FetchModeType = (ObjectiveFetchFromContainer.FetchModeTypes)_reader.ReadByte();
			int num3 = _reader.ReadByte();
			if (num3 > 0)
			{
				SharedWithList = new int[num3];
				for (int l = 0; l < num3; l++)
				{
					SharedWithList[l] = _reader.ReadInt32();
				}
			}
			else
			{
				SharedWithList = null;
			}
			break;
		}
		case QuestEventTypes.SetupRestorePower:
		{
			blockIndex = _reader.ReadString();
			eventName = _reader.ReadString();
			int num2 = _reader.ReadByte();
			if (num2 > 0)
			{
				SharedWithList = new int[num2];
				for (int j = 0; j < num2; j++)
				{
					SharedWithList[j] = _reader.ReadInt32();
				}
			}
			else
			{
				SharedWithList = null;
			}
			num2 = _reader.ReadByte();
			activateList = new List<Vector3i>();
			if (num2 > 0)
			{
				for (int k = 0; k < num2; k++)
				{
					activateList.Add(StreamUtils.ReadVector3i(_reader));
				}
			}
			break;
		}
		case QuestEventTypes.RallyMarkerLocked:
			extraData = _reader.ReadUInt64();
			break;
		case QuestEventTypes.LockPOI:
		{
			questID = _reader.ReadString();
			int num = _reader.ReadByte();
			if (num > 0)
			{
				SharedWithList = new int[num];
				for (int i = 0; i < num; i++)
				{
					SharedWithList[i] = _reader.ReadInt32();
				}
			}
			else
			{
				SharedWithList = null;
			}
			break;
		}
		case QuestEventTypes.ResetTraderQuests:
			factionPointOverride = _reader.ReadInt32();
			break;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityID);
		StreamUtils.Write(_writer, prefabPos);
		_writer.Write((byte)eventType);
		_writer.Write(questTags.ToString());
		_writer.Write(questCode);
		switch (eventType)
		{
		case QuestEventTypes.ClearSleeper:
			_writer.Write(SubscribeTo);
			break;
		case QuestEventTypes.SetupFetch:
		{
			_writer.Write((byte)FetchModeType);
			if (SharedWithList == null)
			{
				_writer.Write((byte)0);
				break;
			}
			_writer.Write((byte)SharedWithList.Length);
			for (int l = 0; l < SharedWithList.Length; l++)
			{
				_writer.Write(SharedWithList[l]);
			}
			break;
		}
		case QuestEventTypes.SetupRestorePower:
		{
			_writer.Write(blockIndex);
			_writer.Write(eventName);
			if (SharedWithList == null)
			{
				_writer.Write((byte)0);
			}
			else
			{
				_writer.Write((byte)SharedWithList.Length);
				for (int j = 0; j < SharedWithList.Length; j++)
				{
					_writer.Write(SharedWithList[j]);
				}
			}
			if (activateList == null)
			{
				_writer.Write((byte)0);
				break;
			}
			_writer.Write((byte)activateList.Count);
			for (int k = 0; k < activateList.Count; k++)
			{
				StreamUtils.Write(_writer, activateList[k]);
			}
			break;
		}
		case QuestEventTypes.RallyMarkerLocked:
			_writer.Write(extraData);
			break;
		case QuestEventTypes.LockPOI:
		{
			_writer.Write(questID);
			if (SharedWithList == null)
			{
				_writer.Write((byte)0);
				break;
			}
			_writer.Write((byte)SharedWithList.Length);
			for (int i = 0; i < SharedWithList.Length; i++)
			{
				_writer.Write(SharedWithList[i]);
			}
			break;
		}
		case QuestEventTypes.ResetTraderQuests:
			_writer.Write(factionPointOverride);
			break;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		switch (eventType)
		{
		case QuestEventTypes.TryRallyMarker:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Vector2 vector = new Vector2(prefabPos.x, prefabPos.z);
				QuestEventTypes questEventTypes = QuestEventTypes.RallyMarkerActivated;
				ulong num;
				switch (QuestEventManager.Current.CheckForPOILockouts(entityID, vector, out num))
				{
				case QuestEventManager.POILockoutReasonTypes.Bedroll:
					questEventTypes = QuestEventTypes.RallyMarker_BedrollLocked;
					break;
				case QuestEventManager.POILockoutReasonTypes.LandClaim:
					questEventTypes = QuestEventTypes.RallyMarker_LandClaimLocked;
					break;
				case QuestEventManager.POILockoutReasonTypes.PlayerInside:
					questEventTypes = QuestEventTypes.RallyMarker_PlayerLocked;
					break;
				case QuestEventManager.POILockoutReasonTypes.QuestLock:
					questEventTypes = QuestEventTypes.RallyMarkerLocked;
					break;
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(questEventTypes, entityID, prefabPos, questCode, num));
			}
			break;
		case QuestEventTypes.RallyMarkerActivated:
		{
			EntityPlayer entityPlayer7 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer7 != null)
			{
				entityPlayer7.QuestJournal.HandleRallyMarkerActivation(questCode, prefabPos, rallyMarkerActivated: true, QuestEventManager.POILockoutReasonTypes.None, 0uL);
			}
			break;
		}
		case QuestEventTypes.RallyMarkerLocked:
		{
			EntityPlayer entityPlayer3 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer3 != null)
			{
				entityPlayer3.QuestJournal.HandleRallyMarkerActivation(questCode, prefabPos, rallyMarkerActivated: false, QuestEventManager.POILockoutReasonTypes.QuestLock, extraData);
			}
			break;
		}
		case QuestEventTypes.RallyMarker_BedrollLocked:
		{
			EntityPlayer entityPlayer4 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer4 != null)
			{
				entityPlayer4.QuestJournal.HandleRallyMarkerActivation(questCode, prefabPos, rallyMarkerActivated: false, QuestEventManager.POILockoutReasonTypes.Bedroll, 0uL);
			}
			break;
		}
		case QuestEventTypes.RallyMarker_PlayerLocked:
		{
			EntityPlayer entityPlayer6 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer6 != null)
			{
				entityPlayer6.QuestJournal.HandleRallyMarkerActivation(questCode, prefabPos, rallyMarkerActivated: false, QuestEventManager.POILockoutReasonTypes.PlayerInside, 0uL);
			}
			break;
		}
		case QuestEventTypes.RallyMarker_LandClaimLocked:
		{
			EntityPlayer entityPlayer8 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer8 != null)
			{
				entityPlayer8.QuestJournal.HandleRallyMarkerActivation(questCode, prefabPos, rallyMarkerActivated: false, QuestEventManager.POILockoutReasonTypes.LandClaim, 0uL);
			}
			break;
		}
		case QuestEventTypes.LockPOI:
			GameManager.Instance.StartCoroutine(QuestEventManager.Current.QuestLockPOI(entityID, QuestClass.GetQuest(questID), prefabPos, questTags, SharedWithList, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(QuestEventTypes.POILocked, entityID), _onlyClientsAttachedToAnEntity: false, entityID);
			}));
			break;
		case QuestEventTypes.POILocked:
			if (ObjectiveRallyPoint.OutstandingRallyPoint != null)
			{
				ObjectiveRallyPoint.OutstandingRallyPoint.RallyPointActivated();
			}
			break;
		case QuestEventTypes.UnlockPOI:
			QuestEventManager.Current.QuestUnlockPOI(entityID, prefabPos);
			break;
		case QuestEventTypes.ClearSleeper:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (SubscribeTo)
				{
					QuestEventManager.Current.SubscribeToUpdateEvent(entityID, prefabPos);
				}
				else
				{
					QuestEventManager.Current.UnSubscribeToUpdateEvent(entityID, prefabPos);
				}
			}
			else
			{
				QuestEventManager.Current.ClearedSleepers(prefabPos);
			}
			break;
		case QuestEventTypes.ShowSleeperVolume:
			QuestEventManager.Current.SleeperVolumePositionAdded(prefabPos);
			break;
		case QuestEventTypes.HideSleeperVolume:
			QuestEventManager.Current.SleeperVolumePositionRemoved(prefabPos);
			break;
		case QuestEventTypes.SetupFetch:
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.SetupFetchForMP(entityID, prefabPos, FetchModeType, SharedWithList);
				break;
			}
			EntityPlayer entityPlayer5 = _world.GetEntity(entityID) as EntityPlayer;
			Quest.PositionDataTypes dataType = ((FetchModeType == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
			if (entityPlayer5 != null)
			{
				entityPlayer5.QuestJournal.SetActivePositionData(dataType, new Vector3i(prefabPos));
			}
			break;
		}
		case QuestEventTypes.SetupRestorePower:
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				List<Vector3i> activateBlockList = new List<Vector3i>();
				QuestEventManager.Current.SetupActivateForMP(entityID, questCode, eventName, activateBlockList, GameManager.Instance.World, prefabPos, blockIndex, SharedWithList);
				break;
			}
			EntityPlayer entityPlayer2 = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer2 != null)
			{
				entityPlayer2.QuestJournal.HandleRestorePowerReceived(prefabPos, activateList);
			}
			break;
		}
		case QuestEventTypes.FinishManagedQuest:
			QuestEventManager.Current.FinishManagedQuest(questCode, _world.GetEntity(entityID) as EntityPlayer);
			break;
		case QuestEventTypes.ResetTraderQuests:
			QuestEventManager.Current.AddTraderResetQuestsForPlayer(entityID, questCode);
			if (factionPointOverride != -1)
			{
				EntityPlayer entityPlayer = _world.GetEntity(entityID) as EntityPlayer;
				if (entityPlayer != null && _world.GetEntity(questCode) is EntityTrader entityTrader)
				{
					entityTrader.ClearActiveQuests(entityPlayer.entityId);
					entityTrader.SetupActiveQuestsForPlayer(entityPlayer, factionPointOverride);
				}
			}
			break;
		case QuestEventTypes.ConfirmRallyMarker:
			break;
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
