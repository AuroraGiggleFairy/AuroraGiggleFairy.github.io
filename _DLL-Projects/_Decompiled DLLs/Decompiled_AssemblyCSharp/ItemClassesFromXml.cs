using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemClassesFromXml
{
	public static void CreateItemsFromBlocks()
	{
		for (int i = 1; i < Block.ItemsStartHere; i++)
		{
			if (Block.list[i] != null)
			{
				ItemClassBlock itemClassBlock = new ItemClassBlock();
				itemClassBlock.SetId(Block.list[i].blockID);
				itemClassBlock.SetName(Block.list[i].GetBlockName());
				itemClassBlock.Stacknumber = new DataItem<int>(Block.list[i].Stacknumber);
				ItemClass.list[itemClassBlock.Id] = itemClassBlock;
				itemClassBlock.Init();
			}
		}
	}

	public static IEnumerator CreateItems(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <items> found!");
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (XElement item in from i in root.Elements("items")
			from a in i.Elements("animation")
			from h in a.Elements("hold_type")
			select h)
		{
			if (!item.HasAttribute("id"))
			{
				throw new Exception("hold_type with missing name or id!");
			}
			dictionary[item.GetAttribute("id")] = StringParsers.ParseBool(item.GetAttribute("newmodel"));
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item2 in root.Elements())
		{
			if (item2.Name == "item")
			{
				parseItem(item2);
			}
			else if (item2.Name == "animation")
			{
				parseAnimation(item2);
			}
			else if (item2.Name == "noise")
			{
				parseNoise(item2);
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement _node)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		string attribute = _node.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		RequirementGroup[] array = new RequirementGroup[3];
		foreach (XElement item in _node.Elements("property"))
		{
			dynamicProperties.Add(item);
			string attribute2 = item.GetAttribute("class");
			if (attribute2.StartsWith("Action"))
			{
				int num = attribute2[attribute2.Length - 1] - 48;
				array[num] = RequirementBase.ParseRequirementGroup(item);
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text);
			if (itemClass == null)
			{
				throw new Exception($"Extends item {text} is not specified for item {attribute}'");
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
			DynamicProperties dynamicProperties2 = new DynamicProperties();
			dynamicProperties2.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties2.CopyFrom(dynamicProperties);
			dynamicProperties = dynamicProperties2;
		}
		ItemClass itemClass2;
		if (dynamicProperties.Values.ContainsKey("Class"))
		{
			string text3 = dynamicProperties.Values["Class"];
			try
			{
				itemClass2 = (ItemClass)Activator.CreateInstance(Type.GetType(text3));
			}
			catch (Exception)
			{
				throw new Exception("No item class '" + text3 + " found!");
			}
		}
		else
		{
			itemClass2 = new ItemClass();
		}
		itemClass2.Properties = dynamicProperties;
		if (dynamicProperties.Params1.ContainsKey("Extends"))
		{
			string text4 = dynamicProperties.Values["Extends"];
			if (ItemClass.GetItemClass(text4) == null)
			{
				throw new Exception($"Extends item {text4} is not specified for item {attribute}'");
			}
		}
		itemClass2.Effects = MinEffectController.ParseXml(_node, null, MinEffectController.SourceParentType.ItemClass, itemClass2.Id);
		itemClass2.SetName(attribute);
		itemClass2.setLocalizedItemName(Localization.Get(attribute));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			itemClass2.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			itemClass2.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			itemClass2.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"]));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			itemClass2.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"]));
		}
		if (!dynamicProperties.Values.ContainsKey("Material"))
		{
			throw new Exception("Attribute 'material' missing on item '" + attribute + "'");
		}
		itemClass2.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		if (itemClass2.MadeOfMaterial == null)
		{
			throw new Exception("Attribute 'material' '" + dynamicProperties.Values["Material"] + "' refers to not existing material in item '" + attribute + "'");
		}
		if (!dynamicProperties.Values.ContainsKey("Meshfile") && itemClass2.CanHold())
		{
			throw new Exception("Attribute 'Meshfile' missing on item '" + attribute + "'");
		}
		itemClass2.MeshFile = dynamicProperties.Values["Meshfile"];
		DataLoader.PreloadBundle(itemClass2.MeshFile);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyOffset"], out itemClass2.StickyOffset);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyColliderRadius"], out itemClass2.StickyColliderRadius);
		StringParsers.TryParseSInt32(dynamicProperties.Values["StickyColliderUp"], out itemClass2.StickyColliderUp);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyColliderLength"], out itemClass2.StickyColliderLength);
		itemClass2.StickyMaterial = dynamicProperties.Values["StickyMaterial"];
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			itemClass2.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			itemClass2.Active = new DataItem<bool>(_startValue: false);
		}
		if (dynamicProperties.Values.ContainsKey(ItemClass.PropIsSticky))
		{
			itemClass2.IsSticky = StringParsers.ParseBool(dynamicProperties.Values[ItemClass.PropIsSticky]);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && itemClass2.CanHold())
		{
			itemClass2.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(itemClass2.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && itemClass2.CanHold())
		{
			itemClass2.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(itemClass2.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string s = dynamicProperties.Values["HoldType"];
			int result = 0;
			if (!int.TryParse(s, out result))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + attribute + "'");
			}
			itemClass2.HoldType = new DataItem<int>(result);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',');
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = new DataItem<string>(array3[j]);
			}
			itemClass2.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int result2 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out result2);
			itemClass2.RepairAmount = new DataItem<int>(result2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float _result = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out _result);
			itemClass2.RepairTime = new DataItem<float>(_result);
		}
		else if (itemClass2.RepairAmount != null)
		{
			itemClass2.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			itemClass2.MaxUseTimes = new DataItem<int>(0);
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(_startValue: false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"]));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(_startValue: true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			itemClass2.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"]);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties3 = dynamicProperties.Classes["Preview"];
			itemClass2.Preview = new PreviewData();
			if (dynamicProperties3.Values.ContainsKey("Zoom"))
			{
				itemClass2.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties3.Values["Zoom"]));
			}
			if (dynamicProperties3.Values.ContainsKey("Pos"))
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties3.Values["Pos"]));
			}
			else
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties3.Values.ContainsKey("Rot"))
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties3.Values["Rot"]));
			}
			else
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int k = 0; k < itemClass2.Actions.Length; k++)
		{
			string text5 = ItemClass.itemActionNames[k];
			if (dynamicProperties.Classes.ContainsKey(text5))
			{
				if (!dynamicProperties.Values.ContainsKey(text5 + ".Class"))
				{
					throw new Exception("No class attribute found on " + text5 + " in item with '" + attribute + "'");
				}
				string text6 = dynamicProperties.Values[text5 + ".Class"];
				ItemAction itemAction;
				try
				{
					itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", text6));
				}
				catch (Exception)
				{
					throw new Exception("ItemAction class '" + text6 + " could not be instantiated");
				}
				itemAction.item = itemClass2;
				itemAction.ActionIndex = k;
				itemAction.ReadFrom(dynamicProperties.Classes[text5]);
				if (array[k] != null)
				{
					itemAction.ExecutionRequirements = array[k];
				}
				itemClass2.Actions[k] = itemAction;
			}
		}
		itemClass2.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAnimation(XElement _node)
	{
		foreach (XElement item in _node.Elements("hold_type"))
		{
			if (!item.HasAttribute("id"))
			{
				throw new Exception("Attribute 'id' missing in hold_type");
			}
			int result = 0;
			if (!int.TryParse(item.GetAttribute("id"), out result))
			{
				throw new Exception("Unknown hold_type id for animation");
			}
			float num = 0f;
			if (item.HasAttribute("ray_cast"))
			{
				num = StringParsers.ParseFloat(item.GetAttribute("ray_cast"));
			}
			float rayCastMoving = num;
			if (item.HasAttribute("ray_cast_moving"))
			{
				num = StringParsers.ParseFloat(item.GetAttribute("ray_cast_moving"));
			}
			float num2 = Constants.cMinHolsterTime;
			if (item.HasAttribute("holster"))
			{
				num2 = Utils.FastMax(StringParsers.ParseFloat(item.GetAttribute("holster")), num2);
			}
			float num3 = Constants.cMinUnHolsterTime;
			if (item.HasAttribute("unholster"))
			{
				num3 = Utils.FastMax(StringParsers.ParseFloat(item.GetAttribute("unholster")), num3);
			}
			bool twoHanded = false;
			if (item.HasAttribute("two_handed"))
			{
				twoHanded = StringParsers.ParseBool(item.GetAttribute("two_handed"));
			}
			AnimationDelayData.AnimationDelay[result] = new AnimationDelayData.AnimationDelays(num, rayCastMoving, num2, num3, twoHanded);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseNoise(XElement _node)
	{
		foreach (XElement item in _node.Elements("sound"))
		{
			if (!item.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing in noise/sound");
			}
			string attribute = item.GetAttribute("name");
			float volume = 0f;
			if (item.HasAttribute("volume"))
			{
				volume = StringParsers.ParseFloat(item.GetAttribute("volume"));
			}
			if (!item.HasAttribute("time"))
			{
				throw new Exception("Attribute 'time' missing in noise/sound name='" + attribute + "'");
			}
			float duration = StringParsers.ParseFloat(item.GetAttribute("time"));
			float muffledWhenCrouched = 1f;
			if (item.HasAttribute("muffled_when_crouched"))
			{
				muffledWhenCrouched = StringParsers.ParseFloat(item.GetAttribute("muffled_when_crouched"));
			}
			float heatMapStrength = 0f;
			if (item.HasAttribute("heat_map_strength"))
			{
				heatMapStrength = StringParsers.ParseFloat(item.GetAttribute("heat_map_strength"));
			}
			float num = 100f;
			if (item.HasAttribute("heat_map_time"))
			{
				num = StringParsers.ParseFloat(item.GetAttribute("heat_map_time"));
			}
			num *= 10f;
			AIDirectorData.AddNoisySound(attribute, new AIDirectorData.Noise(attribute, volume, duration, muffledWhenCrouched, heatMapStrength, (ulong)num));
		}
	}
}
