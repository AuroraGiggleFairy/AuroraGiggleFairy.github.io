using System;
using System.Collections;
using System.Xml.Linq;

public class BlockTexturesFromXML
{
	public static IEnumerator CreateBlockTextures(XmlFile _xmlFile)
	{
		BlockTextureData.InitStatic();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <block_textures> found!");
		}
		foreach (XElement item in root.Elements("paint"))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			foreach (XElement item2 in item.Elements("property"))
			{
				dynamicProperties.Add(item2);
			}
			BlockTextureData blockTextureData = new BlockTextureData();
			blockTextureData.Name = item.GetAttribute("name");
			blockTextureData.LocalizedName = Localization.Get(blockTextureData.Name);
			blockTextureData.ID = int.Parse(item.GetAttribute("id"));
			if (dynamicProperties.Values.ContainsKey("Group"))
			{
				blockTextureData.Group = dynamicProperties.Values["Group"];
			}
			if (dynamicProperties.Values.ContainsKey("PaintCost"))
			{
				blockTextureData.PaintCost = Convert.ToUInt16(dynamicProperties.Values["PaintCost"]);
			}
			else
			{
				blockTextureData.PaintCost = 1;
			}
			if (dynamicProperties.Values.ContainsKey("TextureId"))
			{
				blockTextureData.TextureID = Convert.ToUInt16(dynamicProperties.Values["TextureId"]);
			}
			if (dynamicProperties.Values.ContainsKey("Hidden"))
			{
				blockTextureData.Hidden = Convert.ToBoolean(dynamicProperties.Values["Hidden"]);
			}
			if (dynamicProperties.Values.ContainsKey("SortIndex"))
			{
				blockTextureData.SortIndex = Convert.ToByte(dynamicProperties.Values["SortIndex"]);
			}
			blockTextureData.Init();
		}
		yield break;
	}
}
