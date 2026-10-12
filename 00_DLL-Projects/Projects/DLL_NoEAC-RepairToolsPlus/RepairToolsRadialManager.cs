using GUI_2;
using UnityEngine;

namespace RepairToolsPlus
{
    public static class RepairToolsRadialManager
    {
        public const int CommandCapWood = 0;
        public const int CommandCapCobble = 1;
        public const int CommandCapConcrete = 2;
        public const int CommandCapSteel = 3;

        public static bool IsHoldingRepairTool(EntityPlayerLocal player)
        {
            if (player?.inventory?.holdingItem == null) return false;
            ItemClass item = player.inventory.holdingItem;
            if (item.Actions == null) return false;

            for (int i = 0; i < item.Actions.Length; i++)
            {
                if (item.Actions[i] is ItemActionRepair) return true;
            }
            return false;
        }

        public static void SetupRadial(XUiC_Radial radial, EntityPlayerLocal player)
        {
            if (radial == null || player == null) return;

            radial.ResetRadialEntries();

            // 0: Wood
            radial.CreateRadialEntry(
                CommandCapWood,
                "resourceWood",
                (RepairToolsSettings.CurrentCap == UpgradeCapTier.Wood) ? new Color(0.2f, 1f, 0.4f) : Color.white,
                "ItemIconAtlas",
                "",
                "Upgrade Cap: Wood",
                RepairToolsSettings.CurrentCap == UpgradeCapTier.Wood
            );

            // 1: Cobblestone
            radial.CreateRadialEntry(
                CommandCapCobble,
                "resourceCobblestones",
                (RepairToolsSettings.CurrentCap == UpgradeCapTier.Cobblestone) ? new Color(0.2f, 1f, 0.4f) : Color.white,
                "ItemIconAtlas",
                "",
                "Upgrade Cap: Cobblestone",
                RepairToolsSettings.CurrentCap == UpgradeCapTier.Cobblestone
            );

            // 2: Concrete
            radial.CreateRadialEntry(
                CommandCapConcrete,
                "resourceConcreteMix",
                (RepairToolsSettings.CurrentCap == UpgradeCapTier.Concrete) ? new Color(0.2f, 1f, 0.4f) : Color.white,
                "ItemIconAtlas",
                "",
                "Upgrade Cap: Concrete",
                RepairToolsSettings.CurrentCap == UpgradeCapTier.Concrete
            );

            // 3: Steel (No Limit)
            radial.CreateRadialEntry(
                CommandCapSteel,
                "resourceForgedSteel",
                (RepairToolsSettings.CurrentCap == UpgradeCapTier.Steel) ? new Color(0.2f, 1f, 0.4f) : Color.white,
                "ItemIconAtlas",
                "",
                "Upgrade Cap: Steel (Max)",
                RepairToolsSettings.CurrentCap == UpgradeCapTier.Steel
            );

            int preSelectedIndex = (int)RepairToolsSettings.CurrentCap - 1;

            radial.SetCommonData(
                UIUtils.GetButtonIconForAction(player.playerInput.Reload),
                HandleRadialCommand,
                new XUiC_Radial.RadialContextHoldingSlotIndex(player.inventory.holdingItemIdx),
                preSelectedIndex,
                _hasSpecialActionPriorToRadialVisibility: false,
                XUiC_Radial.RadialValidSameHoldingSlotIndex
            );
        }

        public static void HandleRadialCommand(XUiC_Radial sender, int commandIndex, XUiC_Radial.RadialContextAbs context)
        {
            EntityPlayerLocal player = sender?.xui?.playerUI?.entityPlayer;
            if (player == null) return;

            switch (commandIndex)
            {
                case CommandCapWood:
                    RepairToolsSettings.CurrentCap = UpgradeCapTier.Wood;
                    break;
                case CommandCapCobble:
                    RepairToolsSettings.CurrentCap = UpgradeCapTier.Cobblestone;
                    break;
                case CommandCapConcrete:
                    RepairToolsSettings.CurrentCap = UpgradeCapTier.Concrete;
                    break;
                case CommandCapSteel:
                    RepairToolsSettings.CurrentCap = UpgradeCapTier.Steel;
                    break;
                case -1:
                    // Quick tap reload on tools with ranged component (Nailgun)
                    if (player.inventory.holdingItem.Actions != null &&
                        player.inventory.holdingItem.Actions.Length > 0 &&
                        player.inventory.holdingItem.Actions[0] is ItemActionRanged rangedAction)
                    {
                        rangedAction.ExecuteAction(player.inventory.holdingItemData.actionData[0], false);
                    }
                    break;
            }
        }
    }
}
