using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsControls : XUiController
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
	public XUiC_ComboBoxFloat comboLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboZoomAccel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboVehicleSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboWeaponAiming;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboInvertMouseLookY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboSprintLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboQuickAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlayerAction, UILabel> actionToValueLabel = new Dictionary<PlayerAction, UILabel>();

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

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		comboLookSensitivity = GetChildById("LookSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboZoomSensitivity = GetChildById("ZoomSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboZoomAccel = GetChildById("ZoomAccel").GetChildByType<XUiC_ComboBoxFloat>();
		comboVehicleSensitivity = GetChildById("VehicleSensitivity").GetChildByType<XUiC_ComboBoxFloat>();
		comboWeaponAiming = GetChildById("WeaponAiming").GetChildByType<XUiC_ComboBoxBool>();
		comboInvertMouseLookY = GetChildById("InvertMouseLookY").GetChildByType<XUiC_ComboBoxBool>();
		comboSprintLock = GetChildById("SprintLock").GetChildByType<XUiC_ComboBoxList<string>>();
		comboQuickAction = GetChildById("QuickAction").GetChildByType<XUiC_ComboBoxList<string>>();
		comboLookSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboZoomSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboZoomAccel.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboVehicleSensitivity.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboWeaponAiming.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboInvertMouseLookY.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboSprintLock.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboQuickAction.OnValueChangedGeneric += Combo_OnValueChangedGeneric;
		comboLookSensitivity.Min = 0.05000000074505806;
		comboLookSensitivity.Max = 1.5;
		comboZoomSensitivity.Min = 0.05000000074505806;
		comboZoomSensitivity.Max = 1.0;
		comboZoomAccel.Min = 0.0;
		comboZoomAccel.Max = 3.0;
		comboVehicleSensitivity.Min = 0.05000000074505806;
		comboVehicleSensitivity.Max = 3.0;
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
		RefreshApplyLabel();
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		btnApply.Enabled = true;
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
			comboLookSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsLookSensitivity);
			comboZoomSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomSensitivity);
			comboZoomAccel.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomAccel);
			comboVehicleSensitivity.Value = (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsVehicleLookSensitivity);
			comboWeaponAiming.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsWeaponAiming);
			comboInvertMouseLookY.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsInvertMouse);
			comboSprintLock.SelectedIndex = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsSprintLock);
			comboQuickAction.SelectedIndex = (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsDefaultQuickAction);
			break;
		case 1:
			foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action in actionSet.Actions)
				{
					if (action.UserData is PlayerActionData.ActionUserData actionUserData8 && actionUserData8.actionGroup.actionTab == PlayerActionData.TabMovement)
					{
						action.ResetBindings();
					}
				}
			}
			break;
		case 2:
			foreach (PlayerActionsBase actionSet2 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action2 in actionSet2.Actions)
				{
					if (action2.UserData is PlayerActionData.ActionUserData actionUserData7 && actionUserData7.actionGroup.actionTab == PlayerActionData.TabToolbelt)
					{
						action2.ResetBindings();
					}
				}
			}
			break;
		case 3:
			foreach (PlayerActionsBase actionSet3 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action3 in actionSet3.Actions)
				{
					if (action3.UserData is PlayerActionData.ActionUserData actionUserData6 && actionUserData6.actionGroup.actionTab == PlayerActionData.TabVehicle)
					{
						action3.ResetBindings();
					}
				}
			}
			break;
		case 4:
			foreach (PlayerActionsBase actionSet4 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action4 in actionSet4.Actions)
				{
					if (action4.UserData is PlayerActionData.ActionUserData actionUserData5 && actionUserData5.actionGroup.actionTab == PlayerActionData.TabMenus)
					{
						action4.ResetBindings();
					}
				}
			}
			break;
		case 5:
			foreach (PlayerActionsBase actionSet5 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action5 in actionSet5.Actions)
				{
					if (action5.UserData is PlayerActionData.ActionUserData actionUserData4 && actionUserData4.actionGroup.actionTab == PlayerActionData.TabUi)
					{
						action5.ResetBindings();
					}
				}
			}
			break;
		case 6:
			foreach (PlayerActionsBase actionSet6 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action6 in actionSet6.Actions)
				{
					if (action6.UserData is PlayerActionData.ActionUserData actionUserData3 && actionUserData3.actionGroup.actionTab == PlayerActionData.TabOther)
					{
						action6.ResetBindings();
					}
				}
			}
			break;
		case 7:
			foreach (PlayerActionsBase actionSet7 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action7 in actionSet7.Actions)
				{
					if (action7.UserData is PlayerActionData.ActionUserData actionUserData2 && actionUserData2.actionGroup.actionTab == PlayerActionData.TabEdit)
					{
						action7.ResetBindings();
					}
				}
			}
			break;
		case 8:
			foreach (PlayerActionsBase actionSet8 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				foreach (PlayerAction action8 in actionSet8.Actions)
				{
					if (action8.UserData is PlayerActionData.ActionUserData actionUserData && actionUserData.actionGroup.actionTab == PlayerActionData.TabGlobal)
					{
						action8.ResetBindings();
					}
				}
			}
			break;
		}
		updateActionBindingLabels();
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		closedForNewBinding = false;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		comboLookSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsLookSensitivity);
		comboZoomSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomSensitivity);
		comboZoomAccel.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomAccel);
		comboVehicleSensitivity.Value = GamePrefs.GetFloat(EnumGamePrefs.OptionsVehicleLookSensitivity);
		comboWeaponAiming.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsWeaponAiming);
		comboInvertMouseLookY.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsInvertMouse);
		comboSprintLock.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsSprintLock);
		comboQuickAction.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsDefaultQuickAction);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createControlsEntries()
	{
		SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> sortedDictionary = new SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>>();
		PlayerActionsBase[] array = new PlayerActionsBase[5]
		{
			base.xui.playerUI.playerInput,
			base.xui.playerUI.playerInput.VehicleActions,
			base.xui.playerUI.playerInput.PermanentActions,
			base.xui.playerUI.playerInput.GUIActions,
			PlayerActionsGlobal.Instance
		};
		for (int i = 0; i < array.Length; i++)
		{
			foreach (PlayerAction action in array[i].Actions)
			{
				if (!(action.UserData is PlayerActionData.ActionUserData actionUserData))
				{
					continue;
				}
				switch (actionUserData.appliesToInputType)
				{
				default:
					throw new ArgumentOutOfRangeException();
				case PlayerActionData.EAppliesToInputType.KbdMouseOnly:
				case PlayerActionData.EAppliesToInputType.Both:
					if (!actionUserData.doNotDisplay)
					{
						SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>> sortedDictionary2;
						if (sortedDictionary.ContainsKey(actionUserData.actionGroup.actionTab))
						{
							sortedDictionary2 = sortedDictionary[actionUserData.actionGroup.actionTab];
						}
						else
						{
							sortedDictionary2 = new SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>();
							sortedDictionary.Add(actionUserData.actionGroup.actionTab, sortedDictionary2);
						}
						List<PlayerAction> list;
						if (sortedDictionary2.ContainsKey(actionUserData.actionGroup))
						{
							list = sortedDictionary2[actionUserData.actionGroup];
						}
						else
						{
							list = new List<PlayerAction>();
							sortedDictionary2.Add(actionUserData.actionGroup, list);
						}
						list.Add(action);
					}
					break;
				case PlayerActionData.EAppliesToInputType.None:
				case PlayerActionData.EAppliesToInputType.ControllerOnly:
					break;
				}
			}
		}
		int num = 1;
		foreach (KeyValuePair<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> item in sortedDictionary)
		{
			tabs.SetTabCaption(num, Localization.Get(item.Key.tabNameKey));
			List<XUiC_KeyboardBindingEntry> list2 = new List<XUiC_KeyboardBindingEntry>((tabs.GetTab(num).GetChildById("controlsGrid").ViewComponent as XUiV_Grid).Controller.GetChildrenByType<XUiC_KeyboardBindingEntry>());
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<PlayerActionData.ActionGroup, List<PlayerAction>> item2 in item.Value)
			{
				if (num2 > 0)
				{
					list2[num3].Hide();
					num3++;
					list2[num3].Hide();
					num3++;
				}
				num2++;
				int num4 = 0;
				foreach (PlayerAction item3 in item2.Value)
				{
					createControl(list2[num3], item3, num4);
					num3++;
					num4++;
				}
				if (num4 % 2 != 0)
				{
					list2[num3].Hide();
					num3++;
				}
			}
			num++;
			foreach (XUiC_KeyboardBindingEntry item4 in list2)
			{
				if (item4.action == null)
				{
					item4.Hide();
				}
			}
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createControl(XUiC_KeyboardBindingEntry _entry, PlayerAction _action, int _controlNum)
	{
		PlayerActionData.ActionUserData obj = (PlayerActionData.ActionUserData)_action.UserData;
		_entry.SetAction(_action);
		actionToValueLabel.Add(_action, _entry.value.Label);
		if (obj.allowRebind)
		{
			_entry.button.Controller.OnPress += newBindingClick;
			_entry.unbind.Controller.OnPress += unbindButtonClick;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator updateActionBindingLabelsLater()
	{
		yield return null;
		updateActionBindingLabels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActionBindingLabels()
	{
		foreach (KeyValuePair<PlayerAction, UILabel> item in actionToValueLabel)
		{
			item.Value.text = item.Key.GetBindingString(_forController: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newBindingClick(XUiController _sender, int _mouseButton)
	{
		XUiC_KeyboardBindingEntry parentByType = _sender.GetParentByType<XUiC_KeyboardBindingEntry>();
		closedForNewBinding = true;
		btnApply.Enabled = true;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		XUiC_OptionsControlsNewBinding.GetNewBinding(base.xui, parentByType.action, windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unbindButtonClick(XUiController _sender, int _mouseButton)
	{
		_sender.GetParentByType<XUiC_KeyboardBindingEntry>().action.UnbindBindingsOfType(_controller: false);
		ThreadManager.StartCoroutine(updateActionBindingLabelsLater());
		btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		GamePrefs.Set(EnumGamePrefs.OptionsLookSensitivity, (float)comboLookSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsZoomSensitivity, (float)comboZoomSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsZoomAccel, (float)comboZoomAccel.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsVehicleLookSensitivity, (float)comboVehicleSensitivity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsWeaponAiming, comboWeaponAiming.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsInvertMouse, comboInvertMouseLookY.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsControlsSprintLock, comboSprintLock.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsControlsDefaultQuickAction, comboQuickAction.SelectedIndex);
		GameOptionsManager.SaveControls();
		GamePrefs.Instance.Save();
		PlayerMoveController.UpdateControlsOptions();
		storeCurrentBindings();
		XUiC_OptionsControls.OnSettingsChanged?.Invoke();
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
		}
		closedForNewBinding = false;
		updateActionBindingLabels();
		base.OnOpen();
		RefreshApplyLabel();
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
			IsDirty = false;
		}
		if (btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			BtnApply_OnPressed(null, 0);
		}
	}
}
