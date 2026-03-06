using System;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using XMLData;

public class ColorMappingsFromXml : MonoBehaviour
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		ColorMappingData.Instance.ColorFromID.Clear();
		ColorMappingData.Instance.IDFromName.Clear();
		ColorMappingData.Instance.NameFromID.Clear();
		foreach (XElement item in root.Elements("color"))
		{
			int num = int.Parse(item.GetAttribute("id"));
			string attribute = item.GetAttribute("name");
			if (ColorUtility.TryParseHtmlString(item.GetAttribute("value"), out var color))
			{
				ColorMappingData.Instance.ColorFromID.Add(num, color);
				ColorMappingData.Instance.IDFromName.Add(attribute, num);
				ColorMappingData.Instance.NameFromID.Add(num, attribute);
			}
			else
			{
				Log.Warning($"No color value for {num} and {attribute}");
			}
		}
		yield return null;
	}
}
