using System.Collections.Generic;
using System.Diagnostics;
using Twitch;
using UnityEngine;

public class Explosion
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct DamageRecord
	{
		public EntityAlive entity;

		public float damage;

		public Vector3 dir;

		public Transform hitTransform;

		public EnumBodyPartHit mainPart;

		public EnumBodyPartHit parts;
	}

	public Dictionary<Vector3i, BlockChangeInfo> ChangedBlockPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosionData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> damagedBlockPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> explosionTag = FastTags<TagGroup.Global>.Parse("explosion");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDisintegrationEpicenterPercent = 0.67f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZombiesForDisintegration = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZombiesForDisintegrationPercent = 0.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cStepDistance = 0.51f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, DamageRecord> hitEntities = new Dictionary<int, DamageRecord>();

	public Explosion(World _world, int _clrIdx, Vector3 _worldPos, Vector3i _blockPos, ExplosionData _explosionData, int _entityId)
	{
		world = _world;
		clrIdx = _clrIdx;
		worldPos = _worldPos;
		blockPos = _blockPos;
		explosionData = _explosionData;
		entityId = _entityId;
		damagedBlockPositions = new HashSet<Vector3i>();
		ChangedBlockPositions = new Dictionary<Vector3i, BlockChangeInfo>();
	}

	public void AttackBlocks(int _entityThatCausedExplosion, ItemValue _itemValueExplosionSource)
	{
		ChunkCluster chunkCluster = world.ChunkClusters[clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		EntityAlive entityAlive = world.GetEntity(_entityThatCausedExplosion) as EntityAlive;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_entityThatCausedExplosion);
		FastTags<TagGroup.Global> fastTags = explosionTag;
		if (_itemValueExplosionSource != null)
		{
			ItemClass itemClass = _itemValueExplosionSource.ItemClass;
			fastTags |= itemClass.ItemTags;
		}
		float num = EffectManager.GetValue(PassiveEffects.ExplosionRadius, _itemValueExplosionSource, explosionData.BlockRadius, entityAlive, null, fastTags);
		if (num == 0f)
		{
			num = 0.01f;
		}
		int num2 = Mathf.CeilToInt(num);
		FastTags<TagGroup.Global> other = ((explosionData.BlockTags.Length > 0) ? FastTags<TagGroup.Global>.Parse(explosionData.BlockTags) : FastTags<TagGroup.Global>.none);
		bool flag = !other.IsEmpty;
		if (chunkCluster.GetBlock(blockPos).Block.shape.IsTerrain() && !chunkCluster.GetBlock(blockPos + Vector3i.up).Block.shape.IsTerrain())
		{
			blockPos.y++;
		}
		Vector3 vector = blockPos.ToVector3Center();
		bool flag2 = false;
		for (int i = -num2; i <= num2; i++)
		{
			for (int j = -num2; j <= num2; j++)
			{
				for (int k = -num2; k <= num2; k++)
				{
					Vector3 vector2 = vector;
					Vector3 vector3 = new Vector3(i, j, k);
					float num3 = vector3.magnitude + 0.001f;
					vector3 *= 0.51f / num3;
					for (; num3 >= -0.1f; num3 -= 0.51f, vector2 += vector3)
					{
						Vector3i vector3i = World.worldToBlockPos(vector2);
						BlockValue block = chunkCluster.GetBlock(vector3i);
						if (block.isair || block.isWater)
						{
							continue;
						}
						if (damagedBlockPositions.Contains(vector3i))
						{
							break;
						}
						Block block2 = block.Block;
						if (block2.StabilityIgnore)
						{
							continue;
						}
						if (vector3i != blockPos)
						{
							damagedBlockPositions.Add(vector3i);
						}
						else
						{
							if (flag2)
							{
								continue;
							}
							flag2 = true;
						}
						float num4 = (vector3i.ToVector3Center() - worldPos).magnitude - 0.5f;
						if (num4 < 0f)
						{
							num4 = 0f;
						}
						FastTags<TagGroup.Global> tags = fastTags;
						tags |= block2.Tags;
						float num5 = EffectManager.GetValue(PassiveEffects.ExplosionBlockDamage, _itemValueExplosionSource, explosionData.BlockDamage, entityAlive, null, tags);
						if ((bool)entityAlive)
						{
							num5 = num5 * entityAlive.GetBlockDamageScale() + 0.5f;
						}
						if (Utils.FastMax(num5, 1f) / (float)(2 * num2 + 1) <= 0f)
						{
							continue;
						}
						float num6 = (1f - num4 / num) * num5;
						if (flag && block2.Tags.Test_AnySet(other) && explosionData.damageMultiplier != null)
						{
							num6 *= explosionData.damageMultiplier.Get("tags");
						}
						if (num6 <= 0f)
						{
							break;
						}
						if (block.ischild)
						{
							vector3i = block2.multiBlockPos.GetParentPos(vector3i, block);
							block = world.GetBlock(vector3i);
							block2 = block.Block;
						}
						if (!ChangedBlockPositions.TryGetValue(vector3i, out var value))
						{
							value = new BlockChangeInfo(clrIdx, vector3i, block);
							value.bChangeDamage = true;
							if (value.blockValue.isWater)
							{
								continue;
							}
							ChangedBlockPositions[vector3i] = value;
						}
						if (value.blockValue.isair || world.IsWithinTraderArea(vector3i) || world.InBoundsForPlayersPercent(vector3i.ToVector3CenterXZ()) < 0.5f)
						{
							continue;
						}
						Block block3 = value.blockValue.Block;
						float explosionResistance = block3.GetExplosionResistance();
						float hardness = block3.GetHardness();
						float num7 = ((clrIdx == 0) ? world.GetLandProtectionHardnessModifier(vector3i, entityAlive, playerDataFromEntityID) : 1f);
						int num8 = 0;
						if (hardness > 0f)
						{
							float num9 = (1f - explosionResistance) * num6 / (hardness * num7);
							if (explosionData.damageMultiplier != null)
							{
								num9 *= explosionData.damageMultiplier.Get(block3.blockMaterial.DamageCategory);
							}
							num8 = Mathf.RoundToInt(num9);
						}
						else if (num7 > 0f)
						{
							num8 = Mathf.RoundToInt((float)block2.MaxDamage / num7);
						}
						if (num8 <= 0)
						{
							continue;
						}
						if (num8 + value.blockValue.damage >= block2.MaxDamage)
						{
							num8 = num8 + value.blockValue.damage - block2.MaxDamage;
							value.bChangeDamage = false;
							Block.DestroyedResult destroyedResult = block3.OnBlockDestroyedByExplosion(world, clrIdx, vector3i, value.blockValue, entityId);
							if (!block3.DowngradeBlock.isair && destroyedResult == Block.DestroyedResult.Downgrade)
							{
								do
								{
									BlockValue downgradeBlock = value.blockValue.Block.DowngradeBlock;
									downgradeBlock = BlockPlaceholderMap.Instance.Replace(downgradeBlock, world.GetGameRandom(), vector3i.x, vector3i.z);
									downgradeBlock.rotation = value.blockValue.rotation;
									downgradeBlock.meta = value.blockValue.meta;
									value.blockValue = downgradeBlock;
									value.blockValue.damage = num8;
									num8 -= value.blockValue.Block.MaxDamage;
								}
								while (num8 > 0 && !value.blockValue.Block.DowngradeBlock.isair);
								if (num8 >= 0)
								{
									value.blockValue = BlockValue.Air;
								}
							}
							else
							{
								value.blockValue = BlockValue.Air;
							}
							if (!value.blockValue.isair)
							{
								break;
							}
							damagedBlockPositions.Remove(vector3i);
						}
						else
						{
							value.blockValue.damage = num8 + value.blockValue.damage;
						}
						if (!value.blockValue.isair)
						{
							break;
						}
					}
				}
			}
		}
	}

	public void AttackEntites(int _entityThatCausedExplosion, ItemValue _itemValueExplosionSource, EnumDamageTypes damageType)
	{
		EntityAlive entityAlive = world.GetEntity(_entityThatCausedExplosion) as EntityAlive;
		FastTags<TagGroup.Global> tags = explosionTag;
		if (_itemValueExplosionSource != null)
		{
			ItemClass itemClass = _itemValueExplosionSource.ItemClass;
			tags |= itemClass.ItemTags;
		}
		float value = EffectManager.GetValue(PassiveEffects.ExplosionEntityDamage, _itemValueExplosionSource, explosionData.EntityDamage, entityAlive, null, tags);
		float value2 = EffectManager.GetValue(PassiveEffects.ExplosionRadius, _itemValueExplosionSource, explosionData.EntityRadius, entityAlive, null, tags);
		Collider[] array = Physics.OverlapSphere(worldPos - Origin.position, value2, -538480645);
		bool flag = false;
		Vector3i vector3i = World.worldToBlockPos(worldPos);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider)
			{
				continue;
			}
			Transform transform = collider.transform;
			if (transform.CompareTag("Item"))
			{
				EntityItem component = transform.GetComponent<EntityItem>();
				if (!component)
				{
					RootTransformRefEntity component2 = transform.GetComponent<RootTransformRefEntity>();
					if ((bool)component2)
					{
						component = component2.RootTransform.GetComponent<EntityItem>();
					}
				}
				if ((bool)component && !component.IsDead() && !hitEntities.ContainsKey(component.entityId))
				{
					hitEntities.Add(component.entityId, default(DamageRecord));
					component.OnDamagedByExplosion();
					component.SetDead();
				}
				continue;
			}
			string tag = transform.tag;
			if (!tag.StartsWith("E_BP_"))
			{
				continue;
			}
			flag = true;
			Transform hitRootTransform = GameUtils.GetHitRootTransform(tag, transform);
			EntityAlive entityAlive2 = (hitRootTransform ? hitRootTransform.GetComponent<EntityAlive>() : null);
			if (!entityAlive2)
			{
				continue;
			}
			entityAlive2.ConditionalTriggerSleeperWakeUp();
			Vector3 vector = transform.position + Origin.position - worldPos;
			float magnitude = vector.magnitude;
			vector.Normalize();
			if (Voxel.Raycast(world, new Ray(worldPos, vector), magnitude, 65536, 66, 0f))
			{
				continue;
			}
			EntityClass entityClass = EntityClass.list[entityAlive2.entityClass];
			float num = ((!transform.CompareTag("E_BP_LArm") && !transform.CompareTag("E_BP_RArm")) ? ((transform.CompareTag("E_BP_LLeg") || transform.CompareTag("E_BP_RLeg")) ? entityClass.LegsExplosionDamageMultiplier : ((!transform.CompareTag("E_BP_Head")) ? entityClass.ChestExplosionDamageMultiplier : entityClass.HeadExplosionDamageMultiplier)) : entityClass.ArmsExplosionDamageMultiplier);
			float num2 = value * num;
			num2 *= 1f - magnitude / value2;
			num2 = (int)EffectManager.GetValue(PassiveEffects.ExplosionIncomingDamage, null, num2, entityAlive2);
			if (num2 >= 3f)
			{
				if (!hitEntities.TryGetValue(entityAlive2.entityId, out var value3))
				{
					value3.entity = entityAlive2;
				}
				EnumBodyPartHit enumBodyPartHit = DamageSource.TagToBodyPart(tag);
				if (num2 > value3.damage)
				{
					value3.damage = num2;
					value3.dir = vector;
					value3.hitTransform = transform;
					value3.mainPart = enumBodyPartHit;
				}
				value3.parts |= enumBodyPartHit;
				hitEntities[entityAlive2.entityId] = value3;
			}
		}
		int num3 = 0;
		int num4 = 0;
		float num5 = (float)hitEntities.Count * 0.8f;
		foreach (KeyValuePair<int, DamageRecord> hitEntity in hitEntities)
		{
			EntityAlive entity = hitEntity.Value.entity;
			if (!(entity != null))
			{
				continue;
			}
			bool flag2 = entity.IsDead();
			int health = entity.Health;
			bool flag3 = hitEntity.Value.damage > (float)entity.GetMaxHealth() * 0.1f;
			EnumBodyPartHit mainPart = hitEntity.Value.mainPart;
			EnumBodyPartHit parts = hitEntity.Value.parts;
			parts &= ~mainPart;
			float num6 = (flag2 ? 0.85f : 0.6f);
			for (int j = 0; j < 11; j++)
			{
				int num7 = 1 << j;
				if (((uint)parts & (uint)num7) != 0 && entity.rand.RandomFloat <= num6)
				{
					parts = (EnumBodyPartHit)((int)parts & ~num7);
				}
			}
			if (entity is EntityEnemy && ((float)num3 >= 4f || flag2))
			{
				if ((entity.position - worldPos).sqrMagnitude < value2 * 0.67f)
				{
					entity.canDisintegrate = true;
				}
				else if ((float)num4 < num5)
				{
					entity.canDisintegrate = true;
				}
			}
			DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, damageType, _entityThatCausedExplosion, hitEntity.Value.dir, hitEntity.Value.hitTransform.name, Vector3.zero, Vector2.zero);
			damageSourceEntity.bodyParts = mainPart | parts;
			damageSourceEntity.AttackingItem = _itemValueExplosionSource;
			damageSourceEntity.DismemberChance = (flag3 ? 0.5f : 0f);
			damageSourceEntity.BlockPosition = vector3i;
			entity.DamageEntity(damageSourceEntity, (int)hitEntity.Value.damage, flag3);
			if (entity.isDisintegrated)
			{
				num4++;
			}
			if (entityAlive != null)
			{
				MinEventTypes eventType = ((health != entity.Health) ? MinEventTypes.onSelfExplosionDamagedOther : MinEventTypes.onSelfExplosionAttackedOther);
				entityAlive.MinEventContext.Self = entityAlive;
				entityAlive.MinEventContext.Other = entity;
				entityAlive.MinEventContext.ItemValue = _itemValueExplosionSource;
				_itemValueExplosionSource?.FireEvent(eventType, entityAlive.MinEventContext);
				entityAlive.FireEvent(eventType, useInventory: false);
			}
			if (explosionData.BuffActions != null)
			{
				for (int k = 0; k < explosionData.BuffActions.Count; k++)
				{
					BuffClass buff = BuffManager.GetBuff(explosionData.BuffActions[k]);
					if (buff != null)
					{
						float originalValue = 1f;
						originalValue = EffectManager.GetValue(PassiveEffects.BuffProcChance, null, originalValue, entityAlive, null, FastTags<TagGroup.Global>.Parse(buff.Name));
						if (entity.rand.RandomFloat <= originalValue)
						{
							entity.Buffs.AddBuff(explosionData.BuffActions[k], _entityThatCausedExplosion);
						}
					}
				}
			}
			if (flag2 || !entity.IsDead())
			{
				continue;
			}
			num3++;
			EntityPlayer entityPlayer = entityAlive as EntityPlayer;
			if ((bool)entityPlayer)
			{
				entityPlayer.AddKillXP(entity);
			}
			if (entity is EntityPlayer player)
			{
				TwitchManager.Current.CheckKiller(player, entityAlive, vector3i);
			}
			if (entityAlive != null)
			{
				if (entityAlive.isEntityRemote)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, entity.entityId, MinEventTypes.onSelfKilledOther, _itemValueExplosionSource), _onlyClientsAttachedToAnEntity: false, entityAlive.entityId);
				}
				else
				{
					entityAlive.FireEvent(MinEventTypes.onSelfKilledOther);
				}
			}
		}
		if (!flag && entityAlive != null)
		{
			if (entityAlive.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, -1, MinEventTypes.onSelfExplosionMissEntity, _itemValueExplosionSource), _onlyClientsAttachedToAnEntity: false, entityAlive.entityId);
			}
			else
			{
				entityAlive.MinEventContext.Self = entityAlive;
				entityAlive.MinEventContext.Other = null;
				entityAlive.MinEventContext.ItemValue = _itemValueExplosionSource;
				entityAlive.FireEvent(MinEventTypes.onSelfExplosionMissEntity);
			}
		}
		hitEntities.Clear();
	}

	[Conditional("DEBUG_EXPLOSION_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string format, params object[] args)
	{
		format = $"{GameManager.frameTime.ToCultureInvariantString()} {GameManager.frameCount} Explosion {format}";
		Log.Warning(format, args);
	}
}
