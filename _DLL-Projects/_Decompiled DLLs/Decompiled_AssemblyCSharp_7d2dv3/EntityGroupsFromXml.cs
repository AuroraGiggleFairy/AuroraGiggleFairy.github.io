using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class EntityGroupsFromXml
{
	public static IEnumerator LoadEntityGroups(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No root element found!");
		}
		int _count = 0;
		foreach (XElement item in root.Elements("entitygroup"))
		{
			parseGroup(item, ref _count);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseGroup(XElement _elementGroup, ref int _count)
	{
		string attribute = _elementGroup.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on entitygroup element");
		}
		List<SEntityClassAndProb> list = new List<SEntityClassAndProb>();
		EntityGroups.list[attribute] = list;
		float _totalProb = 0f;
		foreach (XNode item in _elementGroup.Nodes())
		{
			if (item.NodeType == XmlNodeType.Text)
			{
				parseTextBasedList(((XText)item).Value, list, ref _totalProb, ref _count);
			}
			if (item.NodeType == XmlNodeType.Element && item is XElement xElement && (xElement.Name == "entity" || xElement.Name == "e"))
			{
				parseElementBased(xElement, attribute, list, ref _totalProb, ref _count);
			}
		}
		if (_totalProb > 0f)
		{
			EntityGroups.Normalize(attribute, _totalProb);
		}
		if (list.Count == 0)
		{
			throw new Exception("Empty entity groups not allowed! Group name: " + attribute);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseTextBasedList(string _nodeText, List<SEntityClassAndProb> _targetList, ref float _totalProb, ref int _count)
	{
		int num = 0;
		int num2 = _nodeText.IndexOf('\n', num);
		if (num2 < 0)
		{
			num2 = _nodeText.Length;
		}
		while (num2 >= 0)
		{
			string text = _nodeText.Substring(num, num2 - num);
			num = num2 + 1;
			if (num >= _nodeText.Length)
			{
				num2 = -1;
			}
			else
			{
				num2 = _nodeText.IndexOf('\n', num);
				if (num2 < 0)
				{
					num2 = _nodeText.Length;
				}
			}
			string text2 = text;
			float prob = 1f;
			int num3 = text.IndexOf(',');
			if (num3 >= 0)
			{
				text2 = text.Substring(0, num3);
				prob = StringParsers.ParseFloat(text, num3 + 1);
			}
			text2 = text2.Trim();
			if (text2.Length > 0)
			{
				addEntity(text2, prob, _targetList, ref _totalProb, ref _count);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseElementBased(XElement _entityElement, string _groupName, List<SEntityClassAndProb> _targetList, ref float _totalProb, ref int _count)
	{
		if (!_entityElement.TryGetAttribute("name", out var _result) && !_entityElement.TryGetAttribute("n", out _result))
		{
			throw new Exception("Attribute 'name' missing on entity in group '" + _groupName + "'");
		}
		_result = _result.Trim();
		if (_result.Length == 0)
		{
			throw new Exception("Attribute 'name' empty on entity in group '" + _groupName + "'");
		}
		float _result2 = 0f;
		if (!_entityElement.ParseAttribute("prob", ref _result2) && !_entityElement.ParseAttribute("p", ref _result2))
		{
			_result2 = 1f;
		}
		addEntity(_result, _result2, _targetList, ref _totalProb, ref _count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addEntity(string _entityName, float _prob, List<SEntityClassAndProb> _targetList, ref float _totalProb, ref int _count)
	{
		int num = 0;
		if (_entityName != "none")
		{
			num = EntityClass.FromString(_entityName);
			if (!EntityClass.list.ContainsKey(num))
			{
				throw new Exception("Entity with name '" + _entityName + "' not found");
			}
		}
		SEntityClassAndProb item = new SEntityClassAndProb
		{
			entityClassId = num,
			prob = _prob
		};
		_targetList.Add(item);
		_count++;
		_totalProb += _prob;
	}
}
