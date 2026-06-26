using System;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_FailedSwap(ItemStack stack)
	{
		ItemClass itemClass = stack.itemValue.ItemClass;
		if (!stack.itemValue.HasQuality || itemClass.HasSubItems)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, Localization.Get("ttCombineInvalidItem"));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Result1_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			merge1.ItemStack = ItemStack.Empty.Clone();
			merge2.ItemStack = ItemStack.Empty.Clone();
			if (lastResult != null)
			{
				base.xui.playerUI.entityPlayer.Progression.AddLevelExp((int)experienceFromLastResult);
				lastResult = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (merge1.ItemStack.IsEmpty() || merge2.ItemStack.IsEmpty() || merge1.ItemStack.itemValue.type != merge2.ItemStack.itemValue.type)
		{
			result1.SlotChangedEvent -= Result1_SlotChangedEvent;
			result1.ItemStack = ItemStack.Empty.Clone();
			result1.HiddenLock = true;
			result1.SlotChangedEvent += Result1_SlotChangedEvent;
		}
		else
		{
			if (merge1.ItemStack.itemValue.type != merge2.ItemStack.itemValue.type)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			ItemStack itemStack;
			ItemStack itemStack2;
			if (merge1.ItemStack.itemValue.Quality > merge2.ItemStack.itemValue.Quality)
			{
				itemStack = merge1.ItemStack;
				itemStack2 = merge2.ItemStack;
			}
			else if (merge2.ItemStack.itemValue.Quality > merge1.ItemStack.itemValue.Quality)
			{
				itemStack = merge2.ItemStack;
				itemStack2 = merge1.ItemStack;
			}
			else if (merge1.ItemStack.itemValue.UseTimes < merge2.ItemStack.itemValue.UseTimes)
			{
				itemStack = merge1.ItemStack;
				itemStack2 = merge2.ItemStack;
			}
			else
			{
				itemStack = merge2.ItemStack;
				itemStack2 = merge1.ItemStack;
			}
			ItemStack itemStack3 = itemStack.Clone();
			num = (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, XUiM_Player.GetPlayer()) + 1;
			experienceFromLastResult = 0f;
			int num2 = 0;
			if (itemStack.itemValue.UseTimes != 0f)
			{
				num2 = Mathf.Min(itemStack2.itemValue.MaxUseTimes - (int)itemStack2.itemValue.UseTimes, (int)itemStack3.itemValue.UseTimes);
				itemStack3.itemValue.UseTimes -= num2;
				experienceFromLastResult += (float)itemStack3.itemValue.MaxUseTimes / (float)(itemStack3.itemValue.MaxUseTimes - num2);
				flag = true;
			}
			if (itemStack2.itemValue.UseTimes + (float)num2 < (float)itemStack2.itemValue.MaxUseTimes && itemStack.itemValue.Quality < 6 && itemStack.itemValue.Quality < num)
			{
				float num3 = ((float)itemStack2.itemValue.MaxUseTimes - itemStack2.itemValue.UseTimes) / (float)itemStack2.itemValue.MaxUseTimes;
				int num4 = (int)Math.Max(1f, (float)(int)itemStack2.itemValue.Quality * num3 * 0.1f);
				if (itemStack3.itemValue.Quality + num4 > num)
				{
					num4 = num - itemStack3.itemValue.Quality;
				}
				if (itemStack3.itemValue.Quality + num4 > 6)
				{
					itemStack3.itemValue.Quality = 6;
				}
				else
				{
					itemStack3.itemValue.Quality += (ushort)num4;
				}
				itemStack3.itemValue = new ItemValue(itemStack3.itemValue.type, itemStack3.itemValue.Quality, itemStack3.itemValue.Quality);
				experienceFromLastResult *= num4;
				flag2 = true;
			}
			if (flag || flag2)
			{
				result1.ItemStack = itemStack3;
				result1.HiddenLock = false;
				lastResult = itemStack3;
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineLimitExceeded"), num), string.Empty, "ui_denied");
			}
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
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		merge1.InfoWindow = childByType;
		merge2.InfoWindow = childByType;
		result1.InfoWindow = childByType;
		IsDirty = true;
		merge1.ItemStack = ItemStack.Empty.Clone();
		merge2.ItemStack = ItemStack.Empty.Clone();
		result1.ItemStack = ItemStack.Empty.Clone();
		base.xui.currentCombineGrid = this;
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
		XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
		if (!merge1.ItemStack.IsEmpty())
		{
			playerInventory.AddItem(merge1.ItemStack);
		}
		if (!merge2.ItemStack.IsEmpty())
		{
			playerInventory.AddItem(merge2.ItemStack);
		}
		base.xui.currentCombineGrid = null;
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
