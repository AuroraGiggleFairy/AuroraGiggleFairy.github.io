using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationToolGrid : XUiC_WorkstationGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredTools = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredToolsOnly;

	public event XuiEvent_WorkstationItemsChanged OnWorkstationToolsChanged;

	public override void Init()
	{
		base.Init();
		string[] array = requiredTools.Split(',');
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (i < array.Length)
			{
				((XUiC_RequiredItemStack)itemControllers[i]).SetAllowedItemClassSingle(array[i]);
				((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemOnly = requiredToolsOnly;
			}
			else
			{
				((XUiC_RequiredItemStack)itemControllers[i]).ClearAllowedItemClasses();
				((XUiC_RequiredItemStack)itemControllers[i]).RequiredItemOnly = false;
			}
		}
	}

	public override bool HasRequirement(Recipe recipe)
	{
		if (recipe == null)
		{
			return false;
		}
		if (recipe.craftingToolType == 0)
		{
			return true;
		}
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.type == recipe.craftingToolType)
			{
				return true;
			}
		}
		return false;
	}

	public void SetToolLocks(bool locked)
	{
		if (locked != isLocked)
		{
			isLocked = locked;
			for (int i = 0; i < itemControllers.Length; i++)
			{
				itemControllers[i].ToolLock = locked;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		workstationData.SetToolStacks(stackList);
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "required_tools"))
			{
				if (!(name == "required_tools_only"))
				{
					return false;
				}
				requiredToolsOnly = StringParsers.ParseBool(value);
			}
			else
			{
				requiredTools = value;
			}
			return true;
		}
		return flag;
	}

	public bool TryAddTool(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!requiredToolsOnly || isLocked)
		{
			return false;
		}
		XUiC_ItemStack[] array = itemControllers;
		for (int i = 0; i < array.Length; i++)
		{
			((XUiC_RequiredItemStack)array[i]).TryStack(newItemStack);
			if (newItemStack.count == 0)
			{
				return true;
			}
		}
		return newItemStack.count == 0;
	}

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		base.HandleSlotChangedEvent(slotNumber, stack);
		if (this.OnWorkstationToolsChanged != null)
		{
			this.OnWorkstationToolsChanged();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.CurrentWorkstationToolGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.CurrentWorkstationToolGrid = null;
	}
}
