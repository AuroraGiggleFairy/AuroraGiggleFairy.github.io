using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class VehiclesFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <vehicles> found!");
		}
		Vehicle.PropertyMap = new Dictionary<string, DynamicProperties>();
		foreach (XElement item in root.Elements("vehicle"))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			string text = "";
			if (item.HasAttribute("name"))
			{
				text = item.GetAttribute("name");
			}
			foreach (XElement item2 in item.Elements("property"))
			{
				dynamicProperties.Add(item2);
			}
			Vehicle.PropertyMap.Add(text.ToLower(), dynamicProperties);
		}
		yield break;
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(Load(xmlFile));
	}
}
