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
		if (base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier != null && !itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool result = false;
			for (int i = 0; i < base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				if (base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i] == null || base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].IsEmpty())
				{
					result = true;
				}
				else if (itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass.ItemTags))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags);
		for (int j = 0; j < base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods.Length; j++)
		{
			if (base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != null && base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass != null && !base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags.IsEmpty && base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != itemValue && itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags))
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
		ItemClassModifier itemClassModifier = base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier == null || itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) || itemClassModifier == null || itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) || base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass.ItemTags))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			base.xui.dragAndDrop.PickUpType = base.StackLocation;
		}
	}
}
