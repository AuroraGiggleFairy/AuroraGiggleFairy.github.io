using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Audio;
using UniLinq;

public class SoundsFromXml
{
	public static IEnumerator CreateSounds(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		Manager.Reset();
		ParseNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(string master, XElement root)
	{
		foreach (XElement item in root.Elements("SoundDataNode"))
		{
			Parse(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(XElement node)
	{
		string value = node.Attributes().First().Value;
		XmlData xmlData = new XmlData();
		xmlData.soundGroupName = value;
		xmlData.playImmediate = false;
		string text = null;
		foreach (XElement item in node.Elements())
		{
			string localName = item.Name.LocalName;
			if (localName.EqualsCaseInsensitive("audiosource"))
			{
				text = item.Attributes().First().Value;
			}
			else if (localName.EqualsCaseInsensitive("noise"))
			{
				string attribute = item.GetAttribute("noise");
				xmlData.noiseData.volume = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute));
				attribute = item.GetAttribute("time");
				xmlData.noiseData.time = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute));
				attribute = item.GetAttribute("heat_map_strength");
				xmlData.noiseData.heatMapStrength = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute));
				attribute = item.GetAttribute("heat_map_time");
				xmlData.noiseData.heatMapTime = (string.IsNullOrEmpty(attribute) ? 100 : ulong.Parse(attribute));
				attribute = item.GetAttribute("muffled_when_crouched");
				xmlData.noiseData.crouchMuffle = (string.IsNullOrEmpty(attribute) ? 1f : StringParsers.ParseFloat(attribute));
			}
			else if (localName.EqualsCaseInsensitive("audioclip"))
			{
				ClipSourceMap clipSourceMap = new ClipSourceMap();
				XElement element = item;
				clipSourceMap.clipName = element.GetAttribute("ClipName");
				clipSourceMap.audioSourceName = element.GetAttribute("AudioSourceName");
				if (element.HasAttribute("Loop"))
				{
					clipSourceMap.forceLoop = StringParsers.ParseBool(element.GetAttribute("Loop"));
				}
				clipSourceMap.clipName_distant = element.GetAttribute("DistantClip");
				clipSourceMap.audioSourceName_distant = element.GetAttribute("DistantSource");
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					clipSourceMap.audioSourceName = text;
				}
				StringParsers.TryParseBool(element.GetAttribute("AltSound"), out var _result);
				if (_result)
				{
					xmlData.AddAltClipSourceMap(clipSourceMap);
				}
				else
				{
					xmlData.audioClipMap.Add(clipSourceMap);
				}
				if (text == null)
				{
					text = clipSourceMap.audioSourceName;
				}
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					Log.Error("ParseSoundDataNode() - missing audio source for " + clipSourceMap.clipName + ".");
				}
				clipSourceMap.subtitleID = element.GetAttribute("Subtitle");
				clipSourceMap.hasSubtitle = !string.IsNullOrEmpty(clipSourceMap.subtitleID);
				if (element.HasAttribute("profanity"))
				{
					clipSourceMap.profanity = StringParsers.ParseBool(element.GetAttribute("profanity"));
				}
				if (clipSourceMap.profanity)
				{
					xmlData.hasProfanity = true;
				}
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName);
				DataLoader.PreloadBundle(clipSourceMap.clipName);
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName_distant);
				DataLoader.PreloadBundle(clipSourceMap.clipName_distant);
			}
			else if (localName.EqualsCaseInsensitive("localcrouchvolumescale"))
			{
				xmlData.localCrouchVolumeScale = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("crouchnoisescale"))
			{
				xmlData.crouchNoiseScale = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("noisescale"))
			{
				xmlData.noiseScale = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxvoices"))
			{
				xmlData.maxVoices = int.Parse(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxVoicesPerEntity"))
			{
				xmlData.maxVoicesPerEntity = int.Parse(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxrepeatrate"))
			{
				xmlData.maxRepeatRate = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("ignoredistancecheck"))
			{
				Manager.AddSoundToIgnoreDistanceCheckList(value);
			}
			else if (localName.EqualsCaseInsensitive("immediate"))
			{
				xmlData.playImmediate = true;
			}
			else if (localName.EqualsCaseInsensitive("sequence"))
			{
				xmlData.sequence = true;
			}
			else if (localName.EqualsCaseInsensitive("runningvolumescale"))
			{
				xmlData.runningVolumeScale = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("lowestpitch"))
			{
				xmlData.lowestPitch = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("highestpitch"))
			{
				xmlData.highestPitch = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("distantfadestart"))
			{
				xmlData.distantFadeStart = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("distantfadeend"))
			{
				xmlData.distantFadeEnd = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("channel"))
			{
				if (item.Attributes().First().Value.EqualsCaseInsensitive("mouth"))
				{
					xmlData.channel = XmlData.Channel.Mouth;
				}
				else
				{
					xmlData.channel = XmlData.Channel.Environment;
				}
			}
			else if (localName.EqualsCaseInsensitive("priority"))
			{
				xmlData.priority = int.Parse(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("vibratecontroller"))
			{
				xmlData.vibratesController = StringParsers.ParseBool(item.Attributes().First().Value);
			}
			else if (localName.EqualsCaseInsensitive("vibrationstrengthmultiply"))
			{
				xmlData.vibrationStrengthMultiplier = StringParsers.ParseFloat(item.Attributes().First().Value);
			}
		}
		Manager.AddAudioData(xmlData);
	}

	public static IEnumerator LoadSubtitleXML(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		ParseSubtitleNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSubtitleNode(string master, XElement root)
	{
		List<SubtitleData> list = new List<SubtitleData>();
		List<SubtitleSpeakerColor> list2 = new List<SubtitleSpeakerColor>();
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "Subtitle")
			{
				SubtitleData subtitleData = new SubtitleData();
				item.Elements();
				subtitleData.name = item.Attributes().First().Value;
				subtitleData.contentLocId = item.GetAttribute("contentLocId");
				subtitleData.speakerColorId = item.GetAttribute("speakerColor");
				subtitleData.speakerLocId = item.GetAttribute("speakerLocId");
				list.Add(subtitleData);
			}
			else
			{
				if (!(item.Name == "SpeakerColors"))
				{
					continue;
				}
				foreach (XElement item2 in item.Elements())
				{
					SubtitleSpeakerColor subtitleSpeakerColor = new SubtitleSpeakerColor();
					subtitleSpeakerColor.name = item2.Attributes().First().Value;
					subtitleSpeakerColor.color = item2.Attributes().Last().Value;
					list2.Add(subtitleSpeakerColor);
				}
			}
		}
		Manager.AddSubtitleData(list, list2);
	}
}
