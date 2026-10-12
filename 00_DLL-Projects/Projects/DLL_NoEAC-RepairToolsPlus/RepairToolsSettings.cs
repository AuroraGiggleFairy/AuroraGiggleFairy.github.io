using System;

namespace RepairToolsPlus
{
    public enum UpgradeCapTier
    {
        Wood = 1,
        Cobblestone = 2,
        Concrete = 3,
        Steel = 4
    }

    public enum DamageFilterMode
    {
        BlocksOnly = 0,
        All = 1,
        TerrainOnly = 2
    }

    public static class RepairToolsSettings
    {
        public static bool IsDamageVisionActive = true;
        public static UpgradeCapTier CurrentCap = UpgradeCapTier.Steel;
        public static DamageFilterMode CurrentFilter = DamageFilterMode.BlocksOnly;

        public static UpgradeCapTier GetUpgradeTargetTier(Block block)
        {
            if (block == null) return UpgradeCapTier.Steel;

            string toBlock = block.Properties.GetString(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockToBlock);
            if (string.IsNullOrEmpty(toBlock)) return UpgradeCapTier.Steel;

            if (toBlock.IndexOf("wood", StringComparison.OrdinalIgnoreCase) >= 0)
                return UpgradeCapTier.Wood;
            if (toBlock.IndexOf("cobble", StringComparison.OrdinalIgnoreCase) >= 0)
                return UpgradeCapTier.Cobblestone;
            if (toBlock.IndexOf("concrete", StringComparison.OrdinalIgnoreCase) >= 0)
                return UpgradeCapTier.Concrete;
            if (toBlock.IndexOf("steel", StringComparison.OrdinalIgnoreCase) >= 0)
                return UpgradeCapTier.Steel;

            return UpgradeCapTier.Steel;
        }

        public static bool CanUpgradeToCap(Block block)
        {
            if (block == null) return true;
            string toBlock = block.Properties.GetString(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockToBlock);
            if (string.IsNullOrEmpty(toBlock)) return true;

            return GetUpgradeTargetTier(block) <= CurrentCap;
        }
    }
}
