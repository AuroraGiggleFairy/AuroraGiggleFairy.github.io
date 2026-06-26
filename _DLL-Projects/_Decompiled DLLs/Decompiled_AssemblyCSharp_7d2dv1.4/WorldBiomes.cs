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
					blockValue = (_instantiateReferences ? getBlockValueForName(item4.GetAttribute("blockname")) : BlockValue.Air);
				}
				BlockValue blockBelow = BlockValue.Air;
				if (item4.HasAttribute("blockbelow"))
				{
					blockBelow = (_instantiateReferences ? getBlockValueForName(item4.GetAttribute("blockbelow")) : BlockValue.Air);
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
						blockValue = (_instantiateReferences ? getBlockValueForName(item5.GetAttribute("blockname")) : BlockValue.Air);
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
		string topSoilBlock = null;
		if (biomeElement.HasAttribute("topsoil_block"))
		{
			topSoilBlock = biomeElement.GetAttribute("topsoil_block");
		}
		string buff = null;
		if (biomeElement.HasAttribute("buff"))
		{
			buff = biomeElement.GetAttribute("buff");
		}
		BiomeDefinition biomeDefinition = new BiomeDefinition(id, subId, name, color, radiationLevel, topSoilBlock, buff);
		if (biomeElement.HasAttribute("prob"))
		{
			biomeDefinition.prob = StringParsers.ParseFloat(biomeElement.GetAttribute("prob"));
		}
		if (biomeElement.HasAttribute("yless"))
		{
			biomeDefinition.yLT = int.Parse(biomeElement.GetAttribute("yless"));
		}
		if (biomeElement.HasAttribute("ygreater"))
		{
			biomeDefinition.yGT = int.Parse(biomeElement.GetAttribute("ygreater"));
		}
		if (biomeElement.HasAttribute("freq"))
		{
			biomeDefinition.freq = StringParsers.ParseFloat(biomeElement.GetAttribute("freq"));
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
			else if (item.Name == "terrain")
			{
				if (!item.HasAttribute("class"))
				{
					throw new Exception("Attribute class missing on terrain in biome " + name);
				}
				string attribute = item.GetAttribute("class");
				Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("TGM", attribute);
				if (!(typeWithPrefix != null))
				{
					continue;
				}
				TGMAbstract tGMAbstract = (TGMAbstract)Activator.CreateInstance(typeWithPrefix);
				if (tGMAbstract == null)
				{
					throw new Exception("Class '" + attribute + "' not found!");
				}
				foreach (XElement item2 in item.Elements("property"))
				{
					tGMAbstract.properties.Add(item2);
				}
				tGMAbstract.Init();
				biomeDefinition.m_Terrain = tGMAbstract;
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
				string name2 = "?";
				if (item.HasAttribute("name"))
				{
					name2 = item.GetAttribute("name");
				}
				float prob = 1f;
				if (item.HasAttribute("prob"))
				{
					prob = StringParsers.ParseFloat(item.GetAttribute("prob"));
				}
				string buff2 = "";
				if (item.HasAttribute("buff"))
				{
					buff2 = item.GetAttribute("buff");
				}
				BiomeDefinition.WeatherGroup weatherGroup = biomeDefinition.AddWeatherGroup(name2, prob, buff2);
				foreach (XElement item3 in item.Elements())
				{
					string localName = item3.Name.LocalName;
					if (localName.EqualsCaseInsensitive("temperature"))
					{
						float min = ((item3.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("min")) : (-50f));
						float max = ((item3.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("max")) : 150f);
						float probability = ((item3.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("prob")) : 1f);
						weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Temperature, min, max, probability);
					}
					else if (localName.EqualsCaseInsensitive("cloudthickness"))
					{
						float min2 = ((item3.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("min")) : 0f);
						float max2 = ((item3.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("max")) : 100f);
						float probability2 = ((item3.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("prob")) : 1f);
						weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.CloudThickness, min2, max2, probability2);
					}
					else if (localName.EqualsCaseInsensitive("precipitation"))
					{
						float min3 = ((item3.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("min")) : 0f);
						float max3 = ((item3.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("max")) : 100f);
						float probability3 = ((item3.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("prob")) : 1f);
						weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Precipitation, min3, max3, probability3);
					}
					else if (localName.EqualsCaseInsensitive("fog"))
					{
						float min4 = ((item3.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("min")) : 0f);
						float max4 = ((item3.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("max")) : 100f);
						float probability4 = ((item3.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("prob")) : 1f);
						weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Fog, min4, max4, probability4);
					}
					else if (localName.EqualsCaseInsensitive("wind"))
					{
						float min5 = ((item3.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("min")) : 0f);
						float max5 = ((item3.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("max")) : 100f);
						float probability5 = ((item3.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("prob")) : 1f);
						weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Wind, min5, max5, probability5);
					}
					else if (localName.EqualsCaseInsensitive("particleeffect"))
					{
						string prefabName = ((item3.GetAttribute("prefab").Length > 0) ? item3.GetAttribute("prefab") : "error");
						int num = (int)((item3.GetAttribute("ChunkMargin").Length > 0) ? StringParsers.ParseFloat(item3.GetAttribute("ChunkMargin")) : 8f);
						BiomeParticleManager.RegisterEffect(name, prefabName, num);
					}
					else if (localName.EqualsCaseInsensitive("spectrum"))
					{
						string attribute3 = item3.GetAttribute("name");
						weatherGroup.spectrum = EnumUtils.Parse(attribute3, SpectrumWeatherType.Biome, _ignoreCase: true);
					}
				}
			}
			else if (item.Name == "layers")
			{
				foreach (XElement item4 in item.Elements("layer"))
				{
					int depth = -1;
					if (item4.HasAttribute("depth") && !item4.GetAttribute("depth").Equals("*"))
					{
						depth = int.Parse(item4.GetAttribute("depth"));
					}
					int fillupto = 0;
					if (item4.HasAttribute("fillupto"))
					{
						fillupto = int.Parse(item4.GetAttribute("fillupto"));
						depth = 0;
					}
					int filluptorg = 0;
					if (item4.HasAttribute("filluptorg"))
					{
						filluptorg = int.Parse(item4.GetAttribute("filluptorg"));
						depth = 0;
					}
					string attribute4 = item4.GetAttribute("blockname");
					BiomeLayer biomeLayer = new BiomeLayer(depth, fillupto, filluptorg, new BiomeBlockDecoration(attribute4, 1f, 1f, _instantiateReferences ? getBlockValueForName(attribute4) : BlockValue.Air, 0));
					biomeDefinition.AddLayer(biomeLayer);
					foreach (XElement item5 in item4.Descendants("resource"))
					{
						float prob2 = StringParsers.ParseFloat(item5.GetAttribute("prob"));
						string text = Convert.ToString(item5.GetAttribute("blockname"));
						BiomeBlockDecoration res = new BiomeBlockDecoration(text, prob2, 0f, _instantiateReferences ? getBlockValueForName(text) : BlockValue.Air, 0);
						biomeLayer.AddResource(res);
					}
				}
			}
			else if (item.Name == "decorations")
			{
				foreach (XElement item6 in item.Elements("decoration"))
				{
					string attribute5 = item6.GetAttribute("type");
					if (attribute5.Equals("block"))
					{
						float prob3 = StringParsers.ParseFloat(item6.GetAttribute("prob"));
						float clusprob = 0f;
						string attribute6 = item6.GetAttribute("blockname");
						int num2 = (item6.HasAttribute("rotatemax") ? int.Parse(item6.GetAttribute("rotatemax")) : 0);
						int checkResource = (item6.HasAttribute("checkresource") ? int.Parse(item6.GetAttribute("checkresource")) : int.MaxValue);
						BlockValue blockValue = (_instantiateReferences ? getBlockValueForName(attribute6) : BlockValue.Air);
						if (!blockValue.isair && blockValue.Block.isMultiBlock && (blockValue.Block.multiBlockPos.dim.x > 1 || blockValue.Block.multiBlockPos.dim.z > 1) && num2 > 3)
						{
							Log.Error("Parsing biomes. Block with name '" + attribute6 + "' supports only rotations 0-3, setting it to 3");
							num2 = 3;
						}
						biomeDefinition.AddDecoBlock(new BiomeBlockDecoration(attribute6, prob3, clusprob, blockValue, num2, checkResource));
						continue;
					}
					if (attribute5.Equals("prefab"))
					{
						float prob4 = StringParsers.ParseFloat(item6.GetAttribute("prob"));
						string attribute7 = item6.GetAttribute("name");
						if (string.IsNullOrEmpty(attribute7))
						{
							throw new Exception("Parsing biomes. No model name specified on prefab in biome '" + name + "'");
						}
						int checkResource2 = (item6.HasAttribute("checkresource") ? int.Parse(item6.GetAttribute("checkresource")) : 10000);
						bool isDecorateOnSlopes = item6.HasAttribute("onslopes") && StringParsers.ParseBool(item6.GetAttribute("onslopes"));
						biomeDefinition.AddDecoPrefab(new BiomePrefabDecoration(attribute7, prob4, isDecorateOnSlopes, checkResource2));
						continue;
					}
					if (attribute5.Equals("terrain"))
					{
						float prob5 = StringParsers.ParseFloat(item6.GetAttribute("prob"));
						string attribute8 = item6.GetAttribute("name");
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
						string text2 = (item6.HasAttribute("scale") ? item6.GetAttribute("scale") : null);
						if (text2 != null && text2.IndexOf(',') > 0)
						{
							string[] array = text2.Split(',');
							minScale = StringParsers.ParseFloat(array[0]);
							maxScale = StringParsers.ParseFloat(array[1]);
						}
						else if (text2 != null)
						{
							minScale = (maxScale = StringParsers.ParseFloat(text2));
						}
						biomeDefinition.AddBluff(new BiomeBluffDecoration(attribute8, prob5, minScale, maxScale));
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
				foreach (XElement item7 in item.Elements("replace"))
				{
					string attribute9 = item7.GetAttribute("source");
					string attribute10 = item7.GetAttribute("target");
					biomeDefinition.AddReplacement(Block.GetBlockValue(attribute9).type, Block.GetBlockValue(attribute10).type);
				}
			}
		}
		biomeDefinition.SetupWeather();
		return biomeDefinition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValueForName(string blockname)
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
