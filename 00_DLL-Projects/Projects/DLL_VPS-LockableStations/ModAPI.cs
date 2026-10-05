using System;
using HarmonyLib;
using Platform;

namespace LockableWorkstations
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				Harmony harmony = new Harmony("com.agfprojects.lockableworkstations");
				foreach (Type type in typeof(ModAPI).Assembly.GetTypes())
				{
					if (type == null || !type.IsClass)
						continue;
					try
					{
						harmony.CreateClassProcessor(type).Patch();
					}
					catch (Exception ex)
					{
						Console.WriteLine("LockableStations: skip patch " + type.Name + ": " + ex.Message);
					}
				}

				LockableWorkstationHelpers.LoadSettings(modInstance?.Path);
				ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData _) =>
				{
					LockableWorkstationHelpers.InitializeServerPersistence();
				});
				ModEvents.WorldShuttingDown.RegisterHandler((ref ModEvents.SWorldShuttingDownData _) =>
				{
					LockableWorkstationHelpers.FlushServerPersistence();
				});
				ModEvents.GameShutdown.RegisterHandler((ref ModEvents.SGameShutdownData _) =>
				{
					LockableWorkstationHelpers.FlushServerPersistence();
				});
				ModEvents.GameUpdate.RegisterHandler((ref ModEvents.SGameUpdateData _) =>
				{
					LockableWorkstationHelpers.TickServerPersistence();
				});
				ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
				{
					ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
					if (manager == null)
						return;

					if (data.IsLocalPlayer && !manager.IsServer)
					{
						string userCombined = PlatformManager.InternalLocalUserIdentifier?.CombinedString ?? string.Empty;
						manager.SendToServer(NetPackageManager.GetPackage<NetPackageLockableWorkstationsClientHello>().Setup(data.EntityId, userCombined));
					}
				});
				ModEvents.PlayerDisconnected.RegisterHandler((ref ModEvents.SPlayerDisconnectedData data) =>
				{
					int entityId = data.ClientInfo?.entityId ?? -1;
					if (entityId >= 0)
						LockableWorkstationHybridRouting.ForgetClientByEntityId(entityId);
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine("LockableStations: Patch registration error: " + ex);
			}
		}
	}
}
