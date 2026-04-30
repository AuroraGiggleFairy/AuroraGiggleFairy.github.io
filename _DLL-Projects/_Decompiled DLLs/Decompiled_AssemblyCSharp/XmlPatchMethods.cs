using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
[XmlPatchMethodsClass]
public static class XmlPatchMethods
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ECsvOperation
	{
		Add,
		Remove
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EWildcardPositions
	{
		None,
		Start,
		End,
		Both
	}

	[XmlPatchMethod("append")]
	public static int AppendByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			XObject xObject = item;
			if (!(xObject is XElement xElement))
			{
				if (xObject is XAttribute xAttribute)
				{
					if (_patchSourceElement.FirstNode is XText xText)
					{
						xAttribute.Value += xText.Value.Trim();
						if (_patchingMod != null)
						{
							XComment content = new XComment($"Attribute \"{xAttribute.Name}\" appended by: \"{_patchingMod.Name}\"");
							xAttribute.Parent?.AddFirst(content);
						}
						continue;
					}
					XAttribute xAttribute2 = xAttribute;
					IXmlLineInfo xmlLineInfo = xAttribute2;
					throw new XmlPatchException(_patchSourceElement, "AppendByXPath", $"Appending to attribute ({xAttribute2.GetXPath()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}) from a non-text source: {_patchSourceElement.FirstNode.NodeType.ToStringCached()}");
				}
				IXmlLineInfo xmlLineInfo2 = item;
				throw new XmlPatchException(_patchSourceElement, "AppendByXPath", $"Matched node type ({item.NodeType.ToStringCached()}, line {xmlLineInfo2.LineNumber} at pos {xmlLineInfo2.LinePosition}) can not be appended to");
			}
			foreach (XElement item2 in _patchSourceElement.Elements())
			{
				XElement xElement2 = new XElement(item2);
				if (_patchingMod != null)
				{
					XComment content2 = new XComment("Element appended by: \"" + _patchingMod.Name + "\"");
					xElement2.AddFirst(content2);
				}
				xElement.Add(xElement2);
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("prepend")]
	public static int PrependByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			XObject xObject = item;
			if (!(xObject is XElement xElement))
			{
				if (xObject is XAttribute xAttribute)
				{
					if (_patchSourceElement.FirstNode is XText xText)
					{
						xAttribute.Value = xText.Value.Trim() + xAttribute.Value;
						if (_patchingMod != null)
						{
							XComment content = new XComment($"Attribute \"{xAttribute.Name}\" prepended by: \"{_patchingMod.Name}\"");
							xAttribute.Parent?.AddFirst(content);
						}
						continue;
					}
					XAttribute xAttribute2 = xAttribute;
					IXmlLineInfo xmlLineInfo = xAttribute2;
					throw new XmlPatchException(_patchSourceElement, "PrependByXPath", $"Prepending to attribute ({xAttribute2.GetXPath()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}) from a non-text source: {_patchSourceElement.FirstNode.NodeType.ToStringCached()}");
				}
				IXmlLineInfo xmlLineInfo2 = item;
				throw new XmlPatchException(_patchSourceElement, "PrependByXPath", $"Matched node type ({item.NodeType.ToStringCached()}, line {xmlLineInfo2.LineNumber} at pos {xmlLineInfo2.LinePosition}) can not be prepended to");
			}
			foreach (XElement item2 in _patchSourceElement.Elements())
			{
				XElement xElement2 = new XElement(item2);
				if (_patchingMod != null)
				{
					XComment content2 = new XComment("Element prepended by: \"" + _patchingMod.Name + "\"");
					xElement2.AddFirst(content2);
				}
				xElement.AddFirst(xElement2);
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("insertAfter")]
	public static int InsertAfterByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			if (!(item is XElement xElement))
			{
				continue;
			}
			foreach (XElement item2 in _patchSourceElement.Elements().Reverse())
			{
				XElement xElement2 = new XElement(item2);
				if (_patchingMod != null)
				{
					XComment content = new XComment("Element inserted by: \"" + _patchingMod.Name + "\"");
					xElement2.AddFirst(content);
				}
				xElement.AddAfterSelf(xElement2);
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("insertBefore")]
	public static int InsertBeforeByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			if (!(item is XElement xElement))
			{
				continue;
			}
			foreach (XElement item2 in _patchSourceElement.Elements())
			{
				XElement xElement2 = new XElement(item2);
				if (_patchingMod != null)
				{
					XComment content = new XComment("Element inserted by: \"" + _patchingMod.Name + "\"");
					xElement2.AddFirst(content);
				}
				xElement.AddBeforeSelf(xElement2);
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("remove")]
	public static int RemoveByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			if (!(item is XAttribute xAttribute))
			{
				if (item is XElement xElement)
				{
					if (_patchingMod != null)
					{
						XComment content = new XComment("Element removed by: \"" + _patchingMod.Name + "\" (XPath: \"" + _xpath + "\")");
						xElement.AddAfterSelf(content);
					}
					xElement.Remove();
					continue;
				}
				IXmlLineInfo xmlLineInfo = item;
				throw new XmlPatchException(_patchSourceElement, "RemoveByXPath", $"Matched node type ({item.NodeType.ToStringCached()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}) can not be removed");
			}
			IXmlLineInfo xmlLineInfo2 = xAttribute;
			throw new XmlPatchException(_patchSourceElement, "RemoveByXPath", $"Can not remove matched Attribute ({xAttribute.GetXPath()}, line {xmlLineInfo2.LineNumber} at pos {xmlLineInfo2.LinePosition}), use removeattribute instead");
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("set")]
	public static int SetByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			XObject xObject = item;
			if (!(xObject is XElement xElement))
			{
				if (!(xObject is XAttribute xAttribute))
				{
					IXmlLineInfo xmlLineInfo = item;
					throw new XmlPatchException(_patchSourceElement, "SetByXPath", $"Matched node type ({item.NodeType.ToStringCached()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}) can not be set");
				}
				if (!_patchSourceElement.Nodes().Any())
				{
					IXmlLineInfo xmlLineInfo2 = item;
					throw new XmlPatchException(_patchSourceElement, "SetByXPath", $"Setting attribute ({xAttribute.GetXPath()}, line {xmlLineInfo2.LineNumber} at pos {xmlLineInfo2.LinePosition}) without any replacement text given as child element");
				}
				XAttribute xAttribute2 = xAttribute;
				if (!(_patchSourceElement.FirstNode is XText xText))
				{
					XAttribute attr = xAttribute;
					IXmlLineInfo xmlLineInfo3 = item;
					throw new XmlPatchException(_patchSourceElement, "SetByXPath", $"Setting attribute ({attr.GetXPath()}, line {xmlLineInfo3.LineNumber} at pos {xmlLineInfo3.LinePosition}) from a non-text source: {_patchSourceElement.FirstNode.NodeType.ToStringCached()}");
				}
				xAttribute2.Value = xText.Value.Trim();
				if (_patchingMod != null)
				{
					XComment content = new XComment($"Attribute \"{xAttribute2.Name}\" replaced by: \"{_patchingMod.Name}\"");
					xAttribute2.Parent?.AddFirst(content);
				}
			}
			else
			{
				xElement.ReplaceNodes(_patchSourceElement.Nodes());
				if (_patchingMod != null)
				{
					XComment content2 = new XComment("Element contents replaced by: \"" + _patchingMod.Name + "\"");
					xElement.AddFirst(content2);
				}
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("setattribute")]
	public static int SetAttributeByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		if (!_patchSourceElement.HasAttribute("name"))
		{
			throw new XmlPatchException(_patchSourceElement, "SetAttributeByXPath", "Patch element does not have a 'name' attribute");
		}
		XName xName = _patchSourceElement.GetAttribute("name");
		string value = ((_patchSourceElement.FirstNode as XText) ?? throw new XmlPatchException(_patchSourceElement, "SetAttributeByXPath", $"Setting attribute ({xName}) from a non-text source: {_patchSourceElement.FirstNode.NodeType.ToStringCached()}")).Value.Trim();
		foreach (XObject item in _matchList)
		{
			if (item is XElement xElement)
			{
				xElement.SetAttributeValue(xName, value);
				if (_patchingMod != null)
				{
					XComment content = new XComment($"Attribute \"{xName}\" added/overwritten by: \"{_patchingMod.Name}\"");
					xElement.AddFirst(content);
				}
				continue;
			}
			IXmlLineInfo xmlLineInfo = item;
			throw new XmlPatchException(_patchSourceElement, "SetAttributeByXPath", $"Matched node (line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}) is not an XML element but {item.NodeType.ToStringCached()}");
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("removeattribute")]
	public static int RemoveAttributeByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		foreach (XObject item in _matchList)
		{
			if (!(item is XAttribute xAttribute))
			{
				IXmlLineInfo xmlLineInfo = item;
				throw new XmlPatchException(_patchSourceElement, "RemoveAttributeByXPath", $"Can only remove Attributes (matched {item.NodeType.ToStringCached()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition}), use remove instead");
			}
			if (_patchingMod != null)
			{
				XComment content = new XComment($"Attribute \"{xAttribute.Name}\" removed by: \"{_patchingMod.Name}\"");
				xAttribute.Parent?.AddFirst(content);
			}
			xAttribute.Remove();
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("csv")]
	public static int CsvOperationsByXPath(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (!_targetFile.GetXpathResults(_xpath, out var _matchList))
		{
			return 0;
		}
		if (!EnumUtils.TryParse<ECsvOperation>((_patchSourceElement.Attribute("op") ?? throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "Patch element does not have an 'op' attribute")).Value, out var _result, _ignoreCase: true))
		{
			throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "Unsupported 'op' attribute value (only supports 'add', 'remove')");
		}
		XAttribute xAttribute = _patchSourceElement.Attribute("delim");
		if (xAttribute != null && xAttribute.Value.Length != 1 && xAttribute.Value != "\\n")
		{
			throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "Patch element 'delim' attribute needs to be exactly 1 character");
		}
		char c = xAttribute?.Value[0] ?? ',';
		if (xAttribute?.Value == "\\n")
		{
			c = '\n';
		}
		bool _result2 = false;
		XAttribute xAttribute2 = _patchSourceElement.Attribute("keep_whitespace");
		if (xAttribute2 != null && !StringParsers.TryParseBool(xAttribute2.Value, out _result2))
		{
			throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "Patch element 'keep_whitespace' attribute needs to be a valid boolean value");
		}
		List<string> list = new List<string>(((_patchSourceElement.FirstNode as XText) ?? throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "CSV operations require a text value: " + _patchSourceElement.FirstNode.NodeType.ToStringCached())).Value.Split(c, int.MaxValue, StringSplitOptions.RemoveEmptyEntries));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num] = list[num].Trim();
			if (list[num].Length == 0)
			{
				list.RemoveAt(num);
			}
		}
		EWildcardPositions[] array = null;
		if (_result == ECsvOperation.Remove)
		{
			array = new EWildcardPositions[list.Count];
		}
		for (int i = 0; i < list.Count; i++)
		{
			bool flag = list[i].StartsWith('*');
			bool flag2 = list[i].EndsWith('*');
			if (_result == ECsvOperation.Add && (flag || flag2))
			{
				throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", "Only 'remove' operation supports wildcards");
			}
			if (_result == ECsvOperation.Remove)
			{
				array[i] = ((flag && flag2) ? EWildcardPositions.Both : (flag ? EWildcardPositions.Start : (flag2 ? EWildcardPositions.End : EWildcardPositions.None)));
				if (flag)
				{
					list[i] = list[i].Substring(1);
				}
				if (flag2)
				{
					list[i] = list[i].Substring(0, list[i].Length - 1);
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (XObject item in _matchList)
		{
			XAttribute xAttribute3 = item as XAttribute;
			XText xText = item as XText;
			if (xAttribute3 == null && xText == null)
			{
				IXmlLineInfo xmlLineInfo = item;
				throw new XmlPatchException(_patchSourceElement, "CsvOperationsByXPath", $"Can only operate on Attributes or Text (matched {item.NodeType.ToStringCached()}, line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition})");
			}
			string text = ((xAttribute3 != null) ? xAttribute3.Value : xText.Value);
			List<string> list2 = new List<string>(text.Split(c, int.MaxValue, StringSplitOptions.RemoveEmptyEntries));
			if (!_result2)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					list2[j] = list2[j].Trim();
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				string text2 = list[k];
				switch (_result)
				{
				case ECsvOperation.Add:
					if (!list2.ContainsCaseInsensitive(text2))
					{
						list2.Add(text2);
					}
					break;
				case ECsvOperation.Remove:
				{
					EWildcardPositions eWildcardPositions = array[k];
					for (int num2 = list2.Count - 1; num2 >= 0; num2--)
					{
						if (eWildcardPositions switch
						{
							EWildcardPositions.None => list2[num2].Trim().EqualsCaseInsensitive(text2), 
							EWildcardPositions.Start => list2[num2].TrimEnd().EndsWith(text2, StringComparison.OrdinalIgnoreCase), 
							EWildcardPositions.End => list2[num2].TrimStart().StartsWith(text2, StringComparison.OrdinalIgnoreCase), 
							EWildcardPositions.Both => list2[num2].ContainsCaseInsensitive(text2), 
							_ => false, 
						})
						{
							list2.RemoveAt(num2);
						}
					}
					break;
				}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				if (l > 0)
				{
					stringBuilder.Append(c);
				}
				stringBuilder.Append(list2[l]);
			}
			text = stringBuilder.ToString();
			stringBuilder.Clear();
			if (xAttribute3 != null)
			{
				xAttribute3.Value = text;
			}
			else
			{
				xText.Value = text;
			}
			if (_patchingMod != null)
			{
				if (xAttribute3 != null)
				{
					XComment content = new XComment($"Attribute \"{xAttribute3.Name}\" CSV manipulated by: \"{_patchingMod.Name}\"");
					xAttribute3.Parent?.AddFirst(content);
				}
				else
				{
					XComment content2 = new XComment("Content CSV manipulated by: \"" + _patchingMod.Name + "\"");
					xText.Parent?.Add(content2);
				}
			}
		}
		return _targetFile.ClearXpathResults();
	}

	[XmlPatchMethod("conditional", false)]
	public static int Conditional(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		if (_patchSourceElement.HasAttribute("evaluator"))
		{
			Log.Warning("XML loader: Patching '" + _targetFile.Filename + "' from mod '" + _patchingMod.Name + "': Conditional patch element ignores 'evaluator' attribute!");
		}
		XElement xElement = XmlPatchConditionEvaluator.FindActiveConditionalBranchElement(_targetFile, _patchSourceElement);
		if (xElement == null)
		{
			return 1;
		}
		if (!XmlPatcher.PatchXml(_targetFile, xElement, _patchFile, _patchingMod))
		{
			return 0;
		}
		return 1;
	}

	[XmlPatchMethod("include", false)]
	public static int Include(XmlFile _targetFile, string _xpath, XElement _patchSourceElement, XmlFile _patchFile, Mod _patchingMod = null)
	{
		string value = (_patchSourceElement.Attribute("filename") ?? throw new XmlPatchException(_patchSourceElement, "Include", "Patch element does not have an 'filename' attribute")).Value;
		string text = Path.Combine(Path.GetDirectoryName(Path.Combine(_patchFile.Directory, _patchFile.Filename)), value);
		if (!SdFile.Exists(text))
		{
			throw new XmlPatchException(_patchSourceElement, "Include", "Given file not found: " + value);
		}
		try
		{
			XmlFile xmlFile = XmlPatcher.ReadPatchXmlWithFixedModFolders(_patchingMod, text);
			if (xmlFile == null)
			{
				throw new XmlPatchException(_patchSourceElement, "Include", "Error loading included patch file: " + value);
			}
			XElement root = xmlFile.XmlDoc.Root;
			return XmlPatcher.PatchXml(_targetFile, root, xmlFile, _patchingMod) ? 1 : 0;
		}
		catch (Exception e)
		{
			Log.Error("XML loader: Patching '" + _targetFile.Filename + "' from mod '" + _patchingMod.Name + "' failed:");
			Log.Exception(e);
		}
		return 0;
	}
}
