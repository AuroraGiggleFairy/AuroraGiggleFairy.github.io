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
			XElement xElement = item;
			if (xElement.Name == "animation")
			{
				foreach (XElement item2 in xElement.Elements("hold_type"))
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
			else
			{
				if (!(item.Name == "smell"))
				{
					continue;
				}
				foreach (XElement item3 in item.Elements("smell"))
				{
					if (!item3.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing in smell");
					}
					string attribute = item3.GetAttribute("name");
					if (!item3.HasAttribute("range"))
					{
						throw new Exception("Attribute 'range' missing in smell name='" + attribute + "'");
					}
					float range = StringParsers.ParseFloat(item3.GetAttribute("range"));
					if (!item3.HasAttribute("belt_range"))
					{
						throw new Exception("Attribute 'belt_range' missing in smell name='" + attribute + "'");
					}
					float beltRange = StringParsers.ParseFloat(item3.GetAttribute("belt_range"));
					float heatMapStrength = 0f;
					if (item3.HasAttribute("heat_map_strength"))
					{
						heatMapStrength = StringParsers.ParseFloat(item3.GetAttribute("heat_map_strength"));
					}
					float num4 = 100f;
					if (item3.HasAttribute("heat_map_time"))
					{
						num4 = StringParsers.ParseFloat(item3.GetAttribute("heat_map_time"));
					}
					num4 *= 10f;
					AIDirectorData.AddSmell(attribute, new AIDirectorData.Smell(attribute, range, beltRange, heatMapStrength, (ulong)num4));
				}
			}
		}
		int num5 = 0;
		int num6 = 0;
		foreach (XElement item4 in root.Elements("trigger_effects"))
		{
			foreach (XElement item5 in item4.Elements("trigger_effect"))
			{
				if (item5.HasAttribute("type_ds"))
				{
					num5++;
				}
				else if (item5.HasAttribute("type_xb"))
				{
					num6++;
				}
			}
		}
		TriggerEffectManager.ControllerTriggerEffectsDS.Clear();
		TriggerEffectManager.ControllerTriggerEffectsXb.Clear();
		TriggerEffectManager.ControllerTriggerEffectsDS.EnsureCapacity(num5);
		TriggerEffectManager.ControllerTriggerEffectsXb.EnsureCapacity(num6);
		foreach (XElement item6 in root.Elements("trigger_effects"))
		{
			foreach (XElement item7 in item6.Elements("trigger_effect"))
			{
				if (!item7.HasAttribute("name"))
				{
					Debug.LogError("Every Trigger effect requires a name attribute set to a unique value");
					continue;
				}
				string attribute2 = item7.GetAttribute("name");
				string _result2;
				if (item7.TryGetAttribute("type_ds", out var _result))
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
						if (!TriggerEffectDualsenseParsers.ParseWeaponEffects(_result, item7, attribute2, out strengths, out effectTypeDualsense, out frequency, out strength, out position2, out endPosition, out amplitude))
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
							if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(_result, item7, attribute2, out strengths))
							{
								continue;
							}
						}
						else if (_result.ContainsCaseInsensitive("SlopeFeedback") || _result.ContainsCaseInsensitive("FeedbackSlope"))
						{
							strengths = null;
							frequency = 0;
							effectTypeDualsense = TriggerEffectManager.EffectDualsense.FeedbackSlope;
							if (!TriggerEffectDualsenseParsers.ParseStartEndPosition(_result, item7, attribute2, out position2, out endPosition) || !TriggerEffectDualsenseParsers.ParseStartEndStrengths(_result, item7, attribute2, out strength, out amplitude))
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
							if (!TriggerEffectDualsenseParsers.ParsePosition(_result, item7, attribute2, out position2) || !TriggerEffectDualsenseParsers.ParseStrength(_result, item7, attribute2, out strength))
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
								Debug.LogError("Trigger effectType cannot be redefined: " + attribute2 + ":NoEffect");
							}
							else
							{
								Debug.LogError("Trigger effectType Not supported: " + attribute2 + ":" + _result);
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
							if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(_result, item7, attribute2, out strengths))
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
							if (!TriggerEffectDualsenseParsers.ParsePosition(_result, item7, attribute2, out position2) || !TriggerEffectDualsenseParsers.ParseAmplitude(_result, item7, attribute2, out amplitude) || !TriggerEffectDualsenseParsers.ParseFrequency(_result, item7, attribute2, out frequency))
							{
								continue;
							}
						}
					}
					if (!TriggerEffectManager.ControllerTriggerEffectsDS.TryAdd(attribute2, new TriggerEffectManager.TriggerEffectDS
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
						Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute2);
					}
				}
				else if (item7.TryGetAttribute("type_xb", out _result2))
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
							if (!TriggerEffectXboxParsers.ParseStartEndPosition(_result2, item7, attribute2, out startPosition, out endPosition2) || !TriggerEffectXboxParsers.ParseStartEndStrength(_result2, item7, attribute2, out startStrength, out endStrength))
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
							if (!TriggerEffectXboxParsers.ParseStrength(_result2, item7, attribute2, out startStrength))
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
							if (!TriggerEffectXboxParsers.ParseStartEndPosition(_result2, item7, attribute2, out startPosition, out endPosition2) || !TriggerEffectXboxParsers.ParseStartEndStrength(_result2, item7, attribute2, out startStrength, out endStrength))
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
							if (!TriggerEffectXboxParsers.ParseStrength(_result2, item7, attribute2, out startStrength))
							{
								continue;
							}
						}
					}
					if (!TriggerEffectManager.ControllerTriggerEffectsXb.TryAdd(attribute2, new TriggerEffectManager.TriggerEffectXB
					{
						Effect = effect,
						StartPosition = startPosition,
						EndPosition = endPosition2,
						Strength = startStrength,
						EndStrength = endStrength
					}))
					{
						Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute2);
					}
				}
				else
				{
					Debug.LogError("Trigger effect needs an xb_type or a ds_type: " + attribute2);
				}
			}
		}
	}
}
