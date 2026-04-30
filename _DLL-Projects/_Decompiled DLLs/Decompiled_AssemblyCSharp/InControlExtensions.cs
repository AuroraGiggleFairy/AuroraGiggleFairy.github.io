using System;
using System.Collections.Generic;
using System.Text;
using InControl;
using Platform;

public static class InControlExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly StringBuilder bindingToString = new StringBuilder();

	public static string GetBindingString(this PlayerAction _action, bool _forController, PlayerInputManager.InputStyle _inputStyle = PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle _emptyStyle = XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle _displayStyle = XUiUtils.DisplayStyle.Plain, bool _isCustomDisplayStyle = false, string _customDisplayStyleString = null)
	{
		if (_action != null)
		{
			switch (_action.Name)
			{
			case "GUI Action Up":
				if (_forController)
				{
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.Inspect.GetBindingOfType(_forController: true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
				return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Up.GetBindingOfType().GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			case "GUI Action Down":
				if (_forController)
				{
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.Submit.GetBindingOfType(_forController: true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
				return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Down.GetBindingOfType().GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			case "GUI Action Left":
				if (_forController)
				{
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.HalfStack.GetBindingOfType(_forController: true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
				return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Left.GetBindingOfType().GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			case "GUI Action Right":
				if (_forController)
				{
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.Cancel.GetBindingOfType(_forController: true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
				return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Right.GetBindingOfType().GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			}
		}
		return _action.GetBindingOfType(_forController).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetBindingSourceString(this BindingSource _bs, PlayerInputManager.InputStyle _inputStyle = PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle _emptyStyle = XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle _displayStyle = XUiUtils.DisplayStyle.Plain, bool _isCustomDisplayStyle = false, string _customDisplayStyleString = null)
	{
		if (_bs == null)
		{
			return _emptyStyle switch
			{
				XUiUtils.EmptyBindingStyle.EmptyString => string.Empty, 
				XUiUtils.EmptyBindingStyle.NullString => null, 
				XUiUtils.EmptyBindingStyle.LocalizedUnbound => TryLocalizeButtonName("Unbound"), 
				XUiUtils.EmptyBindingStyle.LocalizedNone => TryLocalizeButtonName("None"), 
				_ => throw new ArgumentOutOfRangeException("_emptyStyle", _emptyStyle, null), 
			};
		}
		return _bs.BindingSourceType switch
		{
			BindingSourceType.DeviceBindingSource => GetGamepadSourceString(((DeviceBindingSource)_bs).Control), 
			BindingSourceType.KeyBindingSource => GetKeyboardSourceString((KeyBindingSource)_bs, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString), 
			BindingSourceType.MouseBindingSource => GetMouseSourceString(((MouseBindingSource)_bs).Control), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetKeyboardSourceString(KeyBindingSource _kbs, XUiUtils.DisplayStyle _displayStyle, bool _isCustomDisplayStyle, string _customDisplayStyleString)
	{
		KeyCombo control = _kbs.Control;
		bindingToString.Clear();
		for (int i = 0; i < control.IncludeCount; i++)
		{
			if (i > 0)
			{
				bindingToString.Append(" + ");
			}
			string localizedName = control.GetInclude(i).GetLocalizedName();
			if (_isCustomDisplayStyle)
			{
				bindingToString.Append(_customDisplayStyleString.Replace("###", localizedName));
				continue;
			}
			switch (_displayStyle)
			{
			case XUiUtils.DisplayStyle.Plain:
				bindingToString.Append(localizedName);
				break;
			case XUiUtils.DisplayStyle.KeyboardWithAngleBrackets:
				bindingToString.Append("<");
				bindingToString.Append(localizedName);
				bindingToString.Append(">");
				break;
			case XUiUtils.DisplayStyle.KeyboardWithParentheses:
				bindingToString.Append("( ");
				bindingToString.Append(localizedName);
				bindingToString.Append(" )");
				break;
			default:
				throw new ArgumentOutOfRangeException("_displayStyle");
			}
		}
		return bindingToString.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetMouseSourceString(Mouse _control)
	{
		return _control switch
		{
			Mouse.LeftButton => "[sp=Mouse_LeftButton_Large]", 
			Mouse.RightButton => "[sp=Mouse_RightButton_Large]", 
			Mouse.MiddleButton => "[sp=Mouse_MiddleButton_Large]", 
			_ => _control.GetLocalizedName(), 
		};
	}

	public static string GetGamepadSourceString(InputControlType _control)
	{
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		switch (_control)
		{
		case InputControlType.Start:
		case InputControlType.Options:
		case InputControlType.Menu:
		case InputControlType.Plus:
			_control = inputStyle switch
			{
				PlayerInputManager.InputStyle.PS4 => InputControlType.Options, 
				PlayerInputManager.InputStyle.XB1 => InputControlType.Menu, 
				_ => _control, 
			};
			break;
		case InputControlType.Back:
		case InputControlType.Select:
		case InputControlType.View:
		case InputControlType.Minus:
		case InputControlType.TouchPadButton:
			_control = inputStyle switch
			{
				PlayerInputManager.InputStyle.PS4 => InputControlType.TouchPadButton, 
				PlayerInputManager.InputStyle.XB1 => InputControlType.Back, 
				_ => _control, 
			};
			break;
		}
		return inputStyle switch
		{
			PlayerInputManager.InputStyle.PS4 => "[sp=PS5_Button_" + _control.ToStringCached() + "]", 
			PlayerInputManager.InputStyle.XB1 => "[sp=XB_Button_" + _control.ToStringCached() + "]", 
			_ => "[sp={_control.ToStringCached ()}]", 
		};
	}

	public static string GetBlankDPadSourceString()
	{
		if (PlayerInputManager.InputStyleFromSelectedIconStyle() == PlayerInputManager.InputStyle.PS4)
		{
			return "[sp=PS5_Button_DPadBlank]";
		}
		return "[sp=XB_Button_DPadBlank]";
	}

	public static string GetStartButtonSourceString()
	{
		if (PlayerInputManager.InputStyleFromSelectedIconStyle() == PlayerInputManager.InputStyle.PS4)
		{
			return "[sp=PS5_Button_Options]";
		}
		return "[sp=XB_Button_Menu]";
	}

	public static void SetApplyButtonString(XUiC_SimpleButton _button, string text_key)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			_button.Text = Localization.Get(text_key).ToUpper();
		}
		else
		{
			_button.Text = GetStartButtonSourceString() + " " + Localization.Get(text_key).ToUpper();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizedName(this Key _key)
	{
		string name = UnityKeyboardProvider.KeyMappings[(int)_key].Name;
		string text = "inpButton" + name.Replace(" ", "");
		string text2 = Localization.Get(text);
		if (text2 != text)
		{
			return text2;
		}
		return name;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizedName(this Mouse _key)
	{
		string text = "inpButton" + _key.ToStringCached();
		string text2 = Localization.Get(text);
		if (text2 != text)
		{
			return text2;
		}
		return _key.ToStringCached();
	}

	public static string TryLocalizeButtonName(string _buttonName)
	{
		string text = "inpButton" + _buttonName.Replace(" ", "");
		string text2 = Localization.Get(text);
		if (text2 != text)
		{
			return text2;
		}
		return _buttonName;
	}

	public static void UnbindBindingsOfType(this PlayerAction _action, bool _controller)
	{
		foreach (BindingSource binding in _action.Bindings)
		{
			if (_controller == (binding.BindingSourceType == BindingSourceType.DeviceBindingSource))
			{
				_action.RemoveBinding(binding);
			}
		}
	}

	public static void UnbindBindingsOfType(this PlayerAction _action, BindingSourceType _bindingType)
	{
		foreach (BindingSource binding in _action.Bindings)
		{
			if (binding.BindingSourceType == _bindingType)
			{
				_action.RemoveBinding(binding);
			}
		}
	}

	public static PlayerAction BindingUsed(this PlayerActionSet _actionSet, BindingSource _binding)
	{
		if (_binding == null)
		{
			return null;
		}
		int count = _actionSet.Actions.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionSet.Actions[i].HasBinding(_binding))
			{
				return _actionSet.Actions[i];
			}
		}
		return null;
	}

	public static BindingSource GetBindingOfType(this PlayerAction _action, bool _forController = false)
	{
		if (_action == null)
		{
			return null;
		}
		foreach (BindingSource binding in _action.Bindings)
		{
			bool flag = BindingSourceType.KeyBindingSource == binding.BindingSourceType || BindingSourceType.MouseBindingSource == binding.BindingSourceType;
			if (_forController != flag)
			{
				return binding;
			}
		}
		return null;
	}

	public static string GetNameForControlType(this InputDeviceProfile _profile, InputControlType _controlType)
	{
		InputControlMapping[] analogMappings = _profile.AnalogMappings;
		foreach (InputControlMapping inputControlMapping in analogMappings)
		{
			if (inputControlMapping.Target == _controlType)
			{
				return inputControlMapping.Target.ToStringCached();
			}
		}
		analogMappings = _profile.ButtonMappings;
		foreach (InputControlMapping inputControlMapping2 in analogMappings)
		{
			if (inputControlMapping2.Target == _controlType)
			{
				return inputControlMapping2.Target.ToStringCached();
			}
		}
		return null;
	}

	public static void GetBoundAction(this InputControlType _controlType, PlayerActionSet[] _actionSets, IList<PlayerAction> _result)
	{
		_result.Clear();
		if (_actionSets == null || _actionSets.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _actionSets.Length; i++)
		{
			foreach (PlayerAction action in _actionSets[i].Actions)
			{
				foreach (BindingSource unfilteredBinding in action.UnfilteredBindings)
				{
					if (unfilteredBinding.BindingSourceType == BindingSourceType.DeviceBindingSource && ((DeviceBindingSource)unfilteredBinding).Control == _controlType)
					{
						_result.Add(action);
					}
				}
			}
		}
	}
}
