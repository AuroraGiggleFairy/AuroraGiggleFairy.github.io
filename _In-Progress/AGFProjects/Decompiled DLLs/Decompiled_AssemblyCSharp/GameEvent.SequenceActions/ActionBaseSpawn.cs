using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseSpawn : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum SpawnTypes
	{
		NearTarget,
		Position,
		NearPosition,
		WanderingHorde
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum SpawnUpdateTypes
	{
		NeedSpawnEntries,
		NeedPosition,
		SpawnEntities
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class SpawnEntry
	{
		public Entity Target;

		public int EntityTypeID;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string entityNames = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string countText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int count = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool singleChoice;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float minDistance = 8f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxDistance = 12f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool safeSpawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AddToGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] AddToGroups;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool attackTarget = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool airSpawn;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasSpawned;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool clearPositionOnComplete;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnTypes spawnType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float yOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string partyAdditionText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool useEntityGroup;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spawnSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEntityNames = "entity_names";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEntityGroup = "entity_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSingleChoice = "single_choice";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnCount = "spawn_count";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinDistance = "min_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDistance = "max_distance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnInSafe = "safe_spawn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAddToGroup = "add_to_group";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAttackTarget = "attack_target";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnInAir = "air_spawn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnType = "spawn_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPartyAddition = "party_addition";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropClearPositionOnComplete = "clear_position_on_complete";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIgnoreSpawnMultiplier = "ignore_multiplier";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropYOffset = "yoffset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRaycastOffset = "raycast_offset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsAggressive = "is_aggressive";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpawnSound = "spawn_sound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<int> entityIDs = new List<int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntityIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float resetTime = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ignoreMultiplier;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float raycastOffset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAggressive = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnUpdateTypes CurrentState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<SpawnEntry> SpawnEntries;

	public virtual bool UseRepeating
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int ModifiedCount(Entity target)
	{
		if (target is EntityAlive entityAlive)
		{
			if (count == -1)
			{
				count = GameEventManager.GetIntValue(entityAlive, countText, 1);
			}
			if (entityAlive is EntityPlayer { Party: not null } entityPlayer)
			{
				int num = count;
				if (!UseRepeating)
				{
					num += GetPartyAdditionCount(entityPlayer);
				}
				if (base.Owner.ActionType != GameEventActionSequence.ActionTypes.Game && !ignoreMultiplier)
				{
					num = (int)EffectManager.GetValue(PassiveEffects.TwitchSpawnMultiplier, null, num, entityPlayer);
				}
				return num;
			}
			if (base.Owner.ActionType != GameEventActionSequence.ActionTypes.Game && !ignoreMultiplier)
			{
				return (int)EffectManager.GetValue(PassiveEffects.TwitchSpawnMultiplier, null, count, entityAlive);
			}
			return count;
		}
		return count;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetPartyAdditionCount(EntityPlayer player)
	{
		if (player.Party != null)
		{
			int intValue = GameEventManager.GetIntValue(player, partyAdditionText);
			return (player.Party.MemberList.Count - 1) * intValue;
		}
		return 0;
	}

	public override bool CanPerform(Entity player)
	{
		count = GameEventManager.GetIntValue(player as EntityAlive, countText, 1);
		if (!useEntityGroup && entityIDs.Count == 0)
		{
			Debug.LogWarning("Error: GameEventSequence missing spawn type: " + base.Owner.Name);
			return false;
		}
		if (player != null && player.IsDead())
		{
			return false;
		}
		if (GameEventManager.Current.CurrentCount + count > GameEventManager.Current.MaxSpawnCount)
		{
			return false;
		}
		if (!safeSpawn)
		{
			if (player != null && !GameManager.Instance.World.CanPlaceBlockAt(new Vector3i(player.position), null))
			{
				return false;
			}
			if (player == null && !GameManager.Instance.World.CanPlaceBlockAt(new Vector3i(base.Owner.TargetPosition), null))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		SetupEntityIDs();
		AddToGroups = AddToGroup.Split(',');
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupEntityIDs()
	{
		if (useEntityGroup)
		{
			entityIDs.Clear();
			return;
		}
		string[] array = entityNames.Split(',');
		entityIDs.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
			{
				if (item.Value.entityClassName == array[i])
				{
					entityIDs.Add(item.Key);
					if (entityIDs.Count == array.Length)
					{
						break;
					}
				}
			}
		}
	}

	public override ActionCompleteStates OnPerformAction()
	{
		if (!base.Owner.HasTarget())
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		HandleExtraAction();
		switch (CurrentState)
		{
		case SpawnUpdateTypes.NeedSpawnEntries:
		{
			if (SpawnEntries != null)
			{
				break;
			}
			if (!useEntityGroup && entityIDs.Count == 0)
			{
				SetupEntityIDs();
				return ActionCompleteStates.InComplete;
			}
			SpawnEntries = new List<SpawnEntry>();
			if (singleChoice && selectedEntityIndex == -1)
			{
				selectedEntityIndex = Random.Range(0, entityIDs.Count);
			}
			GameStageDefinition gameStageDefinition = null;
			int lastClassId = -1;
			if (useEntityGroup)
			{
				gameStageDefinition = GameStageDefinition.GetGameStage(entityNames);
				if (gameStageDefinition == null)
				{
					return ActionCompleteStates.InCompleteRefund;
				}
			}
			if (targetGroup != "")
			{
				List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
				if (entityGroup != null)
				{
					for (int k = 0; k < entityGroup.Count; k++)
					{
						if (!(entityGroup[k] is EntityPlayer entityPlayer) || (base.Owner.ActionType == GameEventActionSequence.ActionTypes.TwitchAction && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled))
						{
							continue;
						}
						int num = ModifiedCount(entityPlayer);
						_ = GameManager.Instance.World;
						for (int l = 0; l < num; l++)
						{
							if (useEntityGroup)
							{
								int randomFromGroup = EntityGroups.GetRandomFromGroup(gameStageDefinition.GetStage(entityPlayer.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId);
								SpawnEntries.Add(new SpawnEntry
								{
									EntityTypeID = randomFromGroup,
									Target = entityPlayer
								});
							}
							else
							{
								int index = ((selectedEntityIndex == -1) ? Random.Range(0, entityIDs.Count) : selectedEntityIndex);
								SpawnEntries.Add(new SpawnEntry
								{
									EntityTypeID = entityIDs[index],
									Target = entityPlayer
								});
							}
						}
						if (attackTarget)
						{
							base.Owner.ReservedSpawnCount += num;
							GameEventManager.Current.ReservedCount += num;
						}
					}
				}
				else
				{
					int num2 = ModifiedCount(base.Owner.Target);
					for (int m = 0; m < num2; m++)
					{
						if (useEntityGroup)
						{
							if (base.Owner.Target is EntityPlayer entityPlayer2)
							{
								int randomFromGroup2 = EntityGroups.GetRandomFromGroup(gameStageDefinition.GetStage(entityPlayer2.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId);
								SpawnEntries.Add(new SpawnEntry
								{
									EntityTypeID = randomFromGroup2,
									Target = entityPlayer2
								});
							}
						}
						else
						{
							int index2 = ((selectedEntityIndex == -1) ? Random.Range(0, entityIDs.Count) : selectedEntityIndex);
							SpawnEntries.Add(new SpawnEntry
							{
								EntityTypeID = entityIDs[index2],
								Target = base.Owner.Target
							});
						}
					}
				}
			}
			else
			{
				int num3 = ModifiedCount(base.Owner.Target);
				for (int n = 0; n < num3; n++)
				{
					if (useEntityGroup)
					{
						if (!(base.Owner.Target is EntityPlayer entityPlayer3))
						{
							Debug.LogWarning("ActionBaseSpawn: Use EntityGroup requires a player target.");
							return ActionCompleteStates.InCompleteRefund;
						}
						int randomFromGroup3 = EntityGroups.GetRandomFromGroup(gameStageDefinition.GetStage(entityPlayer3.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId);
						SpawnEntries.Add(new SpawnEntry
						{
							EntityTypeID = randomFromGroup3,
							Target = entityPlayer3
						});
					}
					else
					{
						int index3 = ((selectedEntityIndex == -1) ? Random.Range(0, entityIDs.Count) : selectedEntityIndex);
						SpawnEntries.Add(new SpawnEntry
						{
							EntityTypeID = entityIDs[index3],
							Target = base.Owner.Target
						});
					}
				}
			}
			CurrentState = SpawnUpdateTypes.NeedPosition;
			break;
		}
		case SpawnUpdateTypes.NeedPosition:
			if (spawnType == SpawnTypes.NearTarget && base.Owner.Target == null && base.Owner.TargetPosition.y != 0f)
			{
				spawnType = SpawnTypes.NearPosition;
			}
			switch (spawnType)
			{
			case SpawnTypes.Position:
				if (base.Owner.TargetPosition.y != 0f)
				{
					position = base.Owner.TargetPosition;
					CurrentState = SpawnUpdateTypes.SpawnEntities;
					resetTime = 3f;
				}
				else if (base.Owner.Target != null)
				{
					if (!FindValidPosition(out position, base.Owner.Target, minDistance, maxDistance, safeSpawn, yOffset, airSpawn))
					{
						return ActionCompleteStates.InComplete;
					}
					CurrentState = SpawnUpdateTypes.SpawnEntities;
					resetTime = 3f;
				}
				else
				{
					spawnType = SpawnTypes.NearTarget;
					CurrentState = SpawnUpdateTypes.SpawnEntities;
				}
				break;
			case SpawnTypes.NearPosition:
				if (base.Owner.TargetPosition.y != 0f)
				{
					position = base.Owner.TargetPosition;
				}
				else if (base.Owner.Target != null)
				{
					position = base.Owner.Target.position;
				}
				CurrentState = SpawnUpdateTypes.SpawnEntities;
				break;
			case SpawnTypes.NearTarget:
				if (base.Owner.Target == null)
				{
					return ActionCompleteStates.InCompleteRefund;
				}
				position = base.Owner.Target.position;
				CurrentState = SpawnUpdateTypes.SpawnEntities;
				break;
			case SpawnTypes.WanderingHorde:
				CurrentState = SpawnUpdateTypes.SpawnEntities;
				if (base.Owner.TargetPosition == Vector3.zero && base.Owner.Target != null)
				{
					base.Owner.TargetPosition = base.Owner.Target.position;
				}
				break;
			}
			break;
		case SpawnUpdateTypes.SpawnEntities:
		{
			if (SpawnEntries.Count == 0)
			{
				if (UseRepeating)
				{
					if (HandleRepeat())
					{
						SpawnEntries = null;
						CurrentState = SpawnUpdateTypes.NeedSpawnEntries;
					}
					return ActionCompleteStates.InComplete;
				}
				if (clearPositionOnComplete)
				{
					base.Owner.TargetPosition = Vector3.zero;
				}
				if (!hasSpawned)
				{
					return ActionCompleteStates.InCompleteRefund;
				}
				return ActionCompleteStates.Complete;
			}
			if (spawnType == SpawnTypes.Position)
			{
				resetTime -= Time.deltaTime;
				if (resetTime <= 0f)
				{
					CurrentState = SpawnUpdateTypes.NeedPosition;
					return ActionCompleteStates.InComplete;
				}
			}
			for (int i = 0; i < SpawnEntries.Count; i++)
			{
				SpawnEntry spawnEntry = SpawnEntries[i];
				if (spawnEntry.Target == null && spawnType != SpawnTypes.Position)
				{
					SpawnEntries.RemoveAt(i);
					break;
				}
				Entity entity = null;
				switch (spawnType)
				{
				case SpawnTypes.Position:
					entity = SpawnEntity(spawnEntry.EntityTypeID, spawnEntry.Target, position, 1f, 4f, safeSpawn, yOffset);
					break;
				case SpawnTypes.NearTarget:
					entity = SpawnEntity(spawnEntry.EntityTypeID, spawnEntry.Target, spawnEntry.Target.position, minDistance, maxDistance, safeSpawn, yOffset);
					break;
				case SpawnTypes.NearPosition:
					entity = SpawnEntity(spawnEntry.EntityTypeID, spawnEntry.Target, position, minDistance, maxDistance, safeSpawn, yOffset);
					break;
				case SpawnTypes.WanderingHorde:
					if (!GameManager.Instance.World.GetMobRandomSpawnPosWithWater(base.Owner.TargetPosition, (int)minDistance, (int)maxDistance, 15, _checkBedrolls: false, out position))
					{
						return ActionCompleteStates.InComplete;
					}
					entity = SpawnEntity(spawnEntry.EntityTypeID, spawnEntry.Target, position, 1f, 1f, safeSpawn, yOffset);
					break;
				}
				if (!(entity != null))
				{
					continue;
				}
				resetTime = 60f;
				AddPropertiesToSpawnedEntity(entity);
				base.Owner.TargetPosition = position;
				if (AddToGroups != null)
				{
					for (int j = 0; j < AddToGroups.Length; j++)
					{
						if (AddToGroups[j] != "")
						{
							base.Owner.AddEntityToGroup(AddToGroups[j], entity);
						}
					}
				}
				if (attackTarget && entity is EntityAlive attacker && base.Owner.Target is EntityAlive entityAlive)
				{
					HandleTargeting(attacker, entityAlive);
					GameEventManager.Current.RegisterSpawnedEntity(entity, entityAlive, base.Owner.Requester, base.Owner, isAggressive);
					base.Owner.ReservedSpawnCount--;
					GameEventManager.Current.ReservedCount--;
				}
				if (base.Owner.Requester != null)
				{
					GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence == null) ? base.Owner : base.Owner.OwnerSequence);
					if (base.Owner.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntitySpawned(gameEventActionSequence.Name, entity.entityId, gameEventActionSequence.Tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameEventActionSequence.Name, -1, string.Empty, gameEventActionSequence.Tag, NetPackageGameEventResponse.ResponseTypes.EntitySpawned, entity.entityId), _onlyClientsAttachedToAnEntity: false, gameEventActionSequence.Requester.entityId);
					}
				}
				hasSpawned = true;
				SpawnEntries.RemoveAt(i);
				break;
			}
			break;
		}
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleExtraAction()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool HandleRepeat()
	{
		return false;
	}

	public virtual void HandleTargeting(EntityAlive attacker, EntityAlive targetAlive)
	{
	}

	public static bool FindValidPosition(out Vector3 newPoint, Entity entity, float minDistance, float maxDistance, bool spawnInSafe, float yOffset = 0f, bool spawnInAir = false)
	{
		return FindValidPosition(out newPoint, entity.position, minDistance, maxDistance, spawnInSafe, yOffset, spawnInAir);
	}

	public static bool FindValidPosition(out Vector3 newPoint, Vector3 startPoint, float minDistance, float maxDistance, bool spawnInSafe, float yOffset = 0f, bool spawnInAir = false, float raycastOffset = 0f)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			newPoint = Vector3.zero;
			return false;
		}
		Vector3 vector = new Vector3(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		float num = world.GetGameRandom().RandomFloat * (maxDistance - minDistance) + minDistance;
		newPoint = startPoint + vector * num;
		newPoint.y = startPoint.y + 1.5f;
		if (yOffset != 0f)
		{
			newPoint += Vector3.up * yOffset;
		}
		startPoint += vector * raycastOffset;
		Ray ray = new Ray(startPoint, (newPoint - startPoint).normalized);
		if (Voxel.Raycast(world, ray, num, -538750981, 67, 0f))
		{
			return false;
		}
		BlockValue block = world.GetBlock(new Vector3i(newPoint - ray.direction * 0.5f));
		if (block.Block.IsCollideMovement || block.Block.IsCollideArrows)
		{
			return false;
		}
		Vector3i blockPos = new Vector3i(startPoint);
		if (!spawnInSafe && !world.CanPlaceBlockAt(blockPos, null))
		{
			return false;
		}
		if (!spawnInAir)
		{
			if (!Voxel.Raycast(world, new Ray(newPoint, Vector3.down), 3f + yOffset, bHitTransparentBlocks: false, bHitNotCollidableBlocks: false))
			{
				return false;
			}
			newPoint = Voxel.voxelRayHitInfo.hit.pos;
		}
		return true;
	}

	public Entity SpawnEntity(int spawnedEntityID, Entity target, Vector3 startPoint, float minDistance, float maxDistance, bool spawnInSafe, float yOffset = 0f)
	{
		World world = GameManager.Instance.World;
		Vector3 rotation = ((target != null) ? new Vector3(0f, target.transform.eulerAngles.y + 180f, 0f) : Vector3.zero);
		Vector3 newPoint = Vector3.zero;
		Entity entity = null;
		if (FindValidPosition(out newPoint, startPoint, minDistance, maxDistance, spawnInSafe, yOffset, airSpawn, raycastOffset))
		{
			int spawnById = -1;
			if (base.Owner.TwitchActivated && target != null)
			{
				spawnById = target.entityId;
			}
			entity = EntityFactory.CreateEntity(spawnedEntityID, newPoint + new Vector3(0f, 0.5f, 0f), rotation, spawnById, base.Owner.ExtraData);
			entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
			world.SpawnEntityInWorld(entity);
			if (target != null && spawnSound != "")
			{
				Manager.BroadcastPlayByLocalPlayer(entity.position, spawnSound);
			}
		}
		return entity;
	}

	public virtual void AddPropertiesToSpawnedEntity(Entity entity)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		CurrentState = SpawnUpdateTypes.NeedSpawnEntries;
		SpawnEntries = null;
		selectedEntityIndex = -1;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropEntityNames, ref entityNames);
		properties.ParseBool(PropSingleChoice, ref singleChoice);
		properties.ParseString(PropSpawnCount, ref countText);
		properties.ParseString(PropPartyAddition, ref partyAdditionText);
		properties.ParseFloat(PropMinDistance, ref minDistance);
		properties.ParseFloat(PropMaxDistance, ref maxDistance);
		properties.ParseBool(PropSpawnInSafe, ref safeSpawn);
		properties.ParseBool(PropAttackTarget, ref attackTarget);
		properties.ParseBool(PropSpawnInAir, ref airSpawn);
		properties.ParseString(PropTargetGroup, ref targetGroup);
		properties.ParseString(PropAddToGroup, ref AddToGroup);
		properties.ParseFloat(PropYOffset, ref yOffset);
		properties.ParseBool(PropClearPositionOnComplete, ref clearPositionOnComplete);
		properties.ParseBool(PropIgnoreSpawnMultiplier, ref ignoreMultiplier);
		properties.ParseEnum(PropSpawnType, ref spawnType);
		properties.ParseFloat(PropRaycastOffset, ref raycastOffset);
		properties.ParseBool(PropIsAggressive, ref isAggressive);
		properties.ParseString(PropSpawnSound, ref spawnSound);
		if (properties.Contains(PropEntityGroup))
		{
			useEntityGroup = true;
			properties.ParseString(PropEntityGroup, ref entityNames);
		}
	}
}
