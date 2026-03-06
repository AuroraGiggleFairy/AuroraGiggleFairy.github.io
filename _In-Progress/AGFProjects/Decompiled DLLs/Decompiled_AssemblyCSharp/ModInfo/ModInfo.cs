using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace ModInfo;

public class ModInfo : IXMLData
{
	public static class Parser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Name",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Description",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Author",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Version",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Website",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static ModInfo Parse(XElement _elem, Dictionary<PositionXmlElement, DataItem<ModInfo>> _updateLater)
		{
			ModInfo modInfo = new ModInfo();
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (XElement item in _elem.Elements())
			{
				if (knownAttributesMultiplicity.ContainsKey(item.Name.LocalName))
				{
					switch (item.Name.LocalName)
					{
					case "Name":
						ParseFieldAttributeName(modInfo, dictionary, item);
						break;
					case "Description":
						ParseFieldAttributeDescription(modInfo, dictionary, item);
						break;
					case "Author":
						ParseFieldAttributeAuthor(modInfo, dictionary, item);
						break;
					case "Version":
						ParseFieldAttributeVersion(modInfo, dictionary, item);
						break;
					case "Website":
						ParseFieldAttributeWebsite(modInfo, dictionary, item);
						break;
					}
					continue;
				}
				throw new UnexpectedElementException("Unknown element \"" + item.Name?.ToString() + "\" found while parsing ModInfo", ((IXmlLineInfo)item).LineNumber);
			}
			foreach (KeyValuePair<string, Range<int>> item2 in knownAttributesMultiplicity)
			{
				int num = (dictionary.ContainsKey(item2.Key) ? dictionary[item2.Key] : 0);
				if ((item2.Value.hasMin && num < item2.Value.min) || (item2.Value.hasMax && num > item2.Value.max))
				{
					throw new IncorrectAttributeOccurrenceException("Element has incorrect number of \"" + item2.Key + "\" attribute instances, found " + num + ", expected " + item2.Value, ((IXmlLineInfo)_elem).LineNumber);
				}
			}
			return modInfo;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ParseFieldAttributeName(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
		{
			string text = null;
			if (_elem != null)
			{
				text = ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true);
			}
			string startValue;
			try
			{
				startValue = stringParser.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"Name\" value \"" + text + "\"", ((IXmlLineInfo)_elem)?.LineNumber ?? (-1), innerException);
			}
			DataItem<string> pName = new DataItem<string>("Name", startValue);
			_entry.pName = pName;
			if (_elem != null)
			{
				if (!_foundAttributes.ContainsKey("Name"))
				{
					_foundAttributes["Name"] = 0;
				}
				_foundAttributes["Name"]++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ParseFieldAttributeDescription(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
		{
			string text = null;
			if (_elem != null)
			{
				text = ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true);
			}
			string startValue;
			try
			{
				startValue = stringParser.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"Description\" value \"" + text + "\"", ((IXmlLineInfo)_elem)?.LineNumber ?? (-1), innerException);
			}
			DataItem<string> pDescription = new DataItem<string>("Description", startValue);
			_entry.pDescription = pDescription;
			if (_elem != null)
			{
				if (!_foundAttributes.ContainsKey("Description"))
				{
					_foundAttributes["Description"] = 0;
				}
				_foundAttributes["Description"]++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ParseFieldAttributeAuthor(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
		{
			string text = null;
			if (_elem != null)
			{
				text = ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true);
			}
			string startValue;
			try
			{
				startValue = stringParser.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"Author\" value \"" + text + "\"", ((IXmlLineInfo)_elem)?.LineNumber ?? (-1), innerException);
			}
			DataItem<string> pAuthor = new DataItem<string>("Author", startValue);
			_entry.pAuthor = pAuthor;
			if (_elem != null)
			{
				if (!_foundAttributes.ContainsKey("Author"))
				{
					_foundAttributes["Author"] = 0;
				}
				_foundAttributes["Author"]++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ParseFieldAttributeVersion(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
		{
			string text = null;
			if (_elem != null)
			{
				text = ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true);
			}
			string startValue;
			try
			{
				startValue = stringParser.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"Version\" value \"" + text + "\"", ((IXmlLineInfo)_elem)?.LineNumber ?? (-1), innerException);
			}
			DataItem<string> pVersion = new DataItem<string>("Version", startValue);
			_entry.pVersion = pVersion;
			if (_elem != null)
			{
				if (!_foundAttributes.ContainsKey("Version"))
				{
					_foundAttributes["Version"] = 0;
				}
				_foundAttributes["Version"]++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ParseFieldAttributeWebsite(ModInfo _entry, Dictionary<string, int> _foundAttributes, XElement _elem)
		{
			string text = null;
			if (_elem != null)
			{
				text = ParserUtils.ParseStringAttribute(_elem, "value", _mandatory: true);
			}
			string startValue;
			try
			{
				startValue = stringParser.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new InvalidValueException("Could not parse attribute \"Website\" value \"" + text + "\"", ((IXmlLineInfo)_elem)?.LineNumber ?? (-1), innerException);
			}
			DataItem<string> pWebsite = new DataItem<string>("Website", startValue);
			_entry.pWebsite = pWebsite;
			if (_elem != null)
			{
				if (!_foundAttributes.ContainsKey("Website"))
				{
					_foundAttributes["Website"] = 0;
				}
				_foundAttributes["Website"]++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pName;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pAuthor;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pWebsite;

	public DataItem<string> Name
	{
		get
		{
			return pName;
		}
		set
		{
			pName = value;
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

	public DataItem<string> Author
	{
		get
		{
			return pAuthor;
		}
		set
		{
			pAuthor = value;
		}
	}

	public DataItem<string> Version
	{
		get
		{
			return pVersion;
		}
		set
		{
			pVersion = value;
		}
	}

	public DataItem<string> Website
	{
		get
		{
			return pWebsite;
		}
		set
		{
			pWebsite = value;
		}
	}
}
