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

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameManager gameManager;

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
	public bool bOnIdealPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int hmOverride;

	public Vector3 FinalPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float radius;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (gameManager == null)
		{
			gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		}
	}

	public void Fire(Vector3 _idealStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f)
	{
		flyDirection = _flyDirection.normalized;
		idealPosition = _idealStartPosition;
		firingEntity = _firingEntity;
		velocity = flyDirection.normalized * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValueLauncher, itemActionProjectile.Velocity, _firingEntity as EntityAlive);
		hmOverride = _hmOverride;
		radius = _radius;
		waterCollisionParticles.Init(ProjectileOwnerID, itemProjectile.MadeOfMaterial.SurfaceCategory, "water", 16);
		Transform transform = base.transform;
		if (_idealStartPosition == Vector3.zero)
		{
			previousPosition = transform.position + Origin.position;
		}
		else
		{
			previousPosition = _idealStartPosition;
		}
		gravity = EffectManager.GetValue(PassiveEffects.ProjectileGravity, itemValueLauncher, itemActionProjectile.Gravity, _firingEntity as EntityAlive);
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
		else
		{
			transform.position = _idealStartPosition;
		}
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
			if (gameManager == null || gameManager.World == null)
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
			if (!bOnIdealPos)
			{
				bOnIdealPos = idealPosition.Equals(Vector3.zero) || stateTime > 0.5f;
			}
			if (bOnIdealPos)
			{
				vector += vector2;
			}
			else
			{
				idealPosition += vector2;
				vector += vector2;
				vector = Vector3.Lerp(vector, idealPosition - Origin.position, stateTime * 2f);
			}
			obj.position = vector;
			if (stateTime >= itemActionProjectile.LifeTime)
			{
				SetState(State.Dead);
			}
		}
		else if (state == State.Dead && stateTime > itemActionProjectile.DeadTime)
		{
			SetState(State.Destroyed);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		checkCollision();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision()
	{
		if (state != State.Active || gameManager == null)
		{
			return;
		}
		World world = gameManager.World;
		if (world == null)
		{
			return;
		}
		Vector3 vector = ((!bOnIdealPos) ? idealPosition : (base.transform.position + Origin.position));
		Vector3 vector2 = vector - previousPosition;
		float magnitude = vector2.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)firingEntity;
		Ray ray = new Ray(previousPosition, vector2.normalized);
		waterCollisionParticles.CheckCollision(ray.origin, ray.direction, magnitude, (entityAlive != null) ? entityAlive.entityId : (-1));
		int num = -1;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			num = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2);
		}
		int hitMask = ((hmOverride == 0) ? 80 : hmOverride);
		bool num2 = Voxel.Raycast(world, ray, magnitude, -538750997, hitMask, radius);
		if (num >= 0)
		{
			entityAlive.SetModelLayer(num);
		}
		if (num2 && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			if (firingEntity != null && !firingEntity.isEntityRemote)
			{
				entityAlive.MinEventContext.Other = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive;
				ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
				{
					WeaponTypeTag = ItemActionAttack.RangedTag
				};
				ItemActionAttack.Hit(Voxel.voxelRayHitInfo, ProjectileOwnerID, EnumDamageTypes.Piercing, Mathf.Lerp(1f, itemActionProjectile.GetDamageBlock(itemValueLauncher, ItemActionAttack.GetBlockHit(world, Voxel.voxelRayHitInfo), entityAlive), actionData.strainPercent), Mathf.Lerp(1f, itemActionProjectile.GetDamageEntity(itemValueLauncher, entityAlive), actionData.strainPercent), 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, itemValueLauncher, itemProjectile.CritChance.Value, entityAlive, null, itemProjectile.ItemTags), ItemAction.GetDismemberChance(actionData, Voxel.voxelRayHitInfo), itemProjectile.MadeOfMaterial.SurfaceCategory, itemActionProjectile.GetDamageMultiplier(), getBuffActions(), attackDetails, 1, itemActionProjectile.ActionExp, itemActionProjectile.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, itemValueLauncher);
				if (entityAlive.MinEventContext.Other == null)
				{
					entityAlive.FireEvent(MinEventTypes.onSelfPrimaryActionMissEntity);
				}
				entityAlive.FireEvent(MinEventTypes.onProjectileImpact, useInventory: false);
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = itemValueProjectile;
				MinEventParams.CachedEventParam.Other = entityAlive.MinEventContext.Other;
				itemProjectile.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
				if (itemActionProjectile.Explosion.ParticleIndex > 0)
				{
					Vector3 hitPos = Voxel.voxelRayHitInfo.hit.pos - vector2.normalized * 0.1f;
					Vector3i vector3i = World.worldToBlockPos(hitPos);
					if (!world.GetBlock(vector3i).isair)
					{
						vector3i = Voxel.OneVoxelStep(vector3i, hitPos, -vector2.normalized, out hitPos, out var _);
					}
					gameManager.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, hitPos, vector3i, Quaternion.identity, itemActionProjectile.Explosion, ProjectileOwnerID, 0f, _bRemoveBlockAtExplPosition: false, itemValueLauncher);
					SetState(State.Dead);
					return;
				}
				if (itemProjectile.IsSticky)
				{
					GameRandom gameRandom = world.GetGameRandom();
					if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
					{
						if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, itemValueLauncher, 0.5f, entityAlive, null, itemProjectile.ItemTags | FastTags<TagGroup.Global>.Parse(Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory)))
						{
							ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector2.normalized, itemValueProjectile.type);
							SetState(State.Sticky);
						}
						else
						{
							gameManager.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, $"{Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory}hit{itemProjectile.MadeOfMaterial.SurfaceCategory}", null), firingEntity.entityId);
							SetState(State.Dead);
						}
					}
					else if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, itemValueLauncher, 0.5f, entityAlive, null, itemProjectile.ItemTags))
					{
						ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector2.normalized, itemValueProjectile.type);
						Utils.SetLayerRecursively(ProjectileManager.GetProjectile(ProjectileID).gameObject, 14);
						SetState(State.Sticky);
					}
					else
					{
						gameManager.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, "bullethitwood", null), firingEntity.entityId);
						SetState(State.Dead);
					}
				}
				else
				{
					SetState(State.Dead);
				}
			}
			else
			{
				SetState(State.Dead);
			}
		}
		previousPosition = vector;
	}

	public void OnDestroy()
	{
		if (!(GameManager.Instance == null) && GameManager.Instance.World != null && !(firingEntity == null) && itemValueProjectile != null && ProjectileID != -1 && firingEntity != null && !firingEntity.isEntityRemote)
		{
			Vector3 position = base.transform.position;
			if (GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(position.x + Origin.position.x), Mathf.CeilToInt(position.y + Origin.position.y), Mathf.CeilToInt(position.z + Origin.position.z)))
			{
				GameManager.Instance.ItemDropServer(new ItemStack(itemValueProjectile, 1), position + Origin.position, Vector3.zero, ProjectileOwnerID);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return itemActionProjectile.BuffActions;
	}
}
