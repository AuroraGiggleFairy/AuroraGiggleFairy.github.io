using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrownWeaponMoveScript : MonoBehaviour
{
	public const int InvalidID = -1;

	public int ProjectileID = -1;

	public int ProjectileOwnerID;

	public ItemActionThrownWeapon itemActionThrownWeapon;

	public ItemClass itemWeapon;

	public ItemValue itemValueWeapon;

	public ItemActionThrowAway.MyInventoryData actionData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameManager gameManager;

	public Vector3 flyDirection;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 idealPosition;

	public Vector3 velocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeShotStarted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bArmed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity firingEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 previousPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 gravity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bOnIdealPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int hmOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public NavObject NavObject;

	public Vector3 FinalPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive stuckInEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (gameManager == null)
		{
			gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		}
	}

	public void Fire(Vector3 _idealStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _velocity = -1f)
	{
		flyDirection = _flyDirection.normalized;
		idealPosition = _idealStartPosition;
		firingEntity = _firingEntity;
		velocity = flyDirection.normalized * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValueWeapon, (_velocity == -1f) ? ((float)itemActionThrownWeapon.Velocity) : _velocity, _firingEntity as EntityAlive);
		bArmed = true;
		hmOverride = _hmOverride;
		waterCollisionParticles.Init(ProjectileOwnerID, itemWeapon.MadeOfMaterial.SurfaceCategory, "water", 16);
		if (_idealStartPosition == Vector3.zero)
		{
			previousPosition = base.transform.position + Origin.position;
		}
		else
		{
			previousPosition = _idealStartPosition;
		}
		CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		gravity = Vector3.up * EffectManager.GetValue(PassiveEffects.ProjectileGravity, itemValueWeapon, itemActionThrownWeapon.Gravity, _firingEntity as EntityAlive);
		if (base.transform.GetComponent<OnActivateItemGameObjectReference>() != null)
		{
			base.transform.GetComponent<OnActivateItemGameObjectReference>().ActivateItem(_activate: true);
		}
		timeShotStarted = Time.time;
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
			Utils.SetLayerRecursively(base.transform.gameObject, 0);
		}
		base.transform.position = _idealStartPosition - Origin.position;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		if (!bArmed)
		{
			if (base.transform.lossyScale.x < 0.01f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (!base.transform.gameObject.activeInHierarchy)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (!stuckInEntity)
			{
				base.transform.position = FinalPosition - Origin.position;
			}
		}
		else
		{
			if (gameManager == null || gameManager.World == null)
			{
				return;
			}
			if (!(Time.time - timeShotStarted < itemActionThrownWeapon.FlyTime))
			{
				velocity += gravity * Time.fixedDeltaTime;
			}
			Vector3 vector = velocity * Time.fixedDeltaTime;
			if (!gameManager.World.IsChunkAreaCollidersLoaded(base.transform.position + vector + Origin.position))
			{
				vector = gravity * Time.fixedDeltaTime;
			}
			base.transform.LookAt(base.transform.position + vector);
			base.transform.Rotate(Vector3.right, 90f);
			checkCollision(vector);
			if (bArmed)
			{
				base.transform.position = base.transform.position + vector;
				if (Time.time - timeShotStarted >= itemActionThrownWeapon.LifeTime)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision(Vector3 _amountToMove)
	{
		if (gameManager == null || gameManager.World == null)
		{
			return;
		}
		Vector3 vector = _amountToMove;
		float magnitude = vector.magnitude;
		if (magnitude <= 0f)
		{
			return;
		}
		Vector3 vector2 = vector * (1f / magnitude);
		magnitude += 0.5f;
		EntityAlive entityAlive = (EntityAlive)firingEntity;
		Ray ray = new Ray(base.transform.position + Origin.position + vector2 * -0.2f, vector2);
		waterCollisionParticles.CheckCollision(ray.origin, ray.direction, magnitude, (entityAlive != null) ? entityAlive.entityId : (-1));
		int layerId = 0;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			layerId = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2);
		}
		bool num = Voxel.Raycast(gameManager.World, ray, magnitude, -538750981, (hmOverride == 0) ? 8 : hmOverride, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(layerId);
		}
		if (!num || (!GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) && !Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			return;
		}
		if (firingEntity != null && !firingEntity.isEntityRemote)
		{
			stuckInEntity = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive;
			entityAlive.MinEventContext.Other = stuckInEntity;
			entityAlive.FireEvent(MinEventTypes.onProjectileImpact, useInventory: false);
			MinEventParams.CachedEventParam.Self = entityAlive;
			MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
			MinEventParams.CachedEventParam.ItemValue = itemValueWeapon;
			itemWeapon.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
			ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
			{
				WeaponTypeTag = ItemActionAttack.ThrownTag
			};
			if (itemValueWeapon.MaxUseTimes > 0)
			{
				itemValueWeapon.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemValueWeapon, 1f, firingEntity as EntityAlive, null, itemValueWeapon.ItemClass.ItemTags | FastTags<TagGroup.Global>.Parse("Secondary"));
			}
			ItemActionAttack.Hit(Voxel.voxelRayHitInfo, ProjectileOwnerID, EnumDamageTypes.Piercing, itemActionThrownWeapon.GetDamageBlock(itemValueWeapon, ItemActionAttack.GetBlockHit(gameManager.World, Voxel.voxelRayHitInfo), entityAlive, 1), itemActionThrownWeapon.GetDamageEntity(itemValueWeapon, entityAlive, 1), 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, itemValueWeapon, itemWeapon.CritChance.Value, entityAlive, null, itemWeapon.ItemTags), ItemAction.GetDismemberChance(actionData, Voxel.voxelRayHitInfo), itemWeapon.MadeOfMaterial.SurfaceCategory, null, getBuffActions(), attackDetails, 1, itemActionThrownWeapon.ActionExp, itemActionThrownWeapon.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, itemValueWeapon);
			NavObject = NavObjectManager.Instance.RegisterNavObject(itemWeapon.NavObject, base.transform);
			if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
			{
				ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector.normalized, itemValueWeapon.type);
			}
			else
			{
				ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector.normalized, itemValueWeapon.type);
			}
			Utils.SetLayerRecursively(ProjectileManager.GetProjectile(ProjectileID).gameObject, 14);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		bArmed = false;
	}

	public void DropStuckProjectile()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnDestroy()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || firingEntity == null || base.transform == null || itemValueWeapon == null)
		{
			return;
		}
		if (stuckInEntity != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(NavObject);
			if (ProjectileID != -1 && firingEntity != null && !firingEntity.isEntityRemote)
			{
				Vector3 bellyPosition = stuckInEntity.getBellyPosition();
				if (GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(bellyPosition.x), Mathf.CeilToInt(bellyPosition.y), Mathf.CeilToInt(bellyPosition.z)))
				{
					GameManager.Instance.ItemDropServer(new ItemStack(itemValueWeapon, 1), bellyPosition, Vector3.zero, ProjectileOwnerID, 1000f);
				}
			}
		}
		else if (ProjectileID != -1 && firingEntity != null && !firingEntity.isEntityRemote && GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(base.transform.position.x + Origin.position.x), Mathf.CeilToInt(base.transform.position.y + Origin.position.y), Mathf.CeilToInt(base.transform.position.z + Origin.position.z)))
		{
			GameManager.Instance.ItemDropServer(new ItemStack(itemValueWeapon, 1), base.transform.position + Origin.position + Vector3.up, Vector3.zero, ProjectileOwnerID, 1000f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return itemActionThrownWeapon.BuffActions;
	}
}
