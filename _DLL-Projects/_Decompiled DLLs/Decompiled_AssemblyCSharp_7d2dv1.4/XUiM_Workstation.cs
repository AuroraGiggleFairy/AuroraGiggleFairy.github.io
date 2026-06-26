public class XUiM_Workstation : XUiModel
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityWorkstation tileEntity;

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

	public void SetInputStacks(ItemStack[] itemStacks)
	{
		tileEntity.Input = itemStacks;
	}

	public void SetInputInSlot(int idx, ItemStack itemStack)
	{
		tileEntity.Input[idx] = itemStack.Clone();
	}

	public ItemStack[] GetOutputStacks()
	{
		return tileEntity.Output;
	}

	public void SetOutputStacks(ItemStack[] itemStacks)
	{
		tileEntity.Output = itemStacks;
	}

	public void SetOutputInSlot(int idx, ItemStack itemStack)
	{
		tileEntity.Output[idx] = itemStack.Clone();
	}

	public ItemStack[] GetToolStacks()
	{
		return tileEntity.Tools;
	}

	public void SetToolStacks(ItemStack[] itemStacks)
	{
		tileEntity.Tools = itemStacks;
	}

	public void SetToolInSlot(int idx, ItemStack itemStack)
	{
		tileEntity.Tools[idx] = itemStack.Clone();
	}

	public ItemStack[] GetFuelStacks()
	{
		return tileEntity.Fuel;
	}

	public void SetFuelStacks(ItemStack[] itemStacks)
	{
		tileEntity.Fuel = itemStacks;
	}

	public void SetFuelInSlot(int idx, ItemStack itemStack)
	{
		tileEntity.Fuel[idx] = itemStack.Clone();
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

	public void SetRecipeQueueItems(RecipeQueueItem[] queueStacks)
	{
		tileEntity.Queue = queueStacks;
	}

	public void SetQueueInSlot(int idx, RecipeQueueItem queueStack)
	{
		tileEntity.Queue[idx] = queueStack;
	}

	public void SetUserAccessing(bool isUserAccessing)
	{
		tileEntity.SetUserAccessing(isUserAccessing);
	}

	public string[] GetMaterialNames()
	{
		return tileEntity.MaterialNames;
	}
}
