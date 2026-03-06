using System.Collections;
using System.Collections.Generic;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRallyPoint : BaseObjective
{
	public enum RallyStartTypes
	{
		Find,
		Create
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PositionSetTypes
	{
		None,
		POIPosition,
		RallyMarkerPosition
	}

	public RallyStartTypes RallyStartType;

	public static string PropRallyStartMode = "start_mode";

	public static string PropAllowedStartTime = "allowed_start_hour";

	public static string PropAllowedEndTime = "allowed_end_hour";

	public static string PropActivateEvent = "activate_event";

	public static string PropRallyMarkerType = "rally_marker_type";

	public static ObjectiveRallyPoint OutstandingRallyPoint = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect outerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int endTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PositionSetTypes positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textActivateRallyPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textHeadToRallyPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textWaitForActivate;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string rallyMarkerType = "questRallyMarker";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string activateEvent = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRallyVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i rallyPos = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDistance = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceNeeded = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setRallyPointVisible;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Distance;

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				return ValueDisplayFormatters.Distance(currentDistance);
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
			{
				return "";
			}
			if (base.ObjectiveState == ObjectiveStates.Failed)
			{
				return Localization.Get("failed");
			}
			return Localization.Get("completed");
		}
	}

	public Vector3i RallyPos => rallyPos;

	public override void SetupObjective()
	{
		textActivateRallyPoint = Localization.Get("ObjectiveRallyPointActivate");
		textHeadToRallyPoint = Localization.Get("ObjectiveRallyPointHeadTo");
		textWaitForActivate = Localization.Get("ObjectiveWaitForActivate_keyword");
		keyword = Localization.Get("ObjectiveBlockActivate_keyword");
		localizedName = ((ID != "" && ID != null) ? Localization.Get(ID) : "Any Block");
		if (base.OwnerQuest.SharedOwnerID != -1)
		{
			base.Description = textWaitForActivate;
		}
		else
		{
			base.Description = textActivateRallyPoint;
		}
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.BlockActivate += Current_BlockActivate;
	}

	public static void SetupFlags(List<BaseObjective> objectives)
	{
		HashSet<ObjectiveRallyPointData> hashSet = new HashSet<ObjectiveRallyPointData>();
		foreach (BaseObjective objective in objectives)
		{
			if (!(objective is ObjectiveRallyPoint { isRallyVisible: not false } objectiveRallyPoint))
			{
				continue;
			}
			Transform blockTransform = getBlockTransform(objectiveRallyPoint.rallyPos);
			if (!(blockTransform != null))
			{
				continue;
			}
			ObjectiveRallyPointData component = blockTransform.gameObject.GetComponent<ObjectiveRallyPointData>();
			if (!(component != null))
			{
				continue;
			}
			Quest ownerQuest = objectiveRallyPoint.OwnerQuest;
			if (ownerQuest.CurrentPhase == objectiveRallyPoint.Phase)
			{
				if (!hashSet.Contains(component))
				{
					hashSet.Add(component);
					component.ClearAllFlags();
				}
				component.AddFlag(objectiveRallyPoint.rallyMarkerType, ownerQuest.SharedOwnerID == -1);
			}
		}
		foreach (ObjectiveRallyPointData item in hashSet)
		{
			item.UpdateAllFlags();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getBlockTransform(Vector3i rallyPos)
	{
		Transform result = null;
		if (GameManager.Instance.World.GetChunkFromWorldPos(rallyPos) is Chunk chunk)
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(rallyPos);
			if (blockEntity != null)
			{
				result = blockEntity.transform;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setVisibility(Transform t, string childName, bool isVisible)
	{
		isRallyVisible = isVisible;
	}

	public override void RemoveObjectives()
	{
		base.RemoveObjectives();
		World world = GameManager.Instance.World;
		if (!(rallyPos != Vector3i.zero))
		{
			return;
		}
		if (RallyStartType == RallyStartTypes.Find)
		{
			if (world.GetChunkFromWorldPos(rallyPos) is Chunk chunk)
			{
				BlockEntityData blockEntity = chunk.GetBlockEntity(rallyPos);
				if (blockEntity != null && blockEntity.transform != null)
				{
					setVisibility(blockEntity.transform, rallyMarkerType, isVisible: false);
				}
			}
		}
		else if (base.OwnerQuest.SharedOwnerID == -1)
		{
			GameManager.Instance.World.SetBlockRPC(rallyPos, BlockValue.Air, sbyte.MaxValue);
		}
		else
		{
			GameManager.Instance.StartCoroutine(setRallyPointVisibility(visible: false));
		}
	}

	public override void RemoveHooks()
	{
		World world = GameManager.Instance.World;
		QuestEventManager.Current.BlockActivate -= Current_BlockActivate;
		if (rallyPos != Vector3i.zero)
		{
			if (RallyStartType == RallyStartTypes.Find)
			{
				bool rallyPointVisibility = base.OwnerQuest.OwnerJournal.HasQuestAtRallyPosition(rallyPos.ToVector3()) != null && base.OwnerQuest.OwnerJournal.ActiveQuest == null;
				GameManager.Instance.StartCoroutine(setRallyPointVisibility(rallyPointVisibility));
			}
			else if (base.OwnerQuest.SharedOwnerID == -1)
			{
				world.SetBlockRPC(rallyPos, BlockValue.Air, sbyte.MaxValue);
			}
			else
			{
				GameManager.Instance.StartCoroutine(setRallyPointVisibility(visible: false));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setRallyPointVisibility(bool visible)
	{
		QuestEventManager.Current.BlockActivate -= Current_BlockActivate;
		if (visible)
		{
			QuestEventManager.Current.BlockActivate += Current_BlockActivate;
		}
		BlockValue bvRallyType = Block.GetBlockValue("questRallyMarker");
		World world = GameManager.Instance.World;
		if (!(world.GetChunkFromWorldPos(rallyPos) is Chunk chunk))
		{
			yield break;
		}
		BlockEntityData bd = chunk.GetBlockEntity(rallyPos);
		if (bd != null)
		{
			while (bd.transform == null)
			{
				yield return null;
			}
			if (!visible)
			{
				ObjectiveRallyPointData component = bd.transform.gameObject.GetComponent<ObjectiveRallyPointData>();
				if (component != null)
				{
					component.RemoveFlag(rallyMarkerType);
					component.UpdateAllFlags();
				}
			}
			if (bd.blockValue.EqualsExceptRotation(bvRallyType))
			{
				setVisibility(bd.transform, rallyMarkerType, visible);
			}
			else
			{
				world.SetBlockRPC(chunk.ClrIdx, rallyPos, bvRallyType);
			}
		}
		else
		{
			world.SetBlockRPC(chunk.ClrIdx, rallyPos, bvRallyType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockName, Vector3i blockPos)
	{
		if (base.OwnerQuest.SharedOwnerID != -1 || base.Complete || base.OwnerQuest.OwnerJournal.ActiveQuest != null || rallyPos != blockPos)
		{
			return;
		}
		if (TwitchManager.HasInstance && TwitchManager.Current.IsVoting)
		{
			GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ttWaitForVoteQuest"));
			return;
		}
		Vector3 pos = Vector3.zero;
		int num = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
		if (startTime != -1 && endTime != -1)
		{
			if (startTime < endTime)
			{
				if (num < startTime || num >= endTime)
				{
					GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, string.Format(Localization.Get("ObjectiveRallyPointInvalidStartTime"), startTime, endTime));
					return;
				}
			}
			else if (num < startTime && num >= endTime)
			{
				GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, string.Format(Localization.Get("ObjectiveRallyPointInvalidStartTime"), startTime, endTime));
				return;
			}
		}
		base.OwnerQuest.RemoveSharedNotInRange();
		if (base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition))
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Vector2 prefabPos = new Vector2(pos.x, pos.z);
				ulong extraData;
				QuestEventManager.POILockoutReasonTypes pOILockoutReasonTypes = QuestEventManager.Current.CheckForPOILockouts(ownerPlayer.entityId, prefabPos, out extraData);
				RallyPointActivate(pos, pOILockoutReasonTypes == QuestEventManager.POILockoutReasonTypes.None, pOILockoutReasonTypes, extraData);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.TryRallyMarker, ownerPlayer.entityId, pos, base.OwnerQuest.QuestCode));
			}
		}
		else if (base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.Location))
		{
			RallyPointActivate(pos, activate: true, QuestEventManager.POILockoutReasonTypes.None, 0uL);
		}
	}

	public void RallyPointActivate(Vector3 prefabPos, bool activate, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData)
	{
		bool flag = base.OwnerQuest.PositionData.ContainsKey(Quest.PositionDataTypes.POIPosition);
		if (activate)
		{
			if (!base.OwnerQuest.CheckRequirements())
			{
				return;
			}
			if (!base.OwnerQuest.QuestClass.CanActivate())
			{
				GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("questunavailable"));
				return;
			}
			HandleParty();
			base.OwnerQuest.RemoveMapObject();
			base.OwnerQuest.RallyMarkerActivated = true;
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			if (flag)
			{
				base.OwnerQuest.OwnerJournal.ActiveQuest = base.OwnerQuest;
				base.OwnerQuest.Tracked = true;
				base.OwnerQuest.OwnerJournal.TrackedQuest = base.OwnerQuest;
				base.OwnerQuest.OwnerJournal.RefreshTracked();
				if (base.OwnerQuest.PositionData.ContainsKey(Quest.PositionDataTypes.TraderPosition))
				{
					base.OwnerQuest.OwnerJournal.AddPOIToTraderData(base.OwnerQuest.QuestClass.DifficultyTier, base.OwnerQuest.PositionData[Quest.PositionDataTypes.TraderPosition], base.OwnerQuest.PositionData[Quest.PositionDataTypes.POIPosition]);
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					GameManager.Instance.StartCoroutine(QuestEventManager.Current.QuestLockPOI(ownerPlayer.entityId, base.OwnerQuest.QuestClass, prefabPos, base.OwnerQuest.QuestTags, base.OwnerQuest.GetSharedWithIDList(), RallyPointActivated));
				}
				else
				{
					OutstandingRallyPoint = this;
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.LockPOI, ownerPlayer.entityId, base.OwnerQuest.ID, base.OwnerQuest.QuestTags, prefabPos, base.OwnerQuest.GetSharedWithIDList()));
				}
			}
			else
			{
				rallyPointActivated();
			}
			if (activateEvent != "")
			{
				GameEventManager.Current.HandleAction(activateEvent, null, ownerPlayer, twitchActivated: false, new Vector3i(prefabPos));
			}
			return;
		}
		EntityPlayer ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		switch (lockoutReason)
		{
		case QuestEventManager.POILockoutReasonTypes.Bedroll:
			GameManager.ShowTooltip(ownerPlayer2 as EntityPlayerLocal, Localization.Get("poiLockoutBedroll"));
			break;
		case QuestEventManager.POILockoutReasonTypes.LandClaim:
			GameManager.ShowTooltip(ownerPlayer2 as EntityPlayerLocal, Localization.Get("poiLockoutLandClaim"));
			break;
		case QuestEventManager.POILockoutReasonTypes.PlayerInside:
			GameManager.ShowTooltip(ownerPlayer2 as EntityPlayerLocal, Localization.Get("poiLockoutPlayerInside"));
			break;
		case QuestEventManager.POILockoutReasonTypes.QuestLock:
		{
			if (extraData == 0L)
			{
				GameManager.ShowTooltip(ownerPlayer2 as EntityPlayerLocal, Localization.Get("poiLockoutQuestActiveQuesters"));
				break;
			}
			(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(extraData);
			int item = tuple.Hours;
			int item2 = tuple.Minutes;
			GameManager.ShowTooltip(ownerPlayer2 as EntityPlayerLocal, Localization.Get("ttQuestLockedUntil"), $"{item:00}:{item2 + 1:00}");
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rallyPointActivated()
	{
		OutstandingRallyPoint = null;
		base.CurrentValue = 1;
		Refresh();
	}

	public void RallyPointActivated()
	{
		rallyPointActivated();
	}

	public bool IsActivated()
	{
		return currentValue == 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParty()
	{
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (ownerPlayer.Party != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyQuestChange>().Setup(ownerPlayer.entityId, base.OwnerQuest.GetObjectiveIndex(this), _isComplete: true, base.OwnerQuest.QuestCode), _onlyClientsAttachedToAnEntity: true);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyQuestChange>().Setup(ownerPlayer.entityId, base.OwnerQuest.GetObjectiveIndex(this), _isComplete: true, base.OwnerQuest.QuestCode));
			}
		}
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 1;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 HandleRallyPoint()
	{
		BlockValue blockValue = Block.GetBlockValue("questRallyMarker");
		if (RallyStartType == RallyStartTypes.Find)
		{
			if (!base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
			{
				return Vector3.zero;
			}
			if (!base.OwnerQuest.GetPositionData(out var pos, Quest.PositionDataTypes.POISize))
			{
				return Vector3.zero;
			}
			Vector3i prefabPosition = new Vector3i(position);
			int num = 32;
			outerRect = new Rect(position.x - (float)num, position.z - (float)num, pos.x + (float)(num * 2), pos.z + (float)(num * 2));
			if (base.OwnerQuest.GetPositionData(out var pos2, Quest.PositionDataTypes.Activate))
			{
				rallyPos = new Vector3i(pos2);
				position = pos2;
				positionSet = PositionSetTypes.RallyMarkerPosition;
			}
			else
			{
				World world = GameManager.Instance.World;
				if ((rallyPos = GetRallyPosition(world, prefabPosition, new Vector3i(pos))) != Vector3i.zero)
				{
					BlockValue block = world.GetBlock(rallyPos);
					if (!(block.Block is BlockRallyMarker))
					{
						return Vector3.zero;
					}
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, rallyPos.ToVector3());
					if (world.GetChunkFromWorldPos(rallyPos) is Chunk chunk)
					{
						BlockEntityData blockEntity = chunk.GetBlockEntity(rallyPos);
						if (blockEntity == null || !(blockEntity.transform != null) || block.type != blockValue.type)
						{
							world.SetBlockRPC(chunk.ClrIdx, rallyPos, blockValue);
							return Vector3.zero;
						}
						setVisibility(blockEntity.transform, rallyMarkerType, base.OwnerQuest.OwnerJournal.ActiveQuest == null);
						positionSet = PositionSetTypes.RallyMarkerPosition;
					}
				}
				else
				{
					rallyPos = new Vector3i(position);
					positionSet = PositionSetTypes.POIPosition;
				}
			}
		}
		else
		{
			base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.Location);
			if (base.OwnerQuest.GetPositionData(out var pos3, Quest.PositionDataTypes.Activate))
			{
				rallyPos = new Vector3i(pos3);
			}
			else
			{
				int num2 = (int)position.x;
				int num3 = (int)position.z;
				int height = GameManager.Instance.World.GetHeight(num2, num3);
				rallyPos = new Vector3i(num2, height, num3);
			}
			World world2 = GameManager.Instance.World;
			if (!(world2.GetChunkFromWorldPos(rallyPos) is Chunk chunk2))
			{
				rallyPos = new Vector3i(position);
				positionSet = PositionSetTypes.POIPosition;
			}
			else
			{
				BlockValue block2 = chunk2.GetBlock(World.toBlock(rallyPos));
				if (block2.ischild)
				{
					rallyPos = new Vector3i(rallyPos.x + block2.parentx, rallyPos.y + block2.parenty, rallyPos.z + block2.parentz);
					block2 = chunk2.GetBlock(World.toBlock(rallyPos));
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, rallyPos.ToVector3());
				}
				if (!(block2.Block is BlockRallyMarker))
				{
					if (!block2.isair && !block2.Block.IsTerrainDecoration)
					{
						rallyPos += Vector3i.up;
					}
					GameManager.Instance.World.SetBlockRPC(rallyPos, blockValue, sbyte.MaxValue);
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, rallyPos.ToVector3());
					return Vector3.zero;
				}
				BlockEntityData blockEntity2 = chunk2.GetBlockEntity(rallyPos);
				if (blockEntity2 == null || !(blockEntity2.transform != null) || block2.type != blockValue.type)
				{
					world2.SetBlockRPC(chunk2.ClrIdx, rallyPos, blockValue);
					return Vector3.zero;
				}
				setVisibility(blockEntity2.transform, rallyMarkerType, isVisible: true);
				positionSet = PositionSetTypes.RallyMarkerPosition;
			}
		}
		position = rallyPos.ToVector3();
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Activate, NavObjectName);
		return rallyPos.ToVector3();
	}

	public Vector3i GetRallyPosition(World _world, Vector3i _prefabPosition, Vector3i _prefabSize)
	{
		int num = World.toChunkXZ(_prefabPosition.x - 1);
		int num2 = World.toChunkXZ(_prefabPosition.x + _prefabSize.x + 1);
		int num3 = World.toChunkXZ(_prefabPosition.z - 1);
		int num4 = World.toChunkXZ(_prefabPosition.z + _prefabSize.z + 1);
		new List<Vector3i>();
		Rect rect = new Rect(_prefabPosition.x, _prefabPosition.z, _prefabSize.x, _prefabSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!(_world.GetChunkSync(i, j) is Chunk chunk))
				{
					continue;
				}
				List<Vector3i> list = chunk.IndexedBlocks["Rally"];
				if (list == null)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 vector = chunk.ToWorldPos(list[k]).ToVector3();
					if (rect.Contains(new Vector2(vector.x, vector.z)))
					{
						base.CurrentValue = 2;
						return chunk.ToWorldPos(list[k]);
					}
				}
			}
		}
		return Vector3i.zero;
	}

	public override BaseObjective Clone()
	{
		ObjectiveRallyPoint objectiveRallyPoint = new ObjectiveRallyPoint();
		CopyValues(objectiveRallyPoint);
		return objectiveRallyPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveRallyPoint obj = (ObjectiveRallyPoint)objective;
		obj.RallyStartType = RallyStartType;
		obj.startTime = startTime;
		obj.endTime = endTime;
		obj.activateEvent = activateEvent;
		obj.rallyMarkerType = rallyMarkerType;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		HandleRallyPoint();
		if (positionSet != PositionSetTypes.None)
		{
			base.CurrentValue = 2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (positionSet != PositionSetTypes.RallyMarkerPosition)
		{
			HandleRallyPoint();
			if (positionSet == PositionSetTypes.None)
			{
				return;
			}
		}
		if (positionSet == PositionSetTypes.RallyMarkerPosition)
		{
			GameManager.Instance.StartCoroutine(setRallyPointVisibility(base.OwnerQuest.OwnerJournal.ActiveQuest == null));
		}
		Vector3 vector = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		if (RallyStartType == RallyStartTypes.Create)
		{
			currentDistance = Vector3.Distance(vector, position + new Vector3(0.5f, 0f, 0.5f));
			if (currentDistance > distanceNeeded)
			{
				if (base.Description == "" || lastDistance <= distanceNeeded)
				{
					base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? textWaitForActivate : textHeadToRallyPoint);
				}
			}
			else
			{
				if (lastDistance > distanceNeeded)
				{
					setRallyPointVisible = true;
				}
				if (base.Description == "" || lastDistance > distanceNeeded)
				{
					base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? textWaitForActivate : textActivateRallyPoint);
				}
			}
			lastDistance = currentDistance;
			return;
		}
		currentDistance = Vector3.Distance(vector, position + new Vector3(0.5f, 0f, 0.5f));
		vector.y = vector.z;
		if (outerRect.Contains(vector))
		{
			if (base.Description == "" || lastDistance > distanceNeeded)
			{
				base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? textWaitForActivate : textActivateRallyPoint);
			}
			if (lastDistance > distanceNeeded)
			{
				setRallyPointVisible = true;
			}
			lastDistance = -1f;
		}
		else
		{
			if (lastDistance == -1f)
			{
				setRallyPointVisible = true;
				lastDistance = 1f;
			}
			if (base.Description == "" || lastDistance <= distanceNeeded)
			{
				base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? textWaitForActivate : textHeadToRallyPoint);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropRallyStartMode))
		{
			RallyStartType = EnumUtils.Parse<RallyStartTypes>(properties.Values[PropRallyStartMode]);
		}
		properties.ParseInt(PropAllowedStartTime, ref startTime);
		properties.ParseInt(PropAllowedEndTime, ref endTime);
		properties.ParseString(PropActivateEvent, ref activateEvent);
		properties.ParseString(PropRallyMarkerType, ref rallyMarkerType);
	}

	public void RallyPointRefresh()
	{
		GameManager.Instance.StartCoroutine(setRallyPointVisibility(visible: true));
	}
}
