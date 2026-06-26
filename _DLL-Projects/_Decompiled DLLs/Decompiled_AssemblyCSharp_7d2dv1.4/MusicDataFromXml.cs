using System;
using System.Collections;
using System.Xml.Linq;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;

public class MusicDataFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		string text = "@:Sounds/Music/";
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "property")
			{
				text = item.GetAttribute("value");
			}
			if (item.Name == "music_groups")
			{
				foreach (XElement item2 in item.Elements("group"))
				{
					int.Parse(item2.GetAttribute("id"));
					int sampleRate = int.Parse(item2.GetAttribute("sample-rate"));
					byte hbLength = byte.Parse(item2.GetAttribute("hyperbar-length"));
					MusicGroup musicGroup = new MusicGroup(sampleRate, hbLength);
					foreach (XElement item3 in item2.Elements("configurations"))
					{
						if (item3.Name == "configurations")
						{
							foreach (XElement item4 in item3.Elements("configuration"))
							{
								musicGroup.ConfigIDs.Add(int.Parse(item4.Attribute("id").Value));
							}
						}
						if (!(item3.Name == "threat_level"))
						{
							continue;
						}
						ThreatLevelLegacyType key = EnumUtils.Parse<ThreatLevelLegacyType>(item3.GetAttribute("name"));
						double tempo = StringParsers.ParseDouble(item3.GetAttribute("tempo"));
						double sigHi = StringParsers.ParseDouble(item3.GetAttribute("sig-hi"));
						double sigLo = StringParsers.ParseDouble(item3.GetAttribute("sig-lo"));
						ThreatLevel threatLevel;
						musicGroup.Add(key, threatLevel = new ThreatLevel(tempo, sigHi, sigLo));
						foreach (XElement item5 in item3.Elements("layer"))
						{
							LayerType key2 = EnumUtils.Parse<LayerType>(item5.GetAttribute("name"));
							Layer layer;
							threatLevel.Add(key2, layer = new Layer());
							foreach (XElement item6 in item5.Elements("instrument_ID"))
							{
								int key3 = int.Parse(item6.GetAttribute("id"));
								InstrumentID instrumentID;
								layer.Add(key3, instrumentID = new InstrumentID());
								instrumentID.Name = item6.GetAttribute("name");
								instrumentID.SourceName = item6.GetAttribute("AudioSource");
								if (!StringParsers.TryParseFloat(item6.GetAttribute("Volume"), out instrumentID.Volume))
								{
									instrumentID.Volume = 1f;
								}
								foreach (XElement item7 in item6.Elements("placement"))
								{
									PlacementType key4 = EnumUtils.Parse<PlacementType>(item7.GetAttribute("value"));
									string attribute = item7.GetAttribute("location");
									instrumentID.Add(key4, text + attribute);
								}
							}
							layer.PopulateQueue();
						}
					}
					MusicGroup.AllGroups.Add(musicGroup);
				}
			}
			if (!(item.Name == "configurations"))
			{
				continue;
			}
			foreach (XElement item8 in item.Elements("configuration"))
			{
				int key5 = int.Parse(item8.GetAttribute("id"));
				ConfigSet configSet = new ConfigSet();
				foreach (XElement item9 in item8.Elements("threat_level"))
				{
					ThreatLevelLegacyType key6 = EnumUtils.Parse<ThreatLevelLegacyType>(item9.GetAttribute("name"));
					ThreatLevelConfig threatLevelConfig;
					configSet.Add(key6, threatLevelConfig = new ThreatLevelConfig());
					foreach (XElement item10 in item9.Elements("layer"))
					{
						LayerType key7 = EnumUtils.Parse<LayerType>(item10.GetAttribute("name"));
						LayerConfig layerConfig;
						threatLevelConfig.Add(key7, layerConfig = new LayerConfig());
						string[] array = item10.GetAttribute("values").Split(',');
						for (int i = 0; i < array.Length; i++)
						{
							layerConfig.Add(byte.Parse(array[i]), (i != 0 && i != array.Length - 1) ? PlacementType.Loop : ((i == 0) ? PlacementType.Begin : PlacementType.End));
						}
					}
				}
				ConfigSet.AllConfigSets.Add(key5, configSet);
			}
		}
		yield break;
	}
}
