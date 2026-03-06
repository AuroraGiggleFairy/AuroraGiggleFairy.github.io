using System;
using System.Collections;
using System.Xml.Linq;

public class BlockPlaceholdersFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		BlockPlaceholderMap.InitStatic();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <placeholder> found!");
		}
		foreach (XElement item in root.Elements())
		{
			if (!item.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing on placeholder");
			}
			string attribute = item.GetAttribute("name");
			BlockValue blockValue = ItemClass.GetItem(attribute).ToBlockValue();
			foreach (XElement item2 in item.Elements("block"))
			{
				if (!item2.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' missing on placeholder block of '" + attribute + "'");
				}
				string attribute2 = item2.GetAttribute("name");
				BlockValue targetValue = ItemClass.GetItem(attribute2).ToBlockValue();
				float _result = 1f;
				if (item2.HasAttribute("prob") && !StringParsers.TryParseFloat(item2.GetAttribute("prob"), out _result))
				{
					throw new Exception("Parsing error prob '" + item2.GetAttribute("prob") + "' in '" + attribute2 + "'");
				}
				if (_result <= 0f)
				{
					throw new Exception("Parsing error prob '" + item2.GetAttribute("prob") + "' in '" + attribute2 + "': can not be negative!");
				}
				string biome = null;
				if (item2.HasAttribute("biome"))
				{
					biome = item2.GetAttribute("biome");
				}
				FastTags<TagGroup.Global> questTags = FastTags<TagGroup.Global>.none;
				if (item2.HasAttribute("questtag"))
				{
					questTags = FastTags<TagGroup.Global>.Parse(item2.GetAttribute("questtag"));
				}
				bool randomRotation = false;
				if (item2.HasAttribute("randomrotation"))
				{
					randomRotation = StringParsers.ParseBool(item2.GetAttribute("randomrotation"));
				}
				if (!questTags.IsEmpty)
				{
					BlockPlaceholderMap.Instance.AddQuestResetPlaceholder(blockValue, targetValue, _result, biome, randomRotation, questTags);
				}
				else
				{
					BlockPlaceholderMap.Instance.AddPlaceholder(blockValue, targetValue, _result, biome, randomRotation);
				}
			}
			BlockPlaceholderMap.Instance.AdjustProbs(blockValue);
		}
		yield break;
	}
}
