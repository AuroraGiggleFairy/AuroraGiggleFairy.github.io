using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The flamethrower beam is a 2 m wide cylinder along the crosshair for the
	/// weapon's range. EntityPenetrationCount is how many bodies past the first
	/// one also burn. The nearest bodies in the cylinder are the ones that burn.
	/// The first solid block along the crosshair ends the beam, and a block
	/// between the beam and a body keeps that body out. Each burned body grants
	/// one skill hit.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionRanged), "fireShot")]
	internal static class Patch_FlameCylinder
	{
		private const float Radius = 1f;
		private const string ItemPrefix = "FlameThrowerQ";

		private static readonly List<Entity> Hits = new List<Entity>();
		private static readonly List<BurnTarget> Burns = new List<BurnTarget>();

		private struct BurnTarget
		{
			public EntityAlive Alive;
			public float Along;
		}

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
			EntityAlive holder = ItemInventoryAccess.Holding(action.invData);
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

			int extra = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, item, 0f, holder));
			if (extra < 0)
			{
				extra = 0;
			}

			int maxBodies = extra + 1;
			Ray look = holder.GetLookRay();
			Vector3 direction = look.direction;
			if (direction.sqrMagnitude < 0.0001f)
			{
				return;
			}

			direction.Normalize();
			Vector3 start = look.origin;
			float length = FirstSolidDistance(world, start, direction, range);
			Vector3 end = start + direction * length;
			Vector3 pad = Vector3.one * (Radius + 3f);
			Bounds box = new Bounds();
			box.SetMinMax(Vector3.Min(start, end) - pad, Vector3.Max(start, end) + pad);
			Hits.Clear();
			Burns.Clear();
			world.GetEntitiesInBounds(typeof(EntityAlive), box, Hits);
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

				if (AlreadyQueued(alive.entityId))
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

				if (gapDistance > 0.05f && SolidBetween(world, onBeam, onBody))
				{
					continue;
				}

				Burns.Add(new BurnTarget
				{
					Alive = alive,
					Along = Vector3.Dot(onBeam - start, direction)
				});
			}

			Burns.Sort(CompareAlong);
			int count = Mathf.Min(maxBodies, Burns.Count);
			string buff = "buffflamethrowerburnQ" + tier;
			for (int i = 0; i < count; i++)
			{
				EntityAlive alive = Burns[i].Alive;
				if (alive == null || alive.Buffs == null || alive.IsDead())
				{
					continue;
				}

				alive.Buffs.AddBuff(buff, holder.entityId, true, false, -1f);
				GrantSkillHit(holder, alive, item);
			}
		}

		private static bool AlreadyQueued(int entityId)
		{
			for (int i = 0; i < Burns.Count; i++)
			{
				if (Burns[i].Alive != null && Burns[i].Alive.entityId == entityId)
				{
					return true;
				}
			}

			return false;
		}

		private static int CompareAlong(BurnTarget a, BurnTarget b)
		{
			return a.Along.CompareTo(b.Along);
		}

		// Bodies do not end the beam. The distance is the first solid block, or the full range.
		private static float FirstSolidDistance(World world, Vector3 start, Vector3 direction, float range)
		{
			float traveled = 0f;
			Vector3 origin = start;
			for (int step = 0; step < 48 && traveled < range - 0.02f; step++)
			{
				float remaining = range - traveled;
				if (!Voxel.Raycast(world, new Ray(origin, direction), remaining, false, false))
				{
					return range;
				}

				string tag = Voxel.voxelRayHitInfo.tag;
				float hop = Mathf.Sqrt(Voxel.voxelRayHitInfo.hit.distanceSq);
				if (tag != null && tag.StartsWith("E_", StringComparison.Ordinal))
				{
					float advance = hop + 0.15f;
					Entity body = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo);
					if (body != null)
					{
						float exit = ExitDistance(body.boundingBox, origin, direction);
						if (exit > hop)
						{
							advance = exit + 0.05f;
						}
					}

					if (advance < 0.05f)
					{
						advance = 0.15f;
					}

					if (traveled + advance >= range)
					{
						return range;
					}

					traveled += advance;
					origin += direction * advance;
					continue;
				}

				return Mathf.Clamp(traveled + hop, 0f, range);
			}

			return Mathf.Clamp(traveled, 0f, range);
		}

		private static bool SolidBetween(World world, Vector3 from, Vector3 to)
		{
			Vector3 gap = to - from;
			float distance = gap.magnitude;
			if (distance <= 0.05f)
			{
				return false;
			}

			float solid = FirstSolidDistance(world, from, gap / distance, distance);
			return solid + 0.05f < distance;
		}

		private static float ExitDistance(Bounds bounds, Vector3 origin, Vector3 direction)
		{
			float tFar = float.PositiveInfinity;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			for (int axis = 0; axis < 3; axis++)
			{
				float o = axis == 0 ? origin.x : (axis == 1 ? origin.y : origin.z);
				float d = axis == 0 ? direction.x : (axis == 1 ? direction.y : direction.z);
				float bmin = axis == 0 ? min.x : (axis == 1 ? min.y : min.z);
				float bmax = axis == 0 ? max.x : (axis == 1 ? max.y : max.z);
				if (Mathf.Abs(d) < 0.00001f)
				{
					if (o < bmin || o > bmax)
					{
						return 0.15f;
					}

					continue;
				}

				float t1 = (bmin - o) / d;
				float t2 = (bmax - o) / d;
				if (t1 > t2)
				{
					float swap = t1;
					t1 = t2;
					t2 = swap;
				}

				if (t2 < tFar)
				{
					tFar = t2;
				}
			}

			if (float.IsInfinity(tFar) || tFar < 0f)
			{
				return 0.15f;
			}

			return tFar;
		}

		private static void GrantSkillHit(EntityAlive holder, EntityAlive target, ItemValue item)
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || !net.IsServer || holder.MinEventContext == null)
			{
				return;
			}

			MinEventParams context = holder.MinEventContext;
			context.Self = holder;
			context.Other = target;
			context.ItemValue = item;
			context.StartPosition = holder.position;
			holder.FireEvent(MinEventTypes.onSelfDamagedOther, false);
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
