using System;
using System.Collections.Generic;

public abstract class XUiC_OptionsDialogBase : XUiController, IXUiWindowConditionalClosing
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]> optionsByTab = new Dictionary<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool unsavedChanges;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_TabSelector TabSelector;

	[XuiBindComponent("buttons.btnApply", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnApply;

	[XuiBindComponent("buttons.btnDefaults", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[XuiBindComponent("buttons.btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_OptionEntryAbs[] AllOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionEntryAbs hoveredOption;

	[XuiXmlBinding("unsaved_changes")]
	public bool UnsavedChanges
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return unsavedChanges;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			unsavedChanges = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("support_defaults")]
	public virtual bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("allow_leaving_window")]
	public virtual bool AllowLeavingWindow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public XUiC_OptionEntryAbs HoveredOption
	{
		get
		{
			return hoveredOption;
		}
		set
		{
			if (hoveredOption != value)
			{
				hoveredOption = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("hovered_custom_attributes")]
	[PublicizedFrom(EAccessModifier.Private)]
	public ObservableDictionary<string, object> bindingCustomAttributes()
	{
		return HoveredOption?.CustomAttributes ?? CustomAttributes;
	}

	public override void Init()
	{
		base.Init();
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		if (TabSelector != null)
		{
			XUiC_TabSelectorTab[] tabs = TabSelector.Tabs;
			foreach (XUiC_TabSelectorTab xUiC_TabSelectorTab in tabs)
			{
				optionsByTab.Add(xUiC_TabSelectorTab, xUiC_TabSelectorTab.GetChildControllers<XUiC_OptionEntryAbs>(""));
			}
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
	}

	public override void OnOpen()
	{
		unsavedChanges = false;
		base.OnOpen();
		XUi.InGameMenuOpen = true;
		XUiC_OptionsMenuNew.ParentSelector.SetSelected(windowGroup.Id);
		xui.playerUI.windowManager.Open(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, _bModal: false);
		windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUi.InGameMenuOpen = false;
		xui.playerUI.windowManager.Close(XUiC_OptionsMenuNew.ParentSelector.WindowGroup);
		if (GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			onDirtyUpdate();
			RefreshBindings();
			IsDirty = false;
		}
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				TryClose(null, null);
			}
			if (UnsavedChanges && xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				saveChanges();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onDirtyUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGamePrefChanged(EnumGamePrefs _pref)
	{
	}

	public virtual void TryClose(Action _onClosed, Action _onCancelled)
	{
		if (!unsavedChanges)
		{
			closeOptions();
			_onClosed?.Invoke();
			return;
		}
		XUiC_MessageBoxWindowGroup.ShowCustom(xui, Localization.Get("xuiUnsavedChangesTitle"), Localization.Get("xuiOptionsUnsavedChangesText"), [PublicizedFrom(EAccessModifier.Internal)] (XUiC_MessageBoxWindowGroup _box) =>
		{
			_box.Buttons[0].DefaultConfirm("xuiDiscard", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				closeOptions();
				_onClosed?.Invoke();
			});
			_box.Buttons[1].Set("xuiSaveAndClose", xui.playerUI.playerInput.GUIActions.HalfStack, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				saveChanges();
				closeOptions();
				_onClosed?.Invoke();
			});
			_box.Buttons[2].DefaultCancel("xuiCancel", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				_onCancelled?.Invoke();
			});
		}, _openMainMenuOnClose: false, _modal: false);
	}

	[XuiBindEvent("ValueChanged", "AllOptions")]
	public void Event_SettingChanged(XUiC_OptionEntryAbs _sender)
	{
		XUiC_OptionEntryAbs.DebugLog("SETTING CHANGED: " + _sender.GetXuiHierarchy());
		updateUnsavedState();
		RefreshBindingsSelfAndChildren();
	}

	[XuiBindEvent("OnTabChanged", "TabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnApply")]
	public void Event_ApplyOnPress(XUiController _sender, int _mouseButton)
	{
		saveChanges();
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	public void Event_DefaultsOnPress(XUiController _sender, int _mouseButton)
	{
		resetToDefaults();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	public void Event_BackOnPress(XUiController _sender, int _mouseButton)
	{
		TryClose(null, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUnsavedState()
	{
		bool flag = false;
		XUiC_OptionEntryAbs[] value = AllOptions;
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i].IsChanged)
			{
				flag = true;
				break;
			}
		}
		UnsavedChanges = flag;
		if (TabSelector == null)
		{
			return;
		}
		foreach (KeyValuePair<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]> item in optionsByTab)
		{
			item.Deconstruct(out var key, out value);
			XUiC_TabSelectorTab xUiC_TabSelectorTab = key;
			XUiC_OptionEntryAbs[] array = value;
			bool tabHighlight = false;
			value = array;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].IsChanged)
				{
					tabHighlight = true;
					break;
				}
			}
			xUiC_TabSelectorTab.TabHighlight = tabHighlight;
		}
	}

	public void SetChanged()
	{
		UnsavedChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void closeOptions()
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveChanges()
	{
		Log.Out("Save options " + GetType().Name);
		XUiC_OptionEntryAbs[] allOptions = AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			allOptions[i].ApplySelection();
		}
		doSaveChangesInternal();
		GamePrefs.Instance.Save();
		afterChangesSaved();
		updateUnsavedState();
		if (unsavedChanges)
		{
			Log.Warning("UnsavedChanges after saving settings!");
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doSaveChangesInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void afterChangesSaved()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discardChanges()
	{
		Log.Out("Discard changes " + GetType().Name);
		XUiC_OptionEntryAbs[] allOptions = AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			allOptions[i].DiscardCurrentChange();
		}
		doDiscardChangesInternal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doDiscardChangesInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetToDefaults()
	{
		Log.Out("Reset to defaults " + GetType().Name);
		XUiC_OptionEntryAbs[] allOptions = AllOptions;
		foreach (XUiC_OptionEntryAbs xUiC_OptionEntryAbs in allOptions)
		{
			if (xUiC_OptionEntryAbs.ViewComponent.IsActiveInHierarchy)
			{
				xUiC_OptionEntryAbs.ResetToDefault();
			}
		}
		doResetToDefaultsInternal();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doResetToDefaultsInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionsDialogBase()
	{
	}
}
