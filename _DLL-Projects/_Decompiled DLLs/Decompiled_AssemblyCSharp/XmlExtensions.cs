using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public static class XmlExtensions
{
	public static string GetElementString(this XElement _elem)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('<');
		stringBuilder.Append(_elem.Name);
		foreach (XAttribute item in _elem.Attributes())
		{
			stringBuilder.Append(' ');
			stringBuilder.Append(item.Name);
			stringBuilder.Append("=\"");
			stringBuilder.Append(item.Value);
			stringBuilder.Append('"');
		}
		stringBuilder.Append(' ');
		return stringBuilder.ToString();
	}

	public static string GetXPath(this XElement _elem)
	{
		StringBuilder stringBuilder = new StringBuilder();
		getXPath(stringBuilder, _elem);
		return stringBuilder.ToString();
	}

	public static string GetXPath(this XAttribute _attr)
	{
		StringBuilder stringBuilder = new StringBuilder();
		getXPath(stringBuilder, _attr.Parent);
		stringBuilder.Append("[@");
		stringBuilder.Append(_attr.Name);
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void getXPath(StringBuilder _sb, XElement _current)
	{
		if (_current.Parent != null)
		{
			getXPath(_sb, _current.Parent);
		}
		_sb.Append('/');
		_sb.Append(_current.Name);
	}

	public static bool HasAttribute(this XElement _element, XName _name)
	{
		return _element.Attribute(_name) != null;
	}

	public static string GetAttribute(this XElement _element, XName _name)
	{
		XAttribute xAttribute = _element.Attribute(_name);
		if (xAttribute == null)
		{
			return "";
		}
		return xAttribute.Value;
	}

	public static bool TryGetAttribute(this XElement _element, XName _name, out string _result)
	{
		XAttribute xAttribute = _element.Attribute(_name);
		_result = xAttribute?.Value;
		return xAttribute != null;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref int _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = int.Parse(_result2);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref string _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = _result2;
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref float _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = StringParsers.ParseFloat(_result2);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref Vector2 _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = StringParsers.ParseVector2(_result2);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref Vector3 _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = StringParsers.ParseVector3(_result2);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref ulong _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = ulong.Parse(_result2);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref bool _result)
	{
		if (_element.TryGetAttribute(_name, out var _result2))
		{
			_result = bool.Parse(_result2);
			return true;
		}
		return false;
	}

	public static List<XmlNode> ToList(this XmlNodeList _xmlNodeList)
	{
		List<XmlNode> list = new List<XmlNode>(_xmlNodeList.Count);
		foreach (XmlNode _xmlNode in _xmlNodeList)
		{
			list.Add(_xmlNode);
		}
		return list;
	}

	public static void CreateXmlDeclaration(this XmlDocument _doc)
	{
		XmlDeclaration newChild = _doc.CreateXmlDeclaration("1.0", "UTF-8", null);
		_doc.InsertBefore(newChild, _doc.DocumentElement);
	}

	public static XmlElement AddXmlElement(this XmlNode _node, string _name)
	{
		XmlDocument xmlDocument = ((_node.NodeType != XmlNodeType.Document) ? _node.OwnerDocument : ((XmlDocument)_node));
		XmlElement xmlElement = xmlDocument.CreateElement(_name);
		_node.AppendChild(xmlElement);
		return xmlElement;
	}

	public static XmlComment AddXmlComment(this XmlNode _node, string _content)
	{
		XmlDocument xmlDocument = ((_node.NodeType != XmlNodeType.Document) ? _node.OwnerDocument : ((XmlDocument)_node));
		XmlComment xmlComment = xmlDocument.CreateComment(_content);
		_node.AppendChild(xmlComment);
		return xmlComment;
	}

	public static XmlText AddXmlText(this XmlNode _node, string _text)
	{
		XmlDocument xmlDocument = ((_node.NodeType != XmlNodeType.Document) ? _node.OwnerDocument : ((XmlDocument)_node));
		XmlText xmlText = xmlDocument.CreateTextNode(_text);
		_node.AppendChild(xmlText);
		return xmlText;
	}

	public static XmlElement SetAttrib(this XmlElement _element, string _name, string _value)
	{
		_element.SetAttribute(_name, _value);
		return _element;
	}

	public static XmlElement AddXmlKeyValueProperty(this XmlNode _node, string _name, string _value)
	{
		return _node.AddXmlElement("property").SetAttrib("name", _name).SetAttrib("value", _value);
	}

	public static bool TryGetAttribute(this XmlElement _element, string _name, out string _result)
	{
		if (!_element.HasAttribute(_name))
		{
			_result = null;
			return false;
		}
		_result = _element.GetAttribute(_name);
		return true;
	}
}
