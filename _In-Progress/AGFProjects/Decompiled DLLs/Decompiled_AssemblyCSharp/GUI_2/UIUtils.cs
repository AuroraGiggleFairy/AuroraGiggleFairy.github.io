using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;

namespace GUI_2;

public static class UIUtils
{
	public enum ButtonIcon
	{
		FaceButtonSouth,
		FaceButtonNorth,
		FaceButtonEast,
		FaceButtonWest,
		ConfirmButton,
		CancelButton,
		LeftBumper,
		RightBumper,
		LeftTrigger,
		RightTrigger,
		LeftStick,
		LeftStickUpDown,
		LeftStickLeftRight,
		LeftStickButton,
		RightStick,
		RightStickUpDown,
		RightStickLeftRight,
		RightStickButton,
		DPadLeft,
		DPadRight,
		DPadUp,
		DPadDown,
		StartButton,
		BackButton,
		None,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static UIAtlas symbolAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string sprite_PS = "PS5_";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string sprite_XB = "XB_";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<ButtonIcon, string> buttonIconMap = new EnumDictionary<ButtonIcon, string>
	{
		{
			ButtonIcon.FaceButtonSouth,
			"Button_Action1"
		},
		{
			ButtonIcon.FaceButtonNorth,
			"Button_Action4"
		},
		{
			ButtonIcon.FaceButtonEast,
			"Button_Action2"
		},
		{
			ButtonIcon.FaceButtonWest,
			"Button_Action3"
		},
		{
			ButtonIcon.ConfirmButton,
			"Button_Action1"
		},
		{
			ButtonIcon.CancelButton,
			"Button_Action2"
		},
		{
			ButtonIcon.LeftBumper,
			"Button_LeftBumper"
		},
		{
			ButtonIcon.LeftTrigger,
			"Button_LeftTrigger"
		},
		{
			ButtonIcon.RightBumper,
			"Button_RightBumper"
		},
		{
			ButtonIcon.RightTrigger,
			"Button_RightTrigger"
		},
		{
			ButtonIcon.LeftStick,
			"Button_LeftStick"
		},
		{
			ButtonIcon.LeftStickUpDown,
			"Button_LeftStickUpDown"
		},
		{
			ButtonIcon.LeftStickLeftRight,
			"Button_LeftStickLeftRight"
		},
		{
			ButtonIcon.LeftStickButton,
			"Button_LeftStickButton"
		},
		{
			ButtonIcon.RightStick,
			"Button_RightStick"
		},
		{
			ButtonIcon.RightStickUpDown,
			"Button_RightStickUpDown"
		},
		{
			ButtonIcon.RightStickLeftRight,
			"Button_RightStickLeftRight"
		},
		{
			ButtonIcon.RightStickButton,
			"Button_RightStickButton"
		},
		{
			ButtonIcon.DPadLeft,
			"Button_DPadLeft"
		},
		{
			ButtonIcon.DPadRight,
			"Button_DPadRight"
		},
		{
			ButtonIcon.DPadUp,
			"Button_DPadUp"
		},
		{
			ButtonIcon.DPadDown,
			"Button_DPadDown"
		},
		{
			ButtonIcon.StartButton,
			"Button_Start"
		},
		{
			ButtonIcon.BackButton,
			"Button_Back"
		},
		{
			ButtonIcon.None,
			""
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<InputControlType, ButtonIcon> iconControlMap = new EnumDictionary<InputControlType, ButtonIcon>
	{
		{
			InputControlType.Action1,
			ButtonIcon.FaceButtonSouth
		},
		{
			InputControlType.Action2,
			ButtonIcon.FaceButtonEast
		},
		{
			InputControlType.Action3,
			ButtonIcon.FaceButtonWest
		},
		{
			InputControlType.Action4,
			ButtonIcon.FaceButtonNorth
		},
		{
			InputControlType.LeftBumper,
			ButtonIcon.LeftBumper
		},
		{
			InputControlType.RightBumper,
			ButtonIcon.RightBumper
		},
		{
			InputControlType.LeftTrigger,
			ButtonIcon.LeftTrigger
		},
		{
			InputControlType.RightTrigger,
			ButtonIcon.RightTrigger
		},
		{
			InputControlType.LeftStickButton,
			ButtonIcon.LeftStickButton
		},
		{
			InputControlType.RightStickButton,
			ButtonIcon.RightStickButton
		},
		{
			InputControlType.DPadUp,
			ButtonIcon.DPadUp
		},
		{
			InputControlType.DPadDown,
			ButtonIcon.DPadDown
		},
		{
			InputControlType.DPadLeft,
			ButtonIcon.DPadLeft
		},
		{
			InputControlType.DPadRight,
			ButtonIcon.DPadRight
		},
		{
			InputControlType.Start,
			ButtonIcon.StartButton
		},
		{
			InputControlType.Menu,
			ButtonIcon.StartButton
		},
		{
			InputControlType.Options,
			ButtonIcon.StartButton
		},
		{
			InputControlType.Plus,
			ButtonIcon.StartButton
		},
		{
			InputControlType.Select,
			ButtonIcon.BackButton
		},
		{
			InputControlType.View,
			ButtonIcon.BackButton
		},
		{
			InputControlType.TouchPadButton,
			ButtonIcon.BackButton
		},
		{
			InputControlType.Minus,
			ButtonIcon.BackButton
		},
		{
			InputControlType.None,
			ButtonIcon.None
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<PlayerAction> loggedMissingBindingSources = new HashSet<PlayerAction>();

	public static UIAtlas IconAtlas => symbolAtlas;

	public static string GetSpriteName(ButtonIcon _icon)
	{
		if (PlayerInputManager.InputStyleFromSelectedIconStyle() == PlayerInputManager.InputStyle.PS4)
		{
			return "PS5_" + buttonIconMap[_icon];
		}
		return "XB_" + buttonIconMap[_icon];
	}

	public static ButtonIcon GetButtonIconForAction(PlayerAction _action)
	{
		if (_action == null)
		{
			return ButtonIcon.None;
		}
		DeviceBindingSource deviceBindingSource = _action.GetBindingOfType(_forController: true) as DeviceBindingSource;
		if (deviceBindingSource == null)
		{
			if (loggedMissingBindingSources.Add(_action))
			{
				Log.Warning("UIUtils: No device binding source could be found for PlayerAction {0}", _action.Name);
			}
			return ButtonIcon.None;
		}
		if (iconControlMap.TryGetValue(deviceBindingSource.Control, out var value))
		{
			return value;
		}
		Log.Warning("UIUtils: Could not assign a ButtonIcon for device control {0}", deviceBindingSource.Control.ToString());
		return ButtonIcon.None;
	}

	public static void LoadAtlas()
	{
		symbolAtlas = Resources.Load<UIAtlas>("GUI/Prefabs/SymbolAtlas");
	}
}
