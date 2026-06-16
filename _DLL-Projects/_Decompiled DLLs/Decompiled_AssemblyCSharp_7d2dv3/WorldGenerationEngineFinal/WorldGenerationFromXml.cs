using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public static class WorldGenerationFromXml
{
	public static void Cleanup()
	{
		WorldBuilderStatic.Properties.Clear();
		WorldBuilderStatic.WorldSizeMapper.Clear();
		WorldBuilderStatic.idToTownshipData.Clear();
		PrefabManagerStatic.prefabWeightData.Clear();
		PrefabManagerStatic.TileMinMaxCounts.Clear();
		PrefabManagerStatic.TileMaxDensityScore.Clear();
		DistrictPlannerStatic.Districts.Clear();
	}

	public static void Reload(XmlFile _xmlFile)
	{
		Debug.LogError("Reloading world generation data!");
		Cleanup();
		ThreadManager.RunCoroutineSync(Load(_xmlFile));
	}

	public static IEnumerator Load(XmlFile file)
	{
		Cleanup();
		int num = 0;
		XElement root = file.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <rwgmixer> found!");
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "world")
			{
				XElement xElement = item;
				if (!xElement.HasAttribute("name"))
				{
					continue;
				}
				string attribute = xElement.GetAttribute("name");
				DynamicProperties dynamicProperties = new DynamicProperties();
				_ = Vector2i.one;
				foreach (XElement item2 in xElement.Elements("property"))
				{
					dynamicProperties.Add(item2);
					if (item2.HasAttribute("name") && item2.GetAttribute("name") == "world_size" && item2.HasAttribute("value"))
					{
						WorldBuilderStatic.WorldSizeMapper[attribute] = StringParsers.ParseVector2i(item2.GetAttribute("value"));
					}
				}
				WorldBuilderStatic.Properties[attribute] = dynamicProperties;
			}
			else if (item.Name == "streettile")
			{
				XElement xElement2 = item;
				string attribute2 = xElement2.GetAttribute("name");
				if (attribute2.Length == 0)
				{
					continue;
				}
				int num2 = 0;
				int num3 = int.MaxValue;
				int num4 = -1;
				foreach (XElement item3 in xElement2.Elements("property"))
				{
					string attribute3 = item3.GetAttribute("name");
					string attribute4 = item3.GetAttribute("value");
					if (attribute3.EqualsCaseInsensitive("maxtiles"))
					{
						num3 = StringParsers.ParseSInt32(attribute4);
					}
					else if (attribute3.EqualsCaseInsensitive("mintiles"))
					{
						num2 = StringParsers.ParseSInt32(attribute4);
					}
					else if (attribute3.EqualsCaseInsensitive("maxdensity"))
					{
						num4 = StringParsers.ParseSInt32(attribute4);
					}
				}
				if (num2 > 0 || num3 != int.MaxValue)
				{
					PrefabManagerStatic.TileMinMaxCounts[attribute2] = new Vector2i(num2, num3);
				}
				if (num4 >= 0)
				{
					PrefabManagerStatic.TileMaxDensityScore[attribute2] = num4;
				}
			}
			else if (item.Name == "district")
			{
				XElement xElement3 = item;
				District district = new District();
				district.name = xElement3.GetAttribute("name").ToLower();
				district.prefabName = district.name;
				district.tag = FastTags<TagGroup.Poi>.Parse(district.name);
				foreach (XElement item4 in xElement3.Elements("property"))
				{
					string attribute5 = item4.GetAttribute("name");
					string text = item4.GetAttribute("value").ToLower();
					if (attribute5.Length > 0 && text.Length > 0)
					{
						if (attribute5.EqualsCaseInsensitive("prefab_name"))
						{
							district.prefabName = text;
						}
						else if (attribute5.EqualsCaseInsensitive("tag"))
						{
							district.tag = FastTags<TagGroup.Poi>.Parse(text);
						}
						else if (attribute5.EqualsCaseInsensitive("spawn_weight"))
						{
							district.weight = StringParsers.ParseFloat(text);
						}
						else if (attribute5.EqualsCaseInsensitive("required_township"))
						{
							district.townships = FastTags<TagGroup.Poi>.Parse(text);
						}
						else if (attribute5.EqualsCaseInsensitive("preview_color"))
						{
							district.preview_color = StringParsers.ParseColor(text);
						}
						else if (attribute5.EqualsCaseInsensitive("spawn_custom_size_prefabs"))
						{
							district.spawnCustomSizePrefabs = StringParsers.ParseBool(text);
						}
						else if (attribute5.EqualsCaseInsensitive("avoided_neighbor_districts"))
						{
							district.avoidedNeighborDistricts = new List<string>(text.Split(','));
						}
					}
				}
				district.prefabName += "_";
				district.Init();
				DistrictPlannerStatic.Districts[district.name] = district;
			}
			else if (item.Name == "township")
			{
				TownshipData townshipData = new TownshipData(item.GetAttribute("name"), num);
				num++;
				foreach (XElement item5 in item.Elements("property"))
				{
					string attribute6 = item5.GetAttribute("name");
					string attribute7 = item5.GetAttribute("value");
					if (attribute6.Length > 0 && attribute7.Length > 0)
					{
						if (attribute6.EqualsCaseInsensitive("spawnable_terrain"))
						{
							townshipData.SpawnableTerrain.AddRange(attribute7.Replace(" ", "").Split(','));
						}
						else if (attribute6.EqualsCaseInsensitive("outskirt_district"))
						{
							string[] array = attribute7.Split(",");
							townshipData.OutskirtDistrict = array[0];
							townshipData.OutskirtDistrictPercent = ((array.Length >= 2) ? float.Parse(array[1]) : 1f);
						}
						else if (attribute6.EqualsCaseInsensitive("spawn_custom_size_prefabs"))
						{
							townshipData.SpawnCustomSizes = StringParsers.ParseBool(attribute7);
						}
						else if (attribute6.EqualsCaseInsensitive("spawn_trader"))
						{
							townshipData.SpawnTrader = StringParsers.ParseBool(attribute7);
						}
						else if (attribute6.EqualsCaseInsensitive("spawn_gateway"))
						{
							townshipData.SpawnGateway = StringParsers.ParseBool(attribute7);
						}
						else if (attribute6.EqualsCaseInsensitive("biomes"))
						{
							townshipData.Biomes = FastTags<TagGroup.Poi>.Parse(attribute7.ToLower());
						}
					}
				}
			}
			else if (item.Name == "prefab_spawn_adjust")
			{
				FastTags<TagGroup.Poi> tags = FastTags<TagGroup.Poi>.none;
				FastTags<TagGroup.Poi> biomeTags = FastTags<TagGroup.Poi>.none;
				float weight = 1f;
				float bias = 0f;
				int maxCount = int.MaxValue;
				int minCount = 1;
				string attribute8 = item.GetAttribute("partial_name");
				if (item.HasAttribute("tags"))
				{
					tags = FastTags<TagGroup.Poi>.Parse(item.GetAttribute("tags"));
				}
				if (item.TryGetAttribute("biomeTags", out var _result))
				{
					biomeTags = FastTags<TagGroup.Poi>.Parse(_result);
				}
				if (item.HasAttribute("weight"))
				{
					weight = StringParsers.ParseFloat(item.GetAttribute("weight"));
				}
				if (item.HasAttribute("bias"))
				{
					bias = StringParsers.ParseFloat(item.GetAttribute("bias"));
				}
				if (item.HasAttribute("min_count"))
				{
					minCount = StringParsers.ParseSInt32(item.GetAttribute("min_count"));
				}
				if (item.HasAttribute("max_count"))
				{
					maxCount = StringParsers.ParseSInt32(item.GetAttribute("max_count"));
				}
				if (!string.IsNullOrEmpty(attribute8) || !tags.IsEmpty)
				{
					PrefabManagerStatic.prefabWeightData.Add(new PrefabManager.POIWeightData(attribute8, tags, biomeTags, weight, bias, minCount, maxCount));
				}
			}
		}
		yield break;
	}
}
