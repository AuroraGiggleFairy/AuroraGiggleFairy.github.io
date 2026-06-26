using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace XMLEditing;

public static class XMLUtils
{
	public const string IndentChars = "\t";

	public const string NewLineChars = "\r\n";

	public static string DefaultBlocksFilePath => GameIO.GetGameDir("Data/Config") + "/blocks.xml";

	public static XDocument LoadXDocument(string filePath)
	{
		using Stream stream = SdFile.OpenRead(filePath);
		return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
	}

	public static XElement SetProperty(XElement _element, string _propertyName, XName _attribName, string _value)
	{
		XElement xElement = (from e in _element.Elements(XNames.property)
			where e.GetAttribute(XNames.name) == _propertyName
			select e).FirstOrDefault();
		if (xElement == null)
		{
			xElement = new XElement(XNames.property, new XAttribute(XNames.name, _propertyName));
			_element.Add("\t");
			_element.Add(xElement);
			_element.Add("\r\n");
			_element.Add("\t");
		}
		xElement.SetAttributeValue(_attribName, _value);
		return xElement;
	}

	public static void SaveXDocument(XDocument doc, string filePath, bool omitXmlDeclaration = false)
	{
		using Stream output = SdFile.Create(filePath);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = Encoding.UTF8;
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.IndentChars = "\t";
		xmlWriterSettings.NewLineChars = "\r\n";
		xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
		xmlWriterSettings.OmitXmlDeclaration = omitXmlDeclaration;
		using XmlWriter writer = XmlWriter.Create(output, xmlWriterSettings);
		doc.WriteTo(writer);
	}

	public static void CleanAndRepairBlocksXML()
	{
		string input = SdFile.ReadAllText(DefaultBlocksFilePath);
		string pattern = "<block [^>]*>[\\s\\S]*?</block>";
		input = Regex.Replace(input, pattern, [PublicizedFrom(EAccessModifier.Internal)] (Match m) => Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(m.Value, "\r\n\\s*\r\n", "\r\n"), "/>\r\n\t(\t)?(<property name=\"(ModelOffset|OversizedBounds)\")", "/>\r\n\t$2"), "\r\n\t</block>", "\r\n</block>"), "\r\n\t <!--[\\s\\S]*?-->\r\n", "\r\n"), "(name=\"MeshDamage\" value=\")([^\"]+)\"", [PublicizedFrom(EAccessModifier.Internal)] (Match match) =>
		{
			string text = Regex.Replace(match.Groups[2].Value, " {2,3}", "\r\n\t\t");
			return match.Groups[1].Value + text + "\"";
		}), " />", "/>"), RegexOptions.Singleline);
		SdFile.WriteAllText(DefaultBlocksFilePath, input);
	}

	public static HashSet<string> ParseStringList(string targetListString, char separator)
	{
		string[] array = targetListString.Split(new char[1] { separator }, StringSplitOptions.RemoveEmptyEntries);
		HashSet<string> hashSet = new HashSet<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string item = array2[i].Trim();
			hashSet.Add(item);
		}
		return hashSet;
	}

	public static HashSet<string> GetReplacementBlockNames(HashSet<string> targetNames)
	{
		XElement root = LoadXDocument(GameIO.GetGameDir("Data/Config") + "/blockplaceholders.xml").Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <blockplaceholders> found!");
		}
		Dictionary<string, XElement> dictionary = new CaseInsensitiveStringDictionary<XElement>();
		foreach (XElement item in root.Elements("placeholder"))
		{
			string attribute = item.GetAttribute(XNames.name);
			dictionary[attribute] = item;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string targetName in targetNames)
		{
			string key = targetName.Trim();
			if (!dictionary.TryGetValue(key, out var value))
			{
				continue;
			}
			foreach (XElement item2 in value.Elements(XNames.block))
			{
				hashSet.Add(item2.GetAttribute(XNames.name).Trim());
			}
		}
		return hashSet;
	}

	public static bool AllAttributesAreEqual(XElement elementA, XElement elementB, StringComparison comparisonType)
	{
		if (elementB.Attributes().Count() != elementA.Attributes().Count())
		{
			return false;
		}
		foreach (XAttribute item in elementA.Attributes())
		{
			XAttribute xAttribute = elementB.Attribute(item.Name);
			if (xAttribute == null)
			{
				return false;
			}
			string a = item.Value.Trim();
			string b = xAttribute.Value.Trim();
			if (!string.Equals(a, b, comparisonType))
			{
				return false;
			}
		}
		return true;
	}

	public static void PopulateReplacementMap(Dictionary<string, HashSet<string>> replacementMap)
	{
		replacementMap.Clear();
		XElement root = LoadXDocument(GameIO.GetGameDir("Data/Config") + "/blockplaceholders.xml").Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <blockplaceholders> found!");
		}
		foreach (XElement item in root.Elements("placeholder"))
		{
			string key = item.GetAttribute(XNames.name).Trim();
			HashSet<string> hashSet = (replacementMap[key] = new HashSet<string>());
			foreach (XElement item2 in item.Elements(XNames.block))
			{
				hashSet.Add(item2.GetAttribute(XNames.name).Trim());
			}
		}
	}
}
