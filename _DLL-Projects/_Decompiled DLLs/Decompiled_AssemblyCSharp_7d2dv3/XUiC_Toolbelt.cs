using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Toolbelt : XUiC_ItemStackGrid
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentHoldingIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastActionSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastActionRunning;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int backendSlotCount = -1;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.ToolBelt;
		}
	}

	public bool HasSecondRow
	{
		get
		{
			if (itemControllers != null)
			{
				return backendSlotCount > itemControllers.Length / 2;
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override ItemStack[] GetSlots()
	{
		return xui.PlayerInventory.GetToolbeltItemStacks();
	}

	public override void Update(float _dt)
	{
		if (!XUi.IsGameRunning())
		{
			return;
		}
		Inventory toolbelt = xui.PlayerInventory.Toolbelt;
		if (currentHoldingIndex != toolbelt.GetFocusedItemIdx())
		{
			if (currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				itemControllers[currentHoldingIndex].IsHolding = false;
			}
			currentHoldingIndex = toolbelt.GetFocusedItemIdx();
			if (currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				itemControllers[currentHoldingIndex].IsHolding = true;
			}
		}
		if (Time.time > updateTime)
		{
			updateTime = Time.time + 0.5f;
			bool flag = toolbelt.IsHoldingItemActionRunning();
			if (lastActionRunning != flag)
			{
				currentHoldingIndex = toolbelt.holdingItemIdx;
				if (currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
				{
					if (flag)
					{
						lastActionSlot = toolbelt.holdingItemIdx;
						itemControllers[toolbelt.holdingItemIdx].HiddenLock = true;
					}
					else
					{
						itemControllers[lastActionSlot].HiddenLock = flag;
					}
				}
				lastActionRunning = flag;
			}
			if (!GameManager.Instance.bCursorVisible)
			{
				ClearHoveredItems();
			}
		}
		UpdateQuickSwap();
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		xui.PlayerInventory.SetToolbeltItemStacks(stackList);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateQuickSwap()
	{
		int quickSwapSlot = xui.PlayerInventory.QuickSwapSlot;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			itemControllers[i].isQuickSwap = i == quickSwapSlot;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
		PlayerInventory_OnToolbeltItemsChanged();
		currentHoldingIndex = xui.PlayerInventory.Toolbelt.holdingItemIdx;
		if (currentHoldingIndex != xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			itemControllers[currentHoldingIndex].IsHolding = true;
		}
		xui.playerUI.windowManager.Open(xui.DragAndDropWindow.WindowGroup, _bModal: false);
		if (backendSlotCount < 0)
		{
			backendSlotCount = xui.PlayerInventory.Toolbelt.PUBLIC_SLOTS;
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		SetStacks(GetSlots());
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.PlayerInventory.OnToolbeltItemsChanged -= PlayerInventory_OnToolbeltItemsChanged;
		xui.playerUI.windowManager.Close(xui.DragAndDropWindow?.WindowGroup);
		if (currentHoldingIndex != xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			itemControllers[currentHoldingIndex].IsHolding = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public XUiC_ItemStack GetSlotControl(int slotIdx)
	{
		return itemControllers[slotIdx];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "secondrow")
		{
			_value = HasSecondRow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
