using System;
using System.Collections;
using System.Xml.Linq;

public class UIDisplayInfoFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <ui_display_info> found!");
		}
		ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "item_display")
			{
				foreach (XElement item2 in item.Elements())
				{
					ParseItemDisplayInfo(item2);
				}
			}
			else if (item.Name == "character_stat_display")
			{
				foreach (XElement item3 in item.Elements())
				{
					DisplayInfoEntry displayInfoEntry = ParseDisplayInfoEntry(item3);
					if (displayInfoEntry != null)
					{
						UIDisplayInfoManager.Current.AddCharacterDisplayInfo(displayInfoEntry);
					}
				}
			}
			else if (item.Name == "crafting_category_display")
			{
				foreach (XElement item4 in item.Elements())
				{
					ParseCraftingCategoryList(item4);
				}
			}
			else if (item.Name == "trader_category_display")
			{
				ParseTraderCategoryList(item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseItemDisplayInfo(XElement e)
	{
		if (!e.HasAttribute("display_type"))
		{
			throw new Exception("item_display_info must have an display_type attribute");
		}
		string attribute = e.GetAttribute("display_type");
		if (UIDisplayInfoManager.Current.ContainsItemDisplayStats(attribute))
		{
			throw new Exception("Duplicate item_display_info entry with tag " + attribute);
		}
		string text = attribute;
		if (e.HasAttribute("display_group"))
		{
			text = e.GetAttribute("display_group");
		}
		UIDisplayInfoManager.Current.AddItemDisplayStats(attribute, text);
		foreach (XElement item in e.Elements("display_entry"))
		{
			DisplayInfoEntry displayInfoEntry = ParseDisplayInfoEntry(item);
			if (displayInfoEntry != null)
			{
				UIDisplayInfoManager.Current.AddItemDisplayInfo(attribute, displayInfoEntry);
			}
		}
		if (e.HasAttribute("extends"))
		{
			string attribute2 = e.GetAttribute("extends");
			if (!UIDisplayInfoManager.Current.ContainsItemDisplayStats(attribute2))
			{
				throw new Exception($"Extends item_display_info {attribute2} is not specified.'");
			}
			ItemDisplayEntry displayStatsForTag = UIDisplayInfoManager.Current.GetDisplayStatsForTag(attribute2);
			for (int i = 0; i < displayStatsForTag.DisplayStats.Count; i++)
			{
				UIDisplayInfoManager.Current.AddItemDisplayInfo(attribute, displayStatsForTag.DisplayStats[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DisplayInfoEntry ParseDisplayInfoEntry(XElement node)
	{
		DisplayInfoEntry displayInfoEntry = new DisplayInfoEntry();
		if (node.HasAttribute("name"))
		{
			string attribute = node.GetAttribute("name");
			try
			{
				displayInfoEntry.StatType = EnumUtils.Parse<PassiveEffects>(attribute, _ignoreCase: true);
			}
			catch
			{
				displayInfoEntry.CustomName = attribute;
			}
		}
		if (node.HasAttribute("display_type"))
		{
			displayInfoEntry.DisplayType = EnumUtils.Parse<DisplayInfoEntry.DisplayTypes>(node.GetAttribute("display_type"), _ignoreCase: true);
		}
		if (node.HasAttribute("show_inverted"))
		{
			displayInfoEntry.ShowInverted = Convert.ToBoolean(node.GetAttribute("show_inverted"));
		}
		if (node.HasAttribute("title_key"))
		{
			displayInfoEntry.TitleOverride = Localization.Get(node.GetAttribute("title_key"));
		}
		if (node.HasAttribute("negative_preferred"))
		{
			displayInfoEntry.NegativePreferred = Convert.ToBoolean(node.GetAttribute("negative_preferred"));
		}
		if (node.HasAttribute("display_leading_plus"))
		{
			displayInfoEntry.DisplayLeadingPlus = Convert.ToBoolean(node.GetAttribute("display_leading_plus"));
		}
		if (node.HasAttribute("tags"))
		{
			displayInfoEntry.Tags = FastTags<TagGroup.Global>.Parse(node.GetAttribute("tags"));
		}
		return displayInfoEntry;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCraftingCategoryList(XElement e)
	{
		if (!e.HasAttribute("display_type"))
		{
			throw new Exception("crafting_category_list must have an display_type attribute");
		}
		string attribute = e.GetAttribute("display_type");
		if (UIDisplayInfoManager.Current.ContainsCraftingCategoryList(attribute))
		{
			throw new Exception("Duplicate crafting_category_list entry with tag " + attribute);
		}
		foreach (XElement item in e.Elements("crafting_category"))
		{
			CraftingCategoryDisplayEntry craftingCategoryDisplayEntry = ParseCraftingCategory(item);
			if (craftingCategoryDisplayEntry != null)
			{
				UIDisplayInfoManager.Current.AddCraftingCategoryDisplayItem(attribute, craftingCategoryDisplayEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderCategoryList(XElement e)
	{
		foreach (XElement item in e.Elements("trader_category"))
		{
			CraftingCategoryDisplayEntry craftingCategoryDisplayEntry = ParseCraftingCategory(item);
			if (craftingCategoryDisplayEntry != null)
			{
				UIDisplayInfoManager.Current.AddTraderCategoryDIsplayItem(craftingCategoryDisplayEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static CraftingCategoryDisplayEntry ParseCraftingCategory(XElement node)
	{
		CraftingCategoryDisplayEntry craftingCategoryDisplayEntry = new CraftingCategoryDisplayEntry();
		if (node.HasAttribute("name"))
		{
			craftingCategoryDisplayEntry.Name = node.GetAttribute("name");
		}
		if (node.HasAttribute("icon"))
		{
			craftingCategoryDisplayEntry.Icon = node.GetAttribute("icon");
		}
		if (node.HasAttribute("display_name"))
		{
			craftingCategoryDisplayEntry.DisplayName = Localization.Get(node.GetAttribute("display_name"));
		}
		return craftingCategoryDisplayEntry;
	}
}
