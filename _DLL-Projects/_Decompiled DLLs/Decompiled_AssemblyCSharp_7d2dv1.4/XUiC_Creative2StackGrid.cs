using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2StackGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Length
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			page = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		Length = itemControllers.Length;
		IsDirty = false;
	}

	public override ItemStack[] GetSlots()
	{
		return items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public void SetSlots(ItemStack[] stackList)
	{
		if (stackList != null)
		{
			items = stackList;
			IsDirty = true;
			XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
			for (int i = 0; i < Length; i++)
			{
				XUiC_ItemStack obj = itemControllers[i];
				obj.InfoWindow = childByType;
				obj.StackLocation = XUiC_ItemStack.StackLocationTypes.Creative;
				itemControllers[i].ViewComponent.IsVisible = true;
			}
		}
	}

	public override void Update(float _dt)
	{
		if (!base.ViewComponent.IsVisible || (GameManager.Instance == null && GameManager.Instance.World == null))
		{
			return;
		}
		if (IsDirty && base.xui.PlayerInventory != null)
		{
			for (int i = 0; i < Length; i++)
			{
				int num = i + Length * page;
				XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
				if (xUiC_ItemStack == null)
				{
					continue;
				}
				if (num < items.Length)
				{
					xUiC_ItemStack.ItemStack = items[num];
					continue;
				}
				xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
				if (xUiC_ItemStack.Selected)
				{
					xUiC_ItemStack.Selected = false;
				}
			}
			IsDirty = false;
		}
		base.Update(_dt);
	}
}
