using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using UnityEngine;

[MemoryPackable(GenerateType.Object)]
public class DynamicProperties : IMemoryPackable<DynamicProperties>, IMemoryPackFormatterRegister
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DynamicPropertiesFormatter : MemoryPackFormatter<DynamicProperties>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DynamicProperties value)
		{
			DynamicProperties.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DynamicProperties value)
		{
			DynamicProperties.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackInclude]
	public Dictionary<string, string> Values = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Params1 = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Params2 = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Data = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, DynamicProperties> Classes = new Dictionary<string, DynamicProperties>();

	[MemoryPackInclude]
	public Dictionary<string, List<Dictionary<string, string>>> Array = new Dictionary<string, List<Dictionary<string, string>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const char PathAccessSeparator = '.';

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] semicolonSeparator;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] equalSeparator;

	public Dictionary<string, string> ParseKeyData(string key)
	{
		try
		{
			if (Data.TryGetValue(key, out var value))
			{
				return ParseData(value);
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseKeyData error parsing key {0}, {1}", key, ex);
		}
		return null;
	}

	public static Dictionary<string, string> ParseData(string data)
	{
		Dictionary<string, string> dictionary = null;
		try
		{
			dictionary = new Dictionary<string, string>();
			if (data.IndexOf(';') < 0)
			{
				string[] array = data.Split(equalSeparator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length >= 2)
				{
					dictionary[array[0]] = array[1];
				}
			}
			else
			{
				string[] array2 = data.Split(semicolonSeparator, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array2.Length; i++)
				{
					string[] array3 = array2[i].Split(equalSeparator, StringSplitOptions.RemoveEmptyEntries);
					if (array3.Length >= 2)
					{
						dictionary[array3[0]] = array3[1];
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseData error parsing {0}, {1}", data, ex);
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateKey(string _key)
	{
		if (string.IsNullOrEmpty(_key) || _key.IndexOf('.') >= 0)
		{
			throw new Exception($"Property name '{_key}' contains illegal character '{'.'}'");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicProperties GetOrCreateClass(string _className)
	{
		ValidateKey(_className);
		if (!Classes.TryGetValue(_className, out var value))
		{
			value = new DynamicProperties();
			Classes[_className] = value;
		}
		return value;
	}

	public DynamicProperties GetClass(string _className)
	{
		if (Classes.TryGetValue(_className, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetValue(string _propName, string _value)
	{
		ValidateKey(_propName);
		Values[_propName] = _value;
	}

	public void SetValue(string _className, string _propName, string _value)
	{
		GetOrCreateClass(_className).SetValue(_propName, _value);
	}

	public string GetValue(string _propName)
	{
		if (Values.TryGetValue(_propName, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TryGetValue(string _propName, out string _value)
	{
		ValidateKey(_propName);
		return Values.TryGetValue(_propName, out _value);
	}

	public bool TryGetValue(string _className, string _propName, out string _value)
	{
		ValidateKey(_className);
		if (!Classes.TryGetValue(_className, out var value))
		{
			_value = null;
			return false;
		}
		return value.TryGetValue(_propName, out _value);
	}

	public bool Contains(string _propName)
	{
		ValidateKey(_propName);
		return Values.ContainsKey(_propName);
	}

	public bool Contains(string _className, string _propName)
	{
		string _value;
		return TryGetValue(_className, _propName, out _value);
	}

	public void SetParam1(string _propName, string _param1)
	{
		ValidateKey(_propName);
		if (!Values.ContainsKey(_propName))
		{
			Values.Add(_propName, null);
		}
		if (Params1.ContainsKey(_propName))
		{
			Params1[_propName] = _param1;
		}
		else
		{
			Params1.Add(_propName, _param1);
		}
	}

	public void SetParam1(string _className, string _propName, string _param1)
	{
		GetOrCreateClass(_className).SetParam1(_propName, _param1);
	}

	public bool TryGetParam1(string _propName, out string _param1)
	{
		ValidateKey(_propName);
		return Params1.TryGetValue(_propName, out _param1);
	}

	public bool TryGetParam1(string _className, string _propName, out string _param1)
	{
		ValidateKey(_className);
		if (!Classes.TryGetValue(_className, out var value))
		{
			_param1 = null;
			return false;
		}
		return value.TryGetParam1(_propName, out _param1);
	}

	public string GetParam1(string _propName)
	{
		if (Params1.TryGetValue(_propName, out var value))
		{
			return value;
		}
		return null;
	}

	public bool GetBool(string _propName)
	{
		if (!TryGetValue(_propName, out var _value))
		{
			return false;
		}
		StringParsers.TryParseBool(_value, out var _result);
		return _result;
	}

	public bool GetBool(string _className, string _propName)
	{
		if (!TryGetValue(_className, _propName, out var _value))
		{
			return false;
		}
		StringParsers.TryParseBool(_value, out var _result);
		return _result;
	}

	public float GetFloat(string _propName)
	{
		if (!TryGetValue(_propName, out var _value))
		{
			return 0f;
		}
		StringParsers.TryParseFloat(_value, out var _result);
		return _result;
	}

	public float GetFloat(string _className, string _propName)
	{
		if (!TryGetValue(_className, _propName, out var _value))
		{
			return 0f;
		}
		StringParsers.TryParseFloat(_value, out var _result);
		return _result;
	}

	public int GetInt(string _propName)
	{
		if (!TryGetValue(_propName, out var _value))
		{
			return 0;
		}
		int.TryParse(_value, out var result);
		return result;
	}

	public int GetInt(string _className, string _propName)
	{
		if (!TryGetValue(_className, _propName, out var _value))
		{
			return 0;
		}
		int.TryParse(_value, out var result);
		return result;
	}

	public string GetString(string _propName)
	{
		if (!TryGetValue(_propName, out var _value))
		{
			return string.Empty;
		}
		return _value;
	}

	public string GetString(string _className, string _propName)
	{
		if (!TryGetValue(_className, _propName, out var _value))
		{
			return string.Empty;
		}
		return _value;
	}

	public bool Load(string _directory, string _name)
	{
		try
		{
			foreach (XElement item in new XmlFile(_directory, _name).XmlDoc.Root.Elements(XNames.property))
			{
				Add(item);
			}
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool Save(string _rootNodeName, Stream stream)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.InsertBefore(newChild, xmlDocument.DocumentElement);
			XmlNode parent = xmlDocument.AppendChild(xmlDocument.CreateElement(_rootNodeName));
			toXml(xmlDocument, parent);
			xmlDocument.Save(stream);
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool Save(string _rootNodeName, string _path, string _name)
	{
		try
		{
			using Stream stream = SdFile.Create(Path.Join(_path, _name + ".xml"));
			return Save(_rootNodeName, stream);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toXml(XmlDocument _doc, XmlNode _parent)
	{
		foreach (KeyValuePair<string, string> value in Values)
		{
			XmlElement xmlElement = _doc.CreateElement("property");
			XmlAttribute xmlAttribute = _doc.CreateAttribute("name");
			xmlAttribute.Value = value.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = _doc.CreateAttribute("value");
			xmlAttribute2.Value = Values[value.Key];
			xmlElement.Attributes.Append(xmlAttribute2);
			if (Params1.ContainsKey(value.Key))
			{
				XmlAttribute xmlAttribute3 = _doc.CreateAttribute("param1");
				xmlAttribute3.Value = Params1[value.Key];
				xmlElement.Attributes.Append(xmlAttribute3);
			}
			if (Data.ContainsKey(value.Key))
			{
				XmlAttribute xmlAttribute4 = _doc.CreateAttribute("fields");
				xmlAttribute4.Value = Data[value.Key];
				xmlElement.Attributes.Append(xmlAttribute4);
			}
			_parent.AppendChild(xmlElement);
		}
		if (Classes.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, DynamicProperties> @class in Classes)
		{
			XmlElement xmlElement2 = _doc.CreateElement("property");
			XmlAttribute xmlAttribute5 = _doc.CreateAttribute("class");
			xmlAttribute5.Value = @class.Key;
			xmlElement2.Attributes.Append(xmlAttribute5);
			@class.Value.toXml(_doc, xmlElement2);
			_parent.AppendChild(xmlElement2);
		}
	}

	public void Add(XElement _propertyNode, bool _doValueReplace = false)
	{
		Parse(_propertyNode, _doValueReplace);
	}

	public void AddArray(XElement _arrayNode)
	{
		string attribute = _arrayNode.GetAttribute(XNames.name);
		if (string.IsNullOrEmpty(attribute))
		{
			return;
		}
		if (!Array.TryGetValue(attribute, out var value) || value == null)
		{
			value = new List<Dictionary<string, string>>();
			Array[attribute] = value;
		}
		value.Clear();
		foreach (XElement item in _arrayNode.Elements(XNames.item))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			value.Add(dictionary);
			foreach (XAttribute item2 in item.Attributes())
			{
				dictionary[item2.Name.ToString()] = item2.Value;
			}
		}
	}

	public void Parse(XElement elementProperty, bool _doValueReplace = false)
	{
		if (elementProperty.HasAttribute(XNames.class_))
		{
			string attribute = elementProperty.GetAttribute(XNames.class_);
			DynamicProperties orCreateClass = GetOrCreateClass(attribute);
			{
				foreach (XElement item in elementProperty.Elements(XNames.property))
				{
					orCreateClass.Parse(item, _doValueReplace);
				}
				return;
			}
		}
		string attribute2 = elementProperty.GetAttribute(XNames.name);
		if (attribute2.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on property");
		}
		ValidateKey(attribute2);
		string text = attribute2;
		string text2 = elementProperty.GetAttribute(XNames.value);
		if (_doValueReplace)
		{
			text2 = EntityClassesFromXml.ReplaceProperty(text2);
		}
		string text3 = null;
		if (elementProperty.HasAttribute(XNames.param1))
		{
			text3 = elementProperty.GetAttribute(XNames.param1);
		}
		string text4 = null;
		if (elementProperty.HasAttribute(XNames.param2))
		{
			text4 = elementProperty.GetAttribute(XNames.param2);
		}
		if (text3 != null)
		{
			Params1[text] = text3;
		}
		if (text4 != null)
		{
			Params2[text] = text4;
		}
		if (text2 != null)
		{
			string attribute3 = elementProperty.GetAttribute(XNames.data);
			if (attribute3.Length > 0)
			{
				Data[text] = attribute3;
			}
		}
		if (Classes.ContainsKey(text))
		{
			throw new Exception("Cannot create property '" + text + "': a class with the same name already exists. Property and class names must be unique.");
		}
		Values[text] = text2;
	}

	public void Clear()
	{
		Values.Clear();
		Params1.Clear();
		Params2.Clear();
		Data.Clear();
		Classes.Clear();
	}

	public void ParseString(string _propName, ref string optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = _value;
		}
	}

	public void ParseString(string _className, string _propName, ref string optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = _value;
		}
	}

	public void ParseStringFloatDictWithSubStringKey(string _propName, char _propSeparator, Dictionary<string, string> _dict, ref Dictionary<string, float> optionalValue)
	{
		foreach (KeyValuePair<string, string> item in _dict)
		{
			if (item.Key.StartsWith(_propName))
			{
				string text = item.Key.Substring(item.Key.IndexOf(_propSeparator) + 1);
				if (text.Length != item.Key.Length)
				{
					optionalValue.Add(text, StringParsers.ParseFloat(item.Value));
				}
			}
		}
	}

	public IntRange TryParseRange(string _propName, IntRange defaultValue = default(IntRange))
	{
		IntRange _result = defaultValue;
		if (TryGetValue(_propName, out var _value))
		{
			try
			{
				if (!StringParsers.TryParseRange(_value, out _result, null, '-'))
				{
					_result = defaultValue;
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}
		return _result;
	}

	public FloatRange TryParseRange(string _propName, FloatRange defaultValue = default(FloatRange))
	{
		FloatRange _result = defaultValue;
		if (TryGetValue(_propName, out var _value))
		{
			try
			{
				if (!StringParsers.TryParseRange(_value, out _result, (float?)null, '-'))
				{
					_result = defaultValue;
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}
		return _result;
	}

	public string GetLocalizedString(string _propName)
	{
		if (TryGetValue(_propName, out var _value))
		{
			return _value;
		}
		return string.Empty;
	}

	public void ParseLocalizedString(string _propName, ref string optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = Localization.Get(_value);
		}
	}

	public void ParseBool(string _propName, ref bool optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseBool(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse bool {0} '{1}'", _propName, _value);
		}
	}

	public void ParseByte(string _propName, ref byte optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseUInt8(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse byte {0} '{1}'", _propName, _value);
		}
	}

	public void ParseColor(string _propName, ref Color optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseColor32(_value);
		}
	}

	public void ParseColorHex(string _propName, ref Color optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseHexColor(_value);
		}
	}

	public void ParseEnum<T>(string _propName, ref T optionalValue) where T : struct, IConvertible
	{
		if (TryGetValue(_propName, out var _value) && EnumUtils.TryParse<T>(_value, out var _result, _ignoreCase: true))
		{
			optionalValue = _result;
		}
	}

	public void ParseFloat(string _propName, ref float optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseFloat(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", _propName, _value);
		}
	}

	public void ParseFloat(string _className, string _propName, ref float optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			if (StringParsers.TryParseFloat(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse float '{0}'", _value);
		}
	}

	public static void ParseFloat(XElement _e, string _propName, ref float optionalValue)
	{
		XAttribute xAttribute = _e.Attribute(_propName);
		if (xAttribute != null)
		{
			string value = xAttribute.Value;
			if (StringParsers.TryParseFloat(value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", _propName, value);
		}
	}

	public void ParseInt(string _propName, ref int optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseSInt32(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse int {0} '{1}'", _propName, _value);
		}
	}

	public void ParseInt(string _className, string _propName, ref int optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			if (StringParsers.TryParseSInt32(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse int {0}.{1} '{2}'", _className, _propName, _value);
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector2 optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value);
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue, float _defaultValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value, _defaultValue);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector2 optionalValue, float _defaultValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3 optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue, float _defaultValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value, _defaultValue);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3 optionalValue, float _defaultValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3i optionalValue)
	{
		if (TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3i(_value);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3i optionalValue)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3i(_value);
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		if (TryGetValue(_propName, out var _value))
		{
			Vector2 vector = StringParsers.ParseVector2(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			Vector2 vector = StringParsers.ParseVector2(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		if (TryGetValue(_propName, out var _value))
		{
			Vector3 vector = StringParsers.ParseVector3(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			Vector3 vector = StringParsers.ParseVector3(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		if (TryGetValue(_propName, out var _value))
		{
			Vector4 vector = StringParsers.ParseVector4(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
			optionalValue4 = vector.w;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		if (TryGetValue(_className, _propName, out var _value))
		{
			Vector4 vector = StringParsers.ParseVector4(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
			optionalValue4 = vector.w;
		}
	}

	public void CopyFrom(DynamicProperties _other, HashSet<string> _exclude = null)
	{
		copyDict(_other.Values, Values, _exclude);
		copyDict(_other.Params1, Params1, _exclude);
		copyDict(_other.Params2, Params2, _exclude);
		copyDict(_other.Data, Data, _exclude);
		foreach (KeyValuePair<string, DynamicProperties> @class in _other.Classes)
		{
			if (_exclude == null || !_exclude.Contains(@class.Key))
			{
				DynamicProperties dynamicProperties = (Classes.ContainsKey(@class.Key) ? Classes[@class.Key] : new DynamicProperties());
				Classes[@class.Key] = dynamicProperties;
				HashSet<string> nestedExclusions = GetNestedExclusions(@class.Key, _exclude);
				dynamicProperties.CopyFrom(@class.Value, nestedExclusions);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> GetNestedExclusions(string _className, HashSet<string> _exclude)
	{
		if (_exclude == null)
		{
			return null;
		}
		HashSet<string> hashSet = null;
		string text = _className + ".";
		foreach (string item in _exclude)
		{
			if (item.StartsWith(text))
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<string>();
				}
				hashSet.Add(item.Substring(text.Length));
			}
		}
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void copyDict(Dictionary<string, string> _source, Dictionary<string, string> _dest, HashSet<string> _exclude)
	{
		foreach (KeyValuePair<string, string> item in _source)
		{
			if (copyKey(item.Key, _exclude))
			{
				_dest[item.Key] = item.Value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool copyKey(string _key, HashSet<string> _exclude)
	{
		if (_exclude == null)
		{
			return true;
		}
		if (_exclude.Contains(_key))
		{
			return false;
		}
		if (_key.IndexOf('.') > 0)
		{
			foreach (string item in _exclude)
			{
				if (_key.StartsWith(item + "."))
				{
					return false;
				}
			}
		}
		return true;
	}

	public string PrettyPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PrettyPrint(stringBuilder, "");
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrettyPrint(StringBuilder sb, string indent)
	{
		sb.AppendFormat("{0}Properties:\n", indent);
		foreach (KeyValuePair<string, string> item in Values.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, string> kvp) => kvp.Key))
		{
			sb.AppendFormat("{2}    name={0}, value={1}", item.Key, Values[item.Key], indent);
			if (Params1.ContainsKey(item.Key))
			{
				sb.AppendFormat(", param1={0}", Params1[item.Key]);
			}
			if (Params2.ContainsKey(item.Key))
			{
				sb.AppendFormat(", param2={0}", Params2[item.Key]);
			}
			if (Data.ContainsKey(item.Key))
			{
				sb.AppendFormat(", fields={0}", Data[item.Key]);
			}
			sb.AppendLine();
		}
		if (Classes.Count <= 0)
		{
			return;
		}
		sb.AppendFormat("{0}Classes:\n", indent);
		foreach (KeyValuePair<string, DynamicProperties> @class in Classes)
		{
			sb.AppendFormat("{1}    class={0}\n", @class.Key, indent);
			@class.Value.PrettyPrint(sb, $"{indent}    ");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicProperties()
	{
		semicolonSeparator = new char[1] { ';' };
		equalSeparator = new char[1] { '=' };
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties>())
		{
			MemoryPackFormatterProvider.Register(new DynamicPropertiesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DynamicProperties>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, string>>())
		{
			MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, string>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, DynamicProperties>>())
		{
			MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, DynamicProperties>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, List<Dictionary<string, string>>>>())
		{
			MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, List<Dictionary<string, string>>>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Dictionary<string, string>>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<Dictionary<string, string>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DynamicProperties? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(6);
		writer.WriteValue(in value.Values);
		writer.WriteValue(in value.Params1);
		writer.WriteValue(in value.Params2);
		writer.WriteValue(in value.Data);
		writer.WriteValue(in value.Classes);
		writer.WriteValue(in value.Array);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DynamicProperties? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Dictionary<string, string> value2;
		Dictionary<string, string> value3;
		Dictionary<string, string> value4;
		Dictionary<string, string> value5;
		Dictionary<string, DynamicProperties> value6;
		Dictionary<string, List<Dictionary<string, string>>> value7;
		if (memberCount == 6)
		{
			if (value != null)
			{
				value2 = value.Values;
				value3 = value.Params1;
				value4 = value.Params2;
				value5 = value.Data;
				value6 = value.Classes;
				value7 = value.Array;
				reader.ReadValue(ref value2);
				reader.ReadValue(ref value3);
				reader.ReadValue(ref value4);
				reader.ReadValue(ref value5);
				reader.ReadValue(ref value6);
				reader.ReadValue(ref value7);
				goto IL_0160;
			}
			value2 = reader.ReadValue<Dictionary<string, string>>();
			value3 = reader.ReadValue<Dictionary<string, string>>();
			value4 = reader.ReadValue<Dictionary<string, string>>();
			value5 = reader.ReadValue<Dictionary<string, string>>();
			value6 = reader.ReadValue<Dictionary<string, DynamicProperties>>();
			value7 = reader.ReadValue<Dictionary<string, List<Dictionary<string, string>>>>();
		}
		else
		{
			if (memberCount > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DynamicProperties), 6, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = null;
				value5 = null;
				value6 = null;
				value7 = null;
			}
			else
			{
				value2 = value.Values;
				value3 = value.Params1;
				value4 = value.Params2;
				value5 = value.Data;
				value6 = value.Classes;
				value7 = value.Array;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						if (memberCount != 3)
						{
							reader.ReadValue(ref value5);
							if (memberCount != 4)
							{
								reader.ReadValue(ref value6);
								if (memberCount != 5)
								{
									reader.ReadValue(ref value7);
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0160;
			}
		}
		value = new DynamicProperties
		{
			Values = value2,
			Params1 = value3,
			Params2 = value4,
			Data = value5,
			Classes = value6,
			Array = value7
		};
		return;
		IL_0160:
		value.Values = value2;
		value.Params1 = value3;
		value.Params2 = value4;
		value.Data = value5;
		value.Classes = value6;
		value.Array = value7;
	}
}
