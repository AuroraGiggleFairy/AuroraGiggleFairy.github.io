using System.Collections.Generic;

public class UIDisplayInfoManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static UIDisplayInfoManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ItemDisplayEntry> ItemDisplayStats = new Dictionary<string, ItemDisplayEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> CharacterDisplayStats = new List<DisplayInfoEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<CraftingCategoryDisplayEntry>> CraftingCategoryDisplayLists = new Dictionary<string, List<CraftingCategoryDisplayEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, CraftingCategoryDisplayEntry> TraderCategoryDisplayDict = new Dictionary<string, CraftingCategoryDisplayEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PassiveEffects, string> StatLocalizationDictionary = new EnumDictionary<PassiveEffects, string>();

	public static UIDisplayInfoManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new UIDisplayInfoManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIDisplayInfoManager()
	{
	}

	public static void Reset()
	{
		if (HasInstance)
		{
			Current.Cleanup();
		}
	}

	public bool ContainsItemDisplayStats(string tag)
	{
		return ItemDisplayStats.ContainsKey(tag);
	}

	public bool ContainsCraftingCategoryList(string tag)
	{
		return CraftingCategoryDisplayLists.ContainsKey(tag);
	}

	public ItemDisplayEntry GetDisplayStatsForTag(string tag)
	{
		if (ItemDisplayStats.ContainsKey(tag))
		{
			return ItemDisplayStats[tag];
		}
		return null;
	}

	public void AddItemDisplayStats(string tag, string group)
	{
		if (!ItemDisplayStats.ContainsKey(tag))
		{
			ItemDisplayStats.Add(tag, new ItemDisplayEntry
			{
				DisplayGroup = group
			});
		}
	}

	public void AddItemDisplayInfo(string tag, DisplayInfoEntry displayInfo)
	{
		ItemDisplayStats[tag].DisplayStats.Add(displayInfo);
		if (!StatLocalizationDictionary.ContainsKey(displayInfo.StatType))
		{
			StatLocalizationDictionary.Add(displayInfo.StatType, Localization.Get(displayInfo.StatType.ToStringCached()));
		}
	}

	public void Cleanup()
	{
		ItemDisplayStats.Clear();
		CharacterDisplayStats.Clear();
		CraftingCategoryDisplayLists.Clear();
	}

	public string GetLocalizedName(PassiveEffects statType)
	{
		if (StatLocalizationDictionary.ContainsKey(statType))
		{
			return StatLocalizationDictionary[statType];
		}
		return "";
	}

	public void AddCharacterDisplayInfo(DisplayInfoEntry displayInfo)
	{
		CharacterDisplayStats.Add(displayInfo);
		if (!StatLocalizationDictionary.ContainsKey(displayInfo.StatType))
		{
			StatLocalizationDictionary.Add(displayInfo.StatType, Localization.Get(displayInfo.StatType.ToStringCached()));
		}
	}

	public List<DisplayInfoEntry> GetCharacterDisplayInfo()
	{
		return CharacterDisplayStats;
	}

	public void AddCraftingCategoryDisplayItem(string categoryListName, CraftingCategoryDisplayEntry entry)
	{
		if (!CraftingCategoryDisplayLists.ContainsKey(categoryListName))
		{
			CraftingCategoryDisplayLists.Add(categoryListName, new List<CraftingCategoryDisplayEntry>());
		}
		CraftingCategoryDisplayLists[categoryListName].Add(entry);
	}

	public List<CraftingCategoryDisplayEntry> GetCraftingCategoryDisplayList(string categoryListName)
	{
		if (CraftingCategoryDisplayLists.ContainsKey(categoryListName))
		{
			return CraftingCategoryDisplayLists[categoryListName];
		}
		return null;
	}

	public void AddTraderCategoryDIsplayItem(CraftingCategoryDisplayEntry entry)
	{
		if (!TraderCategoryDisplayDict.ContainsKey(entry.Name))
		{
			TraderCategoryDisplayDict.Add(entry.Name, entry);
		}
	}

	public CraftingCategoryDisplayEntry GetTraderCategoryDisplay(string name)
	{
		if (TraderCategoryDisplayDict.ContainsKey(name))
		{
			return TraderCategoryDisplayDict[name];
		}
		return null;
	}
}
