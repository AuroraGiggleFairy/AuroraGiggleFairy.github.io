using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct PlayerMetaInfo(PlatformUserIdentifierAbs _nativeId, string _name, int _level, float _distanceWalked)
{
	public const string Ext = "meta";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string NativeIdAttr = "nativeid";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string NameAttr = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LevelAttr = "level";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DistanceWalkedAttr = "distanceWalked";

	public readonly PlatformUserIdentifierAbs NativeId = _nativeId;

	public readonly string Name = _name;

	public readonly int Level = _level;

	public readonly float DistanceWalked = _distanceWalked;

	public static bool TryRead(string _filePath, out PlayerMetaInfo _playerMetaInfo)
	{
		if (!SdFile.Exists(_filePath))
		{
			Debug.LogError("Failed to read PlayerMetaInfo. No file found at path: " + _filePath);
			_playerMetaInfo = default(PlayerMetaInfo);
			return false;
		}
		try
		{
			XElement root = SdXDocument.Load(_filePath).Root;
			if (root == null)
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + _filePath + "\". Could not find root node.");
				_playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("name", out var _result))
			{
				Debug.LogWarning("No name in PlayerMetaInfo at path \"" + _filePath + "\". Could not find name attribute.");
				_result = null;
			}
			PlatformUserIdentifierAbs _userIdentifier;
			if (!root.TryGetAttribute("nativeid", out var _result2))
			{
				Debug.LogWarning("No native id in PlayerMetaInfo at path \"" + _filePath + "\". Could not find nativeid attribute.");
				_userIdentifier = null;
			}
			else if (!PlatformUserIdentifierAbs.TryFromCombinedString(_result2, out _userIdentifier))
			{
				Debug.LogError("Could not parse native id from PlayerMetaInfo at path \"" + _filePath + "\". Combined id string: " + _result2);
				_playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("level", out var _result3) || !int.TryParse(_result3, out var result))
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + _filePath + "\". Could not find level attribute.");
				_playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			if (!root.TryGetAttribute("distanceWalked", out var _result4) || !float.TryParse(_result4, out var result2))
			{
				Debug.LogError("Failed to read PlayerMetaInfo at path \"" + _filePath + "\". Could not find distanceWalked attribute.");
				_playerMetaInfo = default(PlayerMetaInfo);
				return false;
			}
			_playerMetaInfo = new PlayerMetaInfo(_userIdentifier, _result, result, result2);
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to read PlayerMetaInfo at path \"{_filePath}\". Failed with exception: \n\n{arg}");
			_playerMetaInfo = default(PlayerMetaInfo);
			return false;
		}
	}

	public void Write(string _filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement xmlElement = xmlDocument.AddXmlElement("PlayerMetaInfo");
		if (NativeId != null)
		{
			xmlElement.SetAttribute("nativeid", NativeId.CombinedString);
		}
		if (Name != null)
		{
			xmlElement.SetAttribute("name", Name);
		}
		int level = Level;
		xmlElement.SetAttribute("level", level.ToString());
		float distanceWalked = DistanceWalked;
		xmlElement.SetAttribute("distanceWalked", distanceWalked.ToString());
		xmlDocument.SdSave(_filePath);
	}

	public static PlayerMetaInfo FromStream(PooledBinaryReader _reader)
	{
		PlatformUserIdentifierAbs nativeId = null;
		if (_reader.ReadBoolean())
		{
			nativeId = PlatformUserIdentifierAbs.FromStream(_reader);
		}
		string name = null;
		if (_reader.ReadBoolean())
		{
			name = _reader.ReadString();
		}
		int level = _reader.ReadInt32();
		float distanceWalked = _reader.ReadSingle();
		return new PlayerMetaInfo(nativeId, name, level, distanceWalked);
	}

	public void Write(PooledBinaryWriter _writer)
	{
		bool flag = NativeId != null;
		_writer.Write(flag);
		if (flag)
		{
			NativeId.ToStream(_writer);
		}
		bool flag2 = Name != null;
		_writer.Write(flag2);
		if (flag2)
		{
			_writer.Write(Name);
		}
		_writer.Write(Level);
		_writer.Write(DistanceWalked);
	}
}
