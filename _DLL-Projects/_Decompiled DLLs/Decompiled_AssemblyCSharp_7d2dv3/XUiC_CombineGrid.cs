using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CombineGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack result1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public float experienceFromLastResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCombine te;

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_ItemStack>();
		XUiController[] array = childrenByType;
		if (array.Length == 3)
		{
			merge1 = (XUiC_RequiredItemStack)array[0];
			merge2 = (XUiC_RequiredItemStack)array[1];
			result1 = (XUiC_RequiredItemStack)array[2];
			merge1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			merge2.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			result1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			merge1.RequiredType = (merge2.RequiredType = (result1.RequiredType = XUiC_RequiredItemStack.RequiredTypes.HasQualityNoParts));
			merge1.SlotChangedEvent += Merge_SlotChangedEvent;
			merge2.SlotChangedEvent += Merge_SlotChangedEvent;
			result1.HiddenLock = true;
			result1.SlotChangedEvent += Result1_SlotChangedEvent;
			result1.TakeOnly = true;
			merge1.FailedSwap += Merge_FailedSwap;
			merge2.FailedSwap += Merge_FailedSwap;
		}
	}

	public void SetTileEntity(TEFeatureCombine _te)
	{
		te = _te;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_FailedSwap(ItemStack stack)
	{
		ItemClass itemClass = stack.itemValue.ItemClass;
		if (!stack.itemValue.HasQuality || itemClass.HasSubItems)
		{
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("ttCombineInvalidItem"));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Result1_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			ItemActionEntryScrap.HandleRemoveAmmo(merge1.ItemStack, xui);
			ItemActionEntryScrap.HandleRemoveAmmo(merge2.ItemStack, xui);
			merge1.ItemStack = ItemStack.Empty;
			merge2.ItemStack = ItemStack.Empty;
			if (lastResult != null)
			{
				xui.playerUI.entityPlayer.Progression.AddLevelExp((int)experienceFromLastResult);
				lastResult = null;
			}
			if (te != null)
			{
				te.HandlePlayComplete();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (merge1.ItemStack.IsEmpty() || merge2.ItemStack.IsEmpty() || merge1.ItemStack.itemValue.type != merge2.ItemStack.itemValue.type)
		{
			result1.SlotChangedEvent -= Result1_SlotChangedEvent;
			result1.ItemStack = ItemStack.Empty;
			result1.HiddenLock = true;
			result1.SlotChangedEvent += Result1_SlotChangedEvent;
			return;
		}
		ItemValue itemValue = merge1.ItemStack.itemValue;
		ItemValue itemValue2 = merge2.ItemStack.itemValue;
		if ((itemValue.HasMods() && itemValue2.HasMods()) || (itemValue.HasCosmetics() && itemValue2.HasCosmetics()))
		{
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineRemoveMods"), 0), string.Empty, "ui_denied");
			return;
		}
		bool flag = true;
		if (!itemValue.EqualsForMerging(itemValue2))
		{
			ItemStack itemStack = merge1.ItemStack.Clone();
			itemStack.itemValue.MergeBest(merge2.ItemStack.itemValue);
			if ((itemStack.itemValue.Quality > itemValue.Quality || !itemStack.itemValue.EqualsForMerging(itemValue)) && (itemStack.itemValue.Quality > itemValue2.Quality || !itemStack.itemValue.EqualsForMerging(itemValue2)))
			{
				itemStack.itemValue.Meta = 0;
				lastResult = itemStack;
				result1.ItemStack = itemStack;
				result1.HiddenLock = false;
				flag = false;
			}
		}
		if (flag)
		{
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineSameItem"), 0), string.Empty, "ui_denied");
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		XUiC_ItemInfoWindow childByType = xui.GetChildByType<XUiC_ItemInfoWindow>();
		merge1.InfoWindow = childByType;
		merge2.InfoWindow = childByType;
		result1.InfoWindow = childByType;
		IsDirty = true;
		merge1.ItemStack = ItemStack.Empty;
		merge2.ItemStack = ItemStack.Empty;
		result1.ItemStack = ItemStack.Empty;
		xui.CurrentCombineGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		IsDirty = true;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		if (!merge1.ItemStack.IsEmpty() && !playerInventory.AddItem(merge1.ItemStack))
		{
			EntityPlayer entityPlayer = xui.playerUI.entityPlayer;
			GameManager.Instance.ItemDropServer(merge1.ItemStack, entityPlayer.GetPosition(), Vector3.zero);
			entityPlayer.PlayOneShot("itemdropped");
		}
		if (!merge2.ItemStack.IsEmpty() && !playerInventory.AddItem(merge2.ItemStack))
		{
			EntityPlayer entityPlayer2 = xui.playerUI.entityPlayer;
			GameManager.Instance.ItemDropServer(merge2.ItemStack, entityPlayer2.GetPosition(), Vector3.zero);
			entityPlayer2.PlayOneShot("itemdropped");
		}
		xui.CurrentCombineGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (merge1.ItemStack.IsEmpty())
		{
			if (itemClass.HasQuality && !itemClass.HasSubItems)
			{
				merge1.ItemStack = itemStack;
				return true;
			}
		}
		else if (merge2.ItemStack.IsEmpty() && itemClass.HasQuality && !itemClass.HasSubItems)
		{
			merge2.ItemStack = itemStack;
			return true;
		}
		return false;
	}
}
