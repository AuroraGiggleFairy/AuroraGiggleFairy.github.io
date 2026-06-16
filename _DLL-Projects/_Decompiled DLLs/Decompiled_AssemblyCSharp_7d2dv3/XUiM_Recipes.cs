using System.Collections.Generic;
using System.Collections.ObjectModel;
using Challenges;
using UnityEngine;

public class XUiM_Recipes : XUiModel
{
	public static float CraftingTimeModifier = 1f;

	public static float CraftingInputModifier = 1f;

	public static float CraftingOutputModifier = 1f;

	public static float SmeltingTimeModifier = 1f;

	public static bool DisableSmelter = false;

	public static float ScrappingOutputModifier = 1f;

	public static float SmeltingOutputModifier = 1f;

	public static float DewCollectorTimeModifier = 1f;

	public static float DewCollectorOutput = 1f;

	public static float DewCollectorInput = 1f;

	public static float ApiaryTimeModifier = 1f;

	public static float ApiaryOutput = 1f;

	public static float ApiaryInput = 1f;

	public static float CollectorTimeModifier = 1f;

	public static int CollectorOutput = 1;

	public static float MiningOutputModifier = 1f;

	public static float HarvestingOutputModifier = 1f;

	public static float CropOutputModifier = 1f;

	public static float SeedDropOutputModifier = 1f;

	public static int CraftingMaxTier = 6;

	public static bool CraftingProgression = true;

	public static BackpackCraftingOptions BackpackCrafting = BackpackCraftingOptions.Enabled;

	public static bool WorkstationCrafting = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Quest sPreviouslyTrackedQuest = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Challenge sPreviouslyTrackedChallenge = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> SandboxIgnoreTag = FastTags<TagGroup.Global>.Parse("sandboxIgnore");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> workbenchTag = FastTags<TagGroup.Global>.Parse("workbench");

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe trackedRecipe;

	public int TrackedRecipeQuality = 1;

	public int TrackedRecipeCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> workbenchOnlyTag = FastTags<TagGroup.Global>.Parse("isworkbench");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> biomeprogressionTag = FastTags<TagGroup.Global>.Parse("biomeProgression");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> useSmelterTag = FastTags<TagGroup.Global>.Parse("use_smelter");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> replaceSmelterTag = FastTags<TagGroup.Global>.Parse("replace_smelter");

	public Recipe TrackedRecipe
	{
		get
		{
			return trackedRecipe;
		}
		set
		{
			if (trackedRecipe != null)
			{
				trackedRecipe.IsTracked = false;
			}
			trackedRecipe = value;
			if (trackedRecipe != null)
			{
				trackedRecipe.IsTracked = true;
			}
			if (this.OnTrackedRecipeChanged != null)
			{
				this.OnTrackedRecipeChanged();
			}
		}
	}

	public event XUiEvent_TrackedQuestChanged OnTrackedRecipeChanged;

	public void SetPreviousTracked(EntityPlayerLocal player)
	{
		XUi xui = player.PlayerUI.xui;
		Quest trackedQuest = player.QuestJournal.TrackedQuest;
		sPreviouslyTrackedChallenge = xui.QuestTracker.TrackedChallenge;
		player.QuestJournal.TrackedQuest = null;
		xui.QuestTracker.TrackedChallenge = null;
		sPreviouslyTrackedQuest = trackedQuest;
	}

	public void ResetToPreviousTracked(EntityPlayerLocal player)
	{
		XUi xui = player.PlayerUI.xui;
		if (sPreviouslyTrackedQuest != null)
		{
			if (!player.QuestJournal.QuestIsActive(sPreviouslyTrackedQuest))
			{
				sPreviouslyTrackedQuest = player.QuestJournal.FindActiveQuest();
			}
		}
		else if (sPreviouslyTrackedChallenge != null && !sPreviouslyTrackedChallenge.IsActive)
		{
			sPreviouslyTrackedChallenge = sPreviouslyTrackedChallenge.Owner.GetNextChallenge(sPreviouslyTrackedChallenge);
		}
		player.QuestJournal.TrackedQuest = sPreviouslyTrackedQuest;
		xui.QuestTracker.TrackedChallenge = sPreviouslyTrackedChallenge;
	}

	public void RefreshTrackedRecipe()
	{
		if (this.OnTrackedRecipeChanged != null)
		{
			this.OnTrackedRecipeChanged();
		}
	}

	public static ReadOnlyCollection<Recipe> GetRecipes()
	{
		return CraftingManager.NonScrapableRecipes;
	}

	public static void UpdateRecipesforBackpackCrafting()
	{
		ReadOnlyCollection<Recipe> recipes = GetRecipes();
		for (int i = 0; i < recipes.Count; i++)
		{
			if (!(recipes[i].craftingArea == ""))
			{
				continue;
			}
			switch (BackpackCrafting)
			{
			case BackpackCraftingOptions.BasicsOnly:
				if (!recipes[i].GetOutputItemClass().IsLimitedItem)
				{
					recipes[i].craftingArea = "workbench";
				}
				break;
			case BackpackCraftingOptions.WorkbenchOnly:
				if (!recipes[i].tags.Test_AnySet(workbenchOnlyTag))
				{
					recipes[i].craftingArea = "workbench";
				}
				break;
			default:
				recipes[i].craftingArea = "workbench";
				break;
			}
		}
	}

	public static void FilterRecipesByCategory(string _category, ref List<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			Recipe recipe = recipeList[i];
			if (recipe.isChallenge)
			{
				list.Add(recipe);
				continue;
			}
			if (recipe.isQuest)
			{
				list.Add(recipe);
				continue;
			}
			if (recipe.IsTracked)
			{
				list.Add(recipe);
				continue;
			}
			ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
			if (forId == null)
			{
				continue;
			}
			string[] array = (forId.IsBlock() ? Block.list[forId.Id].GroupNames : forId.Groups);
			if (array == null)
			{
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != null && array[j].EqualsCaseInsensitive(_category))
				{
					list.Add(recipe);
					break;
				}
			}
		}
		recipeList = list;
	}

	public static List<Recipe> FilterRecipesByWorkstation(string _workstation, IList<Recipe> recipeList)
	{
		if (_workstation == null)
		{
			_workstation = "";
		}
		List<Recipe> list = new List<Recipe>();
		if (_workstation != "")
		{
			if (!WorkstationCrafting)
			{
				return list;
			}
		}
		else if (BackpackCrafting == BackpackCraftingOptions.Disabled)
		{
			return list;
		}
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (recipeList[i] == null)
			{
				continue;
			}
			Recipe recipe = recipeList[i];
			if ((!World.BiomeProgressionEnabled && recipe.tags.Test_AnySet(biomeprogressionTag)) || (ItemClass.MaxTechType < ItemClass.ItemTechTypes.T3 && recipe.GetOutputItemClass().ItemTechType > ItemClass.MaxTechType))
			{
				continue;
			}
			if (_workstation != "")
			{
				Block blockByName = Block.GetBlockByName(_workstation);
				if (blockByName != null && blockByName.Properties.Contains("Workstation", "CraftingAreaRecipes"))
				{
					string text = blockByName.Properties.GetString("Workstation", "CraftingAreaRecipes");
					string[] array = new string[1] { text };
					if (text.Contains(","))
					{
						array = text.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",")
							.Split(',');
					}
					bool flag = false;
					for (int j = 0; j < array.Length; j++)
					{
						if (recipe.craftingArea != null && recipe.craftingArea.EqualsCaseInsensitive(array[j]))
						{
							if (recipe.craftingArea == "forge")
							{
								if (DisableSmelter && recipe.tags.Test_AnySet(useSmelterTag))
								{
									flag = true;
									continue;
								}
								if (!DisableSmelter && recipe.tags.Test_AnySet(replaceSmelterTag))
								{
									flag = true;
									continue;
								}
							}
							list.Add(recipe);
							flag = true;
							break;
						}
						if ((recipe.craftingArea == null || recipe.craftingArea == "") && array[j].EqualsCaseInsensitive("player"))
						{
							list.Add(recipe);
							flag = true;
							break;
						}
					}
					if (!flag && recipe.craftingArea != null && recipe.craftingArea.EqualsCaseInsensitive(_workstation))
					{
						list.Add(recipe);
					}
				}
				else if (recipe.craftingArea != null && recipe.craftingArea.EqualsCaseInsensitive(_workstation))
				{
					list.Add(recipe);
				}
			}
			else if ((recipe.craftingArea == null || recipe.craftingArea == "") && FilterByBackpackSandboxSetting(recipe))
			{
				list.Add(recipe);
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByName(string _name, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			Recipe recipe = recipeList[i];
			if (recipe.craftingArea.EqualsCaseInsensitive("assembly") || (!World.BiomeProgressionEnabled && recipe.tags.Test_AnySet(biomeprogressionTag)))
			{
				continue;
			}
			ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
			if (forId == null || (ItemClass.MaxTechType < ItemClass.ItemTechTypes.T3 && forId.ItemTechType > ItemClass.MaxTechType))
			{
				continue;
			}
			if (Localization.TryGet(recipe.GetName(), out var _localizedString) && _localizedString.ContainsCaseInsensitive(_name))
			{
				list.Add(recipe);
				continue;
			}
			if (!forId.IsBlock())
			{
				if (forId.GetItemName().ContainsCaseInsensitive(_name))
				{
					list.Add(recipe);
				}
				else if (forId.GetLocalizedItemName().ContainsCaseInsensitive(_name))
				{
					list.Add(recipe);
				}
				continue;
			}
			Block block = Block.list[forId.Id];
			if (block != null)
			{
				if (block.GetBlockName().ContainsCaseInsensitive(_name))
				{
					list.Add(recipe);
				}
				else if (block.GetLocalizedBlockName().ContainsCaseInsensitive(_name))
				{
					list.Add(recipe);
				}
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByIngredient(ItemStack stack, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		ItemValue[] items = new ItemValue[1] { stack.itemValue };
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (recipeList[i].ContainsIngredients(items))
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByItem(List<int> itemIDs, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (itemIDs.Contains(recipeList[i].itemValueType))
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByID(int itemID, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (itemID == recipeList[i].itemValueType)
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static bool FilterByBackpackSandboxSetting(Recipe recipe)
	{
		if ((BackpackCrafting == BackpackCraftingOptions.Disabled && recipe.craftingArea == "") || (BackpackCrafting == BackpackCraftingOptions.WorkbenchOnly && !recipe.tags.Test_AnySet(workbenchOnlyTag)))
		{
			return false;
		}
		if (BackpackCrafting == BackpackCraftingOptions.BasicsOnly)
		{
			ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
			if (forId == null)
			{
				return false;
			}
			string[] array = (forId.IsBlock() ? Block.list[forId.Id].GroupNames : forId.Groups);
			if (array == null)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].EqualsCaseInsensitive("limited"))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static int GetCount(XUi xui)
	{
		return CraftingManager.GetRecipes().Count;
	}

	public static string GetRecipeName(Recipe _recipe)
	{
		return _recipe.GetName();
	}

	public static string GetRecipeSpriteName(Recipe _recipe)
	{
		return _recipe.GetIcon();
	}

	public static bool GetRecipeIsUnlocked(XUi xui, Recipe _recipe)
	{
		return _recipe.IsUnlocked(xui.playerUI.entityPlayer);
	}

	public static bool GetRecipeIsUnlocked(XUi xui, string itemName)
	{
		Recipe recipe = CraftingManager.GetRecipe(itemName);
		if (recipe == null)
		{
			return false;
		}
		return GetRecipeIsUnlocked(xui, recipe);
	}

	public static bool GetRecipeIsUnlockable(XUi xui, Recipe _recipe)
	{
		return _recipe.IsLearnable;
	}

	public static bool GetRecipeIsFavorite(XUi xui, Recipe _recipe)
	{
		return CraftingManager.RecipeIsFavorite(_recipe);
	}

	public static float GetRecipeCraftTime(XUi xui, Recipe _recipe)
	{
		float num = EffectManager.GetValue(PassiveEffects.CraftingTime, null, _recipe.craftingTime, xui.playerUI.entityPlayer, _recipe, _recipe.tags) * CraftingTimeModifier;
		if (num < 0f)
		{
			return 0f;
		}
		return num;
	}

	public static int GetRecipeCraftOutputCount(XUi xui, Recipe _recipe)
	{
		float num = CraftingOutputModifier;
		if (_recipe.tags.Test_AnySet(SandboxIgnoreTag))
		{
			num = 1f;
		}
		return (int)Mathf.Max(EffectManager.GetValue(PassiveEffects.CraftingOutputCount, null, _recipe.count, xui.playerUI.entityPlayer, _recipe, _recipe.tags) * num, 1f);
	}

	public static ItemStack GetRecipeOutput(Recipe _recipe)
	{
		return new ItemStack(new ItemValue(_recipe.itemValueType), _recipe.count);
	}

	public static List<ItemStack> GetRecipeIngredients(Recipe _recipe)
	{
		return _recipe.GetIngredientsSummedUp();
	}

	public static void GetCurrentSlots()
	{
	}

	public static bool HasIngredientsForRecipe(IList<ItemStack> allItems, Recipe _recipe, EntityAlive _ea = null)
	{
		return _recipe.CanCraftAny(allItems, _ea);
	}

	public static float GetCraftingInputModifier(Recipe recipe)
	{
		if (recipe.tags.Test_AnySet(SandboxIgnoreTag))
		{
			return 1f;
		}
		return CraftingInputModifier;
	}
}
