using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiM_Workstation workstationData;

	public XUiM_Workstation WorkstationData => workstationData;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public virtual void SetSlots(ItemStack[] stacks)
	{
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		workstationData = ((XUiC_WorkstationWindowGroup)windowGroup.Controller).WorkstationData;
		IsDirty = true;
		IsDormant = false;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		IsDirty = true;
		IsDormant = true;
	}

	public int AddToItemStackArray(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		int num = -1;
		int num2 = 0;
		while (num == -1 && num2 < slots.Length)
		{
			if (slots[num2].CanStackWith(_itemStack))
			{
				slots[num2].count += _itemStack.count;
				_itemStack.count = 0;
				num = num2;
			}
			num2++;
		}
		int num3 = 0;
		while (num == -1 && num3 < slots.Length)
		{
			if (slots[num3].IsEmpty())
			{
				slots[num3] = _itemStack;
				num = num3;
			}
			num3++;
		}
		if (num != -1)
		{
			SetSlots(slots);
			UpdateBackend(slots);
		}
		return num;
	}
}
