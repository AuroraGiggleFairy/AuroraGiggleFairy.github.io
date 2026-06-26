using UnityEngine.Scripting;

[Preserve]
public class XUiC_RequiredItemStack : XUiC_ItemStack
{
	public enum RequiredTypes
	{
		ItemClass,
		IsPart,
		HasQuality,
		HasQualityNoParts
	}

	public RequiredTypes RequiredType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass requiredItemClass;

	public bool RequiredItemOnly = true;

	public bool TakeOnly;

	public ItemClass RequiredItemClass
	{
		get
		{
			return requiredItemClass;
		}
		set
		{
			requiredItemClass = value;
			IsDirty = true;
		}
	}

	public override string ItemIcon
	{
		get
		{
			if (RequiredItemClass != null && itemStack.IsEmpty())
			{
				return RequiredItemClass.GetIconName();
			}
			return base.ItemIcon;
		}
	}

	public override string ItemIconColor
	{
		get
		{
			if (base.itemClass != null)
			{
				base.GreyedOut = false;
				return base.ItemIconColor;
			}
			if (requiredItemClass != null && !base.StackLock)
			{
				base.GreyedOut = true;
				return "255,255,255,255";
			}
			base.GreyedOut = false;
			return "255,255,255,0";
		}
	}

	public event XUiEvent_RequiredSlotFailedSwapEventHandler FailedSwap;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		if (TakeOnly && !stack.IsEmpty())
		{
			return false;
		}
		bool flag = false;
		flag = ((RequiredType == RequiredTypes.ItemClass && RequiredItemClass != null && RequiredItemOnly) ? (stack.itemValue.ItemClass == RequiredItemClass) : ((RequiredType == RequiredTypes.IsPart) ? (stack.itemValue.ItemClass.PartParentId != null) : ((RequiredType == RequiredTypes.HasQuality) ? stack.itemValue.HasQuality : (RequiredType != RequiredTypes.HasQualityNoParts || (stack.itemValue.HasQuality && !stack.itemValue.ItemClass.HasSubItems)))));
		if (!flag && this.FailedSwap != null)
		{
			this.FailedSwap(stack);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (base.itemStack.IsEmpty() && CanSwap(currentStack))
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = base.StackLocation;
				base.ItemStack = itemStack;
				PlayPlaceSound();
			}
		}
	}
}
