using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearVariants;

[Serializable]
public class SexGearTables
{
	public List<string> gearPaths = new List<string>();

	public List<string> gearGuids = new List<string>();

	public StringTable2D head = new StringTable2D();

	public StringTable2D hands = new StringTable2D();

	public StringTable2D feet = new StringTable2D();

	public StringTable2D GetTable(GearVariantMatrixSO.GearPart part)
	{
		return part switch
		{
			GearVariantMatrixSO.GearPart.Head => head, 
			GearVariantMatrixSO.GearPart.Hands => hands, 
			GearVariantMatrixSO.GearPart.Feet => feet, 
			_ => hands, 
		};
	}

	public void EnsureShapes()
	{
		while (gearGuids.Count < gearPaths.Count)
		{
			gearGuids.Add(string.Empty);
		}
		if (gearGuids.Count > gearPaths.Count)
		{
			gearGuids.RemoveRange(gearPaths.Count, gearGuids.Count - gearPaths.Count);
		}
		head.EnsureShapeSymmetric(gearPaths, gearGuids);
		hands.EnsureShapeSymmetric(gearPaths, gearGuids);
		feet.EnsureShapeSymmetric(gearPaths, gearGuids);
	}

	public void ApplyUnifiedReorder(IList<string> newOrder)
	{
		if (newOrder == null)
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < gearPaths.Count; i++)
		{
			dictionary[gearPaths[i]] = i;
		}
		int[] array = new int[newOrder.Count];
		for (int j = 0; j < newOrder.Count; j++)
		{
			if (!dictionary.TryGetValue(newOrder[j], out var value))
			{
				value = Mathf.Clamp(j, 0, gearPaths.Count - 1);
			}
			array[j] = value;
		}
		List<string> list = new List<string>(newOrder);
		List<string> list2 = new List<string>(list.Count);
		for (int k = 0; k < list.Count; k++)
		{
			int num = array[k];
			string item = ((num >= 0 && num < gearGuids.Count) ? gearGuids[num] : string.Empty);
			list2.Add(item);
		}
		gearPaths = list;
		gearGuids = list2;
		head.ApplyReorder(array);
		hands.ApplyReorder(array);
		feet.ApplyReorder(array);
	}
}
