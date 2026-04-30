using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class EntityGroupsFromXml
{
	public static IEnumerator LoadEntityGroups(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entitygroups> found!");
		}
		int num = 0;
		foreach (XElement item2 in root.Elements("entitygroup"))
		{
			string attribute = item2.GetAttribute("name");
			if (attribute.Length == 0)
			{
				throw new Exception("Attribute 'name' missing on entitygroup tag");
			}
			List<SEntityClassAndProb> list = new List<SEntityClassAndProb>();
			EntityGroups.list[attribute] = list;
			if (EntityGroups.DefaultGroupName == null)
			{
				EntityGroups.DefaultGroupName = attribute;
			}
			float num2 = 0f;
			foreach (XNode item3 in item2.Nodes())
			{
				if (item3.NodeType == XmlNodeType.Text)
				{
					string value = ((XText)item3).Value;
					int num3 = 0;
					int num4 = value.IndexOf('\n', num3);
					if (num4 < 0)
					{
						num4 = value.Length;
					}
					while (num4 >= 0)
					{
						string text = value.Substring(num3, num4 - num3);
						num3 = num4 + 1;
						if (num3 >= value.Length)
						{
							num4 = -1;
						}
						else
						{
							num4 = value.IndexOf('\n', num3);
							if (num4 < 0)
							{
								num4 = value.Length;
							}
						}
						string text2 = text;
						float num5 = 1f;
						int num6 = text.IndexOf(',');
						if (num6 >= 0)
						{
							text2 = text.Substring(0, num6);
							num5 = StringParsers.ParseFloat(text, num6 + 1);
						}
						text2 = text2.Trim();
						if (text2.Length <= 0)
						{
							continue;
						}
						int num7 = 0;
						if (text2 != "none")
						{
							num7 = EntityClass.FromString(text2);
							if (!EntityClass.list.ContainsKey(num7))
							{
								throw new Exception("Entity with name '" + text2 + "' not found");
							}
						}
						list.Add(new SEntityClassAndProb
						{
							entityClassId = num7,
							prob = num5
						});
						num++;
						num2 += num5;
					}
				}
				if (item3.NodeType != XmlNodeType.Element || !(((XElement)item3).Name == "entity"))
				{
					continue;
				}
				XElement element = (XElement)item3;
				SEntityClassAndProb item = default(SEntityClassAndProb);
				string attribute2 = element.GetAttribute("name");
				if (attribute2.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on entity in group '" + attribute + "'");
				}
				int num8 = 0;
				if (attribute2 != "none")
				{
					num8 = EntityClass.FromString(attribute2);
					if (!EntityClass.list.ContainsKey(num8))
					{
						throw new Exception("Entity with name '" + attribute2 + "' not found");
					}
				}
				item.entityClassId = num8;
				float num9 = 1f;
				string attribute3 = element.GetAttribute("prob");
				if (attribute3.Length > 0)
				{
					num9 = StringParsers.ParseFloat(attribute3);
				}
				item.prob = num9;
				list.Add(item);
				num++;
				num2 += num9;
			}
			if (num2 > 0f)
			{
				EntityGroups.Normalize(attribute, num2);
			}
			if (list.Count == 0)
			{
				throw new Exception("Empty entity groups not allowed! Group name: " + attribute);
			}
		}
		yield break;
	}
}
