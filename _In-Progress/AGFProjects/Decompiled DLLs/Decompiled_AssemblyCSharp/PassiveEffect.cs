using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class PassiveEffect
{
	public enum ValueModifierTypes
	{
		base_set,
		base_add,
		base_subtract,
		perc_set,
		perc_add,
		perc_subtract,
		COUNT
	}

	public PassiveEffects Type;

	public ValueModifierTypes Modifier;

	public string[] CVarValues;

	public float[] Values;

	public float[] Levels;

	public RequirementGroup Requirements;

	public FastTags<TagGroup.Global> Tags;

	public bool MatchAnyTags = true;

	public bool InvertTagCheck;

	public void ModifyValue(EntityAlive _ea, float _level, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int _stackEffectMultiplier = 1)
	{
		if (CVarValues != null)
		{
			for (int i = 0; i < CVarValues.Length; i++)
			{
				string text = CVarValues[i];
				if (text != null)
				{
					if (_ea.Buffs.HasCustomVar(text))
					{
						Values[i] = _ea.Buffs.GetCustomVar(text);
						continue;
					}
					_ea.Buffs.AddCustomVar(text, 0f);
					Values[i] = 0f;
				}
			}
		}
		ModValue(Modifier, _level, ref _base_value, ref _perc_value, Levels, Values, _stackEffectMultiplier);
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSource, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, MinEffectController.SourceParentType _parentType, EntityAlive _ea, float _level, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int _stackEffectMultiplier = 1, object _parentPointer = null)
	{
		float _base_value2 = 0f;
		float _perc_value2 = 1f;
		if (CVarValues != null)
		{
			for (int i = 0; i < CVarValues.Length; i++)
			{
				string text = CVarValues[i];
				if (text != null)
				{
					if (_ea.Buffs.HasCustomVar(text))
					{
						Values[i] = _ea.Buffs.GetCustomVar(text);
						continue;
					}
					Log.Out("PassiveEffects: CVar '{0}' was not found in custom variable dictionary for entity '{1}'", text, _ea.EntityName);
				}
			}
		}
		ModValue(Modifier, _level, ref _base_value2, ref _perc_value2, Levels, Values, _stackEffectMultiplier);
		if (_base_value2 != 0f || _perc_value2 != 1f)
		{
			EffectManager.ModifierValuesAndSources modifierValuesAndSources = new EffectManager.ModifierValuesAndSources
			{
				ValueSource = _sourceType,
				ParentType = _parentType,
				Source = _parentPointer,
				ModifierType = Modifier,
				Tags = Tags
			};
			if (Modifier.ToStringCached().Contains("base"))
			{
				modifierValuesAndSources.Value = _base_value2;
			}
			else
			{
				modifierValuesAndSources.Value = _perc_value2;
			}
			_modValueSource.Add(modifierValuesAndSources);
		}
	}

	public bool RequirementsMet(MinEventParams _params)
	{
		if (!hasMatchingTag(_params.Tags))
		{
			return false;
		}
		if (Requirements != null)
		{
			return Requirements.IsValid(_params);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasMatchingTag(FastTags<TagGroup.Global> _tags)
	{
		if (_tags.IsEmpty && !Tags.IsEmpty)
		{
			return false;
		}
		if (MatchAnyTags)
		{
			if (Tags.IsEmpty)
			{
				if (!InvertTagCheck)
				{
					return true;
				}
				return false;
			}
			if (!InvertTagCheck)
			{
				return _tags.Test_AnySet(Tags);
			}
			return !_tags.Test_AnySet(Tags);
		}
		if (!InvertTagCheck)
		{
			return _tags.Test_AllSet(Tags);
		}
		return !_tags.Test_AllSet(Tags);
	}

	public static PassiveEffect ParsePassiveEffect(XElement _element)
	{
		string attribute = _element.GetAttribute("name");
		if (attribute.Length == 0)
		{
			return null;
		}
		string attribute2 = _element.GetAttribute("modifier");
		if (attribute2.Length == 0)
		{
			attribute2 = _element.GetAttribute("operation");
			if (attribute2.Length == 0)
			{
				return null;
			}
		}
		string text = _element.GetAttribute("value");
		if (text.Length == 0)
		{
			return null;
		}
		if (text[0] == '^')
		{
			text = EntityClassesFromXml.sReplacePassiveEffects[text];
		}
		PassiveEffect passiveEffect = new PassiveEffect();
		passiveEffect.Type = EnumUtils.Parse<PassiveEffects>(attribute, _ignoreCase: true);
		if (passiveEffect.Type == PassiveEffects.None)
		{
			return null;
		}
		if (text.Contains(","))
		{
			string[] array = text.Split(',');
			passiveEffect.Values = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				float _result;
				if (array[i].StartsWith("@"))
				{
					if (passiveEffect.CVarValues == null)
					{
						passiveEffect.CVarValues = new string[array.Length];
					}
					passiveEffect.CVarValues[i] = array[i].Trim().Remove(0, 1);
				}
				else if (StringParsers.TryParseFloat(array[i], out _result))
				{
					passiveEffect.Values[i] = _result;
				}
			}
		}
		else if (text.StartsWith("@"))
		{
			passiveEffect.CVarValues = new string[1] { text.Trim().Remove(0, 1) };
			passiveEffect.Values = new float[1];
		}
		else
		{
			passiveEffect.Values = new float[1] { StringParsers.ParseFloat(text) };
		}
		if (passiveEffect.CVarValues != null)
		{
			for (int j = 0; j < passiveEffect.CVarValues.Length; j++)
			{
				string text2 = passiveEffect.CVarValues[j];
				if (text2 != null && text2.Contains("@"))
				{
					Log.Error("CVar reference contains an '@' symbol! This will break calls to it.");
				}
			}
		}
		string attribute3 = _element.GetAttribute("level");
		if (attribute3.Length > 0)
		{
			if (attribute3.Contains(","))
			{
				string[] array2 = attribute3.Split(',');
				passiveEffect.Levels = new float[array2.Length];
				for (int k = 0; k < array2.Length; k++)
				{
					passiveEffect.Levels[k] = StringParsers.ParseFloat(array2[k]);
				}
			}
			else
			{
				passiveEffect.Levels = new float[1] { StringParsers.ParseFloat(attribute3) };
			}
		}
		else if ((attribute3 = _element.GetAttribute("tier")).Length > 0)
		{
			if (attribute3.Contains(","))
			{
				string[] array3 = attribute3.Split(',');
				passiveEffect.Levels = new float[array3.Length];
				for (int l = 0; l < array3.Length; l++)
				{
					passiveEffect.Levels[l] = StringParsers.ParseFloat(array3[l]);
				}
			}
			else
			{
				passiveEffect.Levels = new float[1] { StringParsers.ParseFloat(attribute3) };
			}
		}
		else if ((attribute3 = _element.GetAttribute("duration")).Length > 0)
		{
			if (attribute3.Contains(","))
			{
				string[] array4 = attribute3.Split(',');
				passiveEffect.Levels = new float[array4.Length];
				for (int m = 0; m < array4.Length; m++)
				{
					passiveEffect.Levels[m] = StringParsers.ParseFloat(array4[m]);
				}
			}
			else
			{
				passiveEffect.Levels = new float[1] { StringParsers.ParseFloat(attribute3) };
			}
		}
		string attribute4 = _element.GetAttribute("tags");
		if (attribute4.Length > 0)
		{
			passiveEffect.Tags = FastTags<TagGroup.Global>.Parse(attribute4);
		}
		else
		{
			attribute4 = _element.GetAttribute("tag");
			if (attribute4.Length > 0)
			{
				passiveEffect.Tags = FastTags<TagGroup.Global>.Parse(attribute4);
			}
		}
		if (_element.HasAttribute("match_all_tags"))
		{
			passiveEffect.MatchAnyTags = false;
		}
		if (_element.HasAttribute("invert_tag_check"))
		{
			passiveEffect.InvertTagCheck = true;
		}
		passiveEffect.Modifier = EnumUtils.Parse<ValueModifierTypes>(attribute2);
		passiveEffect.Requirements = RequirementBase.ParseRequirementGroup(_element);
		return passiveEffect;
	}

	public static PassiveEffect CreateEmptyPassiveEffect(PassiveEffects type)
	{
		PassiveEffect passiveEffect = new PassiveEffect();
		passiveEffect.Type = type;
		passiveEffect.Modifier = ValueModifierTypes.perc_add;
		passiveEffect.Values = new float[1];
		return passiveEffect;
	}

	public void AddColoredInfoStrings(ref List<string> _infoList, float _level = -1f)
	{
		if (Levels != null)
		{
			if (_level == -1f)
			{
				for (int i = 0; i < Levels.Length; i++)
				{
					_infoList.Add(GetDisplayValue(Levels[i]));
				}
			}
			else
			{
				_infoList.Add(GetDisplayValue(_level));
			}
		}
		else
		{
			_infoList.Add(GetDisplayValue(0f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ModValue(ValueModifierTypes _modifier, float _level, ref float _base_value, ref float _perc_value, float[] _levels, float[] _values, float _multiplier = 1f, int _seed = 0)
	{
		if (_levels != null)
		{
			if (_values == null)
			{
				return;
			}
			if (_values.Length == _levels.Length)
			{
				if (_levels.Length >= 2)
				{
					for (int num = _levels.Length - 1; num > 0; num--)
					{
						if (InLevelRange(_level, _levels[num - 1], _levels[num]))
						{
							switch (_modifier)
							{
							case ValueModifierTypes.base_set:
								_base_value = Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							case ValueModifierTypes.perc_set:
								_perc_value = Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							case ValueModifierTypes.base_add:
								_base_value += Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							case ValueModifierTypes.perc_add:
								_perc_value += Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							case ValueModifierTypes.base_subtract:
								_base_value -= Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							case ValueModifierTypes.perc_subtract:
								_perc_value -= Mathf.Lerp(_values[num - 1], _values[num], (_level - _levels[num - 1]) / (_levels[num] - _levels[num - 1]));
								break;
							}
							break;
						}
					}
				}
				else if (_levels.Length >= 1 && Mathf.FloorToInt(_level) == Mathf.FloorToInt(_levels[0]))
				{
					switch (_modifier)
					{
					case ValueModifierTypes.base_set:
						_base_value = _values[0];
						break;
					case ValueModifierTypes.perc_set:
						_perc_value = _values[0];
						break;
					case ValueModifierTypes.base_add:
						_base_value += _values[0];
						break;
					case ValueModifierTypes.perc_add:
						_perc_value += _values[0];
						break;
					case ValueModifierTypes.base_subtract:
						_base_value -= _values[0];
						break;
					case ValueModifierTypes.perc_subtract:
						_perc_value -= _values[0];
						break;
					}
				}
			}
			else if (_levels.Length == 1 && _values.Length == 2 && Mathf.FloorToInt(_level) == Mathf.FloorToInt(_levels[0]))
			{
				if (MinEventParams.CachedEventParam.Seed == 0)
				{
					switch (_modifier)
					{
					case ValueModifierTypes.base_set:
						_base_value = (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_set:
						_perc_value = (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.base_add:
						_base_value += (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_add:
						_perc_value += (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.base_subtract:
						_base_value -= (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_subtract:
						_perc_value -= (_values[0] + _values[1]) * 0.5f;
						break;
					}
					return;
				}
				GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
				switch (_modifier)
				{
				case ValueModifierTypes.base_set:
					_base_value = tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_set:
					_perc_value = tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.base_add:
					_base_value += tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_add:
					_perc_value += tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.base_subtract:
					_base_value -= tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_subtract:
					_perc_value -= tempGameRandom.RandomRange(_values[0], _values[1]);
					break;
				}
			}
			else if (_values.Length == 1 && _levels.Length == 2 && InLevelRange(_level, _levels[0], _levels[1]))
			{
				switch (_modifier)
				{
				case ValueModifierTypes.base_set:
					_base_value = _values[0];
					break;
				case ValueModifierTypes.perc_set:
					_perc_value = _values[0];
					break;
				case ValueModifierTypes.base_add:
					_base_value += _values[0];
					break;
				case ValueModifierTypes.perc_add:
					_perc_value += _values[0];
					break;
				case ValueModifierTypes.base_subtract:
					_base_value -= _values[0];
					break;
				case ValueModifierTypes.perc_subtract:
					_perc_value -= _values[0];
					break;
				}
			}
		}
		else
		{
			if (_values == null)
			{
				return;
			}
			if (_values.Length == 1)
			{
				switch (_modifier)
				{
				case ValueModifierTypes.base_set:
					_base_value = _values[0];
					break;
				case ValueModifierTypes.perc_set:
					_perc_value = _values[0];
					break;
				case ValueModifierTypes.base_add:
					_base_value += _values[0];
					break;
				case ValueModifierTypes.perc_add:
					_perc_value += _values[0];
					break;
				case ValueModifierTypes.base_subtract:
					_base_value -= _values[0];
					break;
				case ValueModifierTypes.perc_subtract:
					_perc_value -= _values[0];
					break;
				}
			}
			else
			{
				if (_values.Length != 2)
				{
					return;
				}
				if (MinEventParams.CachedEventParam.Seed == 0)
				{
					switch (_modifier)
					{
					case ValueModifierTypes.base_set:
						_base_value = (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_set:
						_perc_value = (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.base_add:
						_base_value += (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_add:
						_perc_value += (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.base_subtract:
						_base_value -= (_values[0] + _values[1]) * 0.5f;
						break;
					case ValueModifierTypes.perc_subtract:
						_perc_value -= (_values[0] + _values[1]) * 0.5f;
						break;
					}
					return;
				}
				GameRandom tempGameRandom2 = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
				switch (_modifier)
				{
				case ValueModifierTypes.base_set:
					_base_value = tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_set:
					_perc_value = tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.base_add:
					_base_value += tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_add:
					_perc_value += tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.base_subtract:
					_base_value -= tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				case ValueModifierTypes.perc_subtract:
					_perc_value -= tempGameRandom2.RandomRange(_values[0], _values[1]);
					break;
				}
			}
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetDisplayValue(float _level, float _base_value = 0f, float _perc_value = 1f, float _multiplier = 1f)
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
								switch (Modifier)
								{
								case ValueModifierTypes.base_set:
								case ValueModifierTypes.base_add:
								case ValueModifierTypes.base_subtract:
									_base_value = Mathf.Lerp(Values[i], Values[i + 1], (_level - Levels[i]) / (Levels[i + 1] - Levels[i]));
									return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
								case ValueModifierTypes.perc_set:
								case ValueModifierTypes.perc_add:
								case ValueModifierTypes.perc_subtract:
									_perc_value = Mathf.Lerp(Values[i], Values[i + 1], (_level - Levels[i]) / (Levels[i + 1] - Levels[i]));
									return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
								}
							}
						}
					}
					else if (Levels.Length >= 1 && _level == Levels[0])
					{
						switch (Modifier)
						{
						case ValueModifierTypes.base_set:
						case ValueModifierTypes.base_add:
						case ValueModifierTypes.base_subtract:
							_base_value = Values[0];
							return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
						case ValueModifierTypes.perc_set:
						case ValueModifierTypes.perc_add:
						case ValueModifierTypes.perc_subtract:
							_perc_value = Values[0];
							return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
						}
					}
				}
				else if (Values.Length == 2 && Levels.Length == 1)
				{
					GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
					switch (Modifier)
					{
					case ValueModifierTypes.base_set:
					case ValueModifierTypes.base_add:
					case ValueModifierTypes.base_subtract:
						_base_value = ((MinEventParams.CachedEventParam.Seed != 0) ? tempGameRandom.RandomRange(Values[0], Values[1]) : ((Values[0] + Values[1]) * 0.5f));
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
					case ValueModifierTypes.perc_set:
					case ValueModifierTypes.perc_add:
					case ValueModifierTypes.perc_subtract:
						_perc_value = ((MinEventParams.CachedEventParam.Seed != 0) ? tempGameRandom.RandomRange(Values[0], Values[1]) : ((Values[0] + Values[1]) * 0.5f));
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
					}
				}
				else if (Values.Length == 1 && Levels.Length == 2 && InLevelRange(_level, Levels[0], Levels[1]))
				{
					switch (Modifier)
					{
					case ValueModifierTypes.base_set:
					case ValueModifierTypes.base_add:
					case ValueModifierTypes.base_subtract:
						_base_value = Values[0];
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
					case ValueModifierTypes.perc_set:
					case ValueModifierTypes.perc_add:
					case ValueModifierTypes.perc_subtract:
						_perc_value = Values[0];
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
					}
				}
			}
		}
		else if (Values != null)
		{
			if (Values.Length == 1)
			{
				switch (Modifier)
				{
				case ValueModifierTypes.base_set:
				case ValueModifierTypes.base_add:
				case ValueModifierTypes.base_subtract:
					_base_value = Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				case ValueModifierTypes.perc_set:
				case ValueModifierTypes.perc_add:
				case ValueModifierTypes.perc_subtract:
					_perc_value = Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
			else if (Values.Length == 2)
			{
				GameRandom tempGameRandom2 = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
				switch (Modifier)
				{
				case ValueModifierTypes.base_set:
				case ValueModifierTypes.base_add:
				case ValueModifierTypes.base_subtract:
					_base_value = tempGameRandom2.RandomRange(Values[0], Values[1]);
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				case ValueModifierTypes.perc_set:
				case ValueModifierTypes.perc_add:
				case ValueModifierTypes.perc_subtract:
					_perc_value = tempGameRandom2.RandomRange(Values[0], Values[1]);
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
			else
			{
				switch (Modifier)
				{
				case ValueModifierTypes.base_set:
				case ValueModifierTypes.base_add:
				case ValueModifierTypes.base_subtract:
					_base_value = Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", Type.ToStringCached(), (_base_value > 0f && Modifier == ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				case ValueModifierTypes.perc_set:
				case ValueModifierTypes.perc_add:
				case ValueModifierTypes.perc_subtract:
					_perc_value = Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", Type.ToStringCached(), (_perc_value > 0f && Modifier == ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
		}
		return null;
	}
}
