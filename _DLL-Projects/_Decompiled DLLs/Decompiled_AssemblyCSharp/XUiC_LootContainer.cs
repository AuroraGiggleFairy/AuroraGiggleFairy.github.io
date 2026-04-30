using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable localTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

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
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public void SetSlots(ITileEntityLootable lootContainer, ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		localTileEntity = lootContainer;
		items = localTileEntity.items;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiV_Grid obj = (XUiV_Grid)viewComponent;
		obj.Columns = lootContainer.GetContainerSize().x;
		obj.Rows = lootContainer.GetContainerSize().y;
		int num = stackList.Length;
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
				xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
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

	public void HandleLootSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		localTileEntity.UpdateSlot(slotNumber, stack);
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
		base.xui.lootContainer = localTileEntity;
		localTileEntity.Destroyed += LocalTileEntity_Destroyed;
		QuestEventManager.Current.OpenedContainer(localTileEntity.EntityId, localTileEntity.ToWorldPos(), localTileEntity);
		blockValue = GameManager.Instance.World.GetBlock(localTileEntity.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity te)
	{
		if (GameManager.Instance != null)
		{
			if (te == localTileEntity)
			{
				base.xui.playerUI.windowManager.Close("looting");
			}
			else
			{
				te.Destroyed -= LocalTileEntity_Destroyed;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.lootContainer = null;
		if (localTileEntity != null)
		{
			localTileEntity.Destroyed -= LocalTileEntity_Destroyed;
			if (localTileEntity.listeners.Contains(this))
			{
				localTileEntity.listeners.Remove(this);
			}
			QuestEventManager.Current.ClosedContainer(localTileEntity.EntityId, localTileEntity.ToWorldPos(), localTileEntity);
			localTileEntity = null;
		}
	}
}
