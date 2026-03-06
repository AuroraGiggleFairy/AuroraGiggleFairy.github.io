using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceSlots : XUiC_ItemStackGrid
{
	public static XUiC_PowerSourceSlots Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			SetSlots(tileEntity.ItemSlots);
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceWindowGroup Owner { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRequirements()
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xUiC_RequiredItemStack.SetAllowedItemClassSingle(tileEntity.SlotItem);
			}
		}
	}

	public virtual void SetSlots(ItemStack[] stacks)
	{
		items = stacks;
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		IsDirty = true;
		SetRequirements();
		XUiC_PowerSourceSlots current = (base.xui.powerSourceSlots = this);
		Current = current;
		IsDormant = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		IsDirty = true;
		XUiC_PowerSourceSlots current = (base.xui.powerSourceSlots = null);
		Current = current;
		IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetOn(bool isOn)
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		tileEntity.ItemSlots = stackList;
		tileEntity.SetSendSlots();
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			base.Update(_dt);
			if (tileEntity.IsOn)
			{
				SetSlots(tileEntity.ItemSlots);
			}
			RefreshBindings();
		}
	}

	public void Refresh()
	{
		SetSlots(tileEntity.ItemSlots);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (itemClass != tileEntity.SlotItem)
		{
			return false;
		}
		bool num = tileEntity.TryAddItemToSlot(itemClass, itemStack);
		if (num)
		{
			SetSlots(tileEntity.ItemSlots);
		}
		return num;
	}
}
