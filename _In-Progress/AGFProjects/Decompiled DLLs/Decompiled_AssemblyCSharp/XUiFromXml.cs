using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NCalc;
using NCalc.Domain;
using UnityEngine;

public static class XUiFromXml
{
	public enum DebugLevel
	{
		Off,
		Warning,
		Verbose
	}

	public class StyleData
	{
		public readonly string Name;

		public readonly string Type;

		public readonly string KeyName;

		public readonly Dictionary<string, StyleEntryData> StyleEntries = new Dictionary<string, StyleEntryData>();

		public StyleData(string _name, string _type)
		{
			Type = _type;
			Name = _name;
			if (!string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Name))
			{
				KeyName = Type + "." + Name;
			}
			else if (!string.IsNullOrEmpty(Type))
			{
				KeyName = Type;
			}
			else if (!string.IsNullOrEmpty(Name))
			{
				KeyName = Name;
			}
			else
			{
				Log.Error("[XUi] Style entry with neither 'Type' or 'Name' attribute");
			}
		}
	}

	public class StyleEntryData
	{
		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public string value;

		public string Value
		{
			get
			{
				string text = value;
				if (!IsStyleRef(text))
				{
					return text;
				}
				string key = text.Substring(1, text.Length - 2);
				if (!styles["global"].StyleEntries.TryGetValue(key, out var styleEntryData))
				{
					return text;
				}
				return value = styleEntryData.Value;
			}
		}

		public StyleEntryData(string _name, string _value)
		{
			Name = _name;
			value = _value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> windowData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedWindows;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> controlData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Dictionary<string, object>> controlDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, StyleData> styles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Expression> expressionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XmlFile mainXuiXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DebugLevel debugXuiLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiController ncalcCurrentViewParent;

	public static DebugLevel DebugXuiLoading => debugXuiLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiFromXml()
	{
		string launchArgument = GameUtils.GetLaunchArgument("debugxui");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				debugXuiLoading = DebugLevel.Verbose;
			}
			else
			{
				debugXuiLoading = DebugLevel.Warning;
			}
		}
		else
		{
			debugXuiLoading = DebugLevel.Off;
		}
	}

	public static void ClearLoadingData()
	{
		mainXuiXml = null;
		windowData?.Clear();
		windowData = null;
		controlData?.Clear();
		controlData = null;
		usedWindows?.Clear();
		usedWindows = null;
		controlDefaults?.Clear();
		controlDefaults = null;
		usedControls?.Clear();
		usedControls = null;
		if (expressionCache != null)
		{
			foreach (Expression item in expressionCache?.Values)
			{
				item.EvaluateFunction -= NCalcIdentifierDefinedFunction;
				item.EvaluateParameter -= NCalcEvaluateParameter;
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
		if (mainXuiXml != null && windowData.Count > 0 && controlData.Count > 0)
		{
			return styles.Count > 0;
		}
		return false;
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		if (!GameManager.IsDedicatedServer)
		{
			if (XUi.Stopwatch == null)
			{
				XUi.Stopwatch = new MicroStopwatch();
			}
			if (!XUi.Stopwatch.IsRunning)
			{
				XUi.Stopwatch.Reset();
				XUi.Stopwatch.Start();
			}
			if (windowData == null)
			{
				windowData = new Dictionary<string, XElement>(StringComparer.Ordinal);
			}
			if (usedWindows == null)
			{
				usedWindows = new SortedDictionary<string, int>(StringComparer.Ordinal);
			}
			if (controlData == null)
			{
				controlData = new Dictionary<string, XElement>(StringComparer.Ordinal);
			}
			if (controlDefaults == null)
			{
				controlDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
			}
			if (usedControls == null)
			{
				usedControls = new SortedDictionary<string, int>(StringComparer.Ordinal);
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
			if (!root.HasElements)
			{
				throw new Exception("No elements found!");
			}
			switch (root.Name.LocalName)
			{
			case "xui":
				mainXuiXml = _xmlFile;
				break;
			case "windows":
				loadWindows(_xmlFile);
				break;
			case "styles":
				loadStyles(_xmlFile);
				break;
			case "controls":
				loadControls(_xmlFile);
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
		foreach (KeyValuePair<string, int> usedControl in usedControls)
		{
			if (debugXuiLoading != DebugLevel.Off && (debugXuiLoading != DebugLevel.Warning || usedControl.Value <= 0))
			{
				if (usedControl.Value > 0)
				{
					Log.Out($"[XUi] Control '{usedControl.Key}' used {usedControl.Value} times!");
				}
				else
				{
					Log.Warning("[XUi] Control '" + usedControl.Key + "' not used!");
				}
			}
		}
		foreach (KeyValuePair<string, int> usedWindow in usedWindows)
		{
			if (debugXuiLoading != DebugLevel.Off && (debugXuiLoading != DebugLevel.Warning || usedWindow.Value <= 0))
			{
				if (usedWindow.Value > 0)
				{
					Log.Out($"[XUi] Window '{usedWindow.Key}' used {usedWindow.Value} times!");
				}
				else
				{
					Log.Warning("[XUi] Window '" + usedWindow.Key + "' not used!");
				}
			}
		}
	}

	public static void GetWindowGroupNames(out List<string> windowGroupNames)
	{
		windowGroupNames = new List<string>();
		foreach (XElement item in mainXuiXml.XmlDoc.Root.Elements("ruleset"))
		{
			foreach (XElement item2 in item.Elements("window_group"))
			{
				if (item2.HasAttribute("name"))
				{
					string attribute = item2.GetAttribute("name");
					if (!windowGroupNames.Contains(attribute))
					{
						windowGroupNames.Add(attribute);
					}
				}
			}
		}
	}

	public static void LoadXui(XUi _xui, string windowGroupToLoad)
	{
		XElement root = mainXuiXml.XmlDoc.Root;
		if (root.HasAttribute("ruleset"))
		{
			_xui.Ruleset = root.GetAttribute("ruleset");
		}
		foreach (XElement item in root.Elements("ruleset"))
		{
			if (item.HasAttribute("name") && !item.GetAttribute("name").EqualsCaseInsensitive(_xui.Ruleset))
			{
				continue;
			}
			if (item.HasAttribute("scale"))
			{
				_xui.SetScale(StringParsers.ParseFloat(item.GetAttribute("scale")));
			}
			if (item.HasAttribute("stackpanel_scale"))
			{
				_xui.SetStackPanelScale(StringParsers.ParseFloat(item.GetAttribute("stackpanel_scale")));
			}
			if (item.HasAttribute("ignore_missing_class"))
			{
				_xui.IgnoreMissingClass = StringParsers.ParseBool(item.GetAttribute("ignore_missing_class"));
			}
			foreach (XElement item2 in item.Elements("window_group"))
			{
				string text = "";
				if (item2.HasAttribute("name"))
				{
					text = item2.GetAttribute("name");
				}
				if (_xui.FindWindowGroupByName(text) != null || !windowGroupToLoad.Equals(text))
				{
					continue;
				}
				XUiWindowGroup.EHasActionSetFor hasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
				if (item2.HasAttribute("actionSet"))
				{
					switch (item2.GetAttribute("actionSet").ToLower().Trim())
					{
					case "true":
						hasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
						break;
					case "false":
						hasActionSetFor = XUiWindowGroup.EHasActionSetFor.None;
						break;
					case "controller":
						hasActionSetFor = XUiWindowGroup.EHasActionSetFor.OnlyController;
						break;
					case "keyboard":
						hasActionSetFor = XUiWindowGroup.EHasActionSetFor.OnlyKeyboard;
						break;
					}
				}
				string defaultSelectedName = "";
				if (item2.HasAttribute("defaultSelected"))
				{
					defaultSelectedName = item2.GetAttribute("defaultSelected");
				}
				XUiWindowGroup xUiWindowGroup = new XUiWindowGroup(text, hasActionSetFor, defaultSelectedName)
				{
					xui = _xui
				};
				if (item2.HasAttribute("stack_panel_y_offset") && int.TryParse(item2.GetAttribute("stack_panel_y_offset"), out var result))
				{
					xUiWindowGroup.StackPanelYOffset = result;
				}
				int stackPanelPadding = 16;
				if (item2.HasAttribute("stack_panel_padding") && int.TryParse(item2.GetAttribute("stack_panel_padding"), out result))
				{
					xUiWindowGroup.StackPanelPadding = stackPanelPadding;
				}
				if (item2.HasAttribute("open_backpack_on_open"))
				{
					StringParsers.TryParseBool(item2.GetAttribute("open_backpack_on_open"), out xUiWindowGroup.openBackpackOnOpen);
				}
				if (item2.HasAttribute("close_compass_on_open"))
				{
					StringParsers.TryParseBool(item2.GetAttribute("close_compass_on_open"), out xUiWindowGroup.closeCompassOnOpen);
				}
				if (item2.HasAttribute("controller"))
				{
					string attribute = item2.GetAttribute("controller");
					Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", attribute);
					if (typeWithPrefix != null)
					{
						xUiWindowGroup.Controller = (XUiController)Activator.CreateInstance(typeWithPrefix);
						xUiWindowGroup.Controller.WindowGroup = xUiWindowGroup;
					}
					else
					{
						logForNode(_xui.IgnoreMissingClass ? LogType.Warning : LogType.Error, item2, "[XUi] Controller '" + attribute + "' not found, using base XUiController");
						xUiWindowGroup.Controller = new XUiController
						{
							WindowGroup = xUiWindowGroup
						};
					}
				}
				else
				{
					xUiWindowGroup.Controller = new XUiController
					{
						WindowGroup = xUiWindowGroup
					};
				}
				xUiWindowGroup.Controller.xui = _xui;
				if (xUiWindowGroup.Controller is XUiC_DragAndDropWindow dragAndDrop)
				{
					_xui.dragAndDrop = dragAndDrop;
				}
				if (xUiWindowGroup.Controller is XUiC_OnScreenIcons onScreenIcons)
				{
					_xui.onScreenIcons = onScreenIcons;
				}
				foreach (XElement item3 in item2.Elements("window"))
				{
					string text2 = "";
					if (item3.HasAttribute("name"))
					{
						text2 = item3.GetAttribute("name");
					}
					XUiV_Window xUiV_Window = null;
					XElement value;
					if (item3.HasElements)
					{
						xUiV_Window = parseWindow(text2, item3, item3, xUiWindowGroup);
					}
					else if (windowData.TryGetValue(text2, out value))
					{
						usedWindows[text2]++;
						xUiV_Window = parseWindow(text2, item3, value, xUiWindowGroup);
					}
					else if (debugXuiLoading != DebugLevel.Off)
					{
						Log.Warning("[XUi] window name '" + text2 + "' not found for window group '" + xUiWindowGroup.ID + "'!");
					}
					if (xUiV_Window != null)
					{
						_xui.AddWindow(xUiV_Window);
					}
				}
				_xui.WindowGroups.Add(xUiWindowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiV_Window parseWindow(string _name, XElement _windowCallingElement, XElement _windowContentElement, XUiWindowGroup _windowGroup)
	{
		XUiView xUiView = parseViewComponents(_windowContentElement, _windowGroup, _windowGroup.Controller);
		if (xUiView == null)
		{
			return null;
		}
		if (!(xUiView is XUiV_Window xUiV_Window))
		{
			Log.Error("[XUi] Failed parsing window name '" + _name + "' in window group '" + _windowGroup.ID + "': Named element is not a 'Window' view but a '" + xUiView.GetType().Name + "'!");
			return null;
		}
		if (_windowCallingElement.HasAttribute("anchor"))
		{
			xUiV_Window.Anchor = _windowCallingElement.GetAttribute("anchor");
		}
		if (_windowCallingElement.HasAttribute("pos"))
		{
			xUiV_Window.Position = StringParsers.ParseVector2i(_windowCallingElement.GetAttribute("pos"));
		}
		return xUiV_Window;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadWindows(XmlFile _xmlFile)
	{
		foreach (XElement item in _xmlFile.XmlDoc.Root.Elements())
		{
			if (!item.HasAttribute("platform") || XUi.IsMatchingPlatform(item.GetAttribute("platform")))
			{
				string attribute = item.GetAttribute("name");
				if (string.IsNullOrEmpty(attribute))
				{
					Log.Warning("[XUi] windows.xml top level element without non-empty 'name' attribute");
				}
				else if (windowData.TryAdd(attribute, item))
				{
					usedWindows[attribute] = 0;
				}
				else if (debugXuiLoading != DebugLevel.Off)
				{
					Log.Warning("[XUi] window data already contains '" + attribute + "'");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadControls(XmlFile _xmlFile)
	{
		foreach (XElement item in _xmlFile.XmlDoc.Root.Elements())
		{
			string localName = item.Name.LocalName;
			Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
			foreach (XAttribute item2 in item.Attributes())
			{
				string text = item2.Value;
				if (IsStyleRef(text))
				{
					string text2 = text.Substring(1, text.Length - 2);
					if (!styles["global"].StyleEntries.TryGetValue(text2, out var value))
					{
						logForNode(LogType.Error, item, "[XUi] Global style key '" + text2 + "' not found!");
						continue;
					}
					text = value.Value;
				}
				if (text.IndexOf("\\n", StringComparison.Ordinal) >= 0)
				{
					text = text.Replace("\\n", "\n", StringComparison.Ordinal);
				}
				dictionary[item2.Name.LocalName] = text;
			}
			int num = item.Elements().Count();
			XElement value2 = item.Elements().First();
			if (num > 1)
			{
				if (debugXuiLoading != DebugLevel.Off)
				{
					Log.Out("[XUi] Control '{0}' cannot have more than a single child node!", localName);
				}
			}
			else if (num < 1)
			{
				if (debugXuiLoading != DebugLevel.Off)
				{
					Log.Out("[XUi] Control '{0}' must have a single child node!", localName);
				}
				continue;
			}
			if (controlData.ContainsKey(localName) && debugXuiLoading != DebugLevel.Off)
			{
				Log.Warning("[XUi] Control '" + localName + "' already defined, overwriting!");
			}
			controlData[localName] = value2;
			controlDefaults[localName] = dictionary;
			usedControls[localName] = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadStyles(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <styles> found!");
		}
		foreach (XElement item in root.Elements())
		{
			StyleData value;
			if (item.Name == "global")
			{
				if (!styles.TryGetValue("global", out value))
				{
					value = new StyleData("global", string.Empty);
					styles.Add(value.KeyName, value);
				}
			}
			else
			{
				string text = null;
				string text2 = null;
				if (item.HasAttribute("name"))
				{
					text = item.GetAttribute("name");
				}
				if (item.HasAttribute("type"))
				{
					text2 = item.GetAttribute("type");
				}
				if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2))
				{
					Log.Warning("[XUi] Style entry with neither 'Type' or 'Name' attribute");
					continue;
				}
				StyleData styleData = new StyleData(text, text2);
				if (styles.TryGetValue(styleData.KeyName, out value))
				{
					if (debugXuiLoading != DebugLevel.Off)
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
				if (!item2.HasAttribute("name"))
				{
					Log.Error("[XUi] Style '" + value.KeyName + "' contains a entry that has no 'name' attribute!");
					continue;
				}
				if (!item2.HasAttribute("value"))
				{
					Log.Error("[XUi] Style '" + value.KeyName + "' contains a entry that has no 'value' attribute!");
					continue;
				}
				string attribute = item2.GetAttribute("value");
				string attribute2 = item2.GetAttribute("name");
				StyleEntryData value2 = new StyleEntryData(attribute2, attribute);
				value.StyleEntries[attribute2] = value2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView parseViewComponents(XElement _node, XUiWindowGroup _windowGroup, XUiController _parent = null, string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
	{
		if (_node.HasAttribute("platform") && !XUi.IsMatchingPlatform(_node.GetAttribute("platform")))
		{
			return null;
		}
		XUi xui = _windowGroup.xui;
		string localName = _node.Name.LocalName;
		string text = localName;
		if (nodeNameOverride == "" && _node.HasAttribute("name"))
		{
			text = _node.GetAttribute("name");
		}
		else if (nodeNameOverride != "")
		{
			text = nodeNameOverride;
		}
		bool _parseChildren = true;
		bool _parseControllerAndAttributes = true;
		bool _replacedByTemplate = false;
		parseControlParams(_node, _parent, _controlParams);
		XUiView xUiView = localName switch
		{
			"window" => new XUiV_Window(text), 
			"panel" => new XUiV_Panel(text), 
			"rect" => new XUiV_Rect(text), 
			"sprite" => new XUiV_Sprite(text), 
			"filledsprite" => new XUiV_FilledSprite(text), 
			"texture" => new XUiV_Texture(text), 
			"label" => new XUiV_Label(text), 
			"textlist" => new XUiV_TextList(text), 
			"widget" => new XUiV_Widget(text), 
			"grid" => new XUiV_Grid(text), 
			"table" => new XUiV_Table(text), 
			"button" => new XUiV_Button(text), 
			"gamepad_icon" => new XUiV_GamepadIcon(text), 
			_ => createFromTemplate(localName, text, _node, _parent, _windowGroup, _controlParams, ref _parseChildren, ref _parseControllerAndAttributes, ref _replacedByTemplate), 
		};
		if (_parseControllerAndAttributes)
		{
			xUiView.xui = xui;
			setController(_node, xUiView, _parent);
			xUiView.SetDefaults(_parent);
			parseAttributes(_node, xUiView, _parent, _controlParams);
			xUiView.SetPostParsingDefaults(_parent);
		}
		xUiView.Controller.WindowGroup = _windowGroup;
		if (!_replacedByTemplate && xUiView.RepeatContent)
		{
			if (_node.Elements().Count() != 1)
			{
				if (debugXuiLoading != DebugLevel.Off)
				{
					logForNode(LogType.Warning, _node, "[XUi] XUiFromXml::parseByElementName: Invalid repeater child count. Must have one child element.");
				}
			}
			else
			{
				int repeatCount = xUiView.RepeatCount;
				if (_controlParams == null)
				{
					_controlParams = new CaseInsensitiveStringDictionary<object>();
				}
				_controlParams["repeat_count"] = repeatCount;
				XElement other = _node.Elements().First();
				for (int i = 0; i < repeatCount; i++)
				{
					_controlParams["repeat_i"] = i;
					xUiView.setRepeatContentTemplateParams(_controlParams, i);
					XElement xElement = new XElement(other);
					_node.Add(xElement);
					parseViewComponents(xElement, _windowGroup, xUiView.Controller, i.ToString(), _controlParams);
					xElement.Remove();
				}
			}
			_parseChildren = false;
		}
		if (_parseChildren)
		{
			foreach (XElement item in _node.Elements())
			{
				parseViewComponents(item, _windowGroup, xUiView.Controller, "", _controlParams);
			}
		}
		return xUiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createFromTemplate(string _templateName, string _viewName, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, Dictionary<string, object> _outerControlParams, ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate)
	{
		if (!controlData.TryGetValue(_templateName, out var value))
		{
			if (debugXuiLoading != DebugLevel.Off)
			{
				logForNode(LogType.Warning, _node, "[XUi] View \"" + _templateName + "\" not found!");
			}
			return createEmptyView(_viewName, _parent, _windowGroup, ref _parseControllerAndAttributes);
		}
		if (_node.HasElements)
		{
			if (debugXuiLoading != DebugLevel.Off)
			{
				logForNode(LogType.Warning, _node, "[XUi] Instantiation of templates may not have any child nodes!");
			}
			_parseChildren = false;
			return createEmptyView(_viewName, _parent, _windowGroup, ref _parseControllerAndAttributes);
		}
		Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
		_outerControlParams?.CopyTo(dictionary);
		if (_parent?.ViewComponent != null)
		{
			dictionary["width"] = _parent.ViewComponent.InnerSize.x;
			dictionary["height"] = _parent.ViewComponent.InnerSize.y;
			dictionary["outerwidth"] = _parent.ViewComponent.Size.x;
			dictionary["outerheight"] = _parent.ViewComponent.Size.y;
		}
		if (controlDefaults.TryGetValue(_templateName, out var value2))
		{
			value2.CopyTo(dictionary, _overwriteExisting: true);
		}
		parseAttributes(_node, null, null, dictionary);
		XElement xElement = new XElement(value);
		usedControls[_templateName]++;
		_node.Add(xElement);
		XUiView xUiView = parseViewComponents(xElement, _windowGroup, _parent, _viewName, dictionary);
		if (xUiView == null)
		{
			return null;
		}
		xUiView.xui = _windowGroup.xui;
		xElement.Remove();
		_parseChildren = false;
		_parseControllerAndAttributes = false;
		_replacedByTemplate = true;
		return xUiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createEmptyView(string _viewName, XUiController _parent, XUiWindowGroup _windowGroup, ref bool _parseControllerAndAttributes)
	{
		XUiView xUiView = new XUiView(_viewName)
		{
			xui = _windowGroup.xui
		};
		xUiView.Controller = new XUiController
		{
			xui = _windowGroup.xui
		};
		if (_parent != null)
		{
			xUiView.Controller.Parent = _parent;
			_parent.AddChild(xUiView.Controller);
		}
		xUiView.SetDefaults(_parent);
		_parseControllerAndAttributes = false;
		return xUiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseControlParams(XElement _node, XUiController _parent, Dictionary<string, object> _controlParams)
	{
		foreach (XAttribute item in _node.Attributes())
		{
			string text = item.Value;
			bool flag = false;
			int num;
			while ((num = text.IndexOf("${", StringComparison.Ordinal)) >= 0)
			{
				int num2 = text.IndexOf('}', num);
				int count = num2 - num + 1;
				if (num2 < 0)
				{
					logForNode(LogType.Error, _node, "[XUi] Expression has unclosed parameter references: " + item.Name?.ToString() + "=" + text);
					break;
				}
				string text2 = text.Substring(num + 2, num2 - (num + 2));
				if (!expressionCache.TryGetValue(text2, out var value))
				{
					value = new Expression(text2, EvaluateOptions.IgnoreCase | EvaluateOptions.UseDoubleForAbsFunction);
					value.EvaluateFunction += NCalcIdentifierDefinedFunction;
					value.EvaluateParameter += NCalcEvaluateParameter;
					expressionCache.Add(text2, value);
				}
				ncalcCurrentViewParent = _parent;
				value.Parameters = _controlParams;
				string value2;
				try
				{
					object obj = value.Evaluate();
					value2 = ((!(obj is decimal value3)) ? ((!(obj is float value4)) ? ((!(obj is double value5)) ? obj.ToString() : value5.ToCultureInvariantString()) : value4.ToCultureInvariantString()) : value3.ToCultureInvariantString("0.########"));
				}
				catch (ArgumentException ex)
				{
					logForNode(LogType.Error, _node, "[XUi] Control parameter '" + ex.ParamName + "' undefined (in: " + item.Name?.ToString() + "=\"" + text + "\")");
					value2 = "";
				}
				catch (Exception e)
				{
					logForNode(LogType.Exception, _node, "[XUi] Control expression can not be evaluated: " + text2);
					Log.Exception(e);
					value2 = "";
				}
				ncalcCurrentViewParent = null;
				text = text.Remove(num, count).Insert(num, value2);
				flag = true;
			}
			if (flag)
			{
				item.Value = text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void NCalcEvaluateParameter(string _name, ParameterArgs _args)
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
	public static void NCalcIdentifierDefinedFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (_name.EqualsCaseInsensitive("defined") && parameters.Length == 1 && parameters[0].ParsedExpression is Identifier { Name: var name })
		{
			_args.Result = parameters[0].Parameters.ContainsKey(name);
			_args.HasResult = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttributes(XElement _node, XUiView _viewComponent, XUiController _parent, Dictionary<string, object> _controlParams = null)
	{
		string localName = _node.Name.LocalName;
		if (styles.TryGetValue(localName, out var value))
		{
			foreach (KeyValuePair<string, StyleEntryData> styleEntry in value.StyleEntries)
			{
				StyleEntryData value2 = styleEntry.Value;
				parseAttribute(_viewComponent, value2.Name, value2.Value, _parent, _controlParams);
			}
		}
		if (_node.HasAttribute("style"))
		{
			string[] array = _node.GetAttribute("style").Replace(" ", "").Split(',');
			foreach (string text in array)
			{
				string text2 = localName + "." + text;
				if (styles.TryGetValue(text2, out value))
				{
					foreach (KeyValuePair<string, StyleEntryData> styleEntry2 in value.StyleEntries)
					{
						StyleEntryData value3 = styleEntry2.Value;
						parseAttribute(_viewComponent, value3.Name, value3.Value, _parent, _controlParams);
					}
				}
				else if (styles.TryGetValue(text, out value))
				{
					foreach (KeyValuePair<string, StyleEntryData> styleEntry3 in value.StyleEntries)
					{
						StyleEntryData value4 = styleEntry3.Value;
						parseAttribute(_viewComponent, value4.Name, value4.Value, _parent, _controlParams);
					}
				}
				else
				{
					logForNode(LogType.Error, _node, "[XUi] Style key '" + text2 + "' not found!");
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
			string text3 = item.Value;
			if (IsStyleRef(text3))
			{
				string text4 = text3.Substring(1, text3.Length - 2);
				if (!styles["global"].StyleEntries.TryGetValue(text4, out var value5))
				{
					logForNode(LogType.Error, _node, "[XUi] Global style key '" + text4 + "' not found!");
					continue;
				}
				text3 = value5.Value;
			}
			string attributeNameLower = localName2.ToLower();
			if (text3.IndexOf("\\n", StringComparison.Ordinal) >= 0)
			{
				text3 = text3.Replace("\\n", "\n", StringComparison.Ordinal);
			}
			parseAttribute(_viewComponent, attributeNameLower, text3, _parent, _controlParams);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttribute(XUiView _viewComponent, string _attributeNameLower, string _value, XUiController _parent, Dictionary<string, object> _controlParams = null)
	{
		if (_viewComponent == null)
		{
			_controlParams[_attributeNameLower] = _value;
		}
		else
		{
			_viewComponent.ParseAttributeViewAndController(_attributeNameLower, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setController(XElement _node, XUiView _viewComponent, XUiController _parent)
	{
		XUi xui = _viewComponent.xui;
		XUiController xUiController = null;
		if (_node.HasAttribute("controller"))
		{
			string attribute = _node.GetAttribute("controller");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", attribute);
			if (typeWithPrefix == null)
			{
				logForNode(xui.IgnoreMissingClass ? LogType.Warning : LogType.Error, _node, "[XUi] Controller '" + attribute + "' not found, using base XUiController");
				xUiController = (xui.IgnoreMissingClass ? new XUiControllerMissing() : new XUiController());
			}
			else if (typeWithPrefix.IsAbstract)
			{
				logForNode(LogType.Error, _node, "[XUi] Controller '" + attribute + "' not instantiable, class is abstract");
				xUiController = new XUiController();
			}
			else
			{
				xUiController = (XUiController)Activator.CreateInstance(typeWithPrefix);
			}
		}
		else
		{
			xUiController = new XUiController();
		}
		_viewComponent.Controller = xUiController;
		xUiController.xui = xui;
		if (_viewComponent.Controller is XUiC_DragAndDropWindow dragAndDrop)
		{
			xui.dragAndDrop = dragAndDrop;
		}
		if (_viewComponent.Controller is XUiC_OnScreenIcons onScreenIcons)
		{
			xui.onScreenIcons = onScreenIcons;
		}
		if (_parent != null)
		{
			xUiController.Parent = _parent;
			_parent.AddChild(_viewComponent.Controller);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logForNode(LogType _level, XElement _node, string _message)
	{
		StringBuilder stringBuilder = new StringBuilder();
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
		if (_node != null)
		{
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsStyleRef(string _value)
	{
		if (_value.StartsWith("[", StringComparison.Ordinal) && _value.IndexOf("]", StringComparison.Ordinal) == _value.Length - 1)
		{
			return _value.IndexOf("[", 1, StringComparison.Ordinal) < 0;
		}
		return false;
	}
}
