using System;
using System.Collections;
using System.Xml;
using System.Xml.Linq;

public class RecipesFromXml
{
	public static bool SaveRecipes(string _filename)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement node = xmlDocument.AddXmlElement("recipes");
		foreach (Recipe allRecipe in CraftingManager.GetAllRecipes())
		{
			XmlElement xmlElement = node.AddXmlElement("recipe").SetAttrib("name", allRecipe.GetName()).SetAttrib("count", allRecipe.count.ToString())
				.SetAttrib("scrapable", allRecipe.scrapable.ToString());
			if (allRecipe.tooltip != null)
			{
				xmlElement.SetAttrib("tooltip", allRecipe.tooltip);
			}
			if (!string.IsNullOrEmpty(allRecipe.craftingArea))
			{
				xmlElement.SetAttrib("craft_area", allRecipe.craftingArea);
			}
			if (allRecipe.craftingToolType != 0)
			{
				xmlElement.SetAttrib("craft_tool", ItemClass.GetForId(allRecipe.craftingToolType).GetItemName());
			}
			for (int i = 0; i < allRecipe.ingredients.Count; i++)
			{
				ItemStack itemStack = allRecipe.ingredients[i];
				if (itemStack?.itemValue != null && ItemClass.GetForId(itemStack.itemValue.type) != null)
				{
					xmlElement.AddXmlElement("ingredient").SetAttrib("name", ItemClass.GetForId(itemStack.itemValue.type).GetItemName()).SetAttrib("count", itemStack.count.ToString());
				}
			}
			if (allRecipe.wildcardForgeCategory)
			{
				xmlElement.AddXmlElement("wildcard_forge_category");
			}
		}
		xmlDocument.SdSave(_filename);
		return true;
	}

	public static IEnumerator LoadRecipies(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <recipes> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item2 in root.Elements("recipe"))
		{
			Recipe recipe = new Recipe();
			if (!item2.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing on recipe");
			}
			string attribute = item2.GetAttribute("name");
			recipe.itemValueType = ItemClass.GetItem(attribute).type;
			if (recipe.itemValueType == 0)
			{
				throw new Exception("No item/block with name '" + attribute + "' existing");
			}
			recipe.count = 1;
			if (item2.HasAttribute("count"))
			{
				recipe.count = int.Parse(item2.GetAttribute("count"));
			}
			recipe.scrapable = false;
			if (item2.HasAttribute("scrapable"))
			{
				recipe.scrapable = StringParsers.ParseBool(item2.GetAttribute("scrapable"));
			}
			recipe.materialBasedRecipe = false;
			if (item2.HasAttribute("material_based"))
			{
				recipe.materialBasedRecipe = StringParsers.ParseBool(item2.GetAttribute("material_based"));
			}
			if (item2.HasAttribute("tags"))
			{
				recipe.tags = FastTags<TagGroup.Global>.Parse(item2.GetAttribute("tags") + "," + attribute);
			}
			else if (item2.HasAttribute("tag"))
			{
				recipe.tags = FastTags<TagGroup.Global>.Parse(item2.GetAttribute("tag") + "," + attribute);
			}
			if (item2.HasAttribute("tooltip"))
			{
				recipe.tooltip = item2.GetAttribute("tooltip");
			}
			if (item2.HasAttribute("craft_area"))
			{
				string attribute2 = item2.GetAttribute("craft_area");
				recipe.craftingArea = attribute2;
			}
			else
			{
				recipe.craftingArea = "";
			}
			if (item2.HasAttribute("craft_tool"))
			{
				recipe.craftingToolType = ItemClass.GetItem(item2.GetAttribute("craft_tool")).type;
				ItemClass.list[ItemClass.GetItem(item2.GetAttribute("craft_tool")).type].bCraftingTool = true;
			}
			else
			{
				recipe.craftingToolType = 0;
			}
			if (item2.HasAttribute("craft_time"))
			{
				float _result = 0f;
				StringParsers.TryParseFloat(item2.GetAttribute("craft_time"), out _result);
				recipe.craftingTime = _result;
			}
			else
			{
				recipe.craftingTime = -1f;
			}
			if (item2.HasAttribute("learn_exp_gain"))
			{
				float _result2 = 0f;
				if (StringParsers.TryParseFloat(item2.GetAttribute("learn_exp_gain"), out _result2))
				{
					recipe.unlockExpGain = (int)_result2;
				}
				else
				{
					recipe.unlockExpGain = 20;
				}
			}
			else
			{
				recipe.unlockExpGain = -1;
			}
			if (item2.HasAttribute("craft_exp_gain"))
			{
				float _result3 = 0f;
				if (StringParsers.TryParseFloat(item2.GetAttribute("craft_exp_gain"), out _result3))
				{
					recipe.craftExpGain = (int)_result3;
				}
				else
				{
					recipe.craftExpGain = 1;
				}
			}
			else
			{
				recipe.craftExpGain = -1;
			}
			if (item2.HasAttribute("is_trackable"))
			{
				recipe.IsTrackable = StringParsers.ParseBool(item2.GetAttribute("is_trackable"));
			}
			else
			{
				recipe.IsTrackable = true;
			}
			recipe.UseIngredientModifier = true;
			if (item2.HasAttribute("use_ingredient_modifier"))
			{
				recipe.UseIngredientModifier = StringParsers.ParseBool(item2.GetAttribute("use_ingredient_modifier"));
			}
			recipe.Effects = MinEffectController.ParseXml(item2);
			foreach (XElement item3 in item2.Elements())
			{
				if (item3.Name == "ingredient")
				{
					XElement element = item3;
					if (!element.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing on ingredient in recipe '" + attribute + "'");
					}
					string attribute3 = element.GetAttribute("name");
					ItemValue item = ItemClass.GetItem(attribute3);
					if (item.IsEmpty())
					{
						throw new Exception("No item/block/material with name '" + attribute3 + "' existing");
					}
					int count = 1;
					if (element.HasAttribute("count"))
					{
						count = int.Parse(element.GetAttribute("count"));
					}
					recipe.AddIngredient(item, count);
				}
				else if (item3.Name == "wildcard_forge_category")
				{
					recipe.wildcardForgeCategory = true;
				}
			}
			recipe.Init();
			CraftingManager.AddRecipe(recipe);
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		CraftingManager.PostInit();
	}
}
