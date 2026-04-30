using System.Collections.Generic;
using Challenges;
using GUI_2;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CategoryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string currentWorkstation = "*";

	public readonly List<XUiC_CategoryEntry> CategoryButtons = new List<XUiC_CategoryEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryEntry currentCategory;

	public bool AllowUnselect;

	public bool AllowKeyPaging = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HideGamepadCallouts;

	public XUiC_CategoryEntry CurrentCategory
	{
		get
		{
			return currentCategory;
		}
		set
		{
			if (currentCategory != null)
			{
				currentCategory.Selected = false;
			}
			currentCategory = value;
			if (currentCategory != null)
			{
				currentCategory.Selected = true;
				currentIndex = CategoryButtons.IndexOf(currentCategory);
			}
			IsDirty = true;
		}
	}

	public int MaxCategories => CategoryButtons.Count;

	public event XUiEvent_CategoryChangedEventHandler CategoryChanged;

	public event XUiEvent_CategoryChangedEventHandler CategoryClickChanged;

	public override void Init()
	{
		base.Init();
		GetChildrenByType(CategoryButtons);
		for (int i = 0; i < CategoryButtons.Count; i++)
		{
			CategoryButtons[i].CategoryList = this;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
		if (AllowKeyPaging && base.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			if (gUIActions.PageUp.WasReleased)
			{
				IncrementCategory(1);
			}
			else if (gUIActions.PageDown.WasReleased)
			{
				IncrementCategory(-1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void HandleCategoryChanged()
	{
		this.CategoryChanged?.Invoke(CurrentCategory);
		this.CategoryClickChanged?.Invoke(CurrentCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryEntry GetCategoryByName(string _category, out int _index)
	{
		_index = 0;
		for (int i = 0; i < CategoryButtons.Count; i++)
		{
			if (CategoryButtons[i].CategoryName == _category)
			{
				_index = i;
				return CategoryButtons[i];
			}
		}
		return null;
	}

	public XUiC_CategoryEntry GetCategoryByIndex(int _index)
	{
		if (_index >= CategoryButtons.Count)
		{
			return null;
		}
		return CategoryButtons[_index];
	}

	public void SetCategoryToFirst()
	{
		CurrentCategory = CategoryButtons[0];
		this.CategoryChanged?.Invoke(CurrentCategory);
	}

	public void SetCategory(string _category)
	{
		int _index;
		XUiC_CategoryEntry categoryByName = GetCategoryByName(_category, out _index);
		if (categoryByName != null || AllowUnselect)
		{
			CurrentCategory = categoryByName;
			this.CategoryChanged?.Invoke(CurrentCategory);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncrementCategory(int _offset)
	{
		if (_offset == 0)
		{
			return;
		}
		int i = 0;
		int num = NGUIMath.RepeatIndex(currentIndex + _offset, CategoryButtons.Count);
		XUiC_CategoryEntry xUiC_CategoryEntry = CategoryButtons[num];
		for (; i < CategoryButtons.Count; i++)
		{
			if (xUiC_CategoryEntry != null && !(xUiC_CategoryEntry.SpriteName == ""))
			{
				break;
			}
			num = NGUIMath.RepeatIndex((_offset > 0) ? (num + 1) : (num - 1), CategoryButtons.Count);
			xUiC_CategoryEntry = CategoryButtons[num];
		}
		if (xUiC_CategoryEntry != null && xUiC_CategoryEntry.SpriteName != "" && xUiC_CategoryEntry.ViewComponent.Enabled)
		{
			CurrentCategory = xUiC_CategoryEntry;
			xUiC_CategoryEntry.PlayButtonClickSound();
			HandleCategoryChanged();
		}
	}

	public void SetCategoryEmpty(int _index)
	{
		XUiC_CategoryEntry xUiC_CategoryEntry = CategoryButtons[_index];
		string text = (xUiC_CategoryEntry.SpriteName = "");
		string categoryDisplayName = (xUiC_CategoryEntry.CategoryName = text);
		xUiC_CategoryEntry.CategoryDisplayName = categoryDisplayName;
		xUiC_CategoryEntry.ViewComponent.IsVisible = false;
		xUiC_CategoryEntry.ViewComponent.IsNavigatable = false;
	}

	public void SetCategoryEntry(int _index, string _categoryName, string _spriteName, string _displayName = null)
	{
		XUiC_CategoryEntry xUiC_CategoryEntry = CategoryButtons[_index];
		xUiC_CategoryEntry.CategoryDisplayName = _displayName ?? _categoryName;
		xUiC_CategoryEntry.CategoryName = _categoryName;
		xUiC_CategoryEntry.SpriteName = _spriteName ?? "";
		xUiC_CategoryEntry.ViewComponent.IsVisible = true;
		xUiC_CategoryEntry.ViewComponent.IsNavigatable = true;
		xUiC_CategoryEntry.ViewComponent.Enabled = true;
		xUiC_CategoryEntry.ViewComponent.DisabledToolTip = null;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!HideGamepadCallouts)
		{
			base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!HideGamepadCallouts)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "allow_unselect":
			AllowUnselect = StringParsers.ParseBool(_value);
			return true;
		case "allow_key_paging":
			AllowKeyPaging = StringParsers.ParseBool(_value);
			return true;
		case "hide_gamepad_callouts":
			HideGamepadCallouts = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "currentcategoryicon"))
		{
			if (_bindingName == "currentcategorydisplayname")
			{
				_value = CurrentCategory?.CategoryDisplayName ?? "";
				return true;
			}
			return false;
		}
		_value = CurrentCategory?.SpriteName ?? "";
		return true;
	}

	public bool SetupCategoriesByWorkstation(string _workstation)
	{
		if (currentWorkstation != _workstation)
		{
			currentWorkstation = _workstation;
			if (_workstation == "skills")
			{
				int num = 0;
				foreach (KeyValuePair<string, ProgressionClass> progressionClass in Progression.ProgressionClasses)
				{
					if (progressionClass.Value.IsAttribute)
					{
						SetCategoryEntry(num, progressionClass.Value.Name, progressionClass.Value.Icon, Localization.Get(progressionClass.Value.Name));
						num++;
					}
				}
			}
			else
			{
				List<CraftingCategoryDisplayEntry> craftingCategoryDisplayList = UIDisplayInfoManager.Current.GetCraftingCategoryDisplayList(_workstation);
				if (craftingCategoryDisplayList != null)
				{
					int num2 = 0;
					for (int i = 0; i < craftingCategoryDisplayList.Count && i < CategoryButtons.Count; i++)
					{
						SetCategoryEntry(num2, craftingCategoryDisplayList[i].Name, craftingCategoryDisplayList[i].Icon, craftingCategoryDisplayList[i].DisplayName);
						num2++;
					}
					for (int j = num2; j < CategoryButtons.Count; j++)
					{
						SetCategoryEmpty(num2++);
					}
				}
			}
			return true;
		}
		return false;
	}

	public bool SetupCategoriesBasedOnItems(List<ItemStack> _items, int _traderStage)
	{
		List<string> list = new List<string>();
		SetCategoryEntry(0, "", "ui_game_symbol_shopping_cart", Localization.Get("lblAll"));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			ItemClass itemClass = _items[i].itemValue.ItemClass;
			TraderStageTemplateGroup traderStageTemplateGroup = null;
			if (itemClass.TraderStageTemplate != null && TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
			{
				traderStageTemplateGroup = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
			}
			if (traderStageTemplateGroup != null && !traderStageTemplateGroup.IsWithin(_traderStage, _items[i].itemValue.Quality))
			{
				continue;
			}
			string[] array = itemClass.Groups;
			if (itemClass.IsBlock())
			{
				array = Block.list[_items[i].itemValue.type].GroupNames;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (!list.Contains(array[j]))
				{
					CraftingCategoryDisplayEntry traderCategoryDisplay = UIDisplayInfoManager.Current.GetTraderCategoryDisplay(array[j]);
					if (traderCategoryDisplay != null)
					{
						int count = list.Count;
						SetCategoryEntry(count, traderCategoryDisplay.Name, traderCategoryDisplay.Icon, traderCategoryDisplay.DisplayName);
						list.Add(array[j]);
					}
				}
			}
		}
		for (int k = list.Count; k < CategoryButtons.Count; k++)
		{
			SetCategoryEmpty(k);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchCategories(List<TwitchActionManager.ActionCategory> _items)
	{
		List<string> list = new List<string>();
		SetCategoryEntry(0, "", "ui_game_symbol_twitch_actions", Localization.Get("lblAll"));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			TwitchActionManager.ActionCategory actionCategory = _items[i];
			if (actionCategory.Icon != "")
			{
				SetCategoryEntry(list.Count, actionCategory.Name, actionCategory.Icon, actionCategory.Name);
				list.Add(actionCategory.Name);
			}
		}
		for (int j = list.Count; j < CategoryButtons.Count; j++)
		{
			SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchActions(List<TwitchAction> _items)
	{
		List<string> list = new List<string>();
		SetCategoryEntry(0, "", "ui_game_symbol_twitch_actions", Localization.Get("lblAll"));
		list.Add("");
		Dictionary<TwitchActionManager.ActionCategory, int> dictionary = new Dictionary<TwitchActionManager.ActionCategory, int>();
		List<TwitchActionManager.ActionCategory> categoryList = TwitchActionManager.Current.CategoryList;
		for (int i = 0; i < categoryList.Count; i++)
		{
			dictionary.Add(categoryList[i], 0);
		}
		for (int j = 0; j < _items.Count; j++)
		{
			TwitchAction twitchAction = _items[j];
			if (twitchAction.DisplayCategory != null)
			{
				dictionary[twitchAction.DisplayCategory]++;
			}
		}
		foreach (TwitchActionManager.ActionCategory key in dictionary.Keys)
		{
			if (dictionary[key] > 0)
			{
				SetCategoryEntry(list.Count, key.Name, key.Icon, key.DisplayName);
				list.Add(key.Name);
			}
		}
		for (int k = list.Count; k < CategoryButtons.Count; k++)
		{
			SetCategoryEmpty(k);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnTwitchVoteCategories(List<TwitchVoteType> _items)
	{
		List<string> list = new List<string>();
		SetCategoryEntry(0, "", "ui_game_symbol_twitch_vote", Localization.Get("lblAll"));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			TwitchVoteType twitchVoteType = _items[i];
			if (twitchVoteType.Icon != "")
			{
				SetCategoryEntry(list.Count, twitchVoteType.Name, twitchVoteType.Icon, twitchVoteType.Title);
				list.Add(twitchVoteType.Name);
			}
		}
		for (int j = list.Count; j < CategoryButtons.Count; j++)
		{
			SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnGameEventCategories(List<GameEventManager.Category> _items)
	{
		List<string> list = new List<string>();
		SetCategoryEntry(0, "", "ui_game_symbol_airdrop", Localization.Get("lblAll"));
		list.Add("");
		for (int i = 0; i < _items.Count; i++)
		{
			GameEventManager.Category category = _items[i];
			if (category.Icon != "")
			{
				SetCategoryEntry(list.Count, category.Name, category.Icon, category.Name);
				list.Add(category.Name);
			}
		}
		for (int j = list.Count; j < CategoryButtons.Count; j++)
		{
			SetCategoryEmpty(j);
		}
		return true;
	}

	public bool SetupCategoriesBasedOnChallengeCategories(List<ChallengeCategory> _items)
	{
		List<string> list = new List<string>();
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		for (int i = 0; i < _items.Count; i++)
		{
			ChallengeCategory challengeCategory = _items[i];
			if (challengeCategory.Icon != "" && challengeCategory.CanShow(entityPlayer))
			{
				SetCategoryEntry(list.Count, challengeCategory.Name, challengeCategory.Icon, challengeCategory.Title);
				list.Add(challengeCategory.Name);
			}
		}
		for (int j = list.Count; j < CategoryButtons.Count; j++)
		{
			SetCategoryEmpty(j);
		}
		return true;
	}
}
