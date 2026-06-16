using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectorFuelGrid : XUiC_VariableHeightGrid, ITileEntityChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum FuelBehavior
	{
		Fuel,
		Catalyst
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public new TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockCollector blockCollector;

	[PublicizedFrom(EAccessModifier.Private)]
	public FuelBehavior fuelBehavior;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] previewSprites;

	[PublicizedFrom(EAccessModifier.Private)]
	public int collectorFuelGridLength;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			XUiC_ItemStack.StackLocationTypes result = XUiC_ItemStack.StackLocationTypes.CollectorFuel;
			if (fuelBehavior == FuelBehavior.Catalyst)
			{
				result = XUiC_ItemStack.StackLocationTypes.CollectorCatalysts;
			}
			return result;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		switch (fuelBehavior)
		{
		case FuelBehavior.Fuel:
			xui.CurrentCollectorFuelGrid = this;
			break;
		case FuelBehavior.Catalyst:
			xui.currentCollectorCatalystGrid = this;
			break;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		switch (fuelBehavior)
		{
		case FuelBehavior.Fuel:
			xui.CurrentCollectorFuelGrid = null;
			break;
		case FuelBehavior.Catalyst:
			xui.currentCollectorCatalystGrid = null;
			break;
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		bool flag = false;
		if (_name == "fuelbehavior")
		{
			if (!Enum.TryParse<FuelBehavior>(_value, out fuelBehavior))
			{
				fuelBehavior = FuelBehavior.Fuel;
			}
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	public bool TryAddFuel(ItemClass newItemClass, ItemStack newItemStack)
	{
		string value = "";
		string[] array = new string[0];
		switch (fuelBehavior)
		{
		case FuelBehavior.Fuel:
			array = tileEntity.GetFuelTypes();
			break;
		case FuelBehavior.Catalyst:
			array = tileEntity.GetCatalystTypes();
			break;
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (newItemClass.Name == text)
			{
				value = text;
				break;
			}
		}
		if (!string.IsNullOrEmpty(value))
		{
			int num = 0;
			switch (fuelBehavior)
			{
			case FuelBehavior.Fuel:
				num = tileEntity.FuelGridLength;
				break;
			case FuelBehavior.Catalyst:
				num = tileEntity.CatalystGridLength;
				break;
			}
			for (int j = 0; j < num; j++)
			{
				XUiC_ItemStack xUiC_ItemStack = itemControllers[j];
				if (xUiC_ItemStack.ItemStack.IsEmpty())
				{
					xUiC_ItemStack.ItemStack = newItemStack.Clone();
					newItemStack.count = 0;
					break;
				}
				int num2 = xUiC_ItemStack.ItemStack.StackTransferCount(newItemStack);
				xUiC_ItemStack.ItemStack.count += num2;
				newItemStack.count -= num2;
				if (newItemStack.count == 0)
				{
					break;
				}
			}
		}
		UpdateBackend(getUISlots());
		return newItemStack.count == 0;
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xUiV_Grid = viewComponent as XUiV_Grid;
		XUiController childById = parent.Parent.GetChildById("slot_preview");
		XUiV_Grid xUiV_Grid2 = childById.ViewComponent as XUiV_Grid;
		InitVariableHeightGrids(new XUiV_Grid[2] { xUiV_Grid, xUiV_Grid2 });
		XUiController[] childrenById = childById.GetChildrenById("slot");
		int num = childrenById.Length;
		previewSprites = new XUiV_Sprite[num];
		for (int i = 0; i < num; i++)
		{
			previewSprites[i] = childrenById[i].ViewComponent as XUiV_Sprite;
		}
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		tileEntity = te;
		if (!tileEntity.listeners.Contains(this))
		{
			tileEntity.listeners.Add(this);
		}
		SetStacks(te.FuelSlots);
		blockCollector = te.blockValue.Block as BlockCollector;
		string[] array = new string[0];
		string[] array2 = new string[0];
		if (blockCollector != null)
		{
			if (fuelBehavior == FuelBehavior.Catalyst)
			{
				SetHeight(te.CatalystGridHeight);
				array = blockCollector.CatalystTypesSprites;
				array2 = blockCollector.CatalystTypesSpriteAtlases;
			}
			else
			{
				SetHeight(te.FuelGridHeight);
				array = blockCollector.FuelTypesSprites;
				array2 = blockCollector.FuelTypesSpriteAtlases;
			}
		}
		int num = array.Length;
		if (num > 0)
		{
			for (int i = 0; i < previewSprites.Length; i++)
			{
				int num2 = i % num;
				previewSprites[i].SpriteName = array[num2];
				previewSprites[i].UIAtlas = array2[num2];
			}
		}
	}

	public void OnTileEntityChanged(ITileEntity te)
	{
		if (te is TileEntityCollector tileEntityCollector && tileEntityCollector == tileEntity)
		{
			switch (fuelBehavior)
			{
			case FuelBehavior.Fuel:
				SetStacks(tileEntityCollector.FuelSlots);
				SetHeight(tileEntityCollector.FuelGridHeight);
				break;
			case FuelBehavior.Catalyst:
				SetStacks(tileEntityCollector.CatalystSlots);
				SetHeight(tileEntityCollector.CatalystGridHeight);
				break;
			}
		}
	}

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		ItemStack[] uISlots = getUISlots();
		FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != FuelBehavior.Fuel && fuelBehavior == FuelBehavior.Catalyst)
		{
			BlockCollector.CatalystConvert[] catalystConverts = blockCollector.CatalystConverts;
			for (int i = 0; i < catalystConverts.Length; i++)
			{
				ItemStack itemStack = catalystConverts[i].Convert(stack);
				if (itemStack != null)
				{
					uISlots[slotNumber] = itemStack;
					break;
				}
			}
		}
		if (items != null)
		{
			items[slotNumber] = stack.Clone();
		}
		UpdateBackend(uISlots);
		fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != FuelBehavior.Fuel && fuelBehavior == FuelBehavior.Catalyst)
		{
			SetStacks(tileEntity.CatalystSlots);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		switch (fuelBehavior)
		{
		case FuelBehavior.Fuel:
			tileEntity.FuelSlots = stackList;
			break;
		case FuelBehavior.Catalyst:
			tileEntity.CatalystSlots = stackList;
			break;
		}
		tileEntity.SetModified();
		windowGroup.Controller.SetAllChildrenDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "container_slots")
		{
			_value = collectorFuelGridLength.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
