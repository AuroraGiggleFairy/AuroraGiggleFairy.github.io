using System;
using System.Collections;
using System.Xml.Linq;

public class WorldGlobalFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <world> found!");
		}
		foreach (XElement item in root.Elements("environment"))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			foreach (XElement item2 in item.Elements("property"))
			{
				dynamicProperties.Add(item2);
			}
			WorldEnvironment.Properties = dynamicProperties;
		}
		WorldEnvironment.OnXMLChanged();
		yield break;
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(Load(xmlFile));
	}
}
