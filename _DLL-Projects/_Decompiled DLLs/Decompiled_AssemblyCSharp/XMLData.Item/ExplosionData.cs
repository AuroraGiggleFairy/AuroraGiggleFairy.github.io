using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class ExplosionData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"BlockDamage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"EntityDamage",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ParticleIndex",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RadiusBlocks",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RadiusEntities",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static ExplosionData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ExplosionData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			ExplosionData explosionData = (ExplosionData)Activator.CreateInstance(type);
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
						case "BlockDamage":
						{
							int startValue2;
							try
							{
								startValue2 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<int> pBlockDamage = new DataItem<int>("BlockDamage", startValue2);
							explosionData.pBlockDamage = pBlockDamage;
							break;
						}
						case "EntityDamage":
						{
							int startValue4;
							try
							{
								startValue4 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<int> pEntityDamage = new DataItem<int>("EntityDamage", startValue4);
							explosionData.pEntityDamage = pEntityDamage;
							break;
						}
						case "ParticleIndex":
						{
							int startValue5;
							try
							{
								startValue5 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException5)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException5);
							}
							DataItem<int> pParticleIndex = new DataItem<int>("ParticleIndex", startValue5);
							explosionData.pParticleIndex = pParticleIndex;
							break;
						}
						case "RadiusBlocks":
						{
							int startValue3;
							try
							{
								startValue3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<int> pRadiusBlocks = new DataItem<int>("RadiusBlocks", startValue3);
							explosionData.pRadiusBlocks = pRadiusBlocks;
							break;
						}
						case "RadiusEntities":
						{
							int startValue;
							try
							{
								startValue = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<int> pRadiusEntities = new DataItem<int>("RadiusEntities", startValue);
							explosionData.pRadiusEntities = pRadiusEntities;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Explosion", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Explosion", ((IXmlLineInfo)childNode).LineNumber);
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
			return explosionData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pBlockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pEntityDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pParticleIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pRadiusBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pRadiusEntities;

	public DataItem<int> BlockDamage
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

	public DataItem<int> EntityDamage
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

	public DataItem<int> ParticleIndex
	{
		get
		{
			return pParticleIndex;
		}
		set
		{
			pParticleIndex = value;
		}
	}

	public DataItem<int> RadiusBlocks
	{
		get
		{
			return pRadiusBlocks;
		}
		set
		{
			pRadiusBlocks = value;
		}
	}

	public DataItem<int> RadiusEntities
	{
		get
		{
			return pRadiusEntities;
		}
		set
		{
			pRadiusEntities = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
