using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsControlsNewBinding : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblAbort;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect rectInUse;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInUseBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction action;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction conflictingAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public BindingSource binding;

	[PublicizedFrom(EAccessModifier.Private)]
	public string windowToOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingSource> bindingAbortActions = new List<BindingSource>
	{
		new DeviceBindingSource(InputControlType.Back),
		new DeviceBindingSource(InputControlType.Options),
		new DeviceBindingSource(InputControlType.View),
		new DeviceBindingSource(InputControlType.Minus),
		new KeyBindingSource(Key.Escape)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BindingSource[] bindingForbidden = new BindingSource[39]
	{
		new KeyBindingSource(Key.F1),
		new KeyBindingSource(Key.F2),
		new KeyBindingSource(Key.F3),
		new KeyBindingSource(Key.F4),
		new KeyBindingSource(Key.F5),
		new KeyBindingSource(Key.F6),
		new KeyBindingSource(Key.F7),
		new KeyBindingSource(Key.F8),
		new KeyBindingSource(Key.F9),
		new KeyBindingSource(Key.F10),
		new KeyBindingSource(Key.F11),
		new KeyBindingSource(Key.F12),
		new DeviceBindingSource(InputControlType.Start),
		new DeviceBindingSource(InputControlType.Back),
		new DeviceBindingSource(InputControlType.LeftStickUp),
		new DeviceBindingSource(InputControlType.LeftStickDown),
		new DeviceBindingSource(InputControlType.LeftStickLeft),
		new DeviceBindingSource(InputControlType.LeftStickRight),
		new DeviceBindingSource(InputControlType.RightStickUp),
		new DeviceBindingSource(InputControlType.RightStickDown),
		new DeviceBindingSource(InputControlType.RightStickLeft),
		new DeviceBindingSource(InputControlType.RightStickRight),
		new DeviceBindingSource(InputControlType.Share),
		new DeviceBindingSource(InputControlType.Menu),
		new DeviceBindingSource(InputControlType.View),
		new DeviceBindingSource(InputControlType.Options),
		new DeviceBindingSource(InputControlType.Plus),
		new DeviceBindingSource(InputControlType.Minus),
		new DeviceBindingSource(InputControlType.TouchPadButton),
		new DeviceBindingSource(InputControlType.Select),
		new DeviceBindingSource(InputControlType.LeftStickY),
		new DeviceBindingSource(InputControlType.LeftStickX),
		new DeviceBindingSource(InputControlType.RightStickY),
		new DeviceBindingSource(InputControlType.RightStickX),
		new DeviceBindingSource(InputControlType.Create),
		new DeviceBindingSource(InputControlType.Guide),
		new DeviceBindingSource(InputControlType.Home),
		new DeviceBindingSource(InputControlType.Mute),
		new DeviceBindingSource(InputControlType.Capture)
	};

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		lblAction = GetChildById("forAction").ViewComponent as XUiV_Label;
		rectInUse = GetChildById("inUse").ViewComponent as XUiV_Rect;
		lblInUseBy = GetChildById("inUseBy").ViewComponent as XUiV_Label;
		lblAbort = GetChildById("newBindingAbort").ViewComponent as XUiV_Label;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnNewBinding")).OnPressed += BtnNewBinding_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnGrabBinding")).OnPressed += BtnGrabBinding_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNewBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		startGetBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnGrabBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		conflictingAction.ClearInputState();
		conflictingAction.RemoveBinding(binding);
		action.UnbindBindingsOfType(forController);
		action.AddBinding(binding);
		ThreadManager.StartCoroutine(closeNextFrame());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startGetBinding()
	{
		base.xui.playerUI.windowManager.IsInputLocked = true;
		InputUtils.EnableAllPlayerActions(_enable: false);
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: true, _bOverrideState: false);
		rectInUse.IsVisible = false;
		conflictingAction = null;
		binding = null;
		action.Owner.ListenOptions.IncludeUnknownControllers = false;
		action.Owner.ListenOptions.IncludeMouseButtons = !forController;
		action.Owner.ListenOptions.IncludeMouseScrollWheel = !forController;
		action.Owner.ListenOptions.IncludeKeys = true;
		action.Owner.ListenOptions.OnBindingFound = onBindingReceived;
		action.ListenForBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stoppedGetBinding(bool _alsoStopListening = true)
	{
		if (_alsoStopListening)
		{
			action.StopListeningForBinding();
		}
		InputUtils.EnableAllPlayerActions(_enable: true);
		base.xui.playerUI.windowManager.IsInputLocked = false;
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onBindingReceived(PlayerAction _action, BindingSource _binding)
	{
		if (bindingAbortActions.Contains(_binding))
		{
			Log.Out("Abort action pressed, aborting listening for new binding for {0}", _action.Name);
			stoppedGetBinding();
			ThreadManager.StartCoroutine(closeNextFrame());
			return false;
		}
		if (forController && (_binding is KeyBindingSource || _binding is MouseBindingSource))
		{
			Log.Out("Cannot accept key or mouse for controller binding");
			return false;
		}
		if (!forController && _binding is DeviceBindingSource)
		{
			Log.Out("Cannot accept device binding source for keyboard/mouse binding");
			return false;
		}
		BindingSource[] array = bindingForbidden;
		foreach (BindingSource bindingSource in array)
		{
			if (_binding == bindingSource)
			{
				Log.Out("Binding {0} not allowed", _binding.Name);
				return false;
			}
		}
		if (forController != (_binding.BindingSourceType == BindingSourceType.DeviceBindingSource))
		{
			Log.Out("New binding ({0}) doesn't match expected input device type ({1})", _binding.BindingSourceType.ToStringCached(), forController ? BindingSourceType.DeviceBindingSource.ToStringCached() : BindingSourceType.KeyBindingSource.ToStringCached());
			return false;
		}
		if (_action.HasBinding(_binding))
		{
			Log.Out("Binding {0} already bound to the current action {1}", _binding.Name, _action.Name);
			stoppedGetBinding();
			ThreadManager.StartCoroutine(closeNextFrame());
			return false;
		}
		if (alreadyBound(_binding, _action))
		{
			Log.Out("Binding {0} already bound to the action {1}", _binding.Name, conflictingAction.Name);
			stoppedGetBinding();
			return false;
		}
		_action.UnbindBindingsOfType(forController);
		stoppedGetBinding(_alsoStopListening: false);
		ThreadManager.StartCoroutine(closeNextFrame());
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alreadyBound(BindingSource _binding, PlayerAction _selfAction)
	{
		PlayerAction playerAction = _selfAction.Owner.BindingUsed(_binding);
		if (playerAction == null && _selfAction.Owner.UserData != null)
		{
			PlayerActionsBase[] bindingsConflictWithSet = ((PlayerActionData.ActionSetUserData)_selfAction.Owner.UserData).bindingsConflictWithSet;
			for (int i = 0; i < bindingsConflictWithSet.Length; i++)
			{
				playerAction = bindingsConflictWithSet[i].BindingUsed(_binding);
				if (playerAction != null)
				{
					break;
				}
			}
		}
		if (playerAction != null)
		{
			PlayerActionData.ActionUserData obj = _selfAction.UserData as PlayerActionData.ActionUserData;
			PlayerActionData.ActionUserData actionUserData = playerAction.UserData as PlayerActionData.ActionUserData;
			if (obj.allowMultipleBindings || actionUserData.allowMultipleBindings)
			{
				PlayerActionSet playerActionSet = base.xui.playerUI.playerInput;
				if (!base.xui.playerUI.playerInput.Actions.Contains(_selfAction) || !base.xui.playerUI.playerInput.Actions.Contains(playerAction))
				{
					if (base.xui.playerUI.playerInput.GUIActions.Actions.Contains(_selfAction) && base.xui.playerUI.playerInput.GUIActions.Actions.Contains(playerAction))
					{
						playerActionSet = base.xui.playerUI.playerInput.GUIActions;
					}
					else if (base.xui.playerUI.playerInput.VehicleActions.Actions.Contains(_selfAction) && base.xui.playerUI.playerInput.VehicleActions.Actions.Contains(playerAction))
					{
						playerActionSet = base.xui.playerUI.playerInput.VehicleActions;
					}
					else
					{
						if (!base.xui.playerUI.playerInput.PermanentActions.Actions.Contains(_selfAction) || !base.xui.playerUI.playerInput.PermanentActions.Actions.Contains(playerAction))
						{
							return false;
						}
						playerActionSet = base.xui.playerUI.playerInput.PermanentActions;
					}
				}
				bool flag = false;
				foreach (PlayerAction action in playerActionSet.Actions)
				{
					if (action != playerAction && action != _selfAction && action.Bindings.Contains(_binding))
					{
						flag = true;
						playerAction = action;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			rectInUse.IsVisible = true;
			binding = playerAction.GetBindingOfType(forController);
			conflictingAction = playerAction;
			if (forController)
			{
				lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction_Controller"), playerAction.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
			}
			else
			{
				lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction"), playerAction.GetBindingString(forController), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
			}
			GetChildById("btnNewBinding").SelectCursorElement(_withDelay: true);
			lblInUseBy.ToolTip = ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedDescription;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeNextFrame()
	{
		yield return null;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startNextFrame()
	{
		yield return null;
		startGetBinding();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (forController)
		{
			lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent_Controller"), ((PlayerActionData.ActionUserData)action.UserData).LocalizedName, action.GetBindingString(_forController: true, PlayerInputManager.InputStyleFromSelectedIconStyle()));
		}
		else
		{
			lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent"), ((PlayerActionData.ActionUserData)action.UserData).LocalizedName, action.GetBindingString(forController));
		}
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		string text;
		if (forController)
		{
			string arg = ((inputStyle != PlayerInputManager.InputStyle.PS4) ? "[sp=XB_Button_Back] / ESC" : "[sp=PS5_Button_Options] / ESC");
			text = string.Format(Localization.Get("xuiNewBindingAbort_Controller"), arg);
		}
		else
		{
			string arg = ((inputStyle != PlayerInputManager.InputStyle.PS4) ? "ESC / [sp=XB_Button_Back]" : "ESC / [sp=PS5_Button_Options]");
			text = string.Format(Localization.Get("xuiNewBindingAbort_Controller"), arg);
		}
		lblAbort.Text = text;
		ThreadManager.StartCoroutine(startNextFrame());
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Open(windowToOpen, _bModal: true);
	}

	public static void GetNewBinding(XUi _xuiInstance, PlayerAction _action, string _windowToOpen, bool _forController = false)
	{
		XUiC_OptionsControlsNewBinding childByType = _xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_OptionsControlsNewBinding>();
		childByType.action = _action;
		childByType.windowToOpen = _windowToOpen;
		childByType.forController = _forController;
		_xuiInstance.playerUI.windowManager.Open(childByType.WindowGroup.ID, _bModal: true);
	}
}
