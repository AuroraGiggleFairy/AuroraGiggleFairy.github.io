using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing;

public static class OversizedConversionUtils
{
	public class ConversionDebugInfo
	{
		public class OffsetCounts
		{
			[field: PublicizedFrom(EAccessModifier.Private)]
			public int Explicit { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int ShapeNew { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int ShapeModelEntity { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int ShapeExt3dModel { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int ShapeOther { get; set; }

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int Default { get; set; }
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<BlockNode> modifiedBlockNodes = new HashSet<BlockNode>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int baseConversionCount;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public OffsetCounts Offsets
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new OffsetCounts();

		public int ModifiedBlockNodeCount => modifiedBlockNodes.Count;

		public int BaseConversionCount => baseConversionCount;

		public void Reset()
		{
			Offsets.Explicit = 0;
			Offsets.ShapeNew = 0;
			Offsets.ShapeModelEntity = 0;
			Offsets.ShapeExt3dModel = 0;
			Offsets.ShapeOther = 0;
			Offsets.Default = 0;
			modifiedBlockNodes.Clear();
			baseConversionCount = 0;
		}

		public void Log()
		{
			Debug.Log(CountsToString());
		}

		public string CountsToString()
		{
			return $"modifiedBlocksCount: {ModifiedBlockNodeCount}\n" + $"baseConversionCount: {BaseConversionCount}\n" + $"Offsets.Explicit: {Offsets.Explicit}\n" + $"Offsets.ShapeNew: {Offsets.ShapeNew}\n" + $"Offsets.ShapeModelEntity: {Offsets.ShapeModelEntity}\n" + $"Offsets.ShapeExt3dModel: {Offsets.ShapeExt3dModel}\n" + $"Offsets.ShapeOther: {Offsets.ShapeOther}\n" + $"Offsets.Default: {Offsets.Default}";
		}

		public void OnBlockModified(BlockNode blockNode)
		{
			modifiedBlockNodes.Add(blockNode);
		}

		public void OnBaseBlockConverted(BlockNode node)
		{
			baseConversionCount++;
		}

		public void OnOriginModelOffsetTypeCalculated(BlockNode.ModelOffsetType modelOffsetType)
		{
			switch (modelOffsetType)
			{
			case BlockNode.ModelOffsetType.Explicit:
				Offsets.Explicit++;
				break;
			case BlockNode.ModelOffsetType.ShapeNew:
				Offsets.ShapeNew++;
				break;
			case BlockNode.ModelOffsetType.ShapeModelEntity:
				Offsets.ShapeModelEntity++;
				break;
			case BlockNode.ModelOffsetType.ShapeExt3dModel:
				Offsets.ShapeExt3dModel++;
				break;
			case BlockNode.ModelOffsetType.ShapeOther:
				Offsets.ShapeOther++;
				break;
			default:
				Offsets.Default++;
				break;
			}
		}
	}

	public static string OversizedConversionTargetsFilePath => GameIO.GetGameDir("Data/Config") + "/OversizedConversionTargets.txt";

	public static void AutoApplyOversizedConversion(XElement root)
	{
		string oversizedConversionTargetsFilePath = OversizedConversionTargetsFilePath;
		HashSet<string> hashSet = new HashSet<string>();
		if (SdFile.Exists(oversizedConversionTargetsFilePath))
		{
			foreach (string item in SdFile.ReadLines(oversizedConversionTargetsFilePath))
			{
				hashSet.Add(item);
			}
			if (hashSet.Count != 0)
			{
				ConversionDebugInfo conversionDebugInfo = new ConversionDebugInfo();
				ApplyOversizedConversion(root, hashSet, conversionDebugInfo);
				Debug.Log("Automatic oversized conversion complete.\n\n" + $"targetNames.Count: {hashSet.Count}\n" + conversionDebugInfo.CountsToString());
			}
		}
		else
		{
			Debug.LogWarning("Oversized conversion not applied: File \"" + oversizedConversionTargetsFilePath + "\" does not exist.");
		}
	}

	public static void ApplyOversizedConversion(XElement root, HashSet<string> targetNames, ConversionDebugInfo debugInfo = null)
	{
		BlockNodeMap blockNodeMap = new BlockNodeMap();
		blockNodeMap.PopulateFromRoot(root);
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		XMLUtils.PopulateReplacementMap(dictionary);
		FixUndersizedHelpers(blockNodeMap, dictionary);
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string targetName in targetNames)
		{
			if (dictionary.TryGetValue(targetName, out var value))
			{
				hashSet.UnionWith(value);
			}
		}
		HashSet<string> hashSet2 = new HashSet<string>(targetNames);
		hashSet2.UnionWith(hashSet);
		HashSet<BlockNode> targetRootNodes = new HashSet<BlockNode>();
		FindMultiBlockDimRootNodes(blockNodeMap, hashSet2, [PublicizedFrom(EAccessModifier.Internal)] (BlockNode node) =>
		{
			targetRootNodes.Add(node);
		});
		ConvertBlocksToOversized(targetRootNodes, debugInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixUndersizedHelpers(BlockNodeMap blockNodes, Dictionary<string, HashSet<string>> replacementMap)
	{
		Bounds bounds = new Bounds(GameUtils.GetMultiBlockBoundsOffset(Vector3i.one), Vector3i.one);
		foreach (KeyValuePair<string, HashSet<string>> item in replacementMap)
		{
			string key = item.Key;
			HashSet<string> value = item.Value;
			if (!blockNodes.TryGetValue(key, out var blockNode))
			{
				Debug.LogWarning("No blockNode found for block: \"" + key + "\"");
				continue;
			}
			Bounds bounds2;
			if (!blockNode.TryGetPropertyParent("MultiBlockDim", out var propertyParentBlockNode, out var propertyElementInfo, out var depth) || propertyElementInfo.Element == null)
			{
				bounds2 = ((!blockNode.TryGetPropertyParent("OversizedBounds", out propertyParentBlockNode, out var propertyElementInfo2, out depth) || propertyElementInfo2.Element == null) ? bounds : StringParsers.ParseBounds(propertyElementInfo2.Element.GetAttribute(XNames.value)));
			}
			else
			{
				Vector3i vector3i = StringParsers.ParseVector3i(propertyElementInfo.Element.GetAttribute(XNames.value));
				bounds2 = new Bounds(GameUtils.GetMultiBlockBoundsOffset(vector3i), vector3i);
			}
			Bounds bounds3 = bounds;
			foreach (string item2 in value)
			{
				BlockNode.ElementInfo propertyElementInfo3;
				if (!blockNodes.TryGetValue(item2, out var blockNode2))
				{
					Debug.LogWarning("No blockNode found for block: \"" + key + "\"");
				}
				else if (blockNode2.TryGetPropertyParent("MultiBlockDim", out propertyParentBlockNode, out propertyElementInfo3, out depth) && propertyElementInfo3.Element != null)
				{
					Vector3i vector3i2 = StringParsers.ParseVector3i(propertyElementInfo3.Element.GetAttribute(XNames.value));
					Bounds bounds4 = new Bounds(GameUtils.GetMultiBlockBoundsOffset(vector3i2), vector3i2);
					bounds3.Encapsulate(bounds4);
				}
			}
			if (bounds2.Contains(bounds3.min) && bounds2.Contains(bounds3.max))
			{
				continue;
			}
			Vector3 vector = bounds3.center - bounds2.center;
			if ((vector.x != 0f || vector.z != 0f) && blockNode.ElementInfos.TryGetValue("ModelOffset", out var value2) && value2.Element != null)
			{
				Vector3 vector2 = StringParsers.ParseVector3(value2.Element.GetAttribute(XNames.value));
				vector2.x -= vector.x;
				vector2.z -= vector.z;
				value2.Element.SetAttributeValue(XNames.value, $"{vector2.x},{vector2.y},{vector2.z}");
			}
			BlockNode.ElementInfo value3;
			bool flag = blockNode.ElementInfos.TryGetValue("MultiBlockDim", out value3);
			if (flag && !value3.CanInherit && value3.Element == null && blockNode.Parent.TryGetPropertyParent("MultiBlockDim", out propertyParentBlockNode, out var propertyElementInfo4, out depth) && propertyElementInfo4.Element != null)
			{
				Vector3i vector3i3 = StringParsers.ParseVector3i(propertyElementInfo4.Element.GetAttribute(XNames.value));
				if (bounds3.size.Equals(vector3i3) && blockNode.ElementInfos.TryGetValue("Extends", out var value4) && value4.Element != null)
				{
					string attribute = value4.Element.GetAttribute(XNames.param1);
					List<string> list = attribute.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
					if (attribute.ContainsCaseInsensitive("MultiBlockDim"))
					{
						list.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (string val) => string.Equals(val, "MultiBlockDim", StringComparison.OrdinalIgnoreCase));
						attribute = string.Join(",", list);
						if (string.IsNullOrWhiteSpace(attribute))
						{
							value4.Element.Attribute(XNames.param1).Remove();
						}
						else
						{
							value4.Element.SetAttributeValue(XNames.param1, attribute);
						}
						value3.CanInherit = true;
						continue;
					}
				}
			}
			XElement element = XMLUtils.SetProperty(blockNode.Element, "MultiBlockDim", XNames.value, $"{bounds3.size.x},{bounds3.size.y},{bounds3.size.z}");
			if (flag)
			{
				value3.Element = element;
				continue;
			}
			value3 = new BlockNode.ElementInfo
			{
				CanInherit = true,
				Element = element,
				IsClass = false
			};
			blockNode.ElementInfos["MultiBlockDim"] = value3;
		}
	}

	public static void ConvertBlocksToOversized(HashSet<BlockNode> targetRootNodes, ConversionDebugInfo debugInfo = null)
	{
		foreach (BlockNode targetRootNode in targetRootNodes)
		{
			if (!targetRootNode.ElementInfos.TryGetValue("MultiBlockDim", out var value))
			{
				Debug.LogError("targetRootNode does not contain ElementInfo for MultiBlockDim.");
				continue;
			}
			Vector3i vector3i = StringParsers.ParseVector3i(value.Element.GetAttribute(XNames.value));
			if (vector3i.x <= 1 && vector3i.z <= 1)
			{
				Debug.LogError("Block \"" + targetRootNode.Name + "\" appears in targetRootNodes despite having XZ dimension of 1.");
				continue;
			}
			if (!targetRootNode.ShapeSupportsModelOffset())
			{
				Debug.LogError("Block \"" + targetRootNode.Name + "\" has MultiBlockDim but is not a BlockShape type which supports ModelOffset. Conversion of such blocks has not been implemented; please contact engineering if this is required.");
				continue;
			}
			Vector3 modelOffset;
			int depth;
			BlockNode.ModelOffsetType modelOffsetType;
			bool num = targetRootNode.TryGetModelOffset(out modelOffset, out depth, out modelOffsetType);
			debugInfo?.OnOriginModelOffsetTypeCalculated(modelOffsetType);
			if (!num)
			{
				Debug.LogError("Block \"" + targetRootNode.Name + "\" has MultiBlockDim but is not a BlockShape type which supports ModelOffset.");
				continue;
			}
			BakeExplicitModelOffsetRecursive(targetRootNode, debugInfo);
			ConvertToOversizedRecursive(targetRootNode, Vector3.zero, debugInfo);
			debugInfo?.OnBaseBlockConverted(targetRootNode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ConvertToOversizedRecursive(BlockNode targetNode, Vector3 inheritedAutoOffset, ConversionDebugInfo debugInfo = null)
	{
		Vector3 vector = Vector3.zero;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		if (targetNode.ElementInfos.TryGetValue("MultiBlockDim", out var value))
		{
			if (!value.CanInherit)
			{
				flag2 = false;
			}
			if (value.Element != null)
			{
				Vector3i vector3i = StringParsers.ParseVector3i(value.Element.GetAttribute(XNames.value));
				if (vector3i.x <= 1 && vector3i.z <= 1)
				{
					inheritedAutoOffset = Vector3.zero;
					Debug.LogWarning("Oversized conversion child block \"" + targetNode.Name + "\" specifies single-block MultiBlockDim override.");
					flag2 = false;
				}
				else
				{
					vector = (inheritedAutoOffset = GameUtils.GetMultiBlockBoundsOffset(vector3i));
					XMLUtils.SetProperty(targetNode.Element, "OversizedBounds", XNames.value, $"({inheritedAutoOffset.x},{inheritedAutoOffset.y},{inheritedAutoOffset.z}),({vector3i.x},{vector3i.y},{vector3i.z})");
					if (value.Element.Parent != null)
					{
						value.Element.Remove();
						value.Element = null;
					}
					flag3 = true;
					flag = true;
				}
			}
		}
		int depth;
		if (targetNode.ElementInfos.TryGetValue("ModelOffset", out var value2))
		{
			inheritedAutoOffset = (value2.CanInherit ? inheritedAutoOffset : vector);
			if (value2.Element != null)
			{
				Vector3 vector2 = StringParsers.ParseVector3(value2.Element.GetAttribute(XNames.value));
				vector2.x += inheritedAutoOffset.x;
				vector2.z += inheritedAutoOffset.z;
				if (value2.CanInherit && targetNode.Parent != null && targetNode.Parent.TryGetModelOffset(out var modelOffset, out depth, out var modelOffsetType) && modelOffsetType == BlockNode.ModelOffsetType.Explicit && modelOffset == vector2)
				{
					value2.Element.Remove();
					value2.Element = null;
				}
				else
				{
					XMLUtils.SetProperty(targetNode.Element, "ModelOffset", XNames.value, $"{vector2.x},{vector2.y},{vector2.z}");
				}
				flag = true;
			}
		}
		if (targetNode.ElementInfos.TryGetValue("Extends", out var value3) && value3.Element != null)
		{
			string attribute = value3.Element.GetAttribute(XNames.param1);
			List<string> list = attribute.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
			bool flag4 = false;
			if (targetNode.Parent != null)
			{
				BlockNode propertyParentBlockNode;
				BlockNode.ElementInfo propertyElementInfo;
				bool flag5 = targetNode.Parent.TryGetPropertyParent("MultiBlockDim", out propertyParentBlockNode, out propertyElementInfo, out depth) && propertyElementInfo.Element != null;
				bool flag6 = attribute.ContainsCaseInsensitive("MultiBlockDim");
				if (flag6 && !flag5)
				{
					list.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (string val) => string.Equals(val, "MultiBlockDim", StringComparison.OrdinalIgnoreCase));
					flag4 = true;
				}
				else if (!flag6 && flag5 && flag3)
				{
					list.Add("MultiBlockDim");
					flag4 = true;
				}
			}
			if (!flag2 && !attribute.ContainsCaseInsensitive("OversizedBounds"))
			{
				list.Add("OversizedBounds");
				flag4 = true;
			}
			if (flag4)
			{
				attribute = string.Join(",", list);
				XMLUtils.SetProperty(targetNode.Element, "Extends", XNames.param1, attribute);
				flag = true;
			}
		}
		else if (!flag2)
		{
			Debug.LogError("Failed to find extends element on block \"" + targetNode.Name + "\".");
		}
		if (flag)
		{
			debugInfo?.OnBlockModified(targetNode);
		}
		foreach (BlockNode child in targetNode.Children)
		{
			ConvertToOversizedRecursive(child, inheritedAutoOffset, debugInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BakeExplicitModelOffsetRecursive(BlockNode blockNode, ConversionDebugInfo debugInfo = null)
	{
		foreach (BlockNode child in blockNode.Children)
		{
			BakeExplicitModelOffsetRecursive(child, debugInfo);
		}
		if (!blockNode.TryGetModelOffset(out var modelOffset, out var _, out var _))
		{
			Debug.LogError("BlockNode " + blockNode.Name + " does not have a valid implict or explicit model offset.");
			return;
		}
		XElement element = XMLUtils.SetProperty(blockNode.Element, "ModelOffset", XNames.value, $"{modelOffset.x},{modelOffset.y},{modelOffset.z}");
		if (blockNode.ElementInfos.TryGetValue("ModelOffset", out var value))
		{
			value.Element = element;
		}
		else
		{
			value = new BlockNode.ElementInfo
			{
				CanInherit = true,
				Element = element,
				IsClass = false
			};
			blockNode.ElementInfos["ModelOffset"] = value;
		}
		debugInfo?.OnBlockModified(blockNode);
	}

	public static void FindMultiBlockDimRootNodes(BlockNodeMap blockNodes, HashSet<string> targetNames, Action<BlockNode> onRootNodeFound)
	{
		foreach (string targetName in targetNames)
		{
			if (!blockNodes.TryGetValue(targetName, out var blockNode))
			{
				Debug.LogWarning("No blockNode found for block: \"" + targetName + "\"");
				continue;
			}
			BlockNode blockNode2 = null;
			for (BlockNode blockNode3 = blockNode; blockNode3 != null; blockNode3 = blockNode3.Parent)
			{
				if (blockNode3.ElementInfos.TryGetValue("MultiBlockDim", out var value) && value.Element != null)
				{
					Vector3i vector3i = StringParsers.ParseVector3i(value.Element.GetAttribute(XNames.value));
					if (vector3i.x > 1 || vector3i.z > 1)
					{
						blockNode2 = blockNode3;
					}
				}
			}
			if (blockNode2 != null)
			{
				onRootNodeFound?.Invoke(blockNode2);
			}
		}
	}
}
