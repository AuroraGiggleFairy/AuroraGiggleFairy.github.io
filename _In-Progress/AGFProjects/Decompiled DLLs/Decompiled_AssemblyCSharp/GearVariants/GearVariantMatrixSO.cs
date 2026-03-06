using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearVariants;

public class GearVariantMatrixSO : ScriptableObject
{
	public enum GearPart
	{
		Head,
		Hands,
		Feet
	}

	public enum Sex
	{
		Male,
		Female
	}

	[Header("Sex: Male")]
	public SexGearTables male = new SexGearTables();

	[Header("Sex: Female")]
	public SexGearTables female = new SexGearTables();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GearVariantMatrixSO _instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> _maleIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _maleIndexVersionRow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> _femaleIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _femaleIndexVersionRow;

	public static GearVariantMatrixSO Instance
	{
		get
		{
			if (_instance == null)
			{
				if (!Application.isPlaying)
				{
					LoadManager.InitSync();
				}
				_instance = DataLoader.LoadAsset<GearVariantMatrixSO>("@:Entities/Player/Common/GearVariantMatrix.asset");
			}
			return _instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnsureIndex(Sex sex)
	{
		List<string> list = ((sex != Sex.Male) ? female?.gearPaths : male?.gearPaths) ?? new List<string>();
		int count = list.Count;
		if (sex == Sex.Male)
		{
			if (_maleIndex == null || _maleIndexVersionRow != count)
			{
				_maleIndex = new Dictionary<string, int>(StringComparer.Ordinal);
				for (int i = 0; i < list.Count; i++)
				{
					_maleIndex[list[i]] = i;
				}
				_maleIndexVersionRow = count;
			}
		}
		else if (_femaleIndex == null || _femaleIndexVersionRow != count)
		{
			_femaleIndex = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int j = 0; j < list.Count; j++)
			{
				_femaleIndex[list[j]] = j;
			}
			_femaleIndexVersionRow = count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SexGearTables GetSexTables(Sex sex)
	{
		if (sex != Sex.Male)
		{
			return female;
		}
		return male;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParseSex(string sexName, out Sex sex)
	{
		string text = sexName.ToLower();
		if (!(text == "male"))
		{
			if (text == "female")
			{
				sex = Sex.Female;
				return true;
			}
			sex = Sex.Male;
			return false;
		}
		sex = Sex.Male;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParsePart(string partName, out GearPart part)
	{
		switch (partName)
		{
		case "head":
			part = GearPart.Head;
			return true;
		case "hands":
			part = GearPart.Hands;
			return true;
		case "feet":
			part = GearPart.Feet;
			return true;
		default:
			switch (partName?.Trim().ToLowerInvariant())
			{
			case "head":
				part = GearPart.Head;
				return true;
			case "hands":
				part = GearPart.Hands;
				return true;
			case "feet":
				part = GearPart.Feet;
				return true;
			default:
				part = GearPart.Hands;
				return false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StringTable2D GetTable(Sex sex, GearPart part)
	{
		return GetSexTables(sex)?.GetTable(part);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetIndices(Sex sex, string sourceGearPath, string targetBodyPath, out int row, out int col)
	{
		row = -1;
		col = -1;
		if (GetSexTables(sex) == null)
		{
			return false;
		}
		EnsureIndex(sex);
		Dictionary<string, int> dictionary = ((sex == Sex.Male) ? _maleIndex : _femaleIndex);
		if (dictionary == null)
		{
			return false;
		}
		if (!dictionary.TryGetValue(sourceGearPath, out row))
		{
			return false;
		}
		if (!dictionary.TryGetValue(targetBodyPath, out col))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetVariantOrEmpty(Sex sex, GearPart part, string sourceGearPath, string targetBodyPath)
	{
		if (string.IsNullOrEmpty(sourceGearPath) || string.IsNullOrEmpty(targetBodyPath))
		{
			return string.Empty;
		}
		StringTable2D table = GetTable(sex, part);
		if (table == null)
		{
			return string.Empty;
		}
		EnsureShapes();
		if (!TryGetIndices(sex, sourceGearPath, targetBodyPath, out var row, out var col))
		{
			return string.Empty;
		}
		if (row < 0 || row >= table.rows.Count)
		{
			return string.Empty;
		}
		if (col < 0 || col >= table.columnKeys.Count)
		{
			return string.Empty;
		}
		string text = table.Get(row, col);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetVariant(Sex sex, GearPart part, string sourceGearPath, string targetBodyPath, out string variant)
	{
		variant = string.Empty;
		if (string.IsNullOrEmpty(sourceGearPath) || string.IsNullOrEmpty(targetBodyPath))
		{
			return false;
		}
		StringTable2D table = GetTable(sex, part);
		if (table == null)
		{
			return false;
		}
		EnsureShapes();
		if (!TryGetIndices(sex, sourceGearPath, targetBodyPath, out var row, out var col))
		{
			return false;
		}
		if (row < 0 || row >= table.rows.Count)
		{
			return false;
		}
		if (col < 0 || col >= table.columnKeys.Count)
		{
			return false;
		}
		variant = table.Get(row, col) ?? string.Empty;
		return true;
	}

	public string GetVariantOrEmpty(string sex, string part, string sourceGearPath, string targetBodyPath)
	{
		if (!TryParsePart(part, out var part2))
		{
			return string.Empty;
		}
		if (!TryParseSex(sex, out var sex2))
		{
			return string.Empty;
		}
		return GetVariantOrEmpty(sex2, part2, sourceGearPath, targetBodyPath);
	}

	public bool TryGetVariant(string sex, string part, string sourceGearPath, string targetBodyPath, out string variant)
	{
		if (!TryParsePart(part, out var part2))
		{
			variant = string.Empty;
			return false;
		}
		if (!TryParseSex(sex, out var sex2))
		{
			variant = string.Empty;
			return false;
		}
		return TryGetVariant(sex2, part2, sourceGearPath, targetBodyPath, out variant);
	}

	public void EnsureShapes()
	{
		male?.EnsureShapes();
		female?.EnsureShapes();
	}
}
