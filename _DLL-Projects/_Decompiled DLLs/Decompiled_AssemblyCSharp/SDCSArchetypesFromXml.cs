using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

public class SDCSArchetypesFromXml
{
	public static void Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			return;
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "archetype")
			{
				parseArchetype(item);
			}
		}
	}

	public static void Save(string _filename, List<Archetype> _archetypes)
	{
		StreamWriter streamWriter = new StreamWriter(GameIO.GetGameDir("Data/Config") + "/" + _filename + ".xml");
		string text = "\t";
		streamWriter.WriteLine("<archetypes>");
		for (int i = 0; i < _archetypes.Count; i++)
		{
			Archetype archetype = _archetypes[i];
			streamWriter.WriteLine($"{text}<archetype name=\"{archetype.Name}\" male=\"{archetype.IsMale.ToString().ToLower()}\" race=\"{archetype.Race}\" variant=\"{archetype.Variant}\" />");
		}
		streamWriter.WriteLine("</archetypes>");
		streamWriter.Flush();
		streamWriter.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseArchetype(XElement element)
	{
		bool canCustomize = false;
		string text = "";
		bool isMale = false;
		string race = "White";
		int variant = 1;
		string hair = "";
		string hairColor = "";
		string mustacheName = "";
		string chopsName = "";
		string beardName = "";
		string eyeColorName = "blue01";
		if (element.Name == "archetype")
		{
			if (element.HasAttribute("name"))
			{
				text = element.GetAttribute("name");
				if (text == "BaseMale" || text == "BaseFemale")
				{
					canCustomize = true;
					isMale = text == "BaseMale";
					race = "White";
					variant = 1;
				}
				else if (element.HasAttribute("male"))
				{
					isMale = StringParsers.ParseBool(element.GetAttribute("male"));
				}
			}
			if (element.HasAttribute("race"))
			{
				race = element.GetAttribute("race");
			}
			if (element.HasAttribute("variant"))
			{
				variant = StringParsers.ParseSInt32(element.GetAttribute("variant"));
			}
			if (element.HasAttribute("hair"))
			{
				hair = element.GetAttribute("hair");
			}
			if (element.HasAttribute("hair_color"))
			{
				hairColor = element.GetAttribute("hair_color");
			}
			if (element.HasAttribute("mustache"))
			{
				mustacheName = element.GetAttribute("mustache");
			}
			if (element.HasAttribute("chops"))
			{
				chopsName = element.GetAttribute("chops");
			}
			if (element.HasAttribute("beard"))
			{
				beardName = element.GetAttribute("beard");
			}
			eyeColorName = ((!element.HasAttribute("eye_color")) ? "Blue01" : element.GetAttribute("eye_color"));
		}
		Archetype archetype = new Archetype(text, isMale, canCustomize);
		archetype.Race = race;
		archetype.Variant = variant;
		archetype.Hair = hair;
		archetype.HairColor = hairColor;
		archetype.MustacheName = mustacheName;
		archetype.ChopsName = chopsName;
		archetype.BeardName = beardName;
		archetype.EyeColorName = eyeColorName;
		foreach (XElement item in element.Elements())
		{
			if (item.Name == "equipment")
			{
				SDCSUtils.SlotData slotData = parseEquipment(item);
				if (slotData != null)
				{
					archetype.AddEquipmentSlot(slotData);
				}
			}
		}
		Archetype.SetArchetype(archetype);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SDCSUtils.SlotData parseEquipment(XElement element)
	{
		string text = "";
		string text2 = "";
		string baseToTurnOff = "";
		if (element.HasAttribute("transform_name"))
		{
			text = element.GetAttribute("transform_name");
		}
		if (element.HasAttribute("prefab"))
		{
			text2 = element.GetAttribute("prefab");
		}
		if (element.HasAttribute("excludes"))
		{
			baseToTurnOff = element.GetAttribute("excludes");
		}
		if (text != "" && text2 != "")
		{
			return new SDCSUtils.SlotData
			{
				PartName = text,
				PrefabName = text2,
				BaseToTurnOff = baseToTurnOff
			};
		}
		return null;
	}
}
