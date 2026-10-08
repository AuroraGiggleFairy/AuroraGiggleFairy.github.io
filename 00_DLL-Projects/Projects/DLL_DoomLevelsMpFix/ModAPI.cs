using System;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			Harmony harmony = new Harmony("com.doom.multiplayer");
			Patch_LootBagAutoPickup.Patch(harmony);
			PackageEmit.Prepare(
				typeof(NetPackageDoomInstance),
				typeof(NetPackageDoomMapPaint),
				typeof(NetPackageDoomMapReport),
				typeof(NetPackageDoomMapTally),
				typeof(NetPackageDoomPickupFx),
				typeof(NetPackageDoomSecretFound),
				typeof(NetPackageDoomShotSwitch),
				typeof(NetPackageDoomSpawnCube),
				typeof(NetPackageDoomTeleportFx));
			foreach (Type type in typeof(ModAPI).Assembly.GetTypes())
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Length == 0)
				{
					continue;
				}

				try
				{
					harmony.CreateClassProcessor(type).Patch();
				}
				catch (Exception e)
				{
					Debug.LogError("[DoomMultiplayer] skipped patch " + type.Name + ": " + e.Message);
				}
			}

			ModEvents.GameUpdate.RegisterHandler(OnUpdate);
			ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnSpawn);
			ModEvents.EntityKilled.RegisterHandler(OnKilled);
			ModEvents.WorldShuttingDown.RegisterHandler(OnWorldDown);
			ModEvents.GameStartDone.RegisterHandler(OnGameStart);
			SpawnQueue.NoLimit = true;
			BrightnessCap.Apply();
			Debug.Log("[DoomMultiplayer] loaded from " + modInstance.Path);
		}

		private static void OnUpdate(ref ModEvents.SGameUpdateData data)
		{
			try
			{
				BrightnessCap.Apply();
				HazardScan.Tick();
				LevelTriggerScan.Tick();
				ItemPickups.Tick();
				GunDropScan.Tick();
				TextureAnimFix.Tick();
				ReturnTeleport.Tick();
				InstanceSync.Watch();
				PartyHud.Tick();
				ScreenFxRetry.Tick();
				RunStats.Tick();
				MapTally.Tick();
				MapTally.SyncTracker();
				MapExplore.Tick();
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] update: " + e.Message);
			}
		}

		private static void OnSpawn(ref ModEvents.SPlayerSpawnedInWorldData data)
		{
			try
			{
				DummyMagazine.FillPlayer(PartyMembers.Find(GameManager.Instance?.World, data.EntityId));
				ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
				if (net == null || net.IsServer || !net.IsClient)
				{
					EntityPlayer returned = PartyMembers.Find(GameManager.Instance?.World, data.EntityId);
					RunStats.Rejoin(returned);
				}

				if (net != null && !net.IsClient)
				{
					if (Instances.IsInside(data.EntityId))
					{
						EntityPlayer spawned = PartyMembers.Find(GameManager.Instance?.World, data.EntityId);
						if (spawned != null)
						{
							InstanceSync.SendEnter(spawned);
						}
					}
					else
					{
						InstanceSync.SendLeave(data.EntityId);
					}

					InstanceSync.SendAllTo(data.EntityId);
				}

				PartyHud.RequestRefresh();
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] spawn sync: " + e.Message);
			}
		}

		private static void OnKilled(ref ModEvents.SEntityKilledData data)
		{
			try
			{
				RunStats.MonsterKilled(data.KilledEntitiy);
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] kill progress: " + e.Message);
			}
		}

		private static void OnGameStart(ref ModEvents.SGameStartDoneData data)
		{
			try
			{
				LootDropPreload.Run();
				BrightnessCap.Apply();
				BrightnessCap.Refresh();
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] loot preload: " + e.Message);
			}
		}

		private static void OnWorldDown(ref ModEvents.SWorldShuttingDownData data)
		{
			RunStats.Flush();
			TextureAnimFix.Reset();
			PartyHud.Reset();
			InstanceSync.Clear();
			PartyInvites.Clear();
			MapTally.Reset();
			MapExplore.Reset();
		}
	}
}
