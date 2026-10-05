using System;
using UnityEngine.Scripting;

namespace StorageLaptop
{
	[Preserve]
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			GameVersion.Initialize();
			PackageEmit.Prepare(typeof(NetPackageStorageLaptop));
			ModEvents.PlayerDisconnected.RegisterHandler((ref ModEvents.SPlayerDisconnectedData data) =>
			{
				int entityId = data.ClientInfo != null ? data.ClientInfo.entityId : -1;
				StorageService.ReleasePlayer(entityId);
			});
			Console.WriteLine("[StorageLaptop] Loaded.");
		}
	}
}
