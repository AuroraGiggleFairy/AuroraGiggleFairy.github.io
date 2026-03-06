using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class CraftingManager
{
	public delegate void OnRecipeUnlocked(string recipeName);

	[Flags]
	public enum RecipeLockTypes
	{
		None = 0,
		Item = 1,
		Skill = 2,
		Quest = 4
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Recipe> recipes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool bSorted;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryKeyList<string, int> lockedRecipeNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<RecipeLockTypes> lockedRecipeTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, WorkstationData> craftingAreaData;

	public static HashSet<string> UnlockedRecipeList;

	public static HashSet<string> FavoriteRecipeList;

	public static HashSet<string> AlreadyCraftedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Recipe> nonScrapableRecipes;

	public static readonly ReadOnlyCollection<Recipe> NonScrapableRecipes;

	public static event OnRecipeUnlocked RecipeUnlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	static CraftingManager()
	{
		recipes = new List<Recipe>();
		lockedRecipeNames = new DictionaryKeyList<string, int>();
		lockedRecipeTypes = new List<RecipeLockTypes>();
		craftingAreaData = new CaseInsensitiveStringDictionary<WorkstationData>();
		UnlockedRecipeList = new HashSet<string>();
		FavoriteRecipeList = new HashSet<string>();
		AlreadyCraftedList = new HashSet<string>();
		nonScrapableRecipes = new List<Recipe>();
		NonScrapableRecipes = nonScrapableRecipes.AsReadOnly();
	}

	public static void InitForNewGame()
	{
		ClearAllRecipes();
		UnlockedRecipeList.Clear();
		AlreadyCraftedList.Clear();
		FavoriteRecipeList.Clear();
		craftingAreaData.Clear();
	}

	public static void PostInit()
	{
		cacheNonScrapableRecipes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void cacheNonScrapableRecipes()
	{
		nonScrapableRecipes.Clear();
		for (int i = 0; i < recipes.Count; i++)
		{
			if (!recipes[i].wildcardForgeCategory)
			{
				nonScrapableRecipes.Add(recipes[i]);
			}
		}
	}

	public static void ClearAllRecipes()
	{
		recipes.Clear();
	}

	public static void ClearLockedData()
	{
		lockedRecipeNames.Clear();
		lockedRecipeTypes.Clear();
	}

	public static void ClearAllGeneralRecipes()
	{
		List<Recipe> list = new List<Recipe>(recipes);
		for (int i = 0; i < list.Count; i++)
		{
			Recipe recipe = list[i];
			if (recipe.craftingArea == null || recipe.craftingArea.Length == 0)
			{
				recipes.Remove(recipe);
			}
		}
	}

	public static void ClearRecipe(Recipe _r)
	{
		recipes.Remove(_r);
	}

	public static void ClearCraftAreaRecipes(string _craftArea, ItemValue _craftTool)
	{
		List<Recipe> list = new List<Recipe>(recipes);
		for (int i = 0; i < list.Count; i++)
		{
			Recipe recipe = list[i];
			if (recipe.craftingArea != null && recipe.craftingArea.Equals(_craftArea) && recipe.craftingToolType == _craftTool.type)
			{
				recipes.Remove(recipe);
			}
		}
	}

	public static void AddRecipe(Recipe _recipe)
	{
		recipes.Add(_recipe);
		bSorted = false;
	}

	public static bool RecipeIsFavorite(Recipe _recipe)
	{
		return FavoriteRecipeList.Contains(_recipe.GetName());
	}

	public static void LockRecipe(string _recipeName, RecipeLockTypes locktype = RecipeLockTypes.Item)
	{
		for (int i = 0; i < lockedRecipeNames.list.Count; i++)
		{
			if (lockedRecipeNames.list[i].EqualsCaseInsensitive(_recipeName))
			{
				lockedRecipeTypes[i] |= locktype;
				return;
			}
		}
		lockedRecipeNames.Add(_recipeName, 0);
		lockedRecipeTypes.Add(locktype);
	}

	public static void UnlockRecipe(Recipe _recipe, EntityPlayer _entity)
	{
		UnlockedRecipeList.Add(_recipe.GetName());
		if (CraftingManager.RecipeUnlocked != null)
		{
			CraftingManager.RecipeUnlocked(_recipe.GetName());
		}
		if (_entity != null)
		{
			_entity.SetCVar(_recipe.GetName(), 1f);
		}
	}

	public static void UnlockRecipe(string _recipeName, EntityPlayer _entity)
	{
		UnlockedRecipeList.Add(_recipeName);
		if (CraftingManager.RecipeUnlocked != null)
		{
			CraftingManager.RecipeUnlocked(_recipeName);
		}
		if (_entity != null)
		{
			_entity.SetCVar(_recipeName, 1f);
		}
	}

	public static void ToggleFavoriteRecipe(Recipe _recipe)
	{
		string name = _recipe.GetName();
		if (FavoriteRecipeList.Contains(name))
		{
			FavoriteRecipeList.Remove(name);
		}
		else
		{
			FavoriteRecipeList.Add(name);
		}
	}

	public static int GetLockedRecipeCount()
	{
		return lockedRecipeNames.list.Count;
	}

	public static int GetUnlockedRecipeCount()
	{
		return UnlockedRecipeList.Count;
	}

	public static List<Recipe> GetRecipes()
	{
		return new List<Recipe>(recipes);
	}

	public static Recipe GetRecipe(int hashCode)
	{
		for (int i = 0; i < recipes.Count; i++)
		{
			if (recipes[i].GetHashCode() == hashCode)
			{
				return recipes[i];
			}
		}
		return null;
	}

	public static Recipe GetRecipe(string _itemName)
	{
		for (int i = 0; i < recipes.Count; i++)
		{
			if (recipes[i].GetName() == _itemName)
			{
				return recipes[i];
			}
		}
		return null;
	}

	public static List<Recipe> GetRecipes(string _itemName)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipes.Count; i++)
		{
			if (_itemName == recipes[i].GetName())
			{
				list.Add(recipes[i]);
			}
		}
		return list;
	}

	public static List<Recipe> GetAllRecipes()
	{
		return recipes;
	}

	public static List<Recipe> GetNonScrapableRecipes(string _itemName)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < nonScrapableRecipes.Count; i++)
		{
			Recipe recipe = nonScrapableRecipes[i];
			if (recipe.GetName() == _itemName)
			{
				list.Add(recipe);
			}
		}
		return list;
	}

	public static List<Recipe> GetAllRecipes(string _itemName)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipes.Count; i++)
		{
			Recipe recipe = recipes[i];
			if (recipe.GetName() == _itemName)
			{
				list.Add(recipe);
			}
		}
		return list;
	}

	public static void GetFavoriteRecipesFromList(ref List<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			Recipe recipe = recipeList[i];
			if (FavoriteRecipeList.Contains(recipe.GetName()))
			{
				list.Add(recipe);
			}
		}
		recipeList = list;
	}

	public static Recipe GetScrapableRecipe(ItemValue _itemValue, int _count = 1)
	{
		MaterialBlock madeOfMaterial = _itemValue.ItemClass.MadeOfMaterial;
		if (madeOfMaterial == null || madeOfMaterial.ForgeCategory == null)
		{
			return null;
		}
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass == null)
		{
			return null;
		}
		if (itemClass.NoScrapping)
		{
			return null;
		}
		MaterialBlock materialBlock = null;
		for (int i = 0; i < recipes.Count; i++)
		{
			Recipe recipe = recipes[i];
			if (recipe.wildcardForgeCategory)
			{
				ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
				materialBlock = forId.MadeOfMaterial;
				if (materialBlock != null && materialBlock.ForgeCategory != null && recipe.itemValueType != _itemValue.type && materialBlock.ForgeCategory.Equals(madeOfMaterial.ForgeCategory) && itemClass.GetWeight() * _count >= forId.GetWeight())
				{
					return recipe;
				}
			}
		}
		return null;
	}

	public static void AddWorkstationData(WorkstationData workstationData)
	{
		if (craftingAreaData.ContainsKey(workstationData.WorkstationName))
		{
			craftingAreaData[workstationData.WorkstationName] = workstationData;
		}
		else
		{
			craftingAreaData.Add(workstationData.WorkstationName, workstationData);
		}
	}

	public static WorkstationData GetWorkstationData(string workstationName)
	{
		if (workstationName != null && craftingAreaData.ContainsKey(workstationName))
		{
			return craftingAreaData[workstationName];
		}
		return null;
	}
}
