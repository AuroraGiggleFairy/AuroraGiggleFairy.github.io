using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRandomPOIGoto : ObjectiveGoto
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int poiTier = -1;

	public static string PropPOITier = "poi_tier";

	public int POITier
	{
		get
		{
			if (poiTier != -1)
			{
				return poiTier;
			}
			return base.OwnerQuest.QuestClass.DifficultyTier;
		}
		set
		{
			poiTier = value;
		}
	}

	public override bool NeedsNPCSetPosition => true;

	public override bool PlayObjectiveComplete => false;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveRallyPointHeadTo");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		icon = "ui_game_symbol_quest";
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return GetPosition(ownerNPC, player, usedPOILocations, entityIDforQuests) != Vector3.zero;
	}

	public override void AddHooks()
	{
		base.AddHooks();
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDistanceOffset(Vector3 POISize)
	{
		if (POISize.x > POISize.z)
		{
			distanceOffset = POISize.x;
		}
		else
		{
			distanceOffset = POISize.z;
		}
	}

	public override void SetPosition(Vector3 POIPosition, Vector3 POISize)
	{
		SetDistanceOffset(POISize);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		base.OwnerQuest.Position = POIPosition;
		position = POIPosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		int traderId = ((ownerNPC == null) ? (-1) : ownerNPC.entityId);
		int playerId = ((entityPlayer == null) ? (-1) : entityPlayer.entityId);
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.GetPositionData(out var pos, Quest.PositionDataTypes.POISize);
			Vector2 vector = new Vector2(position.x + pos.x / 2f, position.z + pos.z / 2f);
			int num = (int)vector.x;
			int num2 = (int)vector.y;
			int num3 = (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y);
			position = new Vector3(num, num3, num2);
			base.OwnerQuest.Position = position;
			SetDistanceOffset(pos);
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		EntityAlive entityAlive = entityPlayer;
		if (entityAlive == null)
		{
			entityAlive = ((ownerNPC == null) ? ((EntityAlive)base.OwnerQuest.OwnerJournal.OwnerPlayer) : ((EntityAlive)ownerNPC));
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PrefabInstance prefabInstance = null;
			prefabInstance = ((!(ownerNPC != null)) ? GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 1000, 4000000, base.OwnerQuest.QuestTags, (byte)POITier, usedPOILocations, entityIDforQuests, biomeFilterType, biomeFilter) : GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearTrader(ownerNPC as EntityTrader, base.OwnerQuest.QuestTags, (byte)POITier, usedPOILocations, entityIDforQuests, biomeFilterType, biomeFilter));
			if (prefabInstance == null)
			{
				return Vector3.zero;
			}
			if (prefabInstance != null)
			{
				Vector2 vector2 = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (vector2.x == -0.1f && vector2.y == -0.1f)
				{
					Log.Error("ObjectiveRandomGoto: No POI found.");
					return Vector3.zero;
				}
				int num4 = (int)vector2.x;
				int num5 = (int)GameManager.Instance.World.GetHeightAt(vector2.x, vector2.y);
				int num6 = (int)vector2.y;
				position = new Vector3(num4, num5, num6);
				if (GameManager.Instance.World.IsPositionInBounds(position))
				{
					base.OwnerQuest.Position = position;
					FinalizePoint(new Vector3(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.y, prefabInstance.boundingBoxPosition.z), new Vector3(prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.y, prefabInstance.boundingBoxSize.z));
					base.OwnerQuest.QuestPrefab = prefabInstance;
					base.OwnerQuest.DataVariables.Add("POIName", Localization.Get(base.OwnerQuest.QuestPrefab.location.Name));
					usedPOILocations?.Add(new Vector2(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z));
					return position;
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(traderId, playerId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI, (byte)POITier, 0, -1, 0f, 0f, 0f, -1f, biomeFilterType, biomeFilter));
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public override BaseObjective Clone()
	{
		ObjectiveRandomPOIGoto objectiveRandomPOIGoto = new ObjectiveRandomPOIGoto();
		CopyValues(objectiveRandomPOIGoto);
		objectiveRandomPOIGoto.poiTier = poiTier;
		objectiveRandomPOIGoto.distance = distance;
		objectiveRandomPOIGoto.biomeFilterType = biomeFilterType;
		objectiveRandomPOIGoto.biomeFilter = biomeFilter;
		objectiveRandomPOIGoto.locationName = locationName;
		return objectiveRandomPOIGoto;
	}

	public override string ParseBinding(string bindingName)
	{
		_ = ID;
		_ = Value;
		switch (bindingName)
		{
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
				EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC != null)
				{
					position.y = 0f;
					Vector3 vector = entityNPC.position;
					vector.y = 0f;
					return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(position.x - vector.x, position.z - vector.z)));
				}
			}
			break;
		}
		return "";
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseInt(PropPOITier, ref poiTier);
	}
}
