using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemStackGrid : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ItemStack[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiEvent_SlotChangedEventHandler handleSlotChangedDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public override void Init()
	{
		base.Init();
		itemControllers = GetChildrenByType<XUiC_ItemStack>();
		bAwakeCalled = true;
		IsDirty = false;
		IsDormant = true;
		handleSlotChangedDelegate = HandleSlotChangedEvent;
	}

	public XUiC_ItemStack[] GetItemStackControllers()
	{
		return itemControllers;
	}

	public virtual ItemStack[] GetSlots()
	{
		return getUISlots();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemStack[] getUISlots()
	{
		ItemStack[] array = new ItemStack[itemControllers.Length];
		for (int i = 0; i < itemControllers.Length; i++)
		{
			array[i] = itemControllers[i].ItemStack.Clone();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemStack[] stackList)
	{
		if (stackList != null)
		{
			XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
			for (int i = 0; i < stackList.Length && itemControllers.Length > i && stackList.Length > i; i++)
			{
				XUiC_ItemStack obj = itemControllers[i];
				obj.SlotChangedEvent -= handleSlotChangedDelegate;
				obj.ItemStack = stackList[i].Clone();
				obj.SlotChangedEvent += handleSlotChangedDelegate;
				obj.SlotNumber = i;
				obj.InfoWindow = childByType;
				obj.StackLocation = StackLocation;
			}
		}
	}

	public void AssembleLockSingleStack(ItemStack stack)
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			if (xUiC_ItemStack.ItemStack.itemValue.Equals(stack.itemValue))
			{
				base.xui.AssembleItem.CurrentItemStackController = xUiC_ItemStack;
				break;
			}
		}
	}

	public virtual void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (items != null)
		{
			items[slotNumber] = stack.Clone();
		}
		UpdateBackend(getUISlots());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBackend(ItemStack[] stackList)
	{
	}

	public virtual void ClearHoveredItems()
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			itemControllers[i].Hovered(_isOver: false);
		}
	}

	public int FindFirstEmptySlot()
	{
		int result = -1;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i].ViewComponent.UiTransform.gameObject.activeInHierarchy && itemControllers[i].ItemStack.Equals(ItemStack.Empty))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
		IsDormant = false;
		base.xui.playerUI.RegisterItemStackGrid(this);
	}

	public override void OnClose()
	{
		base.OnClose();
		ClearHoveredItems();
		IsDormant = true;
		base.xui.playerUI.UnregisterItemStackGrid(this);
	}
}
