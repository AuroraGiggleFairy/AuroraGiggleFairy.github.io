using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct PlayerMetaInfo(PlatformUserIdentifierAbs nativeId, string name, int level, float distanceWalked)
{
	public const string EXT = "meta";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string nativeIdAttr = "nativeid";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string nameAttr = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string levelAttr = "level";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string distanceWalkedAttr = "distanceWalked";

	public readonly PlatformUserIdentifierAbs nativeId = nativeId;

	public readonly string name = name;

	public readonly int level = level;

	public readonly float distanceWalked = distanceWalked;

	public static bool TryRead(string filePath, out PlayerMetaInfo playerMetaInfo)
	{
		if (!SdFile.Exists(filePath))
		{
			Debug.LogError("Failed to read PlayerMetaInfo. No file found at path: " + filePath);
			playerMetaInfo = default(PlayerMetaInfo);
			return false;
		}
		try
		{
			XElement root = SdXDocument.Load(filePath).Root;
			if (root == null)
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + filePath + "\". Could not find root node.");
				playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("name", out var _result))
			{
				Debug.LogWarning("No name in PlayerMetaInfo at path \"" + filePath + "\". Could not find name attribute.");
				_result = null;
			}
			PlatformUserIdentifierAbs _userIdentifier;
			if (!root.TryGetAttribute("nativeid", out var _result2))
			{
				Debug.LogWarning("No native id in PlayerMetaInfo at path \"" + filePath + "\". Could not find nativeid attribute.");
				_userIdentifier = null;
			}
			else if (!PlatformUserIdentifierAbs.TryFromCombinedString(_result2, out _userIdentifier))
			{
				Debug.LogError("Could not parse native id from PlayerMetaInfo at path \"" + filePath + "\". Combined id string: " + _result2);
				playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("level", out var _result3) || !int.TryParse(_result3, out var result))
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + filePath + "\". Could not find level attribute.");
				playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("distanceWalked", out var _result4) || !float.TryParse(_result4, out var result2))
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + filePath + "\". Could not find distanceWalked attribute.");
				playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			playerMetaInfo = new PlayerMetaInfo(_userIdentifier, _result, result, result2);
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to read PlayerMetaInfo at path \"{filePath}\". Failed with exception: \n\n{arg}");
			playerMetaInfo = default(PlayerMetaInfo);
			return false;
		}
	}

	public void Write(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement xmlElement = xmlDocument.AddXmlElement("PlayerMetaInfo");
		if (nativeId != null)
		{
			xmlElement.SetAttribute("nativeid", nativeId.CombinedString);
		}
		if (name != null)
		{
			xmlElement.SetAttribute("name", name);
		}
		int num = level;
		xmlElement.SetAttribute("level", num.ToString());
		float num2 = distanceWalked;
		xmlElement.SetAttribute("distanceWalked", num2.ToString());
		xmlDocument.SdSave(filePath);
	}
}
