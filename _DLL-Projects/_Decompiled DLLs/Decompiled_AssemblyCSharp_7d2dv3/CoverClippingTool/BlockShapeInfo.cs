using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using XMLEditing;

namespace CoverClippingTool;

public class BlockShapeInfo
{
	public static readonly Vector3 PlacerModelOffset = new Vector3(0.5f, 0f, 0.5f);

	public static readonly Vector3 DefaultModelOffset = new Vector3(0f, 0.5f, 0f);

	public readonly string Name;

	public readonly BlockShapeInfo Extends;

	public readonly DataSource Source;

	public readonly XElement Element;

	public readonly XDocument Doc;

	public readonly string ModelName;

	public readonly Vector3 ModelOffset;

	public readonly Vector3i MultiBlockDim;

	public readonly bool IsOversized;

	public readonly Bounds OversizeBounds;

	public readonly Block.MultiBlockArray MultiBlockArray;

	public BlockFaceFlag CoverFaceMask;

	public XElement CoverFaceMaskElement;

	public readonly Dictionary<Vector3i, BlockFaceFlag> CoverFaceMaskMulti = new Dictionary<Vector3i, BlockFaceFlag>();

	public XElement CoverFaceMaskMultiElement;

	public readonly BoundsInt BlockBounds;

	public BlockFaceFlag modifiedCoverFaceMask;

	public readonly Dictionary<Vector3i, BlockFaceFlag> modifiedCoverFaceMaskMulti = new Dictionary<Vector3i, BlockFaceFlag>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly BlockFaceFlag[] cubeSideGizmoFaceFlags = new BlockFaceFlag[6]
	{
		BlockFaceFlag.Top,
		BlockFaceFlag.Bottom,
		BlockFaceFlag.North,
		BlockFaceFlag.West,
		BlockFaceFlag.South,
		BlockFaceFlag.East
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] cubeSideGizmoFaceChars = new char[6] { 'T', 'B', 'N', 'W', 'S', 'E' };

	public bool HasPendingChanges
	{
		get
		{
			if (modifiedCoverFaceMask != CoverFaceMask)
			{
				return true;
			}
			if (!CoverFaceMaskMulti.ValuesEquals(modifiedCoverFaceMaskMulti))
			{
				return true;
			}
			return false;
		}
	}

	public Vector3 RenderOffset
	{
		get
		{
			if (Source == DataSource.Block)
			{
				Vector3 modelOffset = ModelOffset;
				Vector3 vector = Vector3.zero;
				if (IsOversized)
				{
					vector = -OversizeBounds.center;
				}
				else
				{
					vector.y = -0.5f;
					if (MultiBlockDim != Vector3i.one)
					{
						if ((MultiBlockDim.x & 1) == 0)
						{
							vector.x = -0.5f;
						}
						if ((MultiBlockDim.z & 1) == 0)
						{
							vector.z = -0.5f;
						}
					}
				}
				modelOffset += vector;
				modelOffset.y += 0.5f;
				return PlacerModelOffset + modelOffset;
			}
			return ModelOffset;
		}
	}

	public void RevertChanges()
	{
		modifiedCoverFaceMask = CoverFaceMask;
	}

	public bool SaveChanges()
	{
		if (HasPendingChanges)
		{
			if (modifiedCoverFaceMask != BlockFaceFlag.None || (Extends != null && Extends.CoverFaceMask != modifiedCoverFaceMask))
			{
				CoverFaceMask = modifiedCoverFaceMask;
				XMLUtils.SetProperty(Element, CoverMaskBlockPlacer.PropCoverMask, XNames.value, SerializeCoverMaskValue(CoverFaceMask));
			}
			else if (CoverFaceMaskElement != null)
			{
				CoverFaceMask = BlockFaceFlag.None;
				if (CoverFaceMaskMultiElement.IsChildOf(Element))
				{
					if (CoverFaceMaskElement.Parent != null)
					{
						CoverFaceMaskElement.Remove();
					}
					CoverFaceMaskElement = null;
				}
				else
				{
					XMLUtils.SetProperty(Element, CoverMaskBlockPlacer.PropCoverMask, XNames.value, SerializeCoverMaskValue(CoverFaceMask));
				}
			}
			if (modifiedCoverFaceMaskMulti.Count > 0 || (Extends != null && !Extends.CoverFaceMaskMulti.ValuesEquals(CoverFaceMaskMulti)))
			{
				CoverFaceMaskMulti.Clear();
				modifiedCoverFaceMaskMulti.CopyTo(CoverFaceMaskMulti);
				CoverFaceMaskMultiElement = XMLUtils.SetArray(Element, CoverMaskBlockPlacer.PropCoverMaskMulti);
				CoverFaceMaskMultiElement.RemoveNodes();
				CoverFaceMaskMultiElement.Add("\r\n");
				foreach (KeyValuePair<Vector3i, BlockFaceFlag> item in CoverFaceMaskMulti.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<Vector3i, BlockFaceFlag> x) => x.Key.ToString()))
				{
					XElement xElement = new XElement(XNames.item);
					CoverFaceMaskMultiElement.Add("\t");
					CoverFaceMaskMultiElement.Add("\t");
					CoverFaceMaskMultiElement.Add("\t");
					CoverFaceMaskMultiElement.Add(xElement);
					CoverFaceMaskMultiElement.Add("\r\n");
					xElement.SetAttributeValue(CoverMaskBlockPlacer.PropCoverOffset, item.Key.ToString());
					xElement.SetAttributeValue(CoverMaskBlockPlacer.PropCoverMask, SerializeCoverMaskValue(item.Value));
				}
				CoverFaceMaskMultiElement.Add("\t");
				CoverFaceMaskMultiElement.Add("\t");
			}
			else if (CoverFaceMaskMultiElement != null)
			{
				CoverFaceMaskMulti.Clear();
				if (CoverFaceMaskMultiElement.IsChildOf(Element))
				{
					if (CoverFaceMaskMultiElement.Parent != null)
					{
						CoverFaceMaskMultiElement.Remove();
					}
					CoverFaceMaskMultiElement = null;
				}
				else
				{
					CoverFaceMaskMultiElement = XMLUtils.SetArray(Element, CoverMaskBlockPlacer.PropCoverMaskMulti);
					CoverFaceMaskMultiElement.RemoveNodes();
					CoverFaceMaskMultiElement.Add("\r\n");
				}
			}
			return true;
		}
		return false;
	}

	public BlockShapeInfo(string name, BlockShapeInfo extends, ShapeDataSet dataSet, XElement element, ShapeData shapeData)
	{
		Name = name;
		Extends = extends;
		Source = dataSet.Source;
		Element = element;
		Doc = element.Document;
		OversizeBounds = default(Bounds);
		if (shapeData.Properties.TryGetValue("Model", out var value))
		{
			ModelName = value.Value;
		}
		ModelOffset = ((Source == DataSource.Block) ? new Vector3(0f, 0.5f, 0f) : new Vector3(1f, 0f, 1f));
		if (shapeData.Properties.TryGetValue("ModelOffset", out var value2))
		{
			ModelOffset = StringParsers.ParseVector3(value2.Value);
		}
		PropertyValue value4;
		if (shapeData.Properties.TryGetValue("MultiBlockDim", out var value3))
		{
			MultiBlockDim = StringParsers.ParseVector3i(value3.Value);
			List<Vector3i> list = new List<Vector3i>();
			if (shapeData.Properties.ContainsKey(Block.PropMultiBlockLayer0))
			{
				int num = 0;
				while (shapeData.Properties.ContainsKey(Block.PropMultiBlockLayer + num))
				{
					string[] array = shapeData.Properties[Block.PropMultiBlockLayer + num].Value.Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = array[i].Trim();
						if (array[i].Length > MultiBlockDim.x)
						{
							throw new Exception("Multi block layer entry " + i + " too long for block ");
						}
						for (int j = 0; j < array[i].Length; j++)
						{
							if (array[i][j] != ' ')
							{
								list.Add(new Vector3i(j, num, i));
							}
						}
					}
					num++;
				}
			}
			else
			{
				int num2 = MultiBlockDim.x / 2;
				int num3 = Mathf.RoundToInt((float)MultiBlockDim.x / 2f + 0.1f) - 1;
				int num4 = MultiBlockDim.z / 2;
				int num5 = Mathf.RoundToInt((float)MultiBlockDim.z / 2f + 0.1f) - 1;
				for (int k = -num2; k <= num3; k++)
				{
					for (int l = 0; l < MultiBlockDim.y; l++)
					{
						for (int m = -num4; m <= num5; m++)
						{
							list.Add(new Vector3i(k, l, m));
						}
					}
				}
			}
			MultiBlockArray = new Block.MultiBlockArray(MultiBlockDim, list);
		}
		else if (shapeData.Properties.TryGetValue(Block.PropOversizedBounds, out value4))
		{
			IsOversized = true;
			OversizeBounds = StringParsers.ParseBounds(value4.Value);
			MultiBlockDim = World.worldToBlockPos(OversizeBounds.size);
		}
		else
		{
			MultiBlockDim = Vector3i.one;
		}
		if (shapeData.Properties.TryGetValue(CoverMaskBlockPlacer.PropCoverMask, out var value5))
		{
			CoverFaceMaskElement = value5.Element;
			if (CoverFaceMaskElement != null && !CoverFaceMaskElement.IsChildOf(Element))
			{
				CoverFaceMaskElement = null;
			}
			CoverFaceMask = StringParsers.ParseCoverFaceMask(value5.Value);
		}
		else
		{
			CoverFaceMaskElement = null;
			CoverFaceMask = BlockFaceFlag.None;
		}
		modifiedCoverFaceMask = CoverFaceMask;
		CoverFaceMaskMulti.Clear();
		if (shapeData.Arrays.TryGetValue(CoverMaskBlockPlacer.PropCoverMaskMulti, out var value6))
		{
			CoverFaceMaskMultiElement = value6.Element;
			if (CoverFaceMaskMultiElement != null && !CoverFaceMaskMultiElement.IsChildOf(Element))
			{
				CoverFaceMaskMultiElement = null;
			}
			foreach (Dictionary<string, PropertyValue> item in value6.Items)
			{
				if (item != null && item.TryGetValue(CoverMaskBlockPlacer.PropCoverOffset, out var value7) && item.TryGetValue(CoverMaskBlockPlacer.PropCoverMask, out var value8) && StringParsers.TryParseVector3i(value7.Value, out var _output))
				{
					CoverFaceMaskMulti[_output] = StringParsers.ParseCoverFaceMask(value8.Value);
				}
			}
		}
		CoverFaceMaskMulti.CopyTo(modifiedCoverFaceMaskMulti);
		if (MultiBlockArray != null)
		{
			HashSet<Vector3i> hashSet = MultiBlockArray.pos.ToHashSet();
			foreach (KeyValuePair<Vector3i, BlockFaceFlag> item2 in modifiedCoverFaceMaskMulti)
			{
				if (!hashSet.Contains(item2.Key))
				{
					modifiedCoverFaceMaskMulti.Remove(item2.Key);
				}
			}
		}
		else
		{
			modifiedCoverFaceMaskMulti.Clear();
		}
		if (MultiBlockArray != null && MultiBlockArray.Length > 0)
		{
			BlockBounds = MultiBlockArray.GetBlockBounds();
		}
		else
		{
			BlockBounds = new BoundsInt(Vector3Int.zero, Vector3Int.zero);
		}
	}

	public static string SerializeCoverMaskValue(BlockFaceFlag coverFaceMask)
	{
		string text = string.Empty;
		for (int i = 0; i < cubeSideGizmoFaceFlags.Length; i++)
		{
			if ((coverFaceMask & cubeSideGizmoFaceFlags[i]) != BlockFaceFlag.None)
			{
				text += $"{cubeSideGizmoFaceChars[i]},";
			}
		}
		return text;
	}
}
