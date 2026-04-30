using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorModGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockCollector blockCollector;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public override void Init()
	{
		base.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		tileEntity.ModSlots = stackList;
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		tileEntity = te;
		blockCollector = te.blockValue.Block as BlockCollector;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				if (i < blockCollector.RequiredMods.Length)
				{
					xUiC_RequiredItemStack.SetAllowedItemClassSingle(blockCollector.RequiredMods[i]);
					xUiC_RequiredItemStack.RequiredItemOnly = blockCollector.RequiredModsOnly;
				}
				else
				{
					xUiC_RequiredItemStack.ClearAllowedItemClasses();
					xUiC_RequiredItemStack.RequiredItemOnly = false;
				}
				xUiC_RequiredItemStack.StackLocation = StackLocation;
			}
		}
		SetStacks(te.ModSlots);
	}

	public bool TryAddMod(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!blockCollector.RequiredModsOnly)
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

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.currentDewCollectorModGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentDewCollectorModGrid = null;
	}
}
