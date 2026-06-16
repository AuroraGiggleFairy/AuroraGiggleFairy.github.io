using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string workstation = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool firstRun = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_RecipeList recipeList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CraftingQueue craftingQueue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CraftingInfoWindow craftInfoWindow;

	public string Workstation
	{
		get
		{
			return workstation;
		}
		set
		{
			workstation = value;
			if (recipeList != null)
			{
				recipeList.Workstation = workstation;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		recipeList = GetChildByType<XUiC_RecipeList>();
		craftingQueue = GetChildByType<XUiC_CraftingQueue>();
		craftCountControl = GetChildByType<XUiC_RecipeCraftCount>();
		categoryList = GetChildByType<XUiC_CategoryList>();
		craftInfoWindow = GetChildByType<XUiC_CraftingInfoWindow>();
		recipeList.InfoWindow = craftInfoWindow;
	}

	public virtual bool AddItemToQueue(Recipe _recipe)
	{
		if (craftCountControl != null)
		{
			return craftingQueue.AddRecipeToCraft(_recipe, craftCountControl.Count);
		}
		return craftingQueue.AddRecipeToCraft(_recipe);
	}

	public virtual bool AddItemToQueue(Recipe _recipe, int _count)
	{
		return craftingQueue.AddRecipeToCraft(_recipe, _count);
	}

	public virtual bool AddItemToQueue(Recipe _recipe, int _count, float _craftTime)
	{
		return craftingQueue.AddRecipeToCraft(_recipe, _count, _craftTime);
	}

	public virtual bool AddRepairItemToQueue(float _repairTime, ItemValue _itemToRepair, int _amountToRepair)
	{
		return craftingQueue.AddItemToRepair(_repairTime, _itemToRepair, _amountToRepair);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (recipeList != null && categoryList != null && categoryList.SetupCategoriesByWorkstation(""))
		{
			recipeList.Workstation = "";
			recipeList.SetCategory("Basics");
			categoryList.SetCategory("Basics");
		}
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("crafting");
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("windowpaging");
	}

	public virtual bool CraftingRequirementsValid(Recipe _recipe)
	{
		return true;
	}

	public virtual string CraftingRequirementsInvalidMessage(Recipe _recipe)
	{
		return "";
	}
}
