using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class AttributesData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"EntityDamage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"BlockDamage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Accuracy",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"FalloffRange",
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
				"DegradationRate",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static AttributesData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "AttributesData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			AttributesData attributesData = (AttributesData)Activator.CreateInstance(type);
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
						case "EntityDamage":
						{
							string startValue8;
							try
							{
								startValue8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException8)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException8);
							}
							DataItem<string> pEntityDamage = new DataItem<string>("EntityDamage", startValue8);
							attributesData.pEntityDamage = pEntityDamage;
							break;
						}
						case "BlockDamage":
						{
							string startValue4;
							try
							{
								startValue4 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<string> pBlockDamage = new DataItem<string>("BlockDamage", startValue4);
							attributesData.pBlockDamage = pBlockDamage;
							break;
						}
						case "Accuracy":
						{
							string startValue6;
							try
							{
								startValue6 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException6)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException6);
							}
							DataItem<string> pAccuracy = new DataItem<string>("Accuracy", startValue6);
							attributesData.pAccuracy = pAccuracy;
							break;
						}
						case "FalloffRange":
						{
							string startValue2;
							try
							{
								startValue2 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<string> pFalloffRange = new DataItem<string>("FalloffRange", startValue2);
							attributesData.pFalloffRange = pFalloffRange;
							break;
						}
						case "GainHealth":
						{
							string startValue7;
							try
							{
								startValue7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException7)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException7);
							}
							DataItem<string> pGainHealth = new DataItem<string>("GainHealth", startValue7);
							attributesData.pGainHealth = pGainHealth;
							break;
						}
						case "GainFood":
						{
							string startValue5;
							try
							{
								startValue5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException5)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException5);
							}
							DataItem<string> pGainFood = new DataItem<string>("GainFood", startValue5);
							attributesData.pGainFood = pGainFood;
							break;
						}
						case "GainWater":
						{
							string startValue3;
							try
							{
								startValue3 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<string> pGainWater = new DataItem<string>("GainWater", startValue3);
							attributesData.pGainWater = pGainWater;
							break;
						}
						case "DegradationRate":
						{
							string startValue;
							try
							{
								startValue = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<string> pDegradationRate = new DataItem<string>("DegradationRate", startValue);
							attributesData.pDegradationRate = pDegradationRate;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Attributes", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Attributes", ((IXmlLineInfo)childNode).LineNumber);
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
			return attributesData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pEntityDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pBlockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pAccuracy;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pFalloffRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pGainHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pGainFood;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pGainWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pDegradationRate;

	public DataItem<string> EntityDamage
	{
		get
		{
			return pEntityDamage;
		}
		set
		{
			pEntityDamage = value;
		}
	}

	public DataItem<string> BlockDamage
	{
		get
		{
			return pBlockDamage;
		}
		set
		{
			pBlockDamage = value;
		}
	}

	public DataItem<string> Accuracy
	{
		get
		{
			return pAccuracy;
		}
		set
		{
			pAccuracy = value;
		}
	}

	public DataItem<string> FalloffRange
	{
		get
		{
			return pFalloffRange;
		}
		set
		{
			pFalloffRange = value;
		}
	}

	public DataItem<string> GainHealth
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

	public DataItem<string> GainFood
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

	public DataItem<string> GainWater
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

	public DataItem<string> DegradationRate
	{
		get
		{
			return pDegradationRate;
		}
		set
		{
			pDegradationRate = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
