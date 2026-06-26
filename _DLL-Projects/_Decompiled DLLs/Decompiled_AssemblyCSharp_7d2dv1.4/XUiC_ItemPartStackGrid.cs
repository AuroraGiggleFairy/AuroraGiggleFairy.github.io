using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemPartStackGrid : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass currentItemClass;

	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ItemStack CurrentItem { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleWindow AssembleWindow { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_ItemPartStack>();
		itemControllers = childrenByType;
		IsDirty = false;
	}

	public override void Update(float _dt)
	{
		if (!(GameManager.Instance == null) || GameManager.Instance.World != null)
		{
			base.Update(_dt);
		}
	}

	public void SetParts(ItemValue[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		currentItemClass = CurrentItem.itemValue.ItemClass;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemPartStack xUiC_ItemPartStack = (XUiC_ItemPartStack)itemControllers[i];
			if (i < CurrentItem.itemValue.Modifications.Length)
			{
				ItemValue itemValue = CurrentItem.itemValue.Modifications[i];
				if (itemValue != null && itemValue.ItemClass is ItemClassModifier)
				{
					xUiC_ItemPartStack.SlotType = (itemValue.ItemClass as ItemClassModifier).Type.ToStringCached().ToLower();
				}
				xUiC_ItemPartStack.SlotChangedEvent -= HandleSlotChangedEvent;
				xUiC_ItemPartStack.ItemValue = ((itemValue != null) ? itemValue : ItemValue.None.Clone());
				xUiC_ItemPartStack.SlotChangedEvent += HandleSlotChangedEvent;
				xUiC_ItemPartStack.SlotNumber = i;
				xUiC_ItemPartStack.InfoWindow = childByType;
				xUiC_ItemPartStack.StackLocation = StackLocation;
				xUiC_ItemPartStack.ViewComponent.IsVisible = true;
			}
			else
			{
				xUiC_ItemPartStack.ViewComponent.IsVisible = false;
			}
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		XUiC_ItemPartStack xUiC_ItemPartStack = (XUiC_ItemPartStack)itemControllers[slotNumber];
		ItemValue itemValue = (xUiC_ItemPartStack.ItemStack.IsEmpty() ? ItemValue.None.Clone() : xUiC_ItemPartStack.ItemStack.itemValue);
		if (itemValue.ItemClass != null)
		{
			if (itemValue.ItemClass.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && CurrentItem.itemValue.CosmeticMods.Length != 0)
			{
				CurrentItem.itemValue.CosmeticMods[0] = itemValue;
			}
			else
			{
				CurrentItem.itemValue.Modifications[slotNumber] = itemValue;
			}
		}
		else
		{
			CurrentItem.itemValue.Modifications[slotNumber] = itemValue;
		}
		AssembleWindow.ItemStack = CurrentItem;
		AssembleWindow.OnChanged();
		base.xui.AssembleItem.RefreshAssembleItem();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBackend(ItemStack[] stackList)
	{
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}
}
