using System;
using System.Collections.Generic;
using UnityEngine;
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
	public readonly List<ItemClass> allowedItemClasses = new List<ItemClass>();

	public bool RequiredItemOnly = true;

	public bool TakeOnly;

	public bool HasAllowedItemClass
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (RequiredType == RequiredTypes.ItemClass && allowedItemClasses.Count > 0)
			{
				return allowedItemClasses[0] != null;
			}
			return false;
		}
	}

	public override string ItemIcon
	{
		get
		{
			if (HasAllowedItemClass && itemStack.IsEmpty())
			{
				return allowedItemClasses[0].GetIconName();
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
			if (HasAllowedItemClass && !base.StackLock)
			{
				base.GreyedOut = true;
				return "255,255,255,255";
			}
			base.GreyedOut = false;
			return "255,255,255,0";
		}
	}

	public event XUiEvent_RequiredSlotFailedSwapEventHandler FailedSwap;

	public bool ItemAllowed(ItemStack _stack)
	{
		return RequiredType switch
		{
			RequiredTypes.ItemClass => !HasAllowedItemClass || !RequiredItemOnly || allowedItemClasses.IndexOf(_stack.itemValue.ItemClass) >= 0, 
			RequiredTypes.IsPart => _stack.itemValue.ItemClass.PartParentId != null, 
			RequiredTypes.HasQuality => _stack.itemValue.HasQuality, 
			RequiredTypes.HasQualityNoParts => _stack.itemValue.HasQuality && !_stack.itemValue.ItemClass.HasSubItems, 
			_ => true, 
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack _stack)
	{
		if (TakeOnly && !_stack.IsEmpty())
		{
			return false;
		}
		bool num = ItemAllowed(_stack);
		if (!num)
		{
			XUiEvent_RequiredSlotFailedSwapEventHandler xUiEvent_RequiredSlotFailedSwapEventHandler = this.FailedSwap;
			if (xUiEvent_RequiredSlotFailedSwapEventHandler == null)
			{
				return num;
			}
			xUiEvent_RequiredSlotFailedSwapEventHandler(_stack);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (TryStack(currentStack, 1))
		{
			base.xui.dragAndDrop.CurrentStack = currentStack;
			PlayPlaceSound();
		}
	}

	public bool TryStack(ItemStack _stack, int _countToPlace = -1)
	{
		if (_stack.IsEmpty())
		{
			return false;
		}
		if (_countToPlace < 0)
		{
			_countToPlace = _stack.count;
		}
		if (_countToPlace > _stack.count)
		{
			_countToPlace = _stack.count;
		}
		if (base.itemStack.IsEmpty())
		{
			if (!ItemAllowed(_stack))
			{
				return false;
			}
			ItemStack itemStack = _stack.Clone();
			itemStack.count = _countToPlace;
			_stack.count -= _countToPlace;
			base.ItemStack = itemStack;
			HandleSlotChangeEvent();
			return true;
		}
		if (_stack.itemValue.type == base.itemStack.itemValue.type)
		{
			ItemClass itemClass = base.itemStack.itemValue.ItemClassOrMissing;
			int num = ((OverrideStackCount == -1) ? itemClass.Stacknumber.Value : Mathf.Min(itemClass.Stacknumber.Value, OverrideStackCount)) - base.itemStack.count;
			if (num <= 0)
			{
				return false;
			}
			if (num < _countToPlace)
			{
				_countToPlace = num;
			}
			ItemStack itemStack2 = base.itemStack.Clone();
			itemStack2.count += _countToPlace;
			_stack.count -= _countToPlace;
			base.ItemStack = itemStack2;
			HandleSlotChangeEvent();
			return true;
		}
		return false;
	}

	public void ClearAllowedItemClasses()
	{
		IsDirty = true;
		allowedItemClasses.Clear();
	}

	public void SetAllowedItemClassSingle(string _itemClassName)
	{
		SetAllowedItemClassSingle(ItemClass.GetItemClass(_itemClassName));
	}

	public void SetAllowedItemClassSingle(ItemClass _itemClass)
	{
		IsDirty = true;
		allowedItemClasses.Clear();
		if (_itemClass != null)
		{
			allowedItemClasses.Add(_itemClass);
		}
	}

	public void SetAllowedItemClasses(ItemClass[] _itemClasses)
	{
		IsDirty = true;
		allowedItemClasses.Clear();
		allowedItemClasses.AddRange(_itemClasses);
	}

	public static void ParseItemClassesFromString(IList<ItemClass> _targetList, string _itemClassesString)
	{
		if (string.IsNullOrEmpty(_itemClassesString))
		{
			return;
		}
		string[] array = _itemClassesString.Split('+', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (text.StartsWith("tags(", StringComparison.Ordinal))
			{
				ParseTagsBasedEntry(text);
			}
			else
			{
				AddItemClassByName(text);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void AddItemClass(ItemClass _itemClass)
		{
			if (_targetList.IndexOf(_itemClass) < 0)
			{
				_targetList.Add(_itemClass);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void AddItemClassByName(string _itemClassName)
		{
			ItemClass itemClass = ItemClass.GetItemClass(_itemClassName);
			if (itemClass == null)
			{
				Log.Warning("ItemClasses: ItemClass '" + _itemClassName + "' not found. From: " + StackTraceUtility.ExtractStackTrace());
			}
			else
			{
				AddItemClass(itemClass);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void ParseTagsBasedEntry(string _text)
		{
			if (_text[_text.Length - 1] != ')')
			{
				Log.Warning("ItemClasses: Malformed tags() entry: '" + _text + "'. From: " + StackTraceUtility.ExtractStackTrace());
				return;
			}
			foreach (ItemClass item in ItemClass.GetItemsWithTag(FastTags<TagGroup.Global>.Parse(_text.Substring("tags(".Length, _text.Length - 1 - "tags(".Length))))
			{
				AddItemClass(item);
			}
		}
	}
}
