using System;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;

public static class MiscFromXml
{
	public static IEnumerator Create(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			yield break;
		}
		foreach (XElement item in root.Elements("animation"))
		{
			if (!(item.Name == "animation"))
			{
				continue;
			}
			foreach (XElement item2 in item.Elements("hold_type"))
			{
				if (!item2.HasAttribute("id"))
				{
					throw new Exception("Attribute 'id' missing in hold_type");
				}
				int result = 0;
				if (!int.TryParse(item2.GetAttribute("id"), out result))
				{
					throw new Exception("Unknown hold_type id for animation");
				}
				float num = 0f;
				if (item2.HasAttribute("ray_cast"))
				{
					num = StringParsers.ParseFloat(item2.GetAttribute("ray_cast"));
				}
				float rayCastMoving = num;
				if (item2.HasAttribute("ray_cast_moving"))
				{
					rayCastMoving = StringParsers.ParseFloat(item2.GetAttribute("ray_cast_moving"));
				}
				float num2 = Constants.cMinHolsterTime;
				if (item2.HasAttribute("holster"))
				{
					num2 = Utils.FastMax(StringParsers.ParseFloat(item2.GetAttribute("holster")), num2);
				}
				float num3 = Constants.cMinUnHolsterTime;
				if (item2.HasAttribute("unholster"))
				{
					num3 = Utils.FastMax(StringParsers.ParseFloat(item2.GetAttribute("unholster")), num3);
				}
				Vector3 position = Vector3.zero;
				if (item2.HasAttribute("third_person_position"))
				{
					position = StringParsers.ParseVector3(item2.GetAttribute("third_person_position"));
				}
				Vector3 rotation = Vector3.zero;
				if (item2.HasAttribute("third_person_rotation"))
				{
					rotation = StringParsers.ParseVector3(item2.GetAttribute("third_person_rotation"));
				}
				bool twoHanded = false;
				if (item2.HasAttribute("two_handed"))
				{
					twoHanded = StringParsers.ParseBool(item2.GetAttribute("two_handed"));
				}
				AnimationDelayData.AnimationDelay[result] = new AnimationDelayData.AnimationDelays(num, rayCastMoving, num2, num3, twoHanded);
				AnimationGunjointOffsetData.AnimationGunjointOffset[result] = new AnimationGunjointOffsetData.AnimationGunjointOffsets(position, rotation);
			}
		}
		int num4 = 0;
		int num5 = 0;
		foreach (XElement item3 in root.Elements("trigger_effects"))
		{
			foreach (XElement item4 in item3.Elements("trigger_effect"))
			{
				if (item4.HasAttribute("type_ds"))
				{
					num4++;
				}
				else if (item4.HasAttribute("type_xb"))
				{
					num5++;
				}
			}
		}
		TriggerEffectManager.ControllerTriggerEffectsDS.Clear();
		TriggerEffectManager.ControllerTriggerEffectsXb.Clear();
		TriggerEffectManager.ControllerTriggerEffectsDS.EnsureCapacity(num4);
		TriggerEffectManager.ControllerTriggerEffectsXb.EnsureCapacity(num5);
		foreach (XElement item5 in root.Elements("trigger_effects"))
		{
			foreach (XElement item6 in item5.Elements("trigger_effect"))
			{
				if (!item6.HasAttribute("name"))
				{
					Debug.LogError("Every Trigger effect requires a name attribute set to a unique value");
					continue;
				}
				string attribute = item6.GetAttribute("name");
				string _result2;
				if (item6.TryGetAttribute("type_ds", out var _result))
				{
					byte[] strengths;
					TriggerEffectManager.EffectDualsense effectTypeDualsense;
					byte frequency;
					byte strength;
					byte position2;
					byte endPosition;
					byte amplitude;
					if (_result.ContainsCaseInsensitive("Weapon"))
					{
						if (!TriggerEffectDualsenseParsers.ParseWeaponEffects(_result, item6, attribute, out strengths, out effectTypeDualsense, out frequency, out strength, out position2, out endPosition, out amplitude))
						{
							continue;
						}
					}
					else if (_result.ContainsCaseInsensitive("Feedback"))
					{
						if (_result.ContainsCaseInsensitive("MultipointFeedback") || _result.ContainsCaseInsensitive("FeedbackMultipoint"))
						{
							strength = 0;
							position2 = 0;
							endPosition = 0;
							amplitude = 0;
							frequency = 0;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.FeedbackMultipoint;
							if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(_result, item6, attribute, out strengths))
							{
								continue;
							}
						}
						else if (_result.ContainsCaseInsensitive("SlopeFeedback") || _result.ContainsCaseInsensitive("FeedbackSlope"))
						{
							strengths = null;
							frequency = 0;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.FeedbackSlope;
							if (!TriggerEffectDualsenseParsers.ParseStartEndPosition(_result, item6, attribute, out position2, out endPosition) || !TriggerEffectDualsenseParsers.ParseStartEndStrengths(_result, item6, attribute, out strength, out amplitude))
							{
								continue;
							}
						}
						else
						{
							strength = 0;
							strengths = null;
							endPosition = 0;
							amplitude = 0;
							frequency = 0;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.FeedbackSingle;
							if (!TriggerEffectDualsenseParsers.ParsePosition(_result, item6, attribute, out position2) || !TriggerEffectDualsenseParsers.ParseStrength(_result, item6, attribute, out strength))
							{
								continue;
							}
						}
					}
					else
					{
						if (!_result.ContainsCaseInsensitive("Vibration"))
						{
							if (_result.ContainsCaseInsensitive("NoEffect"))
							{
								Debug.LogError("Trigger effectType cannot be redefined: " + attribute + ":NoEffect");
							}
							else
							{
								Debug.LogError("Trigger effectType Not supported: " + attribute + ":" + _result);
							}
							continue;
						}
						if (_result.ContainsCaseInsensitive("Multipoint"))
						{
							strength = 0;
							amplitude = 0;
							frequency = 0;
							position2 = 0;
							endPosition = 0;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.VibrationMultipoint;
							if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(_result, item6, attribute, out strengths))
							{
								continue;
							}
						}
						else
						{
							if (_result.ContainsCaseInsensitive("Slope"))
							{
								Debug.LogWarning("Trigger effectType: " + _result + "(type_ds) is not implemented");
								continue;
							}
							endPosition = 0;
							strength = 0;
							strengths = null;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.VibrationSingle;
							if (!TriggerEffectDualsenseParsers.ParsePosition(_result, item6, attribute, out position2) || !TriggerEffectDualsenseParsers.ParseAmplitude(_result, item6, attribute, out amplitude) || !TriggerEffectDualsenseParsers.ParseFrequency(_result, item6, attribute, out frequency))
							{
								continue;
							}
						}
					}
					if (!TriggerEffectManager.ControllerTriggerEffectsDS.TryAdd(attribute, new TriggerEffectManager.TriggerEffectDS
					{
						Effect = effectTypeDualsense,
						Frequency = frequency,
						Position = position2,
						EndPosition = endPosition,
						Strength = strength,
						AmplitudeEndStrength = amplitude,
						Strengths = strengths
					}))
					{
						Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute);
					}
				}
				else if (item6.TryGetAttribute("type_xb", out _result2))
				{
					TriggerEffectManager.EffectXbox effect;
					float startPosition;
					float endPosition2;
					float startStrength;
					float endStrength;
					if (_result2.ContainsCaseInsensitive("Feedback"))
					{
						if (_result2.ContainsCaseInsensitive("SlopeFeedback") || _result2.ContainsCaseInsensitive("FeedbackSlope"))
						{
							effect = TriggerEffectManager.EffectXbox.FeedbackSlope;
							if (!TriggerEffectXboxParsers.ParseStartEndPosition(_result2, item6, attribute, out startPosition, out endPosition2) || !TriggerEffectXboxParsers.ParseStartEndStrength(_result2, item6, attribute, out startStrength, out endStrength))
							{
								continue;
							}
						}
						else
						{
							startPosition = 0f;
							endPosition2 = 0f;
							endStrength = 0f;
							effect = TriggerEffectManager.EffectXbox.FeedbackSingle;
							if (!TriggerEffectXboxParsers.ParseStrength(_result2, item6, attribute, out startStrength))
							{
								continue;
							}
						}
					}
					else
					{
						if (!_result2.ContainsCaseInsensitive("Vibration"))
						{
							if (_result2.ContainsCaseInsensitive("NoEffect"))
							{
								Debug.LogError("Trigger effectType cannot be redefined: " + _result2);
							}
							else
							{
								Debug.LogError("Trigger effectType Not supported: " + _result2);
							}
							continue;
						}
						if (_result2.ContainsCaseInsensitive("SlopeVibration") || _result2.ContainsCaseInsensitive("VibrationSlope"))
						{
							effect = TriggerEffectManager.EffectXbox.VibrationSlope;
							if (!TriggerEffectXboxParsers.ParseStartEndPosition(_result2, item6, attribute, out startPosition, out endPosition2) || !TriggerEffectXboxParsers.ParseStartEndStrength(_result2, item6, attribute, out startStrength, out endStrength))
							{
								continue;
							}
						}
						else
						{
							startPosition = 0f;
							endPosition2 = 0f;
							endStrength = 0f;
							effect = TriggerEffectManager.EffectXbox.VibrationSingle;
							if (!TriggerEffectXboxParsers.ParseStrength(_result2, item6, attribute, out startStrength))
							{
								continue;
							}
						}
					}
					if (!TriggerEffectManager.ControllerTriggerEffectsXb.TryAdd(attribute, new TriggerEffectManager.TriggerEffectXB
					{
						Effect = effect,
						StartPosition = startPosition,
						EndPosition = endPosition2,
						Strength = startStrength,
						EndStrength = endStrength
					}))
					{
						Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute);
					}
				}
				else
				{
					Debug.LogError("Trigger effect needs an xb_type or a ds_type: " + attribute);
				}
			}
		}
	}
}
