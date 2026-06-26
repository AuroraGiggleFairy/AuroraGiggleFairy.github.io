using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct RemoteWorldInfo(string gameName, string worldName, VersionInformation gameVersion, long saveSize)
{
	public const string FileNameString = "RemoteWorldInfo.xml";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameNameString = "gameName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string WorldNameString = "worldName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameVersionString = "gameVersion";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SaveSizeString = "saveSize";

	public readonly string gameName = gameName;

	public readonly string worldName = worldName;

	public readonly VersionInformation gameVersion = gameVersion;

	public readonly long saveSize = saveSize;

	public static bool TryRead(string filePath, out RemoteWorldInfo remoteWorldInfo)
	{
		if (!SdFile.Exists(filePath))
		{
			remoteWorldInfo = default(RemoteWorldInfo);
			return false;
		}
		try
		{
			XElement root = SdXDocument.Load(filePath).Root;
			if (root == null)
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find root node.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!root.TryGetAttribute("gameName", out var _result))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find gameName attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!root.TryGetAttribute("worldName", out var _result2))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find worldName attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!root.TryGetAttribute("gameVersion", out var _result3))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find gameVersion attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!VersionInformation.TryParseSerializedString(_result3, out var _result4))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Failed to parse gameVersion value.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!root.TryGetAttribute("saveSize", out var _result5))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find saveSize attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			if (!long.TryParse(_result5, out var result))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Failed to parse saveSize value.");
				remoteWorldInfo = default(RemoteWorldInfo);
				return false;
			}
			remoteWorldInfo = new RemoteWorldInfo(_result, _result2, _result4, result);
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to read RemoteWorldInfo at path \"{filePath}\". Failed with exception: \n\n{arg}");
			remoteWorldInfo = default(RemoteWorldInfo);
			return false;
		}
	}

	public void Write(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement element = xmlDocument.AddXmlElement("RemoteWorldInfo");
		element.SetAttrib("gameName", gameName);
		element.SetAttrib("worldName", worldName);
		element.SetAttrib("gameVersion", gameVersion.SerializableString);
		long num = saveSize;
		element.SetAttrib("saveSize", num.ToString());
		xmlDocument.SdSave(filePath);
	}
}
