using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class EntityClassesFromXml
{
	public const char cReplaceChar = '^';

	public static Dictionary<string, string> sReplaceProperties = new Dictionary<string, string>();

	public static Dictionary<string, string> sReplacePassiveEffects = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, XElement> sEntityClassElements;

	public static IEnumerator LoadEntityClasses(XmlFile _xmlFile)
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		EntityClass.list.Clear();
		sEntityClassElements = new Dictionary<int, XElement>();
		EntityClass.sColors.Clear();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entity_classes> found!");
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "color")
			{
				string attribute = item.GetAttribute("name");
				string attribute2 = item.GetAttribute("value");
				EntityClass.sColors.Add(attribute, StringParsers.ParseColor(attribute2));
			}
			else if (item.Name == "replace_properties")
			{
				sReplaceProperties.Clear();
				foreach (XElement item2 in item.Elements())
				{
					if (item2.Name == "property")
					{
						sReplaceProperties.Add("^" + item2.GetAttribute("name"), item2.GetAttribute("value"));
					}
				}
			}
			else if (item.Name == "replace_passive_effect")
			{
				sReplacePassiveEffects.Clear();
				foreach (XElement item3 in item.Elements())
				{
					if (item3.Name == "property")
					{
						sReplacePassiveEffects.Add("^" + item3.GetAttribute("name"), item3.GetAttribute("value"));
					}
				}
			}
			if (item.Name == "entity_class")
			{
				XElement xElement = item;
				EntityClass entityClass = new EntityClass();
				string attribute3 = xElement.GetAttribute("name");
				if (attribute3.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on property in entity_class");
				}
				entityClass.entityClassName = attribute3;
				int num = EntityClass.FromString(entityClass.entityClassName);
				if (!sEntityClassElements.TryAdd(num, xElement))
				{
					string attribute4 = sEntityClassElements[num].GetAttribute("name");
					throw new ArgumentException("Can not add entity '" + attribute3 + "' with conflicting hash to existing entity '" + attribute4 + "'");
				}
				string attribute5 = xElement.GetAttribute("extends");
				if (attribute5.Length > 0)
				{
					int num2 = EntityClass.FromString(attribute5);
					if (!EntityClass.list.ContainsKey(num2))
					{
						throw new Exception("Did not find 'extends' entity '" + attribute5 + "'");
					}
					HashSet<string> hashSet = new HashSet<string>();
					if (xElement.HasAttribute("ignore"))
					{
						string[] array = xElement.GetAttribute("ignore").Split(',');
						foreach (string text in array)
						{
							hashSet.Add(text.Trim());
						}
					}
					hashSet.Add("HideInSpawnMenu");
					entityClass.CopyFrom(EntityClass.list[num2], hashSet);
					entityClass.Effects = MinEffectController.ParseXml(xElement, sEntityClassElements[num2], MinEffectController.SourceParentType.EntityClass, num);
				}
				else
				{
					entityClass.Effects = MinEffectController.ParseXml(xElement, null, MinEffectController.SourceParentType.EntityClass, num);
				}
				foreach (XElement item4 in xElement.Elements())
				{
					if (item4.Name == "property")
					{
						entityClass.Properties.Add(item4, _addClassesToMain: true, _doValueReplace: true);
					}
					if (item4.Name == "drop")
					{
						XElement element = item4;
						int _minCount = 1;
						int _maxCount = 1;
						if (element.HasAttribute("count"))
						{
							StringParsers.ParseMinMaxCount(element.GetAttribute("count"), out _minCount, out _maxCount);
						}
						float prob = 1f;
						if (element.HasAttribute("prob"))
						{
							prob = StringParsers.ParseFloat(element.GetAttribute("prob"));
						}
						string attribute6 = element.GetAttribute("name");
						EnumDropEvent eEvent = EnumDropEvent.Destroy;
						if (element.HasAttribute("event"))
						{
							eEvent = EnumUtils.Parse<EnumDropEvent>(element.GetAttribute("event"));
						}
						float stickChance = 0f;
						if (element.HasAttribute("stick_chance"))
						{
							stickChance = StringParsers.ParseFloat(element.GetAttribute("stick_chance"));
						}
						string toolCategory = null;
						if (element.HasAttribute("tool_category"))
						{
							toolCategory = element.GetAttribute("tool_category");
						}
						string tag = "";
						if (element.HasAttribute("tag"))
						{
							tag = element.GetAttribute("tag");
						}
						entityClass.AddDroppedId(eEvent, attribute6, _minCount, _maxCount, prob, stickChance, toolCategory, tag);
					}
				}
				EntityClass.list[num] = entityClass;
				entityClass.Init();
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		sEntityClassElements.Clear();
		sEntityClassElements = null;
	}

	public static string ReplaceProperty(string _value)
	{
		if (_value.Length > 0 && _value[0] == '^')
		{
			_value = sReplaceProperties[_value];
		}
		return _value;
	}
}
