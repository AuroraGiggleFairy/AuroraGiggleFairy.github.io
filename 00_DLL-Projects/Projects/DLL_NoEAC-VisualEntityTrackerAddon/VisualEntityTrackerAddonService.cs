namespace VisualEntityTrackerAddon
{
    public static class VisualEntityTrackerAddonService
    {
        private const string TrackerBuffName = "buffZAlert";
        private const string TrackerCVarName = "agfVet";

        public static void ApplySavedMode(int entityId)
        {
            VisualEntityTrackerMode mode = VisualEntityTrackerModeSettings.GetModeForEntityId(
                entityId,
                VisualEntityTrackerModeSettings.GetServerDefaultMode());
            ApplyServerSideMode(entityId, mode == VisualEntityTrackerMode.On);
        }

        public static void ApplyServerSideMode(int entityId, bool enabled)
        {
            if (entityId < 0)
            {
                return;
            }

            World world = GameManager.Instance?.World;
            if (world == null)
            {
                return;
            }

            EntityPlayer player = world.GetEntity(entityId) as EntityPlayer;
            if (player?.Buffs == null)
            {
                return;
            }

            if (!VisualEntityTrackerPresence.IsPresent())
            {
                return;
            }

            try
            {
                bool hasBuff = player.Buffs.HasBuff(TrackerBuffName);
                if (enabled && !hasBuff)
                {
                    player.Buffs.AddBuff(TrackerBuffName, -1, true, false, -1f);
                }
                else if (!enabled)
                {
                    if (hasBuff)
                    {
                        player.Buffs.RemoveBuff(TrackerBuffName, -1, true);
                    }

                    if (player.Buffs.HasCustomVar(TrackerCVarName))
                    {
                        player.Buffs.RemoveCustomVar(TrackerCVarName);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
