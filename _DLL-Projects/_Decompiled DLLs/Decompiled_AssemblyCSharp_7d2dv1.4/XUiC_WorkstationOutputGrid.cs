using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationOutputGrid : XUiC_WorkstationGrid
{
	public void UpdateData(ItemStack[] stackList)
	{
		UpdateBackend(stackList);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		workstationData.SetOutputStacks(stackList);
		windowGroup.Controller.SetAllChildrenDirty();
	}
}
