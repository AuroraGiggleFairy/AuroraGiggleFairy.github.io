using UnityEngine;

public class BlockProjectileMoveScript : ProjectileMoveScript
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkCollision()
	{
		GameManager instance = GameManager.Instance;
		if (!instance || instance.World == null)
		{
			return;
		}
		Vector3 vector = ((!isOnIdealPos) ? idealPosition : base.transform.position);
		Vector3 direction = vector - previousPosition;
		float magnitude = direction.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray = new Ray(previousPosition, direction);
		previousPosition = vector;
		int layerId = 0;
		EntityAlive entityAlive = (EntityAlive)firingEntity;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			layerId = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2);
		}
		hitMask = 32;
		bool num = Voxel.Raycast(instance.World, ray, magnitude, -538750981, hitMask, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(layerId);
		}
		if (!num || (!GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) && !Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			return;
		}
		base.enabled = false;
		Object.Destroy(base.transform.gameObject);
		Transform hitRootTransform = Voxel.voxelRayHitInfo.transform;
		string text = null;
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_BP_"))
		{
			text = Voxel.voxelRayHitInfo.tag.Substring("E_BP_".Length).ToLower();
			hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
		}
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component == null)
			{
				return;
			}
			DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, ProjectileOwnerID)
			{
				AttackingItem = itemValueProjectile
			};
			int strength = (int)GetProjectileDamageEntity();
			bool num2 = component.IsDead();
			component.DamageEntity(damageSourceEntity, strength, _criticalHit: false);
			if (itemActionProjectile.BuffActions != null && component is EntityAlive entityAlive2)
			{
				ItemAction.ExecuteBuffActions(context: (text != null) ? GameUtils.GetChildTransformPath(entityAlive2.transform, Voxel.voxelRayHitInfo.transform) : null, actions: itemActionProjectile.BuffActions, instigatorId: -1, target: entityAlive2, isCritical: false, hitLocation: damageSourceEntity.GetEntityDamageBodyPart(entityAlive2));
			}
			if (!num2 && component.IsDead())
			{
				EntityPlayer entityPlayer = instance.World.GetEntity(ProjectileOwnerID) as EntityPlayer;
				if (entityPlayer != null && EntityClass.list.ContainsKey(component.entityClass))
				{
					float value = EffectManager.GetValue(PassiveEffects.ElectricalTrapXP, entityPlayer.inventory.holdingItemItemValue, 0f, entityPlayer);
					if (value > 0f)
					{
						entityPlayer.AddKillXP(component as EntityAlive, value);
					}
				}
			}
		}
		if (itemActionProjectile.Explosion.ParticleIndex > 0)
		{
			Vector3 hitPos = Voxel.voxelRayHitInfo.hit.pos - direction.normalized * 0.1f;
			Vector3i vector3i = World.worldToBlockPos(hitPos);
			if (!instance.World.GetBlock(vector3i).isair)
			{
				vector3i = Voxel.OneVoxelStep(vector3i, hitPos, -direction.normalized, out hitPos, out var _);
			}
			instance.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, hitPos, vector3i, Quaternion.identity, itemActionProjectile.Explosion, ProjectileOwnerID, 0f, _bRemoveBlockAtExplPosition: false, itemValueProjectile);
		}
	}

	public float GetProjectileDamageEntity()
	{
		return itemActionProjectile.GetDamageEntity(itemValueProjectile);
	}
}
