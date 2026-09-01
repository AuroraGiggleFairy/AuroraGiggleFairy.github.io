using System.Collections.Generic;
using UnityEngine;

namespace FullautoLauncher.Scripts.ProjectileManager;

public class ProjectileParams
{
	public class ItemInfo
	{
		public ItemActionProjectile itemActionProjectile;

		public ItemClass itemProjectile;

		public ItemValue itemValueProjectile;

		public ItemValue itemValueLauncher;

		public ItemActionData actionData;

		public IProjectileItemGroup group;
	}

	public int ProjectileID;

	public ItemInfo info;

	public Vector3 flyDirection;

	public Vector3 renderPosition;

	public Vector3 velocity;

	public Vector3 previousPosition;

	public Vector3 currentPosition;

	public Vector3 gravity;

	public Vector3 moveDir;

	public float timeShotStarted;

	public int hmOverride;

	public float radius;

	public bool bOnIdealPosition = false;

	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	public ProjectileParams(int projectileID)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		ProjectileID = projectileID;
	}

	public void Fire(ItemInfo _info, Vector3 _idealStartPosition, Vector3 _realStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		info = _info;
		flyDirection = ((Vector3)(ref _flyDirection)).normalized;
		moveDir = flyDirection;
		previousPosition = (currentPosition = _idealStartPosition);
		renderPosition = _realStartPosition;
		velocity = ((Vector3)(ref flyDirection)).normalized * EffectManager.GetValue((PassiveEffects)71, info.itemValueLauncher, info.itemActionProjectile.Velocity, (EntityAlive)(object)((_firingEntity is EntityAlive) ? _firingEntity : null), (Recipe)null, default(FastTags<Global>), true, true, true, true, true, 1, true, false);
		hmOverride = _hmOverride;
		radius = _radius;
		waterCollisionParticles.Init(_firingEntity.entityId, info.itemProjectile.MadeOfMaterial.SurfaceCategory, "water", 16);
		gravity = Vector3.up * EffectManager.GetValue((PassiveEffects)70, info.itemValueLauncher, info.itemActionProjectile.Gravity, (EntityAlive)(object)((_firingEntity is EntityAlive) ? _firingEntity : null), (Recipe)null, default(FastTags<Global>), true, true, true, true, true, 1, true, false);
		timeShotStarted = Time.time;
	}

	public override int GetHashCode()
	{
		return ProjectileID;
	}

	public bool UpdatePosition()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time - timeShotStarted;
		if (num >= info.itemActionProjectile.LifeTime)
		{
			return true;
		}
		if (num >= info.itemActionProjectile.FlyTime)
		{
			velocity += gravity * Time.fixedDeltaTime;
		}
		moveDir = velocity * Time.fixedDeltaTime;
		previousPosition = currentPosition;
		currentPosition += moveDir;
		renderPosition += moveDir;
		if (!bOnIdealPosition)
		{
			bOnIdealPosition = num > 0.5f;
		}
		if (!bOnIdealPosition)
		{
			renderPosition = Vector3.Lerp(renderPosition, currentPosition, num * 2f);
		}
		return false;
	}

	public bool CheckCollision(EntityAlive entityAlive)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Expected O, but got Unknown
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Expected O, but got Unknown
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		World world = GameManager.Instance.World;
		Vector3 val = currentPosition - previousPosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < 0.04f)
		{
			return false;
		}
		Ray val2 = default(Ray);
		((Ray)(ref val2))._002Ector(previousPosition, val);
		waterCollisionParticles.CheckCollision(((Ray)(ref val2)).origin, ((Ray)(ref val2)).direction, magnitude, ((Object)(object)entityAlive != (Object)null) ? ((Entity)entityAlive).entityId : (-1));
		int num = ((hmOverride == 0) ? 80 : hmOverride);
		if (Voxel.Raycast(world, val2, magnitude, -538750997, num, radius) && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			if ((Object)(object)entityAlive != (Object)null && !((Entity)entityAlive).isEntityRemote)
			{
				ref EntityAlive other = ref entityAlive.MinEventContext.Other;
				Entity obj = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo);
				other = (EntityAlive)(object)((obj is EntityAlive) ? obj : null);
				AttackHitInfo val3 = new AttackHitInfo
				{
					WeaponTypeTag = ItemActionAttack.RangedTag
				};
				ItemActionAttack.Hit(Voxel.voxelRayHitInfo, ((Entity)entityAlive).entityId, (EnumDamageTypes)1, ((ItemActionAttack)info.itemActionProjectile).GetDamageBlock(info.itemValueLauncher, ItemActionAttack.GetBlockHit(world, Voxel.voxelRayHitInfo), entityAlive, info.actionData.indexInEntityOfAction), ((ItemActionAttack)info.itemActionProjectile).GetDamageEntity(info.itemValueLauncher, entityAlive, info.actionData.indexInEntityOfAction), 1f, 1f, EffectManager.GetValue((PassiveEffects)194, info.itemValueLauncher, info.itemProjectile.CritChance.Value, entityAlive, (Recipe)null, info.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false), ItemAction.GetDismemberChance(info.actionData, Voxel.voxelRayHitInfo), info.itemProjectile.MadeOfMaterial.SurfaceCategory, ((ItemActionAttack)info.itemActionProjectile).GetDamageMultiplier(), ((ItemAction)info.itemActionProjectile).BuffActions, val3, 1, ((ItemAction)info.itemActionProjectile).ActionExp, ((ItemAction)info.itemActionProjectile).ActionExpBonusMultiplier, (ItemActionAttack)null, (Dictionary<string, Bonuses>)null, (EnumAttackMode)0, (Dictionary<string, string>)null, -1, info.itemValueLauncher, false, false, true, (Dictionary<string, float>)null);
				if ((Object)(object)entityAlive.MinEventContext.Other == (Object)null)
				{
					entityAlive.FireEvent((MinEventTypes)31, true);
				}
				entityAlive.FireEvent((MinEventTypes)97, false);
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = info.itemValueProjectile;
				MinEventParams.CachedEventParam.Other = entityAlive.MinEventContext.Other;
				info.itemProjectile.FireEvent((MinEventTypes)97, MinEventParams.CachedEventParam);
				BlockValue val6;
				if (info.itemActionProjectile.Explosion.ParticleIndex > 0)
				{
					Vector3 val4 = Voxel.voxelRayHitInfo.hit.pos - normalized * 0.1f;
					Vector3i val5 = World.worldToBlockPos(val4);
					val6 = ((WorldBase)world).GetBlock(val5);
					if (!((BlockValue)(ref val6)).isair)
					{
						BlockFace val7 = default(BlockFace);
						val5 = Voxel.OneVoxelStep(val5, val4, -normalized, ref val4, ref val7);
					}
					GameManager.Instance.ExplosionServer(val4, val5, Quaternion.identity, info.itemActionProjectile.Explosion, ((Entity)entityAlive).entityId, 0f, false, info.itemValueProjectile);
				}
				else if (info.itemProjectile.IsSticky)
				{
					GameRandom gameRandom = ((WorldBase)world).GetGameRandom();
					if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
					{
						float randomFloat = gameRandom.RandomFloat;
						ItemValue itemValueLauncher = info.itemValueLauncher;
						FastTags<Global> itemTags = info.itemProjectile.ItemTags;
						val6 = ((HitInfoDetails)(ref Voxel.voxelRayHitInfo.fmcHit)).blockValue;
						if (randomFloat < EffectManager.GetValue((PassiveEffects)72, itemValueLauncher, 0.5f, entityAlive, (Recipe)null, itemTags | FastTags<Global>.Parse(((BlockValue)(ref val6)).Block.blockMaterial.SurfaceCategory), true, true, true, true, true, 1, true, false))
						{
							ProjectileManager.AddProjectileItem(CreateStickyProjectile(info, entityAlive, this), -1, Voxel.voxelRayHitInfo.hit.pos, normalized, info.itemValueProjectile.type);
						}
						else
						{
							GameManager instance = GameManager.Instance;
							Vector3 pos = Voxel.voxelRayHitInfo.hit.pos;
							Quaternion val8 = Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace);
							Color white = Color.white;
							val6 = ((HitInfoDetails)(ref Voxel.voxelRayHitInfo.fmcHit)).blockValue;
							instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", pos, val8, 1f, white, $"{((BlockValue)(ref val6)).Block.blockMaterial.SurfaceCategory}hit{info.itemProjectile.MadeOfMaterial.SurfaceCategory}", (Transform)null, 1f, ""), ((Entity)entityAlive).entityId, false, false);
						}
					}
					else if (gameRandom.RandomFloat < EffectManager.GetValue((PassiveEffects)72, info.itemValueLauncher, 0.5f, entityAlive, (Recipe)null, info.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false))
					{
						int num2 = ProjectileManager.AddProjectileItem(CreateStickyProjectile(info, entityAlive, this), -1, Voxel.voxelRayHitInfo.hit.pos, normalized, info.itemValueProjectile.type);
						Utils.SetLayerRecursively(((Component)ProjectileManager.GetProjectile(num2)).gameObject, 14, (string[])null);
					}
					else
					{
						GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, "bullethitwood", (Transform)null, 1f, ""), ((Entity)entityAlive).entityId, false, false);
					}
				}
			}
			return true;
		}
		return false;
	}

	public static Transform CreateStickyProjectile(ItemInfo info, EntityAlive entity, ProjectileParams par)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Transform stickyTransform = info.group.GetStickyTransform();
		((Component)stickyTransform).gameObject.SetActive(true);
		stickyTransform.position = par.currentPosition;
		stickyTransform.forward = ((Vector3)(ref par.velocity)).normalized;
		ProjectileMoveScript orAddComponent = Extensions.GetOrAddComponent<ProjectileMoveScript>(((Component)stickyTransform).gameObject);
		orAddComponent.itemActionProjectile = info.itemActionProjectile;
		orAddComponent.itemValueProjectile = info.itemValueProjectile;
		orAddComponent.itemValueLauncher = info.itemValueLauncher;
		ref ItemActionDataLauncher actionData = ref orAddComponent.actionData;
		ItemActionData actionData2 = info.actionData;
		actionData = (ItemActionDataLauncher)(object)((actionData2 is ItemActionDataLauncher) ? actionData2 : null);
		orAddComponent.itemProjectile = info.itemProjectile;
		orAddComponent.ProjectileOwnerID = ((Entity)entity).entityId;
		return stickyTransform;
	}
}
