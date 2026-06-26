using UnityEngine.Scripting;

[Preserve]
public class XUiC_SlotPreview : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] itemStacks;

	public override void Init()
	{
		base.Init();
		slots = GetChildrenById("slot");
		XUiController[] childrenByType = parent.GetChildrenByType<XUiC_ItemStack>();
		itemStacks = childrenByType;
	}

	public override void OnOpen()
	{
		for (int i = 0; i < itemStacks.Length; i++)
		{
			XUiC_SlotPreview_SlotChangedEvent(i, ((XUiC_ItemStack)itemStacks[i]).ItemStack);
			((XUiC_ItemStack)itemStacks[i]).SlotChangedEvent += XUiC_SlotPreview_SlotChangedEvent;
			((XUiC_ItemStack)itemStacks[i]).ToolLockChangedEvent += XUiC_SlotPreview_ToolLockChangedEvent;
			XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)itemStacks[i];
			slots[i].ViewComponent.IsVisible = ((XUiC_ItemStack)itemStacks[i]).ItemStack.IsEmpty() && !xUiC_ItemStack.ToolLock;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SlotPreview_ToolLockChangedEvent(int slotNumber, ItemStack stack, bool locked)
	{
		slots[slotNumber].ViewComponent.IsVisible = stack.IsEmpty() && !locked;
	}

	public override void OnClose()
	{
		for (int i = 0; i < itemStacks.Length; i++)
		{
			((XUiC_ItemStack)itemStacks[i]).SlotChangedEvent -= XUiC_SlotPreview_SlotChangedEvent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SlotPreview_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		slots[slotNumber].ViewComponent.IsVisible = stack.IsEmpty();
	}
}
