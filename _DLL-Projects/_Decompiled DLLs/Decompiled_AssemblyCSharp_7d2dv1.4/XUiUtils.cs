using System;
using System.Text;
using InControl;
using Platform;
using UnityEngine;

public static class XUiUtils
{
	public enum EmptyBindingStyle
	{
		EmptyString,
		NullString,
		LocalizedUnbound,
		LocalizedNone
	}

	public enum DisplayStyle
	{
		Plain,
		KeyboardWithAngleBrackets,
		KeyboardWithParentheses
	}

	public enum ForceLabelInputStyle
	{
		Off,
		Keyboard,
		Controller
	}

	public static string GetBindingXuiMarkupString(this PlayerAction _action, EmptyBindingStyle _emptyStyle = EmptyBindingStyle.EmptyString, DisplayStyle _displayStyle = DisplayStyle.Plain, string _customDisplayStyle = null)
	{
		if (_action == null)
		{
			return "";
		}
		string name = ((PlayerActionsBase)_action.Owner).Name;
		string name2 = _action.Name;
		bool num = _emptyStyle != EmptyBindingStyle.EmptyString;
		bool flag = !string.IsNullOrEmpty(_customDisplayStyle);
		bool flag2 = _displayStyle != DisplayStyle.Plain || flag;
		string text = ((num || flag2) ? (":" + _emptyStyle.ToStringCached()) : "");
		string text2 = (flag2 ? (":" + (flag ? _customDisplayStyle : _displayStyle.ToStringCached())) : "");
		return "[action:" + name + ":" + name2 + text + text2 + "]";
	}

	public static bool ParseActionsMarkup(XUi _xui, string _input, out string _parsed, string _defaultCustomFormat = null, ForceLabelInputStyle _forceInputStyle = ForceLabelInputStyle.Off)
	{
		bool result = false;
		int num;
		while ((num = _input.IndexOf("[action:", StringComparison.OrdinalIgnoreCase)) >= 0)
		{
			int num2 = num + "[action:".Length;
			int num3 = _input.IndexOf(':', num2);
			int num4 = _input.IndexOf(':', num3 + 1);
			int num5 = _input.IndexOf(':', num4 + 1);
			int num6 = 0;
			int num7 = num2;
			while (num6 >= 0)
			{
				int num8 = _input.IndexOf('[', num7);
				int num9 = _input.IndexOf(']', num7);
				bool num10 = num8 >= 0;
				bool flag = num9 >= 0;
				if (num10 && num8 < num9)
				{
					num6++;
					num7 = num8 + 1;
					continue;
				}
				if (!flag)
				{
					break;
				}
				num6--;
				num7 = num9 + 1;
			}
			if (num6 >= 0)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, no closing bracket found");
				break;
			}
			int num11 = num7 - 1;
			bool flag2 = num4 >= 0 && num4 < num11;
			bool flag3 = flag2 && num5 >= 0 && num5 < num11;
			if (num11 < 0)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, no closing bracket found");
				break;
			}
			if (num3 < 0 || num3 > num11)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, no separator between action set name and action found");
				break;
			}
			int num12 = (flag2 ? (num4 - 1) : (num11 - 1));
			int num13 = (flag3 ? (num5 - 1) : (num11 - 1));
			string text = _input.Substring(num2, num3 - num2);
			string text2 = _input.Substring(num3 + 1, num12 - num3);
			string text3 = (flag2 ? _input.Substring(num4 + 1, num13 - num4) : null);
			string text4 = (flag3 ? _input.Substring(num5 + 1, num11 - num5 - 1) : null);
			PlayerActionsBase actionSetForName = PlatformManager.NativePlatform.Input.GetActionSetForName(text);
			if (actionSetForName == null)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, action set \"" + text + "\" not found");
				break;
			}
			PlayerAction playerActionByName = actionSetForName.GetPlayerActionByName(text2);
			if (playerActionByName == null)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, action \"" + text2 + "\" not found");
				break;
			}
			EmptyBindingStyle _result = EmptyBindingStyle.EmptyString;
			if (flag2 && text3.Length > 0 && !EnumUtils.TryParse<EmptyBindingStyle>(text3, out _result, _ignoreCase: true))
			{
				Log.Warning("[XUi] Could not parse action descriptor empty style, \"" + text3 + "\" unknown");
			}
			bool isCustomDisplayStyle = false;
			DisplayStyle _result2 = DisplayStyle.Plain;
			if (flag3 && !EnumUtils.TryParse<DisplayStyle>(text4, out _result2, _ignoreCase: true))
			{
				if (text4.Length < 1)
				{
					Log.Warning("[XUi] Could not parse action descriptor display type, \"" + text4 + "\" unknown");
				}
				else if (text4.IndexOf("###", StringComparison.Ordinal) < 0)
				{
					Log.Warning("[XUi] Could not parse action descriptor display type, \"" + text4 + "\" assumed to be a custom format, missing the '#' placeholder");
				}
				else
				{
					isCustomDisplayStyle = true;
				}
			}
			if (!flag3 && !string.IsNullOrEmpty(_defaultCustomFormat))
			{
				isCustomDisplayStyle = true;
				text4 = _defaultCustomFormat;
			}
			string bindingString = playerActionByName.GetBindingString(_forceInputStyle != ForceLabelInputStyle.Keyboard && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || _forceInputStyle == ForceLabelInputStyle.Controller), GetInputStyleFromForcedStyle(_forceInputStyle), _result, _result2, isCustomDisplayStyle, text4);
			_input = _input.Remove(num, num11 - num + 1);
			_input = _input.Insert(num, bindingString);
			result = true;
		}
		while ((num = _input.IndexOf("[button:", StringComparison.OrdinalIgnoreCase)) >= 0)
		{
			int num14 = num + "[button:".Length;
			int num15 = _input.IndexOf(']', num);
			if (num15 < 0)
			{
				Log.Warning("[XUi] Could not parse button descriptor in label text, no closing bracket found");
				break;
			}
			string value = InControlExtensions.TryLocalizeButtonName(_input.Substring(num14, num15 - num14));
			_input = _input.Remove(num, num15 - num + 1);
			_input = _input.Insert(num, value);
			result = true;
		}
		_parsed = _input;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInputManager.InputStyle GetInputStyleFromForcedStyle(ForceLabelInputStyle _forceStyle)
	{
		return _forceStyle switch
		{
			ForceLabelInputStyle.Keyboard => PlayerInputManager.InputStyle.Keyboard, 
			ForceLabelInputStyle.Controller => PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, 
			_ => PlatformManager.NativePlatform.Input.CurrentInputStyle, 
		};
	}

	public static string GetXuiHierarchy(this XUiController _current)
	{
		StringBuilder stringBuilder = new StringBuilder();
		getXuiHierarchyRec(_current, stringBuilder);
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void getXuiHierarchyRec(XUiController _current, StringBuilder _sb)
	{
		if (_current.Parent != null)
		{
			getXuiHierarchyRec(_current.Parent, _sb);
			_sb.Append(" -> ");
		}
		string text = null;
		string text2 = null;
		if (_current.ViewComponent != null)
		{
			text = _current.ViewComponent.GetType().Name.Replace("XUiV_", "");
			text2 = _current.ViewComponent.ID;
		}
		else
		{
			text = "windowgroup";
			text2 = _current.WindowGroup.ID;
		}
		if (text.EqualsCaseInsensitive(text2))
		{
			_sb.Append(text2);
			return;
		}
		_sb.Append(text);
		_sb.Append(" (");
		_sb.Append(text2);
		_sb.Append(")");
	}

	public static string ToXuiColorString(this Color32 _color)
	{
		return $"{_color.r},{_color.g},{_color.b},{_color.a}";
	}
}
