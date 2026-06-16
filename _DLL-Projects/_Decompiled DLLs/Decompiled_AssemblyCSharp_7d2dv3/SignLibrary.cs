using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class SignLibrary
{
	public const int CurrentVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string VersionAttributeName = "version";

	public readonly Dictionary<Guid, SignData> signs = new Dictionary<Guid, SignData>();

	public void WriteXml(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement xmlElement = xmlDocument.AddXmlElement("signs");
		xmlElement.SetAttribute("version", 2.ToString());
		foreach (KeyValuePair<Guid, SignData> sign in signs)
		{
			sign.Value.WriteXml(xmlElement);
		}
		xmlDocument.SdSave(filePath);
	}

	public void ReadXml(XmlFile _xmlFile)
	{
		signs.Clear();
		XElement root = _xmlFile.XmlDoc.Root;
		int num = ReadVersion(root);
		foreach (XElement item in MigrateToCurrentVersion(root, num, _xmlFile).Elements(XNames.sign))
		{
			SignData signData = SignData.ReadXML(item);
			if (signs.ContainsKey(signData.guid))
			{
				Log.Error($"Duplicate sign guid '{signData.guid}' found in sign library: '{_xmlFile.Directory}/{_xmlFile.Filename}'");
			}
			else
			{
				signs[signData.guid] = signData;
			}
		}
		if (num < 2)
		{
			Log.Error("Out-of-date sign library loaded in build: " + _xmlFile.Directory + "/" + _xmlFile.Filename);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ReadVersion(XElement root)
	{
		XAttribute xAttribute = root.Attribute("version");
		if (xAttribute == null)
		{
			Log.Warning("Sign library has no version attribute, assuming legacy format (v0)");
			return 0;
		}
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			Log.Error("Invalid version attribute value: '" + xAttribute.Value + "', assuming legacy format (v0)");
			return 0;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XElement MigrateToCurrentVersion(XElement root, int fromVersion, XmlFile xmlFile)
	{
		if (fromVersion == 2)
		{
			return root;
		}
		if (fromVersion > 2)
		{
			Log.Error($"Sign library version {fromVersion} is newer than supported version {2}. " + "File: '" + xmlFile.Directory + "/" + xmlFile.Filename + "'. Data loss may occur!");
			return root;
		}
		Log.Out($"Migrating sign library from v{fromVersion} to v{2}: '{xmlFile.Directory}/{xmlFile.Filename}'");
		XElement xElement = root;
		for (int i = fromVersion; i < 2; i++)
		{
			xElement = SignLibraryMigrations.Migrate(xElement, i, i + 1);
		}
		return xElement;
	}
}
