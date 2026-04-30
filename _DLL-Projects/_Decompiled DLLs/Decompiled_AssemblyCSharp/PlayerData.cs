using System.IO;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class PlayerData
{
	public readonly IPlatformUserData PlatformData;

	public readonly PlatformUserIdentifierAbs PrimaryId;

	public readonly PlatformUserIdentifierAbs NativeId;

	public readonly EPlayGroup PlayGroup;

	public readonly AuthoredText PlayerName;

	public PlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		PrimaryId = _primaryId;
		NativeId = _nativeId;
		PlayGroup = _playGroup;
		PlayerName = _playerName;
		PlatformData = PlatformUserManager.GetOrCreate(_primaryId);
		PlatformData.NativeId = _nativeId;
	}

	public static PlayerData Read(BinaryReader _reader)
	{
		PlatformUserIdentifierAbs primaryId = PlatformUserIdentifierAbs.FromStream(_reader);
		PlatformUserIdentifierAbs nativeId = PlatformUserIdentifierAbs.FromStream(_reader);
		AuthoredText authoredText = AuthoredText.FromStream(_reader);
		EPlayGroup playGroup = (EPlayGroup)_reader.ReadByte();
		GeneratedTextManager.PrefilterText(authoredText);
		return new PlayerData(primaryId, nativeId, authoredText, playGroup);
	}

	public void Write(BinaryWriter _writer)
	{
		PrimaryId.ToStream(_writer);
		NativeId.ToStream(_writer);
		AuthoredText.ToStream(PlayerName, _writer);
		_writer.Write((byte)PlayGroup);
	}
}
