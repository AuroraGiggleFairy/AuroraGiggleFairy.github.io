using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRandomGoto : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum GotoStates
	{
		NoPosition,
		WaitingForPoint,
		TryComplete,
		Completed
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float completionDistance = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string biomeFilter = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public new float updateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool completeWithinRange = true;

	public static string PropDistance = "distance";

	public static string PropCompletionDistance = "completion_distance";

	public static string PropBiomeFilterType = "biome_filter_type";

	public static string PropBiomeFilter = "biome_filter";

	public override ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return ObjectiveValueTypes.Distance;
			}
			return ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (!positionSet)
				{
					return "--";
				}
				return ValueDisplayFormatters.Distance(distance);
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
		keyword = Localization.Get("ObjectiveRallyPointHeadTo");
		SetupIcon();
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, NavObjectName);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupIcon()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.Location))
		{
			base.OwnerQuest.Position = position;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.TreasurePoint))
		{
			positionSet = true;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, base.OwnerQuest.Position);
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, NavObjectName);
			base.CurrentValue = 2;
			return position;
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		float num = 50f;
		float num2 = 50f;
		if (Value != null && Value != "")
		{
			if (StringParsers.TryParseFloat(Value, out distance))
			{
				num = (num2 = distance);
			}
			else if (Value.Contains("-"))
			{
				string[] array = Value.Split('-');
				num = StringParsers.ParseFloat(array[0]);
				num2 = StringParsers.ParseFloat(array[1]);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			distance = GameManager.Instance.World.GetGameRandom().RandomFloat * (num2 - num) + num;
			Vector3i vector3i = CalculateRandomPoint(ownerPlayer.entityId, distance, base.OwnerQuest.ID, canBeWithinPOI: false, biomeFilterType, biomeFilter);
			if (!GameManager.Instance.World.CheckForLevelNearbyHeights(vector3i.x, vector3i.z, 5) || GameManager.Instance.World.GetWaterAt(vector3i.x, vector3i.z))
			{
				return Vector3.zero;
			}
			World world = GameManager.Instance.World;
			if (vector3i.y > 0 && world.IsPositionInBounds(vector3i) && !world.IsPositionWithinPOI(vector3i, 5))
			{
				FinalizePoint(vector3i.x, vector3i.y, vector3i.z);
				return position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(ownerPlayer.entityId, distance, 1, base.OwnerQuest.QuestCode));
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	public static Vector3i CalculateRandomPoint(int entityID, float distance, string questID, bool canBeWithinPOI = false, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		World world = GameManager.Instance.World;
		EntityAlive entityAlive = world.GetEntity(entityID) as EntityAlive;
		Vector3 vector = new Vector3(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		Vector3 vector2 = entityAlive.position + vector * distance;
		int x = (int)vector2.x;
		int z = (int)vector2.z;
		int y = (int)world.GetHeightAt(vector2.x, vector2.z);
		Vector3i vector3i = new Vector3i(x, y, z);
		Vector3 vector3 = new Vector3(vector3i.x, vector3i.y, vector3i.z);
		if (world.IsPositionInBounds(vector3) && (!(entityAlive is EntityPlayer) || world.CanPlaceBlockAt(vector3i, GameManager.Instance.GetPersistentLocalPlayer())) && (canBeWithinPOI || !world.IsPositionWithinPOI(vector3, 2)))
		{
			if (!world.CheckForLevelNearbyHeights(vector2.x, vector2.z, 5) || world.GetWaterAt(vector2.x, vector2.z))
			{
				return new Vector3i(0, -99999, 0);
			}
			if (biomeFilterType != BiomeFilterTypes.AnyBiome)
			{
				string[] array = null;
				BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector2.x, (int)vector2.z);
				switch (biomeFilterType)
				{
				case BiomeFilterTypes.OnlyBiome:
					if (biomeAt.m_sBiomeName != biomeFilter)
					{
						return new Vector3i(0, -99999, 0);
					}
					break;
				case BiomeFilterTypes.ExcludeBiome:
				{
					if (array == null)
					{
						array = biomeFilter.Split(',');
					}
					bool flag = false;
					for (int i = 0; i < array.Length; i++)
					{
						if (biomeAt.m_sBiomeName == array[i])
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						return new Vector3i(0, -99999, 0);
					}
					break;
				}
				case BiomeFilterTypes.SameBiome:
				{
					BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)entityAlive.position.x, (int)entityAlive.position.z);
					if (biomeAt != biomeAt2)
					{
						return new Vector3i(0, -99999, 0);
					}
					break;
				}
				}
			}
			return vector3i;
		}
		return new Vector3i(0, -99999, 0);
	}

	public static Vector3i CalculateRandomPositionFromFlatAreas(int entityID, float minDistance, float maxDistance, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		World world = GameManager.Instance.World;
		EntityAlive entityAlive = world.GetEntity(entityID) as EntityAlive;
		if (biomeFilterType == BiomeFilterTypes.SameBiome)
		{
			biomeFilter = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)entityAlive.position.x, (int)entityAlive.position.z).m_sBiomeName;
		}
		List<FlatArea> areasWithinRange = GameManager.Instance.World.FlatAreaManager.GetAreasWithinRange(entityAlive.position, minDistance, maxDistance, eFlatAreaSizeFilter.All, biomeFilterType, biomeFilter.Split(','));
		if (areasWithinRange.Count > 0)
		{
			return Vector3i.FromVector3Rounded(areasWithinRange[world.GetGameRandom().RandomRange(areasWithinRange.Count)].GetRandomPosition(1f));
		}
		return new Vector3i(0, -99999, 0);
	}

	public void FinalizePoint(int x, int y, int z)
	{
		position = new Vector3(x, y, z);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, position);
		base.OwnerQuest.Position = position;
		positionSet = true;
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, NavObjectName);
		base.CurrentValue = 2;
	}

	public override void Update(float deltaTime)
	{
		if (!(Time.time > updateTime))
		{
			return;
		}
		updateTime = Time.time + 1f;
		if (!positionSet && base.CurrentValue != 1)
		{
			_ = GetPosition() != Vector3.zero;
			OnStart();
		}
		switch ((GotoStates)base.CurrentValue)
		{
		case GotoStates.NoPosition:
			_ = GetPosition() != Vector3.zero;
			break;
		case GotoStates.TryComplete:
		{
			EntityPlayerLocal ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != position)
			{
				base.OwnerQuest.NavObject.TrackedPosition = position;
			}
			Vector3 a2 = ownerPlayer2.position;
			distance = Vector3.Distance(a2, position);
			if (distance < completionDistance && base.OwnerQuest.CheckRequirements())
			{
				base.CurrentValue = 3;
				Refresh();
			}
			break;
		}
		case GotoStates.Completed:
		{
			if (completeWithinRange)
			{
				QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
				break;
			}
			EntityPlayerLocal ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != position)
			{
				base.OwnerQuest.NavObject.TrackedPosition = position;
			}
			Vector3 a = ownerPlayer.position;
			distance = Vector3.Distance(a, position);
			if (distance > completionDistance)
			{
				base.CurrentValue = 2;
				Refresh();
			}
			break;
		}
		case GotoStates.WaitingForPoint:
			break;
		}
	}

	public virtual void OnStart()
	{
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 3;
		base.Complete = complete;
		if (base.Complete)
		{
			base.ObjectiveState = ObjectiveStates.Complete;
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, PlayObjectiveComplete);
			base.OwnerQuest.RemoveMapObject();
			RemoveHooks();
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveRandomGoto objectiveRandomGoto = new ObjectiveRandomGoto();
		CopyValues(objectiveRandomGoto);
		objectiveRandomGoto.position = position;
		objectiveRandomGoto.positionSet = positionSet;
		objectiveRandomGoto.completionDistance = completionDistance;
		objectiveRandomGoto.biomeFilter = biomeFilter;
		objectiveRandomGoto.biomeFilterType = biomeFilterType;
		return objectiveRandomGoto;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		FinalizePoint((int)pos.x, (int)pos.y, (int)pos.z);
		return true;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropDistance, ref Value);
		properties.ParseFloat(PropCompletionDistance, ref completionDistance);
		properties.ParseEnum(PropBiomeFilterType, ref biomeFilterType);
		properties.ParseString(PropBiomeFilter, ref biomeFilter);
	}
}
