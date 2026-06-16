using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using UnityEngine;

namespace Webserver.Permissions;

public class AdminWebModules : AdminSectionAbs
{
	public readonly struct WebModule
	{
		public readonly string Name;

		public readonly int LevelGlobal;

		public readonly int[] LevelPerMethod;

		public readonly bool IsDefault;

		public WebModule(string _name, int _level, bool _isDefault)
		{
			LevelPerMethod = null;
			Name = _name;
			LevelGlobal = _level;
			IsDefault = _isDefault;
		}

		public WebModule(string _name, int _levelGlobal, int[] _levelPerMethod, bool _isDefault)
		{
			if (_levelPerMethod != null && _levelPerMethod.Length != 7)
			{
				LevelPerMethod = createDefaultPerMethodArray();
				for (int i = 0; i < 7; i++)
				{
					if (_levelPerMethod != null && i < _levelPerMethod.Length)
					{
						LevelPerMethod[i] = _levelPerMethod[i];
					}
				}
			}
			else
			{
				LevelPerMethod = _levelPerMethod;
			}
			Name = _name;
			LevelGlobal = _levelGlobal;
			IsDefault = _isDefault;
		}

		public void ToXml(XmlElement _parent)
		{
			bool num = LevelPerMethod != null;
			XmlElement element = _parent.AddXmlElement("module").SetAttrib("name", Name);
			int levelGlobal = LevelGlobal;
			XmlElement node = element.SetAttrib("permission_level", levelGlobal.ToString());
			if (!num)
			{
				return;
			}
			for (int i = 0; i < LevelPerMethod.Length; i++)
			{
				ERequestMethod enumValue = (ERequestMethod)i;
				int num2 = LevelPerMethod[i];
				if (num2 != -2147483647)
				{
					node.AddXmlElement("method").SetAttrib("name", enumValue.ToStringCached()).SetAttrib("permission_level", (num2 == int.MinValue) ? "inherit" : num2.ToString());
				}
			}
		}

		public static bool TryParse(XmlElement _element, out WebModule _result)
		{
			_result = default(WebModule);
			if (!_element.TryGetAttribute("name", out var _result2))
			{
				Log.Warning("[Web] [Perms] Ignoring module-entry because of missing 'name' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.TryGetAttribute("permission_level", out var _result3))
			{
				Log.Warning("[Web] [Perms] Ignoring module-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			if (!int.TryParse(_result3, out var result))
			{
				Log.Warning("[Web] [Perms] Ignoring module-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			int[] array = null;
			foreach (XmlNode childNode in _element.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				XmlElement xmlElement = (XmlElement)childNode;
				if (xmlElement.Name != "method")
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, invalid element name: " + xmlElement.OuterXml);
					continue;
				}
				if (!xmlElement.TryGetAttribute("name", out var _result4))
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, missing 'name' attribute: " + xmlElement.OuterXml);
					continue;
				}
				if (!EnumUtils.TryParse<ERequestMethod>(_result4, out var _result5, _ignoreCase: true))
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, unknown method name in 'name' attribute: " + xmlElement.OuterXml);
					continue;
				}
				if (_result5 >= ERequestMethod.Count)
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, invalid method name in 'name' attribute: " + xmlElement.OuterXml);
					continue;
				}
				if (!xmlElement.TryGetAttribute("permission_level", out _result3))
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, missing 'permission_level' attribute: " + xmlElement.OuterXml);
					continue;
				}
				int result2;
				if (_result3.EqualsCaseInsensitive("inherit"))
				{
					result2 = int.MinValue;
				}
				else if (!int.TryParse(_result3, out result2))
				{
					Log.Warning("[Web] [Perms] Ignoring module child element, invalid (non-numeric) value for 'permission_level' attribute: " + xmlElement.OuterXml);
					continue;
				}
				if (array == null)
				{
					array = createDefaultPerMethodArray();
				}
				array[(int)_result5] = result2;
			}
			_result = new WebModule(_result2, result, array, _isDefault: false);
			return true;
		}

		[MustUseReturnValue]
		public WebModule SetLevelGlobal(int _level)
		{
			int[] array = ((LevelPerMethod == null) ? null : new int[LevelPerMethod.Length]);
			if (array != null)
			{
				Array.Copy(LevelPerMethod, array, array.Length);
			}
			return new WebModule(Name, _level, array, _isDefault: false);
		}

		[MustUseReturnValue]
		public WebModule SetLevelForMethod(ERequestMethod _method, int _level)
		{
			int[] array = createDefaultPerMethodArray();
			if (LevelPerMethod != null)
			{
				Array.Copy(LevelPerMethod, array, array.Length);
			}
			array[(int)_method] = _level;
			return new WebModule(Name, LevelGlobal, array, _isDefault: false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int[] createDefaultPerMethodArray()
		{
			int[] array = new int[7];
			for (int i = 0; i < 7; i++)
			{
				array[i] = -2147483647;
			}
			return array;
		}

		[MustUseReturnValue]
		public WebModule FixPermissionLevelsFromKnownModule(WebModule _knownModule)
		{
			if (_knownModule.LevelPerMethod == null)
			{
				if (LevelPerMethod != null)
				{
					return new WebModule(Name, LevelGlobal, _isDefault: false);
				}
				return this;
			}
			WebModule result = this;
			for (int i = 0; i < _knownModule.LevelPerMethod.Length; i++)
			{
				if (result.LevelPerMethod == null || result.LevelPerMethod[i] == -2147483647)
				{
					result = result.SetLevelForMethod((ERequestMethod)i, _knownModule.LevelPerMethod[i]);
				}
			}
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, WebModule> modules = new CaseInsensitiveStringDictionary<WebModule>();

	public const int MethodLevelInheritGlobal = int.MinValue;

	public const string MethodLevelInheritKeyword = "inherit";

	public const int MethodLevelNotSupported = -2147483647;

	public const int PermissionLevelUser = 1000;

	public const int PermissionLevelGuest = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, WebModule> knownModules = new CaseInsensitiveStringDictionary<WebModule>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<WebModule> allModulesList = new List<WebModule>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WebModule defaultModulePermission = new WebModule("", 0, _isDefault: true);

	public static AdminWebModules Instance => GameManager.Instance.adminTools.WebModules;

	public AdminWebModules(AdminTools _parent)
		: base(_parent, "webmodules")
	{
	}

	public override void Clear()
	{
		allModulesList.Clear();
		modules.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		allModulesList.Clear();
		if (WebModule.TryParse(_childElement, out var _result))
		{
			if (knownModules.TryGetValue(_result.Name, out var value))
			{
				_result = _result.FixPermissionLevelsFromKnownModule(value);
			}
			modules[_result.Name] = _result;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement parent = _root.AddXmlElement(SectionTypeName);
		foreach (var (_, webModule2) in modules)
		{
			webModule2.ToXml(parent);
		}
	}

	public void AddModule(WebModule _module)
	{
		lock (Parent)
		{
			allModulesList.Clear();
			modules[_module.Name] = _module;
			Parent.Save();
		}
	}

	public bool RemoveModule(string _module)
	{
		lock (Parent)
		{
			allModulesList.Clear();
			bool num = modules.Remove(_module);
			if (num)
			{
				Parent.Save();
			}
			return num;
		}
	}

	public List<WebModule> GetModules()
	{
		lock (Parent)
		{
			if (allModulesList.Count != 0)
			{
				return allModulesList;
			}
			foreach (var (key, webModule2) in knownModules)
			{
				allModulesList.Add(modules.TryGetValue(key, out var value) ? value : webModule2);
			}
			return allModulesList;
		}
	}

	public void AddKnownModule(WebModule _module)
	{
		if (!_module.IsDefault)
		{
			Log.Warning("Call to AddKnownModule with IsDefault==false! From:\n" + StackTraceUtility.ExtractStackTrace());
		}
		if (string.IsNullOrEmpty(_module.Name))
		{
			return;
		}
		lock (Parent)
		{
			allModulesList.Clear();
			knownModules[_module.Name] = _module;
			if (modules.TryGetValue(_module.Name, out var value))
			{
				value = value.FixPermissionLevelsFromKnownModule(_module);
				modules[_module.Name] = value;
			}
		}
	}

	public bool IsKnownModule(string _module)
	{
		if (string.IsNullOrEmpty(_module))
		{
			return false;
		}
		lock (Parent)
		{
			return knownModules.ContainsKey(_module);
		}
	}

	public bool ModuleAllowedWithLevel(string _module, int _level)
	{
		lock (Parent)
		{
			return GetModule(_module).LevelGlobal >= _level;
		}
	}

	public WebModule GetModule(string _module)
	{
		lock (Parent)
		{
			if (modules.TryGetValue(_module, out var value))
			{
				return value;
			}
			return knownModules.TryGetValue(_module, out value) ? value : defaultModulePermission;
		}
	}
}
