using System;
using System.Collections;
using System.Xml.Linq;

public class ProgressionFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <progression> found!");
		}
		if (Progression.ProgressionClasses != null)
		{
			Progression.ProgressionClasses.Clear();
		}
		else
		{
			Progression.ProgressionClasses = new CaseInsensitiveStringDictionary<ProgressionClass>();
		}
		foreach (XElement item in root.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "level":
				parseLevelNode(item);
				break;
			case "attributes":
			case "skills":
			case "perks":
			case "book_groups":
			case "crafting_skills":
				parseProgressionItems(item);
				break;
			}
		}
		foreach (string key in Progression.ProgressionClasses.Keys)
		{
			ProgressionClass progressionClass = Progression.ProgressionClasses[key];
			if (progressionClass.ParentName == null || !(progressionClass.ParentName != string.Empty))
			{
				continue;
			}
			if (!Progression.ProgressionClasses.TryGetValue(progressionClass.ParentName, out var value))
			{
				Log.Error("Progression class '" + key + "' has non-existing parent name '" + progressionClass.ParentName + "'");
			}
			else
			{
				value.Children.Add(progressionClass);
				if (progressionClass.IsBook || progressionClass.IsBookGroup)
				{
					Progression.ProgressionClasses[progressionClass.ParentName].DisplayType = ProgressionClass.DisplayTypes.Book;
				}
				else if (progressionClass.IsCrafting)
				{
					Progression.ProgressionClasses[progressionClass.ParentName].DisplayType = ProgressionClass.DisplayTypes.Crafting;
				}
			}
		}
		clearProgressionValueLinks();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void clearProgressionValueLinks()
	{
		if (!GameManager.Instance)
		{
			return;
		}
		DictionaryList<int, Entity> dictionaryList = GameManager.Instance.World?.Entities;
		if (dictionaryList == null || dictionaryList.Count == 0)
		{
			return;
		}
		foreach (Entity item in dictionaryList.list)
		{
			if (item is EntityAlive entityAlive)
			{
				entityAlive.Progression?.ClearProgressionClassLinks();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseLevelNode(XElement element)
	{
		if (element.HasAttribute("exp_to_level"))
		{
			Progression.BaseExpToLevel = int.Parse(element.GetAttribute("exp_to_level"));
		}
		else
		{
			Progression.BaseExpToLevel = 500;
		}
		if (element.HasAttribute("clamp_exp_cost_at_level"))
		{
			Progression.ClampExpCostAtLevel = int.Parse(element.GetAttribute("clamp_exp_cost_at_level"));
		}
		else
		{
			Progression.ClampExpCostAtLevel = 300;
		}
		if (element.HasAttribute("experience_multiplier"))
		{
			Progression.ExpMultiplier = StringParsers.ParseFloat(element.GetAttribute("experience_multiplier"));
		}
		else
		{
			Progression.ExpMultiplier = 1.02f;
		}
		if (element.HasAttribute("skill_points_per_level"))
		{
			Progression.SkillPointsPerLevel = int.Parse(element.GetAttribute("skill_points_per_level"));
		}
		else
		{
			Progression.SkillPointsPerLevel = 1;
		}
		if (element.HasAttribute("skill_point_multiplier"))
		{
			Progression.SkillPointMultiplier = StringParsers.ParseFloat(element.GetAttribute("skill_point_multiplier"));
		}
		else
		{
			Progression.SkillPointMultiplier = 0f;
		}
		if (element.HasAttribute("max_level"))
		{
			Progression.MaxLevel = int.Parse(element.GetAttribute("max_level"));
		}
		else
		{
			Progression.MaxLevel = 200;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseProgressionItems(XElement element)
	{
		int max_level = 0;
		int min_level = 0;
		int base_cost = 1;
		float cost_multiplier_per_level = 1f;
		float max_level_ratio_to_parent = 1f;
		if (element.HasAttribute("min_level"))
		{
			min_level = int.Parse(element.GetAttribute("min_level"));
		}
		else if (element.Name == "attributes")
		{
			min_level = 1;
		}
		else if (element.Name == "skills")
		{
			min_level = 1;
		}
		else if (element.Name == "perks")
		{
			min_level = 0;
		}
		else if (element.Name == "crafting_skills")
		{
			min_level = 1;
		}
		if (element.HasAttribute("max_level"))
		{
			max_level = int.Parse(element.GetAttribute("max_level"));
		}
		else if (element.Name == "attributes")
		{
			max_level = 10;
		}
		else if (element.Name == "skills")
		{
			max_level = 100;
		}
		else if (element.Name == "perks")
		{
			max_level = 5;
		}
		else if (element.Name == "crafting_skills")
		{
			max_level = 100;
		}
		if (element.HasAttribute("max_level_ratio_to_parent"))
		{
			max_level_ratio_to_parent = StringParsers.ParseFloat(element.GetAttribute("max_level_ratio_to_parent"));
		}
		if (element.HasAttribute("base_skill_point_cost"))
		{
			base_cost = int.Parse(element.GetAttribute("base_skill_point_cost"));
		}
		else if (element.HasAttribute("base_exp_cost"))
		{
			base_cost = int.Parse(element.GetAttribute("base_exp_cost"));
		}
		if (element.HasAttribute("cost_multiplier_per_level"))
		{
			cost_multiplier_per_level = StringParsers.ParseFloat(element.GetAttribute("cost_multiplier_per_level"));
		}
		if (element.Name == "crafting_skills" && element.HasAttribute("complete_sound"))
		{
			ProgressionClass.DisplayData.CompletionSound = element.GetAttribute("complete_sound");
		}
		float order = 1f;
		foreach (XElement item in element.Elements())
		{
			parseProgressionItem(item, max_level, min_level, base_cost, cost_multiplier_per_level, max_level_ratio_to_parent, ref order);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseProgressionItem(XElement childElement, int max_level, int min_level, int base_cost, float cost_multiplier_per_level, float max_level_ratio_to_parent, ref float order)
	{
		ProgressionType progressionType = ProgressionType.None;
		if (childElement.Name == "attribute")
		{
			progressionType = ProgressionType.Attribute;
		}
		else if (childElement.Name == "skill")
		{
			progressionType = ProgressionType.Skill;
		}
		else if (childElement.Name == "perk")
		{
			progressionType = ProgressionType.Perk;
		}
		else if (childElement.Name == "book")
		{
			progressionType = ProgressionType.Book;
		}
		else if (childElement.Name == "book_group")
		{
			progressionType = ProgressionType.BookGroup;
		}
		else if (childElement.Name == "crafting_skill")
		{
			progressionType = ProgressionType.Crafting;
		}
		if (progressionType == ProgressionType.None)
		{
			return;
		}
		if (childElement.HasAttribute("min_level"))
		{
			min_level = int.Parse(childElement.GetAttribute("min_level"));
		}
		if (childElement.HasAttribute("max_level"))
		{
			max_level = int.Parse(childElement.GetAttribute("max_level"));
		}
		if (childElement.HasAttribute("max_level_ratio_to_parent"))
		{
			max_level_ratio_to_parent = StringParsers.ParseFloat(childElement.GetAttribute("max_level_ratio_to_parent"));
		}
		if (childElement.HasAttribute("base_skill_point_cost"))
		{
			base_cost = int.Parse(childElement.GetAttribute("base_skill_point_cost"));
		}
		else if (childElement.HasAttribute("base_exp_cost"))
		{
			base_cost = int.Parse(childElement.GetAttribute("base_exp_cost"));
		}
		if (childElement.HasAttribute("cost_multiplier_per_level"))
		{
			cost_multiplier_per_level = StringParsers.ParseFloat(childElement.GetAttribute("cost_multiplier_per_level"));
		}
		if (!childElement.HasAttribute("name"))
		{
			return;
		}
		ProgressionClass progressionClass = new ProgressionClass(childElement.GetAttribute("name").ToLower())
		{
			MinLevel = min_level,
			MaxLevel = max_level,
			Type = progressionType,
			BaseCostToLevel = base_cost,
			CostMultiplier = ((cost_multiplier_per_level > 0f) ? cost_multiplier_per_level : 1f),
			ParentMaxLevelRatio = max_level_ratio_to_parent
		};
		progressionClass.ListSortOrder = order;
		order += 1f;
		if (childElement.HasAttribute("parent"))
		{
			progressionClass.ParentName = childElement.GetAttribute("parent").ToLower();
		}
		if (childElement.HasAttribute("name_key"))
		{
			progressionClass.NameKey = childElement.GetAttribute("name_key");
		}
		if (childElement.HasAttribute("max_level_ratio_to_parent"))
		{
			progressionClass.ParentMaxLevelRatio = StringParsers.ParseFloat(childElement.GetAttribute("max_level_ratio_to_parent"));
		}
		if (childElement.HasAttribute("desc_key"))
		{
			progressionClass.DescKey = childElement.GetAttribute("desc_key");
		}
		if (childElement.HasAttribute("long_desc_key"))
		{
			progressionClass.LongDescKey = childElement.GetAttribute("long_desc_key");
		}
		if (childElement.HasAttribute("icon"))
		{
			progressionClass.Icon = childElement.GetAttribute("icon");
		}
		childElement.ParseAttribute("hidden", ref progressionClass.Hidden);
		if (childElement.HasAttribute("override_cost"))
		{
			string[] array = childElement.GetAttribute("override_cost").Split(',');
			progressionClass.OverrideCost = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				progressionClass.OverrideCost[i] = StringParsers.ParseSInt32(array[i]);
			}
		}
		foreach (XElement item2 in childElement.Elements())
		{
			if (item2.Name == "level_requirements")
			{
				int level = 0;
				if (item2.HasAttribute("level"))
				{
					level = StringParsers.ParseSInt32(item2.GetAttribute("level"));
				}
				LevelRequirement lr = new LevelRequirement(level);
				if (item2.HasElements)
				{
					lr.Requirements = RequirementBase.ParseRequirementGroup(item2);
				}
				progressionClass.AddLevelRequirement(lr);
			}
			else
			{
				if (!(item2.Name == "display_entry"))
				{
					continue;
				}
				string[] array2 = item2.GetAttribute("unlock_level").Split(',');
				int[] array3 = new int[array2.Length];
				string item = "";
				string[] customName = null;
				string[] array4 = null;
				string[] customIconTint = null;
				bool customHasQuality = false;
				for (int j = 0; j < array2.Length; j++)
				{
					array3[j] = StringParsers.ParseSInt32(array2[j]);
				}
				if (item2.HasAttribute("item"))
				{
					item = item2.GetAttribute("item");
				}
				if (item2.HasAttribute("has_quality"))
				{
					customHasQuality = StringParsers.ParseBool(item2.GetAttribute("has_quality"));
				}
				if (item2.HasAttribute("icon"))
				{
					array4 = item2.GetAttribute("icon").Split(',');
				}
				if (array4 != null)
				{
					customIconTint = ((!item2.HasAttribute("icontint")) ? new string[array4.Length] : item2.GetAttribute("icontint").Split(','));
				}
				if (item2.HasAttribute("name"))
				{
					customName = item2.GetAttribute("name").Split(',');
				}
				if (item2.HasAttribute("name_key"))
				{
					string[] array5 = item2.GetAttribute("name_key").Split(',');
					for (int k = 0; k < array5.Length; k++)
					{
						array5[k] = Localization.Get(array5[k]);
					}
					customName = array5;
				}
				ProgressionClass.DisplayData displayData = progressionClass.AddDisplayData(item, array3, array4, customIconTint, customName, customHasQuality);
				if (displayData.ItemName != "")
				{
					displayData.AddUnlockData(displayData.ItemName, 0, null);
				}
				if (!item2.HasElements)
				{
					continue;
				}
				foreach (XElement item3 in item2.Elements("unlock_entry"))
				{
					int unlockTier = 0;
					if (item3.HasAttribute("unlock_tier"))
					{
						unlockTier = StringParsers.ParseSInt32(item3.GetAttribute("unlock_tier")) - 1;
					}
					string[] recipeList = null;
					if (item3.HasAttribute("recipes"))
					{
						recipeList = item3.GetAttribute("recipes").Split(',');
					}
					if (item3.HasAttribute("item"))
					{
						string[] array6 = item3.GetAttribute("item").Split(',');
						for (int l = 0; l < array6.Length; l++)
						{
							displayData.AddUnlockData(array6[l], unlockTier, recipeList);
						}
					}
				}
			}
		}
		progressionClass.Effects = MinEffectController.ParseXml(childElement, null, MinEffectController.SourceParentType.ProgressionClass, progressionClass.Name);
		progressionClass.PostInit();
		Progression.ProgressionClasses.Add(progressionClass.Name, progressionClass);
	}
}
