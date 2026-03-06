using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

public class PreviewData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Zoom",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Pos",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Rot",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			}
		};

		public static PreviewData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "PreviewData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			PreviewData previewData = (PreviewData)Activator.CreateInstance(type);
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
						case "Zoom":
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
							DataItem<int> pZoom = new DataItem<int>("Zoom", startValue2);
							previewData.pZoom = pZoom;
							break;
						}
						case "Pos":
						{
							Vector2 startValue3;
							try
							{
								startValue3 = Vector2Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<Vector2> pPos = new DataItem<Vector2>("Pos", startValue3);
							previewData.pPos = pPos;
							break;
						}
						case "Rot":
						{
							Vector3 startValue;
							try
							{
								startValue = Vector3Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<Vector3> pRot = new DataItem<Vector3>("Rot", startValue);
							previewData.pRot = pRot;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Preview", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Preview", ((IXmlLineInfo)childNode).LineNumber);
				case XmlNodeType.Comment:
					break;
				}
			}
			if (!dictionary.ContainsKey("Zoom"))
			{
				int startValue4;
				try
				{
					startValue4 = intParser.Parse("0");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"0\" for attribute \"Zoom\" could not be parsed", -1);
				}
				DataItem<int> pZoom2 = new DataItem<int>("Zoom", startValue4);
				previewData.pZoom = pZoom2;
				dictionary["Zoom"] = 1;
			}
			if (!dictionary.ContainsKey("Pos"))
			{
				Vector2 startValue5;
				try
				{
					startValue5 = Vector2Parser.Parse("0,0");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"0,0\" for attribute \"Pos\" could not be parsed", -1);
				}
				DataItem<Vector2> pPos2 = new DataItem<Vector2>("Pos", startValue5);
				previewData.pPos = pPos2;
				dictionary["Pos"] = 1;
			}
			if (!dictionary.ContainsKey("Rot"))
			{
				Vector3 startValue6;
				try
				{
					startValue6 = Vector3Parser.Parse("0,0,0");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"0,0,0\" for attribute \"Rot\" could not be parsed", -1);
				}
				DataItem<Vector3> pRot2 = new DataItem<Vector3>("Rot", startValue6);
				previewData.pRot = pRot2;
				dictionary["Rot"] = 1;
			}
			foreach (KeyValuePair<string, Range<int>> item in knownAttributesMultiplicity)
			{
				int num = (dictionary.ContainsKey(item.Key) ? dictionary[item.Key] : 0);
				if ((item.Value.hasMin && num < item.Value.min) || (item.Value.hasMax && num > item.Value.max))
				{
					throw new IncorrectAttributeOccurrenceException("Element has incorrect number of \"" + item.Key + "\" attribute instances, found " + num + ", expected " + item.Value.ToString(), _elem.LineNumber);
				}
			}
			return previewData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pZoom;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<Vector2> pPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<Vector3> pRot;

	public DataItem<int> Zoom
	{
		get
		{
			return pZoom;
		}
		set
		{
			pZoom = value;
		}
	}

	public DataItem<Vector2> Pos
	{
		get
		{
			return pPos;
		}
		set
		{
			pPos = value;
		}
	}

	public DataItem<Vector3> Rot
	{
		get
		{
			return pRot;
		}
		set
		{
			pRot = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
