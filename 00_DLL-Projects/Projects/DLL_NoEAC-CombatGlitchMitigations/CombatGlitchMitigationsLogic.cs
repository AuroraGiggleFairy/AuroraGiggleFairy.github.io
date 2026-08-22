using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombatGlitchMitigations
{
	public enum BarrierKind
	{
		None,
		Between,
		Embedded
	}

	public static class CombatGlitchMitigationsLogic
	{
		public const int LayerMaskBlocks = -538488845;

		const float SnapDelaySeconds = 1f;
		const float SnapCooldownSeconds = 2.5f;
		const float GetUpRayRadius = 0.45f;
		const float FaceHoldMaxSeconds = 1.4f;

		static readonly Dictionary<int, PendingSnap> PendingSnaps = new Dictionary<int, PendingSnap>();
		static readonly Dictionary<int, FaceHold> FaceHolds = new Dictionary<int, FaceHold>();
		static readonly List<Entity> NearbyScratch = new List<Entity>();

		struct PendingSnap
		{
			public float DueTime;
			public int PlayerEntityId;
			public BarrierKind Kind;
		}

		struct FaceHold
		{
			public int AttackerEntityId;
			public float StartDist;
			public float UntilTime;
		}

		public static bool IsServerContext()
		{
			try
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null
					&& SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsZombieMeleeAgainstPlayer(EntityAlive victim, DamageSource source, out EntityAlive attacker)
		{
			attacker = null;
			if (victim == null || !(victim is EntityPlayer) || source == null)
			{
				return false;
			}

			if (source.GetSource() != EnumDamageSource.External)
			{
				return false;
			}

			EnumDamageTypes damageType = source.GetDamageType();
			if (damageType == EnumDamageTypes.Heat
				|| damageType == EnumDamageTypes.Suicide
				|| damageType == EnumDamageTypes.Falling
				|| damageType == EnumDamageTypes.Disease
				|| damageType == EnumDamageTypes.Starvation
				|| damageType == EnumDamageTypes.Dehydration
				|| damageType == EnumDamageTypes.Radiation
				|| damageType == EnumDamageTypes.BloodLoss)
			{
				return false;
			}

			World world = victim.world;
			if (world == null)
			{
				return false;
			}

			attacker = world.GetEntity(source.getEntityId()) as EntityAlive;
			if (attacker == null || attacker is EntityPlayer || attacker.IsDead())
			{
				return false;
			}

			return HoldingIsMelee(attacker);
		}

		public static bool HoldingIsMelee(EntityAlive entity)
		{
			try
			{
				ItemClass holding = entity.inventory?.holdingItem;
				ItemAction action = holding?.Actions != null && holding.Actions.Length > 0
					? holding.Actions[0]
					: null;
				return action is ItemActionMelee || action is ItemActionDynamicMelee;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsSwinging(EntityAlive attacker)
		{
			try
			{
				AvatarController avatar = attacker.emodel?.avatarController;
				if (avatar != null && avatar.IsAnimationAttackPlaying())
				{
					return true;
				}

				return attacker.IsHoldingItemInUse(0);
			}
			catch
			{
				return true;
			}
		}

		public static float GetMeleeReach(EntityAlive attacker)
		{
			float range = 1.65f;
			float sphere = 0.1f;
			try
			{
				ItemClass holding = attacker.inventory?.holdingItem;
				ItemAction action = holding?.Actions != null && holding.Actions.Length > 0
					? holding.Actions[0]
					: null;
				if (action != null && action.Range > 0f)
				{
					range = action.Range;
					sphere = action.SphereRadius;
				}
			}
			catch
			{
			}

			return range + sphere;
		}

		public static float DistanceForReachCheck(EntityAlive attacker, EntityAlive victim)
		{
			Vector3 origin = attacker.GetMeleeRay().origin;
			return Vector3.Distance(origin, Chest(victim));
		}

		public static Vector3 Chest(EntityAlive entity)
		{
			try
			{
				return entity.getChestPosition();
			}
			catch
			{
				return entity.position + new Vector3(0f, Mathf.Max(0.8f, entity.GetEyeHeight() * 0.65f), 0f);
			}
		}

		public static BarrierKind FindBarrier(EntityAlive fromEntity, EntityAlive toEntity)
		{
			World world = fromEntity.world;
			if (world == null)
			{
				return BarrierKind.None;
			}

			Vector3 from = Chest(fromEntity);
			Vector3 to = Chest(toEntity);
			if (IsEmbedded(world, from))
			{
				return BarrierKind.Embedded;
			}

			Vector3 delta = to - from;
			float dist = delta.magnitude;
			if (dist < 0.05f)
			{
				return BarrierKind.None;
			}

			Vector3 dir = delta / dist;
			if (Physics.Raycast(from, dir, out RaycastHit hit, dist - 0.05f, LayerMaskBlocks, QueryTriggerInteraction.Ignore))
			{
				Block block = BlockAt(world, hit.point + dir * 0.02f);
				if (IsClosedMeleeBarrier(block))
				{
					return BarrierKind.Between;
				}
			}

			return BarrierKind.None;
		}

		public static bool IsEmbedded(World world, Vector3 chest)
		{
			if (!IsClosedMeleeBarrier(BlockAt(world, chest)))
			{
				return false;
			}

			return Physics.CheckSphere(chest, 0.08f, LayerMaskBlocks, QueryTriggerInteraction.Ignore);
		}

		public static bool IsClosedMeleeBarrier(Block block)
		{
			if (block == null)
			{
				return false;
			}

			return block.IsCollideMelee && block.IsCollideMovement;
		}

		public static bool IsLoosePlant(Block block)
		{
			if (block == null || !block.IsCollideMelee || block.IsCollideMovement)
			{
				return false;
			}

			if (IsTrapBlock(block))
			{
				return false;
			}

			string mat = "";
			try
			{
				if (block.blockMaterial != null)
				{
					mat = block.blockMaterial.id ?? "";
				}
			}
			catch
			{
			}

			mat = mat.ToLowerInvariant();
			if (mat.Contains("plant") || mat.Contains("grass") || mat.Contains("leaf") || mat.Contains("cloth"))
			{
				return true;
			}

			try
			{
				if (block.IsTerrainDecoration || block.IsDecoration)
				{
					return true;
				}
			}
			catch
			{
			}

			return true;
		}

		static bool IsTrapBlock(Block block)
		{
			string name = "";
			try
			{
				name = block.GetBlockName() ?? "";
			}
			catch
			{
			}

			return name.IndexOf("trap", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("Spike", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static Block BlockAt(World world, Vector3 worldPos)
		{
			Vector3i bp = World.worldToBlockPos(worldPos);
			BlockValue bv = world.GetBlock(bp);
			if (bv.isair || bv.Block == null)
			{
				return null;
			}

			if (bv.ischild)
			{
				bp = bv.Block.multiBlockPos.GetParentPos(bp, bv);
				bv = world.GetBlock(bp);
			}

			return bv.isair ? null : bv.Block;
		}

		public static bool ShouldCancelZombiePunch(
			EntityAlive player,
			EntityAlive attacker,
			out BarrierKind barrier,
			out string reason)
		{
			barrier = BarrierKind.None;
			reason = null;

			if (CombatGlitchMitigationsSettings.BlockPunchWithNoSwing && !IsSwinging(attacker))
			{
				reason = "no-swing";
				return true;
			}

			if (CombatGlitchMitigationsSettings.RecheckReachWhenPunchLands)
			{
				float reach = GetMeleeReach(attacker);
				if (DistanceForReachCheck(attacker, player) > reach)
				{
					reason = "out-of-reach";
					return true;
				}
			}

			if (CombatGlitchMitigationsSettings.BlockPunchThroughClosedBarrier)
			{
				barrier = FindBarrier(attacker, player);
				if (barrier != BarrierKind.None)
				{
					reason = barrier == BarrierKind.Embedded ? "embedded" : "barrier";
					return true;
				}
			}

			return false;
		}

		public static void ScheduleSnap(EntityAlive attacker, EntityAlive player, BarrierKind kind)
		{
			if (!CombatGlitchMitigationsSettings.SnapOutOfFloorAndWalls || kind == BarrierKind.None)
			{
				return;
			}

			if (IsInGetUpOrRagdoll(attacker))
			{
				return;
			}

			int id = attacker.entityId;
			if (PendingSnaps.ContainsKey(id))
			{
				return;
			}

			PendingSnaps[id] = new PendingSnap
			{
				DueTime = Time.time + SnapDelaySeconds,
				PlayerEntityId = player.entityId,
				Kind = kind
			};
		}

		public static void TickEntity(EntityAlive entity)
		{
			if (entity == null || entity.IsDead())
			{
				if (entity != null)
				{
					PendingSnaps.Remove(entity.entityId);
					FaceHolds.Remove(entity.entityId);
				}

				return;
			}

			TickFaceHold(entity);
			TickSnap(entity);
		}

		static void TickSnap(EntityAlive zombie)
		{
			if (!PendingSnaps.TryGetValue(zombie.entityId, out PendingSnap snap))
			{
				return;
			}

			if (Time.time < snap.DueTime)
			{
				return;
			}

			PendingSnaps.Remove(zombie.entityId);
			if (IsInGetUpOrRagdoll(zombie))
			{
				return;
			}

			EntityAlive player = zombie.world?.GetEntity(snap.PlayerEntityId) as EntityAlive;
			if (player == null)
			{
				return;
			}

			TrySnap(zombie, player, snap.Kind);
		}

		static void TrySnap(EntityAlive zombie, EntityAlive player, BarrierKind kind)
		{
			if (kind == BarrierKind.Embedded && TrySnapUp(zombie))
			{
				Log("snap-up #" + zombie.entityId);
				return;
			}

			if (TrySnapAway(zombie, player))
			{
				Log("snap-back #" + zombie.entityId);
			}
		}

		static bool TrySnapUp(EntityAlive zombie)
		{
			Vector3 start = zombie.position;
			for (int i = 1; i <= 14; i++)
			{
				Vector3 test = start + new Vector3(0f, 0.15f * i, 0f);
				Vector3 chest = Chest(zombie) + (test - start);
				if (!IsEmbedded(zombie.world, chest))
				{
					zombie.SetPosition(test);
					return true;
				}
			}

			return false;
		}

		static bool TrySnapAway(EntityAlive zombie, EntityAlive player)
		{
			Vector3 away = zombie.position - player.position;
			away.y = 0f;
			if (away.sqrMagnitude < 0.04f)
			{
				away = -zombie.GetLookVector();
				away.y = 0f;
			}

			if (away.sqrMagnitude < 0.01f)
			{
				return false;
			}

			away.Normalize();
			Vector3 start = zombie.position;
			for (int i = 1; i <= 8; i++)
			{
				Vector3 test = start + away * (0.25f * i);
				Vector3 chest = Chest(zombie) + (test - start);
				if (!IsEmbedded(zombie.world, chest))
				{
					zombie.SetPosition(test);
					return true;
				}
			}

			return false;
		}

		public static void StartFaceHold(EntityAlive zombie, EntityAlive attacker)
		{
			if (!CombatGlitchMitigationsSettings.BlockFaceHitStaggerTowardPlayer
				|| zombie == null
				|| attacker == null)
			{
				return;
			}

			Vector3 flat = zombie.position - attacker.position;
			flat.y = 0f;
			FaceHolds[zombie.entityId] = new FaceHold
			{
				AttackerEntityId = attacker.entityId,
				StartDist = Mathf.Max(0.15f, flat.magnitude),
				UntilTime = Time.time + FaceHoldMaxSeconds
			};
		}

		static void TickFaceHold(EntityAlive zombie)
		{
			if (!FaceHolds.TryGetValue(zombie.entityId, out FaceHold hold))
			{
				return;
			}

			bool animRunning = false;
			try
			{
				animRunning = zombie.emodel?.avatarController != null
					&& zombie.emodel.avatarController.IsAnimationHitRunning();
			}
			catch
			{
			}

			if (Time.time > hold.UntilTime)
			{
				FaceHolds.Remove(zombie.entityId);
				return;
			}

			if (!animRunning)
			{
				return;
			}

			EntityAlive attacker = zombie.world?.GetEntity(hold.AttackerEntityId) as EntityAlive;
			if (attacker == null)
			{
				FaceHolds.Remove(zombie.entityId);
				return;
			}

			Vector3 flat = zombie.position - attacker.position;
			flat.y = 0f;
			float dist = flat.magnitude;
			if (dist >= hold.StartDist - 0.02f || dist < 0.05f)
			{
				return;
			}

			flat *= hold.StartDist / dist;
			Vector3 pos = zombie.position;
			pos.x = attacker.position.x + flat.x;
			pos.z = attacker.position.z + flat.z;
			zombie.SetPosition(pos);
		}

		public static bool IsInGetUpOrRagdoll(EntityAlive entity)
		{
			try
			{
				if (entity.emodel != null && entity.emodel.IsRagdollActive)
				{
					return true;
				}

				if (entity.bodyDamage.CurrentStun == EnumEntityStunType.Getup)
				{
					return true;
				}
			}
			catch
			{
			}

			return false;
		}

		public static bool CapsuleOff(EntityAlive entity)
		{
			try
			{
				return entity.PhysicsTransform != null && !entity.PhysicsTransform.gameObject.activeInHierarchy;
			}
			catch
			{
				return false;
			}
		}

		public static bool TryRewritePlayerTarget(
			EntityAlive holder,
			ItemAction action,
			WorldRayHitInfo hit,
			bool allowPlantSkip,
			bool allowGetUp)
		{
			if (holder == null || !(holder is EntityPlayer) || hit == null || action == null)
			{
				return false;
			}

			World world = holder.world;
			if (world == null)
			{
				return false;
			}

			Ray ray = hit.ray.origin.sqrMagnitude > 0.0001f ? hit.ray : holder.GetLookRay();
			float range = action.Range > 0f ? action.Range + action.SphereRadius : 3.5f;
			if (range < 0.5f)
			{
				range = 3.5f;
			}

			bool hitIsEntity = hit.bHitValid && hit.tag != null && hit.tag.StartsWith("E_");
			if (hitIsEntity)
			{
				return false;
			}

			bool hitIsLoosePlant = false;
			if (allowPlantSkip && CombatGlitchMitigationsSettings.PlayerSwingThroughLoosePlants
				&& hit.bHitValid && hit.tag != null && GameUtils.IsBlockOrTerrain(hit.tag))
			{
				Block block = BlockAt(world, hit.hit.pos);
				hitIsLoosePlant = IsLoosePlant(block);
			}

			bool needGetUp = allowGetUp && CombatGlitchMitigationsSettings.CountHitsDuringGetUp;
			if (!hitIsLoosePlant && !needGetUp)
			{
				return false;
			}

			EntityAlive best = FindEntityOnRay(world, holder, ray, range, hitIsLoosePlant, needGetUp);
			if (best == null)
			{
				return false;
			}

			if (FindBarrier(holder, best) != BarrierKind.None)
			{
				return false;
			}

			ApplyEntityHit(hit, ray, best);
			return true;
		}

		static EntityAlive FindEntityOnRay(
			World world,
			EntityAlive holder,
			Ray ray,
			float range,
			bool wantPlantSkip,
			bool wantGetUp)
		{
			NearbyScratch.Clear();
			Bounds bounds = new Bounds(ray.origin + ray.direction * (range * 0.5f), Vector3.one * (range * 2f));
			world.GetEntitiesInBounds(typeof(EntityAlive), bounds, NearbyScratch);

			EntityAlive best = null;
			float bestDist = range + 0.01f;
			for (int i = 0; i < NearbyScratch.Count; i++)
			{
				EntityAlive other = NearbyScratch[i] as EntityAlive;
				if (other == null || other.entityId == holder.entityId || other.IsDead() || other is EntityPlayer)
				{
					continue;
				}

				Vector3 chest = Chest(other);
				Vector3 pelvis = other.position + new Vector3(0f, 0.35f, 0f);
				if (!RayNearPoint(ray, chest, range, GetUpRayRadius, out float chestDist)
					&& !RayNearPoint(ray, pelvis, range, GetUpRayRadius, out chestDist))
				{
					continue;
				}

				bool getUpHit = wantGetUp && CapsuleOff(other);
				if (!wantPlantSkip && !getUpHit)
				{
					continue;
				}

				if (wantPlantSkip && !getUpHit && CombatGlitchMitigationsSettings.PlayerSwingThroughLoosePlants)
				{
					// Plant skip: any living target on this ray is enough.
				}
				else if (!getUpHit)
				{
					continue;
				}

				if (chestDist < bestDist)
				{
					bestDist = chestDist;
					best = other;
				}
			}

			return best;
		}

		static bool RayNearPoint(Ray ray, Vector3 point, float maxDist, float radius, out float along)
		{
			along = Vector3.Dot(point - ray.origin, ray.direction);
			if (along < 0.05f || along > maxDist)
			{
				return false;
			}

			Vector3 closest = ray.origin + ray.direction * along;
			return (point - closest).sqrMagnitude <= radius * radius;
		}

		static void ApplyEntityHit(WorldRayHitInfo hit, Ray ray, EntityAlive target)
		{
			Vector3 chest = Chest(target);
			float dist = Vector3.Distance(ray.origin, chest);
			hit.bHitValid = true;
			hit.tag = "E_BP_Body";
			hit.ray = ray;
			hit.hit.pos = chest;
			hit.hit.distanceSq = dist * dist;
			try
			{
				Transform model = target.emodel != null ? target.emodel.GetModelTransform() : target.transform;
				hit.transform = model;
			}
			catch
			{
				hit.transform = target.transform;
			}
		}

		public static void Log(string message)
		{
			Console.WriteLine("CombatGlitchMitigations: " + message);
		}

		public static void TryDebugLine(EntityAlive from, Vector3 to, Color color)
		{
			if (!CombatGlitchMitigationsSettings.DebugDrawReach || from == null)
			{
				return;
			}

			try
			{
				DebugLines.Create(
					"CGM" + from.entityId,
					from.RootTransform,
					from.position,
					to,
					color,
					color,
					0.04f,
					0.02f,
					2f);
			}
			catch
			{
			}
		}
	}
}
