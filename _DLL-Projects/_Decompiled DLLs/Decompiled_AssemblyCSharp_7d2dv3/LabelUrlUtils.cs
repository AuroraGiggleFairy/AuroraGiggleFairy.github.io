using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class LabelUrlUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct SHandlerData(HandleTextClickUrlDelegate _clickDelegate, HandleTextHoverUrlDelegate _hoverDelegate)
	{
		public readonly HandleTextClickUrlDelegate ClickDelegate = _clickDelegate;

		public readonly HandleTextHoverUrlDelegate HoverDelegate = _hoverDelegate;
	}

	public delegate void HandleTextClickUrlDelegate(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements);

	public delegate void HandleTextHoverUrlDelegate(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText);

	public const string LabelUrlTypeFieldName = "Type";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, SHandlerData> urlHandlers;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex urlMatcher;

	public static void RegisterLabelUrlHandler(string _type, HandleTextClickUrlDelegate _clickHandler, HandleTextHoverUrlDelegate _hoverHandler)
	{
		if (urlHandlers.ContainsKey(_type))
		{
			Log.Warning("Registering a new label URL handler for the type '" + _type + "'");
		}
		urlHandlers[_type] = new SHandlerData(_clickHandler, _hoverHandler);
	}

	public static void HandleLabelUrlClick(XUiView _view, UILabel _label, HashSet<string> _allowedTypes = null)
	{
		if (tryGetDataForLabelPos(_view, _label, _allowedTypes, out var _handler, out var _fullUrlString, out var _urlElements))
		{
			_handler.ClickDelegate?.Invoke(_view, _fullUrlString, _urlElements);
		}
	}

	public static void HandleLabelUrlHover(XUiView _view, UILabel _label, HashSet<string> _allowedTypes, out bool _isOverUrl, out string _tooltipText)
	{
		if (tryGetDataForLabelPos(_view, _label, _allowedTypes, out var _handler, out var _fullUrlString, out var _urlElements) && _handler.HoverDelegate != null)
		{
			_isOverUrl = true;
			_handler.HoverDelegate(_view, _fullUrlString, _urlElements, out _tooltipText);
		}
		else
		{
			_isOverUrl = false;
			_tooltipText = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetDataForLabelPos(XUiView _view, UILabel _label, HashSet<string> _allowedTypes, out SHandlerData _handler, out string _fullUrlString, out Dictionary<string, string> _urlElements)
	{
		_handler = default(SHandlerData);
		_urlElements = null;
		_fullUrlString = _label.GetUrlAtPosition(UICamera.lastWorldPosition);
		if (string.IsNullOrEmpty(_fullUrlString))
		{
			return false;
		}
		if (_fullUrlString.StartsWith("http://") || _fullUrlString.StartsWith("https://"))
		{
			_urlElements = new Dictionary<string, string>
			{
				["Type"] = "HTTP",
				["Target"] = _fullUrlString
			};
		}
		else
		{
			MatchCollection matchCollection = urlMatcher.Matches(_fullUrlString);
			if (matchCollection.Count == 0)
			{
				Log.Warning("Text URL ('" + _fullUrlString + "'): Invalid URL");
				return false;
			}
			_urlElements = new Dictionary<string, string>();
			foreach (Match item in matchCollection)
			{
				_urlElements[item.Groups[1].Value] = item.Groups[2].Value;
			}
		}
		if (!_urlElements.TryGetValue("Type", out var value))
		{
			Log.Warning("Text URL ('" + _fullUrlString + "'): No explicit or implicit type defined in '" + _fullUrlString + "'");
			return false;
		}
		if (!urlHandlers.TryGetValue(value, out _handler))
		{
			Log.Warning("Text URL ('" + _fullUrlString + "'): No handler for type '" + value + "'");
			return false;
		}
		if (_allowedTypes != null && !_allowedTypes.Contains(value))
		{
			Log.Warning("Text URL ('" + _fullUrlString + "'): URL type '" + value + "' not allowed on this label");
			return false;
		}
		return true;
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

	[PublicizedFrom(EAccessModifier.Private)]
	static LabelUrlUtils()
	{
		urlHandlers = new Dictionary<string, SHandlerData>();
		urlMatcher = new Regex("([^|]+)>([^|]+)", RegexOptions.Compiled | RegexOptions.Singleline);
		RegisterLabelUrlHandler("Chat", handleChatTargetUrlClick, handleChatTargetUrlHover);
		RegisterLabelUrlHandler("HTTP", handleHttpUrl, handleHttpUrlHover);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleChatTargetUrlHover(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText)
	{
		_tooltipText = null;
		if (!_urlElements.TryGetValue("ChatType", out var value))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): No ChatType defined");
			return;
		}
		_urlElements.TryGetValue("Sender", out var value2);
		string _targetTooltip;
		if (!EnumUtils.TryParse<EChatType>(value, out var _result))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): Invalid chat type value '" + value + "'");
		}
		else if (XUiC_Chat.GetChatTargetTooltip(_sender.xui, _result, value2, out _targetTooltip))
		{
			_tooltipText = "Chat with: " + _targetTooltip;
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
	public static void handleHttpUrlHover(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText)
	{
		_tooltipText = null;
		if (!_urlElements.TryGetValue("Target", out var value))
		{
			Log.Warning("Web URL (" + _sourceUrl + "): No Target defined");
		}
		else
		{
			_tooltipText = value;
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
}
