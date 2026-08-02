using HarmonyLib;
using GameEvent.SequenceActions;

namespace AGFProjects.DestroyBiomeBadgeFix
{
	/// <summary>
	/// Vanilla ActionBaseItemAction treats BiomeBadge as equipment slots j &gt;= 4.
	/// That range also includes ClothingHead..ClothingFeet (8-11), so DestroyBiomeBadge
	/// (and any RemoveItems with BiomeBadge) clears worn clothing. Block only clothing slots.
	/// </summary>
	[HarmonyPatch(typeof(ActionRemoveItems), "HandleItemValueChange")]
	public class Patch_ActionRemoveItems_HandleItemValueChange
	{
		static bool Prefix(ref ItemValue itemValue, EntityPlayer player)
		{
			if (itemValue == null || itemValue.IsEmpty())
			{
				return true;
			}

			ItemValue[] items = player.equipment.GetItems();
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] == null || items[i].IsEmpty() || !items[i].Equals(itemValue))
				{
					continue;
				}

				if (i >= (int)EquipmentSlots.ClothingHead && i <= (int)EquipmentSlots.ClothingFeet)
				{
					return false;
				}

				break;
			}

			return true;
		}
	}
}
