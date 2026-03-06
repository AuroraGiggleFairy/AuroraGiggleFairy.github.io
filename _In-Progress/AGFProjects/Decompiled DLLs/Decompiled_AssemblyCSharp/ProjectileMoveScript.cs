using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{
	public enum State
	{
		Idle,
		Active,
		Sticky,
		StickyDestroyed,
		Dead,
		Destroyed
	}

	public const int InvalidID = -1;

	public int ProjectileID = -1;

	public int ProjectileOwnerID;

	public ItemActionProjectile itemActionProjectile;

	public ItemClass itemProjectile;

	public ItemValue itemValueProjectile;

	public ItemValue itemValueLauncher;

	public ItemActionLauncher.ItemActionDataLauncher actionData;

	public Vector3 FinalPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 flyDirection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 idealPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 velocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public State state;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float stateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity firingEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 previousPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float gravity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOnIdealPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitMask;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float radius;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float collisionStartBack;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool explosionDisabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
	}

	public void Fire(Vector3 _idealStartPos, Vector3 _dirOrPos, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f, bool _isBallistic = false)
	{
		Transform transform = base.transform;
		isOnIdealPos = true;
		previousPosition = transform.position + Origin.position;
		if (_idealStartPos.y != 0f)
		{
			idealPosition = _idealStartPos;
			isOnIdealPos = false;
		}
		firingEntity = _firingEntity;
		hitMask = ((_hmOverride == 0) ? 80 : _hmOverride);
		radius = ((_radius >= 0f) ? _radius : itemActionProjectile.collisionRadius);
		if (itemActionProjectile.FlyTime < 0f)
		{
			if (_firingEntity is EntityAlive entityAlive)
			{
				if (_isBallistic)
				{
					Vector3 vector = _dirOrPos - _idealStartPos;
					float magnitude = vector.magnitude;
					flyDirection = vector / magnitude;
					float num = Utils.FastLerp(0.4f, 0f - itemActionProjectile.FlyTime, magnitude / itemActionProjectile.Velocity);
					velocity = vector / num;
					velocity.y += itemActionProjectile.Gravity * -0.5f * num;
					Vector3 vector2 = velocity * 0.015f;
					previousPosition += vector2;
					idealPosition += vector2;
				}
				else
				{
					flyDirection = entityAlive.GetForwardVector();
					velocity = flyDirection * 2f;
					explosionDisabled = true;
				}
			}
			gravity = itemActionProjectile.Gravity;
		}
		else
		{
			flyDirection = _dirOrPos.normalized;
			velocity = flyDirection * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValueLauncher, itemActionProjectile.Velocity, _firingEntity as EntityAlive);
			gravity = EffectManager.GetValue(PassiveEffects.ProjectileGravity, itemValueLauncher, itemActionProjectile.Gravity, _firingEntity as EntityAlive);
		}
		waterCollisionParticles.Init(ProjectileOwnerID, itemProjectile.MadeOfMaterial.SurfaceCategory, "water", 16);
		OnActivateItemGameObjectReference component = transform.GetComponent<OnActivateItemGameObjectReference>();
		if ((bool)component)
		{
			component.ActivateItem(_activate: true);
		}
		if ((bool)transform.parent)
		{
			transform.SetParent(null);
			Utils.SetLayerRecursively(transform.gameObject, 0);
		}
		transform.position = previousPosition - Origin.position;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		SetState(State.Active);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetState(State _state)
	{
		state = _state;
		stateTime = 0f;
		if (state == State.Dead)
		{
			Transform obj = base.transform;
			Transform transform = obj.Find("MeshExplode");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
			Light componentInChildren = obj.GetComponentInChildren<Light>();
			if ((bool)componentInChildren)
			{
				componentInChildren.gameObject.SetActive(value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		stateTime += fixedDeltaTime;
		if (state == State.Active)
		{
			GameManager instance = GameManager.Instance;
			if (!instance || instance.World == null)
			{
				return;
			}
			if (stateTime > itemActionProjectile.FlyTime)
			{
				velocity.y += gravity * fixedDeltaTime;
			}
			Transform obj = base.transform;
			Vector3 vector = obj.position;
			Vector3 vector2 = velocity * fixedDeltaTime;
			obj.LookAt(vector + vector2);
			vector += vector2;
			if (!isOnIdealPos)
			{
				idealPosition += vector2;
				vector = Vector3.Lerp(vector, idealPosition - Origin.position, stateTime * 5f);
				isOnIdealPos = stateTime >= 0.2f;
			}
			obj.position = vector;
			if (stateTime >= itemActionProjectile.LifeTime)
			{
				SetState(State.Dead);
			}
		}
		else if (state == State.Sticky)
		{
			if (stateTime >= 180f)
			{
				SetState(State.StickyDestroyed);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else if (state == State.Dead && stateTime > itemActionProjectile.DeadTime)
		{
			SetState(State.Destroyed);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (state == State.Active)
		{
			checkCollision();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision()
	{
		GameManager instance = GameManager.Instance;
		if (!instance)
		{
			return;
		}
		World world = instance.World;
		if (world == null)
		{
			return;
		}
		Vector3 vector = ((!isOnIdealPos) ? idealPosition : (base.transform.position + Origin.position));
		Vector3 direction = vector - previousPosition;
		float magnitude = direction.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray = new Ray(previousPosition, direction);
		previousPosition = vector;
		EntityAlive entityAlive = (EntityAlive)firingEntity;
		waterCollisionParticles.CheckCollision(ray.origin, ray.direction, magnitude, entityAlive ? entityAlive.entityId : (-1));
		int num = -1;
		if ((bool)entityAlive && (bool)entityAlive.emodel)
		{
			num = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2);
		}
		float num2 = radius + collisionStartBack;
		ray.origin -= ray.direction * num2;
		bool num3 = Voxel.Raycast(world, ray, magnitude + num2, -538750997, hitMask, radius);
		collisionStartBack = 0.1f;
		if (num >= 0)
		{
			entityAlive.SetModelLayer(num);
		}
		if (!num3 || (!GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) && !Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			return;
		}
		if ((bool)firingEntity && !firingEntity.isEntityRemote)
		{
			EntityAlive entityAlive2 = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive;
			EntityDrone entityDrone = entityAlive2 as EntityDrone;
			EntityAlive entityAlive3 = firingEntity as EntityAlive;
			if ((bool)entityDrone && (bool)entityAlive3 && entityDrone.isAlly(entityAlive3))
			{
				return;
			}
			entityAlive.MinEventContext.Other = entityAlive2;
			ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
			{
				WeaponTypeTag = ItemActionAttack.RangedTag
			};
			MinEventParams.CachedEventParam.Self = entityAlive;
			MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
			MinEventParams.CachedEventParam.ItemValue = itemValueProjectile;
			MinEventParams.CachedEventParam.Other = entityAlive2;
			MinEventParams.CachedEventParam.ItemActionData = actionData;
			itemProjectile.FireEvent(MinEventTypes.onProjectilePreImpact, MinEventParams.CachedEventParam);
			float blockDamage = Utils.FastLerp(1f, itemActionProjectile.GetDamageBlock(itemValueLauncher, ItemActionAttack.GetBlockHit(world, Voxel.voxelRayHitInfo), entityAlive), actionData.strainPercent);
			float entityDamage = Utils.FastLerp(1f, itemActionProjectile.GetDamageEntity(itemValueLauncher, entityAlive), actionData.strainPercent);
			ItemActionAttack.Hit(Voxel.voxelRayHitInfo, ProjectileOwnerID, EnumDamageTypes.Piercing, blockDamage, entityDamage, 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, itemValueLauncher, itemProjectile.CritChance.Value, entityAlive, null, itemProjectile.ItemTags), ItemAction.GetDismemberChance(actionData, Voxel.voxelRayHitInfo), itemProjectile.MadeOfMaterial.SurfaceCategory, itemActionProjectile.GetDamageMultiplier(), getBuffActions(), attackDetails, 1, itemActionProjectile.ActionExp, itemActionProjectile.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, itemValueLauncher);
			if (!entityAlive2)
			{
				entityAlive.FireEvent(MinEventTypes.onSelfPrimaryActionMissEntity);
			}
			entityAlive.FireEvent(MinEventTypes.onProjectileImpact, useInventory: false);
			MinEventParams.CachedEventParam.Self = entityAlive;
			MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
			MinEventParams.CachedEventParam.ItemValue = itemValueProjectile;
			MinEventParams.CachedEventParam.Other = entityAlive2;
			MinEventParams.CachedEventParam.ItemActionData = actionData;
			itemProjectile.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
			if (itemActionProjectile.Explosion.ParticleIndex > 0 && !explosionDisabled)
			{
				Vector3 hitPos = Voxel.voxelRayHitInfo.hit.pos - direction.normalized * 0.1f;
				Vector3i vector3i = World.worldToBlockPos(hitPos);
				if (!world.GetBlock(vector3i).isair)
				{
					vector3i = Voxel.OneVoxelStep(vector3i, hitPos, -direction.normalized, out hitPos, out var _);
				}
				instance.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, hitPos, vector3i, Quaternion.identity, itemActionProjectile.Explosion, ProjectileOwnerID, 0f, _bRemoveBlockAtExplPosition: false, itemValueLauncher);
				SetState(State.Dead);
			}
			else
			{
				if (entityAlive2 is EntitySwarm)
				{
					return;
				}
				if (itemProjectile.IsSticky)
				{
					GameRandom gameRandom = world.GetGameRandom();
					if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
					{
						if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, itemValueLauncher, 0.5f, entityAlive, null, itemProjectile.ItemTags | FastTags<TagGroup.Global>.Parse(Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory)))
						{
							ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, direction.normalized, itemValueProjectile.type);
							SetState(State.Sticky);
						}
						else
						{
							instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, $"{Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory}hit{itemProjectile.MadeOfMaterial.SurfaceCategory}", null), firingEntity.entityId);
							SetState(State.Dead);
						}
					}
					else if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, itemValueLauncher, 0.5f, entityAlive, null, itemProjectile.ItemTags))
					{
						ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, direction.normalized, itemValueProjectile.type);
						Utils.SetLayerRecursively(ProjectileManager.GetProjectile(ProjectileID).gameObject, 14);
						SetState(State.Sticky);
					}
					else
					{
						instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, "bullethitwood", null), firingEntity.entityId);
						SetState(State.Dead);
					}
				}
				else
				{
					SetState(State.Dead);
				}
			}
		}
		else
		{
			SetState(State.Dead);
		}
	}

	public void OnDestroy()
	{
		GameManager instance = GameManager.Instance;
		if ((bool)instance && instance.World != null && (bool)firingEntity && itemValueProjectile != null && state != State.StickyDestroyed && ProjectileID != -1 && !firingEntity.isEntityRemote)
		{
			Vector3 position = base.transform.position;
			if (instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(position.x + Origin.position.x), Mathf.CeilToInt(position.y + Origin.position.y), Mathf.CeilToInt(position.z + Origin.position.z)))
			{
				instance.ItemDropServer(new ItemStack(itemValueProjectile, 1), position + Origin.position, Vector3.zero, ProjectileOwnerID);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return itemActionProjectile.BuffActions;
	}
}
