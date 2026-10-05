using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DoomAutoPickup;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The shipped bag check bails on every client, which includes a listen host.
	/// A pure client still bails. A listen host keeps the existing server grant.
	/// </summary>
	[HarmonyPatch(typeof(AutoPickup), nameof(AutoPickup.Touched))]
	internal static class Patch_AutoPickupListenHost
	{
		private static bool ClientOnly(ConnectionManager net)
		{
			return net != null && net.IsClient && !net.IsServer;
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo getter = AccessTools.PropertyGetter(typeof(ConnectionManager), "IsClient");
			MethodInfo replacement = AccessTools.Method(typeof(Patch_AutoPickupListenHost), nameof(ClientOnly));
			foreach (CodeInstruction instruction in instructions)
			{
				if (getter != null && replacement != null && instruction.Calls(getter))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = replacement;
				}

				yield return instruction;
			}
		}
	}

	internal static class Patch_LootBagAutoPickup
	{
		internal static void Patch(Harmony harmony)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			MethodInfo start = typeof(EntityLootContainer).GetMethod("Start", flags);
			MethodInfo awake = typeof(EntityLootContainer).GetMethod("Awake", flags);
			MethodInfo target = start != null ? start : awake;
			MethodInfo postfix = typeof(Patch_LootBagAutoPickup).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
			if (target == null || postfix == null)
			{
				return;
			}

			harmony.Patch(target, postfix: new HarmonyMethod(postfix));
		}

		private static void Postfix(EntityLootContainer __instance)
		{
			Apply(__instance);
		}

		internal static void Apply(EntityLootContainer instance)
		{
			if (instance == null)
			{
				return;
			}

			AutoPickup pickup = instance.GetComponent<AutoPickup>();
			if (pickup == null || instance.GetComponent<AutoPickupServerScan>() != null)
			{
				return;
			}

			instance.gameObject.AddComponent<AutoPickupServerScan>().Pickup = pickup;
		}

	}

	/// <summary>
	/// Start only attaches the walk check when the gun component already exists.
	/// Bags already on the ground, in a level or on the main map, get it on this sweep.
	/// </summary>
	internal static class GunDropScan
	{
		private const float TickSeconds = 0.25f;

		private static readonly HashSet<string> Classes = new HashSet<string>
		{
			"EntityLootContainerFormerSoldierBM",
			"EntityLootContainerFormerSergeantBM",
			"EntityLootContainerFormerCommandoBM"
		};

		private static float _next;

		internal static void Tick()
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || net.IsClient || Time.unscaledTime < _next)
			{
				return;
			}

			_next = Time.unscaledTime + TickSeconds;
			World world = GameManager.Instance?.World;
			if (world?.Entities?.list == null)
			{
				return;
			}

			List<Entity> list = world.Entities.list;
			for (int i = 0; i < list.Count; i++)
			{
				EntityLootContainer bag = list[i] as EntityLootContainer;
				if (bag == null || bag.GetComponent<AutoPickupServerScan>() != null)
				{
					continue;
				}

				string name = EntityClass.list[bag.entityClass]?.entityClassName;
				if (string.IsNullOrEmpty(name) || !Classes.Contains(name))
				{
					continue;
				}

				AutoPickup pickup = bag.GetComponent<AutoPickup>();
				if (pickup == null)
				{
					pickup = bag.gameObject.AddComponent<AutoPickup>();
					pickup.Configure(bag);
				}

				bag.gameObject.AddComponent<AutoPickupServerScan>().Pickup = pickup;
			}
		}
	}

	internal sealed class AutoPickupServerScan : MonoBehaviour
	{
		private const float TriggerRadius = 0.8f;

		internal AutoPickup Pickup;

		private SphereCollider _sphere;
		private bool _looked;

		private void Update()
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (Pickup == null || net == null || net.IsClient)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			SphereCollider sphere = Trigger();
			Vector3 center = sphere != null ? sphere.transform.position : transform.position;
			float radius = Radius(sphere);

			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null || player.IsDead() || !PickupBody.OverlapsSphere(player, center + Origin.position, radius, out Collider body))
				{
					continue;
				}

				Pickup.Touched(body);
			}
		}

		private SphereCollider Trigger()
		{
			if (_looked)
			{
				return _sphere;
			}

			_looked = true;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name != "DoomAutoPickupTrigger")
				{
					continue;
				}

				_sphere = child.GetComponent<SphereCollider>();
				break;
			}

			return _sphere;
		}

		private static float Radius(SphereCollider sphere)
		{
			if (sphere == null)
			{
				return TriggerRadius;
			}

			Vector3 scale = sphere.transform.lossyScale;
			float mag = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
			if (mag < 0.01f)
			{
				mag = 1f;
			}

			return sphere.radius * mag;
		}

	}

	/// <summary>
	/// The original pickups grant when the player's body meets the volume:
	/// the sprite's trigger box for a ground item, the 0.8 sphere for a weapon bag.
	/// This uses that same body and volume. It does not ask the collider for a closest point.
	/// </summary>
	internal static class PickupBody
	{
		internal static bool Overlaps(EntityPlayer player, Collider volume, out Collider touched)
		{
			touched = OnPlayer(player);
			if (player == null || volume == null || touched == null)
			{
				return false;
			}

			Bounds box = volume.bounds;
			box.center += Origin.position;
			float radius = Radius(player);
			Vector3 grow = Vector3.one * radius;
			return SegmentHits(Bottom(player, radius), Top(player, radius), box.min - grow, box.max + grow);
		}

		internal static bool OverlapsSphere(EntityPlayer player, Vector3 worldCenter, float triggerRadius, out Collider touched)
		{
			touched = OnPlayer(player);
			if (player == null || touched == null || triggerRadius <= 0f)
			{
				return false;
			}

			float radius = Radius(player);
			Vector3 bottom = Bottom(player, radius);
			Vector3 top = Top(player, radius);
			Vector3 axis = top - bottom;
			float lengthSq = axis.sqrMagnitude;
			float along = lengthSq < 0.0001f ? 0f : Mathf.Clamp01(Vector3.Dot(worldCenter - bottom, axis) / lengthSq);
			Vector3 nearest = bottom + axis * along;
			float allow = triggerRadius + radius;
			return (nearest - worldCenter).sqrMagnitude <= allow * allow;
		}

		private static Collider OnPlayer(EntityPlayer player)
		{
			return player != null ? player.GetComponentInChildren<Collider>() : null;
		}

		private static float Radius(EntityPlayer player)
		{
			if (player != null && player.m_characterController != null)
			{
				float radius = player.m_characterController.GetRadius();
				if (radius > 0.01f)
				{
					return radius;
				}
			}

			return 0.4f;
		}

		private static float Height(EntityPlayer player)
		{
			float height = player != null ? player.GetHeight() : 0f;
			return height > 0.01f ? height : 1.8f;
		}

		private static Vector3 Bottom(EntityPlayer player, float radius)
		{
			return player.position + Vector3.up * radius;
		}

		private static Vector3 Top(EntityPlayer player, float radius)
		{
			float height = Height(player);
			return player.position + Vector3.up * Mathf.Max(radius, height - radius);
		}

		private static bool SegmentHits(Vector3 a, Vector3 b, Vector3 min, Vector3 max)
		{
			Vector3 delta = b - a;
			float enter = 0f;
			float exit = 1f;
			for (int axis = 0; axis < 3; axis++)
			{
				float origin = axis == 0 ? a.x : axis == 1 ? a.y : a.z;
				float dir = axis == 0 ? delta.x : axis == 1 ? delta.y : delta.z;
				float low = axis == 0 ? min.x : axis == 1 ? min.y : min.z;
				float high = axis == 0 ? max.x : axis == 1 ? max.y : max.z;
				if (Mathf.Abs(dir) < 0.00001f)
				{
					if (origin < low || origin > high)
					{
						return false;
					}

					continue;
				}

				float inv = 1f / dir;
				float t1 = (low - origin) * inv;
				float t2 = (high - origin) * inv;
				if (t1 > t2)
				{
					float swap = t1;
					t1 = t2;
					t2 = swap;
				}

				if (t1 > enter)
				{
					enter = t1;
				}

				if (t2 < exit)
				{
					exit = t2;
				}

				if (enter > exit)
				{
					return false;
				}
			}

			return true;
		}
	}
}
