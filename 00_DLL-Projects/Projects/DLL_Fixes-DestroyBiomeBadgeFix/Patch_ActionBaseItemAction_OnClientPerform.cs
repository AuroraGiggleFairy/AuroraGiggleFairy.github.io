using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using GameEvent.SequenceActions;

namespace AGFProjects.DestroyBiomeBadgeFix
{
	/// <summary>
	/// Vanilla ActionBaseItemAction treats BiomeBadge as equipment slots j &gt;= 4.
	/// That range also includes ClothingHead..ClothingFeet (8-11), so DestroyBiomeBadge
	/// (and any RemoveItems with BiomeBadge) clears worn clothing. Block only clothing slots.
	/// </summary>
	[HarmonyLib.HarmonyPatch(typeof(ActionRemoveItems), "HandleItemValueChange")]
	public class Patch_ActionRemoveItems_HandleItemValueChange
	{
		private static readonly MethodInfo GetItems = typeof(Equipment).GetMethod("GetItems", Type.EmptyTypes);

		static bool Prefix(ref ItemValue itemValue, EntityPlayer player)
		{
			if (itemValue == null || itemValue.IsEmpty() || player?.equipment == null)
			{
				return true;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33
				? AllowV33(itemValue, player.equipment)
				: AllowPre33(itemValue, player.equipment);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool AllowV33(ItemValue itemValue, Equipment equipment)
		{
			int slotCount = equipment.GetSlotCount();
			for (int i = 0; i < slotCount; i++)
			{
				if (Decide(i, equipment.GetSlotItem(i), itemValue, out bool allow))
				{
					return allow;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool AllowPre33(ItemValue itemValue, Equipment equipment)
		{
			if (GetItems == null)
			{
				return AllowV33(itemValue, equipment);
			}

			ItemValue[] items = GetItems.Invoke(equipment, null) as ItemValue[];
			if (items == null)
			{
				return true;
			}

			for (int i = 0; i < items.Length; i++)
			{
				if (Decide(i, items[i], itemValue, out bool allow))
				{
					return allow;
				}
			}

			return true;
		}

		private static bool Decide(int slot, ItemValue slotItem, ItemValue itemValue, out bool allow)
		{
			allow = true;
			if (slotItem == null || slotItem.IsEmpty() || !slotItem.Equals(itemValue))
			{
				return false;
			}

			allow = slot < (int)EquipmentSlots.ClothingHead || slot > (int)EquipmentSlots.ClothingFeet;
			return true;
		}
	}
}
