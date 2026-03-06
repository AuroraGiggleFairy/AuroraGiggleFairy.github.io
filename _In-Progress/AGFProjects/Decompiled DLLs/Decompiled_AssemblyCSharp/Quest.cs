using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Audio;
using Quests;
using Quests.Requirements;
using UnityEngine;

public class Quest
{
	public enum QuestState
	{
		NotStarted,
		InProgress,
		ReadyForTurnIn,
		Completed,
		Failed
	}

	public enum PositionDataTypes
	{
		QuestGiver,
		Location,
		POIPosition,
		POISize,
		TreasurePoint,
		FetchContainer,
		HiddenCache,
		Activate,
		TreasureOffset,
		TraderPosition
	}

	public QuestJournal OwnerJournal;

	public static byte FileVersion = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestState _currentState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedOwnerID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> sharedWithList;

	public int QuestCode;

	public int QuestGiverID = -1;

	public static int MaxQuestTier = 5;

	public static int QuestsPerTier = 10;

	public byte QuestFaction;

	public bool RallyMarkerActivated;

	public FastTags<TagGroup.Global> QuestTags = FastTags<TagGroup.Global>.none;

	public bool NeedsNPCSetPosition;

	public int QuestProgressDay = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tracked;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public MapObject mapObject;

	public PrefabInstance QuestPrefab;

	public NavObject NavObject;

	public Dictionary<PositionDataTypes, Vector3> PositionData = new EnumDictionary<PositionDataTypes, Vector3>();

	public Dictionary<string, string> DataVariables = new Dictionary<string, string>();

	public List<BaseQuestAction> Actions = new List<BaseQuestAction>();

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseObjective> Objectives = new List<BaseObjective>();

	public List<BaseReward> Rewards = new List<BaseReward>();

	public TrackingHandler TrackingHelper = new TrackingHandler();

	public QuestState CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			if (_currentState != value)
			{
				_currentState = value;
				PrefabInstance.RefreshSwitchesInContainingPoi(this);
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentQuestVersion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string PreviousQuest { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool OptionalComplete
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ulong FinishTime { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentPhase { get; set; }

	public int SharedOwnerID
	{
		get
		{
			return sharedOwnerID;
		}
		set
		{
			if (sharedOwnerID != value)
			{
				sharedOwnerID = value;
			}
		}
	}

	public static int QuestsPerDay => GameStats.GetInt(EnumGameStats.QuestProgressionDailyLimit);

	public bool Active
	{
		get
		{
			if (CurrentState != QuestState.InProgress)
			{
				return CurrentState == QuestState.ReadyForTurnIn;
			}
			return true;
		}
	}

	public Vector3 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public bool HasPosition
	{
		get
		{
			if (MapObject == null)
			{
				return NavObject != null;
			}
			return true;
		}
	}

	public string RequirementsString
	{
		get
		{
			if (Requirements.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < Requirements.Count; i++)
				{
					stringBuilder.Append(Requirements[i].CheckRequirement() ? "[DEFAULT_COLOR]" : "[MISSING_COLOR]");
					stringBuilder.Append(Requirements[i].Description);
					stringBuilder.Append("[-]");
					stringBuilder.Append((i < Requirements.Count - 1) ? ", " : "");
				}
				return stringBuilder.ToString();
			}
			return "";
		}
	}

	public bool Tracked
	{
		get
		{
			return tracked;
		}
		set
		{
			if (tracked)
			{
				SetMapObjectSelected(isSelected: false);
			}
			tracked = value;
			if (tracked)
			{
				SetMapObjectSelected(isSelected: true);
			}
		}
	}

	public int ActiveObjectives
	{
		get
		{
			int num = 0;
			for (int i = 0; i < Objectives.Count; i++)
			{
				if ((Objectives[i].Phase == 0 || Objectives[i].Phase == CurrentPhase) && !Objectives[i].HiddenObjective)
				{
					num++;
				}
			}
			return num;
		}
	}

	public QuestClass QuestClass
	{
		get
		{
			if (questClass == null)
			{
				questClass = QuestClass.GetQuest(ID);
			}
			return questClass;
		}
	}

	public bool IsShareable
	{
		get
		{
			if (SharedOwnerID == -1 && QuestClass.Shareable && !RallyMarkerActivated)
			{
				return CurrentState == QuestState.InProgress;
			}
			return false;
		}
	}

	public MapObject MapObject
	{
		get
		{
			return mapObject;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (mapObject != null)
			{
				GameManager.Instance.World.ObjectOnMapRemove(mapObject.type, (int)mapObject.key);
			}
			mapObject = value;
		}
	}

	public bool AddsProgression => QuestProgressDay > 0;

	public void AddQuestTag(FastTags<TagGroup.Global> tag)
	{
		QuestTags |= tag;
	}

	public byte GetActionIndex(BaseQuestAction action)
	{
		for (int i = 0; i < Actions.Count; i++)
		{
			if (action == Actions[i])
			{
				return (byte)i;
			}
		}
		return 0;
	}

	public byte GetObjectiveIndex(BaseObjective objective)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (objective == Objectives[i])
			{
				return (byte)i;
			}
		}
		return 0;
	}

	public void HandleMapObject(PositionDataTypes dataType, string navObjectName, int defaultTreasureRadius = -1)
	{
		if (OwnerJournal == null)
		{
			return;
		}
		RemoveMapObject();
		Vector3 pos = Vector3.zero;
		_ = Vector3.zero;
		float extraData = -1f;
		bool flag = false;
		switch (dataType)
		{
		case PositionDataTypes.TreasurePoint:
		{
			if (!GetPositionData(out pos, PositionDataTypes.TreasurePoint))
			{
				break;
			}
			if (defaultTreasureRadius == -1)
			{
				defaultTreasureRadius = ObjectiveTreasureChest.TreasureRadiusInitial;
			}
			float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, defaultTreasureRadius, OwnerJournal.OwnerPlayer);
			value = Mathf.Clamp(value, 0f, defaultTreasureRadius);
			_ = GameManager.Instance.World;
			GetPositionData(out var pos3, PositionDataTypes.TreasureOffset);
			Position = pos + pos3 * value;
			if (navObjectName == "")
			{
				if (MapObject is MapObjectTreasureChest)
				{
					(MapObject as MapObjectTreasureChest).SetPosition(Position);
				}
				else
				{
					MapObject = new MapObjectTreasureChest(Position, QuestCode, defaultTreasureRadius);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
			}
			else
			{
				extraData = defaultTreasureRadius;
			}
			flag = true;
			break;
		}
		case PositionDataTypes.FetchContainer:
			if (GetPositionData(out pos, PositionDataTypes.FetchContainer))
			{
				Position = pos;
				if (navObjectName == "")
				{
					MapObject = new MapObjectFetchItem(pos + Vector3.one * 0.5f);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case PositionDataTypes.HiddenCache:
			if (GetPositionData(out pos, PositionDataTypes.HiddenCache))
			{
				Position = pos;
				if (navObjectName == "")
				{
					MapObject = new MapObjectHiddenCache(pos + Vector3.one * 0.5f);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case PositionDataTypes.Activate:
			if (GetPositionData(out pos, PositionDataTypes.Activate))
			{
				Position = pos;
				if (navObjectName == "")
				{
					MapObject = new MapObjectQuest(pos + Vector3.one * 0.5f);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case PositionDataTypes.Location:
			if (GetPositionData(out pos, PositionDataTypes.Location))
			{
				Position = pos;
				if (navObjectName == "")
				{
					MapObject = new MapObjectQuest(pos);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case PositionDataTypes.POIPosition:
		{
			if (!GetPositionData(out pos, PositionDataTypes.POIPosition))
			{
				break;
			}
			Vector3 pos2 = Vector3.zero;
			if (GetPositionData(out pos2, PositionDataTypes.POISize))
			{
				Vector3 vector = new Vector3(pos.x + pos2.x / 2f, pos.y, pos.z + pos2.z / 2f);
				vector.y = (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y);
				Position = vector;
				if (navObjectName == "")
				{
					MapObject = new MapObjectQuest(vector);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
			}
			new Vector3(0.5f, 0f, 0.5f);
			flag = true;
			break;
		}
		case PositionDataTypes.QuestGiver:
			if (GetPositionData(out pos, PositionDataTypes.QuestGiver))
			{
				Position = pos;
				if (navObjectName == "")
				{
					MapObject = new MapObjectQuest(pos);
					GameManager.Instance.World.ObjectOnMapAdd(MapObject);
				}
				flag = true;
			}
			break;
		}
		if (navObjectName != "" && flag)
		{
			World world = GameManager.Instance.World;
			EntityPlayer entityPlayer = world.GetEntity(sharedOwnerID) as EntityPlayer;
			if (entityPlayer == null)
			{
				entityPlayer = world.GetPrimaryPlayer();
			}
			NavObject = NavObjectManager.Instance.RegisterNavObject(navObjectName, Position + new Vector3(0.5f, 0f, 0.5f), "", hiddenOnCompass: false, -1, entityPlayer);
			NavObject.IsActive = false;
			NavObject.ExtraData = extraData;
			QuestClass questClass = QuestClass;
			NavObject.name = $"{questClass.Name} ({entityPlayer.PlayerDisplayName})";
		}
		SetMapObjectSelected(tracked);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMapObjectSelected(bool isSelected)
	{
		if (NavObject != null)
		{
			NavObject.IsActive = isSelected;
		}
		if (MapObject != null)
		{
			if (MapObject is MapObjectQuest)
			{
				((MapObjectQuest)MapObject).IsSelected = isSelected;
			}
			else if (MapObject is MapObjectTreasureChest)
			{
				((MapObjectTreasureChest)MapObject).IsSelected = isSelected;
			}
			else if (MapObject is MapObjectFetchItem)
			{
				((MapObjectFetchItem)MapObject).IsSelected = isSelected;
			}
			else if (MapObject is MapObjectHiddenCache)
			{
				((MapObjectHiddenCache)MapObject).IsSelected = isSelected;
			}
			else if (MapObject is MapObjectRestorePower)
			{
				((MapObjectRestorePower)MapObject).IsSelected = isSelected;
			}
		}
	}

	public void SetupQuestCode()
	{
		if (QuestCode == 0)
		{
			QuestCode = (Time.unscaledTime + "_" + ID + "_" + OwnerJournal.OwnerPlayer.entityId + "_" + QuestGiverID).GetHashCode();
		}
	}

	public void RemoveMapObject()
	{
		if (MapObject != null)
		{
			MapObject = null;
		}
		if (NavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(NavObject);
			NavObject = null;
		}
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POIPosition))
		{
			Vector3 pos2 = Vector3.zero;
			if (GetPositionData(out pos2, PositionDataTypes.POISize))
			{
				Vector3 vector = new Vector3(pos.x + pos2.x / 2f, OwnerJournal.OwnerPlayer.position.y, pos.z + pos2.z / 2f);
				GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.Quest, vector);
			}
		}
		else if (GetPositionData(out pos, PositionDataTypes.Location))
		{
			GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.Quest, pos);
		}
		else if (GetPositionData(out pos, PositionDataTypes.TreasurePoint))
		{
			GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.TreasureChest, QuestCode);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void UnhookQuest()
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].HandleRemoveHooks();
		}
		for (int j = 0; j < Objectives.Count; j++)
		{
			Objectives[j].RemoveObjectives();
		}
		RemoveMapObject();
	}

	public Quest(string id)
	{
		ID = id;
		CurrentPhase = 1;
		CurrentState = QuestState.InProgress;
	}

	public void SetupTags()
	{
		NeedsNPCSetPosition = false;
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].OwnerQuest = this;
			Objectives[i].HandleVariables();
			Objectives[i].SetupQuestTag();
			if (Objectives[i].NeedsNPCSetPosition)
			{
				NeedsNPCSetPosition = true;
			}
		}
	}

	public bool SetupPosition(EntityNPC ownerNPC, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i].SetupPosition(ownerNPC, player, usedPOILocations, entityIDforQuests))
			{
				return true;
			}
		}
		return false;
	}

	public void SetPosition(EntityNPC ownerNPC, Vector3 position, Vector3 size)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].OwnerQuest = this;
			Objectives[i].HandleVariables();
			Objectives[i].SetupQuestTag();
		}
		for (int j = 0; j < Objectives.Count; j++)
		{
			Objectives[j].SetPosition(position, size);
		}
	}

	public void SetObjectivePosition(PositionDataTypes dataType, Vector3i position)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].OwnerQuest = this;
			Objectives[i].HandleVariables();
			Objectives[i].SetupQuestTag();
		}
		for (int j = 0; j < Objectives.Count; j++)
		{
			Objectives[j].SetPosition(dataType, position);
		}
	}

	public bool HandleRallyMarkerActivation(Vector3 prefabPos, bool rallyMarkerActivated, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData)
	{
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POIPosition))
		{
			if (pos != prefabPos)
			{
				return false;
			}
			for (int i = 0; i < Objectives.Count; i++)
			{
				Objectives[i].OwnerQuest = this;
				Objectives[i].HandleVariables();
				Objectives[i].SetupQuestTag();
			}
			for (int j = 0; j < Objectives.Count; j++)
			{
				if (Objectives[j] is ObjectiveRallyPoint objectiveRallyPoint)
				{
					objectiveRallyPoint.RallyPointActivate(prefabPos, rallyMarkerActivated, lockoutReason, extraData);
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void RefreshRallyMarker()
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i] is ObjectiveRallyPoint && Objectives[i].Phase == CurrentPhase)
			{
				(Objectives[i] as ObjectiveRallyPoint).RallyPointRefresh();
				break;
			}
		}
	}

	public bool CheckIsQuestGiver(int entityID)
	{
		Entity entity = GameManager.Instance.World.GetEntity(entityID);
		if (QuestGiverID == entityID || (entity != null && (entity.position - GetQuestGiverLocation()).magnitude < 3f))
		{
			return true;
		}
		return false;
	}

	public Vector3 GetQuestGiverLocation()
	{
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.QuestGiver))
		{
			return pos;
		}
		return Vector3.zero;
	}

	public Vector3 GetLocation()
	{
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POIPosition))
		{
			return pos;
		}
		if (GetPositionData(out pos, PositionDataTypes.TreasurePoint))
		{
			return pos;
		}
		if (GetPositionData(out pos, PositionDataTypes.Location))
		{
			return pos;
		}
		return Vector3.zero;
	}

	public Vector3 GetLocationSize()
	{
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POISize))
		{
			return pos;
		}
		return Vector3.zero;
	}

	public Rect GetLocationRect()
	{
		int num = 5;
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POIPosition))
		{
			Vector3 pos2 = Vector3.zero;
			if (GetPositionData(out pos2, PositionDataTypes.POISize))
			{
				return new Rect(pos.x - (float)num, pos.z - (float)num, pos2.x + (float)(num * 2), pos2.z + (float)(num * 2));
			}
		}
		return Rect.zero;
	}

	public void StartQuest(bool newQuest = true, bool notify = true)
	{
		if (newQuest)
		{
			CurrentState = QuestState.InProgress;
		}
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].OwnerQuest = this;
			Actions[i].HandleVariables();
			Actions[i].SetupAction();
			if (newQuest && Actions[i].Phase == CurrentPhase && !Actions[i].OnComplete)
			{
				Actions[i].HandlePerformAction();
			}
		}
		for (int j = 0; j < Requirements.Count; j++)
		{
			Requirements[j].OwnerQuest = this;
			Requirements[j].HandleVariables();
			Requirements[j].SetupRequirement();
		}
		for (int k = 0; k < Objectives.Count; k++)
		{
			Objectives[k].OwnerQuest = this;
			Objectives[k].HandleVariables();
			Objectives[k].SetupQuestTag();
		}
		for (int l = 0; l < Objectives.Count; l++)
		{
			Objectives[l].SetupObjective();
			Objectives[l].SetupDisplay();
		}
		for (int m = 0; m < Objectives.Count; m++)
		{
			if (Objectives[m].Phase == CurrentPhase)
			{
				if (Objectives[m].ObjectiveState == BaseObjective.ObjectiveStates.NotStarted)
				{
					Objectives[m].ObjectiveState = BaseObjective.ObjectiveStates.InProgress;
				}
				if (CurrentState == QuestState.InProgress && Objectives[m].Phase == CurrentPhase)
				{
					Objectives[m].HandleRemoveHooks();
					Objectives[m].HandleAddHooks();
					Objectives[m].Refresh();
				}
			}
		}
		bool flag = false;
		for (int n = 0; n < Rewards.Count; n++)
		{
			Rewards[n].OwnerQuest = this;
			Rewards[n].HandleVariables();
			if (Rewards[n].ReceiveStage == BaseReward.ReceiveStages.QuestStart && newQuest)
			{
				Rewards[n].GiveReward();
			}
			if (Rewards[n].RewardIndex > 0 && !flag)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			SetupRewards();
		}
		if (newQuest && notify)
		{
			QuestClass quest = QuestClass.GetQuest(ID);
			string arg = ((quest.Name == quest.SubTitle) ? quest.Name : $"{quest.Name} - {quest.SubTitle}");
			GameManager.ShowTooltip(OwnerJournal.OwnerPlayer, string.Format("{0} {1}: {2}", quest.Category, Localization.Get("started"), arg));
			Manager.PlayInsidePlayerHead("quest_started");
			GameManager.Instance.StartCoroutine(trackLater(this));
		}
		SetupQuestCode();
		TrackingHelper.LocalPlayer = OwnerJournal.OwnerPlayer;
		TrackingHelper.QuestCode = QuestCode;
		RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, playObjectiveComplete: false);
	}

	public void SetupSharedQuest()
	{
		CurrentState = QuestState.NotStarted;
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].OwnerQuest = this;
			Actions[i].HandleVariables();
			Actions[i].SetupAction();
		}
		for (int j = 0; j < Requirements.Count; j++)
		{
			Requirements[j].OwnerQuest = this;
			Requirements[j].HandleVariables();
			Requirements[j].SetupRequirement();
		}
		for (int k = 0; k < Objectives.Count; k++)
		{
			Objectives[k].OwnerQuest = this;
			Objectives[k].HandleVariables();
			Objectives[k].SetupObjective();
			Objectives[k].SetupDisplay();
		}
		for (int l = 0; l < Rewards.Count; l++)
		{
			Rewards[l].OwnerQuest = this;
			Rewards[l].HandleVariables();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AdvancePhase()
	{
		CurrentPhase++;
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Phase == CurrentPhase && !Actions[i].OnComplete)
			{
				Actions[i].HandlePerformAction();
			}
		}
		for (int j = 0; j < Objectives.Count; j++)
		{
			if (CurrentState == QuestState.InProgress)
			{
				if (Objectives[j].Phase == CurrentPhase - 1)
				{
					Objectives[j].HandlePhaseCompleted();
				}
				if (Objectives[j].Phase == CurrentPhase || Objectives[j].Phase == 0)
				{
					if (Objectives[j].ObjectiveState == BaseObjective.ObjectiveStates.NotStarted)
					{
						Objectives[j].ObjectiveState = BaseObjective.ObjectiveStates.InProgress;
					}
					Objectives[j].HandleRemoveHooks();
					Objectives[j].HandleAddHooks();
				}
				else
				{
					Objectives[j].HandleRemoveHooks();
				}
			}
			else
			{
				Objectives[j].HandleRemoveHooks();
			}
		}
	}

	public void SetupRewards()
	{
		int num = 0;
		for (int i = 0; i < Rewards.Count; i++)
		{
			BaseReward baseReward = Rewards[i];
			baseReward.RewardIndex = (byte)i;
			if (!baseReward.isChainReward && baseReward.isChosenReward && !baseReward.isFixedLocation)
			{
				num++;
			}
		}
		if (num <= 1)
		{
			return;
		}
		World world = OwnerJournal.OwnerPlayer.world;
		for (int j = 0; j < 100; j++)
		{
			int num2 = world.GetGameRandom().RandomRange(Rewards.Count);
			int num3 = world.GetGameRandom().RandomRange(Rewards.Count);
			if (num2 != num3)
			{
				BaseReward baseReward2 = Rewards[num2];
				BaseReward baseReward3 = Rewards[num3];
				if (!baseReward2.isFixedLocation && baseReward2.isChosenReward && !baseReward3.isFixedLocation && baseReward3.isChosenReward)
				{
					byte rewardIndex = Rewards[num2].RewardIndex;
					Rewards[num2].RewardIndex = Rewards[num3].RewardIndex;
					Rewards[num3].RewardIndex = rewardIndex;
				}
			}
		}
	}

	public bool HandleActivateListReceived(Vector3 prefabPos, List<Vector3i> activateList)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i].SetupActivationList(prefabPos, activateList))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TrackQuest_Event(object obj)
	{
		Quest q = (Quest)obj;
		GameManager.Instance.StartCoroutine(trackLater(q));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator trackLater(Quest q)
	{
		yield return new WaitForSeconds(0.5f);
		if (!XUi.IsGameRunning())
		{
			yield break;
		}
		if (q.CurrentState == QuestState.InProgress)
		{
			if (OwnerJournal != null && null != OwnerJournal.OwnerPlayer)
			{
				LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(OwnerJournal.OwnerPlayer);
				if (null != uIForPlayer && null != uIForPlayer.xui && uIForPlayer.xui.QuestTracker != null)
				{
					XUiM_Quest questTracker = uIForPlayer.xui.QuestTracker;
					if (uIForPlayer.xui.Recipes.TrackedRecipe == null && questTracker.TrackedChallenge == null && questTracker.TrackedQuest == null)
					{
						q.Tracked = uIForPlayer.xui.QuestTracker.TrackedQuest == null || uIForPlayer.xui.QuestTracker.TrackedQuest == q;
					}
				}
				else
				{
					q.Tracked = false;
				}
				OwnerJournal.RefreshTracked();
			}
			else
			{
				q.Tracked = false;
			}
		}
		else
		{
			q.Tracked = false;
		}
	}

	public bool CheckRequirements()
	{
		for (int i = 0; i < Requirements.Count; i++)
		{
			if ((Requirements[i].Phase == 0 || Requirements[i].Phase == CurrentPhase) && !Requirements[i].CheckRequirement())
			{
				return false;
			}
		}
		return true;
	}

	public Quest Clone()
	{
		Quest quest = new Quest(ID);
		quest.ID = ID;
		quest.OwnerJournal = OwnerJournal;
		quest.CurrentQuestVersion = CurrentQuestVersion;
		quest.CurrentState = CurrentState;
		quest.FinishTime = FinishTime;
		quest.SharedOwnerID = SharedOwnerID;
		quest.QuestGiverID = QuestGiverID;
		quest.CurrentPhase = CurrentPhase;
		quest.QuestCode = QuestCode;
		quest.RallyMarkerActivated = RallyMarkerActivated;
		quest.Tracked = Tracked;
		quest.OptionalComplete = OptionalComplete;
		quest.QuestTags = QuestTags;
		quest.TrackingHelper = TrackingHelper;
		quest.QuestFaction = QuestFaction;
		quest.QuestProgressDay = QuestProgressDay;
		for (int i = 0; i < Actions.Count; i++)
		{
			BaseQuestAction baseQuestAction = Actions[i].Clone();
			baseQuestAction.OwnerQuest = quest;
			quest.Actions.Add(baseQuestAction);
		}
		for (int j = 0; j < Requirements.Count; j++)
		{
			BaseRequirement baseRequirement = Requirements[j].Clone();
			baseRequirement.OwnerQuest = quest;
			quest.Requirements.Add(baseRequirement);
		}
		for (int k = 0; k < Objectives.Count; k++)
		{
			BaseObjective baseObjective = Objectives[k].Clone();
			baseObjective.OwnerQuest = quest;
			quest.Objectives.Add(baseObjective);
		}
		for (int l = 0; l < Rewards.Count; l++)
		{
			BaseReward baseReward = Rewards[l].Clone();
			baseReward.OwnerQuest = quest;
			quest.Rewards.Add(baseReward);
		}
		foreach (KeyValuePair<string, string> dataVariable in DataVariables)
		{
			quest.DataVariables.Add(dataVariable.Key, dataVariable.Value);
		}
		foreach (KeyValuePair<PositionDataTypes, Vector3> positionDatum in PositionData)
		{
			quest.PositionData.Add(positionDatum.Key, positionDatum.Value);
		}
		return quest;
	}

	public void ResetObjectives()
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].ResetObjective();
		}
	}

	public void ResetToRallyPointObjective()
	{
		if (CurrentPhase == QuestClass.HighestPhase || !QuestClass.LoginRallyReset)
		{
			return;
		}
		RallyMarkerActivated = false;
		int num = -1;
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i] is ObjectiveRallyPoint)
			{
				num = Objectives[i].Phase;
			}
			if (num != -1 && Objectives[i].Phase >= num && Objectives[i].Phase <= CurrentPhase)
			{
				Objectives[i].ResetObjective();
			}
		}
		if (num != -1 && num < CurrentPhase)
		{
			CurrentPhase = (byte)num;
		}
	}

	public void RefreshQuestCompletion(QuestClass.CompletionTypes currentCompletionType = QuestClass.CompletionTypes.AutoComplete, List<BaseReward> rewardChoice = null, bool playObjectiveComplete = true, EntityNPC turnInNPC = null)
	{
		refreshQuestCompletion(currentCompletionType, rewardChoice, playObjectiveComplete, turnInNPC);
		PrefabInstance.RefreshSwitchesInContainingPoi(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshQuestCompletion(QuestClass.CompletionTypes currentCompletionType, List<BaseReward> rewardChoice, bool playObjectiveComplete, EntityNPC turnInNPC)
	{
		if ((CurrentState != QuestState.InProgress && CurrentState != QuestState.ReadyForTurnIn) || OwnerJournal == null)
		{
			return;
		}
		if (CurrentState == QuestState.InProgress)
		{
			OwnerJournal.RefreshQuest(this);
			OptionalComplete = true;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < Objectives.Count; i++)
			{
				if (Objectives[i].Phase != CurrentPhase && Objectives[i].Phase != 0)
				{
					continue;
				}
				if (Objectives[i].Optional)
				{
					if (!Objectives[i].Complete)
					{
						OptionalComplete = false;
					}
				}
				else if (!Objectives[i].Complete && !Objectives[i].AlwaysComplete)
				{
					flag = true;
				}
				else if (Objectives[i].ForcePhaseFinish)
				{
					flag2 = true;
				}
			}
			if (flag)
			{
				if (flag2)
				{
					CloseQuest(QuestState.Failed);
				}
				return;
			}
			if (CurrentPhase < QuestClass.HighestPhase)
			{
				AdvancePhase();
				if (playObjectiveComplete)
				{
					Manager.PlayInsidePlayerHead("quest_objective_complete");
				}
				if (CurrentPhase == QuestClass.HighestPhase && OwnerJournal.ActiveQuest == this)
				{
					OwnerJournal.ActiveQuest = null;
					OwnerJournal.RefreshRallyMarkerPositions();
				}
				return;
			}
		}
		if (currentCompletionType != QuestClass.CompletionType)
		{
			CurrentState = QuestState.ReadyForTurnIn;
			return;
		}
		QuestEventManager.Current.QuestCompleted(QuestTags, questClass);
		CloseQuest(QuestState.Completed, rewardChoice);
		if (QuestClass.ResetTraderQuests && turnInNPC is EntityTrader entityTrader)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayerLocal ownerPlayer = OwnerJournal.OwnerPlayer;
				entityTrader.ClearActiveQuests(ownerPlayer.entityId);
				entityTrader.SetupActiveQuestsForPlayer(ownerPlayer);
			}
			else
			{
				EntityPlayer ownerPlayer2 = OwnerJournal.OwnerPlayer;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ResetTraderQuests, ownerPlayer2.entityId, QuestGiverID, entityTrader.GetQuestFactionPoints(ownerPlayer2)));
			}
		}
	}

	public void CloseQuest(QuestState finalState, List<BaseReward> rewardChoice = null)
	{
		if (finalState != QuestState.Completed && finalState != QuestState.Failed)
		{
			Log.Warning($"Ending a quest in a state {finalState}. Should be {QuestState.Completed} or {QuestState.Failed}");
		}
		if (OwnerJournal == null)
		{
			return;
		}
		OwnerJournal.RefreshQuest(this);
		CurrentState = finalState;
		bool flag = finalState == QuestState.Completed;
		HandleUnlockPOI();
		bool flag2 = !string.IsNullOrEmpty(PreviousQuest);
		bool flag3 = flag && flag2;
		if (flag3)
		{
			for (int i = 0; i < Rewards.Count; i++)
			{
				if (Rewards[i] is RewardQuest && (Rewards[i] as RewardQuest).IsChainQuest)
				{
					flag3 = false;
				}
			}
		}
		ToolTipEvent toolTipEvent = new ToolTipEvent();
		for (int j = 0; j < Rewards.Count; j++)
		{
			if (Rewards[j].ReceiveStage == BaseReward.ReceiveStages.AfterCompleteNotification)
			{
				toolTipEvent.EventHandler += QuestRewardsLater_Event;
				toolTipEvent.Parameter = this;
				break;
			}
		}
		EntityPlayerLocal ownerPlayer = OwnerJournal.OwnerPlayer;
		string arg = ((questClass.Name == questClass.SubTitle) ? questClass.Name : $"{questClass.Name} - {questClass.SubTitle}");
		string arg2 = (flag ? Localization.Get("completed") : Localization.Get("failed"));
		string alertSound = (flag ? "quest_subtask_complete" : "quest_failed");
		ToolTipEvent handler = ((flag && !flag3) ? toolTipEvent : null);
		GameManager.ShowTooltip(ownerPlayer, $"{questClass.Category} {arg2}: {arg}", string.Empty, alertSound, handler);
		if (flag3)
		{
			GameManager.ShowTooltip(ownerPlayer, string.Format("{0} {1}: {2}", Localization.Get("questChain"), Localization.Get("completed"), questClass.GroupName), string.Empty, "quest_master_complete", toolTipEvent);
		}
		if (OwnerJournal.TrackedQuest == this)
		{
			OwnerJournal.TrackedQuest = null;
		}
		for (int k = 0; k < Objectives.Count; k++)
		{
			Objectives[k].HandleRemoveHooks();
		}
		for (int l = 0; l < Objectives.Count; l++)
		{
			Objectives[l].RemoveObjectives();
			if (flag)
			{
				Objectives[l].HandleCompleted();
			}
			else
			{
				Objectives[l].HandleFailed();
			}
		}
		RemoveMapObject();
		if (flag)
		{
			if (AddsProgression)
			{
				QuestEventManager.Current.HandleNewCompletedQuest(OwnerJournal.OwnerPlayer, QuestFaction, QuestClass.DifficultyTier, QuestClass.AddsToTierComplete);
			}
			for (int m = 0; m < Rewards.Count; m++)
			{
				if (Rewards[m].ReceiveStage == BaseReward.ReceiveStages.QuestCompletion && (!Rewards[m].Optional || (Rewards[m].Optional && OptionalComplete)) && (!Rewards[m].isChosenReward || (Rewards[m].isChosenReward && rewardChoice != null && rewardChoice.Contains(Rewards[m]))))
				{
					Rewards[m].GiveReward();
				}
			}
			OwnerJournal.CompleteQuest(this);
			for (int n = 0; n < Actions.Count; n++)
			{
				BaseQuestAction baseQuestAction = Actions[n];
				if (baseQuestAction.OnComplete)
				{
					baseQuestAction.HandlePerformAction();
				}
			}
		}
		else
		{
			OptionalComplete = false;
			tracked = false;
			OwnerJournal.FailedQuest(this);
		}
		if (OwnerJournal.ActiveQuest == this)
		{
			OwnerJournal.ActiveQuest = null;
			OwnerJournal.RefreshRallyMarkerPositions();
		}
		if (QuestClass.ResetTraderQuests)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.AddTraderResetQuestsForPlayer(OwnerJournal.OwnerPlayer.entityId, QuestGiverID);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ResetTraderQuests, OwnerJournal.OwnerPlayer.entityId, QuestGiverID, -1));
			}
		}
	}

	public void HandleUnlockPOI(EntityPlayer player = null)
	{
		Vector3 pos = Vector3.zero;
		if (GetPositionData(out pos, PositionDataTypes.POIPosition))
		{
			if (player == null)
			{
				player = OwnerJournal.OwnerPlayer;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.QuestUnlockPOI(player.entityId, pos);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.UnlockPOI, player.entityId, pos));
			}
		}
	}

	public void HandleQuestEvent(Quest ownerQuest, string eventType)
	{
		for (int i = 0; i < questClass.Events.Count; i++)
		{
			if (questClass.Events[i].EventType == eventType)
			{
				questClass.Events[i].HandleEvent(ownerQuest);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestRewardsLater_Event(object obj)
	{
		Quest q = (Quest)obj;
		GameManager.Instance.StartCoroutine(GiveRewardsLater(q));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator GiveRewardsLater(Quest q)
	{
		yield return new WaitForSeconds(3f);
		if (!XUi.IsGameRunning() || q.CurrentState != QuestState.Completed)
		{
			yield break;
		}
		for (int i = 0; i < Rewards.Count; i++)
		{
			if (Rewards[i].ReceiveStage == BaseReward.ReceiveStages.AfterCompleteNotification && (!Rewards[i].Optional || (Rewards[i].Optional && q.OptionalComplete)))
			{
				Rewards[i].GiveReward();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestAction AddAction(BaseQuestAction action)
	{
		if (action != null)
		{
			Actions.Add(action);
		}
		return action;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseRequirement AddRequirement(BaseRequirement requirement)
	{
		if (requirement != null)
		{
			Requirements.Add(requirement);
		}
		return requirement;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseObjective AddObjective(BaseObjective objective)
	{
		if (objective != null)
		{
			Objectives.Add(objective);
		}
		return objective;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseReward AddReward(BaseReward reward)
	{
		if (reward != null)
		{
			Rewards.Add(reward);
		}
		return reward;
	}

	public string ParseVariable(string value)
	{
		if (value != null && value.Contains("{"))
		{
			int num = value.IndexOf("{") + 1;
			int num2 = value.IndexOf("}", num);
			if (num2 != -1)
			{
				string key = value.Substring(num, num2 - num);
				if (DataVariables.ContainsKey(key))
				{
					value = DataVariables[key];
				}
			}
		}
		return value;
	}

	public void Read(PooledBinaryReader _br)
	{
		bool flag = true;
		CurrentFileVersion = _br.ReadByte();
		CurrentState = (QuestState)_br.ReadByte();
		SharedOwnerID = _br.ReadInt32();
		QuestGiverID = _br.ReadInt32();
		if (CurrentState == QuestState.InProgress)
		{
			tracked = _br.ReadBoolean();
			CurrentPhase = _br.ReadByte();
			QuestCode = _br.ReadInt32();
		}
		else if (CurrentState == QuestState.Completed)
		{
			CurrentPhase = QuestClass.HighestPhase;
		}
		PooledBinaryReader.StreamReadSizeMarker _sizeMarker = default(PooledBinaryReader.StreamReadSizeMarker);
		if (CurrentFileVersion >= 7)
		{
			_sizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
		}
		uint _bytesReceived;
		try
		{
			for (int i = 0; i < Objectives.Count; i++)
			{
				Objectives[i].Read(_br);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		finally
		{
			if (CurrentFileVersion >= 7 && !_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived))
			{
				Objectives.Clear();
				Log.Error("Loading player quests: Quest with ID " + ID + ": Failed loading objectives");
				flag = false;
			}
		}
		int num = _br.ReadByte();
		for (int j = 0; j < num; j++)
		{
			string key = _br.ReadString();
			string value = _br.ReadString();
			if (!DataVariables.ContainsKey(key))
			{
				DataVariables.Add(key, value);
			}
			else
			{
				DataVariables[key] = value;
			}
		}
		if (CurrentState == QuestState.InProgress)
		{
			PositionData.Clear();
			int num2 = _br.ReadByte();
			for (int k = 0; k < num2; k++)
			{
				PositionDataTypes dataType = (PositionDataTypes)_br.ReadByte();
				Vector3 value2 = StreamUtils.ReadVector3(_br);
				SetPositionData(dataType, value2);
			}
			RallyMarkerActivated = _br.ReadBoolean();
		}
		else
		{
			FinishTime = _br.ReadUInt64();
		}
		if (CurrentState == QuestState.InProgress || CurrentState == QuestState.ReadyForTurnIn)
		{
			PooledBinaryReader.StreamReadSizeMarker _sizeMarker2 = default(PooledBinaryReader.StreamReadSizeMarker);
			if (CurrentFileVersion >= 7)
			{
				_sizeMarker2 = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			}
			try
			{
				if (CurrentFileVersion <= 5)
				{
					for (int l = 0; l < Rewards.Count; l++)
					{
						Rewards[l].Read(_br);
					}
				}
				else
				{
					int num3 = _br.ReadInt32();
					for (int m = 0; m < num3; m++)
					{
						Rewards[m].Read(_br);
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
			finally
			{
				if (CurrentFileVersion >= 7 && !_br.ValidateSizeMarker(ref _sizeMarker2, out _bytesReceived))
				{
					Rewards.Clear();
					Log.Error("Loading player quests: Quest with ID " + ID + ": Failed loading rewards");
					flag = false;
				}
			}
		}
		if (CurrentFileVersion > 4)
		{
			QuestFaction = _br.ReadByte();
		}
		if (!flag && CurrentState != QuestState.Completed)
		{
			CurrentState = QuestState.Failed;
		}
		if (CurrentFileVersion >= 8)
		{
			QuestProgressDay = _br.ReadInt32();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(ID);
		_bw.Write(CurrentQuestVersion);
		_bw.Write(FileVersion);
		_bw.Write((byte)CurrentState);
		_bw.Write(SharedOwnerID);
		_bw.Write(QuestGiverID);
		if (CurrentState == QuestState.InProgress)
		{
			_bw.Write(Tracked);
			_bw.Write(CurrentPhase);
			_bw.Write(QuestCode);
		}
		PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
		for (int i = 0; i < Objectives.Count; i++)
		{
			Objectives[i].Write(_bw);
		}
		_bw.FinalizeSizeMarker(ref _sizeMarker);
		_bw.Write((byte)DataVariables.Count);
		foreach (KeyValuePair<string, string> dataVariable in DataVariables)
		{
			_bw.Write(dataVariable.Key);
			_bw.Write(dataVariable.Value);
		}
		if (CurrentState == QuestState.InProgress)
		{
			_bw.Write((byte)PositionData.Count);
			foreach (KeyValuePair<PositionDataTypes, Vector3> positionDatum in PositionData)
			{
				_bw.Write((byte)positionDatum.Key);
				StreamUtils.Write(_bw, positionDatum.Value);
			}
			_bw.Write(RallyMarkerActivated);
		}
		else
		{
			_bw.Write(FinishTime);
		}
		if (CurrentState == QuestState.InProgress || CurrentState == QuestState.ReadyForTurnIn)
		{
			PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker2 = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			_bw.Write(Rewards.Count);
			for (int j = 0; j < Rewards.Count; j++)
			{
				Rewards[j].Write(_bw);
			}
			_bw.FinalizeSizeMarker(ref _sizeMarker2);
		}
		_bw.Write(QuestFaction);
		_bw.Write(QuestProgressDay);
	}

	public void AddSharedLocation(Vector3 pos, Vector3 size)
	{
		for (int i = 0; i < Objectives.Count && (Objectives[i].Phase != CurrentPhase || !Objectives[i].SetLocation(pos, size)); i++)
		{
		}
	}

	public void AddSharedKill(string enemyType)
	{
		for (int i = 0; i < Objectives.Count; i++)
		{
			if (Objectives[i].Phase == CurrentPhase && Objectives[i].ID == enemyType)
			{
				Objectives[i].CurrentValue++;
				Objectives[i].Refresh();
			}
		}
	}

	public int GetSharedWithCount()
	{
		if (sharedWithList == null)
		{
			return 0;
		}
		return sharedWithList.Count;
	}

	public int GetSharedWithCountNotInRange()
	{
		if (sharedWithList == null)
		{
			return 0;
		}
		EntityPlayer ownerPlayer = OwnerJournal.OwnerPlayer;
		int num = 0;
		Rect locationRect = GetLocationRect();
		for (int i = 0; i < sharedWithList.Count; i++)
		{
			EntityPlayer entityPlayer = sharedWithList[i];
			if (locationRect != Rect.zero)
			{
				Vector3 point = entityPlayer.position;
				point.y = point.z;
				if (!locationRect.Contains(point))
				{
					num++;
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				num++;
			}
		}
		return num;
	}

	public List<EntityPlayer> GetSharedWithListNotInRange()
	{
		if (sharedWithList == null)
		{
			return null;
		}
		EntityPlayer ownerPlayer = OwnerJournal.OwnerPlayer;
		Rect locationRect = GetLocationRect();
		List<EntityPlayer> list = new List<EntityPlayer>();
		for (int i = 0; i < sharedWithList.Count; i++)
		{
			EntityPlayer entityPlayer = sharedWithList[i];
			if (locationRect != Rect.zero)
			{
				Vector3 point = entityPlayer.position;
				point.y = point.z;
				if (!locationRect.Contains(point))
				{
					list.Add(entityPlayer);
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				list.Add(entityPlayer);
			}
		}
		return list;
	}

	public void RemoveSharedNotInRange()
	{
		if (sharedWithList == null)
		{
			return;
		}
		EntityPlayer ownerPlayer = OwnerJournal.OwnerPlayer;
		Rect locationRect = GetLocationRect();
		for (int num = sharedWithList.Count - 1; num >= 0; num--)
		{
			EntityPlayer entityPlayer = sharedWithList[num];
			if (locationRect != Rect.zero)
			{
				Vector3 point = entityPlayer.position;
				point.y = point.z;
				if (!locationRect.Contains(point))
				{
					sharedWithList.RemoveAt(num);
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				sharedWithList.RemoveAt(num);
			}
		}
	}

	public int[] GetSharedWithIDList()
	{
		if (sharedWithList == null)
		{
			return null;
		}
		int[] array = new int[sharedWithList.Count];
		for (int i = 0; i < sharedWithList.Count; i++)
		{
			array[i] = sharedWithList[i].entityId;
		}
		return array;
	}

	public bool HasSharedWith(EntityPlayer player)
	{
		if (sharedWithList == null)
		{
			return false;
		}
		for (int i = 0; i < sharedWithList.Count; i++)
		{
			if (sharedWithList[i] == player)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSharedWith(EntityPlayer player)
	{
		if (sharedWithList == null)
		{
			sharedWithList = new List<EntityPlayer>();
		}
		if (!sharedWithList.Contains(player))
		{
			sharedWithList.Add(player);
		}
	}

	public bool RemoveSharedWith(EntityPlayer player)
	{
		bool result = false;
		if (sharedWithList == null)
		{
			return false;
		}
		for (int num = sharedWithList.Count - 1; num >= 0; num--)
		{
			if (sharedWithList[num].entityId == player.entityId)
			{
				result = true;
				sharedWithList.RemoveAt(num);
			}
		}
		if (sharedWithList.Count == 0)
		{
			sharedWithList = null;
		}
		return result;
	}

	public void SetPositionData(PositionDataTypes dataType, Vector3 value)
	{
		if (!PositionData.ContainsKey(dataType))
		{
			PositionData.Add(dataType, value);
		}
		else
		{
			PositionData[dataType] = value;
		}
		if (OwnerJournal != null)
		{
			GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(OwnerJournal.OwnerPlayer.entityId)?.AddQuestPosition(QuestCode, dataType, value);
		}
	}

	public void RemovePositionData(PositionDataTypes dataType)
	{
		if (PositionData.ContainsKey(dataType))
		{
			PositionData.Remove(dataType);
		}
	}

	public bool GetPositionData(out Vector3 pos, PositionDataTypes dataType)
	{
		if (PositionData.ContainsKey(dataType))
		{
			pos = PositionData[dataType];
			return true;
		}
		pos = Vector3.zero;
		return false;
	}

	public string GetParsedText(string text)
	{
		if (text.Contains("{"))
		{
			text = ParseBindingVariables(text);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string ParseBindingVariables(string response)
	{
		if (string.IsNullOrEmpty(response))
		{
			return response;
		}
		for (int num = response.IndexOf('{'); num != -1; num = ((num + 1 >= response.Length) ? (-1) : response.IndexOf('{', num + 1)))
		{
			int num2 = response.IndexOf('}', num);
			if (num2 != -1)
			{
				string text = response.Substring(num, num2 - num + 1);
				string[] array = text.Substring(1, text.Length - 2).Split('_', '.');
				if (array.Length == 2)
				{
					response = response.Replace(text, GetVariableText(array[0], -1, array[1]));
				}
				if (array.Length == 3)
				{
					response = response.Replace(text, GetVariableText(array[0], Convert.ToInt32(array[1]), array[2]));
				}
			}
		}
		return response;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetVariableText(string field, int index, string variableName)
	{
		int num = 0;
		switch (field)
		{
		case "fetch":
		{
			for (int l = 0; l < Objectives.Count; l++)
			{
				if ((Objectives[l] is ObjectiveFetch || Objectives[l] is ObjectiveFetchKeep) && (++num == index || index == -1))
				{
					return Objectives[l].ParseBinding(variableName);
				}
			}
			break;
		}
		case "buff":
		{
			for (int n = 0; n < Objectives.Count; n++)
			{
				if (Objectives[n] is ObjectiveBuff && (++num == index || index == -1))
				{
					return Objectives[n].ParseBinding(variableName);
				}
			}
			break;
		}
		case "kill":
		{
			for (int j = 0; j < Objectives.Count; j++)
			{
				if (Objectives[j] is ObjectiveEntityKill && (++num == index || index == -1))
				{
					return Objectives[j].ParseBinding(variableName);
				}
			}
			break;
		}
		case "goto":
		{
			for (int m = 0; m < Objectives.Count; m++)
			{
				if (Objectives[m] is ObjectiveGoto && (++num == index || index == -1))
				{
					return Objectives[m].ParseBinding(variableName);
				}
			}
			break;
		}
		case "poi":
		{
			for (int k = 0; k < Objectives.Count; k++)
			{
				if (Objectives[k] is ObjectiveRandomPOIGoto && (++num == index || index == -1))
				{
					return Objectives[k].ParseBinding(variableName);
				}
			}
			break;
		}
		case "treasure":
		{
			for (int i = 0; i < Objectives.Count; i++)
			{
				if (Objectives[i] is ObjectiveTreasureChest && (++num == index || index == -1))
				{
					return Objectives[i].ParseBinding(variableName);
				}
			}
			break;
		}
		}
		return field;
	}

	public string GetPOIName()
	{
		if (DataVariables.ContainsKey("POIName"))
		{
			return DataVariables["POIName"];
		}
		return "";
	}

	public bool CanTurnInQuest(List<BaseReward> rewardChoice)
	{
		_ = OwnerJournal.OwnerPlayer;
		ItemStack[] array = OwnerJournal.OwnerPlayer.bag.CloneItemStack();
		ItemStack[] array2 = OwnerJournal.OwnerPlayer.inventory.CloneItemStack();
		ItemStack[] array3 = new ItemStack[array.Length + array2.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array3[num++] = array[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			array3[num++] = array2[j];
		}
		for (int k = 0; k < Rewards.Count; k++)
		{
			if (Rewards[k].ReceiveStage != BaseReward.ReceiveStages.QuestCompletion || (Rewards[k].Optional && (!Rewards[k].Optional || !OptionalComplete)) || (Rewards[k].isChosenReward && (!Rewards[k].isChosenReward || rewardChoice == null || !rewardChoice.Contains(Rewards[k]))))
			{
				continue;
			}
			ItemStack rewardItem = Rewards[k].GetRewardItem();
			if (!rewardItem.IsEmpty())
			{
				XUiM_PlayerInventory.TryStackItem(0, rewardItem, array3);
				if (rewardItem.count > 0 && ItemStack.AddToItemStackArray(array3, rewardItem) == -1)
				{
					return false;
				}
			}
		}
		return true;
	}
}
