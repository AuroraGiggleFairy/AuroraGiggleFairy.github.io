using HarmonyLib;

namespace ExpandedInteractionPrompts
{
    // Vanilla UpdateTick resets worldTimeTouched while a player is within ~8 blocks of an empty
    // world container (opening it counts). First open already stamps the time in LootContainerOpened
    // and does not stamp again. Keep that stamp so the timer runs out even if you reopen or walk up.
    // Empty + due still flips bTouched so loot generates on the next open. Non-empty still waits.
    [HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.UpdateTick))]
    public static class Patch_TEFeatureStorage_UpdateTick
    {
        public static bool Prefix(TEFeatureStorage __instance, World _world)
        {
            if (__instance == null || _world == null)
                return false;

            if (__instance.Parent != null && __instance.Parent.PlayerPlaced)
                return false;
            if (!__instance.bTouched || __instance.bPlayerStorage || !__instance.IsEmpty())
                return false;

            int respawnDays = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
            if (respawnDays <= 0)
                return false;

            int touchedHours = GameUtils.WorldTimeToTotalHours(__instance.worldTimeTouched);
            if ((GameUtils.WorldTimeToTotalHours(_world.worldTime) - touchedHours) / 24 < respawnDays)
                return false;

            __instance.bWasTouched = false;
            __instance.bTouched = false;
            __instance.SetModified();
            return false;
        }
    }
}
