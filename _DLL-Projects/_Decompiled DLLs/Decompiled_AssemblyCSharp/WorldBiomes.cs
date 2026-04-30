using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

public class WorldBiomes
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, BiomeDefinition> m_Color2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] m_Id2BiomeArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BiomeDefinition> m_Name2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, PoiMapElement> m_PoiMap;

	public static WorldBiomes Instance;

	public WorldBiomes(XDocument _genxml, bool _instantiateReferences)
	{
		Instance = this;
		m_Color2BiomeMap = new Dictionary<uint, BiomeDefinition>();
		m_Id2BiomeArr = new BiomeDefinition[256];
		m_Name2BiomeMap = new CaseInsensitiveStringDictionary<BiomeDefinition>();
		m_PoiMap = new Dictionary<uint, PoiMapElement>();
		readXML(_genxml, _instantiateReferences);
	}

	public void Cleanup()
	{
	}

	public static void CleanupStatic()
	{
		if (Instance != null)
		{
			Instance.Cleanup();
		}
	}

	public int GetBiomeCount()
	{
		if (m_Color2BiomeMap == null)
		{
			return 0;
		}
		return m_Color2BiomeMap.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Dictionary<uint, BiomeDefinition> GetBiomeMap()
	{
		return m_Color2BiomeMap;
	}

	public BiomeDefinition GetBiome(Color32 _color)
	{
		if (m_Color2BiomeMap.ContainsKey((uint)((_color.r << 16) | (_color.g << 8) | _color.b)))
		{
			return m_Color2BiomeMap[(uint)((_color.r << 16) | (_color.g << 8) | _color.b)];
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(byte _id)
	{
		return m_Id2BiomeArr[_id];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetBiome(byte _id, out BiomeDefinition _bd)
	{
		_bd = m_Id2BiomeArr[_id];
		return _bd != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(string _name)
	{
		if (!m_Name2BiomeMap.ContainsKey(_name))
		{
			return null;
		}
		return m_Name2BiomeMap[_name];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readXML(XDocument _xml, bool _instantiateReferences)
	{
		Array.Clear(m_Id2BiomeArr, 0, m_Id2BiomeArr.Length);
		m_Color2BiomeMap.Clear();
		m_Name2BiomeMap.Clear();
		if (BiomeDefinition.nameToId == null)
		{
			BiomeDefinition.nameToId = new Dictionary<string, byte>();
		}
		foreach (XElement item in _xml.Descendants("biomemap"))
		{
			string attribute = item.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute))
			{
				BiomeDefinition.nameToId.Add(attribute, byte.Parse(item.GetAttribute("id")));
			}
		}
		foreach (XElement item2 in _xml.Descendants("biome"))
		{
			string attribute2 = item2.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute2))
			{
				throw new Exception("Parsing biomes. Biome with name '" + attribute2 + "' also needs an entry in the biomemap");
			}
			byte id = BiomeDefinition.nameToId[attribute2];
			BiomeDefinition biomeDefinition = parseBiome(id, 0, attribute2, item2, _instantiateReferences);
			m_Id2BiomeArr[biomeDefinition.m_Id] = biomeDefinition;
			m_Color2BiomeMap.Add(biomeDefinition.m_uiColor, biomeDefinition);
			m_Name2BiomeMap.Add(biomeDefinition.m_sBiomeName, biomeDefinition);
		}
		BiomeParticleManager.RegistrationCompleted = true;
		foreach (XElement item3 in _xml.Descendants("pois"))
		{
			foreach (XElement item4 in item3.Descendants("poi"))
			{
				uint num = Convert.ToUInt32(item4.GetAttribute("poimapcolor").Substring(1), 16);
				int iSO = 0;
				int num2 = 0;
				int iST = 0;
				if (item4.HasAttribute("surfaceoffset"))
				{
					iSO = Convert.ToInt32(item4.GetAttribute("surfaceoffset"));
				}
				if (item4.HasAttribute("smoothness"))
				{
					num2 = Convert.ToInt32(item4.GetAttribute("smoothness"));
					num2 = ((num2 >= 0) ? num2 : 0);
				}
				if (item4.HasAttribute("starttunnel"))
				{
					iST = Convert.ToInt32(item4.GetAttribute("starttunnel"));
					iST = ((iST >= 0) ? iST : 0);
				}
				BlockValue blockValue = BlockValue.Air;
				if (item4.HasAttribute("blockname"))
				{
					blockValue = (_instantiateReferences ? GetBlockValueForName(item4.GetAttribute("blockname")) : BlockValue.Air);
				}
				BlockValue blockBelow = BlockValue.Air;
				if (item4.HasAttribute("blockbelow"))
				{
					blockBelow = (_instantiateReferences ? GetBlockValueForName(item4.GetAttribute("blockbelow")) : BlockValue.Air);
				}
				int ypos = -1;
				if (item4.HasAttribute("ypos"))
				{
					ypos = int.Parse(item4.GetAttribute("ypos"));
				}
				int yposFill = -1;
				if (item4.HasAttribute("yposfill"))
				{
					yposFill = int.Parse(item4.GetAttribute("yposfill"));
				}
				PoiMapElement poiMapElement = new PoiMapElement(num, item4.GetAttribute("prefab"), blockValue, blockBelow, iSO, ypos, yposFill, iST);
				m_PoiMap.Add(num, poiMapElement);
				foreach (XElement item5 in item4.Elements())
				{
					if (item5.Name == "decal")
					{
						int texIndex = Convert.ToInt32(item5.GetAttribute("texture"));
						BlockFace face = (BlockFace)Convert.ToInt32(item5.GetAttribute("face"));
						float prob = StringParsers.ParseFloat(item5.GetAttribute("prob"));
						poiMapElement.decals.Add(new PoiMapDecal(texIndex, face, prob));
					}
					if (item5.Name == "blockontop")
					{
						blockValue = (_instantiateReferences ? GetBlockValueForName(item5.GetAttribute("blockname")) : BlockValue.Air);
						float prob2 = StringParsers.ParseFloat(item5.GetAttribute("prob"));
						int offset = (item5.HasAttribute("offset") ? int.Parse(item5.GetAttribute("offset")) : 0);
						poiMapElement.blocksOnTop.Add(new PoiMapBlock(blockValue, prob2, offset));
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition parseBiome(byte id, byte subId, string name, XElement biomeElement, bool _instantiateReferences)
	{
		uint color = 0u;
		if (biomeElement.HasAttribute("biomemapcolor"))
		{
			color = Convert.ToUInt32(biomeElement.GetAttribute("biomemapcolor").Substring(1), 16);
		}
		int radiationLevel = 0;
		if (biomeElement.HasAttribute("radiationlevel"))
		{
			radiationLevel = int.Parse(biomeElement.GetAttribute("radiationlevel"));
		}
		string buff = null;
		if (biomeElement.HasAttribute("buff"))
		{
			buff = biomeElement.GetAttribute("buff");
		}
		BiomeDefinition biomeDefinition = new BiomeDefinition(id, subId, name, color, radiationLevel, buff);
		string attribute = biomeElement.GetAttribute("noise");
		if (attribute.Length > 0)
		{
			Vector3 vector = StringParsers.ParseVector3(attribute);
			biomeDefinition.noiseFreq = vector.x;
			biomeDefinition.noiseMin = vector.y;
			biomeDefinition.noiseMax = vector.z;
		}
		attribute = biomeElement.GetAttribute("noiseoffset");
		if (attribute.Length > 0)
		{
			biomeDefinition.noiseOffset = StringParsers.ParseVector2(attribute);
		}
		if (biomeElement.HasAttribute("gamestage_modifier"))
		{
			biomeDefinition.GameStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_modifier"));
		}
		if (biomeElement.HasAttribute("gamestage_bonus"))
		{
			biomeDefinition.GameStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_bonus"));
		}
		if (biomeElement.HasAttribute("lootstage_modifier"))
		{
			biomeDefinition.LootStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_modifier"));
		}
		if (biomeElement.HasAttribute("lootstage_bonus"))
		{
			biomeDefinition.LootStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_bonus"));
		}
		if (biomeElement.HasAttribute("lootstage_min"))
		{
			biomeDefinition.LootStageMin = StringParsers.ParseSInt32(biomeElement.GetAttribute("lootstage_min"));
		}
		if (biomeElement.HasAttribute("lootstage_max"))
		{
			biomeDefinition.LootStageMax = StringParsers.ParseSInt32(biomeElement.GetAttribute("lootstage_max"));
		}
		if (biomeElement.HasAttribute("difficulty"))
		{
			biomeDefinition.Difficulty = StringParsers.ParseSInt32(biomeElement.GetAttribute("difficulty"));
		}
		foreach (XElement item in biomeElement.Elements())
		{
			if (item.Name == "subbiome")
			{
				subId++;
				BiomeDefinition biomeDefinition2 = parseBiome(id, subId, name, item, _instantiateReferences);
				biomeDefinition.addSubBiome(biomeDefinition2);
				if (biomeDefinition2.m_DecoBlocks.Count == 0 && biomeDefinition2.m_DecoPrefabs.Count == 0)
				{
					biomeDefinition2.m_DecoBlocks = biomeDefinition.m_DecoBlocks;
					biomeDefinition2.m_DistantDecoBlocks = biomeDefinition.m_DistantDecoBlocks;
					biomeDefinition2.m_DecoPrefabs = biomeDefinition.m_DecoPrefabs;
				}
			}
			else if (item.Name == "spectrum")
			{
				if (item.HasAttribute("name"))
				{
					string attribute2 = item.GetAttribute("name");
					biomeDefinition.m_SpectrumName = attribute2;
				}
			}
			else if (item.Name == "weather")
			{
				ParseWeather(biomeDefinition, item);
			}
			else if (item.Name == "layers")
			{
				foreach (XElement item2 in item.Elements("layer"))
				{
					int depth = -1;
					string attribute3 = item2.GetAttribute("depth");
					if (attribute3.Length > 0 && !attribute3.Equals("*"))
					{
						depth = int.Parse(attribute3);
					}
					string attribute4 = item2.GetAttribute("blockname");
					BiomeLayer biomeLayer = new BiomeLayer(depth, new BiomeBlockDecoration(attribute4, 1f, 1f, _instantiateReferences, 0));
					biomeDefinition.AddLayer(biomeLayer);
					foreach (XElement item3 in item2.Descendants("resource"))
					{
						BiomeBlockDecoration res = new BiomeBlockDecoration(_prob: StringParsers.ParseFloat(item3.GetAttribute("prob")), _name: Convert.ToString(item3.GetAttribute("blockname")), _clusprob: 0f, _instantiateReferences: _instantiateReferences, _randomRotateMax: 0);
						biomeLayer.AddResource(res);
					}
				}
			}
			else if (item.Name == "decorations")
			{
				foreach (XElement item4 in item.Elements("decoration"))
				{
					string attribute5 = item4.GetAttribute("type");
					if (attribute5.Equals("block"))
					{
						float prob = StringParsers.ParseFloat(item4.GetAttribute("prob"));
						float clusprob = 0f;
						string attribute6 = item4.GetAttribute("blockname");
						int randomRotateMax = (item4.HasAttribute("rotatemax") ? int.Parse(item4.GetAttribute("rotatemax")) : 0);
						int checkResource = (item4.HasAttribute("checkresource") ? int.Parse(item4.GetAttribute("checkresource")) : int.MaxValue);
						biomeDefinition.AddDecoBlock(new BiomeBlockDecoration(attribute6, prob, clusprob, _instantiateReferences, randomRotateMax, checkResource));
						continue;
					}
					if (attribute5.Equals("prefab"))
					{
						float prob2 = StringParsers.ParseFloat(item4.GetAttribute("prob"));
						string attribute7 = item4.GetAttribute("name");
						if (string.IsNullOrEmpty(attribute7))
						{
							throw new Exception("Parsing biomes. No model name specified on prefab in biome '" + name + "'");
						}
						int checkResource2 = (item4.HasAttribute("checkresource") ? int.Parse(item4.GetAttribute("checkresource")) : 10000);
						bool isDecorateOnSlopes = item4.HasAttribute("onslopes") && StringParsers.ParseBool(item4.GetAttribute("onslopes"));
						biomeDefinition.AddDecoPrefab(new BiomePrefabDecoration(attribute7, prob2, isDecorateOnSlopes, checkResource2));
						continue;
					}
					if (attribute5.Equals("terrain"))
					{
						float prob3 = StringParsers.ParseFloat(item4.GetAttribute("prob"));
						string attribute8 = item4.GetAttribute("name");
						if (string.IsNullOrEmpty(attribute8))
						{
							throw new Exception("Parsing biomes. No name specified on terrain in biome '" + name + "'");
						}
						if (_instantiateReferences && !SdFile.Exists(GameIO.GetGameDir("Data/Bluffs") + "/" + attribute8 + ".tga"))
						{
							throw new Exception("Parsing biomes. Prefab with name '" + attribute8 + ".tga' not found!");
						}
						float minScale = 1f;
						float maxScale = 1f;
						string text = (item4.HasAttribute("scale") ? item4.GetAttribute("scale") : null);
						if (text != null && text.IndexOf(',') > 0)
						{
							string[] array = text.Split(',');
							minScale = StringParsers.ParseFloat(array[0]);
							maxScale = StringParsers.ParseFloat(array[1]);
						}
						else if (text != null)
						{
							minScale = (maxScale = StringParsers.ParseFloat(text));
						}
						biomeDefinition.AddBluff(new BiomeBluffDecoration(attribute8, prob3, minScale, maxScale));
						continue;
					}
					throw new Exception("Unknown decoration type " + attribute5);
				}
			}
			else
			{
				if (!(item.Name == "replacements"))
				{
					continue;
				}
				foreach (XElement item5 in item.Elements("replace"))
				{
					string attribute9 = item5.GetAttribute("source");
					string attribute10 = item5.GetAttribute("target");
					biomeDefinition.AddReplacement(Block.GetBlockValue(attribute9).type, Block.GetBlockValue(attribute10).type);
				}
			}
		}
		biomeDefinition.SetupWeather();
		return biomeDefinition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseWeather(BiomeDefinition _bd, XElement _xe)
	{
		string _result = "?";
		_xe.ParseAttribute("name", ref _result);
		float _result2 = 1f;
		_xe.ParseAttribute("prob", ref _result2);
		float _result3 = 3f;
		_xe.ParseAttribute("duration", ref _result3);
		Vector2 _result4 = Vector2.zero;
		_xe.ParseAttribute("delay", ref _result4);
		string attribute = _xe.GetAttribute("buff");
		BiomeDefinition.WeatherGroup weatherGroup = _bd.AddWeatherGroup(_result, _result2, _result3, _result4, attribute);
		Vector2 _result5 = default(Vector2);
		foreach (XElement item in _xe.Elements())
		{
			string localName = item.Name.LocalName;
			BiomeDefinition.Probabilities.ProbType probType = BiomeDefinition.Probabilities.ProbType.Count;
			_result5.x = 0f;
			_result5.y = 100f;
			if (localName.EqualsCaseInsensitive("temperature"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Temperature;
				_result5.x = -50f;
				_result5.y = 150f;
			}
			else if (localName.EqualsCaseInsensitive("cloudthickness"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.CloudThickness;
			}
			else if (localName.EqualsCaseInsensitive("precipitation"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Precipitation;
			}
			else if (localName.EqualsCaseInsensitive("fog"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Fog;
			}
			else if (localName.EqualsCaseInsensitive("wind"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Wind;
			}
			else if (localName.EqualsCaseInsensitive("particleeffect"))
			{
				string prefabName = ((item.GetAttribute("prefab").Length > 0) ? item.GetAttribute("prefab") : "error");
				int num = (int)((item.GetAttribute("ChunkMargin").Length > 0) ? StringParsers.ParseFloat(item.GetAttribute("ChunkMargin")) : 8f);
				BiomeParticleManager.RegisterEffect(_bd.m_sBiomeName, prefabName, num);
			}
			else if (localName.EqualsCaseInsensitive("spectrum"))
			{
				string attribute2 = item.GetAttribute("name");
				weatherGroup.spectrum = EnumUtils.Parse(attribute2, SpectrumWeatherType.Biome, _ignoreCase: true);
			}
			if (probType != BiomeDefinition.Probabilities.ProbType.Count)
			{
				item.ParseAttribute("min", ref _result5.x);
				item.ParseAttribute("max", ref _result5.y);
				item.ParseAttribute("range", ref _result5);
				float _result6 = 1f;
				item.ParseAttribute("prob", ref _result6);
				weatherGroup.AddProbability(probType, _result5, _result6);
			}
		}
	}

	public static BlockValue GetBlockValueForName(string blockname)
	{
		ItemValue item = ItemClass.GetItem(blockname);
		if (item.IsEmpty())
		{
			throw new Exception("Block with name '" + blockname + "' not found!");
		}
		return item.ToBlockValue();
	}

	public PoiMapElement getPoiForColor(uint uiColor)
	{
		if (m_PoiMap.TryGetValue(uiColor, out var value))
		{
			return value;
		}
		return null;
	}

	public void AddPoiMapElement(PoiMapElement _newElement)
	{
		if (!m_PoiMap.ContainsKey(_newElement.m_uColorId))
		{
			m_PoiMap.Add(_newElement.m_uColorId, _newElement);
		}
	}

	public int GetTotalBluffsCount()
	{
		int num = 0;
		for (int i = 0; i < m_Id2BiomeArr.Length; i++)
		{
			if (m_Id2BiomeArr[i] != null)
			{
				num += m_Id2BiomeArr[i].m_DecoBluffs.Count;
			}
		}
		return num;
	}
}
