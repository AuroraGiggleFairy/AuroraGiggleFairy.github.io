using System;
using System.Collections.Generic;

public static class HideDlcCatalog
{
	public static readonly HideDlcPack[] Packs = new HideDlcPack[]
	{
		new HideDlcPack("desert", "DesertSet"),
		new HideDlcPack("hoarder", "HoarderSet"),
		new HideDlcPack("marauder", "MarauderSet"),
		new HideDlcPack("butcher", "ButcherSet"),
		new HideDlcPack("pirate", "PirateSet"),
		new HideDlcPack("hellreaver", "HellreaverSet"),
		new HideDlcPack("classicSurvivor", "ClassicSurvivorSet"),
		new HideDlcPack("holidayHats", "HolidayHatsSet"),
		new HideDlcPack("beachwear", "BeachwearSet"),
		new HideDlcPack("bigBeak", "BigBeakSet")
	};

	private static readonly Dictionary<ItemClass, string> PackKeyByItem = new Dictionary<ItemClass, string>();
	private static readonly Dictionary<string, string> ArmorGroupToPack = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "groupDesert", "desert" },
		{ "groupHoarder", "hoarder" },
		{ "groupMarauder", "marauder" },
		{ "groupButcher", "butcher" },
		{ "groupPirate", "pirate" },
		{ "groupHellreaver", "hellreaver" },
		{ "groupClassicSurvivor", "classicSurvivor" },
		{ "groupElfHat", "holidayHats" },
		{ "groupReindeerHat", "holidayHats" },
		{ "groupSnowmanHat", "holidayHats" },
		{ "groupTreeHat", "holidayHats" },
		{ "groupBeach", "beachwear" },
		{ "groupBigbeak", "bigBeak" }
	};

	public static bool TryGetPack(string key, out HideDlcPack pack)
	{
		pack = null;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}

		for (int i = 0; i < Packs.Length; i++)
		{
			if (string.Equals(Packs[i].Key, key, StringComparison.OrdinalIgnoreCase))
			{
				pack = Packs[i];
				return true;
			}
		}

		return false;
	}

	public static bool IsHiddenItem(ItemClass item)
	{
		string packKey;
		return TryGetPackKey(item, out packKey) && HideDlcSettings.IsHidden(packKey);
	}

	public static bool ShouldHideFromWardrobeList(Equipment equipment, ItemClass item)
	{
		if (IsHiddenItem(item))
		{
			return true;
		}

		string filter = HideDlcSettings.CosmeticListFilter;
		if (string.Equals(filter, HideDlcSettings.CosmeticListAll, StringComparison.OrdinalIgnoreCase))
		{
			return IsDlcStoreItem(equipment, item);
		}

		if (string.Equals(filter, HideDlcSettings.CosmeticListUnpurchased, StringComparison.OrdinalIgnoreCase))
		{
			return IsUnpurchasedStoreItem(equipment, item);
		}

		return false;
	}

	public static bool IsDlcStoreItem(Equipment equipment, ItemClass item)
	{
		if (equipment == null || item == null || item == ItemClass.MissingItem)
		{
			return false;
		}

		try
		{
			return equipment.HasCosmeticUnlocked(item).set != EntitlementSetEnum.None;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsUnpurchasedStoreItem(Equipment equipment, ItemClass item)
	{
		if (equipment == null || item == null || item == ItemClass.MissingItem)
		{
			return false;
		}

		try
		{
			(bool isUnlocked, EntitlementSetEnum set) unlocked = equipment.HasCosmeticUnlocked(item);
			return !unlocked.isUnlocked && unlocked.set != EntitlementSetEnum.None;
		}
		catch
		{
			return false;
		}
	}

	public static bool TryGetPackKey(ItemClass item, out string packKey)
	{
		packKey = null;
		if (item == null || item == ItemClass.MissingItem)
		{
			return false;
		}

		if (PackKeyByItem.TryGetValue(item, out string cached))
		{
			packKey = cached;
			return !string.IsNullOrEmpty(cached);
		}

		ItemClassArmor armor = item as ItemClassArmor;
		if (armor != null && armor.ArmorGroup != null && armor.ArmorGroup.Length > 0)
		{
			string group = armor.ArmorGroup[0];
			if (!string.IsNullOrEmpty(group) && ArmorGroupToPack.TryGetValue(group, out packKey))
			{
				PackKeyByItem[item] = packKey;
				return true;
			}
		}

		if (item.SDCSData != null && !string.IsNullOrEmpty(item.SDCSData.PrefabName))
		{
			try
			{
				EntitlementManager manager = EntitlementManager.Instance;
				if (manager == null)
				{
					return false;
				}

				packKey = PackKeyFromEntitlement(manager.GetSetForAsset(item.SDCSData.PrefabName));
				if (!string.IsNullOrEmpty(packKey))
				{
					PackKeyByItem[item] = packKey;
					return true;
				}
			}
			catch
			{
			}
		}

		PackKeyByItem[item] = null;
		return false;
	}

	private static string PackKeyFromEntitlement(EntitlementSetEnum set)
	{
		switch (set)
		{
			case EntitlementSetEnum.DesertCosmetic:
				return "desert";
			case EntitlementSetEnum.HoarderCosmetic:
				return "hoarder";
			case EntitlementSetEnum.MarauderCosmetic:
				return "marauder";
			case EntitlementSetEnum.ButcherCosmetic:
				return "butcher";
			case EntitlementSetEnum.PirateCosmetic:
				return "pirate";
			case EntitlementSetEnum.HellreaverCosmetic:
				return "hellreaver";
			case EntitlementSetEnum.ClassicSurvivorCosmetic:
				return "classicSurvivor";
			case EntitlementSetEnum.ChristmasCosmetics:
				return "holidayHats";
			case EntitlementSetEnum.BeachCosmetic:
				return "beachwear";
			case EntitlementSetEnum.HenpocalypseCosmetic:
				return "bigBeak";
			default:
				return null;
		}
	}
}
