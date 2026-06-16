using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable localTileEntity;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector2i GridCellSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override ItemStack[] GetSlots()
	{
		return localTileEntity.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] _stackList)
	{
	}

	public void SetSlots(ITileEntityLootable _lootContainer, ItemStack[] _stackList)
	{
		if (_stackList == null)
		{
			return;
		}
		localTileEntity = _lootContainer;
		items = localTileEntity.items;
		XUiC_ItemInfoWindow childByType = xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiV_Grid obj = (XUiV_Grid)viewComponent;
		obj.Columns = _lootContainer.GetContainerSize().x;
		obj.Rows = _lootContainer.GetContainerSize().y;
		int num = _stackList.Length;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			xUiC_ItemStack.InfoWindow = childByType;
			xUiC_ItemStack.SlotNumber = i;
			xUiC_ItemStack.SlotChangedEvent -= HandleLootSlotChangedEvent;
			xUiC_ItemStack.InfoWindow = childByType;
			xUiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				xUiC_ItemStack.ForceSetItemStack(localTileEntity.items[i].Clone());
				itemControllers[i].ViewComponent.IsVisible = true;
				xUiC_ItemStack.SlotChangedEvent += HandleLootSlotChangedEvent;
			}
			else
			{
				xUiC_ItemStack.ItemStack = ItemStack.Empty;
				itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xUiV_Grid = (XUiV_Grid)viewComponent;
		GridCellSize = new Vector2i(xUiV_Grid.CellWidth, xUiV_Grid.CellHeight);
	}

	public void HandleLootSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		localTileEntity.UpdateSlot(_slotNumber, _stack);
		localTileEntity.SetModified();
	}

	public void OnTileEntityChanged(ITileEntity _te)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			XUiC_ItemStack obj = itemControllers[i];
			obj.SlotChangedEvent -= HandleLootSlotChangedEvent;
			obj.ItemStack = slots[i];
			obj.SlotChangedEvent += HandleLootSlotChangedEvent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!localTileEntity.listeners.Contains(this))
		{
			localTileEntity.listeners.Add(this);
		}
		xui.LootContainer = localTileEntity;
		localTileEntity.Destroyed += LocalTileEntity_Destroyed;
		QuestEventManager.Current.OpenedContainer(localTileEntity.ToWorldPos(), localTileEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity _te)
	{
		if (GameManager.Instance != null)
		{
			if (_te == localTileEntity)
			{
				xui.playerUI.windowManager.Close(XUiC_LootWindowGroup.ID);
			}
			else
			{
				_te.Destroyed -= LocalTileEntity_Destroyed;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.LootContainer = null;
		if (localTileEntity != null)
		{
			localTileEntity.Destroyed -= LocalTileEntity_Destroyed;
			if (localTileEntity.listeners.Contains(this))
			{
				localTileEntity.listeners.Remove(this);
			}
			QuestEventManager.Current.ClosedContainer(localTileEntity.ToWorldPos(), localTileEntity);
			localTileEntity = null;
		}
	}
}
