using System;
using System.Collections.Generic;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxOptions : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class TabChangeHelper
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_TabSelectorTab tab;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_SandBoxOptionEntry[] tabOptions;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string localizationKey;

		public TabChangeHelper(XUiC_TabSelectorTab _tab, XUiC_SandBoxOptionEntry[] _tabOptions, string _localizationKey)
		{
			tab = _tab;
			tabOptions = _tabOptions;
			localizationKey = _localizationKey;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasAnyNonDefault()
		{
			for (int i = 0; i < tabOptions.Length; i++)
			{
				if (!tabOptions[i].IsDefaultValue())
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasAnyUnsavedChange()
		{
			for (int i = 0; i < tabOptions.Length; i++)
			{
				if (tabOptions[i].IsChanged)
				{
					return true;
				}
			}
			return false;
		}

		public void UpdateTab()
		{
			tab.TabHighlight = hasAnyNonDefault();
			tab.TabHeaderText = Localization.Get(localizationKey) + (hasAnyUnsavedChange() ? "[FF]" : "[00]") + "*[FF]";
		}
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector tabSelector;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelectorTab[] tabs;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxPresetSelector presetSelector;

	[XuiBindComponent("btnSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSavePreset;

	[XuiBindComponent("btnCopyCode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopyCode;

	[XuiBindComponent("btnCopy", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopy;

	[XuiBindComponent("btnCreatePreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCreatePreset;

	[XuiBindComponent("btnDelete", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDelete;

	[XuiBindComponent("btnPasteCode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnPasteCode;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnDefaults", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry> sandboxOptionDict = new Dictionary<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, TabChangeHelper> tabHelpers = new Dictionary<string, TabChangeHelper>();

	[PublicizedFrom(EAccessModifier.Private)]
	public SandboxPresetInfo currentPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClose;

	[XuiXmlBinding("gamename")]
	public string GameName
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!string.IsNullOrEmpty(sandboxManager?.GameName))
			{
				return sandboxManager.GameName;
			}
			return "New World";
		}
	}

	[XuiXmlBinding("worldname")]
	public string WorldName
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return sandboxManager?.WorldName ?? "";
		}
	}

	[XuiXmlBinding("is_custom")]
	public bool CurrentIsCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentPreset.IsCustomPreset;
		}
	}

	[XuiXmlBinding("is_userpreset")]
	public bool CurrentIsUserPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentPreset.IsUserPreset;
		}
	}

	[XuiXmlBinding("has_changes")]
	public bool HasChanges
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			foreach (KeyValuePair<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry> item in sandboxOptionDict)
			{
				item.Deconstruct(out var _, out var value);
				if (value.IsChanged)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XuiXmlBinding("sandboxcode")]
	public string SandBoxCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (string.IsNullOrEmpty(currentPreset.SandboxCode))
			{
				return "";
			}
			return currentPreset.SandboxCode;
		}
	}

	public override void Init()
	{
		base.Init();
		setupOptions();
	}

	[XuiBindEvent("OnTabChanged", "tabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TabSelector_OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		resetTabHeaders();
	}

	[XuiBindEvent("OnPress", "btnCopyCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopyCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		updateCode();
		GUIUtility.systemCopyBuffer = currentPreset.SandboxCode;
	}

	[XuiBindEvent("OnPress", "btnPasteCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPasteCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (sandboxManager.LoadOptionsFromCode(GUIUtility.systemCopyBuffer, SandboxOptionManager.CustomPreset))
		{
			presetSelector.SelectPresetByName("Custom");
			IsDirty = true;
		}
	}

	[XuiBindEvent("OnPress", "btnCopy")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		duplicateCurrentSettingsToCustomPreset();
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnCreatePreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCreatePreset_OnPressed(XUiController _sender, int _mouseButton)
	{
		sandboxManager.ResetAllToDefault();
		sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		presetSelector.SelectPresetByName("Custom");
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnSavePreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnSavePreset.ViewComponent.Enabled)
		{
			return;
		}
		applyEntryChanges(_forceChange: true);
		if (currentPreset.IsCustomPreset)
		{
			SandboxOptionPreset sandboxOptionPreset = sandboxManager.SaveCurrentToNewPreset("tempPreset", "", isUserPreset: true);
			XUiC_SandboxSettingsSaveAsPreset.Open(xui, sandboxOptionPreset.SandboxCode, "", [PublicizedFrom(EAccessModifier.Private)] (string _presetName) =>
			{
				presetSelector.SelectPresetByName(_presetName);
				IsDirty = true;
			});
		}
		else
		{
			SandboxOptionPreset preset = sandboxManager.GetPreset(currentPreset.InternalName);
			sandboxManager.SaveCurrentToPreset(preset);
			sandboxManager.SavePreset(preset, addToDictionary: true, saveToFile: true);
			updateOptionsToPreset(preset);
			currentPreset.SandboxCode = preset.SandboxCode;
			applyPresetSettings();
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyPresetSettings()
	{
		GamePrefs.Set(EnumGamePrefs.SandboxPreset, currentPreset.InternalName);
		GamePrefs.Set(EnumGamePrefs.SandboxCode, currentPreset.IsCustomPreset ? currentPreset.SandboxCode : "");
	}

	[XuiBindEvent("OnPress", "btnDelete")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowCustom(xui, Localization.Get("xuiDeleteSandboxPreset"), string.Format(Localization.Get("xuiDeleteSandboxConfirmation"), currentPreset.FormattedName), [PublicizedFrom(EAccessModifier.Private)] (XUiC_MessageBoxWindowGroup _box) =>
		{
			_box.Buttons[0].DefaultConfirm("btnConfirm", [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				sandboxManager.DeletePreset(currentPreset.InternalName);
				presetSelector.SelectPresetByName("", "User");
				IsDirty = true;
			}, _enabled: true, 0f, 1.5f);
			_box.Buttons[2].DefaultCancel("xuiCancel", [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
			});
		}, _openMainMenuOnClose: false, _modal: false);
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		foreach (var (_, xUiC_SandBoxOptionEntry2) in sandboxOptionDict)
		{
			if (!(xUiC_SandBoxOptionEntry2.Option.CategoryName != tabSelector.SelectedTab.TabKey))
			{
				xUiC_SandBoxOptionEntry2.ResetToDefault(_storeAsOriginal: false);
			}
		}
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("SandboxPresetSelectionChanged", "presetSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Presets_OnValueChanged(SandboxPresetInfo _oldValue, SandboxPresetInfo _newValue)
	{
		SandboxOptionPreset preset = sandboxManager.GetPreset(_newValue.InternalName);
		if (_oldValue.IsCustomPreset)
		{
			applyEntryChanges(_forceChange: false);
			sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		}
		updateOptionsToPreset(preset);
		currentPreset = _newValue;
		updateCode();
		applyPresetSettings();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SandboxOptionEntry_OnChangeHandler(XUiC_SandBoxOptionEntry _entry, global::SandboxOptions.SandboxOptions _option)
	{
		if (!currentPreset.IsUserPreset && !currentPreset.IsCustomPreset)
		{
			XUiC_SandBoxOptionEntry.SandboxOptionValue originalValue = _entry.OriginalValue;
			duplicateCurrentSettingsToCustomPreset();
			_entry.ForceOriginalValue(originalValue);
		}
		setEntriesDirty();
		IsDirty = true;
		resetTabHeaders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void duplicateCurrentSettingsToCustomPreset()
	{
		applyEntryChanges(_forceChange: true);
		sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		presetSelector.SelectPresetByName("Custom");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCode()
	{
		applyEntryChanges(_forceChange: true);
		SandboxOptionPreset sandboxOptionPreset = sandboxManager.SaveCurrentToNewPreset("temppreset", "temp");
		currentPreset.SandboxCode = sandboxOptionPreset.SandboxCode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		DictionaryList<string, List<BaseSandboxOption>> optionsByCategory = sandboxManager.OptionsByCategory;
		for (int i = 0; i < tabs.Length; i++)
		{
			XUiC_TabSelectorTab xUiC_TabSelectorTab = tabs[i];
			if (i >= optionsByCategory.Count)
			{
				xUiC_TabSelectorTab.TabVisible = false;
				continue;
			}
			List<BaseSandboxOption> list = optionsByCategory.list[i];
			if (list.Count == 0)
			{
				xUiC_TabSelectorTab.TabVisible = false;
				continue;
			}
			string text = (xUiC_TabSelectorTab.TabKey = list[0].CategoryName);
			XUiC_SandBoxOptionEntry[] childControllers = xUiC_TabSelectorTab.GetChildControllers<XUiC_SandBoxOptionEntry>("");
			tabHelpers.Add(text, new TabChangeHelper(xUiC_TabSelectorTab, childControllers, "sandboxOptionCategory" + text));
			int num = -1;
			if (xUiC_TabSelectorTab.TryGetChildView<XUiV_Grid>("", out var _child) && _child.Arrangement == UIGrid.Arrangement.Horizontal)
			{
				num = _child.Columns;
			}
			if (list.Count > childControllers.Length)
			{
				Log.Error($"[XUi] More sandbox options ({list.Count}) in category '{text}' than XUiC_SandBoxOptionEntries in respective tab ({childControllers.Length})!");
			}
			int j = 0;
			for (int k = 0; k < list.Count; k++)
			{
				if (j >= childControllers.Length)
				{
					break;
				}
				BaseSandboxOption baseSandboxOption = list[k];
				if (baseSandboxOption.NewUISection)
				{
					if (num <= 0)
					{
						childControllers[j++].setupSeparator();
					}
					else
					{
						int num2 = j % num;
						if (num2 > 0)
						{
							int num3 = num - num2;
							for (int l = 0; l < num3; l++)
							{
								childControllers[j++].setupSeparator();
							}
						}
						for (int m = 0; m < num; m++)
						{
							childControllers[j++].setupSeparator();
						}
					}
				}
				XUiC_SandBoxOptionEntry xUiC_SandBoxOptionEntry = childControllers[j++];
				xUiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(xUiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
				xUiC_SandBoxOptionEntry.setupOption(baseSandboxOption);
				xUiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(xUiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
				sandboxOptionDict.Add(xUiC_SandBoxOptionEntry.Option.Option, xUiC_SandBoxOptionEntry);
			}
			for (; j < childControllers.Length; j++)
			{
				childControllers[j].setupOption(null);
			}
		}
		resetTabHeaders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyEntryChanges(bool _forceChange)
	{
		foreach (XUiC_SandBoxOptionEntry value in sandboxOptionDict.Values)
		{
			value.ApplyChange(_forceChange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setEntriesDirty()
	{
		foreach (XUiC_SandBoxOptionEntry value in sandboxOptionDict.Values)
		{
			value.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptionsToPreset(SandboxOptionPreset _preset)
	{
		foreach (XUiC_SandBoxOptionEntry value in sandboxOptionDict.Values)
		{
			value.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(value.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
			value.ResetToDefault(_storeAsOriginal: true);
			value.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(value.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
		}
		foreach (global::SandboxOptions.SandboxOptions key in _preset.PresetValues.Keys)
		{
			XUiC_SandBoxOptionEntry xUiC_SandBoxOptionEntry = sandboxOptionDict[key];
			xUiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(xUiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
			xUiC_SandBoxOptionEntry.SetComboBoxIndex(_preset.PresetValues[key]);
			xUiC_SandBoxOptionEntry.ApplyChange(_forceChange: true);
			xUiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(xUiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(SandboxOptionEntry_OnChangeHandler));
		}
		resetTabHeaders();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetTabHeaders()
	{
		foreach (TabChangeHelper value in tabHelpers.Values)
		{
			value.UpdateTab();
		}
	}

	public override void OnOpen()
	{
		windowGroup.isEscClosable = false;
		string presetName = GamePrefs.GetString(EnumGamePrefs.SandboxPreset);
		base.OnOpen();
		presetSelector.SelectPresetByName(presetName, null, _forceChangedCallback: true);
		resetTabHeaders();
		tabSelector.SelectedTabIndex = 0;
	}

	public override void OnClose()
	{
		base.OnClose();
		onClose?.Invoke();
		onClose = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				BtnBack_OnPressed(this, -1);
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				BtnSave_OnPressed(null, 0);
			}
		}
		if (IsDirty)
		{
			updateCode();
		}
		handleDirtyUpdateDefault();
	}

	public static void Open(XUi _xui, Action _onClose)
	{
		XUiC_SandboxOptions childByType = _xui.GetChildByType<XUiC_SandboxOptions>();
		childByType.onClose = _onClose;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
