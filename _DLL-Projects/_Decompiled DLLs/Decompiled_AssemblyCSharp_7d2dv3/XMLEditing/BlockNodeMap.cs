using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing;

public class BlockNodeMap : IEnumerable<KeyValuePair<string, BlockNode>>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BlockNode> blockNodes = new Dictionary<string, BlockNode>(StringComparer.OrdinalIgnoreCase);

	[PublicizedFrom(EAccessModifier.Private)]
	public XElement root;

	public int Count => blockNodes.Count;

	public IEnumerator<KeyValuePair<string, BlockNode>> GetEnumerator()
	{
		return blockNodes.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return blockNodes.GetEnumerator();
	}

	public bool TryGetValue(string targetName, out BlockNode blockNode)
	{
		return blockNodes.TryGetValue(targetName, out blockNode);
	}

	public void PopulateFromFile(string blocksFilePath)
	{
		XDocument xDocument = XMLUtils.LoadXDocument(blocksFilePath);
		root = xDocument.Root;
		Refresh();
	}

	public void PopulateFromRoot(XElement root)
	{
		this.root = root;
		Refresh();
	}

	public void Refresh()
	{
		if (root == null)
		{
			Debug.LogError("Refresh failed: root element is null. This may occur if you have not called one of the PopulateFrom[...] methods prior to calling Refresh. Otherwise there may be an error in the source xml.");
			return;
		}
		if (!root.HasElements)
		{
			Debug.LogError("Refresh failed: root element has no child elements.");
			return;
		}
		blockNodes.Clear();
		foreach (XElement item in root.Elements(XNames.block))
		{
			string attribute = item.GetAttribute(XNames.name);
			BlockNode value = new BlockNode
			{
				Name = attribute,
				Element = item
			};
			blockNodes[attribute] = value;
		}
		foreach (BlockNode value5 in blockNodes.Values)
		{
			foreach (XElement item2 in value5.Element.Elements(XNames.property))
			{
				string text = item2.GetAttribute(XNames.name);
				if (text == "Extends")
				{
					string attribute2 = item2.GetAttribute(XNames.value);
					if (blockNodes.TryGetValue(attribute2, out var value2))
					{
						value2.AddChild(value5);
						string[] array = item2.GetAttribute(XNames.param1).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string key in array)
						{
							if (!value5.ElementInfos.TryGetValue(key, out var value3))
							{
								value3 = new BlockNode.ElementInfo();
								value5.ElementInfos[key] = value3;
							}
							value3.CanInherit = false;
						}
						BlockNode.ElementInfo elementInfo = new BlockNode.ElementInfo();
						elementInfo.CanInherit = false;
						elementInfo.Element = item2;
						elementInfo.IsClass = false;
						value5.ElementInfos["Extends"] = elementInfo;
					}
					else
					{
						Debug.LogError("Failed to find parent BlockNode \"" + attribute2 + "\" for block \"" + value5.Name + "\"");
					}
					continue;
				}
				bool isClass = false;
				if (string.IsNullOrWhiteSpace(text))
				{
					string attribute3 = item2.GetAttribute(XNames.class_);
					if (string.IsNullOrWhiteSpace(attribute3))
					{
						continue;
					}
					isClass = true;
					text = attribute3;
				}
				if (!value5.ElementInfos.TryGetValue(text, out var value4))
				{
					value4 = new BlockNode.ElementInfo();
					value4.CanInherit = text != "CreativeMode";
					value5.ElementInfos[text] = value4;
				}
				value4.Element = item2;
				value4.IsClass = isClass;
			}
		}
	}
}
