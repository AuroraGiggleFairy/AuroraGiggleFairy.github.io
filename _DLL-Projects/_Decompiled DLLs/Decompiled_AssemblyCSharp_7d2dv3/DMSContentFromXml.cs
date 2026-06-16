using System;
using System.Collections;
using System.Xml.Linq;
using DynamicMusic;
using MusicUtils.Enums;

public class DMSContentFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		Content.SamplesFor.Clear();
		Content.SourcePathFor.Clear();
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "contents")
			{
				foreach (XElement item2 in item.Elements())
				{
					if (!(item2.Name == "section") && !(item2.Name == "content"))
					{
						continue;
					}
					SectionType sectionType = EnumUtils.Parse<SectionType>(item2.GetAttribute("name"));
					if (item2.HasAttribute("source"))
					{
						Content.SourcePathFor.Add(sectionType, item2.GetAttribute("source"));
					}
					if (item2.Name == "section")
					{
						if (item2.HasAttribute("samples"))
						{
							Content.SamplesFor.Add(sectionType, int.Parse(item2.GetAttribute("samples")));
						}
						foreach (XElement item3 in item2.Elements("layer"))
						{
							int num = int.Parse(item3.GetAttribute("num"));
							string attribute = item3.GetAttribute("contentType");
							string attribute2 = item3.GetAttribute("clipAdapterType");
							LayerType layer = EnumUtils.Parse<LayerType>(item3.GetAttribute("name"));
							for (int i = 0; i < num; i++)
							{
								if (Content.CreateWrapper(attribute) is LayeredContent layeredContent)
								{
									layeredContent.SetData(attribute2, i, sectionType, layer, item3.HasAttribute("loopOnly"));
								}
							}
						}
					}
					if (item2.Name == "content")
					{
						Content.CreateWrapper(item2.GetAttribute("type")).ParseFromXml(item2);
					}
				}
			}
			if (!(item.Name == "configurations"))
			{
				continue;
			}
			foreach (XElement item4 in item.Elements("configuration"))
			{
				if (item4.HasAttribute("type"))
				{
					AbstractConfiguration.CreateWrapper(item4.GetAttribute("type")).ParseFromXml(item4);
				}
			}
		}
		yield break;
	}
}
