using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class EntityClassesFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, XElement> sEntityClassElements;

	public static IEnumerator LoadEntityClasses(XmlFile _xmlFile)
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		EntityClass.list.Clear();
		sEntityClassElements = new Dictionary<int, XElement>();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entity_classes> found!");
		}
		foreach (XElement item in root.Elements())
		{
			_ = item.Name == "LevelingTable";
			if (item.Name == "entity_class")
			{
				XElement xElement = item;
				EntityClass entityClass = new EntityClass();
				string attribute = xElement.GetAttribute("name");
				if (attribute.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on property in entity_class");
				}
				entityClass.entityClassName = attribute;
				int num = EntityClass.FromString(entityClass.entityClassName);
				if (!sEntityClassElements.TryAdd(num, xElement))
				{
					string attribute2 = sEntityClassElements[num].GetAttribute("name");
					throw new ArgumentException("Can not add entity '" + attribute + "' with conflicting hash to existing entity '" + attribute2 + "'");
				}
				string attribute3 = xElement.GetAttribute("extends");
				if (attribute3.Length > 0)
				{
					int num2 = EntityClass.FromString(attribute3);
					if (!EntityClass.list.ContainsKey(num2))
					{
						throw new Exception("Did not find 'extends' entity '" + attribute3 + "'");
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
				foreach (XElement item2 in xElement.Elements())
				{
					if (item2.Name == "property")
					{
						entityClass.Properties.Add(item2);
					}
					if (item2.Name == "drop")
					{
						XElement element = item2;
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
						string attribute4 = element.GetAttribute("name");
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
						entityClass.AddDroppedId(eEvent, attribute4, _minCount, _maxCount, prob, stickChance, toolCategory, tag);
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
}
