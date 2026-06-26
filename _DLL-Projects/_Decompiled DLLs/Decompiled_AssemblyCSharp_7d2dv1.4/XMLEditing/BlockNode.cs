using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing;

public class BlockNode
{
	public enum ModelOffsetType
	{
		None,
		Explicit,
		ShapeNew,
		ShapeModelEntity,
		ShapeExt3dModel,
		ShapeOther,
		DefaultShapeNew
	}

	public class ElementInfo
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool CanInherit { get; set; } = true;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool IsClass { get; set; } = true;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public XElement Element { get; set; }
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XElement Element { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public BlockNode Parent { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<BlockNode> Children { get; } = new List<BlockNode>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ElementInfo> ElementInfos
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new Dictionary<string, ElementInfo>();

	public void AddChild(BlockNode child)
	{
		child.Parent = this;
		Children.Add(child);
	}

	public bool TryGetPropertyParent(string targetPropertyName, out BlockNode propertyParentBlockNode, out ElementInfo propertyElementInfo, out int depth)
	{
		depth = 0;
		BlockNode blockNode = this;
		while (blockNode != null)
		{
			if (depth >= 100)
			{
				Debug.LogError("Max recursion depth exceeded!");
				break;
			}
			if (blockNode.ElementInfos.TryGetValue(targetPropertyName, out var value))
			{
				if (value.Element != null)
				{
					propertyParentBlockNode = blockNode;
					propertyElementInfo = value;
					return true;
				}
				if (!value.CanInherit)
				{
					propertyParentBlockNode = null;
					propertyElementInfo = null;
					return false;
				}
			}
			blockNode = blockNode.Parent;
			depth++;
		}
		propertyParentBlockNode = null;
		propertyElementInfo = null;
		return false;
	}

	public bool TryGetModelOffset(out Vector3 modelOffset, out int depth, out ModelOffsetType modelOffsetType)
	{
		if (TryGetPropertyParent("ModelOffset", out var propertyParentBlockNode, out var propertyElementInfo, out depth))
		{
			modelOffsetType = ModelOffsetType.Explicit;
			modelOffset = StringParsers.ParseVector3(propertyElementInfo.Element.GetAttribute(XNames.value));
			return true;
		}
		if (TryGetPropertyParent("Shape", out propertyParentBlockNode, out var propertyElementInfo2, out depth))
		{
			string text = propertyElementInfo2.Element.GetAttribute(XNames.value).Trim();
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("BlockShape", text);
			if (typeWithPrefix == null)
			{
				modelOffsetType = ModelOffsetType.None;
				Debug.LogError("Failed to create shape type \"BlockShape" + text + "\" for block: " + Name);
				modelOffset = Vector3.zero;
				return false;
			}
			if (typeof(BlockShapeNew).IsAssignableFrom(typeWithPrefix))
			{
				modelOffsetType = ModelOffsetType.ShapeNew;
				modelOffset = new Vector3(1f, 0f, 1f);
				return true;
			}
			if (typeof(BlockShapeModelEntity).IsAssignableFrom(typeWithPrefix))
			{
				modelOffsetType = ModelOffsetType.ShapeModelEntity;
				modelOffset = new Vector3(0f, 0.5f, 0f);
				return true;
			}
			if (typeof(BlockShapeExt3dModel).IsAssignableFrom(typeWithPrefix))
			{
				modelOffsetType = ModelOffsetType.ShapeExt3dModel;
				modelOffset = Vector3.zero;
				return true;
			}
			modelOffsetType = ModelOffsetType.None;
			modelOffset = Vector3.zero;
			return false;
		}
		depth = 0;
		BlockNode blockNode = this;
		while (blockNode.Parent != null)
		{
			blockNode = blockNode.Parent;
			depth++;
		}
		modelOffsetType = ModelOffsetType.DefaultShapeNew;
		modelOffset = new Vector3(1f, 0f, 1f);
		return true;
	}

	public bool ShapeSupportsModelOffset()
	{
		if (TryGetPropertyParent("Shape", out var _, out var propertyElementInfo, out var _))
		{
			string text = propertyElementInfo.Element.GetAttribute(XNames.value).Trim();
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("BlockShape", text);
			if (typeWithPrefix == null)
			{
				Debug.LogError("Failed to create shape type \"BlockShape" + text + "\" for block: " + Name);
				return false;
			}
			if (!typeof(BlockShapeNew).IsAssignableFrom(typeWithPrefix) && !typeof(BlockShapeModelEntity).IsAssignableFrom(typeWithPrefix) && !typeof(BlockShapeExt3dModel).IsAssignableFrom(typeWithPrefix))
			{
				return false;
			}
		}
		return true;
	}
}
