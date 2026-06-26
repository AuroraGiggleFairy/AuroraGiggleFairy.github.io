using System;
using System.Reflection;
using System.Xml.Linq;

public class WeatherSurvivalParametersFromXml
{
	public static void Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <weathersurvival> found!");
		}
		DynamicProperties dynamicProperties = new DynamicProperties();
		foreach (XElement item in root.Elements("property"))
		{
			dynamicProperties.Add(item);
		}
		WeatherManager.ClearTemperatureOffSetHeights();
		foreach (XElement item2 in root.Descendants("TemperatureHeight"))
		{
			float height = StringParsers.ParseFloat(item2.GetAttribute("height"));
			float degreesOffset = StringParsers.ParseFloat(item2.GetAttribute("addDegrees"));
			WeatherManager.AddTemperatureOffSetHeight(height, degreesOffset);
		}
		FieldInfo[] fields = typeof(WeatherParams).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.DeclaringType == typeof(WeatherParams) && fieldInfo.FieldType == typeof(float) && dynamicProperties.Contains(fieldInfo.Name))
			{
				fieldInfo.SetValue(null, dynamicProperties.GetFloat(fieldInfo.Name));
			}
		}
	}
}
