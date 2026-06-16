using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UAI;
using UnityEngine.Scripting;

[Preserve]
public class UAIFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <ai_packages> found!");
		}
		foreach (XElement item in root.Elements("ai_packages"))
		{
			parseAIPackagesNode(item);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAIPackagesNode(XElement _element)
	{
		foreach (XElement item in _element.Elements("ai_package"))
		{
			parseAIPackageNode(item);
		}
	}

	public static void Cleanup()
	{
		UAIBase.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAIPackageNode(XElement _element)
	{
		string text = "";
		float weight = 1f;
		if (_element.HasAttribute("name"))
		{
			text = _element.GetAttribute("name");
		}
		if (_element.HasAttribute("weight"))
		{
			weight = StringParsers.ParseFloat(_element.GetAttribute("weight"));
		}
		UAIPackage uAIPackage = new UAIPackage(text, weight);
		foreach (XElement item in _element.Elements("action"))
		{
			parseActionNode(uAIPackage, item);
		}
		if (!UAIBase.AIPackages.ContainsKey(text))
		{
			UAIBase.AIPackages.Add(text, uAIPackage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseActionNode(UAIPackage _package, XElement _element)
	{
		string name = "";
		float weight = 1f;
		if (_element.HasAttribute("name"))
		{
			name = _element.GetAttribute("name");
		}
		if (_element.HasAttribute("weight"))
		{
			weight = StringParsers.ParseFloat(_element.GetAttribute("weight"));
		}
		UAIAction action = new UAIAction(name, weight);
		foreach (XElement item in _element.Elements())
		{
			if (item.Name == "task")
			{
				parseTaskNode(action, item);
			}
			if (item.Name == "consideration")
			{
				parseConsiderationNode(action, item);
			}
		}
		_package.AddAction(action);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseTaskNode(UAIAction _action, XElement _element)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XAttribute item in _element.Attributes())
		{
			dictionary.Add(item.Name.LocalName, item.Value);
		}
		if (_element.HasAttribute("class"))
		{
			string text = "";
			text = _element.GetAttribute("class");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("UAI.UAITask", text);
			if (typeWithPrefix != null)
			{
				UAITaskBase uAITaskBase = (UAITaskBase)Activator.CreateInstance(typeWithPrefix);
				uAITaskBase.Name = text;
				uAITaskBase.Parameters = dictionary;
				_action.AddTask(uAITaskBase);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseConsiderationNode(UAIAction _action, XElement _element)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XAttribute item in _element.Attributes())
		{
			dictionary.Add(item.Name.LocalName, item.Value);
		}
		if (_element.HasAttribute("class"))
		{
			string text = "";
			text = _element.GetAttribute("class");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("UAI.UAIConsideration", text);
			if (typeWithPrefix != null)
			{
				UAIConsiderationBase uAIConsiderationBase = (UAIConsiderationBase)Activator.CreateInstance(typeWithPrefix);
				uAIConsiderationBase.Name = text;
				uAIConsiderationBase.Init(dictionary);
				_action.AddConsideration(uAIConsiderationBase);
			}
		}
	}
}
