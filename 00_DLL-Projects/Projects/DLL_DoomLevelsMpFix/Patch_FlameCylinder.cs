using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The flamethrower beam is a 2 m wide cylinder along the crosshair for the
	/// weapon's range. Each living enemy in that cylinder gets the burn once per
	/// shot. Bodies do not stop it. The first solid block along the crosshair
	/// ends it, and a block between the beam and a body keeps that body out.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionRanged), "fireShot")]
	internal static class Patch_FlameCylinder
	{
		private const float Radius = 1f;
		private const string ItemPrefix = "FlameThrowerQ";

		private static readonly List<Entity> Hits = new List<Entity>();

		private static void Postfix(int _shotIdx, ItemActionData _actionData)
		{
			if (_shotIdx != 0 || _actionData?.invData == null)
			{
				return;
			}

			try
			{
				Apply(_actionData);
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] flamethrower cylinder: " + e.Message);
			}
		}

		private static void Apply(ItemActionData action)
		{
			EntityAlive holder = action.invData.holdingEntity;
			ItemValue item = action.invData.itemValue;
			ItemClass itemClass = item != null ? item.ItemClass : null;
			if (holder == null || itemClass == null || itemClass.Name == null || !itemClass.Name.StartsWith(ItemPrefix, StringComparison.Ordinal))
			{
				return;
			}

			string tier = itemClass.Name.Substring(ItemPrefix.Length);
			if (tier.Length != 1 || tier[0] < '1' || tier[0] > '6')
			{
				return;
			}

			World world = holder.world;
			if (world == null)
			{
				return;
			}

			float range = EffectManager.GetValue(PassiveEffects.MaxRange, item, 0f, holder);
			if (range <= 0f)
			{
				return;
			}

			Ray look = holder.GetLookRay();
			Vector3 direction = look.direction;
			if (direction.sqrMagnitude < 0.0001f)
			{
				return;
			}

			direction.Normalize();
			float length = range;
			if (Voxel.Raycast(world, new Ray(look.origin, direction), range, false, false))
			{
				float blockDistance = Mathf.Sqrt(Voxel.voxelRayHitInfo.hit.distanceSq);
				if (blockDistance < length)
				{
					length = Mathf.Max(0f, blockDistance);
				}
			}

			Vector3 start = look.origin;
			Vector3 end = start + direction * length;
			Vector3 pad = Vector3.one * (Radius + 3f);
			Bounds box = new Bounds();
			box.SetMinMax(Vector3.Min(start, end) - pad, Vector3.Max(start, end) + pad);
			Hits.Clear();
			world.GetEntitiesInBounds(typeof(EntityAlive), box, Hits);
			string buff = "buffflamethrowerburnQ" + tier;
			for (int i = 0; i < Hits.Count; i++)
			{
				EntityAlive alive = Hits[i] as EntityAlive;
				if (alive == null || alive.entityId == holder.entityId || alive.IsDead() || alive.Buffs == null)
				{
					continue;
				}

				if (alive is EntityDrone drone && holder is EntityPlayer player && drone.isAlly(player))
				{
					continue;
				}

				if (alive.GetCVar("_underwater") >= 0.3f)
				{
					continue;
				}

				Bounds body = alive.boundingBox;
				Vector3 onBeam = ClosestPointOnSegment(body.center, start, end);
				Vector3 onBody = body.ClosestPoint(onBeam);
				Vector3 gap = onBody - onBeam;
				float gapDistance = gap.magnitude;
				if (gapDistance > Radius)
				{
					continue;
				}

				if (gapDistance > 0.05f && Voxel.Raycast(world, new Ray(onBeam, gap / gapDistance), gapDistance, false, false))
				{
					float blocked = Mathf.Sqrt(Voxel.voxelRayHitInfo.hit.distanceSq);
					if (blocked + 0.05f < gapDistance)
					{
						continue;
					}
				}

				alive.Buffs.AddBuff(buff, holder.entityId, true, false, -1f);
			}
		}

		private static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
		{
			Vector3 span = b - a;
			float lengthSquared = span.sqrMagnitude;
			if (lengthSquared < 0.0001f)
			{
				return a;
			}

			float t = Mathf.Clamp01(Vector3.Dot(point - a, span) / lengthSquared);
			return a + span * t;
		}
	}
}
