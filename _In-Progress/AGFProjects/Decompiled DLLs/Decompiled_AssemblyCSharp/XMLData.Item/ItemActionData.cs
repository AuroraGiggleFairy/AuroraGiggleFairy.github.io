using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class ItemActionData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Delay",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Range",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundStart",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundRepeat",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundEnd",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundEmpty",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundReload",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundWarning",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"StaminaUsage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UseTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname1",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname2",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname3",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname4",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname5",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname6",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname7",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname8",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FocusedBlockname9",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ChangeItemTo",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ChangeBlockTo",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DoBlockAction",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainHealth",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainFood",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainWater",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainStamina",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainSickness",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainWellness",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Buff",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"BuffChance",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Debuff",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CreateItem",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ConditionRaycastBlock",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"GainGas",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Consume",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Blockname",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ThrowStrengthDefault",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ThrowStrengthMax",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"MaxStrainTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"MagazineSize",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"MagazineItem",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ReloadTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"BulletIcon",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RaysPerShot",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RaysSpread",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Sphere",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CrosshairMinDistance",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CrosshairMaxDistance",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DamageEntity",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DamageBlock",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ParticlesMuzzleFire",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ParticlesMuzzleSmoke",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"BlockRange",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"AutoFire",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"HordeMeterRate",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"HordeMeterDistance",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"HitmaskOverride",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SingleMagazineUsage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"BulletMaterial",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"InfiniteAmmo",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ZoomMaxOut",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ZoomMaxIn",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ZoomOverlay",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Velocity",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FlyTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"LifeTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CollisionRadius",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ProjectileInitialVelocity",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Fertileblock",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Adjacentblock",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RepairAmount",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UpgradeHitOffset",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"AllowedUpgradeItems",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RestrictedUpgradeItems",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UpgradeActionSound",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RepairActionSound",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ReferenceItem",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Mesh",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ActionIdx",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Title",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Description",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RecipesToLearn",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"InstantiateOnLoad",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundDraw",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DamageBonus",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Explosion",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static DataItem<string> ParseItem(string _string, PositionXmlElement _elem)
		{
			string startValue;
			try
			{
				startValue = stringParser.Parse(ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true));
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"" + _elem.Name + "\" value \"" + ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true) + "\"", _elem.LineNumber, innerException);
			}
			return new DataItem<string>(_string, startValue);
		}

		public static ItemAction Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemAction");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			ItemAction itemAction = (ItemAction)Activator.CreateInstance(type);
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (XmlNode childNode in _elem.ChildNodes)
			{
				switch (childNode.NodeType)
				{
				case XmlNodeType.Element:
				{
					PositionXmlElement positionXmlElement = (PositionXmlElement)childNode;
					if (knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
					{
						switch (positionXmlElement.Name)
						{
						case "Delay":
						{
							float startValue38;
							try
							{
								startValue38 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException36)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException36);
							}
							DataItem<float> pDelay = new DataItem<float>("Delay", startValue38);
							itemAction.pDelay = pDelay;
							break;
						}
						case "Range":
						{
							float startValue70;
							try
							{
								startValue70 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException68)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException68);
							}
							DataItem<float> pRange = new DataItem<float>("Range", startValue70);
							itemAction.pRange = pRange;
							break;
						}
						case "SoundStart":
						{
							string startValue6;
							try
							{
								startValue6 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<string> pSoundStart = new DataItem<string>("SoundStart", startValue6);
							itemAction.pSoundStart = pSoundStart;
							break;
						}
						case "SoundRepeat":
						{
							string startValue54;
							try
							{
								startValue54 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException52)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException52);
							}
							DataItem<string> pSoundRepeat = new DataItem<string>("SoundRepeat", startValue54);
							itemAction.pSoundRepeat = pSoundRepeat;
							break;
						}
						case "SoundEnd":
						{
							string startValue22;
							try
							{
								startValue22 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException20)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException20);
							}
							DataItem<string> pSoundEnd = new DataItem<string>("SoundEnd", startValue22);
							itemAction.pSoundEnd = pSoundEnd;
							break;
						}
						case "SoundEmpty":
						{
							string startValue78;
							try
							{
								startValue78 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException76)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException76);
							}
							DataItem<string> pSoundEmpty = new DataItem<string>("SoundEmpty", startValue78);
							itemAction.pSoundEmpty = pSoundEmpty;
							break;
						}
						case "SoundReload":
						{
							string startValue62;
							try
							{
								startValue62 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException60)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException60);
							}
							DataItem<string> pSoundReload = new DataItem<string>("SoundReload", startValue62);
							itemAction.pSoundReload = pSoundReload;
							break;
						}
						case "SoundWarning":
						{
							string startValue46;
							try
							{
								startValue46 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException44)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException44);
							}
							DataItem<string> pSoundWarning = new DataItem<string>("SoundWarning", startValue46);
							itemAction.pSoundWarning = pSoundWarning;
							break;
						}
						case "StaminaUsage":
						{
							string startValue30;
							try
							{
								startValue30 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException28)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException28);
							}
							DataItem<string> pStaminaUsage = new DataItem<string>("StaminaUsage", startValue30);
							itemAction.pStaminaUsage = pStaminaUsage;
							break;
						}
						case "UseTime":
						{
							string startValue14;
							try
							{
								startValue14 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException12)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException12);
							}
							DataItem<string> pUseTime = new DataItem<string>("UseTime", startValue14);
							itemAction.pUseTime = pUseTime;
							break;
						}
						case "FocusedBlockname1":
						{
							string startValue82;
							try
							{
								startValue82 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException80)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException80);
							}
							DataItem<string> pFocusedBlockname9 = new DataItem<string>("FocusedBlockname1", startValue82);
							itemAction.pFocusedBlockname1 = pFocusedBlockname9;
							break;
						}
						case "FocusedBlockname2":
						{
							string startValue74;
							try
							{
								startValue74 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException72)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException72);
							}
							DataItem<string> pFocusedBlockname8 = new DataItem<string>("FocusedBlockname2", startValue74);
							itemAction.pFocusedBlockname2 = pFocusedBlockname8;
							break;
						}
						case "FocusedBlockname3":
						{
							string startValue66;
							try
							{
								startValue66 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException64)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException64);
							}
							DataItem<string> pFocusedBlockname7 = new DataItem<string>("FocusedBlockname3", startValue66);
							itemAction.pFocusedBlockname3 = pFocusedBlockname7;
							break;
						}
						case "FocusedBlockname4":
						{
							string startValue58;
							try
							{
								startValue58 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException56)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException56);
							}
							DataItem<string> pFocusedBlockname6 = new DataItem<string>("FocusedBlockname4", startValue58);
							itemAction.pFocusedBlockname4 = pFocusedBlockname6;
							break;
						}
						case "FocusedBlockname5":
						{
							string startValue50;
							try
							{
								startValue50 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException48)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException48);
							}
							DataItem<string> pFocusedBlockname5 = new DataItem<string>("FocusedBlockname5", startValue50);
							itemAction.pFocusedBlockname5 = pFocusedBlockname5;
							break;
						}
						case "FocusedBlockname6":
						{
							string startValue42;
							try
							{
								startValue42 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException40)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException40);
							}
							DataItem<string> pFocusedBlockname4 = new DataItem<string>("FocusedBlockname6", startValue42);
							itemAction.pFocusedBlockname6 = pFocusedBlockname4;
							break;
						}
						case "FocusedBlockname7":
						{
							string startValue34;
							try
							{
								startValue34 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException32)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException32);
							}
							DataItem<string> pFocusedBlockname3 = new DataItem<string>("FocusedBlockname7", startValue34);
							itemAction.pFocusedBlockname7 = pFocusedBlockname3;
							break;
						}
						case "FocusedBlockname8":
						{
							string startValue26;
							try
							{
								startValue26 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException24)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException24);
							}
							DataItem<string> pFocusedBlockname2 = new DataItem<string>("FocusedBlockname8", startValue26);
							itemAction.pFocusedBlockname8 = pFocusedBlockname2;
							break;
						}
						case "FocusedBlockname9":
						{
							string startValue18;
							try
							{
								startValue18 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException16)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException16);
							}
							DataItem<string> pFocusedBlockname = new DataItem<string>("FocusedBlockname9", startValue18);
							itemAction.pFocusedBlockname9 = pFocusedBlockname;
							break;
						}
						case "ChangeItemTo":
						{
							string startValue10;
							try
							{
								startValue10 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException8)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException8);
							}
							DataItem<string> pChangeItemTo = new DataItem<string>("ChangeItemTo", startValue10);
							itemAction.pChangeItemTo = pChangeItemTo;
							break;
						}
						case "ChangeBlockTo":
						{
							string startValue85;
							try
							{
								startValue85 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException83)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException83);
							}
							DataItem<string> pChangeBlockTo = new DataItem<string>("ChangeBlockTo", startValue85);
							itemAction.pChangeBlockTo = pChangeBlockTo;
							break;
						}
						case "DoBlockAction":
							itemAction.pDoBlockAction = ParseItem("DoBlockAction", positionXmlElement);
							break;
						case "GainHealth":
						{
							float startValue80;
							try
							{
								startValue80 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException78)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException78);
							}
							DataItem<float> pGainHealth = new DataItem<float>("GainHealth", startValue80);
							itemAction.pGainHealth = pGainHealth;
							break;
						}
						case "GainFood":
						{
							float startValue76;
							try
							{
								startValue76 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException74)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException74);
							}
							DataItem<float> pGainFood = new DataItem<float>("GainFood", startValue76);
							itemAction.pGainFood = pGainFood;
							break;
						}
						case "GainWater":
						{
							float startValue72;
							try
							{
								startValue72 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException70)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException70);
							}
							DataItem<float> pGainWater = new DataItem<float>("GainWater", startValue72);
							itemAction.pGainWater = pGainWater;
							break;
						}
						case "GainStamina":
						{
							float startValue68;
							try
							{
								startValue68 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException66)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException66);
							}
							DataItem<float> pGainStamina = new DataItem<float>("GainStamina", startValue68);
							itemAction.pGainStamina = pGainStamina;
							break;
						}
						case "GainSickness":
						{
							float startValue64;
							try
							{
								startValue64 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException62)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException62);
							}
							DataItem<float> pGainSickness = new DataItem<float>("GainSickness", startValue64);
							itemAction.pGainSickness = pGainSickness;
							break;
						}
						case "GainWellness":
						{
							float startValue60;
							try
							{
								startValue60 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException58)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException58);
							}
							DataItem<float> pGainWellness = new DataItem<float>("GainWellness", startValue60);
							itemAction.pGainWellness = pGainWellness;
							break;
						}
						case "Buff":
						{
							string startValue56;
							try
							{
								startValue56 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException54)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException54);
							}
							DataItem<string> pBuff = new DataItem<string>("Buff", startValue56);
							itemAction.pBuff = pBuff;
							break;
						}
						case "BuffChance":
						{
							string startValue52;
							try
							{
								startValue52 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException50)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException50);
							}
							DataItem<string> pBuffChance = new DataItem<string>("BuffChance", startValue52);
							itemAction.pBuffChance = pBuffChance;
							break;
						}
						case "Debuff":
						{
							string startValue48;
							try
							{
								startValue48 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException46)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException46);
							}
							DataItem<string> pDebuff = new DataItem<string>("Debuff", startValue48);
							itemAction.pDebuff = pDebuff;
							break;
						}
						case "CreateItem":
						{
							string startValue44;
							try
							{
								startValue44 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException42)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException42);
							}
							DataItem<string> pCreateItem = new DataItem<string>("CreateItem", startValue44);
							itemAction.pCreateItem = pCreateItem;
							break;
						}
						case "ConditionRaycastBlock":
						{
							int startValue40;
							try
							{
								startValue40 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException38)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException38);
							}
							DataItem<int> pConditionRaycastBlock = new DataItem<int>("ConditionRaycastBlock", startValue40);
							itemAction.pConditionRaycastBlock = pConditionRaycastBlock;
							break;
						}
						case "GainGas":
						{
							int startValue36;
							try
							{
								startValue36 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException34)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException34);
							}
							DataItem<int> pGainGas = new DataItem<int>("GainGas", startValue36);
							itemAction.pGainGas = pGainGas;
							break;
						}
						case "Consume":
						{
							bool startValue32;
							try
							{
								startValue32 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException30)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException30);
							}
							DataItem<bool> pConsume = new DataItem<bool>("Consume", startValue32);
							itemAction.pConsume = pConsume;
							break;
						}
						case "Blockname":
						{
							string startValue28;
							try
							{
								startValue28 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException26)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException26);
							}
							DataItem<string> pBlockname = new DataItem<string>("Blockname", startValue28);
							itemAction.pBlockname = pBlockname;
							break;
						}
						case "ThrowStrengthDefault":
						{
							float startValue24;
							try
							{
								startValue24 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException22)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException22);
							}
							DataItem<float> pThrowStrengthDefault = new DataItem<float>("ThrowStrengthDefault", startValue24);
							itemAction.pThrowStrengthDefault = pThrowStrengthDefault;
							break;
						}
						case "ThrowStrengthMax":
						{
							float startValue20;
							try
							{
								startValue20 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException18)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException18);
							}
							DataItem<float> pThrowStrengthMax = new DataItem<float>("ThrowStrengthMax", startValue20);
							itemAction.pThrowStrengthMax = pThrowStrengthMax;
							break;
						}
						case "MaxStrainTime":
						{
							float startValue16;
							try
							{
								startValue16 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException14)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException14);
							}
							DataItem<float> pMaxStrainTime = new DataItem<float>("MaxStrainTime", startValue16);
							itemAction.pMaxStrainTime = pMaxStrainTime;
							break;
						}
						case "MagazineSize":
						{
							int startValue12;
							try
							{
								startValue12 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException10)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException10);
							}
							DataItem<int> pMagazineSize = new DataItem<int>("MagazineSize", startValue12);
							itemAction.pMagazineSize = pMagazineSize;
							break;
						}
						case "MagazineItem":
						{
							string startValue8;
							try
							{
								startValue8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException6)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException6);
							}
							DataItem<string> pMagazineItem = new DataItem<string>("MagazineItem", startValue8);
							itemAction.pMagazineItem = pMagazineItem;
							break;
						}
						case "ReloadTime":
						{
							float startValue4;
							try
							{
								startValue4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<float> pReloadTime = new DataItem<float>("ReloadTime", startValue4);
							itemAction.pReloadTime = pReloadTime;
							break;
						}
						case "BulletIcon":
						{
							string startValue84;
							try
							{
								startValue84 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException82)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException82);
							}
							DataItem<string> pBulletIcon = new DataItem<string>("BulletIcon", startValue84);
							itemAction.pBulletIcon = pBulletIcon;
							break;
						}
						case "RaysPerShot":
						{
							int startValue83;
							try
							{
								startValue83 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException81)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException81);
							}
							DataItem<int> pRaysPerShot = new DataItem<int>("RaysPerShot", startValue83);
							itemAction.pRaysPerShot = pRaysPerShot;
							break;
						}
						case "RaysSpread":
						{
							float startValue81;
							try
							{
								startValue81 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException79)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException79);
							}
							DataItem<float> pRaysSpread = new DataItem<float>("RaysSpread", startValue81);
							itemAction.pRaysSpread = pRaysSpread;
							break;
						}
						case "Sphere":
						{
							float startValue79;
							try
							{
								startValue79 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException77)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException77);
							}
							DataItem<float> pSphere = new DataItem<float>("Sphere", startValue79);
							itemAction.pSphere = pSphere;
							break;
						}
						case "CrosshairMinDistance":
						{
							int startValue77;
							try
							{
								startValue77 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException75)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException75);
							}
							DataItem<int> pCrosshairMinDistance = new DataItem<int>("CrosshairMinDistance", startValue77);
							itemAction.pCrosshairMinDistance = pCrosshairMinDistance;
							break;
						}
						case "CrosshairMaxDistance":
						{
							int startValue75;
							try
							{
								startValue75 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException73)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException73);
							}
							DataItem<int> pCrosshairMaxDistance = new DataItem<int>("CrosshairMaxDistance", startValue75);
							itemAction.pCrosshairMaxDistance = pCrosshairMaxDistance;
							break;
						}
						case "DamageEntity":
						{
							int startValue73;
							try
							{
								startValue73 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException71)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException71);
							}
							DataItem<int> pDamageEntity = new DataItem<int>("DamageEntity", startValue73);
							itemAction.pDamageEntity = pDamageEntity;
							break;
						}
						case "DamageBlock":
						{
							float startValue71;
							try
							{
								startValue71 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException69)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException69);
							}
							DataItem<float> pDamageBlock = new DataItem<float>("DamageBlock", startValue71);
							itemAction.pDamageBlock = pDamageBlock;
							break;
						}
						case "ParticlesMuzzleFire":
						{
							string startValue69;
							try
							{
								startValue69 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException67)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException67);
							}
							DataItem<string> pParticlesMuzzleFire = new DataItem<string>("ParticlesMuzzleFire", startValue69);
							itemAction.pParticlesMuzzleFire = pParticlesMuzzleFire;
							break;
						}
						case "ParticlesMuzzleSmoke":
						{
							string startValue67;
							try
							{
								startValue67 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException65)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException65);
							}
							DataItem<string> pParticlesMuzzleSmoke = new DataItem<string>("ParticlesMuzzleSmoke", startValue67);
							itemAction.pParticlesMuzzleSmoke = pParticlesMuzzleSmoke;
							break;
						}
						case "BlockRange":
						{
							float startValue65;
							try
							{
								startValue65 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException63)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException63);
							}
							DataItem<float> pBlockRange = new DataItem<float>("BlockRange", startValue65);
							itemAction.pBlockRange = pBlockRange;
							break;
						}
						case "AutoFire":
						{
							bool startValue63;
							try
							{
								startValue63 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException61)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException61);
							}
							DataItem<bool> pAutoFire = new DataItem<bool>("AutoFire", startValue63);
							itemAction.pAutoFire = pAutoFire;
							break;
						}
						case "HordeMeterRate":
						{
							float startValue61;
							try
							{
								startValue61 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException59)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException59);
							}
							DataItem<float> pHordeMeterRate = new DataItem<float>("HordeMeterRate", startValue61);
							itemAction.pHordeMeterRate = pHordeMeterRate;
							break;
						}
						case "HordeMeterDistance":
						{
							float startValue59;
							try
							{
								startValue59 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException57)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException57);
							}
							DataItem<float> pHordeMeterDistance = new DataItem<float>("HordeMeterDistance", startValue59);
							itemAction.pHordeMeterDistance = pHordeMeterDistance;
							break;
						}
						case "HitmaskOverride":
						{
							string startValue57;
							try
							{
								startValue57 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException55)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException55);
							}
							DataItem<string> pHitmaskOverride = new DataItem<string>("HitmaskOverride", startValue57);
							itemAction.pHitmaskOverride = pHitmaskOverride;
							break;
						}
						case "SingleMagazineUsage":
						{
							bool startValue55;
							try
							{
								startValue55 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException53)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException53);
							}
							DataItem<bool> pSingleMagazineUsage = new DataItem<bool>("SingleMagazineUsage", startValue55);
							itemAction.pSingleMagazineUsage = pSingleMagazineUsage;
							break;
						}
						case "BulletMaterial":
						{
							string startValue53;
							try
							{
								startValue53 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException51)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException51);
							}
							DataItem<string> pBulletMaterial = new DataItem<string>("BulletMaterial", startValue53);
							itemAction.pBulletMaterial = pBulletMaterial;
							break;
						}
						case "InfiniteAmmo":
						{
							bool startValue51;
							try
							{
								startValue51 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException49)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException49);
							}
							DataItem<bool> pInfiniteAmmo = new DataItem<bool>("InfiniteAmmo", startValue51);
							itemAction.pInfiniteAmmo = pInfiniteAmmo;
							break;
						}
						case "ZoomMaxOut":
						{
							float startValue49;
							try
							{
								startValue49 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException47)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException47);
							}
							DataItem<float> pZoomMaxOut = new DataItem<float>("ZoomMaxOut", startValue49);
							itemAction.pZoomMaxOut = pZoomMaxOut;
							break;
						}
						case "ZoomMaxIn":
						{
							float startValue47;
							try
							{
								startValue47 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException45)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException45);
							}
							DataItem<float> pZoomMaxIn = new DataItem<float>("ZoomMaxIn", startValue47);
							itemAction.pZoomMaxIn = pZoomMaxIn;
							break;
						}
						case "ZoomOverlay":
						{
							string startValue45;
							try
							{
								startValue45 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException43)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException43);
							}
							DataItem<string> pZoomOverlay = new DataItem<string>("ZoomOverlay", startValue45);
							itemAction.pZoomOverlay = pZoomOverlay;
							break;
						}
						case "Velocity":
						{
							int startValue43;
							try
							{
								startValue43 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException41)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException41);
							}
							DataItem<int> pVelocity = new DataItem<int>("Velocity", startValue43);
							itemAction.pVelocity = pVelocity;
							break;
						}
						case "FlyTime":
						{
							float startValue41;
							try
							{
								startValue41 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException39)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException39);
							}
							DataItem<float> pFlyTime = new DataItem<float>("FlyTime", startValue41);
							itemAction.pFlyTime = pFlyTime;
							break;
						}
						case "LifeTime":
						{
							float startValue39;
							try
							{
								startValue39 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException37)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException37);
							}
							DataItem<float> pLifeTime = new DataItem<float>("LifeTime", startValue39);
							itemAction.pLifeTime = pLifeTime;
							break;
						}
						case "CollisionRadius":
						{
							float startValue37;
							try
							{
								startValue37 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException35)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException35);
							}
							DataItem<float> pCollisionRadius = new DataItem<float>("CollisionRadius", startValue37);
							itemAction.pCollisionRadius = pCollisionRadius;
							break;
						}
						case "ProjectileInitialVelocity":
						{
							int startValue35;
							try
							{
								startValue35 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException33)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException33);
							}
							DataItem<int> pProjectileInitialVelocity = new DataItem<int>("ProjectileInitialVelocity", startValue35);
							itemAction.pProjectileInitialVelocity = pProjectileInitialVelocity;
							break;
						}
						case "Fertileblock":
						{
							string startValue33;
							try
							{
								startValue33 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException31)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException31);
							}
							DataItem<string> pFertileblock = new DataItem<string>("Fertileblock", startValue33);
							itemAction.pFertileblock = pFertileblock;
							break;
						}
						case "Adjacentblock":
						{
							string startValue31;
							try
							{
								startValue31 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException29)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException29);
							}
							DataItem<string> pAdjacentblock = new DataItem<string>("Adjacentblock", startValue31);
							itemAction.pAdjacentblock = pAdjacentblock;
							break;
						}
						case "RepairAmount":
						{
							int startValue29;
							try
							{
								startValue29 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException27)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException27);
							}
							DataItem<int> pRepairAmount = new DataItem<int>("RepairAmount", startValue29);
							itemAction.pRepairAmount = pRepairAmount;
							break;
						}
						case "UpgradeHitOffset":
						{
							int startValue27;
							try
							{
								startValue27 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException25)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException25);
							}
							DataItem<int> pUpgradeHitOffset = new DataItem<int>("UpgradeHitOffset", startValue27);
							itemAction.pUpgradeHitOffset = pUpgradeHitOffset;
							break;
						}
						case "AllowedUpgradeItems":
						{
							string startValue25;
							try
							{
								startValue25 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException23)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException23);
							}
							DataItem<string> pAllowedUpgradeItems = new DataItem<string>("AllowedUpgradeItems", startValue25);
							itemAction.pAllowedUpgradeItems = pAllowedUpgradeItems;
							break;
						}
						case "RestrictedUpgradeItems":
						{
							string startValue23;
							try
							{
								startValue23 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException21)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException21);
							}
							DataItem<string> pRestrictedUpgradeItems = new DataItem<string>("RestrictedUpgradeItems", startValue23);
							itemAction.pRestrictedUpgradeItems = pRestrictedUpgradeItems;
							break;
						}
						case "UpgradeActionSound":
						{
							string startValue21;
							try
							{
								startValue21 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException19)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException19);
							}
							DataItem<string> pUpgradeActionSound = new DataItem<string>("UpgradeActionSound", startValue21);
							itemAction.pUpgradeActionSound = pUpgradeActionSound;
							break;
						}
						case "RepairActionSound":
						{
							string startValue19;
							try
							{
								startValue19 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException17)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException17);
							}
							DataItem<string> pRepairActionSound = new DataItem<string>("RepairActionSound", startValue19);
							itemAction.pRepairActionSound = pRepairActionSound;
							break;
						}
						case "ReferenceItem":
						{
							string startValue17;
							try
							{
								startValue17 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException15)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException15);
							}
							DataItem<string> pReferenceItem = new DataItem<string>("ReferenceItem", startValue17);
							itemAction.pReferenceItem = pReferenceItem;
							break;
						}
						case "Mesh":
						{
							string startValue15;
							try
							{
								startValue15 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException13)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException13);
							}
							DataItem<string> pMesh = new DataItem<string>("Mesh", startValue15);
							itemAction.pMesh = pMesh;
							break;
						}
						case "ActionIdx":
						{
							int startValue13;
							try
							{
								startValue13 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException11)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException11);
							}
							DataItem<int> pActionIdx = new DataItem<int>("ActionIdx", startValue13);
							itemAction.pActionIdx = pActionIdx;
							break;
						}
						case "Title":
						{
							string startValue11;
							try
							{
								startValue11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException9)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException9);
							}
							DataItem<string> pTitle = new DataItem<string>("Title", startValue11);
							itemAction.pTitle = pTitle;
							break;
						}
						case "Description":
						{
							string startValue9;
							try
							{
								startValue9 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException7)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException7);
							}
							DataItem<string> pDescription = new DataItem<string>("Description", startValue9);
							itemAction.pDescription = pDescription;
							break;
						}
						case "RecipesToLearn":
						{
							string startValue7;
							try
							{
								startValue7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException5)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException5);
							}
							DataItem<string> pRecipesToLearn = new DataItem<string>("RecipesToLearn", startValue7);
							itemAction.pRecipesToLearn = pRecipesToLearn;
							break;
						}
						case "InstantiateOnLoad":
						{
							string startValue5;
							try
							{
								startValue5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<string> pInstantiateOnLoad = new DataItem<string>("InstantiateOnLoad", startValue5);
							itemAction.pInstantiateOnLoad = pInstantiateOnLoad;
							break;
						}
						case "SoundDraw":
						{
							string startValue3;
							try
							{
								startValue3 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<string> pSoundDraw = new DataItem<string>("SoundDraw", startValue3);
							itemAction.pSoundDraw = pSoundDraw;
							break;
						}
						case "DamageBonus":
						{
							DamageBonusData startValue2 = DamageBonusData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<DamageBonusData> pDamageBonus = new DataItem<DamageBonusData>("DamageBonus", startValue2);
							itemAction.pDamageBonus = pDamageBonus;
							break;
						}
						case "Explosion":
						{
							ExplosionData startValue = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<ExplosionData> pExplosion = new DataItem<ExplosionData>("Explosion", startValue);
							itemAction.pExplosion = pExplosion;
							break;
						}
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						dictionary[positionXmlElement.Name]++;
						break;
					}
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing ItemAction", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing ItemAction", ((IXmlLineInfo)childNode).LineNumber);
				case XmlNodeType.Comment:
					break;
				}
			}
			foreach (KeyValuePair<string, Range<int>> item in knownAttributesMultiplicity)
			{
				int num = (dictionary.ContainsKey(item.Key) ? dictionary[item.Key] : 0);
				if ((item.Value.hasMin && num < item.Value.min) || (item.Value.hasMax && num > item.Value.max))
				{
					throw new IncorrectAttributeOccurrenceException("Element has incorrect number of \"" + item.Key + "\" attribute instances, found " + num + ", expected " + item.Value.ToString(), _elem.LineNumber);
				}
			}
			return itemAction;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundRepeat;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundReload;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundWarning;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pStaminaUsage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pUseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname1;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname2;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname3;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname4;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname5;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname6;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname7;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname8;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFocusedBlockname9;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pChangeItemTo;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pChangeBlockTo;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pDoBlockAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainFood;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainSickness;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGainWellness;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBuffChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pDebuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pCreateItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pConditionRaycastBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pGainGas;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pConsume;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBlockname;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pThrowStrengthDefault;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pThrowStrengthMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pMaxStrainTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pMagazineSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pMagazineItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pReloadTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBulletIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pRaysPerShot;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pRaysSpread;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pSphere;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pCrosshairMinDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pCrosshairMaxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pDamageEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pDamageBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pParticlesMuzzleFire;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pParticlesMuzzleSmoke;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pBlockRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pAutoFire;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pHordeMeterRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pHordeMeterDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pHitmaskOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pSingleMagazineUsage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBulletMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pInfiniteAmmo;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pZoomMaxOut;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pZoomMaxIn;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pZoomOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pFlyTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pLifeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pCollisionRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pProjectileInitialVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFertileblock;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pAdjacentblock;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pRepairAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pUpgradeHitOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pAllowedUpgradeItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pRestrictedUpgradeItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pUpgradeActionSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pRepairActionSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pReferenceItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pActionIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pRecipesToLearn;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pInstantiateOnLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundDraw;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<DamageBonusData> pDamageBonus;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ExplosionData> pExplosion;

	public DataItem<float> Delay
	{
		get
		{
			return pDelay;
		}
		set
		{
			pDelay = value;
		}
	}

	public DataItem<float> Range
	{
		get
		{
			return pRange;
		}
		set
		{
			pRange = value;
		}
	}

	public DataItem<string> SoundStart
	{
		get
		{
			return pSoundStart;
		}
		set
		{
			pSoundStart = value;
		}
	}

	public DataItem<string> SoundRepeat
	{
		get
		{
			return pSoundRepeat;
		}
		set
		{
			pSoundRepeat = value;
		}
	}

	public DataItem<string> SoundEnd
	{
		get
		{
			return pSoundEnd;
		}
		set
		{
			pSoundEnd = value;
		}
	}

	public DataItem<string> SoundEmpty
	{
		get
		{
			return pSoundEmpty;
		}
		set
		{
			pSoundEmpty = value;
		}
	}

	public DataItem<string> SoundReload
	{
		get
		{
			return pSoundReload;
		}
		set
		{
			pSoundReload = value;
		}
	}

	public DataItem<string> SoundWarning
	{
		get
		{
			return pSoundWarning;
		}
		set
		{
			pSoundWarning = value;
		}
	}

	public DataItem<string> StaminaUsage
	{
		get
		{
			return pStaminaUsage;
		}
		set
		{
			pStaminaUsage = value;
		}
	}

	public DataItem<string> UseTime
	{
		get
		{
			return pUseTime;
		}
		set
		{
			pUseTime = value;
		}
	}

	public DataItem<string> FocusedBlockname1
	{
		get
		{
			return pFocusedBlockname1;
		}
		set
		{
			pFocusedBlockname1 = value;
		}
	}

	public DataItem<string> FocusedBlockname2
	{
		get
		{
			return pFocusedBlockname2;
		}
		set
		{
			pFocusedBlockname2 = value;
		}
	}

	public DataItem<string> FocusedBlockname3
	{
		get
		{
			return pFocusedBlockname3;
		}
		set
		{
			pFocusedBlockname3 = value;
		}
	}

	public DataItem<string> FocusedBlockname4
	{
		get
		{
			return pFocusedBlockname4;
		}
		set
		{
			pFocusedBlockname4 = value;
		}
	}

	public DataItem<string> FocusedBlockname5
	{
		get
		{
			return pFocusedBlockname5;
		}
		set
		{
			pFocusedBlockname5 = value;
		}
	}

	public DataItem<string> FocusedBlockname6
	{
		get
		{
			return pFocusedBlockname6;
		}
		set
		{
			pFocusedBlockname6 = value;
		}
	}

	public DataItem<string> FocusedBlockname7
	{
		get
		{
			return pFocusedBlockname7;
		}
		set
		{
			pFocusedBlockname7 = value;
		}
	}

	public DataItem<string> FocusedBlockname8
	{
		get
		{
			return pFocusedBlockname8;
		}
		set
		{
			pFocusedBlockname8 = value;
		}
	}

	public DataItem<string> FocusedBlockname9
	{
		get
		{
			return pFocusedBlockname9;
		}
		set
		{
			pFocusedBlockname9 = value;
		}
	}

	public DataItem<string> ChangeItemTo
	{
		get
		{
			return pChangeItemTo;
		}
		set
		{
			pChangeItemTo = value;
		}
	}

	public DataItem<string> ChangeBlockTo
	{
		get
		{
			return pChangeBlockTo;
		}
		set
		{
			pChangeBlockTo = value;
		}
	}

	public DataItem<string> DoBlockAction
	{
		get
		{
			return pDoBlockAction;
		}
		set
		{
			pDoBlockAction = value;
		}
	}

	public DataItem<float> GainHealth
	{
		get
		{
			return pGainHealth;
		}
		set
		{
			pGainHealth = value;
		}
	}

	public DataItem<float> GainFood
	{
		get
		{
			return pGainFood;
		}
		set
		{
			pGainFood = value;
		}
	}

	public DataItem<float> GainWater
	{
		get
		{
			return pGainWater;
		}
		set
		{
			pGainWater = value;
		}
	}

	public DataItem<float> GainStamina
	{
		get
		{
			return pGainStamina;
		}
		set
		{
			pGainStamina = value;
		}
	}

	public DataItem<float> GainSickness
	{
		get
		{
			return pGainSickness;
		}
		set
		{
			pGainSickness = value;
		}
	}

	public DataItem<float> GainWellness
	{
		get
		{
			return pGainWellness;
		}
		set
		{
			pGainWellness = value;
		}
	}

	public DataItem<string> Buff
	{
		get
		{
			return pBuff;
		}
		set
		{
			pBuff = value;
		}
	}

	public DataItem<string> BuffChance
	{
		get
		{
			return pBuffChance;
		}
		set
		{
			pBuffChance = value;
		}
	}

	public DataItem<string> Debuff
	{
		get
		{
			return pDebuff;
		}
		set
		{
			pDebuff = value;
		}
	}

	public DataItem<string> CreateItem
	{
		get
		{
			return pCreateItem;
		}
		set
		{
			pCreateItem = value;
		}
	}

	public DataItem<int> ConditionRaycastBlock
	{
		get
		{
			return pConditionRaycastBlock;
		}
		set
		{
			pConditionRaycastBlock = value;
		}
	}

	public DataItem<int> GainGas
	{
		get
		{
			return pGainGas;
		}
		set
		{
			pGainGas = value;
		}
	}

	public DataItem<bool> Consume
	{
		get
		{
			return pConsume;
		}
		set
		{
			pConsume = value;
		}
	}

	public DataItem<string> Blockname
	{
		get
		{
			return pBlockname;
		}
		set
		{
			pBlockname = value;
		}
	}

	public DataItem<float> ThrowStrengthDefault
	{
		get
		{
			return pThrowStrengthDefault;
		}
		set
		{
			pThrowStrengthDefault = value;
		}
	}

	public DataItem<float> ThrowStrengthMax
	{
		get
		{
			return pThrowStrengthMax;
		}
		set
		{
			pThrowStrengthMax = value;
		}
	}

	public DataItem<float> MaxStrainTime
	{
		get
		{
			return pMaxStrainTime;
		}
		set
		{
			pMaxStrainTime = value;
		}
	}

	public DataItem<int> MagazineSize
	{
		get
		{
			return pMagazineSize;
		}
		set
		{
			pMagazineSize = value;
		}
	}

	public DataItem<string> MagazineItem
	{
		get
		{
			return pMagazineItem;
		}
		set
		{
			pMagazineItem = value;
		}
	}

	public DataItem<float> ReloadTime
	{
		get
		{
			return pReloadTime;
		}
		set
		{
			pReloadTime = value;
		}
	}

	public DataItem<string> BulletIcon
	{
		get
		{
			return pBulletIcon;
		}
		set
		{
			pBulletIcon = value;
		}
	}

	public DataItem<int> RaysPerShot
	{
		get
		{
			return pRaysPerShot;
		}
		set
		{
			pRaysPerShot = value;
		}
	}

	public DataItem<float> RaysSpread
	{
		get
		{
			return pRaysSpread;
		}
		set
		{
			pRaysSpread = value;
		}
	}

	public DataItem<float> Sphere
	{
		get
		{
			return pSphere;
		}
		set
		{
			pSphere = value;
		}
	}

	public DataItem<int> CrosshairMinDistance
	{
		get
		{
			return pCrosshairMinDistance;
		}
		set
		{
			pCrosshairMinDistance = value;
		}
	}

	public DataItem<int> CrosshairMaxDistance
	{
		get
		{
			return pCrosshairMaxDistance;
		}
		set
		{
			pCrosshairMaxDistance = value;
		}
	}

	public DataItem<int> DamageEntity
	{
		get
		{
			return pDamageEntity;
		}
		set
		{
			pDamageEntity = value;
		}
	}

	public DataItem<float> DamageBlock
	{
		get
		{
			return pDamageBlock;
		}
		set
		{
			pDamageBlock = value;
		}
	}

	public DataItem<string> ParticlesMuzzleFire
	{
		get
		{
			return pParticlesMuzzleFire;
		}
		set
		{
			pParticlesMuzzleFire = value;
		}
	}

	public DataItem<string> ParticlesMuzzleSmoke
	{
		get
		{
			return pParticlesMuzzleSmoke;
		}
		set
		{
			pParticlesMuzzleSmoke = value;
		}
	}

	public DataItem<float> BlockRange
	{
		get
		{
			return pBlockRange;
		}
		set
		{
			pBlockRange = value;
		}
	}

	public DataItem<bool> AutoFire
	{
		get
		{
			return pAutoFire;
		}
		set
		{
			pAutoFire = value;
		}
	}

	public DataItem<float> HordeMeterRate
	{
		get
		{
			return pHordeMeterRate;
		}
		set
		{
			pHordeMeterRate = value;
		}
	}

	public DataItem<float> HordeMeterDistance
	{
		get
		{
			return pHordeMeterDistance;
		}
		set
		{
			pHordeMeterDistance = value;
		}
	}

	public DataItem<string> HitmaskOverride
	{
		get
		{
			return pHitmaskOverride;
		}
		set
		{
			pHitmaskOverride = value;
		}
	}

	public DataItem<bool> SingleMagazineUsage
	{
		get
		{
			return pSingleMagazineUsage;
		}
		set
		{
			pSingleMagazineUsage = value;
		}
	}

	public DataItem<string> BulletMaterial
	{
		get
		{
			return pBulletMaterial;
		}
		set
		{
			pBulletMaterial = value;
		}
	}

	public DataItem<bool> InfiniteAmmo
	{
		get
		{
			return pInfiniteAmmo;
		}
		set
		{
			pInfiniteAmmo = value;
		}
	}

	public DataItem<float> ZoomMaxOut
	{
		get
		{
			return pZoomMaxOut;
		}
		set
		{
			pZoomMaxOut = value;
		}
	}

	public DataItem<float> ZoomMaxIn
	{
		get
		{
			return pZoomMaxIn;
		}
		set
		{
			pZoomMaxIn = value;
		}
	}

	public DataItem<string> ZoomOverlay
	{
		get
		{
			return pZoomOverlay;
		}
		set
		{
			pZoomOverlay = value;
		}
	}

	public DataItem<int> Velocity
	{
		get
		{
			return pVelocity;
		}
		set
		{
			pVelocity = value;
		}
	}

	public DataItem<float> FlyTime
	{
		get
		{
			return pFlyTime;
		}
		set
		{
			pFlyTime = value;
		}
	}

	public DataItem<float> LifeTime
	{
		get
		{
			return pLifeTime;
		}
		set
		{
			pLifeTime = value;
		}
	}

	public DataItem<float> CollisionRadius
	{
		get
		{
			return pCollisionRadius;
		}
		set
		{
			pCollisionRadius = value;
		}
	}

	public DataItem<int> ProjectileInitialVelocity
	{
		get
		{
			return pProjectileInitialVelocity;
		}
		set
		{
			pProjectileInitialVelocity = value;
		}
	}

	public DataItem<string> Fertileblock
	{
		get
		{
			return pFertileblock;
		}
		set
		{
			pFertileblock = value;
		}
	}

	public DataItem<string> Adjacentblock
	{
		get
		{
			return pAdjacentblock;
		}
		set
		{
			pAdjacentblock = value;
		}
	}

	public DataItem<int> RepairAmount
	{
		get
		{
			return pRepairAmount;
		}
		set
		{
			pRepairAmount = value;
		}
	}

	public DataItem<int> UpgradeHitOffset
	{
		get
		{
			return pUpgradeHitOffset;
		}
		set
		{
			pUpgradeHitOffset = value;
		}
	}

	public DataItem<string> AllowedUpgradeItems
	{
		get
		{
			return pAllowedUpgradeItems;
		}
		set
		{
			pAllowedUpgradeItems = value;
		}
	}

	public DataItem<string> RestrictedUpgradeItems
	{
		get
		{
			return pRestrictedUpgradeItems;
		}
		set
		{
			pRestrictedUpgradeItems = value;
		}
	}

	public DataItem<string> UpgradeActionSound
	{
		get
		{
			return pUpgradeActionSound;
		}
		set
		{
			pUpgradeActionSound = value;
		}
	}

	public DataItem<string> RepairActionSound
	{
		get
		{
			return pRepairActionSound;
		}
		set
		{
			pRepairActionSound = value;
		}
	}

	public DataItem<string> ReferenceItem
	{
		get
		{
			return pReferenceItem;
		}
		set
		{
			pReferenceItem = value;
		}
	}

	public DataItem<string> Mesh
	{
		get
		{
			return pMesh;
		}
		set
		{
			pMesh = value;
		}
	}

	public DataItem<int> ActionIdx
	{
		get
		{
			return pActionIdx;
		}
		set
		{
			pActionIdx = value;
		}
	}

	public DataItem<string> Title
	{
		get
		{
			return pTitle;
		}
		set
		{
			pTitle = value;
		}
	}

	public DataItem<string> Description
	{
		get
		{
			return pDescription;
		}
		set
		{
			pDescription = value;
		}
	}

	public DataItem<string> RecipesToLearn
	{
		get
		{
			return pRecipesToLearn;
		}
		set
		{
			pRecipesToLearn = value;
		}
	}

	public DataItem<string> InstantiateOnLoad
	{
		get
		{
			return pInstantiateOnLoad;
		}
		set
		{
			pInstantiateOnLoad = value;
		}
	}

	public DataItem<string> SoundDraw
	{
		get
		{
			return pSoundDraw;
		}
		set
		{
			pSoundDraw = value;
		}
	}

	public DataItem<DamageBonusData> DamageBonus
	{
		get
		{
			return pDamageBonus;
		}
		set
		{
			pDamageBonus = value;
		}
	}

	public DataItem<ExplosionData> Explosion
	{
		get
		{
			return pExplosion;
		}
		set
		{
			pExplosion = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		List<IDataItem> list = new List<IDataItem>();
		if (_recursive && pDamageBonus != null)
		{
			list.AddRange(pDamageBonus.Value.GetDisplayValues());
		}
		if (_recursive && pExplosion != null)
		{
			list.AddRange(pExplosion.Value.GetDisplayValues());
		}
		return list;
	}
}
