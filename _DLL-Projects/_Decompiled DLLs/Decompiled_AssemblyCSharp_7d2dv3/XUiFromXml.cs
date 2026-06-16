using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NCalc;
using NCalc.Domain;
using Platform;
using UnityEngine;

public static class XUiFromXml
{
	public enum DebugLevel
	{
		Off,
		Warning,
		Verbose
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StyleData
	{
		public readonly string KeyName;

		public readonly Dictionary<string, StyleEntryData> StyleEntries = new Dictionary<string, StyleEntryData>();

		public StyleData(string _name, string _type)
		{
			KeyName = _type + "." + _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StyleEntryData
	{
		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public string value;

		public string Value
		{
			get
			{
				string result = value;
				if (tryResolveStyleRef(result, out var _resolved))
				{
					result = (value = _resolved);
				}
				return result;
			}
		}

		public StyleEntryData(string _name, string _value)
		{
			Name = _name;
			value = _value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string globalStyleName = "global";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> windowData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedWindows;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> templateData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Dictionary<string, object>> templateDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedTemplates;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, StyleData> styles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Expression> expressionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement mainXuiXmlRoot;

	public static readonly DebugLevel DebugXuiLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiController ncalcCurrentViewParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, string[]> styleNameSplitCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Dictionary<string, string>> styleNameTypeToStyleKeyCache;

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiFromXml()
	{
		styleNameSplitCache = new Dictionary<string, string[]>();
		styleNameTypeToStyleKeyCache = new Dictionary<string, Dictionary<string, string>>();
		string launchArgument = GameUtils.GetLaunchArgument("debugxui");
		if (launchArgument != null)
		{
			DebugXuiLoading = ((!(launchArgument == "verbose")) ? DebugLevel.Warning : DebugLevel.Verbose);
		}
		else
		{
			DebugXuiLoading = DebugLevel.Off;
		}
	}

	public static void ClearLoadingData()
	{
		mainXuiXmlRoot = null;
		windowData?.Clear();
		windowData = null;
		templateData?.Clear();
		templateData = null;
		usedWindows?.Clear();
		usedWindows = null;
		templateDefaults?.Clear();
		templateDefaults = null;
		usedTemplates?.Clear();
		usedTemplates = null;
		if (expressionCache != null)
		{
			foreach (var (_, expression2) in expressionCache)
			{
				expression2.EvaluateFunction -= nCalcFunctions;
				expression2.EvaluateParameter -= nCalcEvaluateParameter;
			}
		}
		expressionCache?.Clear();
		expressionCache = null;
	}

	public static void ClearData()
	{
		ClearLoadingData();
		styles?.Clear();
		styles = null;
	}

	public static bool HasData()
	{
		if (mainXuiXmlRoot != null && windowData.Count > 0 && templateData.Count > 0)
		{
			return styles.Count > 0;
		}
		return false;
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		if (!GameManager.IsDedicatedServer)
		{
			if (windowData == null)
			{
				windowData = new Dictionary<string, XElement>(StringComparer.Ordinal);
			}
			if (usedWindows == null)
			{
				usedWindows = new SortedDictionary<string, int>(StringComparer.Ordinal);
			}
			if (templateData == null)
			{
				templateData = new Dictionary<string, XElement>(StringComparer.Ordinal);
			}
			if (templateDefaults == null)
			{
				templateDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
			}
			if (usedTemplates == null)
			{
				usedTemplates = new SortedDictionary<string, int>(StringComparer.Ordinal);
			}
			if (styles == null)
			{
				styles = new CaseInsensitiveStringDictionary<StyleData>();
			}
			if (expressionCache == null)
			{
				expressionCache = new Dictionary<string, Expression>();
			}
			XElement root = _xmlFile.XmlDoc.Root;
			if (root == null || !root.HasElements)
			{
				throw new Exception("No root element found in " + _xmlFile.Filename + "!");
			}
			switch (root.Name.LocalName)
			{
			case "xui":
				mainXuiXmlRoot = root;
				break;
			case "windows":
				loadWindows(root);
				break;
			case "styles":
				loadStyles(root);
				break;
			case "templates":
				loadTemplates(root);
				break;
			}
		}
		yield break;
	}

	public static void LoadDone(bool _logUnused)
	{
		if (!_logUnused)
		{
			return;
		}
		string key;
		int value;
		foreach (KeyValuePair<string, int> usedTemplate in usedTemplates)
		{
			usedTemplate.Deconstruct(out key, out value);
			string text = key;
			int num = value;
			if (DebugXuiLoading != DebugLevel.Off && (DebugXuiLoading != DebugLevel.Warning || num <= 0))
			{
				if (num > 0)
				{
					Log.Out($"[XUi] Template '{text}' used {num} times!");
				}
				else
				{
					Log.Out("[XUi] Template '" + text + "' not used!");
				}
			}
		}
		foreach (KeyValuePair<string, int> usedWindow in usedWindows)
		{
			usedWindow.Deconstruct(out key, out value);
			string text2 = key;
			int num2 = value;
			if (DebugXuiLoading != DebugLevel.Off && (DebugXuiLoading != DebugLevel.Warning || num2 <= 0))
			{
				if (num2 > 0)
				{
					Log.Out($"[XUi] Window '{text2}' used {num2} times!");
				}
				else
				{
					Log.Out("[XUi] Window '" + text2 + "' not used!");
				}
			}
		}
	}

	public static void GetWindowGroupNames(out List<string> _windowGroupNames)
	{
		_windowGroupNames = new List<string>();
		foreach (XElement item in mainXuiXmlRoot.Elements("window_group"))
		{
			if (item.TryGetAttribute("name", out var _result) && !_windowGroupNames.Contains(_result))
			{
				_windowGroupNames.Add(_result);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadWindows(XElement _root)
	{
		foreach (XElement item in _root.Elements())
		{
			if (!item.TryGetAttribute("platform", out var _result) || IsMatchingPlatform(_result))
			{
				string attribute = item.GetAttribute("name");
				if (string.IsNullOrEmpty(attribute))
				{
					Log.Warning("[XUi] windows.xml top level element with empty/missing 'name' attribute");
				}
				else if (windowData.TryAdd(attribute, item))
				{
					usedWindows[attribute] = 0;
				}
				else if (DebugXuiLoading != DebugLevel.Off)
				{
					Log.Warning("[XUi] window data already contains '" + attribute + "'");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadTemplates(XElement _root)
	{
		foreach (XElement item in _root.Elements())
		{
			string localName = item.Name.LocalName;
			Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
			foreach (XAttribute item2 in item.Attributes())
			{
				string _resolved = item2.Value;
				if (!tryResolveStyleRef(_resolved, out _resolved))
				{
					logForNode(LogType.Error, item, "Style key '" + _resolved + "' not found!");
					continue;
				}
				if (_resolved.IndexOf("\\n", StringComparison.Ordinal) >= 0)
				{
					_resolved = _resolved.Replace("\\n", "\n", StringComparison.Ordinal);
				}
				dictionary[item2.Name.LocalName] = _resolved;
			}
			int num = item.Elements().Count();
			if (num > 1)
			{
				if (DebugXuiLoading != DebugLevel.Off)
				{
					Log.Out("[XUi] Template '{0}' cannot have more than a single child node!", localName);
				}
			}
			else if (num < 1)
			{
				if (DebugXuiLoading != DebugLevel.Off)
				{
					Log.Warning("[XUi] Template '{0}' must have a single child node!", localName);
				}
				continue;
			}
			if (templateData.ContainsKey(localName) && DebugXuiLoading != DebugLevel.Off)
			{
				Log.Warning("[XUi] Template '" + localName + "' already defined, overwriting!");
			}
			templateData[localName] = item.Elements().First();
			templateDefaults[localName] = dictionary;
			usedTemplates[localName] = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadStyles(XElement _root)
	{
		foreach (XElement item in _root.Elements())
		{
			StyleData value;
			if (item.Name == "global")
			{
				if (!styles.TryGetValue("global", out value))
				{
					value = new StyleData("global", string.Empty);
					styles.Add("global", value);
				}
			}
			else
			{
				item.TryGetAttribute("name", out var _result);
				item.TryGetAttribute("type", out var _result2);
				if (string.IsNullOrEmpty(_result) && string.IsNullOrEmpty(_result2))
				{
					Log.Warning("[XUi] Style entry with neither 'Type' or 'Name' attribute");
					continue;
				}
				if (_result == "*")
				{
					_result = "";
				}
				if (_result2 == "*")
				{
					_result2 = "";
				}
				StyleData styleData = new StyleData(_result, _result2);
				if (styles.TryGetValue(styleData.KeyName, out value))
				{
					if (DebugXuiLoading != DebugLevel.Off)
					{
						Log.Warning("[XUi] Style '" + styleData.KeyName + "' already defined, merging contents");
					}
				}
				else
				{
					styles.Add(styleData.KeyName, styleData);
					value = styleData;
				}
			}
			foreach (XElement item2 in item.Elements())
			{
				if (!item2.TryGetAttribute("name", out var _result3))
				{
					Log.Error("[XUi] Style '" + value.KeyName + "' contains a entry that has no 'name' attribute!");
					continue;
				}
				if (!item2.TryGetAttribute("value", out var _result4))
				{
					Log.Error("[XUi] Style '" + value.KeyName + "' contains a entry that has no 'value' attribute!");
					continue;
				}
				StyleEntryData value2 = new StyleEntryData(_result3, _result4);
				value.StyleEntries[_result3] = value2;
			}
		}
	}

	public static void LoadXui(XUi _xui, string _windowGroupToLoad)
	{
		if (mainXuiXmlRoot.TryGetAttribute("scale", out var _result))
		{
			_xui.SetScale(StringParsers.ParseFloat(_result));
		}
		if (mainXuiXmlRoot.TryGetAttribute("stackpanel_scale", out var _result2))
		{
			_xui.SetStackPanelScale(StringParsers.ParseFloat(_result2));
		}
		foreach (XElement item in mainXuiXmlRoot.Elements("window_group"))
		{
			if (parseWindowGroup(_xui, _windowGroupToLoad, item, out var _windowGroup))
			{
				_xui.WindowGroups.Add(_windowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseWindowGroup(XUi _xui, string _windowGroupToLoad, XElement _groupElement, out XUiWindowGroup _windowGroup)
	{
		if (!_groupElement.TryGetAttribute("name", out var _result))
		{
			_result = "";
		}
		if (_xui.FindWindowGroupByName(_result) != null || !_windowGroupToLoad.EqualsCaseInsensitive(_result))
		{
			_windowGroup = null;
			return false;
		}
		XUiWindowGroup.EHasActionSetFor eHasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
		if (_groupElement.TryGetAttribute("actionset", out var _result2))
		{
			eHasActionSetFor = _result2.ToLower().Trim() switch
			{
				"true" => XUiWindowGroup.EHasActionSetFor.Both, 
				"false" => XUiWindowGroup.EHasActionSetFor.None, 
				"controller" => XUiWindowGroup.EHasActionSetFor.OnlyController, 
				"keyboard" => XUiWindowGroup.EHasActionSetFor.OnlyKeyboard, 
				_ => eHasActionSetFor, 
			};
		}
		if (!_groupElement.TryGetAttribute("defaultselected", out var _result3))
		{
			_result3 = "";
		}
		if (!_groupElement.TryGetAttribute("stack_panel_y_offset", out var _result4) || !int.TryParse(_result4, out var result))
		{
			result = int.MinValue;
		}
		if (!_groupElement.TryGetAttribute("stack_panel_padding", out var _result5) || !int.TryParse(_result5, out var result2))
		{
			result2 = int.MinValue;
		}
		if (!_groupElement.TryGetAttribute("open_backpack_on_open", out var _result6) || !StringParsers.TryParseBool(_result6, out var _result7))
		{
			_result7 = false;
		}
		if (!_groupElement.TryGetAttribute("close_compass_on_open", out var _result8) || !StringParsers.TryParseBool(_result8, out var _result9))
		{
			_result9 = false;
		}
		_windowGroup = new XUiWindowGroup(_xui, _result, eHasActionSetFor, _result3, result, result2, _result7, _result9);
		_windowGroup.Controller = parseController(_groupElement, _xui, _windowGroup, null);
		if (_groupElement.TryGetAttribute("always_update", out var _result10))
		{
			StringParsers.TryParseBool(_result10, out _windowGroup.Controller.AlwaysUpdate);
		}
		foreach (XElement item in _groupElement.Elements("window"))
		{
			parseWindow(item, _windowGroup, out var _);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseWindow(XElement _windowElement, XUiWindowGroup _windowGroup, out XUiV_Window _window)
	{
		_window = null;
		string text = "";
		if (_windowElement.HasAttribute("name"))
		{
			text = _windowElement.GetAttribute("name");
		}
		XElement node;
		if (_windowElement.HasElements)
		{
			node = _windowElement;
		}
		else
		{
			if (!windowData.TryGetValue(text, out var value))
			{
				if (DebugXuiLoading != DebugLevel.Off)
				{
					Log.Warning("[XUi] window name '" + text + "' not found for window group '" + _windowGroup.Id + "'!");
				}
				return false;
			}
			usedWindows[text]++;
			node = value;
		}
		XUiView xUiView = parseViewComponents(node, _windowGroup, _windowGroup.Controller);
		if (xUiView == null)
		{
			return false;
		}
		if (!(xUiView is XUiV_Window xUiV_Window))
		{
			Log.Error("[XUi] Failed parsing window name '" + text + "' in window group '" + _windowGroup.Id + "': Named element is not a 'Window' view but a '" + xUiView.GetType().Name + "'!");
			return false;
		}
		_window = xUiV_Window;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView parseViewComponents(XElement _node, XUiWindowGroup _windowGroup, XUiController _parent, string _nodeNameOverride = "", Dictionary<string, object> _templateParams = null)
	{
		if (_node.TryGetAttribute("platform", out var _result) && !IsMatchingPlatform(_result))
		{
			return null;
		}
		XUi xui = _windowGroup.xui;
		string localName = _node.Name.LocalName;
		string _result2;
		if (!string.IsNullOrEmpty(_nodeNameOverride))
		{
			_result2 = _nodeNameOverride;
		}
		else if (!_node.TryGetAttribute("name", out _result2))
		{
			_result2 = localName;
		}
		bool _parseChildren = true;
		bool _parseControllerAndAttributes = true;
		bool _replacedByTemplate = false;
		parseParams(_node, _parent, _templateParams);
		XUiView xUiView = createView(xui, _result2, localName, _node, _parent, _windowGroup, _templateParams, ref _parseChildren, ref _parseControllerAndAttributes, ref _replacedByTemplate);
		if (_parseControllerAndAttributes)
		{
			xUiView.Controller = parseController(_node, xui, _windowGroup, _parent);
			xUiView.SetDefaults(_parent);
			parseAttributes(_node, xUiView, _templateParams);
			xUiView.SetPostParsingDefaults(_parent);
		}
		parseTweeners(_node, xUiView, _parent, _templateParams);
		if (!_replacedByTemplate && xUiView.RepeatContent)
		{
			if (_node.Elements().Count() != 1)
			{
				if (DebugXuiLoading != DebugLevel.Off)
				{
					logForNode(LogType.Warning, _node, "XUiFromXml::parseByElementName: Invalid repeater child count. Must have one child element.");
				}
			}
			else
			{
				int repeatCount = xUiView.RepeatCount;
				if (_templateParams == null)
				{
					_templateParams = new CaseInsensitiveStringDictionary<object>();
				}
				_templateParams["repeat_count"] = repeatCount;
				XElement other = _node.Elements().First();
				for (int i = 0; i < repeatCount; i++)
				{
					_templateParams["repeat_i"] = i;
					xUiView.SetRepeatContentTemplateParams(_templateParams, i);
					XElement xElement = new XElement(other);
					_node.Add(xElement);
					parseViewComponents(xElement, _windowGroup, xUiView.Controller, i.ToString(), _templateParams);
					xElement.Remove();
				}
			}
			_parseChildren = false;
		}
		if (_parseChildren)
		{
			foreach (XElement item in _node.Elements())
			{
				parseViewComponents(item, _windowGroup, xUiView.Controller, "", _templateParams);
			}
		}
		return xUiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseTweeners(XElement _node, XUiView _viewComponent, XUiController _parent, Dictionary<string, object> _templateParams)
	{
		foreach (XElement item in _node.Elements("tween"))
		{
			if (parseTween(_viewComponent, item, _parent, _templateParams, out var _result))
			{
				_viewComponent.Tweeners.Add(_result);
			}
		}
		_node.Elements("tween").Remove();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseTween(XUiView _view, XElement _tweenElement, XUiController _parent, Dictionary<string, object> _templateParams, out XUiTweenAbs _result)
	{
		_result = null;
		parseParams(_tweenElement, _parent, _templateParams);
		if (!_tweenElement.TryGetAttribute("type", out var _result2))
		{
			logForNode(LogType.Error, _tweenElement, "Tween element without 'type' attribute");
			return false;
		}
		if (!EnumUtils.TryParse<XUiTweenAbs.ETweenType>(_result2, out var _result3, _ignoreCase: true))
		{
			logForNode(LogType.Error, _tweenElement, "Tween element with invalid 'type' value ('" + _result2 + "')");
			return false;
		}
		try
		{
			_result = _result3 switch
			{
				XUiTweenAbs.ETweenType.Alpha => new XUiTweenAlpha(_view), 
				XUiTweenAbs.ETweenType.Color => new XUiTweenColor(_view), 
				XUiTweenAbs.ETweenType.Fill => new XUiTweenFill(_view), 
				XUiTweenAbs.ETweenType.Height => new XUiTweenHeight(_view), 
				XUiTweenAbs.ETweenType.Position => new XUiTweenPosition(_view), 
				XUiTweenAbs.ETweenType.Rotation => new XUiTweenRotation(_view), 
				XUiTweenAbs.ETweenType.Scale => new XUiTweenScale(_view), 
				XUiTweenAbs.ETweenType.Width => new XUiTweenWidth(_view), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		catch (Exception ex)
		{
			logForNode(LogType.Error, _tweenElement, ex.Message);
			Log.Exception(ex);
			return false;
		}
		foreach (XAttribute item in _tweenElement.Attributes())
		{
			string text = item.Name.LocalName.ToLower();
			if (text == "type")
			{
				continue;
			}
			string _resolved = item.Value;
			if (!tryResolveStyleRef(_resolved, out _resolved))
			{
				logForNode(LogType.Error, _tweenElement, "Style key '" + _resolved + "' not found!");
				continue;
			}
			if (_resolved.IndexOf("\\n", StringComparison.Ordinal) >= 0)
			{
				_resolved = _resolved.Replace("\\n", "\n", StringComparison.Ordinal);
			}
			_result.ParseInitialAttributeValue(text, _resolved);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createView(XUi _xui, string _name, string _type, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, Dictionary<string, object> _templateParams, ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate)
	{
		return _type switch
		{
			"window" => new XUiV_Window(_xui, _name), 
			"panel" => new XUiV_Panel(_xui, _name), 
			"rect" => new XUiV_Rect(_xui, _name), 
			"sprite" => new XUiV_Sprite(_xui, _name), 
			"filledsprite" => new XUiV_FilledSprite(_xui, _name), 
			"texture" => new XUiV_Texture(_xui, _name), 
			"label" => new XUiV_Label(_xui, _name), 
			"textlist" => new XUiV_TextList(_xui, _name), 
			"grid" => new XUiV_Grid(_xui, _name), 
			"table" => new XUiV_Table(_xui, _name), 
			"button" => new XUiV_Button(_xui, _name), 
			"gamepad_icon" => new XUiV_GamepadIcon(_xui, _name), 
			"scrollbar" => new XUiV_ScrollBar(_xui, _name), 
			"scrollview" => new XUiV_ScrollView(_xui, _name), 
			"video" => new XUiV_Video(_xui, _name), 
			_ => createFromTemplate(_type, _name, _node, _parent, _windowGroup, _templateParams, ref _parseChildren, ref _parseControllerAndAttributes, ref _replacedByTemplate), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createFromTemplate(string _templateName, string _viewName, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, Dictionary<string, object> _outerParams, ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate)
	{
		if (!templateData.TryGetValue(_templateName, out var value))
		{
			if (DebugXuiLoading != DebugLevel.Off)
			{
				logForNode(LogType.Warning, _node, "Template \"" + _templateName + "\" not found!");
			}
			return createEmptyView(_viewName, _parent, _windowGroup, out _parseControllerAndAttributes);
		}
		if (_node.HasElements)
		{
			if (DebugXuiLoading != DebugLevel.Off)
			{
				logForNode(LogType.Warning, _node, "Instantiation of templates may not have any child nodes!");
			}
			_parseChildren = false;
			return createEmptyView(_viewName, _parent, _windowGroup, out _parseControllerAndAttributes);
		}
		Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
		_outerParams?.CopyTo(dictionary);
		if (_parent?.ViewComponent != null)
		{
			dictionary["width"] = _parent.ViewComponent.InnerSize.x;
			dictionary["height"] = _parent.ViewComponent.InnerSize.y;
			dictionary["outerwidth"] = _parent.ViewComponent.Size.x;
			dictionary["outerheight"] = _parent.ViewComponent.Size.y;
		}
		if (templateDefaults.TryGetValue(_templateName, out var value2))
		{
			value2.CopyTo(dictionary, _overwriteExisting: true);
		}
		parseAttributes(_node, null, dictionary);
		XElement xElement = new XElement(value);
		usedTemplates[_templateName]++;
		_node.Add(xElement);
		XUiView xUiView = parseViewComponents(xElement, _windowGroup, _parent, _viewName, dictionary);
		if (xUiView == null)
		{
			return null;
		}
		xElement.Remove();
		_parseChildren = false;
		_parseControllerAndAttributes = false;
		_replacedByTemplate = true;
		return xUiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createEmptyView(string _viewName, XUiController _parent, XUiWindowGroup _windowGroup, out bool _parseControllerAndAttributes)
	{
		XUiV_Empty xUiV_Empty = new XUiV_Empty(_windowGroup.xui, _viewName);
		xUiV_Empty.Controller = new XUiController
		{
			xui = _windowGroup.xui,
			WindowGroup = _windowGroup,
			Parent = _parent
		};
		xUiV_Empty.SetDefaults(_parent);
		_parseControllerAndAttributes = false;
		return xUiV_Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseParams(XElement _node, XUiController _parent, Dictionary<string, object> _templateParams)
	{
		foreach (XAttribute item in _node.Attributes())
		{
			string text = item.Value;
			bool flag = false;
			int num;
			while ((num = text.LastIndexOf("${", StringComparison.Ordinal)) >= 0)
			{
				int num2 = text.IndexOf('}', num);
				int count = num2 - num + 1;
				if (num2 < 0)
				{
					logForNode(LogType.Error, _node, $"Expression has unclosed parameter references: {item.Name}={text}");
					break;
				}
				string text2 = text.Substring(num + 2, num2 - (num + 2));
				if (!expressionCache.TryGetValue(text2, out var value))
				{
					value = new Expression(text2, EvaluateOptions.IgnoreCase | EvaluateOptions.UseDoubleForAbsFunction);
					value.EvaluateFunction += nCalcFunctions;
					value.EvaluateParameter += nCalcEvaluateParameter;
					expressionCache.Add(text2, value);
				}
				ncalcCurrentViewParent = _parent;
				value.Parameters = _templateParams;
				string value5;
				try
				{
					object obj = value.Evaluate();
					string text3 = ((obj is decimal value2) ? value2.ToCultureInvariantString("0.########") : ((obj is float value3) ? value3.ToCultureInvariantString() : ((!(obj is double value4)) ? obj.ToString() : value4.ToCultureInvariantString())));
					value5 = text3;
				}
				catch (ArgumentException ex)
				{
					logForNode(LogType.Error, _node, $"Template parameter '{ex.ParamName}' undefined (in: {item.Name}=\"{text}\")");
					value5 = "";
				}
				catch (Exception e)
				{
					logForNode(LogType.Exception, _node, "Template expression can not be evaluated: " + text2);
					Log.Exception(e);
					value5 = "";
				}
				ncalcCurrentViewParent = null;
				text = text.Remove(num, count).Insert(num, value5);
				flag = true;
			}
			if (flag)
			{
				item.Value = text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		XUiView xUiView = ncalcCurrentViewParent?.ViewComponent;
		if (xUiView != null)
		{
			switch (_name)
			{
			case "parentinnerwidth":
				_args.Result = xUiView.InnerSize.x;
				break;
			case "parentinnerheight":
				_args.Result = xUiView.InnerSize.y;
				break;
			case "parentouterwidth":
				_args.Result = xUiView.Size.x;
				break;
			case "parentouterheight":
				_args.Result = xUiView.Size.y;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFunctions(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("defined"))
		{
			nCalcFuncIdentifierDefined(_args, _ignoreCase);
		}
		else if (_name.EqualsCaseInsensitive("style"))
		{
			nCalcFuncStyle(_args, _ignoreCase);
		}
		else if (_name.EqualsCaseInsensitive("length"))
		{
			nCalcFuncLength(_args, _ignoreCase);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncLength(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length == 1 && parameters[0].Evaluate() is string text)
		{
			_args.Result = text.Length;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncStyle(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length == 1 && parameters[0].Evaluate() is string text)
		{
			if (!tryResolveStyleRef(text, out var _resolved, _expectBrackets: false))
			{
				throw new ArgumentException("", text);
			}
			_args.Result = _resolved;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncIdentifierDefined(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length == 1 && parameters[0].ParsedExpression is Identifier { Name: var name })
		{
			_args.Result = parameters[0].Parameters.ContainsKey(name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttributes(XElement _node, XUiView _viewComponent, Dictionary<string, object> _templateParams = null)
	{
		string localName = _node.Name.LocalName;
		tryApplyAttributesFromStyle("", "", _viewComponent, _templateParams);
		tryApplyAttributesFromStyle(localName, "", _viewComponent, _templateParams);
		if (_node.TryGetAttribute("style", out var _result))
		{
			if (!styleNameSplitCache.TryGetValue(_result, out var value))
			{
				value = _result.Replace(" ", "").Split(',');
				styleNameSplitCache[_result] = value;
			}
			foreach (string text in value)
			{
				if (!(tryApplyAttributesFromStyle("", text, _viewComponent, _templateParams) | tryApplyAttributesFromStyle(localName, text, _viewComponent, _templateParams)))
				{
					logForNode(LogType.Error, _node, "No style with name '" + text + "' (and optional type '" + localName + "') found!");
				}
			}
		}
		foreach (XAttribute item in _node.Attributes())
		{
			string localName2 = item.Name.LocalName;
			if (localName2 == "style")
			{
				continue;
			}
			string _resolved = item.Value;
			if (!tryResolveStyleRef(_resolved, out _resolved))
			{
				logForNode(LogType.Error, _node, "Style key '" + _resolved + "' not found!");
				continue;
			}
			if (_resolved.IndexOf("\\n", StringComparison.Ordinal) >= 0)
			{
				_resolved = _resolved.Replace("\\n", "\n", StringComparison.Ordinal);
			}
			string attributeNameLower = localName2.ToLower();
			parseAttribute(_viewComponent, attributeNameLower, _resolved, _templateParams);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryApplyAttributesFromStyle(string _typeName, string _styleName, XUiView _viewComponent, Dictionary<string, object> _templateParams = null)
	{
		if (_typeName == null)
		{
			_typeName = "";
		}
		if (_styleName == null)
		{
			_styleName = "";
		}
		if (!styleNameTypeToStyleKeyCache.TryGetValue(_styleName, out var value))
		{
			value = new Dictionary<string, string>();
			styleNameTypeToStyleKeyCache[_styleName] = value;
		}
		if (!value.TryGetValue(_typeName, out var value2))
		{
			value2 = (value[_typeName] = _typeName + "." + _styleName);
		}
		if (!styles.TryGetValue(value2, out var value3))
		{
			return false;
		}
		foreach (var (_, styleEntryData2) in value3.StyleEntries)
		{
			parseAttribute(_viewComponent, styleEntryData2.Name, styleEntryData2.Value, _templateParams);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttribute(XUiView _viewComponent, string _attributeNameLower, string _value, Dictionary<string, object> _templateParams)
	{
		if (_viewComponent == null)
		{
			_templateParams[_attributeNameLower] = _value;
		}
		else
		{
			_viewComponent.ParseInitialAttributeValue(_attributeNameLower, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiController parseController(XElement _node, XUi _xui, XUiWindowGroup _windowGroup, XUiController _parent)
	{
		XUiController xUiController = null;
		if (_node.TryGetAttribute("controller", out var _result))
		{
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", _result);
			if (typeWithPrefix == null)
			{
				logForNode(LogType.Error, _node, "Controller '" + _result + "' not found, using base XUiController");
			}
			else if (typeWithPrefix.IsAbstract)
			{
				logForNode(LogType.Error, _node, "Controller '" + _result + "' not instantiable, class is abstract");
			}
			else
			{
				xUiController = (XUiController)Activator.CreateInstance(typeWithPrefix);
			}
		}
		if (xUiController == null)
		{
			xUiController = new XUiController();
		}
		xUiController.xui = _xui;
		xUiController.WindowGroup = _windowGroup;
		xUiController.Parent = _parent;
		return xUiController;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logForNode(LogType _level, XElement _node, string _message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[XUi] ");
		stringBuilder.Append(_message);
		stringBuilder.Append(" --- hierarchy: ");
		logTree(stringBuilder, _node);
		string txt = stringBuilder.ToString();
		switch (_level)
		{
		case LogType.Error:
		case LogType.Exception:
			Log.Error(txt);
			break;
		case LogType.Warning:
			Log.Warning(txt);
			break;
		case LogType.Log:
			Log.Out(txt);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logTree(StringBuilder _sb, XElement _node)
	{
		if (_node.Parent != null)
		{
			logTree(_sb, _node.Parent);
			_sb.Append(" -> ");
		}
		if (_node.HasAttribute("name"))
		{
			_sb.Append(_node.Name);
			_sb.Append(" (");
			_sb.Append(_node.GetAttribute("name"));
			_sb.Append(")");
		}
		else
		{
			_sb.Append(_node.Name);
		}
	}

	public static bool IsMatchingPlatform(string _platformStr)
	{
		bool result = true;
		string[] array = _platformStr.Split(",");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim().ToUpper();
			if (!array[i].StartsWith("!"))
			{
				result = false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (Submission.Enabled)
			{
				if (array[j] == "SUBMISSION")
				{
					return true;
				}
				if (array[j] == "!SUBMISSION")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneWindows.IsCurrent())
			{
				if (array[j] == "WINDOWS")
				{
					return true;
				}
				if (array[j] == "!WINDOWS")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneLinux.IsCurrent())
			{
				if (array[j] == "LINUX")
				{
					return true;
				}
				if (array[j] == "!LINUX")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneOSX.IsCurrent())
			{
				if (array[j] == "OSX")
				{
					return true;
				}
				if (array[j] == "!OSX")
				{
					return false;
				}
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
			{
				if (array[j] == "STANDALONE")
				{
					return true;
				}
				if (array[j] == "!STANDALONE")
				{
					return false;
				}
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				if (array[j] == "PS5")
				{
					return true;
				}
				if (array[j] == "!PS5")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				if (array[j] == "XBOX_S")
				{
					return true;
				}
				if (array[j] == "!XBOX_S")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				if (array[j] == "XBOX_X")
				{
					return true;
				}
				if (array[j] == "!XBOX_X")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				if (array[j] == "XBOX")
				{
					return true;
				}
				if (array[j] == "!XBOX")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				if (array[j] == "CONSOLE")
				{
					return true;
				}
				if (array[j] == "!CONSOLE")
				{
					return false;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryResolveStyleRef(string _value, out string _resolved, bool _expectBrackets = true)
	{
		_resolved = _value;
		if (_expectBrackets && (!_value.StartsWith("[", StringComparison.Ordinal) || _value.IndexOf("]", StringComparison.Ordinal) != _value.Length - 1 || _value.IndexOf("[", 1, StringComparison.Ordinal) >= 0))
		{
			return true;
		}
		string text = _value;
		if (text.StartsWith("[", StringComparison.Ordinal))
		{
			text = text.Substring(1, _value.Length - 2);
		}
		int num = text.IndexOf(':');
		if (num > 0)
		{
			string text2 = text.Substring(0, num);
			string key = text.Substring(num + 1);
			if (!styles.TryGetValue(text2, out var value))
			{
				styles.TryGetValue("." + text2, out value);
			}
			if (value != null && value.StyleEntries.TryGetValue(key, out var value2))
			{
				_resolved = value2.Value;
				return true;
			}
		}
		if (styles["global"].StyleEntries.TryGetValue(text, out var value3))
		{
			_resolved = value3.Value;
			return true;
		}
		return false;
	}
}
