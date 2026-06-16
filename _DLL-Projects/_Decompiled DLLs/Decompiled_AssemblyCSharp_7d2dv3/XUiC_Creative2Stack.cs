using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2Stack : XUiC_ItemStack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setItemStack(ItemStack _stack)
	{
		itemStack = _stack;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClickComplete()
	{
		lastClicked = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		xui.DragAndDropWindow.CurrentStack = itemStack.Clone();
		xui.DragAndDropWindow.PickUpType = base.StackLocation;
		HandleSlotChangeEvent();
	}
}
