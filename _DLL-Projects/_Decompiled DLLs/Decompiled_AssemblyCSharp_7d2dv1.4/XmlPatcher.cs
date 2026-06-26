using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

public class XmlPatcher
{
	public enum EEvaluator
	{
		Host,
		Client
	}

	public struct PatchMethodDefinition(XpathDelegate _delegate, bool _requiresXpath)
	{
		public readonly XpathDelegate Delegate = _delegate;

		public readonly bool RequiresXpath = _requiresXpath;
	}

	public delegate int XpathDelegate(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod);

	public static readonly Dictionary<string, PatchMethodDefinition> XpathPatchMethods;

	public static IEnumerator LoadAndPatchConfig(string _configName, Action<XmlFile> _callback)
	{
		if (!_configName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
		{
			_configName += ".xml";
		}
		Exception xmlLoadException = null;
		XmlFile xmlFile = new XmlFile(GameIO.GetGameDir("Data/Config"), _configName, [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
		{
			if (_exception != null)
			{
				xmlLoadException = _exception;
			}
		});
		while (!xmlFile.Loaded && xmlLoadException == null)
		{
			yield return null;
		}
		if (xmlLoadException != null)
		{
			Log.Error("XML loader: Loading base XML '" + xmlFile.Filename + "' failed:");
			Log.Exception(xmlLoadException);
			yield break;
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (Mod loadedMod in ModManager.GetLoadedMods())
		{
			string text = loadedMod.Path + "/Config/" + _configName;
			if (!SdFile.Exists(text))
			{
				continue;
			}
			try
			{
				XmlFile xmlFile2 = ReadPatchXmlWithFixedModFolders(loadedMod, text);
				if (xmlFile2 == null)
				{
					continue;
				}
				XElement root = xmlFile2.XmlDoc.Root;
				PatchXml(xmlFile, root, xmlFile2, loadedMod);
				goto IL_01e3;
			}
			catch (Exception e)
			{
				Log.Error("XML loader: Patching '" + xmlFile.Filename + "' from mod '" + loadedMod.Name + "' failed:");
				Log.Exception(e);
				goto IL_01e3;
			}
			IL_01e3:
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		_callback(xmlFile);
	}

	public static XmlFile ReadPatchXmlWithFixedModFolders(Mod _parentMod, string _file)
	{
		string text = SdFile.ReadAllText(_file, Encoding.UTF8);
		text = text.Replace("@modfolder:", "@modfolder(" + _parentMod.Name + "):");
		string fileName = Path.GetFileName(_file);
		try
		{
			return new XmlFile(text, Path.GetDirectoryName(_file), fileName, _throwExc: true);
		}
		catch (Exception e)
		{
			Log.Error("XML loader: Loading XML patch file '" + fileName + "' from mod '" + _parentMod.Name + "' failed:");
			Log.Exception(e);
		}
		return null;
	}

	public static bool PatchXml(XmlFile _xmlFile, XElement _containerElement, XmlFile _patchFile, Mod _patchingMod)
	{
		if (_containerElement == null)
		{
			return false;
		}
		bool flag = true;
		foreach (XElement item in _containerElement.Elements())
		{
			bool flag2 = singlePatch(_xmlFile, item, _patchFile, _patchingMod);
			if (!flag2)
			{
				IXmlLineInfo xmlLineInfo = item;
				Log.Warning($"XML patch for \"{_xmlFile.Filename}\" from mod \"{_patchingMod.Name}\" did not apply: {item.GetElementString()} (line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition})");
			}
			flag = flag && flag2;
		}
		return flag;
	}

	public static IEnumerator ApplyConditionalXmlBlocks(string _xmlName, XmlFile _xmlFile, MicroStopwatch _timer, EEvaluator _evaluator, Action _errorCallback)
	{
		_timer?.ResetAndRestart();
		string xpath = ((_evaluator == EEvaluator.Host) ? "//conditional[(@evaluator='host' or not(@evaluator)) and not(ancestor::conditional[@evaluator='host' or not(@evaluator)])]" : "//conditional[@evaluator='client' and not(ancestor::conditional)]");
		List<XObject> _matchList;
		while (_xmlFile.GetXpathResults(xpath, out _matchList))
		{
			foreach (XObject item in _matchList)
			{
				if (item is XElement xElement)
				{
					XElement xElement2 = XmlPatchConditionEvaluator.FindActiveConditionalBranchElement(_xmlFile, xElement);
					if (xElement2 != null)
					{
						xElement.AddAfterSelf(xElement2.Nodes());
					}
					xElement.Remove();
				}
			}
		}
		_xmlFile.ClearXpathResults();
		_timer?.Stop();
		if ((_timer?.ElapsedMilliseconds ?? 0) > 50)
		{
			Log.Out($"Conditionals handling in {_xmlName} for {_evaluator} took {_timer.ElapsedMilliseconds} ms");
		}
		_timer?.Start();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool singlePatch(XmlFile _targetFile, XElement _patchElement, XmlFile _patchFile, Mod _patchingMod)
	{
		string localName = _patchElement.Name.LocalName;
		if (!XpathPatchMethods.TryGetValue(localName, out var value))
		{
			Log.Warning($"XML.Patch ({_patchElement.GetXPath()}, line {((IXmlLineInfo)_patchElement).LineNumber} at pos {((IXmlLineInfo)_patchElement).LinePosition}): Patch type ({localName}) unknown");
			return false;
		}
		if (value.RequiresXpath && !_patchElement.HasAttribute("xpath"))
		{
			throw new Exception($"XML.Patch ({_patchElement.GetXPath()}, line {((IXmlLineInfo)_patchElement).LineNumber} at pos {((IXmlLineInfo)_patchElement).LinePosition}): Patch element does not have an 'xpath' attribute");
		}
		string attribute = _patchElement.GetAttribute("xpath");
		try
		{
			return value.Delegate(_targetFile, attribute, _patchElement, _patchFile, _patchingMod) > 0;
		}
		catch (XPathException ex)
		{
			throw new XPathException($"XML.Patch ({_patchElement.GetXPath()}, line {((IXmlLineInfo)_patchElement).LineNumber} at pos {((IXmlLineInfo)_patchElement).LinePosition}): XPath evaluation failed: {ex.Message}");
		}
		catch (XmlException)
		{
			Log.Error($"XML.Patch ({_patchElement.GetXPath()}, line {((IXmlLineInfo)_patchElement).LineNumber} at pos {((IXmlLineInfo)_patchElement).LinePosition}): Unknown XML exception while applying patch:");
			throw;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XmlPatcher()
	{
		XpathPatchMethods = new CaseInsensitiveStringDictionary<PatchMethodDefinition>();
		ReflectionHelpers.FindTypesWithAttribute<XmlPatchMethodsClassAttribute>(TypeFoundCallback, _allowAbstract: true);
		[PublicizedFrom(EAccessModifier.Internal)]
		static void TypeFoundCallback(Type _type)
		{
			ReflectionHelpers.GetMethodsWithAttribute<XmlPatchMethodAttribute>(_type, MethodFoundCallback);
			[PublicizedFrom(EAccessModifier.Internal)]
			static void MethodFoundCallback(MethodInfo _method)
			{
				if (!ReflectionHelpers.MethodCompatibleWithDelegate<XpathDelegate>(_method))
				{
					Log.Error("XML patch method " + _method.DeclaringType.FullName + "." + _method.Name + " does not have the expected signature");
					return;
				}
				foreach (Attribute customAttribute in _method.GetCustomAttributes(typeof(XmlPatchMethodAttribute)))
				{
					if (customAttribute is XmlPatchMethodAttribute { PatchName: var patchName, RequiresXpath: var requiresXpath })
					{
						addXmlFilePatchMethod(patchName, _method, requiresXpath);
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addXmlFilePatchMethod(string _patchName, string _methodName, bool _requiresXpath = true)
	{
		MethodInfo method = typeof(XmlFile).GetMethod(_methodName);
		XpathDelegate xpathDelegate = (XpathDelegate)Delegate.CreateDelegate(typeof(XpathDelegate), method);
		if (!XpathPatchMethods.TryAdd(_patchName, new PatchMethodDefinition(xpathDelegate, _requiresXpath)))
		{
			redeclarationLog(_patchName, method);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addXmlFilePatchMethod(string _patchName, MethodInfo _method, bool _requiresXpath = true)
	{
		XpathDelegate xpathDelegate = (XpathDelegate)Delegate.CreateDelegate(typeof(XpathDelegate), _method);
		if (!XpathPatchMethods.TryAdd(_patchName, new PatchMethodDefinition(xpathDelegate, _requiresXpath)))
		{
			redeclarationLog(_patchName, _method);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void redeclarationLog(string _patchName, MethodInfo _newMethod)
	{
		MethodInfo method = XpathPatchMethods[_patchName].Delegate.Method;
		Log.Warning("XML patch method '" + _patchName + "' already defined in " + method.DeclaringType.FullName + "." + method.Name + ". Redeclaration in " + _newMethod.DeclaringType.FullName + "." + _newMethod.Name);
	}
}
