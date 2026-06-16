public class XUiM_AssembleItem : XUiModel
{
	public XUiC_AssembleWindow AssembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack currentItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack currentItemStackController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack currentEquipmentStackController;

	public ItemStack CurrentItem
	{
		get
		{
			return currentItem;
		}
		set
		{
			currentItem = value;
			if (value != null)
			{
				SetPartCount();
			}
		}
	}

	public XUiC_ItemStack CurrentItemStackController
	{
		get
		{
			return currentItemStackController;
		}
		set
		{
			if (currentItemStackController != null)
			{
				currentItemStackController.AssembleLock = false;
			}
			currentItemStackController = value;
			if (currentItemStackController != null)
			{
				currentItemStackController.AssembleLock = true;
			}
		}
	}

	public XUiC_EquipmentStack CurrentEquipmentStackController
	{
		get
		{
			return currentEquipmentStackController;
		}
		set
		{
			currentEquipmentStackController = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int PartCount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPartCount()
	{
		PartCount = 0;
		for (int i = 0; i < CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (CurrentItem.itemValue.Modifications[i] == null)
			{
				CurrentItem.itemValue.Modifications[i] = ItemValue.None;
			}
			if (!CurrentItem.itemValue.Modifications[i].IsEmpty())
			{
				PartCount++;
			}
		}
	}

	public void RefreshAssembleItem()
	{
		PartCount = 0;
		bool flag = false;
		for (int i = 0; i < CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (!CurrentItem.itemValue.Modifications[i].IsEmpty())
			{
				PartCount++;
			}
			else
			{
				flag = true;
			}
		}
		if (CurrentItemStackController != null)
		{
			CurrentItemStackController.ForceSetItemStack(CurrentItem);
			CurrentItemStackController.AssembleLock = true;
		}
		if (currentEquipmentStackController != null)
		{
			currentEquipmentStackController.ItemStack = CurrentItem;
		}
		if (flag)
		{
			QuestEventManager.Current.AssembledItem(CurrentItem);
		}
	}

	public bool AddPartToItem(ItemStack _partStack, out ItemStack _resultStack)
	{
		if (CurrentItem == null || CurrentItem.IsEmpty())
		{
			_resultStack = _partStack;
			return false;
		}
		ItemClassModifier itemClassModifier = _partStack.itemValue.ItemClass as ItemClassModifier;
		if (itemClassModifier != null)
		{
			if (CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
			{
				_resultStack = _partStack;
				return false;
			}
			if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
			{
				for (int i = 0; i < CurrentItem.itemValue.CosmeticMods.Length; i++)
				{
					if (CurrentItem.itemValue.CosmeticMods[i] != null && CurrentItem.itemValue.CosmeticMods[i].ItemClass != null && CurrentItem.itemValue.CosmeticMods[i].ItemClass is ItemClassModifier itemClassModifier2 && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
					{
						_resultStack = _partStack;
						return false;
					}
				}
			}
			else
			{
				int num = 0;
				for (int j = 0; j < CurrentItem.itemValue.Modifications.Length; j++)
				{
					if (CurrentItem.itemValue.Modifications[j] != null && CurrentItem.itemValue.Modifications[j].ItemClass != null && !itemClassModifier.HasAnyTags(EntityDrone.StorageModifierTags) && CurrentItem.itemValue.Modifications[j].ItemClass is ItemClassModifier itemClassModifier3 && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier3.ModifierTags))
					{
						num++;
					}
				}
				if (num >= itemClassModifier.MaxModsAllowed)
				{
					_resultStack = _partStack;
					return false;
				}
			}
		}
		if (CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
			{
				if (CurrentItem.itemValue.CosmeticMods != null)
				{
					for (int k = 0; k < CurrentItem.itemValue.CosmeticMods.Length; k++)
					{
						if (CurrentItem.itemValue.CosmeticMods[k] == null || CurrentItem.itemValue.CosmeticMods[k].IsEmpty())
						{
							float num2 = 1f - CurrentItem.itemValue.PercentUsesLeft;
							CurrentItem.itemValue.CosmeticMods[k] = _partStack.itemValue.Clone();
							if (CurrentItemStackController != null)
							{
								XUiC_AssembleWindowGroup.GetWindowGroup(CurrentItemStackController.xui).ItemStack = CurrentItem;
							}
							if (currentEquipmentStackController != null)
							{
								XUiC_AssembleWindowGroup.GetWindowGroup(CurrentEquipmentStackController.xui).ItemStack = CurrentItem;
							}
							RefreshAssembleItem();
							if (CurrentItem.itemValue.MaxUseTimes > 0)
							{
								CurrentItem.itemValue.UseTimes = (int)(num2 * (float)CurrentItem.itemValue.MaxUseTimes);
							}
							UpdateAssembleWindow();
							_resultStack = ItemStack.Empty;
							return true;
						}
					}
				}
			}
			else if (CurrentItem.itemValue.Modifications != null)
			{
				for (int l = 0; l < CurrentItem.itemValue.Modifications.Length; l++)
				{
					if (CurrentItem.itemValue.Modifications[l] == null || CurrentItem.itemValue.Modifications[l].IsEmpty())
					{
						float num3 = 1f - CurrentItem.itemValue.PercentUsesLeft;
						CurrentItem.itemValue.Modifications[l] = _partStack.itemValue.Clone();
						if (CurrentItemStackController != null)
						{
							XUiC_AssembleWindowGroup.GetWindowGroup(CurrentItemStackController.xui).ItemStack = CurrentItem;
						}
						if (currentEquipmentStackController != null)
						{
							XUiC_AssembleWindowGroup.GetWindowGroup(CurrentEquipmentStackController.xui).ItemStack = CurrentItem;
						}
						RefreshAssembleItem();
						if (CurrentItem.itemValue.MaxUseTimes > 0)
						{
							CurrentItem.itemValue.UseTimes = (int)(num3 * (float)CurrentItem.itemValue.MaxUseTimes);
						}
						UpdateAssembleWindow();
						_resultStack = ItemStack.Empty;
						return true;
					}
				}
			}
		}
		_resultStack = _partStack;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAssembleWindow()
	{
		if (AssembleWindow != null)
		{
			AssembleWindow.ItemStack = CurrentItem;
			AssembleWindow.OnChanged();
		}
	}
}
