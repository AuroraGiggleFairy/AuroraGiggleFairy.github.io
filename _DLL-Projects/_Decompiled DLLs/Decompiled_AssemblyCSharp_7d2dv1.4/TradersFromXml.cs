using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class TradersFromXml
{
	public static IEnumerator LoadTraderInfo(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <traders> found!");
		}
		ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement e)
	{
		if (e.HasAttribute("buy_markup"))
		{
			TraderInfo.BuyMarkup = StringParsers.ParseFloat(e.GetAttribute("buy_markup"));
		}
		if (e.HasAttribute("sell_markdown"))
		{
			TraderInfo.SellMarkdown = StringParsers.ParseFloat(e.GetAttribute("sell_markdown"));
		}
		TraderManager.QuestTierMod = new float[5];
		TraderManager.TraderStageTemplates.Clear();
		if (e.HasAttribute("quest_tier_mod"))
		{
			string attribute = e.GetAttribute("quest_tier_mod");
			if (attribute.Contains(","))
			{
				string[] array = attribute.Split(',');
				TraderManager.QuestTierMod = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					TraderManager.QuestTierMod[i] = StringParsers.ParseFloat(array[i]);
				}
			}
			else
			{
				TraderManager.QuestTierMod = new float[1] { StringParsers.ParseFloat(attribute) };
			}
		}
		TraderInfo.QualityMaxMod = 1f;
		TraderInfo.QualityMinMod = 1f;
		if (e.HasAttribute("quality_mod"))
		{
			float _minCount = 1f;
			float _maxCount = 1f;
			StringParsers.ParseMinMaxCount(e.GetAttribute("quality_mod"), out _minCount, out _maxCount);
			TraderInfo.QualityMinMod = _minCount;
			TraderInfo.QualityMaxMod = _maxCount;
		}
		if (e.HasAttribute("currency_item"))
		{
			TraderInfo.CurrencyItem = e.GetAttribute("currency_item");
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "trader_info")
			{
				ParseTraderInfo(item);
				continue;
			}
			if (item.Name == "trader_item_groups")
			{
				ParseTraderItemGroups(item);
				continue;
			}
			if (item.Name == "traderstage_templates")
			{
				ParseTraderStageTemplates(item);
				continue;
			}
			throw new Exception("Unrecognized xml element " + item.Name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderInfo(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("trader must have an id attribute");
		}
		int result = 0;
		if (!int.TryParse(e.GetAttribute("id"), out result))
		{
			throw new Exception("Parsing error id '" + e.GetAttribute("id") + "'");
		}
		if (TraderInfo.traderInfoList[result] != null)
		{
			throw new Exception("Duplicate lootlist entry with id " + result);
		}
		TraderInfo traderInfo = new TraderInfo();
		traderInfo.Id = result;
		TraderInfo.traderInfoList[result] = traderInfo;
		if (e.HasAttribute("reset_interval"))
		{
			traderInfo.ResetInterval = int.Parse(e.GetAttribute("reset_interval"));
			traderInfo.ResetIntervalInTicks = traderInfo.ResetInterval * 24000;
		}
		if (e.HasAttribute("allow_buy"))
		{
			traderInfo.AllowSell = StringParsers.ParseBool(e.GetAttribute("allow_buy"));
		}
		if (e.HasAttribute("allow_sell"))
		{
			traderInfo.AllowSell = StringParsers.ParseBool(e.GetAttribute("allow_sell"));
		}
		if (e.HasAttribute("override_buy_markup"))
		{
			traderInfo.OverrideBuyMarkup = StringParsers.ParseFloat(e.GetAttribute("override_buy_markup"));
		}
		if (e.HasAttribute("override_sell_markup"))
		{
			traderInfo.OverrideSellMarkdown = StringParsers.ParseFloat(e.GetAttribute("override_sell_markup"));
		}
		if (e.HasAttribute("player_owned"))
		{
			traderInfo.PlayerOwned = StringParsers.ParseBool(e.GetAttribute("player_owned"));
		}
		if (e.HasAttribute("rentable"))
		{
			traderInfo.Rentable = StringParsers.ParseBool(e.GetAttribute("rentable"));
		}
		if (e.HasAttribute("rent_cost"))
		{
			traderInfo.RentCost = int.Parse(e.GetAttribute("rent_cost"));
		}
		if (e.HasAttribute("rent_time"))
		{
			traderInfo.RentTimeInDays = int.Parse(e.GetAttribute("rent_time"));
		}
		if (e.HasAttribute("open_time"))
		{
			string[] array = e.GetAttribute("open_time").Split(':');
			traderInfo.OpenTime = GameUtils.DayTimeToWorldTime(1, Convert.ToInt32(array[0]), (array.Length > 1) ? Convert.ToInt32(array[1]) : 0);
			traderInfo.UseOpenHours = true;
		}
		if (e.HasAttribute("close_time"))
		{
			string[] array2 = e.GetAttribute("close_time").Split(':');
			traderInfo.CloseTime = GameUtils.DayTimeToWorldTime(1, Convert.ToInt32(array2[0]), (array2.Length > 1) ? Convert.ToInt32(array2[1]) : 0);
			traderInfo.WarningTime = traderInfo.CloseTime - 300;
			traderInfo.UseOpenHours = true;
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "trader_items")
			{
				ParseTraderItems(traderInfo, item);
				continue;
			}
			if (item.Name == "tier_items")
			{
				ParseTierItems(traderInfo, item);
				continue;
			}
			throw new Exception("Unrecognized xml element " + item.Name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItems(TraderInfo info, XElement e)
	{
		info.minCount = 1;
		info.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				info.minCount = -1;
				info.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out info.minCount, out info.maxCount);
			}
		}
		parseItemList(info.Id.ToString(), e.Elements("item"), info.traderItems, -1, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTierItems(TraderInfo info, XElement e)
	{
		TraderInfo.TierItemGroup tierItemGroup = new TraderInfo.TierItemGroup();
		tierItemGroup.minCount = 1;
		tierItemGroup.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				tierItemGroup.minCount = -1;
				tierItemGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out tierItemGroup.minCount, out tierItemGroup.maxCount);
			}
		}
		if (e.HasAttribute("level"))
		{
			StringParsers.ParseMinMaxCount(e.GetAttribute("level"), out tierItemGroup.minLevel, out tierItemGroup.maxLevel);
			parseItemList(info.Id.ToString(), e.Elements("item"), tierItemGroup.traderItems, -1, -1);
			info.TierItemGroups.Add(tierItemGroup);
			return;
		}
		throw new Exception("level range missing on tier items group.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItemGroups(XElement e)
	{
		foreach (XElement item in e.Elements("trader_item_group"))
		{
			ParseTraderItemGroup(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItemGroup(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("trader item group must have a name attribute");
		}
		string attribute = e.GetAttribute("name");
		if (TraderInfo.traderItemGroups.ContainsKey(attribute))
		{
			throw new Exception("Duplicate trader_item_group entry with name " + attribute);
		}
		TraderInfo.TraderItemGroup traderItemGroup = new TraderInfo.TraderItemGroup();
		traderItemGroup.name = attribute;
		TraderInfo.traderItemGroups[attribute] = traderItemGroup;
		traderItemGroup.minCount = 1;
		traderItemGroup.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				traderItemGroup.minCount = -1;
				traderItemGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out traderItemGroup.minCount, out traderItemGroup.maxCount);
			}
		}
		if (e.HasAttribute("mods"))
		{
			traderItemGroup.modsToInstall = e.GetAttribute("mods").Split(',');
		}
		else
		{
			traderItemGroup.modsToInstall = new string[0];
		}
		if (e.HasAttribute("mod_chance"))
		{
			traderItemGroup.modChance = StringParsers.ParseFloat(e.GetAttribute("mod_chance"));
		}
		if (e.HasAttribute("unique_only"))
		{
			traderItemGroup.uniqueOnly = StringParsers.ParseBool(e.GetAttribute("unique_only"));
		}
		parseItemList(traderItemGroup.name, e.Elements("item"), traderItemGroup.items, -1, -1);
		for (int i = 0; i < traderItemGroup.items.Count; i++)
		{
			traderItemGroup.items[i].parentGroup = traderItemGroup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderStageTemplates(XElement e)
	{
		foreach (XElement item in e.Elements("traderstage_template"))
		{
			ParseTraderStageTemplate(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderStageTemplate(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("traderstage template must have a name attribute");
		}
		string attribute = e.GetAttribute("name");
		if (TraderManager.TraderStageTemplates.ContainsKey(attribute))
		{
			throw new Exception("Duplicate traderstage_template entry with name " + attribute);
		}
		TraderStageTemplateGroup traderStageTemplateGroup = new TraderStageTemplateGroup();
		traderStageTemplateGroup.Name = attribute;
		TraderManager.TraderStageTemplates.Add(attribute, traderStageTemplateGroup);
		foreach (XElement item in e.Elements("entry"))
		{
			TraderStageTemplate traderStageTemplate = new TraderStageTemplate();
			if (item.HasAttribute("min"))
			{
				traderStageTemplate.Min = StringParsers.ParseSInt32(item.GetAttribute("min"));
			}
			if (item.HasAttribute("max"))
			{
				traderStageTemplate.Max = StringParsers.ParseSInt32(item.GetAttribute("max"));
			}
			if (item.HasAttribute("quality"))
			{
				traderStageTemplate.Quality = StringParsers.ParseSInt32(item.GetAttribute("quality"));
			}
			traderStageTemplateGroup.Templates.Add(traderStageTemplate);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItemList(string _containerId, IEnumerable<XElement> _childNodes, List<TraderInfo.TraderItemEntry> _itemList, int minQualityBase, int maxQualityBase)
	{
		foreach (XElement _childNode in _childNodes)
		{
			TraderInfo.TraderItemEntry traderItemEntry = new TraderInfo.TraderItemEntry();
			traderItemEntry.prob = 1f;
			if (_childNode.HasAttribute("prob") && !StringParsers.TryParseFloat(_childNode.GetAttribute("prob"), out traderItemEntry.prob))
			{
				throw new Exception("Parsing error prob '" + _childNode.GetAttribute("prob") + "'");
			}
			if (_childNode.HasAttribute("group"))
			{
				string attribute = _childNode.GetAttribute("group");
				if (!TraderInfo.traderItemGroups.TryGetValue(attribute, out traderItemEntry.group))
				{
					throw new Exception("traderItemGroup '" + attribute + "' does not exist or has not been defined before being reference by trader_items/trader_item_group id='" + _containerId + "'");
				}
			}
			else
			{
				if (!_childNode.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' or 'group' missing on item in lootcontainer/lootgroup id='" + _containerId + "'");
				}
				traderItemEntry.item = new TraderInfo.TraderItem();
				string attribute2 = _childNode.GetAttribute("name");
				traderItemEntry.item.itemValue = ItemClass.GetItem(attribute2);
				if (traderItemEntry.item.itemValue.IsEmpty())
				{
					throw new Exception("Item with name '" + attribute2 + "' not found!");
				}
			}
			traderItemEntry.minCount = 1;
			traderItemEntry.maxCount = 1;
			if ((traderItemEntry.item == null || ItemClass.GetForId(traderItemEntry.item.itemValue.type).CanStack()) && _childNode.HasAttribute("count"))
			{
				StringParsers.ParseMinMaxCount(_childNode.GetAttribute("count"), out traderItemEntry.minCount, out traderItemEntry.maxCount);
			}
			traderItemEntry.minQuality = minQualityBase;
			traderItemEntry.maxQuality = maxQualityBase;
			if (_childNode.HasAttribute("quality"))
			{
				StringParsers.ParseMinMaxCount(_childNode.GetAttribute("quality"), out traderItemEntry.minQuality, out traderItemEntry.maxQuality);
			}
			if (_childNode.HasAttribute("unique_only"))
			{
				traderItemEntry.uniqueOnly = StringParsers.ParseBool(_childNode.GetAttribute("unique_only"));
			}
			if (_childNode.HasAttribute("mods"))
			{
				traderItemEntry.modsToInstall = _childNode.GetAttribute("mods").Split(',');
			}
			else
			{
				traderItemEntry.modsToInstall = new string[0];
			}
			if (_childNode.HasAttribute("mod_chance"))
			{
				traderItemEntry.modChance = StringParsers.ParseFloat(_childNode.GetAttribute("mod_chance"));
			}
			_itemList.Add(traderItemEntry);
		}
	}
}
