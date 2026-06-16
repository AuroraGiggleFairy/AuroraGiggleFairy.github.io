using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class DamageBonusData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Head",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Glass",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Stone",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Cloth",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Concrete",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Boulder",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Metal",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Wood",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Earth",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Snow",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Plants",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Leaves",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static DamageBonusData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "DamageBonusData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			DamageBonusData damageBonusData = (DamageBonusData)Activator.CreateInstance(type);
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
						case "Head":
						{
							float startValue8;
							try
							{
								startValue8 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException8)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException8);
							}
							DataItem<float> pHead = new DataItem<float>("Head", startValue8);
							damageBonusData.pHead = pHead;
							break;
						}
						case "Glass":
						{
							float startValue12;
							try
							{
								startValue12 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException12)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException12);
							}
							DataItem<float> pGlass = new DataItem<float>("Glass", startValue12);
							damageBonusData.pGlass = pGlass;
							break;
						}
						case "Stone":
						{
							float startValue4;
							try
							{
								startValue4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<float> pStone = new DataItem<float>("Stone", startValue4);
							damageBonusData.pStone = pStone;
							break;
						}
						case "Cloth":
						{
							float startValue10;
							try
							{
								startValue10 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException10)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException10);
							}
							DataItem<float> pCloth = new DataItem<float>("Cloth", startValue10);
							damageBonusData.pCloth = pCloth;
							break;
						}
						case "Concrete":
						{
							float startValue6;
							try
							{
								startValue6 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException6)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException6);
							}
							DataItem<float> pConcrete = new DataItem<float>("Concrete", startValue6);
							damageBonusData.pConcrete = pConcrete;
							break;
						}
						case "Boulder":
						{
							float startValue2;
							try
							{
								startValue2 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<float> pBoulder = new DataItem<float>("Boulder", startValue2);
							damageBonusData.pBoulder = pBoulder;
							break;
						}
						case "Metal":
						{
							float startValue11;
							try
							{
								startValue11 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException11)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException11);
							}
							DataItem<float> pMetal = new DataItem<float>("Metal", startValue11);
							damageBonusData.pMetal = pMetal;
							break;
						}
						case "Wood":
						{
							float startValue9;
							try
							{
								startValue9 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException9)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException9);
							}
							DataItem<float> pWood = new DataItem<float>("Wood", startValue9);
							damageBonusData.pWood = pWood;
							break;
						}
						case "Earth":
						{
							float startValue7;
							try
							{
								startValue7 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException7)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException7);
							}
							DataItem<float> pEarth = new DataItem<float>("Earth", startValue7);
							damageBonusData.pEarth = pEarth;
							break;
						}
						case "Snow":
						{
							float startValue5;
							try
							{
								startValue5 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException5)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException5);
							}
							DataItem<float> pSnow = new DataItem<float>("Snow", startValue5);
							damageBonusData.pSnow = pSnow;
							break;
						}
						case "Plants":
						{
							float startValue3;
							try
							{
								startValue3 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<float> pPlants = new DataItem<float>("Plants", startValue3);
							damageBonusData.pPlants = pPlants;
							break;
						}
						case "Leaves":
						{
							float startValue;
							try
							{
								startValue = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<float> pLeaves = new DataItem<float>("Leaves", startValue);
							damageBonusData.pLeaves = pLeaves;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing DamageBonus", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing DamageBonus", ((IXmlLineInfo)childNode).LineNumber);
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
			return damageBonusData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pHead;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pGlass;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pStone;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pCloth;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pConcrete;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pBoulder;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pMetal;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pWood;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pEarth;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pSnow;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pPlants;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pLeaves;

	public DataItem<float> Head
	{
		get
		{
			return pHead;
		}
		set
		{
			pHead = value;
		}
	}

	public DataItem<float> Glass
	{
		get
		{
			return pGlass;
		}
		set
		{
			pGlass = value;
		}
	}

	public DataItem<float> Stone
	{
		get
		{
			return pStone;
		}
		set
		{
			pStone = value;
		}
	}

	public DataItem<float> Cloth
	{
		get
		{
			return pCloth;
		}
		set
		{
			pCloth = value;
		}
	}

	public DataItem<float> Concrete
	{
		get
		{
			return pConcrete;
		}
		set
		{
			pConcrete = value;
		}
	}

	public DataItem<float> Boulder
	{
		get
		{
			return pBoulder;
		}
		set
		{
			pBoulder = value;
		}
	}

	public DataItem<float> Metal
	{
		get
		{
			return pMetal;
		}
		set
		{
			pMetal = value;
		}
	}

	public DataItem<float> Wood
	{
		get
		{
			return pWood;
		}
		set
		{
			pWood = value;
		}
	}

	public DataItem<float> Earth
	{
		get
		{
			return pEarth;
		}
		set
		{
			pEarth = value;
		}
	}

	public DataItem<float> Snow
	{
		get
		{
			return pSnow;
		}
		set
		{
			pSnow = value;
		}
	}

	public DataItem<float> Plants
	{
		get
		{
			return pPlants;
		}
		set
		{
			pPlants = value;
		}
	}

	public DataItem<float> Leaves
	{
		get
		{
			return pLeaves;
		}
		set
		{
			pLeaves = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
