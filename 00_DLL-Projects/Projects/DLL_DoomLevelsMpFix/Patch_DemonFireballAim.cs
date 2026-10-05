using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The dedicated server owns the hit, including DoomSandbox projectile speed and
	/// any spread already applied. This does not change that shot, and it does not
	/// enable the fireball's collider. Bullets pass through because that capsule is
	/// disabled and the hit is the projectile ray.
	/// On the watching client only, the authored pieces are pulled onto that same
	/// projectile. A local owner gets that same bind and keeps the shot it fired.
	/// World-space clouds were left behind when the root moved, and the
	/// stretched swirl is a circle emitter flung backward along local -Z, which
	/// reads as a second object above a downward shot.
	/// </summary>
	internal static class DemonFireballAim
	{
		private static readonly HashSet<string> ProjectileItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"meleeHandDemonImpProjectile",
			"meleeHandDemonNightmareImpProjectile",
			"meleeHandDemonBabyCacoProjectile",
			"meleeHandDemonCacodemonProjectile",
			"meleeHandDemonHellKnightProjectile",
			"meleeHandDemonBaronHellProjectile",
			"meleeHandDemonPainElementalProjectile",
			"meleeHandDemonRevenantProjectile",
			"meleeHandDemonMancubusProjectile",
			"meleeHandDemonCyberProjectile",
		};

		[ThreadStatic]
		private static bool active;

		[ThreadStatic]
		private static Vector3 direction;

		internal static bool Active => active;

		internal static Vector3 Direction => direction;

		internal static void Clear()
		{
			active = false;
		}

		internal static bool IsDemonVomit(ItemActionVomit vomit, ItemActionData actionData)
		{
			EntityAlive entity = ItemInventoryAccess.Holding(actionData?.invData);
			if (vomit == null || entity == null || entity.inventory == null)
			{
				return false;
			}

			string[] names = vomit.MagazineItemNames;
			int index = ItemInventoryAccess.AmmoIndex(entity.inventory.holdingItemItemValue);
			if (names == null || index < 0 || index >= names.Length)
			{
				return false;
			}

			return ProjectileItems.Contains(names[index]);
		}

		internal static void Arm(ItemActionVomit vomit, ItemActionData actionData, Vector3 serverDirection, int userData)
		{
			active = false;
			if (userData > 0 || serverDirection.sqrMagnitude < 0.0001f)
			{
				return;
			}

			EntityAlive entity = ItemInventoryAccess.Holding(actionData?.invData);
			// Dedicated server owns the hit. This never runs there.
			if (GameManager.IsDedicatedServer || entity == null || !entity.isEntityRemote || !IsDemonVomit(vomit, actionData))
			{
				return;
			}

			active = true;
			direction = serverDirection.normalized;
		}

		private static bool EmitsContinuously(ParticleSystem system)
		{
			ParticleSystem.MinMaxCurve rate = system.emission.rateOverTime;
			if (rate.mode == ParticleSystemCurveMode.Constant)
			{
				return rate.constant > 0.01f;
			}

			if (rate.mode == ParticleSystemCurveMode.TwoConstants)
			{
				return rate.constantMax > 0.01f;
			}

			return false;
		}

		private static bool VelocityLeavesTheLine(ParticleSystem.MinMaxCurve curve)
		{
			if (curve.mode == ParticleSystemCurveMode.Constant)
			{
				return Mathf.Abs(curve.constant) > 1f;
			}

			if (curve.mode == ParticleSystemCurveMode.TwoConstants)
			{
				return Mathf.Abs(curve.constantMin) > 1f || Mathf.Abs(curve.constantMax) > 1f;
			}

			return false;
		}

		/// <summary>
		/// Ride the projectile transform. Do not clear a one-shot local core:
		/// that burst is the solid ball, and Play does not bring it back.
		/// </summary>
		private static void BindToProjectile(ParticleSystem system)
		{
			ParticleSystem.MainModule main = system.main;
			bool wasWorld = main.simulationSpace == ParticleSystemSimulationSpace.World;
			main.simulationSpace = ParticleSystemSimulationSpace.Local;

			// Particle collision can push the core off the ray when it touches
			// the ledge. The prefab capsule stays disabled, so this is not a bullet blocker.
			ParticleSystem.CollisionModule collision = system.collision;
			if (collision.enabled)
			{
				collision.enabled = false;
			}

			// Imp stretched cloud: local Z about -2 to -10, lifetime about 0.2s.
			// That is up to two meters off the ray. A short backward speed stays
			// on the shot and still gives the stretch renderer a direction.
			ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
			if (velocity.enabled && VelocityLeavesTheLine(velocity.z))
			{
				velocity.z = new ParticleSystem.MinMaxCurve(-0.45f, -0.15f);
			}

			// Same cloud is a circle of radius 0.7, so the sprites spawn off the ray
			// even after the transform offset is removed.
			ParticleSystem.ShapeModule shape = system.shape;
			if (shape.enabled && shape.shapeType == ParticleSystemShapeType.Circle && shape.radius > 0.15f)
			{
				shape.radius = 0.08f;
			}

			if (wasWorld || EmitsContinuously(system))
			{
				ParticleSystem.EmissionModule emission = system.emission;
				emission.enabled = false;
				system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}

		internal static void SetVisible(Transform projectile, bool visible)
		{
			if (projectile == null)
			{
				return;
			}

			Renderer[] renderers = projectile.GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = visible;
			}

			Light[] lights = projectile.GetComponentsInChildren<Light>(true);
			for (int i = 0; i < lights.Length; i++)
			{
				lights[i].enabled = visible;
			}
		}

		internal static void CollapseToCenter(Transform projectile)
		{
			Transform[] parts = projectile.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < parts.Length; i++)
			{
				if (parts[i] == projectile)
				{
					continue;
				}

				parts[i].localPosition = Vector3.zero;
			}
		}

		internal static void Silence(Transform projectile)
		{
			if (projectile == null)
			{
				return;
			}

			// Child offsets (the stretched piece sits 1m along local Z) and world
			// simulation both leave particles off the projectile. Stack the
			// transforms, then bind every system to the root before anything is drawn.
			SetVisible(projectile, false);
			CollapseToCenter(projectile);
			ParticleSystem[] systems = projectile.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < systems.Length; i++)
			{
				BindToProjectile(systems[i]);
			}
		}

		internal static void PlayOnShot(Transform projectile)
		{
			if (projectile == null)
			{
				return;
			}

			SetVisible(projectile, true);

			ParticleSystem[] systems = projectile.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < systems.Length; i++)
			{
				ParticleSystem system = systems[i];
				if (!EmitsContinuously(system))
				{
					continue;
				}

				ParticleSystem.EmissionModule emission = system.emission;
				emission.enabled = true;
				system.Play(true);
			}

			TrailRenderer[] trails = projectile.GetComponentsInChildren<TrailRenderer>(true);
			for (int i = 0; i < trails.Length; i++)
			{
				trails[i].Clear();
			}
		}

		internal static bool IsDemonProjectile(ProjectileMoveScript shot)
		{
			string name = shot?.itemProjectile?.GetItemName();
			return !string.IsNullOrEmpty(name) && ProjectileItems.Contains(name);
		}

		/// <summary>
		/// Singleplayer and a listen host own the demon. The picture binds there,
		/// and the shot they already fired stays theirs.
		/// </summary>
		internal static bool OwnsShot(ProjectileMoveScript shot)
		{
			if (shot == null || GameManager.IsDedicatedServer || !IsDemonProjectile(shot))
			{
				return false;
			}

			EntityAlive owner = ItemInventoryAccess.Holding(shot.actionData?.invData);
			return owner != null && !owner.isEntityRemote;
		}
	}

	/// <summary>
	/// The preload sits in the demon's hand and emits before the shot exists.
	/// On any machine that draws it, keep it quiet until Fire places the projectile.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.instantiateProjectile))]
	internal static class Patch_DemonFireballSilenceUntilFired
	{
		private static void Postfix(ItemActionLauncher __instance, ItemActionData _actionData, Transform __result)
		{
			if (__result == null || GameManager.IsDedicatedServer)
			{
				return;
			}

			ItemActionVomit vomit = __instance as ItemActionVomit;
			EntityAlive entity = ItemInventoryAccess.Holding(_actionData?.invData);
			if (vomit == null || entity == null || !DemonFireballAim.IsDemonVomit(vomit, _actionData))
			{
				return;
			}

			DemonFireballAim.Silence(__result);
		}
	}

	[HarmonyPatch(typeof(ItemActionVomit), nameof(ItemActionVomit.ItemActionEffects))]
	internal static class Patch_DemonFireballUseServerAim
	{
		private static void Prefix(ItemActionVomit __instance, ItemActionData _actionData, Vector3 _direction, int _userData)
		{
			DemonFireballAim.Arm(__instance, _actionData, _direction, _userData);
		}

		private static void Postfix()
		{
			DemonFireballAim.Clear();
		}
	}

	[HarmonyPatch(typeof(ItemActionRanged), "getDirectionRandomOffset")]
	internal static class Patch_DemonFireballKeepServerAim
	{
		private static void Postfix(ref Vector3 __result)
		{
			if (DemonFireballAim.Active)
			{
				__result = DemonFireballAim.Direction;
			}
		}
	}

	[HarmonyPatch(typeof(ItemActionRanged), "getDirectionOffset", new System.Type[] { typeof(ItemActionRanged.ItemActionDataRanged), typeof(Vector3), typeof(int) })]
	internal static class Patch_DemonFireballKeepServerAimOffset
	{
		private static void Postfix(ref Vector3 __result)
		{
			if (DemonFireballAim.Active)
			{
				__result = DemonFireballAim.Direction;
			}
		}
	}

	/// <summary>
	/// Vomit always creates a projectile, and the launcher also fires one that was
	/// already sitting in the hand. On a client those two leave on slightly
	/// different lines. Drop the hand copy so one shot is one fireball.
	/// </summary>
	[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.ItemActionEffects))]
	internal static class Patch_DemonFireballSingleVisual
	{
		private static void Prefix(ItemActionData _actionData)
		{
			EntityAlive entity = ItemInventoryAccess.Holding(_actionData?.invData);
			if (!DemonFireballAim.Active || entity == null || !entity.isEntityRemote)
			{
				return;
			}

			ItemActionLauncher.ItemActionDataLauncher data = _actionData as ItemActionLauncher.ItemActionDataLauncher;
			if (data == null)
			{
				return;
			}

			for (int i = 0; i < data.projectileTs.Count; i++)
			{
				Transform held = data.projectileTs[i];
				if (held != null)
				{
					UnityEngine.Object.Destroy(held.gameObject);
				}
			}

			data.projectileTs.Clear();
		}
	}

	/// <summary>
	/// Face and place the client graphic on the shot the server already sent.
	/// Nothing is retargeted on the dedicated server.
	/// </summary>
	[HarmonyPatch(typeof(ProjectileMoveScript), nameof(ProjectileMoveScript.Fire))]
	internal static class Patch_DemonFireballParticlesFollowShot
	{
		private static void Postfix(ProjectileMoveScript __instance)
		{
			if (__instance == null || GameManager.IsDedicatedServer)
			{
				return;
			}

			if (DemonFireballAim.Active)
			{
				Vector3 dir = DemonFireballAim.Direction;
				if (dir.sqrMagnitude > 0.0001f)
				{
					Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
					__instance.transform.rotation = Quaternion.LookRotation(dir, up);
				}

				if (!__instance.isOnIdealPos && __instance.idealPosition.sqrMagnitude > 0.01f)
				{
					__instance.previousPosition = __instance.idealPosition;
					__instance.transform.position = __instance.idealPosition - Origin.position;
					__instance.isOnIdealPos = true;
				}

				DemonFireballAim.PlayOnShot(__instance.transform);
				return;
			}

			if (DemonFireballAim.OwnsShot(__instance))
			{
				DemonFireballAim.PlayOnShot(__instance.transform);
			}
		}
	}
}
