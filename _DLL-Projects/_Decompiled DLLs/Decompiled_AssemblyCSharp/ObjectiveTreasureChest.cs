using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTreasureChest : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ContainerTypes
	{
		TreasureChest,
		Supplies
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum TreasureChestStates
	{
		NoPosition,
		WaitingForPoint,
		TryCreate,
		ValidateCreation,
		Created,
		Completed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ContainerTypes containerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue expectedBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue altExpectedBlockValue;

	public static int TreasureRadiusInitial = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDistance = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance = 50f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	public int DefaultTreasureRadius = 9;

	public int CurrentRadius = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public string DirectNavObjectName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string altBlockName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string radiusReductionSound = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int blocksPerReduction = 1;

	public int CurrentBlocksPerReduction = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int destroyCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastExplosionTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float explosionEventDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string explosionEvent = "";

	public static string PropBlock = "block";

	public static string PropAltBlock = "alt_block";

	public static string PropDistance = "distance";

	public static string PropContainerType = "container_type";

	public static string PropDefaultRadius = "default_radius";

	public static string PropDirectNavObject = "direct_nav_object";

	public static string PropBlocksPerReduction = "blocks_per_reduction";

	public static string PropRadiusReductionSound = "radius_reduction_sound";

	public static string PropUseNearby = "use_nearby";

	public static string PropExplosionEventDelay = "explosion_event_delay";

	public static string PropExplosionEvent = "explosion_event";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useNearby;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i neededContainerLocation = new Vector3i(-5000, -5000, -5000);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionAdjusted;

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject directNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoundaryProjectorTreasure projector;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameObject prefabProjector;

	public override ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.ObjectiveState != ObjectiveStates.Complete)
			{
				return ObjectiveValueTypes.Distance;
			}
			return ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override bool NeedsNPCSetPosition => !useNearby;

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (base.CurrentValue != 1)
				{
					if (currentDistance > 10f)
					{
						return ValueDisplayFormatters.Distance(currentDistance);
					}
					return Localization.Get("ObjectiveNearby_keyword");
				}
				return "";
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

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.craftingTag);
	}

	public override void SetupObjective()
	{
		keyword = ((containerType == ContainerTypes.TreasureChest) ? Localization.Get("ObjectiveTreasureChest_keyword") : Localization.Get("ObjectiveLocateSupplies_keyword"));
		expectedBlockValue = Block.GetBlockValue(ID);
		if (expectedBlockValue.isair)
		{
			Log.Error("ObjectiveTreasureChest: Invalid treasure container name.");
		}
		if (altBlockName != "")
		{
			altExpectedBlockValue = Block.GetBlockValue(altBlockName);
		}
		else
		{
			altExpectedBlockValue = BlockValue.Air;
		}
	}

	public override void SetupDisplay()
	{
		base.Description = keyword;
		StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager current = QuestEventManager.Current;
		current.AddObjectiveToBeUpdated(this);
		current.ContainerOpened += Current_ContainerOpened;
		current.BlockDestroy += Current_BlockDestroy;
		current.ExplosionDetected += Current_ExplosionDetected;
	}

	public override void RemoveHooks()
	{
		QuestEventManager current = QuestEventManager.Current;
		current.RemoveObjectiveToBeUpdated(this);
		current.ContainerOpened -= Current_ContainerOpened;
		current.BlockDestroy -= Current_BlockDestroy;
		current.ExplosionDetected -= Current_ExplosionDetected;
		if (projector != null)
		{
			Object.Destroy(projector.gameObject);
			projector = null;
		}
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		FinalizePoint((int)pos.x, (int)pos.y, (int)pos.z);
		return true;
	}

	public override void SetPosition(Vector3 position, Vector3 size)
	{
		FinalizePoint((int)position.x, (int)position.y, (int)position.z);
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		if (useNearby)
		{
			return false;
		}
		return GetPosition(ownerNPC);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetPosition(EntityNPC ownerNPC = null)
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.TreasurePoint) && base.OwnerQuest.GetPositionData(out offset, Quest.PositionDataTypes.TreasureOffset))
		{
			float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			value = Mathf.Clamp(value, 0f, DefaultTreasureRadius);
			_ = GameManager.Instance.World;
			Vector3 vector = position + offset * value;
			CurrentBlocksPerReduction = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			base.OwnerQuest.Position = vector;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
			base.CurrentValue = 2;
			if (useNearby && base.OwnerQuest.RallyMarkerActivated)
			{
				base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
			}
			return true;
		}
		EntityAlive entityAlive = ((ownerNPC == null) ? ((EntityAlive)base.OwnerQuest.OwnerJournal.OwnerPlayer) : ((EntityAlive)ownerNPC));
		if (Value != null && Value != "" && !StringParsers.TryParseFloat(Value, out distance) && Value.Contains("-"))
		{
			string[] array = Value.Split('-');
			float min = StringParsers.ParseFloat(array[0]);
			float maxExclusive = StringParsers.ParseFloat(array[1]);
			distance = GameManager.Instance.World.GetGameRandom().RandomRange(min, maxExclusive);
		}
		if (useNearby)
		{
			float num = Mathf.Clamp(EffectManager.GetValue(PassiveEffects.TreasureRadius, null, DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer), 0f, DefaultTreasureRadius) * 0.5f - 0.5f;
			distance = GameManager.Instance.World.GetGameRandom().RandomRange(num * -1f, num);
		}
		Vector3i _position = Vector3i.zero;
		CurrentBlocksPerReduction = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (base.OwnerQuest.Position == Vector3.zero)
		{
			base.OwnerQuest.Position = entityAlive.position;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (QuestEventManager.Current.GetTreasureContainerPosition(base.OwnerQuest.QuestCode, distance, DefaultTreasureRadius + 2, DefaultTreasureRadius, base.OwnerQuest.Position, entityAlive.entityId, useNearby, CurrentBlocksPerReduction, out CurrentBlocksPerReduction, out _position, out offset))
			{
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, offset);
				FinalizePoint(_position.x, _position.y, _position.z);
				return true;
			}
		}
		else
		{
			QuestEventManager.Current.GetTreasureContainerPosition(base.OwnerQuest.QuestCode, distance, DefaultTreasureRadius + 2, DefaultTreasureRadius, base.OwnerQuest.Position, entityAlive.entityId, useNearby, CurrentBlocksPerReduction, out CurrentBlocksPerReduction, out _position, out offset);
			base.CurrentValue = 1;
		}
		return false;
	}

	public static bool CalculateTreasurePoint(Vector3 startPosition, float distance, int offset, float treasureRadius, bool useNearby, out Vector3i position, out Vector3 treasureOffset)
	{
		World world = GameManager.Instance.World;
		treasureOffset = Vector3.zero;
		Vector3 treasureOffset2 = GetTreasureOffset(world);
		treasureOffset2.Normalize();
		Vector3 vector = startPosition + treasureOffset2 * (distance - 1f);
		position = new Vector3i(vector.x, (int)GameManager.Instance.World.GetHeightAt(vector.x, vector.z) - 3, vector.z);
		Vector3 vector2 = position.ToVector3();
		if (world.IsPositionInBounds(vector2) && world.IsEmptyPosition(position) && !world.IsPositionWithinPOI(vector2, offset) && !world.IsPositionRadiated(vector2))
		{
			if (!GameManager.Instance.World.CheckForLevelNearbyHeights(vector.x, vector.z, 5))
			{
				return false;
			}
			if (GameManager.Instance.World.GetWaterAt(vector.x, vector.z))
			{
				return false;
			}
			if (vector.y <= 10f)
			{
				return false;
			}
			if (useNearby)
			{
				treasureOffset = (startPosition - vector2) / treasureRadius;
				treasureOffset.Normalize();
			}
			else
			{
				treasureOffset = GetTreasureOffset(world);
				treasureOffset *= 0.9f;
			}
			return true;
		}
		return false;
	}

	public static Vector3 GetTreasureOffset(World world)
	{
		Vector2 randomInsideUnitCircle = world.GetGameRandom().RandomInsideUnitCircle;
		return new Vector3(randomInsideUnitCircle.x, 0f, randomInsideUnitCircle.y);
	}

	public void FinalizePoint(int x, int y, int z)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || base.CurrentValue == 1)
		{
			position = new Vector3(x, y, z);
			neededContainerLocation = new Vector3i(x, y, z);
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, position);
			if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
			{
				base.OwnerQuest.DataVariables["treasurecontainer"] = $"{x},{y},{z}";
			}
			else
			{
				base.OwnerQuest.DataVariables.Add("treasurecontainer", $"{x},{y},{z}");
			}
			if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
			{
				float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer);
				value = Mathf.Clamp(value, 0f, DefaultTreasureRadius);
				World world = GameManager.Instance.World;
				if (!base.OwnerQuest.GetPositionData(out offset, Quest.PositionDataTypes.TreasureOffset))
				{
					if (useNearby)
					{
						offset = (base.OwnerQuest.Position - position) / value;
						offset.Normalize();
					}
					else
					{
						offset = GetTreasureOffset(world);
					}
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, offset);
				}
				Vector3 vector = position + offset * (value - 1f);
				base.OwnerQuest.Position = vector;
				positionSet = true;
				base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
				base.CurrentValue = 2;
			}
			else
			{
				base.OwnerQuest.Position = position;
			}
		}
		else
		{
			if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.TreasurePoint))
			{
				return;
			}
			position = new Vector3(x, y, z);
			neededContainerLocation = new Vector3i(x, y, z);
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, position);
			if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
			{
				base.OwnerQuest.DataVariables["treasurecontainer"] = $"{x},{y},{z}";
			}
			else
			{
				base.OwnerQuest.DataVariables.Add("treasurecontainer", $"{x},{y},{z}");
			}
			if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
			{
				float value2 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer);
				value2 = Mathf.Clamp(value2, 0f, DefaultTreasureRadius);
				World world2 = GameManager.Instance.World;
				if (!base.OwnerQuest.GetPositionData(out offset, Quest.PositionDataTypes.TreasureOffset))
				{
					offset = GetTreasureOffset(world2);
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, offset);
				}
				Vector3 vector2 = position + offset * value2;
				base.OwnerQuest.Position = vector2;
				positionSet = true;
				base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
				base.CurrentValue = 2;
			}
			else
			{
				base.OwnerQuest.Position = position;
			}
		}
	}

	public void FinalizePointFromServer(int _blocksPerReduction, Vector3i _chestPos, Vector3 _treasureOffset)
	{
		CurrentBlocksPerReduction = _blocksPerReduction;
		position = _chestPos.ToVector3();
		neededContainerLocation = _chestPos;
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, position);
		if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
		{
			base.OwnerQuest.DataVariables["treasurecontainer"] = $"{_chestPos.x},{_chestPos.y},{_chestPos.z}";
		}
		else
		{
			base.OwnerQuest.DataVariables.Add("treasurecontainer", $"{_chestPos.x},{_chestPos.y},{_chestPos.z}");
		}
		if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
		{
			float value = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			value = Mathf.Clamp(value, 0f, DefaultTreasureRadius);
			_ = GameManager.Instance.World;
			offset = _treasureOffset;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, offset);
			Vector3 vector = position + offset * value;
			base.OwnerQuest.Position = vector;
			positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
			base.CurrentValue = 2;
		}
		else
		{
			base.OwnerQuest.Position = position;
			offset = _treasureOffset;
		}
	}

	public override void Update(float updateTime)
	{
		if (projector == null)
		{
			projector = CreateProjector().GetComponent<BoundaryProjectorTreasure>();
		}
		if (base.OwnerQuest.Active && base.OwnerQuest.MapObject == null && base.OwnerQuest.NavObject == null)
		{
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
		}
		switch ((TreasureChestStates)base.CurrentValue)
		{
		case TreasureChestStates.NoPosition:
			GetPosition();
			break;
		case TreasureChestStates.TryCreate:
		{
			EntityPlayer ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			position = new Vector3(position.x, ownerPlayer2.position.y, position.z);
			lastDistance = currentDistance;
			currentDistance = Vector3.Distance(ownerPlayer2.position, position + offset * CurrentRadius);
			if (!(currentDistance <= 20f))
			{
				break;
			}
			if (!positionAdjusted)
			{
				int num = (int)position.x;
				int num2 = (int)position.z;
				int num3 = GameManager.Instance.World.GetTerrainHeight(num, num2) - 3;
				Vector3i blockPos = new Vector3i(num, num3, num2);
				if (GameManager.Instance.World.GetChunkFromWorldPos(blockPos) != null)
				{
					positionAdjusted = true;
					position = new Vector3(num, num3, num2);
					neededContainerLocation = blockPos;
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, position);
				}
			}
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				if (!(GameManager.Instance.World.GetChunkFromWorldPos(neededContainerLocation) is Chunk))
				{
					break;
				}
				BlockValue block2 = GameManager.Instance.World.GetBlock(neededContainerLocation);
				if (block2.type != expectedBlockValue.type && (altExpectedBlockValue.isair || altExpectedBlockValue.type != block2.type))
				{
					GameManager.Instance.World.SetBlockRPC(neededContainerLocation, expectedBlockValue, sbyte.MaxValue);
				}
				if (useNearby)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						QuestEventManager.Current.SetTreasureContainerPosition(base.OwnerQuest.QuestCode, neededContainerLocation);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(base.OwnerQuest.QuestCode, neededContainerLocation));
					}
				}
				base.CurrentValue = 4;
				base.OwnerQuest.RallyMarkerActivated = true;
			}
			else if (GameManager.Instance.World.GetBlock(neededContainerLocation).type == expectedBlockValue.type)
			{
				base.CurrentValue = 4;
				base.OwnerQuest.RallyMarkerActivated = true;
			}
			break;
		}
		case TreasureChestStates.ValidateCreation:
		{
			if (GameManager.Instance.World.GetBlock(neededContainerLocation).type != expectedBlockValue.type)
			{
				base.CurrentValue = 2;
				break;
			}
			TileEntityLootContainer obj = (TileEntityLootContainer)GameManager.Instance.World.GetTileEntity(0, neededContainerLocation);
			obj.bPlayerBackpack = true;
			obj.SetModified();
			base.CurrentValue = 4;
			break;
		}
		case TreasureChestStates.Created:
		{
			if (!positionSet)
			{
				break;
			}
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			lastDistance = currentDistance;
			currentDistance = Vector2.Distance(new Vector2(ownerPlayer.position.x, ownerPlayer.position.z), new Vector2(position.x + offset.x * (float)CurrentRadius, position.z + offset.z * (float)CurrentRadius));
			if (currentDistance < 30f)
			{
				HandleNavObjects(ownerPlayer);
				if (!projector.IsInitialized || lastDistance >= 30f)
				{
					ownerPlayer.FireEvent(MinEventTypes.onTreasureRadiusEntered);
				}
				if (!projector.IsInitialized)
				{
					RadiusBoundsChanged();
					projector.IsInitialized = true;
					projector.gameObject.SetActive(value: true);
				}
				else if (lastDistance >= 30f)
				{
					RadiusBoundsChanged();
					projector.gameObject.SetActive(value: true);
				}
				if (originPos != Origin.position)
				{
					ResetProjectorPosition(CurrentRadius);
				}
				projector.WithinRadius = currentDistance <= projector.CurrentRadius;
				projector.transform.position = new Vector3(projector.transform.position.x, ownerPlayer.transform.position.y, projector.transform.position.z);
				if (GameManager.Instance.World.GetChunkFromWorldPos(neededContainerLocation) is Chunk chunk && chunk.IsDisplayed)
				{
					BlockValue block = GameManager.Instance.World.GetBlock(neededContainerLocation);
					if (block.type != expectedBlockValue.type && (altExpectedBlockValue.isair || altExpectedBlockValue.type != block.type))
					{
						base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
					}
				}
			}
			else if (currentDistance > 30f)
			{
				projector.gameObject.SetActive(value: false);
				if (lastDistance <= 30f)
				{
					ownerPlayer.FireEvent(MinEventTypes.onTreasureRadiusExited);
				}
			}
			break;
		}
		case TreasureChestStates.Completed:
			QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
			if (directNavObject != null)
			{
				NavObjectManager.Instance.UnRegisterNavObject(directNavObject);
				directNavObject = null;
			}
			base.OwnerQuest.OwnerJournal.OwnerPlayer.FireEvent(MinEventTypes.onTreasureRadiusCompleted);
			break;
		case TreasureChestStates.WaitingForPoint:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleNavObjects(EntityPlayer player)
	{
		float originalValue = CurrentRadius;
		originalValue = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, originalValue, player);
		originalValue = Mathf.Clamp(originalValue, 0f, originalValue);
		if (directNavObject == null)
		{
			directNavObject = NavObjectManager.Instance.RegisterNavObject(DirectNavObjectName, neededContainerLocation.ToVector3() + new Vector3(0.5f, 0f, 0.5f));
			if (base.OwnerQuest != null)
			{
				QuestClass questClass = base.OwnerQuest.QuestClass;
				directNavObject.name = questClass.Name;
			}
		}
		else
		{
			if (directNavObject.DisplayName == "" && base.OwnerQuest != null)
			{
				QuestClass questClass2 = base.OwnerQuest.QuestClass;
				directNavObject.name = questClass2.Name;
			}
			directNavObject.ExtraData = originalValue;
		}
		if (originalValue == 0f)
		{
			directNavObject.ForceDisabled = false;
			base.OwnerQuest.NavObject.ForceDisabled = true;
		}
		else
		{
			directNavObject.ForceDisabled = true;
			base.OwnerQuest.NavObject.ForceDisabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RadiusBoundsChanged()
	{
		if (!(projector == null))
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			float originalValue = CurrentRadius;
			originalValue = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, originalValue, ownerPlayer);
			originalValue = Mathf.Clamp(originalValue, 0f, originalValue);
			projector.SetRadius(0, originalValue);
			ResetProjectorPosition(originalValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetProjectorPosition(float radius)
	{
		projector.transform.position = position - Origin.position + offset * radius + Vector3.up * 4f;
		originPos = Origin.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (!base.Complete && entityId == -1 && neededContainerLocation.x == containerLocation.x && neededContainerLocation.y == containerLocation.y && neededContainerLocation.z == containerLocation.z && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 5;
			if (projector != null)
			{
				Object.Destroy(projector.gameObject);
				projector = null;
			}
			NavObjectManager.Instance.UnRegisterNavObject(directNavObject);
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockDestroy(Block block, Vector3i blockPos)
	{
		if (blocksPerReduction > 0 && !base.Complete && block.shape is BlockShapeTerrain)
		{
			Vector3 vector = position + offset * CurrentRadius;
			if (Vector3.Distance(new Vector3(blockPos.x, 0f, blockPos.z), new Vector3(vector.x, 0f, vector.z)) < (float)CurrentRadius + 0.5f && AddToDestroyCount())
			{
				HandleParty();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ExplosionDetected(Vector3 pos, int entityID, float blockDamage)
	{
		if (!(explosionEvent == "") && base.OwnerQuest.SharedOwnerID == -1 && !(lastExplosionTime > Time.time) && Vector3.Distance(position, pos) <= (float)(DefaultTreasureRadius * 2) && blockDamage > 1f && GameManager.Instance.World.GetEntity(entityID) is EntityPlayer)
		{
			GameEventManager.Current.HandleAction(explosionEvent, null, base.OwnerQuest.OwnerJournal.OwnerPlayer, twitchActivated: false, neededContainerLocation);
			lastExplosionTime = Time.time + explosionEventDelay;
		}
	}

	public bool AddToDestroyCount()
	{
		if (CurrentRadius != 0)
		{
			destroyCount++;
			int num = CurrentBlocksPerReduction;
			int num2 = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			if (num2 != num)
			{
				num = (CurrentBlocksPerReduction = num2);
				QuestEventManager.Current.UpdateTreasureBlocksPerReduction(base.OwnerQuest.QuestCode, num);
			}
			if (destroyCount >= num)
			{
				CurrentRadius--;
				destroyCount -= num;
			}
			if (CurrentRadius < 0)
			{
				CurrentRadius = 0;
			}
			if (CurrentRadius != lastRadius)
			{
				_ = GameManager.Instance.World;
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, offset);
				base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, NavObjectName, CurrentRadius);
				RadiusBoundsChanged();
				base.OwnerQuest.HandleQuestEvent(base.OwnerQuest, "TreasureRadiusReduction");
				GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ttTreasureRadiusReduced"));
				if (radiusReductionSound != "" && Vector3.Distance(base.OwnerQuest.OwnerJournal.OwnerPlayer.position, position + offset * CurrentRadius) <= (float)(DefaultTreasureRadius * 2))
				{
					Manager.PlayInsidePlayerHead(radiusReductionSound);
				}
				lastRadius = CurrentRadius;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParty()
	{
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (ownerPlayer.Party != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureRadiusBreak, ownerPlayer.entityId, base.OwnerQuest.QuestCode), _onlyClientsAttachedToAnEntity: true);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureRadiusBreak, ownerPlayer.entityId, base.OwnerQuest.QuestCode));
			}
		}
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 5;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveTreasureChest obj = (ObjectiveTreasureChest)objective;
		obj.containerType = containerType;
		obj.altBlockName = altBlockName;
		obj.altExpectedBlockValue = altExpectedBlockValue;
		obj.distance = distance;
		obj.DefaultTreasureRadius = DefaultTreasureRadius;
		obj.CurrentRadius = CurrentRadius;
		obj.DirectNavObjectName = DirectNavObjectName;
		obj.destroyCount = destroyCount;
		obj.lastRadius = lastRadius;
		obj.blocksPerReduction = blocksPerReduction;
		obj.radiusReductionSound = radiusReductionSound;
		obj.originPos = originPos;
		obj.useNearby = useNearby;
		obj.explosionEventDelay = explosionEventDelay;
		obj.explosionEvent = explosionEvent;
	}

	public override BaseObjective Clone()
	{
		ObjectiveTreasureChest objectiveTreasureChest = new ObjectiveTreasureChest();
		CopyValues(objectiveTreasureChest);
		return objectiveTreasureChest;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBlock))
		{
			ID = properties.Values[PropBlock];
		}
		if (properties.Values.ContainsKey(PropAltBlock))
		{
			altBlockName = properties.Values[PropAltBlock];
		}
		if (properties.Values.ContainsKey(PropDistance))
		{
			Value = properties.Values[PropDistance];
		}
		if (properties.Values.ContainsKey(PropContainerType))
		{
			containerType = EnumUtils.Parse<ContainerTypes>(properties.Values[PropContainerType]);
		}
		if (properties.Values.ContainsKey(PropDefaultRadius))
		{
			DefaultTreasureRadius = StringParsers.ParseSInt32(properties.Values[PropDefaultRadius]);
		}
		CurrentRadius = DefaultTreasureRadius;
		lastRadius = CurrentRadius;
		if (properties.Values.ContainsKey(PropDirectNavObject))
		{
			DirectNavObjectName = properties.Values[PropDirectNavObject];
		}
		if (properties.Values.ContainsKey(PropBlocksPerReduction))
		{
			blocksPerReduction = StringParsers.ParseSInt32(properties.Values[PropBlocksPerReduction]);
		}
		if (properties.Values.ContainsKey(PropRadiusReductionSound))
		{
			radiusReductionSound = properties.Values[PropRadiusReductionSound];
		}
		if (properties.Values.ContainsKey(PropUseNearby))
		{
			useNearby = StringParsers.ParseBool(properties.Values[PropUseNearby]);
		}
		properties.ParseFloat(PropExplosionEventDelay, ref explosionEventDelay);
		properties.ParseString(PropExplosionEvent, ref explosionEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject CreateProjector()
	{
		if (prefabProjector == null)
		{
			prefabProjector = Resources.Load<GameObject>("Prefabs/prefabBoundaryProjectorTreasure");
		}
		GameObject gameObject = Object.Instantiate(prefabProjector);
		gameObject.name = "Projector";
		gameObject.transform.position = new Vector3(-999f, -999f, -999f);
		return gameObject;
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		if (projector != null)
		{
			Object.Destroy(projector.gameObject);
			projector = null;
		}
		if (directNavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(directNavObject);
			directNavObject = null;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode));
		}
		base.OwnerQuest.OwnerJournal.OwnerPlayer.FireEvent(MinEventTypes.onTreasureRadiusCompleted);
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		if (projector != null)
		{
			Object.Destroy(projector.gameObject);
			projector = null;
		}
		if (directNavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(directNavObject);
			directNavObject = null;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode));
		}
		base.OwnerQuest.OwnerJournal.OwnerPlayer.FireEvent(MinEventTypes.onTreasureRadiusCompleted);
	}

	public override void Read(BinaryReader _br)
	{
		destroyCount = _br.ReadInt32();
		CurrentRadius = _br.ReadInt32();
		lastRadius = CurrentRadius;
	}

	public override void Write(BinaryWriter _bw)
	{
		_bw.Write(destroyCount);
		_bw.Write(CurrentRadius);
	}

	public override string ParseBinding(string bindingName)
	{
		_ = ID;
		_ = Value;
		if (!(bindingName == "distance"))
		{
			if (bindingName == "direction" && base.OwnerQuest.QuestGiverID != -1)
			{
				EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
				if (entityNPC != null)
				{
					if (useNearby)
					{
						Vector3 vector = base.OwnerQuest.Position;
						vector.y = 0f;
						Vector3 vector2 = entityNPC.position;
						vector2.y = 0f;
						return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(vector.x - vector2.x, vector.z - vector2.z)));
					}
					position.y = 0f;
					Vector3 vector3 = entityNPC.position;
					vector3.y = 0f;
					return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(position.x - vector3.x, position.z - vector3.z)));
				}
			}
		}
		else if (base.OwnerQuest.QuestGiverID != -1)
		{
			EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
			if (entityNPC2 != null)
			{
				if (useNearby)
				{
					Vector3 vector4 = base.OwnerQuest.Position;
					vector4.y = 0f;
					vector4.y = 0f;
					Vector3 a = entityNPC2.position;
					a.y = 0f;
					currentDistance = Vector3.Distance(a, vector4 + offset);
					return ValueDisplayFormatters.Distance(currentDistance);
				}
				position.y = 0f;
				Vector3 a2 = entityNPC2.position;
				a2.y = 0f;
				currentDistance = Vector3.Distance(a2, position + offset);
				return ValueDisplayFormatters.Distance(currentDistance);
			}
		}
		return "";
	}
}
