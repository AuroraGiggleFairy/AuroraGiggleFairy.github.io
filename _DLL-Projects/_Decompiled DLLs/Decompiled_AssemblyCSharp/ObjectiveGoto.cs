using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveGoto : BaseObjective
{
	public static string PropAllowCurrentPOI = "allow_current_poi";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowCurrentPoi;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance = 20f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distanceOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string locationVariable = "gotolocation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string biomeFilter = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string locationName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool poiNotFound;

	public static string PropLocation = "location_tag";

	public static string PropDistance = "distance";

	public static string PropBiomeFilterType = "biome_filter_type";

	public static string PropBiomeFilter = "biome_filter";

	public static string PropLocationName = "location_name";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Distance;

	public override bool NeedsNPCSetPosition => true;

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
			if (poiNotFound && base.OwnerQuest.Position == Vector3.zero)
			{
				return "NO TRADER";
			}
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

	public override void SetupObjective()
	{
		if (ID == "trader")
		{
			Localization.Get("xuiTrader");
		}
		keyword = string.Format(Localization.Get("ObjectiveGoto_keyword"), Localization.Get(locationName));
		distance = StringParsers.ParseFloat(Value);
		SetupIcon();
		if (base.OwnerQuest.Active)
		{
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
		}
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
		StatusText = "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupIcon()
	{
		if (ID.EqualsCaseInsensitive("trader"))
		{
			icon = "ui_game_symbol_map_trader";
		}
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return GetPosition(ownerNPC, player, usedPOILocations, entityIDforQuests) != Vector3.zero;
	}

	public override void SetPosition(Vector3 POIPosition, Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			distanceOffset = POISize.x;
		}
		else
		{
			distanceOffset = POISize.z;
		}
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		position = GetMidPOIPosition(POIPosition, POISize);
		base.OwnerQuest.Position = position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 GetMidPOIPosition(Vector3 poiPosition, Vector3 poiSize)
	{
		int num = (int)(poiPosition.x + poiSize.x / 2f);
		int num2 = (int)(poiPosition.z + poiSize.z / 2f);
		int num3 = (int)GameManager.Instance.World.GetHeightAt(num, num2);
		return new Vector3(num, num3, num2);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		EntityAlive entityAlive = ((ownerNPC == null) ? ((EntityAlive)base.OwnerQuest.OwnerJournal.OwnerPlayer) : ((EntityAlive)ownerNPC));
		if (entityPlayer == null)
		{
			entityPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		}
		int traderId = ((ownerNPC == null) ? (-1) : ownerNPC.entityId);
		int playerId = ((entityPlayer == null) ? (-1) : entityPlayer.entityId);
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		if (!string.IsNullOrEmpty(ID))
		{
			fastTags = FastTags<TagGroup.Global>.Parse(ID);
		}
		Vector3 pos = Vector3.zero;
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition) && base.OwnerQuest.GetPositionData(out pos, Quest.PositionDataTypes.POISize))
		{
			position = GetMidPOIPosition(position, pos);
			base.OwnerQuest.Position = position;
			positionSet = true;
			if (pos.x > pos.z)
			{
				distanceOffset = pos.x;
			}
			else
			{
				distanceOffset = pos.z;
			}
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PrefabInstance prefabInstance = null;
			if (prefabInstance == null)
			{
				prefabInstance = base.OwnerQuest.QuestPrefab;
			}
			int factionID = ((ownerNPC != null) ? ownerNPC.NPCInfo.QuestFaction : base.OwnerQuest.QuestFaction);
			usedPOILocations = ((entityPlayer != null) ? entityPlayer.QuestJournal.GetTraderList(factionID) : null);
			if (prefabInstance == null)
			{
				string questKey = "";
				if (base.OwnerQuest != null && base.OwnerQuest.QuestClass != null)
				{
					questKey = base.OwnerQuest.QuestClass.UniqueKey;
				}
				prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(fastTags, new Vector2(entityAlive.position.x, entityAlive.position.z), usedPOILocations, -1, !allowCurrentPoi, biomeFilterType, biomeFilter, questKey);
				if (prefabInstance == null)
				{
					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(fastTags, new Vector2(entityAlive.position.x, entityAlive.position.z), usedPOILocations, -1, !allowCurrentPoi, BiomeFilterTypes.SameBiome, "", questKey);
				}
			}
			if (prefabInstance == null)
			{
				return Vector3.zero;
			}
			Vector2 vector = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
			if (vector.x == -0.1f && vector.y == -0.1f)
			{
				Log.Error("ObjectiveGoto: No Trader found.");
				return Vector3.zero;
			}
			int num = (int)vector.x;
			int num2 = (int)vector.y;
			int num3 = (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y);
			position = new Vector3(num, num3, num2);
			if (GameManager.Instance.World.IsPositionInBounds(position))
			{
				base.OwnerQuest.Position = position;
				FinalizePoint(new Vector3(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.y, prefabInstance.boundingBoxPosition.z), new Vector3(prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.y, prefabInstance.boundingBoxSize.z));
				base.OwnerQuest.QuestPrefab = prefabInstance;
				base.OwnerQuest.DataVariables.Add("POIName", Localization.Get(base.OwnerQuest.QuestPrefab.location.Name));
				return position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(traderId, playerId, fastTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.Trader, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1f, biomeFilterType, biomeFilter));
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(Vector3 POIPosition, Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			distanceOffset = POISize.x;
		}
		else
		{
			distanceOffset = POISize.z;
		}
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		position = GetMidPOIPosition(POIPosition, POISize);
		positionSet = true;
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
		if (GameSparksCollector.CollectGamePlayData && base.OwnerQuest.QuestClass.ID == "quest_whiterivercitizen1")
		{
			GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestStarterTraderDistance, ((int)Vector3.Distance(position, base.OwnerQuest.OwnerJournal.OwnerPlayer.position) / 50 * 50).ToString(), 1);
		}
		base.CurrentValue = 2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		int entityIDforQuests = -1;
		if (base.OwnerQuest != null)
		{
			entityIDforQuests = base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId;
		}
		if (GetPosition(null, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, entityIDforQuests) != Vector3.zero)
		{
			poiNotFound = false;
		}
		else
		{
			poiNotFound = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (!positionSet)
		{
			GetPosition();
			return;
		}
		if (position.y == 0f)
		{
			position.y = (int)GameManager.Instance.World.GetHeightAt(position.x, position.z);
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != position)
		{
			base.OwnerQuest.NavObject.TrackedPosition = position;
		}
		currentDistance = Vector3.Distance(ownerPlayer.position, position);
		if (currentDistance < distance + distanceOffset && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 3;
			Refresh();
		}
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 3;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveGoto objectiveGoto = new ObjectiveGoto();
		CopyValues(objectiveGoto);
		return objectiveGoto;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveGoto obj = (ObjectiveGoto)objective;
		obj.distance = distance;
		obj.biomeFilterType = biomeFilterType;
		obj.biomeFilter = biomeFilter;
		obj.locationName = locationName;
		obj.allowCurrentPoi = allowCurrentPoi;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		FinalizePoint(pos, size);
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropDistance))
		{
			Value = properties.Values[PropDistance];
			distance = StringParsers.ParseFloat(Value);
		}
		if (properties.Values.ContainsKey(PropAllowCurrentPOI))
		{
			allowCurrentPoi = StringParsers.ParseBool(properties.Values[PropAllowCurrentPOI]);
		}
		properties.ParseString(PropLocation, ref ID);
		properties.ParseEnum(PropBiomeFilterType, ref biomeFilterType);
		properties.ParseString(PropBiomeFilter, ref biomeFilter);
		properties.ParseString(PropLocationName, ref locationName);
	}

	public override void HandleCompleted()
	{
		if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.GetPositionData(out var pos, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.OwnerJournal.AddTraderPOI(new Vector2(pos.x, pos.z), base.OwnerQuest.QuestFaction);
		}
	}

	public override string ParseBinding(string bindingName)
	{
		_ = ID;
		_ = Value;
		switch (bindingName)
		{
		case "location":
			return ID;
		case "name":
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (base.OwnerQuest.DataVariables.ContainsKey("POIName"))
				{
					return base.OwnerQuest.DataVariables["POIName"];
				}
				if (base.OwnerQuest.QuestPrefab == null)
				{
					return "";
				}
				return Localization.Get(base.OwnerQuest.QuestPrefab.location.Name);
			}
			if (!base.OwnerQuest.DataVariables.ContainsKey("POIName"))
			{
				return "";
			}
			return base.OwnerQuest.DataVariables["POIName"];
		case "distance":
			if (base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC2 != null)
				{
					Vector3 a = entityNPC2.position;
					currentDistance = Vector3.Distance(a, position);
					return ValueDisplayFormatters.Distance(currentDistance);
				}
			}
			break;
		case "direction":
			if (base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC3 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC3 != null)
				{
					position.y = 0f;
					Vector3 vector2 = entityNPC3.position;
					vector2.y = 0f;
					return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(position.x - vector2.x, position.z - vector2.z)));
				}
			}
			break;
		case "directionfull":
			if (base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC != null)
				{
					position.y = 0f;
					Vector3 vector = entityNPC.position;
					vector.y = 0f;
					return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(position.x - vector.x, position.z - vector.z)), _useLongGeoDirs: true);
				}
			}
			break;
		}
		return "";
	}
}
