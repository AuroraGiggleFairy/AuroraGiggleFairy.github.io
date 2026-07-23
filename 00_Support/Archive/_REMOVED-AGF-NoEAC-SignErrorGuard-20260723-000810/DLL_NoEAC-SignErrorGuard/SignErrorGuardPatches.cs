using HarmonyLib;
using UnityEngine;

namespace SignErrorGuard
{
    [HarmonyPatch(typeof(SignDataManager), nameof(SignDataManager.TryGetSignData))]
    internal static class Patch_SignDataManager_TryGetSignData
    {
        private static bool Prefix(GlobalSignId signId, ref SignData signData, ref bool __result)
        {
            if (!string.IsNullOrEmpty(signId.libraryId))
            {
                return true;
            }

            signData = null;
            __result = false;
            InvalidSignGuardState.LogOnce("TryGetSignData", signId);
            return false;
        }
    }

    [HarmonyPatch(typeof(SignDataManager), nameof(SignDataManager.TryGetRenderingData))]
    internal static class Patch_SignDataManager_TryGetRenderingData
    {
        private static bool Prefix(GlobalSignId signId, ref SignDataManager.SignRenderingData signRenderingData, ref bool __result)
        {
            if (!string.IsNullOrEmpty(signId.libraryId))
            {
                return true;
            }

            signRenderingData = null;
            __result = false;
            InvalidSignGuardState.LogOnce("TryGetRenderingData", signId);
            return false;
        }
    }

    internal static class InvalidSignGuardState
    {
        private static bool _logged;

        public static void LogOnce(string source, GlobalSignId signId)
        {
            if (_logged)
            {
                return;
            }

            _logged = true;
            Debug.LogWarning($"[SignErrorGuard] Blocked invalid sign id in {source}. libraryId='<null-or-empty>' guid='{signId.signGuid}'.");
        }
    }
}
