using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

	public delegate void HandleTextUrlDelegate(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements);

	public const string LabelUrlTypeFieldName = "Type";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, HandleTextUrlDelegate> urlHandlers;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex urlMatcher;

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

	public static XUiView FindHierarchyClosestView(XUiController _startController, string _name)
	{
		XUiController controller = _startController.WindowGroup.Controller;
		XUiController parent = _startController.Parent;
		do
		{
			XUiController childById = parent.GetChildById(_name);
			if (childById != null)
			{
				return childById.ViewComponent;
			}
			parent = parent.Parent;
		}
		while (parent != controller);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiUtils()
	{
		urlHandlers = new Dictionary<string, HandleTextUrlDelegate>();
		urlMatcher = new Regex("([^|]+)>([^|]+)", RegexOptions.Compiled | RegexOptions.Singleline);
		RegisterLabelUrlHandler("Chat", handleChatTargetUrlClick);
		RegisterLabelUrlHandler("HTTP", handleHttpUrl);
	}

	public static void RegisterLabelUrlHandler(string _type, HandleTextUrlDelegate _handler)
	{
		if (urlHandlers.ContainsKey(_type))
		{
			Log.Warning("Register a new label URL handler for the type '" + _type + "'");
		}
		urlHandlers[_type] = _handler;
	}

	public static void HandleLabelUrlClick(XUiView _view, UILabel _label, HashSet<string> _allowedTypes = null)
	{
		string urlAtPosition = _label.GetUrlAtPosition(UICamera.lastWorldPosition);
		if (string.IsNullOrEmpty(urlAtPosition))
		{
			return;
		}
		Dictionary<string, string> dictionary;
		if (urlAtPosition.StartsWith("http://") || urlAtPosition.StartsWith("https://"))
		{
			dictionary = new Dictionary<string, string>
			{
				["Type"] = "HTTP",
				["Target"] = urlAtPosition
			};
		}
		else
		{
			MatchCollection matchCollection = urlMatcher.Matches(urlAtPosition);
			if (matchCollection.Count == 0)
			{
				Log.Warning("Text URL ('" + urlAtPosition + "'): Invalid URL");
				return;
			}
			dictionary = new Dictionary<string, string>();
			foreach (Match item in matchCollection)
			{
				dictionary[item.Groups[1].Value] = item.Groups[2].Value;
			}
		}
		HandleTextUrlDelegate value2;
		if (!dictionary.TryGetValue("Type", out var value))
		{
			Log.Warning("Text URL ('" + urlAtPosition + "'): No explicit or implicit type defined in '" + urlAtPosition + "'");
		}
		else if (!urlHandlers.TryGetValue(value, out value2))
		{
			Log.Warning("Text URL ('" + urlAtPosition + "'): No handler for type '" + value + "'");
		}
		else if (_allowedTypes != null && !_allowedTypes.Contains(value))
		{
			Log.Warning("Text URL ('" + urlAtPosition + "'): URL type '" + value + "' not allowed on this label");
		}
		else
		{
			value2(_view, urlAtPosition, dictionary);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleChatTargetUrlClick(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
	{
		if (!_urlElements.TryGetValue("ChatType", out var value))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): No ChatType defined");
			return;
		}
		_urlElements.TryGetValue("Sender", out var value2);
		if (!EnumUtils.TryParse<EChatType>(value, out var _result))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): Invalid chat type value '" + value + "'");
		}
		else
		{
			XUiC_Chat.SetChatTarget(_sender.xui, _result, value2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleHttpUrl(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
	{
		if (!_urlElements.TryGetValue("Target", out var value))
		{
			Log.Warning("Web URL (" + _sourceUrl + "): No Target defined");
		}
		else
		{
			XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(_sender.xui, value);
		}
	}

	public static string BuildUrlFunctionString(string _type)
	{
		return "[url=Type>" + _type + "]";
	}

	public static string BuildUrlFunctionString(string _type, (string key, string value) _param1)
	{
		return "[url=Type>" + _type + "|" + _param1.key + ">" + _param1.value + "]";
	}

	public static string BuildUrlFunctionString(string _type, (string key, string value) _param1, (string key, string value) _param2)
	{
		return "[url=Type>" + _type + "|" + _param1.key + ">" + _param1.value + "|" + _param2.key + ">" + _param2.value + "]";
	}

	public static string BuildUrlFunctionString(string _type, (string key, string value) _param1, (string key, string value) _param2, (string key, string value) _param3)
	{
		return "[url=Type>" + _type + "|" + _param1.key + ">" + _param1.value + "|" + _param2.key + ">" + _param2.value + "|" + _param3.key + ">" + _param3.value + "]";
	}
}
