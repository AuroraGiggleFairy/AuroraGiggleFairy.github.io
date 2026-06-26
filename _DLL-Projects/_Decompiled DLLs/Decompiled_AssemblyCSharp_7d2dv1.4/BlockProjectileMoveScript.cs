using UnityEngine;

public class BlockProjectileMoveScript : ProjectileMoveScript
{
	public BlockProjectileMoveScript()
	{
		hmOverride = 32;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkCollision()
	{
		hmOverride = 32;
		if (state != State.Active || ProjectileMoveScript.gameManager == null || ProjectileMoveScript.gameManager.World == null)
		{
			return;
		}
		Vector3 vector = ((!bOnIdealPos) ? idealPosition : base.transform.position);
		Vector3 vector2 = vector - previousPosition;
		float magnitude = vector2.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray = new Ray(previousPosition, vector2.normalized);
		int layerId = 0;
		EntityAlive entityAlive = (EntityAlive)firingEntity;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			layerId = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2);
		}
		int hitMask = ((hmOverride == 0) ? 80 : hmOverride);
		bool num = Voxel.Raycast(ProjectileMoveScript.gameManager.World, ray, magnitude, -538750981, hitMask, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(layerId);
		}
		if (num && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
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
				DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, -1);
				damageSourceEntity.AttackingItem = itemValueProjectile;
				int strength = (int)GetProjectileDamageEntity();
				bool num2 = component.IsDead();
				component.DamageEntity(damageSourceEntity, strength, _criticalHit: false);
				if (itemActionProjectile.BuffActions != null && component is EntityAlive)
				{
					ItemAction.ExecuteBuffActions(context: (text != null) ? GameUtils.GetChildTransformPath(component.transform, Voxel.voxelRayHitInfo.transform) : null, actions: itemActionProjectile.BuffActions, instigatorId: -1, target: component as EntityAlive, isCritical: false, hitLocation: damageSourceEntity.GetEntityDamageBodyPart(component));
				}
				if (!num2 && component.IsDead())
				{
					EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(ProjectileOwnerID) as EntityPlayer;
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
				Vector3 hitPos = Voxel.voxelRayHitInfo.hit.pos - vector2.normalized * 0.1f;
				Vector3i vector3i = World.worldToBlockPos(hitPos);
				if (!ProjectileMoveScript.gameManager.World.GetBlock(vector3i).isair)
				{
					vector3i = Voxel.OneVoxelStep(vector3i, hitPos, -vector2.normalized, out hitPos, out var _);
				}
				ProjectileMoveScript.gameManager.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, hitPos, vector3i, Quaternion.identity, itemActionProjectile.Explosion, ProjectileOwnerID, 0f, _bRemoveBlockAtExplPosition: false, itemValueProjectile);
			}
		}
		previousPosition = vector;
	}

	public float GetProjectileDamageEntity()
	{
		return itemActionProjectile.GetDamageEntity(itemValueProjectile);
	}
}
