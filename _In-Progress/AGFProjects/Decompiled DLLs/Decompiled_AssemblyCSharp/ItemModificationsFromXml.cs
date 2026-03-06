using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemModificationsFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <item_modifiers> found!");
		}
		ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement item in root.Elements("item_modifier"))
		{
			ParseModifier(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseModifier(XElement _element)
	{
		ItemClassModifier itemClassModifier = new ItemClassModifier();
		itemClassModifier.Groups = new string[1] { "Mods" };
		if (_element.HasAttribute("installable_tags"))
		{
			itemClassModifier.InstallableTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("installable_tags"));
		}
		if (_element.HasAttribute("blocked_tags"))
		{
			itemClassModifier.DisallowedTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("blocked_tags"));
		}
		if (_element.HasAttribute("modifier_tags"))
		{
			itemClassModifier.ItemTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("modifier_tags"));
		}
		if (_element.HasAttribute("type"))
		{
			itemClassModifier.Type = EnumUtils.Parse<ItemClassModifier.ModifierTypes>(_element.GetAttribute("type"), _ignoreCase: true);
		}
		parseItem(_element, itemClassModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement elementItem, ItemClassModifier item)
	{
		string empty = string.Empty;
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!elementItem.HasAttribute("name"))
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		empty = elementItem.GetAttribute("name");
		item.CosmeticInstallChance = 1f;
		if (elementItem.HasAttribute("cosmetic_install_chance"))
		{
			item.CosmeticInstallChance = StringParsers.ParseFloat(elementItem.GetAttribute("cosmetic_install_chance"));
		}
		item.PropertyOverrides = new Dictionary<string, DynamicProperties>();
		RequirementGroup[] array = new RequirementGroup[3];
		foreach (XElement item2 in elementItem.Elements())
		{
			if (item2.Name == XNames.property)
			{
				dynamicProperties.Add(item2);
				string attribute = item2.GetAttribute(XNames.class_);
				if (attribute.StartsWith("Action"))
				{
					int num = attribute[attribute.Length - 1] - 48;
					array[num] = RequirementBase.ParseRequirementGroup(item2);
				}
			}
			else
			{
				if (!(item2.Name == "item_property_overrides") || !item2.HasAttribute(XNames.name))
				{
					continue;
				}
				string attribute2 = item2.GetAttribute(XNames.name);
				DynamicProperties dynamicProperties2 = new DynamicProperties();
				foreach (XElement item3 in item2.Elements(XNames.property))
				{
					dynamicProperties2.Add(item3);
				}
				if (dynamicProperties2.Values.Dict.Count > 0)
				{
					item.PropertyOverrides[attribute2] = dynamicProperties2;
				}
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text);
			if (itemClass == null)
			{
				throw new Exception($"Extends item {text} is not specified for item {empty}'");
			}
			HashSet<string> hashSet = new HashSet<string> { Block.PropCreativeMode };
			if (dynamicProperties.Params1.ContainsKey("Extends"))
			{
				string[] array2 = dynamicProperties.Params1["Extends"].Split(',');
				foreach (string text2 in array2)
				{
					hashSet.Add(text2.Trim());
				}
			}
			DynamicProperties dynamicProperties3 = new DynamicProperties();
			dynamicProperties3.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties3.CopyFrom(dynamicProperties);
			dynamicProperties = dynamicProperties3;
		}
		item.Properties = dynamicProperties;
		item.Effects = MinEffectController.ParseXml(elementItem, null, MinEffectController.SourceParentType.ItemModifierClass, item.Id);
		item.SetName(empty);
		item.setLocalizedItemName(Localization.Get(empty));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			item.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			item.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			item.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"]));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			item.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"]));
		}
		if (dynamicProperties.Values.ContainsKey("Material"))
		{
			item.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		}
		else
		{
			item.MadeOfMaterial = MaterialBlock.fromString("Miron");
		}
		if (dynamicProperties.Values.ContainsKey("Meshfile") && item.CanHold())
		{
			item.MeshFile = dynamicProperties.Values["Meshfile"];
			DataLoader.PreloadBundle(item.MeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("StickyOffset"))
		{
			StringParsers.TryParseFloat(dynamicProperties.Values["StickyOffset"], out item.StickyOffset);
		}
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			item.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			item.Active = new DataItem<bool>(_startValue: false);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && item.CanHold())
		{
			item.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(item.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && item.CanHold())
		{
			item.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(item.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string s = dynamicProperties.Values["HoldType"];
			int result = 0;
			if (!int.TryParse(s, out result))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + empty + "'");
			}
			item.HoldType = new DataItem<int>(result);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',');
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = new DataItem<string>(array3[j]);
			}
			item.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int result2 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out result2);
			item.RepairAmount = new DataItem<int>(result2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float _result = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out _result);
			item.RepairTime = new DataItem<float>(_result);
		}
		else if (item.RepairAmount != null)
		{
			item.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			item.MaxUseTimes = new DataItem<int>(0);
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(_startValue: false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"]));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(_startValue: true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			item.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"]);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties4 = dynamicProperties.Classes["Preview"];
			item.Preview = new PreviewData();
			if (dynamicProperties4.Values.ContainsKey("Zoom"))
			{
				item.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties4.Values["Zoom"]));
			}
			if (dynamicProperties4.Values.ContainsKey("Pos"))
			{
				item.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties4.Values["Pos"]));
			}
			else
			{
				item.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties4.Values.ContainsKey("Rot"))
			{
				item.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties4.Values["Rot"]));
			}
			else
			{
				item.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int k = 0; k < item.Actions.Length; k++)
		{
			string text3 = ItemClass.itemActionNames[k];
			if (dynamicProperties.Classes.ContainsKey(text3))
			{
				ItemAction itemAction = null;
				if (!dynamicProperties.Values.ContainsKey(text3 + ".Class"))
				{
					throw new Exception("No class attribute found on " + text3 + " in item with '" + empty + "'");
				}
				string text4 = dynamicProperties.Values[text3 + ".Class"];
				try
				{
					itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", text4));
				}
				catch (Exception)
				{
					throw new Exception("ItemAction class '" + text4 + " could not be instantiated");
				}
				itemAction.item = item;
				itemAction.ReadFrom(dynamicProperties.Classes[text3]);
				if (array[k] != null)
				{
					itemAction.ExecutionRequirements = array[k];
				}
				item.Actions[k] = itemAction;
			}
		}
		item.Init();
	}
}
