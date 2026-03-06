using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

public class UMAData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Mesh",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"OverlayTints",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Overlay",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Layer",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UISlot",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ShowHair",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static UMAData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "UMAData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			UMAData uMAData = (UMAData)Activator.CreateInstance(type);
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
						case "Mesh":
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
							DataItem<string> pMesh = new DataItem<string>("Mesh", startValue4);
							uMAData.pMesh = pMesh;
							break;
						}
						case "OverlayTints":
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
							DataItem<string> pOverlayTints = new DataItem<string>("OverlayTints", startValue6);
							uMAData.pOverlayTints = pOverlayTints;
							break;
						}
						case "Overlay":
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
							DataItem<string> pOverlay = new DataItem<string>("Overlay", startValue2);
							uMAData.pOverlay = pOverlay;
							break;
						}
						case "Layer":
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
							DataItem<int> pLayer = new DataItem<int>("Layer", startValue5);
							uMAData.pLayer = pLayer;
							break;
						}
						case "UISlot":
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
							DataItem<string> pUISlot = new DataItem<string>("UISlot", startValue3);
							uMAData.pUISlot = pUISlot;
							break;
						}
						case "ShowHair":
						{
							bool startValue;
							try
							{
								startValue = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<bool> pShowHair = new DataItem<bool>("ShowHair", startValue);
							uMAData.pShowHair = pShowHair;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing UMA", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing UMA", ((IXmlLineInfo)childNode).LineNumber);
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
			return uMAData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pOverlayTints;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pUISlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pShowHair;

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

	public DataItem<string> OverlayTints
	{
		get
		{
			return pOverlayTints;
		}
		set
		{
			pOverlayTints = value;
		}
	}

	public DataItem<string> Overlay
	{
		get
		{
			return pOverlay;
		}
		set
		{
			pOverlay = value;
		}
	}

	public DataItem<int> Layer
	{
		get
		{
			return pLayer;
		}
		set
		{
			pLayer = value;
		}
	}

	public DataItem<string> UISlot
	{
		get
		{
			return pUISlot;
		}
		set
		{
			pUISlot = value;
		}
	}

	public DataItem<bool> ShowHair
	{
		get
		{
			return pShowHair;
		}
		set
		{
			pShowHair = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
