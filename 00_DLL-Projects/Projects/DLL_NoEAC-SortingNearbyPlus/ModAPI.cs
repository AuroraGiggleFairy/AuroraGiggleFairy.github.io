using System;
using HarmonyLib;

namespace SortingNearbyPlus
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod _modInstance)
		{
			try
			{
				new Harmony("com.agfprojects.sortingnearbyplus").PatchAll();
				SettingsManager.Load();
				NearbyScanner.RefreshLandClaimRadius();
				ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData _) =>
				{
					SettingsManager.Load();
					NearbyScanner.RefreshLandClaimRadius();
				});
				ModEvents.ChatMessage.RegisterHandler((ref ModEvents.SChatMessageData data) => ChatCmdSortingNearbyPlus.OnChatMessage(ref data));
				Log.Info("Loaded. Based on Kanaverum / Asylum Robotic Inbox. Maintained by AGF.");
			}
			catch (Exception ex)
			{
				Log.Error("Init failed", ex);
			}
		}
	}
}
