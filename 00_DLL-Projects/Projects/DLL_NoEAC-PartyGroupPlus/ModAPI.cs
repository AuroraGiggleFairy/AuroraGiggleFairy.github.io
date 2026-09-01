using System;
using System.Reflection;
using HarmonyLib;

namespace PartyGroupPlus
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				PartyGroupPlusSettings.Load(_modInstance?.Path);
				PartyGroupPlusColorStore.LoadPreference();
				PartyGroupPlusPriorityStore.Load();
				new Harmony("com.agfprojects.partygroupplus").PatchAll(Assembly.GetExecutingAssembly());
				ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData _) =>
				{
					PartyGroupPlusForceParty.ApplyLoaded();
				});
				ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
				{
					try
					{
						int entityId = data.EntityId;
						if (entityId < 0 && data.ClientInfo != null)
						{
							entityId = data.ClientInfo.entityId;
						}

						EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
						if (local != null && entityId >= 0 && local.entityId == entityId)
						{
							PartyGroupPlusColorStore.ApplyLoadedPreference(entityId);
							PartyGroupPlusColorStore.RequestSync();
						}

						PartyGroupPlusForceParty.EnrollSoon(entityId, data.RespawnType);
					}
					catch (Exception ex)
					{
						Console.WriteLine("PartyGroupPlus: PlayerSpawnedInWorld error: " + ex);
					}
				});
				Console.WriteLine("PartyGroupPlus: Harmony registered (uncapped party, 30% XP floor, closest-7 HUD, 192 party colors).");
			}
			catch (Exception ex)
			{
				Console.WriteLine("PartyGroupPlus: Patch registration error: " + ex);
			}
		}
	}
}
