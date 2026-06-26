using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemCosmeticStackGrid : XUiController
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
		XUiController[] childrenByType = GetChildrenByType<XUiC_ItemCosmeticStack>();
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
			XUiC_ItemCosmeticStack xUiC_ItemCosmeticStack = (XUiC_ItemCosmeticStack)itemControllers[i];
			if (i < CurrentItem.itemValue.CosmeticMods.Length)
			{
				ItemValue itemValue = CurrentItem.itemValue.CosmeticMods[i];
				if (itemValue != null && itemValue.ItemClass is ItemClassModifier)
				{
					xUiC_ItemCosmeticStack.SlotType = (itemValue.ItemClass as ItemClassModifier).Type.ToStringCached().ToLower();
				}
				xUiC_ItemCosmeticStack.SlotChangedEvent -= HandleSlotChangedEvent;
				xUiC_ItemCosmeticStack.ItemValue = ((itemValue != null) ? itemValue : ItemValue.None.Clone());
				xUiC_ItemCosmeticStack.SlotChangedEvent += HandleSlotChangedEvent;
				xUiC_ItemCosmeticStack.SlotNumber = i;
				xUiC_ItemCosmeticStack.InfoWindow = childByType;
				xUiC_ItemCosmeticStack.StackLocation = StackLocation;
				xUiC_ItemCosmeticStack.ViewComponent.IsVisible = true;
			}
			else
			{
				xUiC_ItemCosmeticStack.ViewComponent.IsVisible = false;
			}
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		XUiC_ItemCosmeticStack xUiC_ItemCosmeticStack = (XUiC_ItemCosmeticStack)itemControllers[slotNumber];
		ItemValue itemValue = (xUiC_ItemCosmeticStack.ItemStack.IsEmpty() ? ItemValue.None.Clone() : xUiC_ItemCosmeticStack.ItemStack.itemValue);
		if (itemValue.ItemClass != null)
		{
			if (!itemValue.ItemClass.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && CurrentItem.itemValue.Modifications.Length != 0)
			{
				for (int i = 0; i < CurrentItem.itemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = CurrentItem.itemValue.Modifications[i];
					if (itemValue2 == null || itemValue2.IsEmpty())
					{
						CurrentItem.itemValue.Modifications[i] = itemValue;
						break;
					}
				}
			}
			else
			{
				CurrentItem.itemValue.CosmeticMods[slotNumber] = itemValue;
			}
		}
		else
		{
			CurrentItem.itemValue.CosmeticMods[slotNumber] = itemValue;
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
