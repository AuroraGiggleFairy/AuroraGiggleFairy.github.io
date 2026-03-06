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

	public DictionarySave<string, string> Params1 = new DictionarySave<string, string>();

	public DictionarySave<string, string> Params2 = new DictionarySave<string, string>();

	public DictionarySave<string, string> Data = new DictionarySave<string, string>();

	public HashSet<string> Display = new HashSet<string>();

	public DictionarySave<string, DynamicProperties> Classes = new DictionarySave<string, DynamicProperties>();

	public DictionarySave<string, string> Values = new DictionarySave<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] semicolonSeparator;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] equalSeparator;

	public DictionarySave<string, string> ParseKeyData(string key)
	{
		try
		{
			if (Data.TryGetValue(key, out var _value))
			{
				return ParseData(_value);
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseKeyData error parsing key {0}, {1}", key, ex);
		}
		return null;
	}

	public static DictionarySave<string, string> ParseData(string data)
	{
		DictionarySave<string, string> dictionarySave = null;
		try
		{
			dictionarySave = new DictionarySave<string, string>();
			if (data.IndexOf(';') < 0)
			{
				string[] array = data.Split(equalSeparator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length >= 2)
				{
					dictionarySave[array[0]] = array[1];
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
						dictionarySave[array3[0]] = array3[1];
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseData error parsing {0}, {1}", data, ex);
		}
		return dictionarySave;
	}

	public bool Load(string _directory, string _name, bool _addClassesToMain = true)
	{
		try
		{
			foreach (XElement item in new XmlFile(_directory, _name).XmlDoc.Root.Elements(XNames.property))
			{
				Add(item, _addClassesToMain);
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
		foreach (KeyValuePair<string, string> item in Values.Dict)
		{
			XmlElement xmlElement = _doc.CreateElement("property");
			XmlAttribute xmlAttribute = _doc.CreateAttribute("name");
			xmlAttribute.Value = item.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = _doc.CreateAttribute("value");
			xmlAttribute2.Value = Values[item.Key];
			xmlElement.Attributes.Append(xmlAttribute2);
			if (Params1.ContainsKey(item.Key))
			{
				XmlAttribute xmlAttribute3 = _doc.CreateAttribute("param1");
				xmlAttribute3.Value = Params1[item.Key];
				xmlElement.Attributes.Append(xmlAttribute3);
			}
			if (Data.ContainsKey(item.Key))
			{
				XmlAttribute xmlAttribute4 = _doc.CreateAttribute("fields");
				xmlAttribute4.Value = Data[item.Key];
				xmlElement.Attributes.Append(xmlAttribute4);
			}
			_parent.AppendChild(xmlElement);
		}
		if (Classes.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, DynamicProperties> item2 in Classes.Dict)
		{
			XmlElement xmlElement2 = _doc.CreateElement("property");
			XmlAttribute xmlAttribute5 = _doc.CreateAttribute("class");
			xmlAttribute5.Value = item2.Key;
			xmlElement2.Attributes.Append(xmlAttribute5);
			item2.Value.toXml(_doc, xmlElement2);
			_parent.AppendChild(xmlElement2);
		}
	}

	public void Add(XElement _propertyNode, bool _addClassesToMain = true, bool _doValueReplace = false)
	{
		Parse(null, _propertyNode, this, _addClassesToMain, _doValueReplace);
	}

	public void Parse(string _className, XElement elementProperty, DynamicProperties _mainProperties, bool _addClassesToMain, bool _doValueReplace = false)
	{
		if (elementProperty.HasAttribute(XNames.class_))
		{
			string attribute = elementProperty.GetAttribute(XNames.class_);
			string text = ((_className != null) ? (_className + "." + attribute) : attribute);
			if (!Classes.TryGetValue(attribute, out var _value))
			{
				_value = new DynamicProperties();
				Classes.Add(attribute, _value);
				if (_addClassesToMain && _mainProperties != this)
				{
					_mainProperties.Classes[text] = _value;
				}
			}
			{
				foreach (XElement item in elementProperty.Elements(XNames.property))
				{
					_value.Parse(text, item, _mainProperties, _addClassesToMain);
				}
				return;
			}
		}
		string attribute2 = elementProperty.GetAttribute(XNames.name);
		if (attribute2.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on property");
		}
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
		string text5 = ((_className != null) ? (_className + "." + attribute2) : attribute2);
		if (text3 != null)
		{
			Params1[attribute2] = text3;
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Params1[text5] = text3;
			}
		}
		if (text4 != null)
		{
			Params2[attribute2] = text4;
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Params2[text5] = text4;
			}
		}
		if (text2 != null)
		{
			string attribute3 = elementProperty.GetAttribute(XNames.data);
			if (attribute3.Length > 0)
			{
				Data[attribute2] = attribute3;
				if (_addClassesToMain && _mainProperties != this)
				{
					_mainProperties.Data[text5] = attribute3;
				}
			}
		}
		Values[attribute2] = text2;
		if (_addClassesToMain && _mainProperties != this)
		{
			_mainProperties.Values[text5] = text2;
		}
		if (elementProperty.HasAttribute(XNames.display) && StringParsers.ParseBool(elementProperty.GetAttribute(XNames.display)))
		{
			Display.Add(attribute2);
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Display.Add(text5);
			}
		}
	}

	public void Clear()
	{
		Values.Clear();
		Params1.Clear();
		Params2.Clear();
	}

	public bool Contains(string _propName)
	{
		return Values.ContainsKey(_propName);
	}

	public bool Contains(string _className, string _propName)
	{
		if (Classes.TryGetValue(_className, out var _value))
		{
			return _value.Contains(_propName);
		}
		return false;
	}

	public bool GetBool(string _propName)
	{
		StringParsers.TryParseBool(Values[_propName], out var _result);
		return _result;
	}

	public float GetFloat(string _propName)
	{
		StringParsers.TryParseFloat(Values[_propName], out var _result);
		return _result;
	}

	public int GetInt(string _propName)
	{
		int.TryParse(Values[_propName], out var result);
		return result;
	}

	public string GetStringValue(string _propName)
	{
		return Values[_propName].ToString();
	}

	public string GetString(string _propName)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			return _value;
		}
		return string.Empty;
	}

	public string GetString(string _className, string _propName)
	{
		if (Classes.TryGetValue(_className, out var _value))
		{
			return _value.GetString(_propName);
		}
		return string.Empty;
	}

	public void SetValue(string _propName, string _value)
	{
		if (Values.ContainsKey(_propName))
		{
			Values[_propName] = _value;
		}
		else
		{
			Values.Add(_propName, _value);
		}
	}

	public void SetValue(string _className, string _propName, string _value)
	{
		if (!Classes.TryGetValue(_className, out var _value2))
		{
			_value2 = new DynamicProperties();
			Classes[_className] = _value2;
		}
		_value2.SetValue(_propName, _value);
		string propName = $"{_className}.{_propName}";
		SetValue(propName, _value);
	}

	public void SetParam1(string _propName, string _param1)
	{
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

	public void ParseString(string _propName, ref string optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = _value;
		}
	}

	public string GetLocalizedString(string _propName)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			return Localization.Get(_value);
		}
		return string.Empty;
	}

	public void ParseLocalizedString(string _propName, ref string optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = Localization.Get(_value);
		}
	}

	public void ParseBool(string _propName, ref bool optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseBool(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse bool {0} '{1}'", _propName, _value);
		}
	}

	public void ParseColor(string _propName, ref Color optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseColor32(_value);
		}
	}

	public void ParseColorHex(string _propName, ref Color optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseHexColor(_value);
		}
	}

	public void ParseEnum<T>(string _propName, ref T optionalValue) where T : struct, IConvertible
	{
		if (Values.TryGetValue(_propName, out var _value) && EnumUtils.TryParse<T>(_value, out var _result, _ignoreCase: true))
		{
			optionalValue = _result;
		}
	}

	public void ParseFloat(string _propName, ref float optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseFloat(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", _propName, _value);
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
		if (Values.TryGetValue(_propName, out var _value))
		{
			if (StringParsers.TryParseSInt32(_value, out var _result))
			{
				optionalValue = _result;
				return;
			}
			Log.Warning("Can't parse int {0} '{1}'", _propName, _value);
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value);
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue, float _defaultValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector2(_value, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue, float _defaultValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3(_value, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3i optionalValue)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			optionalValue = StringParsers.ParseVector3i(_value);
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			Vector2 vector = StringParsers.ParseVector2(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		if (Values.TryGetValue(_propName, out var _value))
		{
			Vector3 vector = StringParsers.ParseVector3(_value);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		if (Values.TryGetValue(_propName, out var _value))
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
		foreach (string item in _other.Display)
		{
			if (copyKey(item, _exclude))
			{
				Display.Add(item);
			}
		}
		foreach (KeyValuePair<string, DynamicProperties> item2 in _other.Classes.Dict)
		{
			if (copyKey(item2.Key, _exclude))
			{
				DynamicProperties dynamicProperties = (Classes.ContainsKey(item2.Key) ? Classes[item2.Key] : new DynamicProperties());
				Classes[item2.Key] = dynamicProperties;
				dynamicProperties.CopyFrom(item2.Value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void copyDict(DictionarySave<string, string> _source, DictionarySave<string, string> _dest, HashSet<string> _exclude)
	{
		foreach (KeyValuePair<string, string> item in _source.Dict)
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
		if (_key.IndexOf('.') <= 0)
		{
			return true;
		}
		foreach (string item in _exclude)
		{
			if (_key.StartsWith(item + "."))
			{
				return false;
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
		foreach (KeyValuePair<string, string> item in Values.Dict.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, string> kvp) => kvp.Key))
		{
			sb.AppendFormat("{2}    name={0}, value={1}", item.Key, Values[item.Key], indent);
			if (Params1.ContainsKey(item.Key))
			{
				sb.AppendFormat(", param1={0}", Params1[item.Key]);
			}
			if (Data.ContainsKey(item.Key))
			{
				sb.AppendFormat(", fields={0}", Params1[item.Key]);
			}
			sb.AppendLine();
		}
		if (Classes.Count <= 0)
		{
			return;
		}
		sb.AppendFormat("{0}Classes:\n", indent);
		foreach (KeyValuePair<string, DynamicProperties> item2 in Classes.Dict)
		{
			sb.AppendFormat("{1}    class={0}\n", item2.Key, indent);
			item2.Value.PrettyPrint(sb, $"{indent}    ");
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
		if (!MemoryPackFormatterProvider.IsRegistered<HashSet<string>>())
		{
			MemoryPackFormatterProvider.Register(new HashSetFormatter<string>());
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
		writer.WritePackable(in value.Params1);
		writer.WritePackable(in value.Params2);
		writer.WritePackable(in value.Data);
		writer.WriteValue(in value.Display);
		writer.WritePackable(in value.Classes);
		writer.WritePackable(in value.Values);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DynamicProperties? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		DictionarySave<string, string> value2;
		DictionarySave<string, string> value3;
		DictionarySave<string, string> value4;
		HashSet<string> value5;
		DictionarySave<string, DynamicProperties> value6;
		DictionarySave<string, string> value7;
		if (memberCount == 6)
		{
			if (value != null)
			{
				value2 = value.Params1;
				value3 = value.Params2;
				value4 = value.Data;
				value5 = value.Display;
				value6 = value.Classes;
				value7 = value.Values;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				reader.ReadValue(ref value5);
				reader.ReadPackable(ref value6);
				reader.ReadPackable(ref value7);
				goto IL_0160;
			}
			value2 = reader.ReadPackable<DictionarySave<string, string>>();
			value3 = reader.ReadPackable<DictionarySave<string, string>>();
			value4 = reader.ReadPackable<DictionarySave<string, string>>();
			value5 = reader.ReadValue<HashSet<string>>();
			value6 = reader.ReadPackable<DictionarySave<string, DynamicProperties>>();
			value7 = reader.ReadPackable<DictionarySave<string, string>>();
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
				value2 = value.Params1;
				value3 = value.Params2;
				value4 = value.Data;
				value5 = value.Display;
				value6 = value.Classes;
				value7 = value.Values;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadValue(ref value5);
							if (memberCount != 4)
							{
								reader.ReadPackable(ref value6);
								if (memberCount != 5)
								{
									reader.ReadPackable(ref value7);
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
			Params1 = value2,
			Params2 = value3,
			Data = value4,
			Display = value5,
			Classes = value6,
			Values = value7
		};
		return;
		IL_0160:
		value.Params1 = value2;
		value.Params2 = value3;
		value.Data = value4;
		value.Display = value5;
		value.Classes = value6;
		value.Values = value7;
	}
}
