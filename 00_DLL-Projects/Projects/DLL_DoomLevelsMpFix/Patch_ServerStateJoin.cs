using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(ServerStateAuthorizer), nameof(ServerStateAuthorizer.Authorize))]
	internal static class Patch_ServerStateJoin
	{
		private static bool logged;

		private static bool Prefix(ref (EAuthorizerSyncResult, GameUtils.KickPlayerData?) __result)
		{
			GameStateManager state = GameManager.Instance?.gameStateManager;
			if (state == null || !state.IsGameStarted())
			{
				return true;
			}

			if (GameStats.GetInt(EnumGameStats.GameState) == 2)
			{
				return true;
			}

			if (!logged)
			{
				logged = true;
				Debug.Log("[DoomMultiplayer] ServerState: world started, allowing join without EOS listing");
			}

			__result = (EAuthorizerSyncResult.SyncAllow, null);
			return false;
		}
	}
}
