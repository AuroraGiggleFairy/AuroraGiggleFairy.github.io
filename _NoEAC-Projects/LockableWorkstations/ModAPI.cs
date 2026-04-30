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
				new Harmony("com.agfprojects.lockableworkstations").PatchAll();
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
					{
						return;
					}

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
					{
						LockableWorkstationHybridRouting.ForgetClientByEntityId(entityId);
					}
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine("LockableWorkstations: Patch registration error: " + ex);
			}
		}
	}
}
