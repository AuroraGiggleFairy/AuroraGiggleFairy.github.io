using System;
using System.Collections;
using System.Xml.Linq;

public class BiomeSpawningFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <spawning> found!");
		}
		foreach (XElement item in root.Elements("biome"))
		{
			string attribute = item.GetAttribute("name");
			if (attribute.Length == 0)
			{
				throw new Exception("Attribute 'name' missing on biome tag");
			}
			BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = new BiomeSpawnEntityGroupList();
			BiomeSpawningClass.list[attribute] = biomeSpawnEntityGroupList;
			foreach (XElement item2 in item.Elements("spawn"))
			{
				string attribute2 = item2.GetAttribute("id");
				int hashCode = attribute2.GetHashCode();
				if (biomeSpawnEntityGroupList.Find(hashCode) != null)
				{
					throw new Exception("Duplicate id hash '" + attribute2 + "' in biome '" + attribute + "'");
				}
				int maxCount = 1;
				if (item2.HasAttribute("maxcount"))
				{
					maxCount = int.Parse(item2.GetAttribute("maxcount"));
				}
				int respawndelay = 0;
				if (item2.HasAttribute("respawndelay"))
				{
					respawndelay = (int)(StringParsers.ParseFloat(item2.GetAttribute("respawndelay")) * 24000f);
				}
				EDaytime daytime = EDaytime.Any;
				if (item2.HasAttribute("time"))
				{
					daytime = EnumUtils.Parse<EDaytime>(item2.GetAttribute("time"));
				}
				BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = new BiomeSpawnEntityGroupData(hashCode, maxCount, respawndelay, daytime);
				string attribute3 = item2.GetAttribute("tags");
				if (attribute3.Length > 0)
				{
					biomeSpawnEntityGroupData.POITags = FastTags<TagGroup.Poi>.Parse(attribute3);
				}
				attribute3 = item2.GetAttribute("notags");
				if (attribute3.Length > 0)
				{
					biomeSpawnEntityGroupData.noPOITags = FastTags<TagGroup.Poi>.Parse(attribute3);
				}
				string attribute4 = item2.GetAttribute("entitygroup");
				if (attribute4.Length == 0)
				{
					throw new Exception("Missing attribute 'entitygroup' in entitygroup of biome '" + attribute + "'");
				}
				if (!EntityGroups.list.ContainsKey(attribute4))
				{
					throw new Exception("Entity group '" + attribute4 + "' not existing!");
				}
				biomeSpawnEntityGroupData.entityGroupName = attribute4;
				biomeSpawnEntityGroupList.list.Add(biomeSpawnEntityGroupData);
			}
		}
		yield break;
	}
}
