using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class ArmorData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Melee",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Bullet",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Puncture",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Blunt",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Explosive",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static ArmorData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ArmorData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			ArmorData armorData = (ArmorData)Activator.CreateInstance(type);
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
						case "Melee":
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
							DataItem<float> pMelee = new DataItem<float>("Melee", startValue2);
							armorData.pMelee = pMelee;
							break;
						}
						case "Bullet":
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
							DataItem<float> pBullet = new DataItem<float>("Bullet", startValue4);
							armorData.pBullet = pBullet;
							break;
						}
						case "Puncture":
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
							DataItem<float> pPuncture = new DataItem<float>("Puncture", startValue5);
							armorData.pPuncture = pPuncture;
							break;
						}
						case "Blunt":
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
							DataItem<float> pBlunt = new DataItem<float>("Blunt", startValue3);
							armorData.pBlunt = pBlunt;
							break;
						}
						case "Explosive":
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
							DataItem<float> pExplosive = new DataItem<float>("Explosive", startValue);
							armorData.pExplosive = pExplosive;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Armor", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Armor", ((IXmlLineInfo)childNode).LineNumber);
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
			return armorData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pMelee;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pBullet;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pPuncture;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pBlunt;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pExplosive;

	public DataItem<float> Melee
	{
		get
		{
			return pMelee;
		}
		set
		{
			pMelee = value;
		}
	}

	public DataItem<float> Bullet
	{
		get
		{
			return pBullet;
		}
		set
		{
			pBullet = value;
		}
	}

	public DataItem<float> Puncture
	{
		get
		{
			return pPuncture;
		}
		set
		{
			pPuncture = value;
		}
	}

	public DataItem<float> Blunt
	{
		get
		{
			return pBlunt;
		}
		set
		{
			pBlunt = value;
		}
	}

	public DataItem<float> Explosive
	{
		get
		{
			return pExplosive;
		}
		set
		{
			pExplosive = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
