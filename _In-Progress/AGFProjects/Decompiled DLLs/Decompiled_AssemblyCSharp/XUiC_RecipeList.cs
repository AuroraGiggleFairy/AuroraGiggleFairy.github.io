using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct RecipeInfo
	{
		public Recipe recipe;

		public bool unlocked;

		public bool hasIngredients;

		public string name;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<RecipeInfo> recipeInfos = new List<RecipeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry[] recipeControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string workStation = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Recipe> recipes = new List<Recipe>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resortRecipes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageChanged;

	public string[] craftingArea = new string[1] { "" };

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> updateStackList = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CraftingWindowGroup craftingWindow;

	public string Workstation
	{
		get
		{
			return workStation;
		}
		set
		{
			workStation = value;
			Block blockByName = Block.GetBlockByName(workStation);
			if (blockByName != null && blockByName.Properties.Values.ContainsKey("Workstation.CraftingAreaRecipes"))
			{
				string text = blockByName.Properties.Values["Workstation.CraftingAreaRecipes"];
				craftingArea = new string[1] { text };
				if (text.Contains(","))
				{
					craftingArea = text.Replace("player", "").Replace(", ", ",").Replace(" ,", ",")
						.Replace(" , ", ",")
						.Split(',');
				}
			}
			else
			{
				craftingArea = new string[1] { workStation };
			}
			GetRecipeData();
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Recipe CurrentRecipe
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount CraftCount { get; set; }

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			if (page != value)
			{
				page = value;
				pager?.SetPage(page);
				if (this.PageNumberChanged != null)
				{
					this.PageNumberChanged(page);
				}
				IsDirty = true;
				pageChanged = true;
				CurrentRecipe = null;
			}
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CraftingInfoWindow InfoWindow { get; set; }

	public XUiC_RecipeEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				InfoWindow.ViewComponent.IsVisible = true;
				this.RecipeChanged(selectedEntry.Recipe, selectedEntry);
				InfoWindow.SetRecipe(selectedEntry);
				CurrentRecipe = selectedEntry.Recipe;
			}
			else
			{
				InfoWindow.SetRecipe(null);
			}
			IsDirty = true;
			pageChanged = true;
		}
	}

	public event XUiEvent_RecipeChangedEventHandler RecipeChanged;

	public event XUiEvent_PageNumberChangedEventHandler PageNumberChanged;

	public override void Init()
	{
		base.Init();
		windowGroup.Controller.GetChildByType<XUiC_CategoryList>().CategoryChanged += HandleCategoryChanged;
		pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
		recipeControls = GetChildrenByType<XUiC_RecipeEntry>();
		for (int num = 0; num < recipeControls.Length; num++)
		{
			XUiC_RecipeEntry obj = recipeControls[num];
			obj.OnScroll += HandleOnScroll;
			obj.OnPress += OnPressRecipe;
			obj.RecipeList = this;
		}
		parent.OnScroll += HandleOnScroll;
		XUiController childById = base.Parent.GetChildById("favorites");
		childById.OnPress += HandleFavoritesChanged;
		favorites = (XUiV_Button)childById.ViewComponent;
		XUiV_Grid xUiV_Grid = (XUiV_Grid)base.ViewComponent;
		if (xUiV_Grid != null)
		{
			length = xUiV_Grid.Columns * xUiV_Grid.Rows;
		}
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangeHandler;
			txtInput.OnSubmitHandler += HandleOnSubmitHandler;
		}
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		showFavorites = !showFavorites;
		favorites.Selected = showFavorites;
		GetRecipeData();
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnSubmitHandler(XUiController _sender, string _text)
	{
		GetRecipeData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GetRecipeData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressRecipe(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_RecipeEntry xUiC_RecipeEntry && this.RecipeChanged != null)
		{
			SelectedEntry = xUiC_RecipeEntry;
			if (InputUtils.ShiftKeyPressed)
			{
				CraftCount.SetToMaxCount();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		SetCategory(_categoryEntry.CategoryName);
		IsDirty = true;
	}

	public void SetCategory(string _category)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		category = _category;
		GetRecipeData();
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	public string GetCategory()
	{
		return category;
	}

	public void RefreshRecipes()
	{
		GetRecipeData();
	}

	public void RefreshCurrentRecipes()
	{
		IsDirty = true;
		pageChanged = true;
		if (showFavorites)
		{
			CraftingManager.GetFavoriteRecipesFromList(ref recipes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetRecipeData()
	{
		ReadOnlyCollection<Recipe> readOnlyCollection = XUiM_Recipes.GetRecipes();
		List<string> questRecipes = base.xui.playerUI.entityPlayer.QuestJournal.GetQuestRecipes();
		List<Recipe> list = ((base.xui.QuestTracker.TrackedChallenge != null) ? base.xui.QuestTracker.TrackedChallenge.CraftedRecipes() : null);
		if (questRecipes.Count > 0 || (list != null && list.Count > 0))
		{
			for (int i = 0; i < readOnlyCollection.Count; i++)
			{
				if (list != null && list.Contains(readOnlyCollection[i]))
				{
					readOnlyCollection[i].isChallenge = true;
					readOnlyCollection[i].isQuest = false;
				}
				else if (questRecipes.Contains(readOnlyCollection[i].GetName()))
				{
					readOnlyCollection[i].isQuest = true;
					readOnlyCollection[i].isChallenge = false;
				}
				else
				{
					readOnlyCollection[i].isQuest = false;
					readOnlyCollection[i].isChallenge = false;
				}
			}
		}
		else
		{
			for (int j = 0; j < readOnlyCollection.Count; j++)
			{
				readOnlyCollection[j].isQuest = false;
				readOnlyCollection[j].isChallenge = false;
			}
		}
		if (txtInput != null && txtInput.Text.Length > 0)
		{
			recipes = XUiM_Recipes.FilterRecipesByName(txtInput.Text, XUiM_Recipes.GetRecipes());
		}
		else
		{
			recipes = XUiM_Recipes.FilterRecipesByWorkstation(workStation, readOnlyCollection);
			if (showFavorites)
			{
				CraftingManager.GetFavoriteRecipesFromList(ref recipes);
			}
			else if (category != "")
			{
				XUiM_Recipes.FilterRecipesByCategory(category, ref recipes);
			}
		}
		Page = 0;
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	public void SetRecipeDataByIngredientStack(ItemStack stack)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		CurrentRecipe = null;
		recipes = XUiM_Recipes.FilterRecipesByIngredient(stack, XUiM_Recipes.GetRecipes());
		Page = 0;
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	public void SetRecipeDataByItems(List<int> items)
	{
		if (items.Count != 0)
		{
			if (txtInput != null)
			{
				txtInput.Text = "";
			}
			CurrentRecipe = null;
			recipes = XUiM_Recipes.FilterRecipesByItem(items, XUiM_Recipes.GetRecipes());
			Page = 0;
			IsDirty = true;
			resortRecipes = true;
			pageChanged = true;
		}
	}

	public void SetRecipeDataByItem(int itemID)
	{
		if (txtInput != null)
		{
			txtInput.Text = "";
		}
		CurrentRecipe = null;
		recipes = XUiM_Recipes.FilterRecipesByID(itemID, XUiM_Recipes.GetRecipes());
		if (recipes != null && recipes.Count > 0)
		{
			CurrentRecipe = recipes[0];
		}
		Page = 0;
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	public override void Update(float _dt)
	{
		if (IsDirty && base.xui.PlayerInventory != null)
		{
			FindShowingWindow();
			if (resortRecipes)
			{
				List<ItemStack> list = updateStackList;
				list.Clear();
				list.AddRange(base.xui.PlayerInventory.GetBackpackItemStacks());
				list.AddRange(base.xui.PlayerInventory.GetToolbeltItemStacks());
				XUiC_WorkstationInputGrid childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
				if (childByType != null)
				{
					list.Clear();
					list.AddRange(childByType.GetSlots());
				}
				BuildRecipeInfosList(list);
				recipeInfos.Sort(CompareRecipeInfos);
				UpdateRecipes();
				resortRecipes = false;
			}
			if (pageChanged)
			{
				for (int i = 0; i < length; i++)
				{
					int num = i + length * page;
					XUiC_RecipeEntry xUiC_RecipeEntry = ((i < recipeControls.Length) ? recipeControls[i] : null);
					if (xUiC_RecipeEntry == null)
					{
						continue;
					}
					if (num < recipeInfos.Count)
					{
						RecipeInfo recipeInfo = recipeInfos[num];
						xUiC_RecipeEntry.SetRecipeAndHasIngredients(recipeInfo.recipe, recipeInfo.hasIngredients);
						xUiC_RecipeEntry.ViewComponent.Enabled = true;
					}
					else
					{
						xUiC_RecipeEntry.SetRecipeAndHasIngredients(null, hasIngredients: false);
						xUiC_RecipeEntry.ViewComponent.Enabled = false;
						if (xUiC_RecipeEntry.Selected)
						{
							xUiC_RecipeEntry.Selected = false;
						}
					}
					if (CurrentRecipe != null && CurrentRecipe == xUiC_RecipeEntry.Recipe && SelectedEntry != xUiC_RecipeEntry)
					{
						SelectedEntry = xUiC_RecipeEntry;
						CraftCount.IsDirty = true;
					}
					if (SelectedEntry != null && SelectedEntry.Recipe != CurrentRecipe)
					{
						ClearSelection();
					}
				}
				pageChanged = false;
			}
			if (pager != null)
			{
				pager.SetLastPageByElementsAndPageLength(recipeInfos.Count, recipeControls.Length);
				pager.CurrentPageNumber = page;
			}
			IsDirty = false;
		}
		base.Update(_dt);
		if (base.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed && base.xui.playerUI.CursorController.navigationTarget != null)
		{
			OnPressRecipe(base.xui.playerUI.CursorController.navigationTarget.Controller, 0);
		}
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		if (base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
		}
		base.xui.playerUI.entityPlayer.QuestChanged += QuestJournal_QuestChanged;
		base.xui.playerUI.entityPlayer.QuestRemoved += QuestJournal_QuestChanged;
		base.xui.QuestTracker.OnTrackedChallengeChanged += QuestTracker_OnTrackedChallengeChanged;
		base.xui.Recipes.OnTrackedRecipeChanged += Recipes_OnTrackedRecipeChanged;
		XUiC_WorkstationMaterialInputWindow childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
		if (childByType != null)
		{
			childByType.OnWorkstationMaterialWeightsChanged += WorkstationMaterial_OnWeightsChanged;
		}
		XUiC_WorkstationFuelGrid childByType2 = windowGroup.Controller.GetChildByType<XUiC_WorkstationFuelGrid>();
		if (childByType2 != null)
		{
			childByType2.OnWorkstationFuelChanged += WorkStation_OnToolsOrFuelChanged;
		}
		XUiC_WorkstationToolGrid childByType3 = windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType3 != null)
		{
			childByType3.OnWorkstationToolsChanged += WorkStation_OnToolsOrFuelChanged;
		}
		ClearSelection();
		if (base.xui.playerUI.entityPlayer.QuestJournal.HasCraftingQuest() && (txtInput == null || txtInput.Text == ""))
		{
			GetRecipeData();
			pageChanged = true;
		}
		if (base.xui.QuestTracker.TrackedChallenge != null)
		{
			GetRecipeData();
			pageChanged = true;
		}
		IsDirty = true;
		resortRecipes = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
		base.xui.playerUI.entityPlayer.QuestChanged -= QuestJournal_QuestChanged;
		base.xui.playerUI.entityPlayer.QuestRemoved -= QuestJournal_QuestChanged;
		base.xui.QuestTracker.OnTrackedChallengeChanged -= QuestTracker_OnTrackedChallengeChanged;
		base.xui.Recipes.OnTrackedRecipeChanged -= Recipes_OnTrackedRecipeChanged;
		XUiC_WorkstationMaterialInputWindow childByType = windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
		if (childByType != null)
		{
			childByType.OnWorkstationMaterialWeightsChanged -= WorkstationMaterial_OnWeightsChanged;
		}
		XUiC_WorkstationFuelGrid childByType2 = windowGroup.Controller.GetChildByType<XUiC_WorkstationFuelGrid>();
		if (childByType2 != null)
		{
			childByType2.OnWorkstationFuelChanged -= WorkStation_OnToolsOrFuelChanged;
		}
		XUiC_WorkstationToolGrid childByType3 = windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType3 != null)
		{
			childByType3.OnWorkstationToolsChanged -= WorkStation_OnToolsOrFuelChanged;
		}
		SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorkStation_OnToolsOrFuelChanged()
	{
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		GetRecipeData();
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedChallengeChanged()
	{
		GetRecipeData();
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorkstationMaterial_OnWeightsChanged()
	{
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Recipes_OnTrackedRecipeChanged()
	{
		GetRecipeData();
		IsDirty = true;
		resortRecipes = true;
		pageChanged = true;
	}

	public void ClearSelection()
	{
		SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildRecipeInfosList(List<ItemStack> _items)
	{
		recipeInfos.Clear();
		RecipeInfo item = default(RecipeInfo);
		for (int i = 0; i < recipes.Count; i++)
		{
			item.recipe = recipes[i];
			item.unlocked = XUiM_Recipes.GetRecipeIsUnlocked(base.xui, item.recipe);
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			bool flag = XUiM_Recipes.HasIngredientsForRecipe(_items, item.recipe, entityPlayer);
			item.hasIngredients = flag && (craftingWindow == null || craftingWindow.CraftingRequirementsValid(item.recipe));
			item.name = Localization.Get(item.recipe.GetName());
			recipeInfos.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRecipes()
	{
		recipes.Clear();
		for (int i = 0; i < recipeInfos.Count; i++)
		{
			recipes.Add(recipeInfos[i].recipe);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CompareRecipeInfos(RecipeInfo lhs, RecipeInfo rhs)
	{
		if (lhs.recipe.IsTracked != rhs.recipe.IsTracked)
		{
			if (!lhs.recipe.IsTracked)
			{
				return 1;
			}
			return -1;
		}
		if (lhs.recipe.isChallenge != rhs.recipe.isChallenge)
		{
			if (!lhs.recipe.isChallenge)
			{
				return 1;
			}
			return -1;
		}
		if (lhs.recipe.isQuest != rhs.recipe.isQuest)
		{
			if (!lhs.recipe.isQuest)
			{
				return 1;
			}
			return -1;
		}
		if (lhs.unlocked != rhs.unlocked)
		{
			if (!lhs.unlocked)
			{
				return 1;
			}
			return -1;
		}
		if (lhs.hasIngredients != rhs.hasIngredients)
		{
			if (!lhs.hasIngredients)
			{
				return 1;
			}
			return -1;
		}
		if (lhs.name == rhs.name)
		{
			if (lhs.recipe.count > rhs.recipe.count)
			{
				return 1;
			}
			if (lhs.recipe.count < rhs.recipe.count)
			{
				return -1;
			}
			if (lhs.recipe.itemValueType > rhs.recipe.itemValueType)
			{
				return 1;
			}
			if (lhs.recipe.itemValueType < rhs.recipe.itemValueType)
			{
				return -1;
			}
			return CompareRecipeIngredients(lhs.recipe.ingredients, rhs.recipe.ingredients);
		}
		return string.Compare(lhs.name, rhs.name, StringComparison.Ordinal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CompareRecipeIngredients(List<ItemStack> lhs, List<ItemStack> rhs)
	{
		if (lhs.Count > rhs.Count)
		{
			return 1;
		}
		if (lhs.Count < rhs.Count)
		{
			return -1;
		}
		for (int i = 0; i < lhs.Count; i++)
		{
			int itemId = lhs[i].itemValue.GetItemId();
			int itemId2 = rhs[i].itemValue.GetItemId();
			if (itemId > itemId2)
			{
				return 1;
			}
			if (itemId < itemId2)
			{
				return -1;
			}
			if (lhs[i].count > rhs[i].count)
			{
				return 1;
			}
			if (lhs[i].count < rhs[i].count)
			{
				return -1;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindShowingWindow()
	{
		List<XUiC_CraftingWindowGroup> childrenByType = base.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				craftingWindow = childrenByType[i];
				break;
			}
		}
	}
}
