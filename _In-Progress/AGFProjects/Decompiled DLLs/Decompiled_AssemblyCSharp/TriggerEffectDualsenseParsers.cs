using System.Xml.Linq;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class TriggerEffectDualsenseParsers
{
	public static bool ParseWeaponEffects(string effectType, XElement elementTriggerEffect, string name, out byte[] strengths, out TriggerEffectManager.EffectDualsense effectTypeDualsense, out byte frequency, out byte strength, out byte position, out byte endPosition, out byte amplitude)
	{
		effectTypeDualsense = TriggerEffectManager.EffectDualsense.Off;
		strength = 0;
		position = 0;
		endPosition = 0;
		strengths = null;
		frequency = 0;
		amplitude = 0;
		if (effectType.ContainsCaseInsensitive("MultipointWeapon") || effectType.ContainsCaseInsensitive("WeaponMultipoint"))
		{
			Debug.LogError("Trigger Effect " + effectType + " is recognized, but not supported.");
			return false;
		}
		effectTypeDualsense = TriggerEffectManager.EffectDualsense.WeaponSingle;
		if (!ParseStartEndPosition(effectType, elementTriggerEffect, name, out position, out endPosition))
		{
			return false;
		}
		if (!ParseStrength(effectType, elementTriggerEffect, name, out strength))
		{
			return false;
		}
		return true;
	}

	public static bool ParseStrength(string effectType, XElement elementTriggerEffect, string name, out byte strength)
	{
		if (elementTriggerEffect.TryGetAttribute("strength", out var _result))
		{
			if (StringParsers.TryParseUInt8(_result, out strength))
			{
				if (strength >= 10)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") strength is invalid, correct values are 0 to 9 inclusive");
					return false;
				}
				return true;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") strength failed to parse as float");
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): strength is missing");
		strength = 0;
		return false;
	}

	public static bool ParseStartEndPosition(string effectType, XElement elementTriggerEffect, string name, out byte startPosition, out byte endPosition)
	{
		if (elementTriggerEffect.TryGetAttribute("startPosition", out var _result))
		{
			if (StringParsers.TryParseUInt8(_result, out startPosition))
			{
				if (startPosition >= 10)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") startPosition is invalid, correct values are 0 to 9 inclusive, and must be less than EndPosition");
					endPosition = 0;
					return false;
				}
				if (elementTriggerEffect.TryGetAttribute("endPosition", out var _result2))
				{
					if (StringParsers.TryParseUInt8(_result2, out endPosition))
					{
						if (endPosition >= 11 || startPosition >= endPosition)
						{
							Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition is invalid, correct values are 1 to 10 inclusive, and must be greater than StartPosition");
							return false;
						}
						return true;
					}
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition failed to parse");
					return false;
				}
				Debug.LogError("Trigger effect " + name + "(" + effectType + "): endPosition is missing");
				startPosition = 0;
				endPosition = 0;
				return false;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition failed to parse");
			endPosition = 0;
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): startPosition is missing");
		startPosition = 0;
		endPosition = 0;
		return false;
	}

	public static bool ParseStartEndStrengths(string effectType, XElement elementTriggerEffect, string name, out byte startStrength, out byte endStrength)
	{
		if (elementTriggerEffect.TryGetAttribute("startStrength", out var _result))
		{
			if (StringParsers.TryParseUInt8(_result, out startStrength))
			{
				if (startStrength >= 10)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") startStrength is invalid, correct values are 0 to 9 inclusive");
					endStrength = 0;
					return false;
				}
				if (elementTriggerEffect.TryGetAttribute("endPosition", out var _result2))
				{
					if (StringParsers.TryParseUInt8(_result2, out endStrength))
					{
						if (endStrength >= 11 || startStrength >= endStrength)
						{
							Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition is invalid, correct values are 1 to 10 inclusive, and must be greater than StartPosition");
							return false;
						}
						return true;
					}
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition failed to parse");
					return false;
				}
				Debug.LogError("Trigger effect " + name + "(" + effectType + "): endPosition is missing");
				startStrength = 0;
				endStrength = 0;
				return false;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") endStrength failed to parse");
			endStrength = 0;
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): startPosition is missing");
		startStrength = 0;
		endStrength = 0;
		return false;
	}

	public static bool ParsePosition(string effectType, XElement elementTriggerEffect, string name, out byte position)
	{
		if (elementTriggerEffect.TryGetAttribute("position", out var _result))
		{
			if (StringParsers.TryParseUInt8(_result, out position))
			{
				if (position > 9)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") position is invalid, correct values are 0 to 9 inclusive");
					return false;
				}
				return true;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") position failed to parse");
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): position is missing");
		position = 0;
		return false;
	}

	public static bool ParseAmplitude(string effectType, XElement elementTriggerEffect, string name, out byte amplitude)
	{
		if (elementTriggerEffect.TryGetAttribute("amplitude", out var _result))
		{
			if (StringParsers.TryParseUInt8(_result, out amplitude))
			{
				if (amplitude > 8)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") amplitude is invalid, correct values are 0 to 8 inclusive");
					return false;
				}
				return true;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") amplitude failed to parse");
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): amplitude is missing");
		amplitude = 0;
		return false;
	}

	public static bool ParseFrequency(string effectType, XElement elementTriggerEffect, string name, out byte frequency)
	{
		if (elementTriggerEffect.TryGetAttribute("frequency", out var _result))
		{
			if (!StringParsers.TryParseUInt8(_result, out frequency))
			{
				Debug.LogError("Trigger effect " + name + "(" + effectType + ") frequency failed to parse, valid values are 0-255 inclusive");
				return false;
			}
			return true;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): frequency is missing");
		frequency = 0;
		return false;
	}

	public static bool ParseEffectStrengths(string effectType, XElement elementTriggerEffect, string name, out byte[] strengths)
	{
		if (elementTriggerEffect.TryGetAttribute("strengths", out var _result))
		{
			strengths = new byte[10];
			int num = 0;
			int num2 = 0;
			while (num < _result.Length && num2 < 10)
			{
				if (StringParsers.TryParseUInt8(_result, out var _result2, num, num))
				{
					if (_result2 >= 9)
					{
						Debug.LogError($"Trigger effect {name}({effectType}) strengths[{num2}] is invalid, correct array values are 0 to 8 inclusive, separated by some character");
						break;
					}
					strengths[num2] = _result2;
					num += 2;
					num2++;
					continue;
				}
				Debug.LogError($"Trigger effect {name}({effectType}) strengths[{num2}] failed to parse at character {num}");
				strengths = null;
				break;
			}
			if (num2 == 11 && strengths != null)
			{
				Debug.LogError($"Trigger effect {name}({effectType}) has invalid strength array definition, it's always a 10 length array of 0 to 9 inclusive. actual length:{num2 + 1}");
				strengths = null;
				return false;
			}
			return true;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): Missing attribute; length 10 array \"strengths\" ");
		strengths = null;
		return false;
	}
}
