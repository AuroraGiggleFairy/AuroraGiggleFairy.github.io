using System;
using System.Xml.Linq;

public class EntitySpawnerClassesFromXml
{
	public static bool LoadEntitySpawnerClasses(XDocument _spawnXml)
	{
		XElement root = _spawnXml.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <spawning> found!");
		}
		foreach (XElement item in root.Elements("entityspawner"))
		{
			if (!item.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing on property in entityspawner");
			}
			string attribute = item.GetAttribute("name");
			EntitySpawnerClassForDay entitySpawnerClassForDay = new EntitySpawnerClassForDay();
			entitySpawnerClassForDay.bDynamicSpawner = item.HasAttribute("dynamic") && StringParsers.ParseBool(item.GetAttribute("dynamic"));
			entitySpawnerClassForDay.bWrapDays = item.HasAttribute("wrapMode") && item.GetAttribute("wrapMode") == "wrap";
			entitySpawnerClassForDay.bClampDays = item.HasAttribute("wrapMode") && item.GetAttribute("wrapMode") == "clamp";
			foreach (XElement item2 in item.Elements("day"))
			{
				Vector2i zero = Vector2i.zero;
				if (!item2.GetAttribute(XNames.value).Equals("*"))
				{
					if (item2.GetAttribute(XNames.value).Contains(","))
					{
						StringParsers.ParseMinMaxCount(item2.GetAttribute(XNames.value), out int _minCount, out int _maxCount);
						zero.x = _minCount;
						zero.y = _maxCount;
					}
					else
					{
						zero.x = int.Parse(item2.GetAttribute(XNames.value));
						zero.y = zero.x;
					}
				}
				for (int i = zero.x; i <= zero.y; i++)
				{
					EntitySpawnerClass entitySpawnerClass = new EntitySpawnerClass();
					entitySpawnerClass.name = attribute;
					foreach (XElement item3 in item2.Elements(XNames.property))
					{
						entitySpawnerClass.Properties.Add(item3);
					}
					entitySpawnerClass.Init();
					entitySpawnerClassForDay.AddForDay(i, entitySpawnerClass);
				}
			}
			if (entitySpawnerClassForDay.Count() == 0)
			{
				throw new Exception("Empty entityspawner not allowed: " + attribute);
			}
			EntitySpawnerClass.list[attribute] = entitySpawnerClassForDay;
		}
		return true;
	}
}
