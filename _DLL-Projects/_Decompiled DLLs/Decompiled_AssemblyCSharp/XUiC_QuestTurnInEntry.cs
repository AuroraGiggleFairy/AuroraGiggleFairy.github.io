using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInEntry : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BaseReward reward;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool chosen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor recipeicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor hasingredientsstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor unlockstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilityColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilityValueFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor rewardiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor rewardchosencolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public Color defaultColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectedColor;

	public BaseReward Reward
	{
		get
		{
			return reward;
		}
		set
		{
			reward = value;
			Refresh();
			if (!base.Selected)
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
	}

	public ItemStack Item
	{
		get
		{
			if (reward != null && item.IsEmpty())
			{
				if (reward is RewardItem)
				{
					item = (reward as RewardItem).Item;
				}
				else if (reward is RewardLootItem)
				{
					item = (reward as RewardLootItem).Item;
				}
			}
			return item;
		}
	}

	public bool Chosen
	{
		get
		{
			return chosen;
		}
		set
		{
			chosen = value;
			RefreshBackground();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		RefreshBackground();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshBackground()
	{
		if (background != null)
		{
			background.Color = (base.Selected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			background.SpriteName = (base.Selected ? "ui_game_select_row" : "menu_empty");
		}
	}

	public void SetBaseReward(BaseReward reward)
	{
		Reward = reward;
		item = ItemStack.Empty;
		isDirty = true;
		RefreshBindings();
		base.ViewComponent.Enabled = reward != null;
		if (reward == null)
		{
			background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiView xUiView = children[i].ViewComponent;
			if (xUiView.ID.EqualsCaseInsensitive("background"))
			{
				background = xUiView as XUiV_Sprite;
			}
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isHovered = _isOver;
		if (background != null && reward != null && !base.Selected)
		{
			if (_isOver)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "hasreward":
			value = (reward != null).ToString();
			return true;
		case "rewardname":
			value = ((reward != null) ? reward.GetRewardText() : "");
			return true;
		case "rewardicon":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					value = item.itemValue.ItemClass.GetIconName();
				}
				else
				{
					value = reward.Icon;
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "hasitemreward":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					value = "true";
				}
				else
				{
					value = "false";
				}
			}
			else
			{
				value = "false";
			}
			return true;
		case "itemtypeicon":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					if (item.itemValue.ItemClass.IsBlock())
					{
						value = Block.list[item.itemValue.type].ItemTypeIcon;
					}
					else
					{
						if (item.itemValue.ItemClass.AltItemTypeIcon != null && item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, item.itemValue.ItemClass, item.itemValue))
						{
							value = item.itemValue.ItemClass.AltItemTypeIcon;
							return true;
						}
						value = item.itemValue.ItemClass.ItemTypeIcon;
					}
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "hasitemtypeicon":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					if (item.itemValue.ItemClass.IsBlock())
					{
						value = (Block.list[item.itemValue.type].ItemTypeIcon != "").ToString();
					}
					else
					{
						value = (item.itemValue.ItemClass.ItemTypeIcon != "").ToString();
					}
				}
				else
				{
					value = "false";
				}
			}
			else
			{
				value = "false";
			}
			return true;
		case "itemtypeicontint":
			value = "255,255,255,255";
			if (item != null && !item.IsEmpty() && item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, item.itemValue.ItemClass, item.itemValue))
			{
				value = altitemtypeiconcolorFormatter.Format(item.itemValue.ItemClass.AltItemTypeIconColor);
			}
			return true;
		case "hasotherreward":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					value = "false";
				}
				else
				{
					value = "true";
				}
			}
			else
			{
				value = "true";
			}
			return true;
		case "rewardicontint":
			if (reward != null)
			{
				if (item != null && !item.IsEmpty())
				{
					value = rewardiconcolorFormatter.Format(item.itemValue.ItemClass.GetIconTint(item.itemValue));
				}
				else
				{
					value = "[iconColor]";
				}
			}
			else
			{
				value = "[iconColor]";
			}
			return true;
		case "chosenicon":
			value = ((reward != null && chosen) ? "ui_game_symbol_check" : "");
			return true;
		case "namecolor":
			value = ((reward != null && chosen) ? rewardchosencolorFormatter.Format(selectedColor) : rewardchosencolorFormatter.Format(defaultColor));
			return true;
		case "hasdurability":
			value = (Item != null && !Item.IsEmpty() && Item.itemValue.ItemClass.ShowQualityBar).ToString();
			return true;
		case "durabilitycolor":
			if (Item != null && !Item.IsEmpty() && Item.itemValue.ItemClass.ShowQualityBar)
			{
				Color32 v = QualityInfo.GetTierColor(Item.itemValue.Quality);
				value = durabilityColorFormatter.Format(v);
			}
			else
			{
				value = "0,0,0,0";
			}
			return true;
		case "durabilityvalue":
			value = "";
			if (Item != null && !Item.IsEmpty())
			{
				if (Item.itemValue.HasQuality || (!Item.IsEmpty() && Item.itemValue.ItemClass.HasSubItems))
				{
					value = ((Item.itemValue.Quality > 0) ? durabilityValueFormatter.Format(Item.itemValue.Quality) : "-");
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "durabilityfill":
			value = ((Item == null || Item.IsEmpty()) ? "0" : ((Item.itemValue.MaxUseTimes == 0) ? "1" : durabilityFillFormatter.Format(Item.itemValue.PercentUsesLeft)));
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "default_color"))
			{
				if (!(name == "selected_color"))
				{
					return false;
				}
				selectedColor = StringParsers.ParseColor32(value);
			}
			else
			{
				defaultColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	public void Refresh()
	{
		isDirty = true;
		RefreshBindings();
	}
}
