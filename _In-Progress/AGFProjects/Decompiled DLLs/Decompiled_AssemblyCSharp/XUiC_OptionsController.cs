using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsController : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct ControllerLabelMapping
	{
		public readonly string[] ControlTypeNames;

		public readonly InputControlType[] ControlTypes;

		public readonly XUiV_Label[] Labels;

		public ControllerLabelMapping(string[] _controlTypeNames, XUiV_Label[] _labels, InputControlType[] _controlTypes = null)
		{
			ControlTypeNames = _controlTypeNames;
			if (_controlTypes == null)
			{
				ControlTypes = new InputControlType[_controlTypeNames.Length];
				for (int i = 0; i < _controlTypeNames.Length; i++)
				{
					ControlTypes[i] = EnumUtils.Parse<InputControlType>(_controlTypeNames[i], _ignoreCase: true);
				}
			}
			else
			{
				ControlTypes = _controlTypes;
			}
			Labels = _labels;
		}

		public ControllerLabelMapping(string _controlTypeName, XUiV_Label[] _labels)
		{
			ControlTypeNames = new string[1] { _controlTypeName };
			ControlTypes = new InputControlType[1] { EnumUtils.Parse<InputControlType>(_controlTypeName, _ignoreCase: true) };
			Labels = _labels;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVibrationStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboInterfaceSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboShowDS4;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLookSensitivityX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLookSensitivityY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboJoystickLayout;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboShowBindingsFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboLookInvert;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLookAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLookAxisDeadzone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboMoveAxisDeadzone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboCursorSnap;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboCursorHoverSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboVehicleSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAimAssists;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboWeaponAiming;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboSprintLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboTriggerEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboIconStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboQuickAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<XUiController, PlayerAction> buttonActionDictionary = new Dictionary<XUiController, PlayerAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<PlayerAction>> actionTabGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closedForNewBinding;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> actionBindingsOnOpen = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ControllerLabelMapping> labelsForControllers = new List<ControllerLabelMapping>();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetIngame;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetVehicles;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsBase actionSetPermanent;

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerLabelMapping leftStickLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerLabelMapping rightStickLabel;

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		RegisterForInputStyleChanges();
		ID = base.WindowGroup.ID;
		XUiController childById = GetChildById("AllowController");
		if (childById != null)
		{
			comboAllowController = childById.GetChildByType<XUiC_ComboBoxBool>();
			comboAllowController.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		}
		comboVibrationStrength = GetChildById("ControllerVibration").GetChildByType<XUiC_ComboBoxList<string>>();
		comboInterfaceSensitivity = GetChildById("ControllerInterfaceSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboShowDS4 = GetChildById("ShowDS4")?.GetChildByType<XUiC_ComboBoxBool>();
		comboShowBindingsFor = GetChildById("ShowBindingsFor").GetChildByType<XUiC_ComboBoxList<string>>();
		comboLookSensitivityX = GetChildById("ControllerLookSensitivityX").GetChildByType<XUiC_ComboBoxFloat>();
		comboLookSensitivityY = GetChildById("ControllerLookSensitivityY").GetChildByType<XUiC_ComboBoxFloat>();
		comboLookInvert = GetChildById("ControllerLookInvert").GetChildByType<XUiC_ComboBoxBool>();
		comboLookAcceleration = GetChildById("ControllerLookAcceleration").GetChildByType<XUiC_ComboBoxFloat>();
		comboJoystickLayout = GetChildById("ControllerJoystickLayout").GetChildByType<XUiC_ComboBoxList<string>>();
		comboZoomSensitivity = GetChildById("ControllerZoomSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboLookAxisDeadzone = GetChildById("ControllerLookAxisDeadzone").GetChildByType<XUiC_ComboBoxFloat>();
		comboMoveAxisDeadzone = GetChildById("ControllerMoveAxisDeadzone").GetChildByType<XUiC_ComboBoxFloat>();
		comboCursorSnap = GetChildById("ControllerCursorSnap").GetChildByType<XUiC_ComboBoxBool>();
		comboCursorHoverSensitivity = GetChildById("ControllerCursorHoverSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboAimAssists = GetChildById("ControllerAimAssists").GetChildByType<XUiC_ComboBoxBool>();
		comboWeaponAiming = GetChildById("WeaponAiming").GetChildByType<XUiC_ComboBoxBool>();
		comboVehicleSensitivity = GetChildById("ControllerVehicleSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboSprintLock = GetChildById("SprintLock").GetChildByType<XUiC_ComboBoxList<string>>();
		comboTriggerEffects = GetChildById("ControllerTriggerEffects")?.GetChildByType<XUiC_ComboBoxBool>();
		comboQuickAction = GetChildById("QuickAction").GetChildByType<XUiC_ComboBoxList<string>>();
		XUiController childById2 = GetChildById("ControllerIconStyle");
		if (childById2 != null)
		{
			comboIconStyle = childById2.GetChildByType<XUiC_ComboBoxList<string>>();
			comboIconStyle.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		}
		comboVibrationStrength.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboInterfaceSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookSensitivityX.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookSensitivityY.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboJoystickLayout.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookAcceleration.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookInvert.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboZoomSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookAxisDeadzone.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboMoveAxisDeadzone.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboCursorSnap.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboCursorHoverSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboAimAssists.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboWeaponAiming.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboVehicleSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboSprintLock.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboQuickAction.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		if (comboTriggerEffects != null)
		{
			comboTriggerEffects.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		}
		comboInterfaceSensitivity.Min = 0.10000000149011612;
		comboInterfaceSensitivity.Max = 1.0;
		if (comboShowDS4 != null)
		{
			comboShowDS4.OnValueChanged += ComboShowDs4_OnOnValueChanged;
		}
		comboShowBindingsFor.OnValueChanged += ComboShowMenuBindings_OnOnValueChanged;
		comboLookSensitivityX.Min = (comboLookSensitivityY.Min = 0.05000000074505806);
		comboLookSensitivityX.Max = (comboLookSensitivityY.Max = 1.0);
		comboZoomSensitivity.Min = (comboVehicleSensitivity.Min = 0.05000000074505806);
		comboZoomSensitivity.Max = (comboVehicleSensitivity.Max = 2.0);
		comboLookAcceleration.Min = 0.0;
		comboLookAcceleration.Max = 10.0;
		comboLookAxisDeadzone.Min = (comboMoveAxisDeadzone.Min = 0.0);
		comboLookAxisDeadzone.Max = (comboMoveAxisDeadzone.Max = 0.20000000298023224);
		comboCursorHoverSensitivity.Min = 0.10000000149011612;
		comboCursorHoverSensitivity.Max = 1.0;
		tabs = GetChildById("tabs") as XUiC_TabSelector;
		btnBack = GetChildById("btnBack") as XUiC_SimpleButton;
		btnDefaults = GetChildById("btnDefaults") as XUiC_SimpleButton;
		btnApply = GetChildById("btnApply") as XUiC_SimpleButton;
		btnBack.OnPressed += BtnBack_OnPressed;
		btnDefaults.OnPressed += BtnDefaults_OnOnPressed;
		btnApply.OnPressed += BtnApply_OnPressed;
		actionSetIngame = base.xui.playerUI.playerInput;
		actionSetVehicles = base.xui.playerUI.playerInput.VehicleActions;
		actionSetMenu = base.xui.playerUI.playerInput.GUIActions;
		actionSetPermanent = base.xui.playerUI.playerInput.PermanentActions;
		AddControllerLabelMappingsForButton("Menu");
		AddControllerLabelMappingsForButton("RightTrigger");
		AddControllerLabelMappingsForButton("RightBumper");
		AddControllerLabelMappingsForButton("Action4");
		AddControllerLabelMappingsForButton("Action3");
		AddControllerLabelMappingsForButton("Action2");
		AddControllerLabelMappingsForButton("Action1");
		AddControllerLabelMappingsForButton("RightStickButton");
		AddControllerLabelMappingsForButton("View");
		AddControllerLabelMappingsForButton("LeftTrigger");
		AddControllerLabelMappingsForButton("LeftBumper");
		AddControllerLabelMappingsForButton("LeftStickButton");
		AddControllerLabelMappingsForButton("DPadUp");
		AddControllerLabelMappingsForButton("DPadLeft");
		AddControllerLabelMappingsForButton("DPadDown");
		AddControllerLabelMappingsForButton("DPadRight");
		AssignControllerLabelMappingsForButton("LeftStick", ref leftStickLabel, new string[4] { "LeftStickLeft", "LeftStickRight", "LeftStickUp", "LeftStickDown" });
		AssignControllerLabelMappingsForButton("RightStick", ref rightStickLabel, new string[4] { "RightStickLeft", "RightStickRight", "RightStickUp", "RightStickDown" });
		(GetChildById("controllerArt").ViewComponent as XUiV_Sprite).Sprite.fixedAspect = true;
		(GetChildById("controllerLines").ViewComponent as XUiV_Sprite).Sprite.fixedAspect = true;
		updateControllerMappingLabels();
		RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddControllerLabelMappingsForButton(string _uiName, string[] _controlTypeNames = null, InputControlType[] _controlTypes = null)
	{
		XUiController[] childrenById = GetChildById("controllerlayout").GetChildrenById(_uiName);
		XUiV_Label[] array = new XUiV_Label[childrenById.Length];
		for (int i = 0; i < childrenById.Length; i++)
		{
			array[i] = (XUiV_Label)childrenById[i].ViewComponent;
		}
		if (_controlTypeNames == null)
		{
			labelsForControllers.Add(new ControllerLabelMapping(_uiName, array));
		}
		else
		{
			labelsForControllers.Add(new ControllerLabelMapping(_controlTypeNames, array, _controlTypes));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AssignControllerLabelMappingsForButton(string _uiName, ref ControllerLabelMapping assignTo, string[] _controlTypeNames = null, InputControlType[] _controlTypes = null)
	{
		XUiController[] childrenById = GetChildById("controllerlayout").GetChildrenById(_uiName);
		XUiV_Label[] array = new XUiV_Label[childrenById.Length];
		for (int i = 0; i < childrenById.Length; i++)
		{
			array[i] = (XUiV_Label)childrenById[i].ViewComponent;
		}
		if (_controlTypeNames == null)
		{
			assignTo = new ControllerLabelMapping(_uiName, array);
		}
		else
		{
			assignTo = new ControllerLabelMapping(_controlTypeNames, array, _controlTypes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		applyChanges();
		btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		switch (tabs.SelectedTabIndex)
		{
		default:
			return;
		case 0:
			if (comboAllowController != null)
			{
				comboAllowController.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsAllowController);
			}
			comboVibrationStrength.Value = ((eControllerVibrationStrength)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerVibrationStrength)/*cast due to .constrained prefix*/).ToString();
			comboInterfaceSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsInterfaceSensitivity);
			comboLookSensitivityX.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerSensitivityX);
			comboLookSensitivityY.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerSensitivityY);
			comboJoystickLayout.Value = ((eControllerJoystickLayout)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerJoystickLayout)/*cast due to .constrained prefix*/).ToString();
			comboLookAcceleration.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookAcceleration);
			comboLookInvert.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookInvert);
			comboZoomSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerZoomSensitivity);
			comboLookAxisDeadzone.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookAxisDeadzone);
			comboMoveAxisDeadzone.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerMoveAxisDeadzone);
			comboCursorSnap.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerCursorSnap);
			comboCursorHoverSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerCursorHoverSensitivity);
			comboVehicleSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerVehicleSensitivity);
			comboWeaponAiming.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerWeaponAiming);
			comboAimAssists.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerAimAssists);
			comboSprintLock.SelectedIndex = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsSprintLock);
			comboQuickAction.SelectedIndex = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsDefaultQuickAction);
			if (comboTriggerEffects != null)
			{
				comboTriggerEffects.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerTriggerEffects);
			}
			if (comboIconStyle != null)
			{
				comboIconStyle.Value = ((PlayerInputManager.ControllerIconStyle)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerIconStyle)/*cast due to .constrained prefix*/).ToString();
			}
			break;
		case 1:
			foreach (PlayerAction item in actionTabGroups["inpTabPlayerOnFoot"])
			{
				item.ResetBindings();
			}
			break;
		case 2:
			foreach (PlayerAction item2 in actionTabGroups["inpTabToolbelt"])
			{
				item2.ResetBindings();
			}
			break;
		case 3:
			foreach (PlayerAction item3 in actionTabGroups["inpTabVehicle"])
			{
				item3.ResetBindings();
			}
			break;
		}
		updateControllerMappingLabels();
		updateActionBindingLabels();
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboShowDs4_OnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		IsDirty = true;
		updateControllerMappingLabels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboShowMenuBindings_OnOnValueChanged(XUiController _sender, string _s, string _newValue1)
	{
		IsDirty = true;
		updateControllerMappingLabels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateControllerMappingLabels()
	{
		List<PlayerAction> list = new List<PlayerAction>();
		PlayerActionsBase playerActionsBase = ((comboShowBindingsFor.SelectedIndex <= 0) ? actionSetIngame : ((comboShowBindingsFor.SelectedIndex == 1) ? actionSetVehicles : actionSetMenu));
		int num = ((comboShowDS4 != null && comboShowDS4.Value) ? 1 : 0);
		foreach (ControllerLabelMapping labelsForController in labelsForControllers)
		{
			string text = "";
			string text2 = "";
			InputControlType[] controlTypes = labelsForController.ControlTypes;
			foreach (InputControlType controlType in controlTypes)
			{
				PlayerActionSet[] actionSets = new PlayerActionsBase[2] { playerActionsBase, actionSetPermanent };
				controlType.GetBoundAction(actionSets, list);
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
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].Text = text3;
		}
		labels = rightStickLabel.Labels;
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].Text = text4;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createControlsEntries()
	{
		PlayerActionsBase[] obj = new PlayerActionsBase[3]
		{
			base.xui.playerUI.playerInput,
			base.xui.playerUI.playerInput.VehicleActions,
			base.xui.playerUI.playerInput.PermanentActions
		};
		actionTabGroups = new Dictionary<string, List<PlayerAction>>();
		actionTabGroups.Add("inpTabPlayerOnFoot", new List<PlayerAction>());
		PlayerActionsBase[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (PlayerAction controllerRebindableAction in array[i].ControllerRebindableActions)
			{
				if (controllerRebindableAction.UserData is PlayerActionData.ActionUserData actionUserData && !controllerRebindableAction.Equals(base.xui.playerUI.playerInput.PermanentActions.PushToTalk))
				{
					if (actionUserData.actionGroup.actionTab.tabNameKey == "inpTabPlayerControl")
					{
						actionTabGroups["inpTabPlayerOnFoot"].Add(controllerRebindableAction);
						continue;
					}
					if (actionTabGroups.ContainsKey(actionUserData.actionGroup.actionTab.tabNameKey))
					{
						actionTabGroups[actionUserData.actionGroup.actionTab.tabNameKey].Add(controllerRebindableAction);
						continue;
					}
					actionTabGroups.Add(actionUserData.actionGroup.actionTab.tabNameKey, new List<PlayerAction>());
					actionTabGroups[actionUserData.actionGroup.actionTab.tabNameKey].Add(controllerRebindableAction);
				}
			}
		}
		actionTabGroups["inpTabPlayerOnFoot"].Add(base.xui.playerUI.playerInput.PermanentActions.PushToTalk);
		int num = 1;
		foreach (KeyValuePair<string, List<PlayerAction>> actionTabGroup in actionTabGroups)
		{
			XUiV_Grid xUiV_Grid = (XUiV_Grid)tabs.GetTab(num).GetChildById("controlsGrid").ViewComponent;
			tabs.SetTabCaption(num, Localization.Get(actionTabGroup.Key));
			for (int j = 0; j < xUiV_Grid.Controller.Children.Count; j++)
			{
				if (j < actionTabGroup.Value.Count)
				{
					AssignActionToBindingEntry(xUiV_Grid.Controller.Children[j], actionTabGroup.Value[j]);
				}
				else
				{
					AssignActionToBindingEntry(xUiV_Grid.Controller.Children[j], null);
				}
			}
			xUiV_Grid.Grid.Reposition();
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		if (comboAllowController != null)
		{
			comboAllowController.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsAllowController);
		}
		comboVibrationStrength.Value = ((eControllerVibrationStrength)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength)/*cast due to .constrained prefix*/).ToString();
		comboInterfaceSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		comboLookSensitivityX.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityX);
		comboLookSensitivityY.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityY);
		comboJoystickLayout.Value = ((eControllerJoystickLayout)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerJoystickLayout)/*cast due to .constrained prefix*/).ToString();
		comboLookAcceleration.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAcceleration);
		comboLookInvert.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerLookInvert);
		comboZoomSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerZoomSensitivity);
		comboLookAxisDeadzone.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAxisDeadzone);
		comboMoveAxisDeadzone.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerMoveAxisDeadzone);
		comboCursorSnap.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap);
		comboCursorHoverSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerCursorHoverSensitivity);
		comboVehicleSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerVehicleSensitivity);
		comboWeaponAiming.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerWeaponAiming);
		comboAimAssists.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerAimAssists);
		comboSprintLock.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsSprintLock);
		comboQuickAction.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsDefaultQuickAction);
		if (comboTriggerEffects != null)
		{
			comboTriggerEffects.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects);
		}
		if (comboIconStyle != null)
		{
			comboIconStyle.Value = ((PlayerInputManager.ControllerIconStyle)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerIconStyle)/*cast due to .constrained prefix*/).ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AssignActionToBindingEntry(XUiController _controller, PlayerAction _action)
	{
		XUiController childById = _controller.GetChildById("label");
		_controller.GetChildById("value");
		XUiController childById2 = _controller.GetChildById("unbind");
		XUiController childById3 = _controller.GetChildById("background");
		if (_action != null)
		{
			_controller.ViewComponent.UiTransform.name = _action.Name;
			PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)_action.UserData;
			buttonActionDictionary.Add(_controller, _action);
			((XUiV_Label)childById.ViewComponent).Text = actionUserData.LocalizedName;
			childById.ViewComponent.ToolTip = actionUserData.LocalizedDescription;
			if (actionUserData.allowRebind)
			{
				childById3.OnPress += NewBindingClicked;
				childById2.OnPress += UnbindButtonClicked;
				childById2.ViewComponent.ToolTip = Localization.Get("xuiRemoveBinding");
			}
		}
		else
		{
			childById2.ViewComponent.ForceHide = true;
			XUiView xUiView = childById2.ViewComponent;
			XUiView xUiView2 = childById2.ViewComponent;
			bool isSnappable = (childById2.ViewComponent.IsVisible = false);
			xUiView.IsNavigatable = (xUiView2.IsSnappable = isSnappable);
			childById3.ViewComponent.ForceHide = true;
			XUiView xUiView3 = childById3.ViewComponent;
			XUiView xUiView4 = childById3.ViewComponent;
			isSnappable = (childById3.ViewComponent.IsVisible = false);
			xUiView3.IsNavigatable = (xUiView4.IsSnappable = isSnappable);
			_controller.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator updateActionBindingLabelsLater()
	{
		yield return null;
		updateActionBindingLabels();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActionBindingLabels()
	{
		foreach (KeyValuePair<XUiController, PlayerAction> item in buttonActionDictionary)
		{
			((XUiV_Label)item.Key.GetChildById("value").ViewComponent).Text = item.Value.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NewBindingClicked(XUiController _sender, int _mouseButton)
	{
		if (buttonActionDictionary.TryGetValue(_sender.Parent, out var value))
		{
			closedForNewBinding = true;
			btnApply.Enabled = true;
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
			XUiC_OptionsControlsNewBinding.GetNewBinding(base.xui, value, windowGroup.ID, _forController: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnbindButtonClicked(XUiController _sender, int _mouseButton)
	{
		if (buttonActionDictionary.TryGetValue(_sender.Parent, out var value))
		{
			value.UnbindBindingsOfType(_controller: true);
			ThreadManager.StartCoroutine(updateActionBindingLabelsLater());
			btnApply.Enabled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		if (comboAllowController != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsAllowController, comboAllowController.Value);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsControllerVibrationStrength, comboVibrationStrength.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsInterfaceSensitivity, (float)comboInterfaceSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerSensitivityX, (float)comboLookSensitivityX.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerSensitivityY, (float)comboLookSensitivityY.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerJoystickLayout, comboJoystickLayout.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerLookInvert, comboLookInvert.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerLookAcceleration, (float)comboLookAcceleration.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerZoomSensitivity, (float)comboZoomSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerLookAxisDeadzone, (float)comboLookAxisDeadzone.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerMoveAxisDeadzone, (float)comboMoveAxisDeadzone.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerCursorHoverSensitivity, (float)comboCursorHoverSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerCursorSnap, comboCursorSnap.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerAimAssists, comboAimAssists.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerVehicleSensitivity, (float)comboVehicleSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControllerWeaponAiming, comboWeaponAiming.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControlsSprintLock, comboSprintLock.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsControlsDefaultQuickAction, comboQuickAction.SelectedIndex);
		if (comboIconStyle != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsControllerIconStyle, comboIconStyle.SelectedIndex);
		}
		if (comboTriggerEffects != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, comboTriggerEffects.Value);
		}
		GameOptionsManager.SaveControls();
		GamePrefs.Instance.Save();
		PlayerMoveController.UpdateControlsOptions();
		CursorControllerAbs.UpdateGamePrefs();
		TriggerEffectManager.UpdateControllerVibrationStrength();
		TriggerEffectManager.SetEnabled(GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects));
		storeCurrentBindings();
		updateControllerMappingLabels();
		XUiC_OptionsController.OnSettingsChanged?.Invoke();
		PlatformManager.NativePlatform.Input.ForceInputStyleChange();
		base.xui.calloutWindow.ForceInputStyleChange(base.CurrentInputStyle, base.CurrentInputStyle);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void storeCurrentBindings()
	{
		actionBindingsOnOpen.Clear();
		foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
		{
			actionBindingsOnOpen.Add(actionSet.Save());
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		if (!initialized)
		{
			createControlsEntries();
			initialized = true;
		}
		if (!closedForNewBinding)
		{
			updateOptions();
			storeCurrentBindings();
			btnApply.Enabled = false;
		}
		closedForNewBinding = false;
		updateActionBindingLabels();
		RefreshApplyLabel();
		base.OnOpen();
		if (initialized)
		{
			List<XUiController> list = new List<XUiController>();
			GetChildrenById("bindingEntry", list);
			foreach (XUiController item in list)
			{
				if (!buttonActionDictionary.ContainsKey(item))
				{
					item.ViewComponent.UiTransform.gameObject.SetActive(value: false);
				}
			}
		}
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

	public override void OnClose()
	{
		base.OnClose();
		if (!closedForNewBinding)
		{
			PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(actionBindingsOnOpen);
			btnApply.Enabled = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			updateActionBindingLabels();
			RefreshApplyLabel();
			IsDirty = false;
		}
		if (btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed && !base.xui.playerUI.windowManager.IsWindowOpen("optionsControlsNewBinding"))
		{
			BtnApply_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "isds4":
			_value = (DeviceFlag.PS5.IsCurrent() || (comboShowDS4 != null && comboShowDS4.Value)).ToString();
			return true;
		case "isxb1":
			_value = ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent() || (comboShowDS4 != null && !comboShowDS4.Value)).ToString();
			return true;
		case "controller_art":
			_value = ((DeviceFlag.PS5.IsCurrent() || (comboShowDS4 != null && comboShowDS4.Value)) ? "Controller_Art_PS5" : "Controller_Art_XB");
			return true;
		case "controller_lines":
			_value = ((DeviceFlag.PS5.IsCurrent() || (comboShowDS4 != null && comboShowDS4.Value)) ? "Controller_Lines_PS5" : "Controller_Lines_XB");
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
