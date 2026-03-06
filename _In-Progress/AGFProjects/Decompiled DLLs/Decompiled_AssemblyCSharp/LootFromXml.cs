using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class LootFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseItemList(string _containerId, IEnumerable<XElement> _childNodes, List<LootContainer.LootEntry> _itemList, int _minQualityBase, int _maxQualityBase)
	{
		foreach (XElement _childNode in _childNodes)
		{
			LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
			lootEntry.prob = 1f;
			if (_childNode.HasAttribute("prob") && !StringParsers.TryParseFloat(_childNode.GetAttribute("prob"), out lootEntry.prob))
			{
				throw new Exception("Parsing error prob '" + _childNode.GetAttribute("prob") + "'");
			}
			if (_childNode.HasAttribute("force_prob"))
			{
				StringParsers.TryParseBool(_childNode.GetAttribute("force_prob"), out lootEntry.forceProb);
			}
			if (_childNode.HasAttribute("group"))
			{
				string attribute = _childNode.GetAttribute("group");
				if (!LootContainer.lootGroups.TryGetValue(attribute, out lootEntry.group))
				{
					throw new Exception("lootgroup '" + attribute + "' does not exist or has not been defined before being reference by lootcontainer/lootgroup name='" + _containerId + "'");
				}
			}
			else
			{
				if (!_childNode.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' or 'group' missing on item in lootcontainer/lootgroup name='" + _containerId + "'");
				}
				lootEntry.item = new LootContainer.LootItem();
				string attribute2 = _childNode.GetAttribute("name");
				lootEntry.item.itemValue = ItemClass.GetItem(attribute2);
				if (lootEntry.item.itemValue.IsEmpty())
				{
					throw new Exception("Item with name '" + attribute2 + "' not found!");
				}
			}
			string attribute3 = _childNode.GetAttribute("tags");
			if (attribute3.Length > 0)
			{
				lootEntry.tags = FastTags<TagGroup.Global>.Parse(attribute3);
			}
			lootEntry.minCount = 1;
			lootEntry.maxCount = 1;
			if ((lootEntry.item == null || ItemClass.GetForId(lootEntry.item.itemValue.type).CanStack()) && _childNode.HasAttribute("count"))
			{
				StringParsers.ParseMinMaxCount(_childNode.GetAttribute("count"), out lootEntry.minCount, out lootEntry.maxCount);
			}
			lootEntry.minQuality = _minQualityBase;
			lootEntry.maxQuality = _maxQualityBase;
			if (_childNode.HasAttribute("quality"))
			{
				StringParsers.ParseMinMaxCount(_childNode.GetAttribute("quality"), out lootEntry.minQuality, out lootEntry.maxQuality);
			}
			if (_childNode.HasAttribute("loot_prob_template"))
			{
				lootEntry.lootProbTemplate = _childNode.GetAttribute("loot_prob_template");
			}
			else
			{
				lootEntry.lootProbTemplate = string.Empty;
			}
			if (_childNode.HasAttribute("mods"))
			{
				lootEntry.modsToInstall = _childNode.GetAttribute("mods").Split(',');
			}
			else
			{
				lootEntry.modsToInstall = new string[0];
			}
			if (_childNode.HasAttribute("mod_chance"))
			{
				lootEntry.modChance = StringParsers.ParseFloat(_childNode.GetAttribute("mod_chance"));
			}
			if (_childNode.HasAttribute("loot_stage_count_mod"))
			{
				lootEntry.lootstageCountMod = StringParsers.ParseFloat(_childNode.GetAttribute("loot_stage_count_mod"));
			}
			if (_childNode.HasElements)
			{
				foreach (XElement item in _childNode.Elements())
				{
					if (!(item.Name == "requirement"))
					{
						continue;
					}
					BaseLootEntryRequirement baseLootEntryRequirement = ParseLootEntryRequirement(item);
					if (baseLootEntryRequirement != null)
					{
						if (lootEntry.Requirements == null)
						{
							lootEntry.Requirements = new List<BaseLootEntryRequirement>();
						}
						lootEntry.Requirements.Add(baseLootEntryRequirement);
					}
				}
			}
			if (_childNode.HasAttribute("buffs"))
			{
				lootEntry.buffsToAdd = _childNode.GetAttribute("buffs").Split(',');
			}
			if (_childNode.HasAttribute("random_durability"))
			{
				lootEntry.randomDurability = StringParsers.ParseBool(_childNode.GetAttribute("random_durability"));
			}
			_itemList.Add(lootEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseLootEntryRequirement ParseLootEntryRequirement(XElement e)
	{
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
			BaseLootEntryRequirement baseLootEntryRequirement = null;
			try
			{
				baseLootEntryRequirement = (BaseLootEntryRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("LootEntryRequirement", text));
			}
			catch (Exception)
			{
				throw new Exception("No loot entry requirement class '" + text + " found!");
			}
			baseLootEntryRequirement.Init(e);
			return baseLootEntryRequirement;
		}
		throw new Exception("Loot Entry Requirement must have a class!");
	}

	public static IEnumerator LoadLootContainers(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No document root or no children found!");
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "lootcontainer")
			{
				LoadLootContainer(item);
			}
			if (item.Name == "lootgroup")
			{
				LoadLootGroup(item);
			}
			if (item.Name == "lootprobtemplates")
			{
				LoadLootProbabilityTemplate(item);
			}
			if (item.Name == "lootqualitytemplates")
			{
				LoadLootQualityTemplate(item);
			}
			if (item.Name == "loot_settings")
			{
				LoadLootSetting(item);
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootSetting(XElement _rootNode)
	{
		if (_rootNode.HasAttribute("poi_tier_count"))
		{
			int num = StringParsers.ParseSInt32(_rootNode.GetAttribute("poi_tier_count"));
			LootManager.POITierMod = new float[num];
			LootManager.POITierBonus = new float[num];
		}
		else
		{
			LootManager.POITierMod = new float[5];
			LootManager.POITierBonus = new float[5];
		}
		if (_rootNode.HasAttribute("poi_tier_mod"))
		{
			string attribute = _rootNode.GetAttribute("poi_tier_mod");
			if (attribute.Contains(","))
			{
				string[] array = attribute.Split(',');
				LootManager.POITierMod = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					LootManager.POITierMod[i] = StringParsers.ParseFloat(array[i]);
				}
			}
			else
			{
				LootManager.POITierMod = new float[1] { StringParsers.ParseFloat(attribute) };
			}
		}
		if (!_rootNode.HasAttribute("poi_tier_bonus"))
		{
			return;
		}
		string attribute2 = _rootNode.GetAttribute("poi_tier_bonus");
		if (attribute2.Contains(","))
		{
			string[] array2 = attribute2.Split(',');
			LootManager.POITierBonus = new float[array2.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				LootManager.POITierBonus[j] = StringParsers.ParseFloat(array2[j]);
			}
		}
		else
		{
			LootManager.POITierBonus = new float[1] { StringParsers.ParseFloat(attribute2) };
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootQualityTemplate(XElement _rootNode)
	{
		foreach (XElement item in _rootNode.Elements("lootqualitytemplate"))
		{
			LootContainer.LootQualityTemplate lootQualityTemplate = new LootContainer.LootQualityTemplate();
			if (item.HasAttribute("name"))
			{
				lootQualityTemplate.name = item.GetAttribute("name");
				if (LootContainer.lootGroups.ContainsKey(lootQualityTemplate.name))
				{
					throw new Exception("lootqualitytemplate '" + lootQualityTemplate.name + "' is defined multiple times");
				}
				foreach (XElement item2 in item.Elements("qualitytemplate"))
				{
					LootContainer.LootGroup lootGroup = new LootContainer.LootGroup();
					lootGroup.minLevel = 0f;
					lootGroup.maxLevel = 1f;
					if (item2.HasAttribute("level"))
					{
						StringParsers.ParseMinMaxCount(item2.GetAttribute("level"), out lootGroup.minLevel, out lootGroup.maxLevel);
					}
					lootGroup.minQuality = -1;
					lootGroup.maxQuality = -1;
					if (item2.HasAttribute("default_quality"))
					{
						StringParsers.ParseMinMaxCount(item2.GetAttribute("default_quality"), out lootGroup.minQuality, out lootGroup.maxQuality);
					}
					foreach (XElement item3 in item2.Elements("loot"))
					{
						LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
						lootEntry.minQuality = lootGroup.minQuality;
						lootEntry.maxQuality = lootGroup.maxQuality;
						if (item3.HasAttribute("quality"))
						{
							StringParsers.ParseMinMaxCount(item3.GetAttribute("quality"), out lootEntry.minQuality, out lootEntry.maxQuality);
						}
						lootEntry.prob = 1f;
						if (item3.HasAttribute("prob") && !StringParsers.TryParseFloat(item3.GetAttribute("prob"), out lootEntry.prob))
						{
							throw new Exception("Parsing error prob '" + item3.GetAttribute("prob") + "' in '" + lootQualityTemplate.name + "' level '" + lootGroup.minLevel.ToCultureInvariantString() + "," + lootGroup.maxLevel.ToCultureInvariantString() + "'");
						}
						lootGroup.items.Add(lootEntry);
					}
					lootQualityTemplate.templates.Add(lootGroup);
				}
				LootContainer.lootQualityTemplates[lootQualityTemplate.name] = lootQualityTemplate;
				continue;
			}
			throw new Exception("Attribute 'name' required on lootqualitytemplate");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootProbabilityTemplate(XElement _rootNode)
	{
		foreach (XElement item in _rootNode.Elements("lootprobtemplate"))
		{
			LootContainer.LootProbabilityTemplate lootProbabilityTemplate = new LootContainer.LootProbabilityTemplate();
			if (item.HasAttribute("name"))
			{
				lootProbabilityTemplate.name = item.GetAttribute("name");
				if (LootContainer.lootGroups.ContainsKey(lootProbabilityTemplate.name))
				{
					throw new Exception("lootprobtemplate '" + lootProbabilityTemplate.name + "' is defined multiple times");
				}
				foreach (XElement item2 in item.Elements("loot"))
				{
					LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
					lootEntry.minLevel = -1f;
					lootEntry.maxLevel = -1f;
					if (item2.HasAttribute("level"))
					{
						StringParsers.ParseMinMaxCount(item2.GetAttribute("level"), out lootEntry.minLevel, out lootEntry.maxLevel);
					}
					lootEntry.prob = 1f;
					if (item2.HasAttribute("prob") && !StringParsers.TryParseFloat(item2.GetAttribute("prob"), out lootEntry.prob))
					{
						throw new Exception("Parsing error prob '" + item2.GetAttribute("prob") + "' in '" + lootProbabilityTemplate.name + "' level '" + lootEntry.minLevel.ToCultureInvariantString() + "," + lootEntry.maxLevel.ToCultureInvariantString() + "'");
					}
					lootProbabilityTemplate.templates.Add(lootEntry);
				}
				LootContainer.lootProbTemplates[lootProbabilityTemplate.name] = lootProbabilityTemplate;
				continue;
			}
			throw new Exception("Attribute 'name' required on lootprobtemplate");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootGroup(XElement _elementGroup)
	{
		LootContainer.LootGroup lootGroup = new LootContainer.LootGroup();
		if (!_elementGroup.HasAttribute("name"))
		{
			throw new Exception("Attribute 'name' required on lootgroup");
		}
		lootGroup.name = _elementGroup.GetAttribute("name");
		if (LootContainer.lootGroups.ContainsKey(lootGroup.name))
		{
			throw new Exception("lootgroup '" + lootGroup.name + "' is defined multiple times");
		}
		if (_elementGroup.HasAttribute("loot_quality_template"))
		{
			lootGroup.lootQualityTemplate = _elementGroup.GetAttribute("loot_quality_template");
		}
		lootGroup.minCount = 1;
		lootGroup.maxCount = 1;
		if (_elementGroup.HasAttribute("count"))
		{
			if (_elementGroup.GetAttribute("count") == "all")
			{
				lootGroup.minCount = -1;
				lootGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("count"), out lootGroup.minCount, out lootGroup.maxCount);
			}
		}
		lootGroup.minLevel = 0f;
		lootGroup.maxLevel = 10000f;
		if (_elementGroup.HasAttribute("level"))
		{
			StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("level"), out lootGroup.minLevel, out lootGroup.maxLevel);
		}
		lootGroup.minQuality = -1;
		lootGroup.maxQuality = -1;
		if (_elementGroup.HasAttribute("quality"))
		{
			StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("quality"), out lootGroup.minQuality, out lootGroup.maxQuality);
		}
		if (_elementGroup.HasAttribute("mods"))
		{
			lootGroup.modsToInstall = _elementGroup.GetAttribute("mods").Split(',');
		}
		else
		{
			lootGroup.modsToInstall = new string[0];
		}
		if (_elementGroup.HasAttribute("mod_chance"))
		{
			lootGroup.modChance = StringParsers.ParseFloat(_elementGroup.GetAttribute("mod_chance"));
		}
		ParseItemList(lootGroup.name, _elementGroup.Elements("item"), lootGroup.items, lootGroup.minQuality, lootGroup.maxQuality);
		for (int i = 0; i < lootGroup.items.Count; i++)
		{
			lootGroup.items[i].parentGroup = lootGroup;
		}
		LootContainer.lootGroups[lootGroup.name] = lootGroup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootContainer(XElement _elementContainer)
	{
		LootContainer lootContainer = new LootContainer();
		if (!_elementContainer.HasAttribute("name"))
		{
			throw new XmlException("Attribute 'name' missing on container");
		}
		string attribute = _elementContainer.GetAttribute("name");
		if (LootContainer.GetLootContainer(attribute, _errorOnMiss: false) != null)
		{
			throw new Exception("Duplicate lootlist entry with name " + attribute);
		}
		lootContainer.Name = attribute;
		if (_elementContainer.HasAttribute("count"))
		{
			StringParsers.ParseMinMaxCount(_elementContainer.GetAttribute("count"), out lootContainer.minCount, out lootContainer.maxCount);
		}
		else
		{
			lootContainer.minCount = (lootContainer.maxCount = 1);
		}
		if (_elementContainer.HasAttribute("size"))
		{
			lootContainer.size = StringParsers.ParseVector2i(_elementContainer.GetAttribute("size"));
			if (lootContainer.size == Vector2i.zero)
			{
				lootContainer.size = new Vector2i(3, 3);
			}
		}
		lootContainer.BuffActions = new List<string>();
		if (_elementContainer.HasAttribute("buff"))
		{
			lootContainer.BuffActions.AddRange(_elementContainer.GetAttribute("buff").Replace(" ", "").Split(','));
		}
		if (_elementContainer.HasAttribute("sound_open"))
		{
			lootContainer.soundOpen = _elementContainer.GetAttribute("sound_open");
		}
		if (_elementContainer.HasAttribute("sound_close"))
		{
			lootContainer.soundClose = _elementContainer.GetAttribute("sound_close");
		}
		if (_elementContainer.HasAttribute("ignore_loot_abundance"))
		{
			lootContainer.ignoreLootAbundance = StringParsers.ParseBool(_elementContainer.GetAttribute("ignore_loot_abundance"));
		}
		if (_elementContainer.HasAttribute("unique_items"))
		{
			lootContainer.UniqueItems = StringParsers.ParseBool(_elementContainer.GetAttribute("unique_items"));
		}
		if (_elementContainer.HasAttribute("ignore_loot_prob"))
		{
			lootContainer.IgnoreLootProb = StringParsers.ParseBool(_elementContainer.GetAttribute("ignore_loot_prob"));
		}
		if (_elementContainer.HasAttribute("unmodified_lootstage"))
		{
			lootContainer.useUnmodifiedLootstage = StringParsers.ParseBool(_elementContainer.GetAttribute("unmodified_lootstage"));
		}
		string attribute2 = _elementContainer.GetAttribute("destroy_on_close");
		if (attribute2.Length > 0)
		{
			lootContainer.destroyOnClose = EnumUtils.Parse<LootContainer.DestroyOnClose>(attribute2, _ignoreCase: true);
		}
		if (_elementContainer.HasAttribute("on_open_event"))
		{
			lootContainer.OnOpenEvent = _elementContainer.GetAttribute("on_open_event");
		}
		if (_elementContainer.HasAttribute("open_time"))
		{
			if (StringParsers.TryParseFloat(_elementContainer.GetAttribute("open_time"), out var _result))
			{
				lootContainer.openTime = _result;
			}
			else
			{
				lootContainer.openTime = 1f;
			}
		}
		else
		{
			lootContainer.openTime = 1f;
		}
		if (_elementContainer.HasAttribute("loot_quality_template"))
		{
			string attribute3 = _elementContainer.GetAttribute("loot_quality_template");
			if (LootContainer.lootQualityTemplates.ContainsKey(attribute3))
			{
				lootContainer.lootQualityTemplate = attribute3;
			}
			else
			{
				Log.Error("LootContainer {0} uses an unknown loot_quality_template \"{1}\"", attribute, attribute3);
			}
		}
		else
		{
			lootContainer.lootQualityTemplate = string.Empty;
		}
		ParseItemList(attribute, _elementContainer.Elements("item"), lootContainer.itemsToSpawn, -1, -1);
		_ = lootContainer.itemsToSpawn.Count;
		lootContainer.Init();
	}
}
