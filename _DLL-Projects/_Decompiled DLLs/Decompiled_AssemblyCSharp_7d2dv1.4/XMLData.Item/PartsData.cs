using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;

namespace XMLData.Item;

[Preserve]
public class PartsData : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Stock",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Receiver",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Pump",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Barrel",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static PartsData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "PartsData");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			PartsData partsData = (PartsData)Activator.CreateInstance(type);
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
						case "Stock":
						{
							ItemClass startValue4;
							try
							{
								startValue4 = null;
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<ItemClass> dataItem4 = new DataItem<ItemClass>("Stock", startValue4);
							_updateLater.Add(positionXmlElement, dataItem4);
							partsData.pStock = dataItem4;
							break;
						}
						case "Receiver":
						{
							ItemClass startValue2;
							try
							{
								startValue2 = null;
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<ItemClass> dataItem2 = new DataItem<ItemClass>("Receiver", startValue2);
							_updateLater.Add(positionXmlElement, dataItem2);
							partsData.pReceiver = dataItem2;
							break;
						}
						case "Pump":
						{
							ItemClass startValue3;
							try
							{
								startValue3 = null;
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<ItemClass> dataItem3 = new DataItem<ItemClass>("Pump", startValue3);
							_updateLater.Add(positionXmlElement, dataItem3);
							partsData.pPump = dataItem3;
							break;
						}
						case "Barrel":
						{
							ItemClass startValue;
							try
							{
								startValue = null;
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<ItemClass> dataItem = new DataItem<ItemClass>("Barrel", startValue);
							_updateLater.Add(positionXmlElement, dataItem);
							partsData.pBarrel = dataItem;
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
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Parts", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Parts", ((IXmlLineInfo)childNode).LineNumber);
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
			return partsData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ItemClass> pStock;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ItemClass> pReceiver;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ItemClass> pPump;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ItemClass> pBarrel;

	public DataItem<ItemClass> Stock
	{
		get
		{
			return pStock;
		}
		set
		{
			pStock = value;
		}
	}

	public DataItem<ItemClass> Receiver
	{
		get
		{
			return pReceiver;
		}
		set
		{
			pReceiver = value;
		}
	}

	public DataItem<ItemClass> Pump
	{
		get
		{
			return pPump;
		}
		set
		{
			pPump = value;
		}
	}

	public DataItem<ItemClass> Barrel
	{
		get
		{
			return pBarrel;
		}
		set
		{
			pBarrel = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		return new List<IDataItem>();
	}
}
