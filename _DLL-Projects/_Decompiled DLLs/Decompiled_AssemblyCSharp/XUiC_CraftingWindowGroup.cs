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
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("crafting");
		base.xui.currentWorkstation = "";
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.currentWorkstation = "";
	}

	public virtual bool CraftingRequirementsValid(Recipe _recipe)
	{
		return true;
	}

	public virtual string CraftingRequirementsInvalidMessage(Recipe _recipe)
	{
		return "";
	}

	public override bool AlwaysUpdate()
	{
		return true;
	}
}
