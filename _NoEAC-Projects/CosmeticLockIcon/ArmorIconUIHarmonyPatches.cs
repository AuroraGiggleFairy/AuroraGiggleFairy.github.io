public static class ArmorIconUIHarmonyPatches
{
	public static bool TryGetCosmeticArmorIcon(ItemClass itemClass, EntityPlayer player, string bindingName, out string icon)
	{
		icon = null;
		if (!(itemClass is ItemClassArmor itemClassArmor))
		{
			return false;
		}
		if (!itemClassArmor.IsCosmetic || player == null)
		{
			return false;
		}
		if (bindingName == "itemtypeicon" || bindingName == "altitemtypeicon")
		{
			(bool, EntitlementSetEnum)? tuple = player.equipment?.HasCosmeticUnlocked(itemClassArmor);
			bool flag = tuple.HasValue && tuple.Value.Item1;
			icon = (flag ? itemClassArmor.AltItemTypeIcon : itemClassArmor.ItemTypeIcon);
			return !string.IsNullOrEmpty(icon);
		}
		return false;
	}
}
