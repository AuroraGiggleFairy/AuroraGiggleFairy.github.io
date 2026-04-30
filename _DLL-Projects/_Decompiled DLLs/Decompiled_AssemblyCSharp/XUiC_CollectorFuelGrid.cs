using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectorFuelGrid : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector tileEntity;

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

	public override void OnOpen()
	{
		base.OnOpen();
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		tileEntity = te;
		if (!tileEntity.listeners.Contains(this))
		{
			tileEntity.listeners.Add(this);
		}
		SetStacks(te.FuelSlots);
	}

	public void OnTileEntityChanged(ITileEntity te)
	{
		if (te is TileEntityCollector tileEntityCollector && tileEntityCollector == tileEntity)
		{
			SetStacks(tileEntityCollector.FuelSlots);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		tileEntity.FuelSlots = stackList;
		windowGroup.Controller.SetAllChildrenDirty();
	}
}
