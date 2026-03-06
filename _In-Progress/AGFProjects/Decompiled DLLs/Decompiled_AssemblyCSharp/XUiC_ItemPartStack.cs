using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemPartStack : XUiC_BasePartStack
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
		ItemValue itemValue = base.xui.AssembleItem.CurrentItem.itemValue;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!itemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			return true;
		}
		if (itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && itemValue.CosmeticMods.Length != 0)
		{
			for (int i = 0; i < itemValue.CosmeticMods.Length; i++)
			{
				if (itemValue.CosmeticMods[i] == null || itemValue.CosmeticMods[i].IsEmpty())
				{
					return true;
				}
			}
			return false;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || itemClass.HasAnyTags(itemClassModifier.InstallableTags);
		int num = 0;
		_ = base.ItemClass;
		for (int j = 0; j < itemValue.Modifications.Length; j++)
		{
			if (itemValue.Modifications[j].ItemClass != null && !itemValue.Modifications[j].ItemClass.ItemTags.IsEmpty && !itemValue.Modifications[j].Equals(this.itemValue) && itemClassModifier.HasAnyTags(itemValue.Modifications[j].ItemClass.ItemTags))
			{
				num++;
			}
		}
		if (num >= stack.itemValue.ItemClass.MaxModsAllowed)
		{
			return false;
		}
		if (flag)
		{
			if (this.itemValue != null && this.itemValue.type != 0)
			{
				return (this.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		bool flag = true;
		if (itemClass is ItemClassModifier itemClassModifier)
		{
			flag = flag && itemClassModifier.Type == ItemClassModifier.ModifierTypes.Attachment;
			if (itemClassModifier.HasAnyTags(EntityVehicle.StorageModifierTags) && base.WindowGroup.Controller is XUiC_VehicleWindowGroup xUiC_VehicleWindowGroup)
			{
				flag = flag && xUiC_VehicleWindowGroup.CurrentVehicleEntity.CanRemoveInventoryMod();
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		ItemClassModifier itemClassModifier = base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier != null && itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
		{
			base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			base.xui.dragAndDrop.PickUpType = base.StackLocation;
		}
	}
}
