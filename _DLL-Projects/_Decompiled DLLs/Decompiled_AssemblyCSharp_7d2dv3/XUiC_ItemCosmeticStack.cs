using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemCosmeticStack : XUiC_BasePartStack
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStackMissing;

	public ItemValue ItemValue
	{
		get
		{
			return itemValue;
		}
		set
		{
			itemValue = value;
			base.ItemStack = new ItemStack(value, 1);
		}
	}

	public ItemClass ExpectedItemClass
	{
		get
		{
			return expectedItemClass;
		}
		set
		{
			expectedItemClass = value;
			SetEmptySpriteName();
		}
	}

	public override void Init()
	{
		base.Init();
		lblStackMissing = Localization.Get("lblPartStackMissing");
	}

	public override string GetAtlas()
	{
		if (base.ItemStack.IsEmpty())
		{
			return "ItemIconAtlasGreyscale";
		}
		return "ItemIconAtlas";
	}

	public override string GetPartName()
	{
		if (itemClass == null && expectedItemClass == null)
		{
			return "";
		}
		if (itemClass == null)
		{
			return string.Format(lblStackMissing, expectedItemClass.GetLocalizedItemName());
		}
		return itemClass.GetLocalizedItemName();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetEmptySpriteName()
	{
		if (expectedItemClass != null && expectedItemClass.Id != 0)
		{
			emptySpriteName = expectedItemClass.GetIconName();
		}
		else
		{
			emptySpriteName = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		if (!(stack.itemValue.ItemClass is ItemClassModifier itemClassModifier))
		{
			return false;
		}
		if (xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier != null && !itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool result = false;
			for (int i = 0; i < xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				if (xui.AssembleItem.CurrentItem.itemValue.Modifications[i] == null || xui.AssembleItem.CurrentItem.itemValue.Modifications[i].IsEmpty())
				{
					result = true;
				}
				else if (xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass is ItemClassModifier itemClassModifier2 && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags);
		for (int j = 0; j < xui.AssembleItem.CurrentItem.itemValue.CosmeticMods.Length; j++)
		{
			if (xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != null && xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass != null && !xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags.IsEmpty && xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass is ItemClassModifier itemClassModifier3 && xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != itemValue && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier3.ModifierTags))
			{
				return false;
			}
		}
		if (flag)
		{
			if (itemValue != null && itemValue.type != 0)
			{
				return (itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		if (!(itemClass is ItemClassModifier))
		{
			return false;
		}
		return (itemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		ItemClassModifier itemClassModifier = xui.DragAndDropWindow.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier == null || itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) || itemClassModifier == null || itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) || xui.AssembleItem.CurrentItem.itemValue.Modifications.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass is ItemClassModifier itemClassModifier2 && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			xui.DragAndDropWindow.PickUpType = base.StackLocation;
		}
	}
}
