public class XUiM_Workstation : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TileEntityWorkstation tileEntity;

	public TileEntityWorkstation TileEntity => tileEntity;

	public XUiM_Workstation(TileEntityWorkstation _te)
	{
		tileEntity = _te;
	}

	public bool GetIsBurning()
	{
		return tileEntity.IsBurning;
	}

	public bool GetIsBesideWater()
	{
		return tileEntity.IsBesideWater;
	}

	public void SetIsBurning(bool _isBurning)
	{
		tileEntity.IsBurning = _isBurning;
		tileEntity.ResetTickTime();
	}

	public ItemStack[] GetInputStacks()
	{
		return tileEntity.Input;
	}

	public void SetInputStacks(ItemStack[] _itemStacks)
	{
		tileEntity.Input = _itemStacks;
	}

	public void SetInputInSlot(int _idx, ItemStack _itemStack)
	{
		tileEntity.Input[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetOutputStacks()
	{
		return tileEntity.Output;
	}

	public void SetOutputStacks(ItemStack[] _itemStacks)
	{
		tileEntity.Output = _itemStacks;
	}

	public void SetOutputInSlot(int _idx, ItemStack _itemStack)
	{
		tileEntity.Output[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetToolStacks()
	{
		return tileEntity.Tools;
	}

	public void SetToolStacks(ItemStack[] _itemStacks)
	{
		tileEntity.Tools = _itemStacks;
	}

	public void SetToolInSlot(int _idx, ItemStack _itemStack)
	{
		tileEntity.Tools[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetFuelStacks()
	{
		return tileEntity.Fuel;
	}

	public void SetFuelStacks(ItemStack[] _itemStacks)
	{
		tileEntity.Fuel = _itemStacks;
	}

	public void SetFuelInSlot(int _idx, ItemStack _itemStack)
	{
		tileEntity.Fuel[_idx] = _itemStack.Clone();
	}

	public float GetBurnTimeLeft()
	{
		if (tileEntity.BurnTimeLeft == 0f)
		{
			return 0f;
		}
		return tileEntity.BurnTimeLeft + 0.5f;
	}

	public float GetTotalBurnTimeLeft()
	{
		if (tileEntity.BurnTotalTimeLeft == 0f)
		{
			return 0f;
		}
		return tileEntity.BurnTotalTimeLeft + 0.5f;
	}

	public RecipeQueueItem[] GetRecipeQueueItems()
	{
		return tileEntity.Queue;
	}

	public void SetRecipeQueueItems(RecipeQueueItem[] _queueStacks)
	{
		tileEntity.Queue = _queueStacks;
	}

	public void SetQueueInSlot(int _idx, RecipeQueueItem _queueStack)
	{
		tileEntity.Queue[_idx] = _queueStack;
	}

	public void SetUserAccessing(bool _isUserAccessing)
	{
		tileEntity.SetUserAccessing(_isUserAccessing);
	}

	public string[] GetMaterialNames()
	{
		return tileEntity.MaterialNames;
	}
}
