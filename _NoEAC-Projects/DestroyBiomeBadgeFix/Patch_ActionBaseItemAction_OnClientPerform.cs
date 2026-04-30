using HarmonyLib;
using GameEvent.SequenceActions;

namespace AGFProjects.DestroyBiomeBadgeFix
{
    [HarmonyPatch(typeof(ActionRemoveItems), "HandleItemValueChange")]
    public class Patch_ActionRemoveItems_HandleItemValueChange
    {
        static bool Prefix(ref ItemValue itemValue, EntityPlayer player, ActionRemoveItems __instance)
        {
            var items = player.equipment.GetItems();
            int slotIndex = -1;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].Equals(itemValue))
                {
                    slotIndex = i;
                    break;
                }
            }
            var slotEnum = (EquipmentSlots)slotIndex;
            if (slotEnum == EquipmentSlots.BiomeBadge ||
                slotEnum == EquipmentSlots.BiomeBadge2 ||
                slotEnum == EquipmentSlots.BiomeBadge3 ||
                slotEnum == EquipmentSlots.BiomeBadge4)
            {
                // Allow clearing for badge slots
                return true;
            }
            // Prevent clearing for non-badge slots
            return false;
        }
    }
}
