using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HarmonyLib;

// OCB 2.5 config files use modinc / modif / modelsif / modelse. This game
// does not know those tags, so the tractor XML never applies. The original
// files stay on disk. This hook applies them when the patcher reaches them.
internal static class OcbLegacyXml
{
	private const string OriginalModName = "OcbLawnMowing";
	private static string fixedBundle = "#@modfolder(OcbLawnMowing):Resources/LawnMowers.unity3d";

	private static readonly Dictionary<string, string> EmptyScope = new Dictionary<string, string>();
	private static readonly Stack<Dictionary<string, string>> TemplateScopes = new Stack<Dictionary<string, string>>();

	private static XElement chainParent;
	private static bool chainMatched;

	public static bool OriginalModLoaded()
	{
		foreach (Mod mod in ModManager.GetLoadedMods())
		{
			if (string.Equals(mod.Name, OriginalModName, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	public static void Install(Harmony harmony, Mod companionMod)
	{
		harmony.Patch(
			AccessTools.Method(typeof(XmlPatcher), "singlePatch"),
			new HarmonyMethod(typeof(OcbLegacyXml), nameof(Prefix)));
		harmony.Patch(
			AccessTools.Method(typeof(XmlPatcher), nameof(XmlPatcher.ReadPatchXmlWithFixedModFolders)),
			postfix: new HarmonyMethod(typeof(OcbLegacyXml), nameof(RedirectOriginalBundle)));
		Log.Out("[LawnTractorV3Fix] OCB config loader is active for " + OriginalModName + ".");
	}

	public static bool Prefix(XmlFile _targetFile, XElement _patchElement, XmlFile _patchFile, Mod _patchingMod, ref bool __result)
	{
		if (_patchingMod == null || _patchElement == null || !string.Equals(_patchingMod.Name, OriginalModName, StringComparison.Ordinal))
		{
			return true;
		}

		switch (_patchElement.Name.LocalName)
		{
			case "modinc":
				__result = ApplyInclude(_targetFile, _patchElement, _patchFile, _patchingMod);
				return false;
			case "modif":
			case "modelsif":
			case "modelse":
				__result = ApplyConditional(_targetFile, _patchElement, _patchFile, _patchingMod);
				return false;
			case "echo":
				__result = ApplyEcho(_patchElement);
				return false;
			default:
				AdaptForV3(_patchElement);
				chainParent = null;
				chainMatched = false;
				return true;
		}
	}

	private static bool ApplyConditional(XmlFile targetFile, XElement element, XmlFile patchFile, Mod patchingMod)
	{
		string kind = element.Name.LocalName;
		XElement parent = element.Parent;
		if (kind == "modif" || chainParent != parent)
		{
			chainParent = parent;
			chainMatched = false;
		}

		bool take = false;
		if (!chainMatched)
		{
			take = kind == "modelse" || IsModLoaded(element.Attribute("condition")?.Value);
		}

		if (!take)
		{
			return true;
		}

		XElement savedParent = chainParent;
		bool savedMatched = chainMatched;
		chainParent = null;
		chainMatched = false;
		bool applied;
		try
		{
			applied = XmlPatcher.PatchXml(targetFile, element, patchFile, patchingMod);
		}
		finally
		{
			chainParent = savedParent;
			chainMatched = savedMatched;
		}

		chainParent = parent;
		chainMatched = true;
		return applied;
	}

	private static bool ApplyInclude(XmlFile targetFile, XElement element, XmlFile patchFile, Mod patchingMod)
	{
		string relativePath = element.Attribute("path")?.Value;
		if (string.IsNullOrEmpty(relativePath))
		{
			Log.Warning("[LawnTractorV3Fix] OCB modinc is missing a path.");
			return false;
		}

		Dictionary<string, string> scope = new Dictionary<string, string>(CurrentScope(), StringComparer.Ordinal);
		foreach (XAttribute attribute in element.Attributes())
		{
			string name = attribute.Name.LocalName;
			if (name.StartsWith("tmpl-", StringComparison.Ordinal))
			{
				scope[name.Substring(5)] = attribute.Value;
			}
		}

		string patchDirectory = Path.GetDirectoryName(Path.Combine(patchFile.Directory, patchFile.Filename));
		string fullPath = Path.Combine(patchDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
		if (!File.Exists(fullPath))
		{
			Log.Warning("[LawnTractorV3Fix] OCB include was not found: " + relativePath);
			return false;
		}

		string text = File.ReadAllText(fullPath);
		foreach (KeyValuePair<string, string> pair in scope)
		{
			text = text.Replace("{{" + pair.Key + "}}", pair.Value);
		}
		text = text.Replace("@modfolder:", "@modfolder(" + patchingMod.Name + "):");

		TemplateScopes.Push(scope);
		try
		{
			XmlFile included = new XmlFile(text, Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath), true);
			XElement root = included.XmlDoc != null ? included.XmlDoc.Root : null;
			if (root == null)
			{
				Log.Warning("[LawnTractorV3Fix] OCB include has no root: " + relativePath);
				return false;
			}
			RewriteBundlePaths(root);
			return XmlPatcher.PatchXml(targetFile, root, included, patchingMod);
		}
		catch (Exception exception)
		{
			Log.Error("[LawnTractorV3Fix] Failed to apply OCB include " + relativePath);
			Log.Exception(exception);
			return false;
		}
		finally
		{
			TemplateScopes.Pop();
		}
	}

	public static void RedirectOriginalBundle(Mod _parentMod, ref XmlFile __result)
	{
		if (_parentMod == null || __result == null || __result.XmlDoc == null || !string.Equals(_parentMod.Name, OriginalModName, StringComparison.Ordinal))
		{
			return;
		}
		RewriteBundlePaths(__result.XmlDoc.Root);
	}

	private static void RewriteBundlePaths(XElement element)
	{
		if (element == null)
		{
			return;
		}
		foreach (XElement node in element.DescendantsAndSelf())
		{
			foreach (XAttribute attribute in node.Attributes())
			{
				string value = attribute.Value;
				if (string.IsNullOrEmpty(value) || value.IndexOf("LawnMowers.unity3d", StringComparison.Ordinal) < 0 || value.IndexOf("modfolder(" + OriginalModName + ")", StringComparison.Ordinal) >= 0)
				{
					continue;
				}
				int asset = value.IndexOf("LawnMowers.unity3d", StringComparison.Ordinal);
				attribute.Value = fixedBundle + value.Substring(asset + "LawnMowers.unity3d".Length);
			}
		}
	}

	// V2.1 car classes and dotted property names abort the whole XML file on 3.2.
	// The original files stay on disk. This rewrites the in-memory patch only.
	private static void AdaptForV3(XElement patchElement)
	{
		if (patchElement == null)
		{
			return;
		}

		foreach (XElement block in patchElement.Descendants("block").ToList())
		{
			AdaptDamagedTractorBlock(block);
		}

		foreach (XElement entity in patchElement.Descendants("entity_class").ToList())
		{
			AdaptEntityClass(entity);
		}

		foreach (XElement property in patchElement.Descendants("property").ToList())
		{
			AdaptDottedProperty(property);
		}
	}

	// 3.2 vehicles read LootList. A21 used LootListAlive, which leaves GetLootList empty
	// and EntityVehicle.getStorageSize null-refs when the tractor is placed.
	private static void AdaptEntityClass(XElement entity)
	{
		bool hasLoot = entity.Elements("property").Any(property => property.Attribute("name")?.Value == "LootList");
		if (hasLoot)
		{
			return;
		}

		XElement alive = entity.Elements("property").FirstOrDefault(property => property.Attribute("name")?.Value == "LootListAlive");
		if (alive != null)
		{
			alive.SetAttributeValue("name", "LootList");
		}
	}

	private static void AdaptDamagedTractorBlock(XElement block)
	{
		XElement classProperty = block.Elements("property").FirstOrDefault(property =>
			property.Attribute("name")?.Value == "Class"
			&& (property.Attribute("value")?.Value == "CarExplodeLoot" || property.Attribute("value")?.Value == "CarExplode"));
		if (classProperty == null)
		{
			return;
		}

		bool hasLoot = classProperty.Attribute("value")?.Value == "CarExplodeLoot";
		classProperty.SetAttributeValue("value", "CompositeTileEntity");

		XElement features = new XElement("property", new XAttribute("class", "CompositeFeatures"));
		XElement explosion = block.Elements("property").FirstOrDefault(property => property.Attribute("class")?.Value == "Explosion");
		if (explosion != null)
		{
			explosion.Remove();
			features.Add(new XElement("property", new XAttribute("class", "TEFeatureExplodable"), explosion));
		}

		XElement lootList = block.Elements("property").FirstOrDefault(property => property.Attribute("name")?.Value == "LootList");
		if (hasLoot && lootList != null)
		{
			string loot = lootList.Attribute("value")?.Value ?? "cars";
			lootList.Remove();
			features.Add(new XElement("property",
				new XAttribute("class", "TEFeatureStorage"),
				new XElement("property", new XAttribute("name", "LootList"), new XAttribute("value", loot))));
		}

		classProperty.AddAfterSelf(features);

		XElement multiBlock = block.Elements("property").FirstOrDefault(property => property.Attribute("name")?.Value == "MultiBlockDim");
		if (multiBlock != null && !block.Elements("property").Any(property => property.Attribute("name")?.Value == "OversizedBounds"))
		{
			multiBlock.Remove();
			features.AddAfterSelf(new XElement("property",
				new XAttribute("name", "OversizedBounds"),
				new XAttribute("value", "(-0.5,0,-0.5),(1.5,1.5,3.5)")));
		}
	}

	private static void AdaptDottedProperty(XElement property)
	{
		string name = property.Attribute("name")?.Value;
		if (string.IsNullOrEmpty(name))
		{
			return;
		}

		int dot = name.IndexOf('.');
		if (dot <= 0 || dot >= name.Length - 1 || name.IndexOf('.', dot + 1) >= 0)
		{
			return;
		}

		string className = name.Substring(0, dot);
		string innerName = name.Substring(dot + 1);
		string value = property.Attribute("value")?.Value ?? "";
		XElement parent = property.Parent;
		if (parent == null)
		{
			return;
		}

		XElement group = parent.Elements("property").FirstOrDefault(candidate =>
			candidate.Attribute("class")?.Value == className && candidate.Attribute("name") == null);
		if (group == null)
		{
			group = new XElement("property", new XAttribute("class", className));
			property.AddBeforeSelf(group);
		}

		group.Add(new XElement("property", new XAttribute("name", innerName), new XAttribute("value", value)));
		property.Remove();
	}

	private static bool ApplyEcho(XElement element)
	{
		string error = element.Attribute("error")?.Value;
		if (!string.IsNullOrEmpty(error))
		{
			Log.Error("[OcbLawnMowing] " + error);
			return true;
		}

		string message = element.Attribute("message")?.Value;
		if (string.IsNullOrEmpty(message))
		{
			message = element.Attribute("msg")?.Value;
		}
		if (!string.IsNullOrEmpty(message))
		{
			Log.Out("[OcbLawnMowing] " + message);
		}
		return true;
	}

	private static bool IsModLoaded(string modName)
	{
		if (string.IsNullOrEmpty(modName))
		{
			return false;
		}
		foreach (Mod mod in ModManager.GetLoadedMods())
		{
			if (string.Equals(mod.Name, modName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static Dictionary<string, string> CurrentScope()
	{
		return TemplateScopes.Count > 0 ? TemplateScopes.Peek() : EmptyScope;
	}
}
