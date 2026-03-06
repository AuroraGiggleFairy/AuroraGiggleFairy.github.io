using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using HarmonyLib;

internal static class ModXmlPatcher
{
	private class DictScopeHelper : IDisposable
	{
		private Dictionary<string, string> prev;

		public DictScopeHelper(XElement element)
		{
			dict = GetTemplateValues(prev = dict, element);
		}

		public void Dispose()
		{
			dict = prev;
		}
	}

	public struct ParserStack
	{
		public int count;

		public bool IfClauseParsed;

		public bool PreviousResult;
	}

	[HarmonyPatch(typeof(XmlPatcher))]
	[HarmonyPatch("PatchXml")]
	public class XmlPatcher_PatchXml
	{
		private static bool Prefix(ref XmlFile _xmlFile, ref XmlFile _patchFile, XElement _containerElement, ref Mod _patchingMod, ref bool __result)
		{
			if (_patchFile == null)
			{
				return false;
			}
			XElement root = _patchFile.XmlDoc.Root;
			if (root == null)
			{
				return false;
			}
			string attribute = root.GetAttribute("patcher-version");
			if (!string.IsNullOrEmpty(attribute) && int.Parse(attribute) > 6)
			{
				return true;
			}
			__result = PatchXml(_xmlFile, _patchFile, _containerElement, _patchingMod);
			_patchFile = null;
			return false;
		}
	}

	public static Dictionary<string, Func<bool>> Conditions = null;

	private static Dictionary<string, string> dict = new Dictionary<string, string>();

	private static int count = 0;

	private static bool EvaluateCondition(string condition)
	{
		if (Conditions != null && Conditions.TryGetValue(condition, out var value))
		{
			return value();
		}
		if (ModManager.GetMod(condition) != null)
		{
			return true;
		}
		return false;
	}

	private static bool EvaluateConditions(string conditions, XmlFile xml)
	{
		if (string.IsNullOrEmpty(conditions))
		{
			return false;
		}
		if (conditions.StartsWith("xpath:"))
		{
			conditions = conditions.Substring(6);
			string[] array = conditions.Split(',');
			foreach (string text in array)
			{
				bool flag = false;
				List<XElement> list;
				if (text.StartsWith("!"))
				{
					flag = true;
					list = xml.XmlDoc.XPathSelectElements(text.Substring(1)).ToList();
				}
				else
				{
					list = xml.XmlDoc.XPathSelectElements(text).ToList();
				}
				bool flag2 = true;
				if (list == null)
				{
					flag2 = false;
				}
				if (list.Count == 0)
				{
					flag2 = false;
				}
				if (flag)
				{
					flag2 = !flag2;
				}
				if (!flag2)
				{
					return false;
				}
			}
		}
		else
		{
			string[] array = conditions.Split(',');
			foreach (string text2 in array)
			{
				bool flag3 = true;
				int num = ((text2[0] == '!') ? 1 : 0);
				int num2 = text2.IndexOf("<");
				int num3 = text2.IndexOf(">");
				int num4 = text2.IndexOf("≤");
				int num5 = text2.IndexOf("≥");
				int num6 = text2.Length - num;
				if (num2 != -1)
				{
					num6 = num2 - num;
				}
				else if (num3 != -1)
				{
					num6 = num3 - num;
				}
				else if (num4 != -1)
				{
					num6 = num4 - num;
				}
				else if (num5 != -1)
				{
					num6 = num5 - num;
				}
				string text3 = text2.Substring(num, num6);
				if (num6 != text2.Length - num)
				{
					Mod mod = ModManager.GetMod(text3);
					if (mod != null)
					{
						string input = text2.Substring(num + num6 + 1);
						Version version = mod.Version;
						Version version2 = Version.Parse(input);
						if (num2 != -1)
						{
							flag3 = version < version2;
						}
						if (num3 != -1)
						{
							flag3 = version > version2;
						}
						if (num4 != -1)
						{
							flag3 = version <= version2;
						}
						if (num5 != -1)
						{
							flag3 = version >= version2;
						}
					}
					else
					{
						flag3 = false;
					}
				}
				else if (!EvaluateCondition(text3))
				{
					flag3 = false;
				}
				if (num == 1)
				{
					flag3 = !flag3;
				}
				if (!flag3)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static Dictionary<string, string> GetTemplateValues(Dictionary<string, string> parent, XElement child)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(parent);
		foreach (XAttribute item in child.Attributes())
		{
			if (item.Name.LocalName.StartsWith("tmpl-"))
			{
				dictionary[item.Name.LocalName.Substring(5)] = item.Value;
			}
		}
		return dictionary;
	}

	private static void ReplaceTemplateOccurences(Dictionary<string, string> dict, ref string text)
	{
		foreach (KeyValuePair<string, string> item in dict)
		{
			text = text.Replace("{{" + item.Key + "}}", item.Value);
		}
	}

	private static bool IncludeAnotherDocument(XmlFile target, XmlFile parent, XElement element, Mod mod)
	{
		bool flag = true;
		string name = mod.Name;
		using (new DictScopeHelper(element))
		{
			foreach (XAttribute item in element.Attributes())
			{
				if (item.Name != "path")
				{
					continue;
				}
				string text = Path.Combine(Path.GetDirectoryName(Path.Combine(parent.Directory, parent.Filename)), item.Value);
				if (File.Exists(text))
				{
					try
					{
						string text2 = File.ReadAllText(text, Encoding.UTF8).Replace("@modfolder:", "@modfolder(" + name + "):");
						ReplaceTemplateOccurences(dict, ref text2);
						XmlFile xmlFile;
						try
						{
							xmlFile = new XmlFile(text2, Path.GetDirectoryName(text), Path.GetFileName(text), _throwExc: true);
						}
						catch (Exception ex)
						{
							Log.Error("XML loader: Loading XML patch include '{0}' from mod '{1}' failed.", new object[2] { text, name });
							Log.Exception(ex);
							flag = false;
							goto end_IL_0074;
						}
						flag &= XmlPatcher.PatchXml(target, xmlFile.XmlDoc.Root, xmlFile, mod);
						end_IL_0074:;
					}
					catch (Exception ex2)
					{
						Log.Error("XML loader: Patching '" + target.Filename + "' from mod '" + name + "' failed.");
						Log.Exception(ex2);
						flag = false;
					}
				}
				else
				{
					Log.Error("XML loader: Can't find XML include '{0}' from mod '{1}'.", new object[2] { text, name });
				}
			}
			return flag;
		}
	}

	public static bool PatchXml(XmlFile xmlFile, XmlFile patchXml, XElement node, Mod mod)
	{
		bool result = true;
		count++;
		ParserStack stack = new ParserStack
		{
			count = count
		};
		foreach (XElement item in node.Elements())
		{
			if (item.Name == "modinc")
			{
				IncludeAnotherDocument(xmlFile, patchXml, item, mod);
			}
			else if (item.Name == "echo")
			{
				foreach (XAttribute item2 in item.Attributes())
				{
					if (item2.Name == "log")
					{
						Log.Out("{1}: {0}", new object[2] { item2.Value, xmlFile.Filename });
					}
					if (item2.Name == "warn")
					{
						Log.Warning("{1}: {0}", new object[2] { item2.Value, xmlFile.Filename });
					}
					if (item2.Name == "error")
					{
						Log.Error("{1}: {0}", new object[2] { item2.Value, xmlFile.Filename });
					}
					if (item2.Name != "log" && item2.Name != "warn" && item2.Name != "error")
					{
						Log.Warning("Echo has no valid name (log, warn or error)");
					}
				}
			}
			else if (!ApplyPatchEntry(xmlFile, patchXml, item, mod, ref stack))
			{
				IXmlLineInfo xmlLineInfo = item;
				Log.Warning($"XML patch for \"{xmlFile.Filename}\" from mod \"{mod.Name}\" did not apply: {item.ToString()} (line {xmlLineInfo.LineNumber} at pos {xmlLineInfo.LinePosition})");
				result = false;
			}
		}
		return result;
	}

	private static bool ApplyPatchEntry(XmlFile _targetFile, XmlFile _patchFile, XElement _patchElement, Mod _patchingMod, ref ParserStack stack)
	{
		switch (_patchElement.Name.ToString())
		{
		case "modinc":
			return IncludeAnotherDocument(_targetFile, _patchFile, _patchElement, _patchingMod);
		case "modif":
			stack.IfClauseParsed = true;
			stack.PreviousResult = false;
			foreach (XAttribute item in _patchElement.Attributes())
			{
				if (item.Name != "condition")
				{
					Log.Warning("Ignoring unknown attribute {0}", new object[1] { item.Name });
				}
				else if (EvaluateConditions(item.Value, _targetFile))
				{
					stack.PreviousResult = true;
					return PatchXml(_targetFile, _patchFile, _patchElement, _patchingMod);
				}
			}
			return true;
		case "modelsif":
			if (!stack.IfClauseParsed)
			{
				Log.Error("Found <modelsif> clause out of order");
				return false;
			}
			if (stack.PreviousResult)
			{
				return true;
			}
			foreach (XAttribute item2 in _patchElement.Attributes())
			{
				if (item2.Name != "condition")
				{
					Log.Warning("Ignoring unknown attribute {0}", new object[1] { item2.Name });
				}
				else if (EvaluateConditions(item2.Value, _targetFile))
				{
					stack.PreviousResult = true;
					return PatchXml(_targetFile, _patchFile, _patchElement, _patchingMod);
				}
			}
			return true;
		case "modelse":
			stack.IfClauseParsed = false;
			if (stack.PreviousResult)
			{
				return true;
			}
			return PatchXml(_targetFile, _patchFile, _patchElement, _patchingMod);
		default:
			stack.IfClauseParsed = false;
			stack.PreviousResult = true;
			return XmlPatcher.singlePatch(_targetFile, _patchElement, _patchFile, _patchingMod);
		}
	}
}
