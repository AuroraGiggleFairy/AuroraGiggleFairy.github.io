using System.Xml.Linq;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class TriggerEffectXboxParsers
{
	public static bool ParseStrength(string effectType, XElement elementTriggerEffect, string name, out float strength)
	{
		if (elementTriggerEffect.TryGetAttribute("strength", out var _result))
		{
			if (StringParsers.TryParseFloat(_result, out strength))
			{
				if (strength > 1f || strength < 0f)
				{
					Debug.LogError($"Trigger effect {name}({effectType}) strength is invalid, correct values are 0 to 1 inclusive, as a floating point number. actual: {strength}");
					return false;
				}
				return true;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") strength failed to parse as a float");
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): strength is missing");
		strength = 0f;
		return false;
	}

	public static bool ParseStartEndPosition(string effectType, XElement elementTriggerEffect, string name, out float startPosition, out float endPosition)
	{
		if (elementTriggerEffect.TryGetAttribute("startPosition", out var _result))
		{
			if (StringParsers.TryParseFloat(_result, out startPosition))
			{
				if (startPosition > 1f || startPosition < 0f)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") startPosition is invalid, correct values are 0 to 1 inclusive, and must be less than EndPosition");
					endPosition = 0f;
					return false;
				}
				if (elementTriggerEffect.TryGetAttribute("endPosition", out var _result2))
				{
					if (StringParsers.TryParseFloat(_result2, out endPosition))
					{
						if (endPosition > 1f || endPosition < 0f || startPosition >= endPosition)
						{
							Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition is invalid, correct array values are 1 to 10 inclusive, and must be greater than StartPosition");
							return false;
						}
						return true;
					}
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition failed to parse");
					return false;
				}
				Debug.LogError("Trigger effect " + name + "(" + effectType + "): endPosition is missing");
				startPosition = 0f;
				endPosition = 0f;
				return false;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") endPosition failed to parse");
			endPosition = 0f;
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): startPosition is missing");
		startPosition = 0f;
		endPosition = 0f;
		return false;
	}

	public static bool ParseStartEndStrength(string effectType, XElement elementTriggerEffect, string name, out float startStrength, out float endStrength)
	{
		if (elementTriggerEffect.TryGetAttribute("startStrength", out var _result))
		{
			if (StringParsers.TryParseFloat(_result, out startStrength))
			{
				if (startStrength > 1f || startStrength < 0f)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") startStrength is invalid, correct values are 0 to 1 inclusive, and must be less than EndStrength");
					endStrength = 0f;
					return false;
				}
				if (elementTriggerEffect.TryGetAttribute("endStrength", out var _result2))
				{
					if (StringParsers.TryParseFloat(_result2, out endStrength))
					{
						if (endStrength > 1f || endStrength < 0f || startStrength >= endStrength)
						{
							Debug.LogError("Trigger effect " + name + "(" + effectType + ") endStrength is invalid, correct array values are 1 to 10 inclusive, and must be greater than StartStrength");
							return false;
						}
						return true;
					}
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") endStrength failed to parse");
					return false;
				}
				Debug.LogError("Trigger effect " + name + "(" + effectType + "): endStrength is missing");
				startStrength = 0f;
				endStrength = 0f;
				return false;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") endStrength failed to parse");
			endStrength = 0f;
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): startStrength is missing");
		startStrength = 0f;
		endStrength = 0f;
		return false;
	}

	public static bool ParseAmplitude(string effectType, XElement elementTriggerEffect, string name, out float amplitude)
	{
		if (elementTriggerEffect.TryGetAttribute("amplitude", out var _result))
		{
			if (StringParsers.TryParseFloat(_result, out amplitude))
			{
				if (amplitude < 8f)
				{
					Debug.LogError("Trigger effect " + name + "(" + effectType + ") amplitude is invalid, correct values are 0 to 1 inclusive");
					return false;
				}
				return true;
			}
			Debug.LogError("Trigger effect " + name + "(" + effectType + ") amplitude failed to parse as float");
			return false;
		}
		Debug.LogError("Trigger effect " + name + "(" + effectType + "): amplitude is missing");
		amplitude = 0f;
		return false;
	}
}
