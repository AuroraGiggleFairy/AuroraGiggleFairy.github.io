using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData;

namespace ModInfo;

[XmlParser]
public static class ModInfoLoader
{
	public static List<ModInfo> ParseXml(string _filename, XmlFile _xml)
	{
		List<ModInfo> list = new List<ModInfo>();
		Dictionary<PositionXmlElement, DataItem<ModInfo>> updateLater = new Dictionary<PositionXmlElement, DataItem<ModInfo>>();
		XElement root = _xml.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			Log.Error("No document root or no children found!");
			return list;
		}
		foreach (XElement item in root.Elements())
		{
			if (item.Name.LocalName == "ModInfo")
			{
				ModInfo modInfo = ModInfo.Parser.Parse(item, updateLater);
				if (modInfo != null)
				{
					list.Add(modInfo);
				}
			}
			else
			{
				Log.Warning($"Unknown element found: {item.Name} (file {_filename}, line {((IXmlLineInfo)item).LineNumber})");
			}
		}
		return list;
	}
}
