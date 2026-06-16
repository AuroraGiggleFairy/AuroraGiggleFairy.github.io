using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsController : XUiC_OptionsControlsBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct ControllerLabelMapping
	{
		public readonly InputControlType[] ControlTypes;

		public readonly XUiV_Label[] Labels;

		public ControllerLabelMapping(string[] _controlTypeNames, XUiV_Label[] _labels)
		{
			ControlTypes = new InputControlType[_controlTypeNames.Length];
			for (int i = 0; i < _controlTypeNames.Length; i++)
			{
				ControlTypes[i] = EnumUtils.Parse<InputControlType>(_controlTypeNames[i], _ignoreCase: true);
			}
			Labels = _labels;
		}

		public ControllerLabelMapping(string _controlTypeName, XUiV_Label[] _labels)
		{
			ControlTypes = new InputControlType[1] { EnumUtils.Parse<InputControlType>(_controlTypeName, _ignoreCase: true) };
			Labels = _labels;
		}
	}

	[XuiBindComponent("ShowDS4", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool comboShowDS4;

	[XuiBindComponent("ShowBindingsFor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboShowBindingsFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetInGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetVehicles;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetPermanent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ControllerLabelMapping> labelsForControllers = new List<ControllerLabelMapping>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerLabelMapping leftStickLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerLabelMapping rightStickLabel;

	public static string ID = "";

	[XuiBindComponent("ControllerIconStyle", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboIconStyle;

	[XuiXmlBinding("isds4")]
	public bool ShowDs4
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!DeviceFlag.PS5.IsCurrent())
			{
				if (comboShowDS4 != null)
				{
					return comboShowDS4.Value;
				}
				return false;
			}
			return true;
		}
	}

	[XuiXmlBinding("isxb1")]
	public bool ShowXb1
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				if (comboShowDS4 != null)
				{
					return !comboShowDS4.Value;
				}
				return false;
			}
			return true;
		}
	}

	[XuiXmlBinding("controller_art")]
	public string ControllerArtSprite
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!ShowDs4)
			{
				return "Controller_Art_XB";
			}
			return "Controller_Art_PS5";
		}
	}

	[XuiXmlBinding("controller_lines")]
	public string ControllerLinesSprite
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!ShowDs4)
			{
				return "Controller_Lines_XB";
			}
			return "Controller_Lines_PS5";
		}
	}

	[XuiXmlBinding("cursor_sensitivity_min")]
	public float CursorSensitivityMin => 0.1f;

	[XuiXmlBinding("cursor_sensitivity_max")]
	public float CursorSensitivityMax => 1f;

	[XuiXmlBinding("controller_sensitivity_min")]
	public float ControllerSensitivityMin => 0.05f;

	[XuiXmlBinding("controller_sensitivity_max")]
	public float ControllerSensitivityMax => 1f;

	[XuiXmlBinding("controller_modifier_sensitivity_max")]
	public float ControllerModifierSensitivityMax => 2f;

	[XuiBindEvent("OnValueChanged", "comboShowDS4")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboShowDs4_OnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		IsDirty = true;
		updateControllerMappingLabels();
	}

	[XuiBindEvent("OnValueChanged", "comboShowBindingsFor")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboShowMenuBindings_OnOnValueChanged(XUiController _sender, string _s, string _newValue1)
	{
		IsDirty = true;
		updateControllerMappingLabels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initControllerLayout()
	{
		actionSetInGame = xui.playerUI.playerInput;
		actionSetVehicles = xui.playerUI.playerInput.VehicleActions;
		actionSetMenu = xui.playerUI.playerInput.GUIActions;
		actionSetPermanent = xui.playerUI.playerInput.PermanentActions;
		TryGetChildController<XUiController>("controllerlayout", out var _child);
		addControllerLabelMappingsForButton(_child, "Menu");
		addControllerLabelMappingsForButton(_child, "RightTrigger");
		addControllerLabelMappingsForButton(_child, "RightBumper");
		addControllerLabelMappingsForButton(_child, "Action4");
		addControllerLabelMappingsForButton(_child, "Action3");
		addControllerLabelMappingsForButton(_child, "Action2");
		addControllerLabelMappingsForButton(_child, "Action1");
		addControllerLabelMappingsForButton(_child, "RightStickButton");
		addControllerLabelMappingsForButton(_child, "View");
		addControllerLabelMappingsForButton(_child, "LeftTrigger");
		addControllerLabelMappingsForButton(_child, "LeftBumper");
		addControllerLabelMappingsForButton(_child, "LeftStickButton");
		addControllerLabelMappingsForButton(_child, "DPadUp");
		addControllerLabelMappingsForButton(_child, "DPadLeft");
		addControllerLabelMappingsForButton(_child, "DPadDown");
		addControllerLabelMappingsForButton(_child, "DPadRight");
		assignControllerLabelMappingsForButton(_child, "LeftStick", out leftStickLabel, new string[4] { "LeftStickLeft", "LeftStickRight", "LeftStickUp", "LeftStickDown" });
		assignControllerLabelMappingsForButton(_child, "RightStick", out rightStickLabel, new string[4] { "RightStickLeft", "RightStickRight", "RightStickUp", "RightStickDown" });
		updateControllerMappingLabels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addControllerLabelMappingsForButton(XUiController _layoutParent, string _uiName, string[] _controlTypeNames = null)
	{
		XUiV_Label[] childViews = _layoutParent.GetChildViews<XUiV_Label>(_uiName);
		if (_controlTypeNames == null)
		{
			labelsForControllers.Add(new ControllerLabelMapping(_uiName, childViews));
		}
		else
		{
			labelsForControllers.Add(new ControllerLabelMapping(_controlTypeNames, childViews));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void assignControllerLabelMappingsForButton(XUiController _layoutParent, string _uiName, out ControllerLabelMapping _assignTo, string[] _controlTypeNames = null)
	{
		XUiV_Label[] childViews = _layoutParent.GetChildViews<XUiV_Label>(_uiName);
		if (_controlTypeNames == null)
		{
			_assignTo = new ControllerLabelMapping(_uiName, childViews);
		}
		else
		{
			_assignTo = new ControllerLabelMapping(_controlTypeNames, childViews);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateControllerMappingLabels()
	{
		List<PlayerAction> list = new List<PlayerAction>();
		int selectedIndex = comboShowBindingsFor.SelectedIndex;
		PlayerActionsBase playerActionsBase = ((selectedIndex <= 0) ? actionSetInGame : ((selectedIndex != 1) ? actionSetMenu : actionSetVehicles));
		PlayerActionsBase playerActionsBase2 = playerActionsBase;
		int num = ((comboShowDS4 != null && comboShowDS4.Value) ? 1 : 0);
		foreach (ControllerLabelMapping labelsForController in labelsForControllers)
		{
			string text = "";
			string text2 = "";
			InputControlType[] controlTypes = labelsForController.ControlTypes;
			for (selectedIndex = 0; selectedIndex < controlTypes.Length; selectedIndex++)
			{
				controlTypes[selectedIndex].GetBoundAction(new PlayerActionSet[2] { playerActionsBase2, actionSetPermanent }, list);
				if (list.Count <= 0)
				{
					continue;
				}
				foreach (PlayerAction item in list)
				{
					PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)item.UserData;
					if (actionUserData != null)
					{
						if ((actionUserData.appliesToInputType == PlayerActionData.EAppliesToInputType.Both || actionUserData.appliesToInputType == PlayerActionData.EAppliesToInputType.ControllerOnly) && !actionUserData.doNotDisplay)
						{
							if (text.Length > 0)
							{
								text += ", ";
								text2 += ", ";
							}
							text += actionUserData.LocalizedName;
							text2 += actionUserData.LocalizedDescription;
						}
					}
					else if (ActionSetManager.DebugLevel > ActionSetManager.EDebugLevel.Off)
					{
						text += " !NULL! ";
						text2 += " !NULL! ";
					}
				}
			}
			if (text.Length == 0)
			{
				text = Localization.Get("inpUnboundControllerKey");
				text2 = Localization.Get("inpUnboundControllerKeyTooltip");
			}
			int num2 = 0;
			if (labelsForController.Labels.Length > 1)
			{
				num2 = num;
			}
			labelsForController.Labels[num2].Text = text;
			labelsForController.Labels[num2].ToolTip = text2;
		}
		string text3 = "";
		string text4 = "";
		switch ((eControllerJoystickLayout)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerJoystickLayout))
		{
		case eControllerJoystickLayout.Standard:
			text3 = Localization.Get("inpStandardLeftStick");
			text4 = Localization.Get("inpStandardRightStick");
			break;
		case eControllerJoystickLayout.Southpaw:
			text3 = Localization.Get("inpStandardRightStick");
			text4 = Localization.Get("inpStandardLeftStick");
			break;
		case eControllerJoystickLayout.Legacy:
			text3 = Localization.Get("inpLegacyLeftStick");
			text4 = Localization.Get("inpLegacyRightStick");
			break;
		case eControllerJoystickLayout.LegacySouthpaw:
			text3 = Localization.Get("inpLegacyRightStick");
			text4 = Localization.Get("inpLegacyLeftStick");
			break;
		}
		XUiV_Label[] labels = leftStickLabel.Labels;
		for (selectedIndex = 0; selectedIndex < labels.Length; selectedIndex++)
		{
			labels[selectedIndex].Text = text3;
		}
		labels = rightStickLabel.Labels;
		for (selectedIndex = 0; selectedIndex < labels.Length; selectedIndex++)
		{
			labels[selectedIndex].Text = text4;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		initControllerLayout();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doResetToDefaultsInternal()
	{
		base.doResetToDefaultsInternal();
		if (comboIconStyle != null)
		{
			comboIconStyle.Value = ((PlayerInputManager.ControllerIconStyle)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerIconStyle)/*cast due to .constrained prefix*/).ToString();
		}
		updateControllerMappingLabels();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createControlsEntries()
	{
		Dictionary<string, List<PlayerAction>> dictionary = new Dictionary<string, List<PlayerAction>>();
		PlayerActionsBase[] obj = new PlayerActionsBase[3]
		{
			xui.playerUI.playerInput,
			xui.playerUI.playerInput.VehicleActions,
			xui.playerUI.playerInput.PermanentActions
		};
		dictionary.Add("inpTabPlayerOnFoot", new List<PlayerAction>());
		PlayerActionsBase[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (PlayerAction controllerRebindableAction in array[i].ControllerRebindableActions)
			{
				if (!(controllerRebindableAction.UserData is PlayerActionData.ActionUserData actionUserData) || controllerRebindableAction.Equals(xui.playerUI.playerInput.PermanentActions.PushToTalk))
				{
					continue;
				}
				if (actionUserData.actionGroup.actionTab.tabNameKey == "inpTabPlayerControl")
				{
					dictionary["inpTabPlayerOnFoot"].Add(controllerRebindableAction);
					continue;
				}
				if (!dictionary.TryGetValue(actionUserData.actionGroup.actionTab.tabNameKey, out var value))
				{
					value = new List<PlayerAction>();
					dictionary.Add(actionUserData.actionGroup.actionTab.tabNameKey, value);
				}
				value.Add(controllerRebindableAction);
			}
		}
		dictionary["inpTabPlayerOnFoot"].Add(xui.playerUI.playerInput.PermanentActions.PushToTalk);
		int num = 1;
		foreach (KeyValuePair<string, List<PlayerAction>> item in dictionary)
		{
			TabSelector.SetTabCaption(num, Localization.Get(item.Key));
			XUiC_BindingEntry[] childControllers = TabSelector.GetTab(num).GetChildControllers<XUiC_BindingEntry>("");
			for (int j = 0; j < childControllers.Length; j++)
			{
				childControllers[j].Action = ((j < item.Value.Count) ? item.Value[j] : null);
			}
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		if (comboIconStyle != null)
		{
			comboIconStyle.Value = ((PlayerInputManager.ControllerIconStyle)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerIconStyle)/*cast due to .constrained prefix*/).ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doSaveChangesInternal()
	{
		base.doSaveChangesInternal();
		if (comboIconStyle != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsControllerIconStyle, comboIconStyle.SelectedIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		updateControllerMappingLabels();
		PlatformManager.NativePlatform.Input.ForceInputStyleChange();
		xui.CalloutWindow.ForceInputStyleChange(base.CurrentInputStyle, base.CurrentInputStyle);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateOptions();
		PlayerInputManager.InputStyle currentControllerInputStyle = PlatformManager.NativePlatform.Input.CurrentControllerInputStyle;
		if (currentControllerInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			bool flag = currentControllerInputStyle == PlayerInputManager.InputStyle.PS4;
			if (comboShowDS4 != null && flag != comboShowDS4.Value)
			{
				comboShowDS4.Value = flag;
				updateControllerMappingLabels();
			}
		}
	}
}
