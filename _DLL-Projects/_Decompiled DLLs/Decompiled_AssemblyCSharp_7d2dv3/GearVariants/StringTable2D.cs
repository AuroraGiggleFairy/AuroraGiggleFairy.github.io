using System;
using System.Collections.Generic;

namespace GearVariants;

[Serializable]
public class StringTable2D
{
	[Serializable]
	public class Row
	{
		public string rowKey;

		public string rowGuid;

		public List<string> cellValues = new List<string>();
	}

	public List<string> rowKeys = new List<string>();

	public List<string> columnKeys = new List<string>();

	public List<Row> rows = new List<Row>();

	public void EnsureShapeSymmetric(IReadOnlyList<string> keys, IReadOnlyList<string> guids)
	{
		IReadOnlyList<string> readOnlyList = keys ?? Array.Empty<string>();
		IReadOnlyList<string> readOnlyList2 = guids ?? Array.Empty<string>();
		Dictionary<string, Row> dictionary = new Dictionary<string, Row>(StringComparer.Ordinal);
		Dictionary<string, Row> dictionary2 = new Dictionary<string, Row>(StringComparer.Ordinal);
		foreach (Row row2 in rows)
		{
			if (!string.IsNullOrEmpty(row2.rowGuid) && !dictionary.ContainsKey(row2.rowGuid))
			{
				dictionary.Add(row2.rowGuid, row2);
			}
			if (!string.IsNullOrEmpty(row2.rowKey) && !dictionary2.ContainsKey(row2.rowKey))
			{
				dictionary2.Add(row2.rowKey, row2);
			}
		}
		rowKeys.Clear();
		columnKeys.Clear();
		rowKeys.AddRange(readOnlyList);
		columnKeys.AddRange(readOnlyList);
		int count = columnKeys.Count;
		List<Row> list = new List<Row>(readOnlyList.Count);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			string text = readOnlyList[i];
			string text2 = ((i < readOnlyList2.Count) ? readOnlyList2[i] : string.Empty);
			Row row = null;
			Row value2;
			if (!string.IsNullOrEmpty(text2) && dictionary.TryGetValue(text2, out var value))
			{
				row = value;
			}
			else if (!string.IsNullOrEmpty(text) && dictionary2.TryGetValue(text, out value2))
			{
				row = value2;
			}
			if (row == null)
			{
				row = new Row
				{
					rowKey = text,
					rowGuid = text2,
					cellValues = new List<string>(count)
				};
			}
			else
			{
				row.rowKey = text;
				row.rowGuid = text2;
			}
			if (row.cellValues == null)
			{
				row.cellValues = new List<string>();
			}
			while (row.cellValues.Count < count)
			{
				row.cellValues.Add(string.Empty);
			}
			if (row.cellValues.Count > count)
			{
				row.cellValues.RemoveRange(count, row.cellValues.Count - count);
			}
			list.Add(row);
		}
		rows = list;
	}

	public string Get(int rowIdx, int colIdx)
	{
		return rows[rowIdx].cellValues[colIdx];
	}

	public void Set(int rowIdx, int colIdx, string value)
	{
		rows[rowIdx].cellValues[colIdx] = value ?? string.Empty;
	}

	public void ClampValuesToValidOptions(Func<string, HashSet<string>> getValidOptionsForRowKey)
	{
		for (int i = 0; i < rows.Count; i++)
		{
			Row row = rows[i];
			HashSet<string> hashSet = getValidOptionsForRowKey?.Invoke(row.rowKey);
			if (hashSet == null)
			{
				continue;
			}
			for (int j = 0; j < row.cellValues.Count; j++)
			{
				string item = row.cellValues[j] ?? string.Empty;
				if (!hashSet.Contains(item))
				{
					row.cellValues[j] = string.Empty;
				}
			}
		}
	}

	public void ApplyReorder(IReadOnlyList<int> newToOld)
	{
		int count = rowKeys.Count;
		if (newToOld == null || newToOld.Count != count)
		{
			return;
		}
		List<Row> list = rows;
		List<Row> list2 = new List<Row>(count);
		for (int i = 0; i < count; i++)
		{
			list2.Add(null);
		}
		for (int j = 0; j < count; j++)
		{
			int index = newToOld[j];
			list2[j] = list[index];
		}
		rows = list2;
		for (int k = 0; k < rows.Count; k++)
		{
			List<string> cellValues = rows[k].cellValues;
			List<string> list3 = new List<string>(new string[count]);
			for (int l = 0; l < count; l++)
			{
				int num = newToOld[l];
				list3[l] = ((num >= 0 && num < cellValues.Count) ? cellValues[num] : string.Empty);
			}
			rows[k].cellValues = list3;
		}
		List<string> list4 = rowKeys;
		List<string> list5 = columnKeys;
		List<string> list6 = new List<string>(count);
		List<string> list7 = new List<string>(count);
		for (int m = 0; m < count; m++)
		{
			list6.Add(list4[newToOld[m]]);
			list7.Add(list5[newToOld[m]]);
		}
		rowKeys = list6;
		columnKeys = list7;
	}
}
