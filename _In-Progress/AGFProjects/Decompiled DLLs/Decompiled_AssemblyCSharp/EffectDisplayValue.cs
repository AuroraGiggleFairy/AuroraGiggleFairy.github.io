using System.Xml.Linq;
using UnityEngine;

public class EffectDisplayValue
{
	public string Name;

	public float[] Values;

	public float[] Levels;

	public RequirementGroup Requirements;

	public bool OrCompare;

	public EffectDisplayValue(string _name, float[] _value, float[] _levels, RequirementGroup _requirements)
	{
		Name = _name;
		Values = _value;
		Levels = _levels;
		Requirements = _requirements;
	}

	public bool IsValid(MinEventParams _params)
	{
		return canRun(_params);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRun(MinEventParams _params)
	{
		if (Requirements != null)
		{
			return Requirements.IsValid(_params);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool InLevelRange(float _level, float _min, float _max)
	{
		if (_level >= _min)
		{
			return _level <= _max;
		}
		return false;
	}

	public float GetValue(int _level)
	{
		if (Levels != null)
		{
			if (Values != null)
			{
				if (Values.Length == Levels.Length)
				{
					if (Levels.Length >= 2)
					{
						for (int i = 0; i < Levels.Length - 1; i += 2)
						{
							if (InLevelRange(_level, Levels[i], Levels[i + 1]))
							{
								return Mathf.Lerp(Values[i], Values[i + 1], ((float)_level - Levels[i]) / (Levels[i + 1] - Levels[i]));
							}
						}
					}
					else if (Levels.Length >= 1 && (float)_level == Levels[0])
					{
						return Values[0];
					}
				}
				else
				{
					if (Values.Length == 2 && Levels.Length == 1)
					{
						GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
						if (MinEventParams.CachedEventParam.Seed == 0)
						{
							return (Values[0] + Values[1]) * 0.5f;
						}
						return tempGameRandom.RandomRange(Values[0], Values[1]);
					}
					if (Values.Length == 1 && Levels.Length == 2 && InLevelRange(_level, Levels[0], Levels[1]))
					{
						return Values[0];
					}
				}
			}
		}
		else if (Values != null)
		{
			if (Values.Length == 1)
			{
				return Values[0];
			}
			if (Values.Length == 2)
			{
				return GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed).RandomRange(Values[0], Values[1]);
			}
			return Values[0];
		}
		return 0f;
	}

	public static EffectDisplayValue ParseDisplayValue(XElement _element)
	{
		if (!_element.HasAttribute("name") || !_element.HasAttribute("value"))
		{
			return null;
		}
		RequirementGroup requirements = RequirementBase.ParseRequirementGroup(_element);
		string attribute = _element.GetAttribute("value");
		float[] array = null;
		if (!string.IsNullOrEmpty(attribute))
		{
			if (attribute.Contains(","))
			{
				string[] array2 = attribute.Split(',');
				array = new float[array2.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					if (StringParsers.TryParseFloat(array2[i], out var _result))
					{
						array[i] = _result;
					}
				}
			}
			else
			{
				array = new float[1] { StringParsers.ParseFloat(attribute) };
			}
		}
		string attribute2 = _element.GetAttribute("tier");
		float[] array3 = null;
		if (!string.IsNullOrEmpty(attribute2))
		{
			if (attribute2.Contains(","))
			{
				string[] array4 = attribute2.Split(',');
				array3 = new float[array4.Length];
				for (int j = 0; j < array4.Length; j++)
				{
					array3[j] = StringParsers.ParseFloat(array4[j]);
				}
			}
			else
			{
				array3 = new float[1] { StringParsers.ParseFloat(attribute2) };
			}
		}
		return new EffectDisplayValue(_element.GetAttribute("name"), array, array3, requirements);
	}
}
