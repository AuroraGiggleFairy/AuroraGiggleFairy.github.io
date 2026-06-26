using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector localTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredItem = "";

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

	public void SetSlots(TileEntityDewCollector lootContainer, ItemStack[] stackList)
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
			xUiC_ItemStack.OverrideStackCount = 1;
			xUiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				SetItemInSlot(i, localTileEntity.items[i], onTEChanged: false);
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
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_DewCollectorStack obj = (XUiC_DewCollectorStack)itemControllers[i];
			obj.RequiredItemClass = ItemClass.GetItemClass(requiredItem);
			obj.RequiredItemOnly = true;
			obj.TakeOnly = true;
		}
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
			SetItemInSlot(i, slots[i], onTEChanged: true);
			obj.SlotChangedEvent += HandleLootSlotChangedEvent;
		}
	}

	public void SetItemInSlot(int i, ItemStack stack, bool onTEChanged)
	{
		if (onTEChanged && itemControllers[i].ItemStack.IsEmpty() && !stack.IsEmpty())
		{
			string convertSound = ((BlockDewCollector)localTileEntity.blockValue.Block).ConvertSound;
			Manager.BroadcastPlayByLocalPlayer(localTileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, convertSound);
		}
		itemControllers[i].ItemStack = stack.Clone();
		XUiC_DewCollectorStack obj = (XUiC_DewCollectorStack)itemControllers[i];
		obj.FillAmount = localTileEntity.fillValues[i];
		obj.MaxFill = localTileEntity.CurrentConvertTime;
		obj.IsCurrentStack = localTileEntity.CurrentIndex == i;
		obj.IsBlocked = localTileEntity.IsBlocked;
		obj.IsModded = localTileEntity.IsModdedConvertItem;
		obj.RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!localTileEntity.listeners.Contains(this))
		{
			localTileEntity.listeners.Add(this);
		}
		localTileEntity.Destroyed += LocalTileEntity_Destroyed;
		blockValue = GameManager.Instance.World.GetBlock(localTileEntity.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity te)
	{
		if (GameManager.Instance != null)
		{
			if (te == localTileEntity)
			{
				XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(te.ToWorldPos());
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
			localTileEntity = null;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "required_item")
		{
			requiredItem = _value;
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}
}
