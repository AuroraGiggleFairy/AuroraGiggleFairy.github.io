using System;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;

public class QualityInfoFromXml : MonoBehaviour
{
	public static IEnumerator CreateQualityInfo(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <quality> found!");
		}
		foreach (XElement item in root.Elements("quality"))
		{
			int num = -1;
			if (item.HasAttribute("key"))
			{
				num = int.Parse(item.GetAttribute("key"));
			}
			string hexColor = "#FFFFFF";
			if (item.HasAttribute("color"))
			{
				hexColor = item.GetAttribute("color");
			}
			if (num > -1)
			{
				QualityInfo.Add(num, hexColor);
			}
		}
		yield break;
	}
}
