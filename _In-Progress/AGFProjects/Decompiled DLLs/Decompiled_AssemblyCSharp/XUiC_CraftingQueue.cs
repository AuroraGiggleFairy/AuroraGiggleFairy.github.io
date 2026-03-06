using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingQueue : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationToolGrid toolGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] queueItems;

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_RecipeStack>();
		queueItems = childrenByType;
		for (int i = 0; i < queueItems.Length; i++)
		{
			((XUiC_RecipeStack)queueItems[i]).Owner = this;
		}
	}

	public void ClearQueue()
	{
		for (int num = queueItems.Length - 1; num >= 0; num--)
		{
			XUiC_RecipeStack obj = (XUiC_RecipeStack)queueItems[num];
			obj.SetRecipe(null, 0, 0f, recipeModification: true);
			obj.IsCrafting = false;
			obj.IsDirty = true;
		}
	}

	public void HaltCrafting()
	{
		((XUiC_RecipeStack)queueItems[queueItems.Length - 1]).IsCrafting = false;
	}

	public void ResumeCrafting()
	{
		((XUiC_RecipeStack)queueItems[queueItems.Length - 1]).IsCrafting = true;
	}

	public bool IsCrafting()
	{
		return ((XUiC_RecipeStack)queueItems[queueItems.Length - 1]).IsCrafting;
	}

	public bool AddItemToRepair(float _repairTimeLeft, ItemValue _itemToRepair, int _amountToRepair)
	{
		for (int num = queueItems.Length - 1; num >= 0; num--)
		{
			XUiC_RecipeStack xUiC_RecipeStack = (XUiC_RecipeStack)queueItems[num];
			if (!xUiC_RecipeStack.HasRecipe() && xUiC_RecipeStack.SetRepairRecipe(_repairTimeLeft, _itemToRepair, _amountToRepair))
			{
				xUiC_RecipeStack.IsCrafting = num == queueItems.Length - 1;
				xUiC_RecipeStack.IsDirty = true;
				return true;
			}
		}
		return false;
	}

	public bool AddRecipeToCraft(Recipe _recipe, int _count = 1, float craftTime = -1f, bool isCrafting = true, float _oneItemCraftingTime = -1f)
	{
		for (int num = queueItems.Length - 1; num >= 0; num--)
		{
			if (AddRecipeToCraftAtIndex(num, _recipe, _count, craftTime, isCrafting, recipeModification: false, -1, -1, _oneItemCraftingTime))
			{
				return true;
			}
		}
		return false;
	}

	public bool AddRecipeToCraftAtIndex(int _index, Recipe _recipe, int _count = 1, float craftTime = -1f, bool isCrafting = true, bool recipeModification = false, int lastQuality = -1, int startingEntityId = -1, float _oneItemCraftingTime = -1f)
	{
		XUiC_RecipeStack xUiC_RecipeStack = (XUiC_RecipeStack)queueItems[_index];
		if (xUiC_RecipeStack.SetRecipe(_recipe, _count, craftTime, recipeModification, -1, -1, _oneItemCraftingTime))
		{
			xUiC_RecipeStack.IsCrafting = _index == queueItems.Length - 1 && isCrafting;
			if (lastQuality != -1)
			{
				xUiC_RecipeStack.OutputQuality = lastQuality;
			}
			if (startingEntityId != -1)
			{
				xUiC_RecipeStack.StartingEntityId = startingEntityId;
			}
			xUiC_RecipeStack.IsDirty = true;
			return true;
		}
		return false;
	}

	public bool AddItemToRepairAtIndex(int _index, float _repairTimeLeft, ItemValue _itemToRepair, int _amountToRepair, bool isCrafting = true, int startingEntityId = -1)
	{
		XUiC_RecipeStack xUiC_RecipeStack = (XUiC_RecipeStack)queueItems[_index];
		if (xUiC_RecipeStack.SetRepairRecipe(_repairTimeLeft, _itemToRepair, _amountToRepair))
		{
			xUiC_RecipeStack.IsCrafting = _index == queueItems.Length - 1;
			xUiC_RecipeStack.StartingEntityId = ((startingEntityId != -1) ? startingEntityId : xUiC_RecipeStack.StartingEntityId);
			xUiC_RecipeStack.IsDirty = true;
			return true;
		}
		return false;
	}

	public XUiC_RecipeStack[] GetRecipesToCraft()
	{
		XUiC_RecipeStack[] array = new XUiC_RecipeStack[queueItems.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (XUiC_RecipeStack)queueItems[i];
		}
		return array;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = false;
		for (int num = queueItems.Length - 1; num >= 0; num--)
		{
			if (((XUiC_RecipeStack)queueItems[num]).HasRecipe())
			{
				flag = true;
				break;
			}
		}
		if (toolGrid != null)
		{
			toolGrid.SetToolLocks(flag);
		}
		if (!flag)
		{
			return;
		}
		XUiC_RecipeStack xUiC_RecipeStack = (XUiC_RecipeStack)queueItems[queueItems.Length - 1];
		if (!xUiC_RecipeStack.HasRecipe())
		{
			for (int num2 = queueItems.Length - 1; num2 >= 0; num2--)
			{
				XUiC_RecipeStack recipeStack = (XUiC_RecipeStack)queueItems[num2];
				if (num2 != 0)
				{
					((XUiC_RecipeStack)queueItems[num2 - 1]).CopyTo(recipeStack);
				}
				else
				{
					((XUiC_RecipeStack)queueItems[0]).SetRecipe(null, 0, 0f, recipeModification: true);
				}
			}
		}
		if (xUiC_RecipeStack.HasRecipe() && !xUiC_RecipeStack.IsCrafting)
		{
			xUiC_RecipeStack.IsCrafting = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		toolGrid = windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
	}

	public void RefreshQueue()
	{
		XUiC_RecipeStack xUiC_RecipeStack = null;
		for (int num = queueItems.Length - 1; num >= 0; num--)
		{
			XUiC_RecipeStack xUiC_RecipeStack2 = (XUiC_RecipeStack)queueItems[num];
			if (xUiC_RecipeStack2.GetRecipe() != null && xUiC_RecipeStack != null && xUiC_RecipeStack.GetRecipe() == null)
			{
				xUiC_RecipeStack2.CopyTo(xUiC_RecipeStack);
				xUiC_RecipeStack2.SetRecipe(null, 0, 0f, recipeModification: true);
			}
			xUiC_RecipeStack = xUiC_RecipeStack2;
		}
	}
}
