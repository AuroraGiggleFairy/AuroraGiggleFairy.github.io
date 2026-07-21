namespace StatControllers
{
    internal static class DoomGuyIconBindingUtil
    {
        private static int snapshotFrame = -1;
        private static int snapshotPlayerId = -1;
        private static string icon = "";

        public static string GetIcon(EntityPlayer player)
        {
            int frame = UnityEngine.Time.frameCount;
            int playerId = player?.entityId ?? -1;
            if (snapshotFrame == frame && snapshotPlayerId == playerId)
            {
                return icon;
            }

            snapshotFrame = frame;
            snapshotPlayerId = playerId;
            icon = ComputeIcon(player);
            return icon;
        }

        private static string ComputeIcon(EntityPlayer player)
        {
            if (player == null || player.Stats == null || player.Stats.Health == null)
            {
                return "";
            }

            if (player.Buffs != null && player.Buffs.HasBuff("buffInvulnerability"))
            {
                return "doomguy_god";
            }

            float percent = player.Stats.Health.ValuePercentUI * 100f;
            if (percent <= 0f)
            {
                return "doomguy_0";
            }
            if (percent < 10f)
            {
                return "doomguy_1";
            }
            if (percent < 20f)
            {
                return "doomguy_2";
            }
            if (percent < 30f)
            {
                return "doomguy_3";
            }
            if (percent < 40f)
            {
                return "doomguy_4";
            }

            return "doomguy_5";
        }
    }

    public class DoomGuyIcon : Binding
    {
        public DoomGuyIcon(int value, string name) : base(value, name)
        {
        }

        public override string GetCurrentValue(EntityPlayer player)
        {
            return DoomGuyIconBindingUtil.GetIcon(player);
        }
    }
}
