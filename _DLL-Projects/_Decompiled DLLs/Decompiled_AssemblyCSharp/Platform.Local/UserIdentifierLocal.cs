using System;
using UnityEngine.Scripting;

namespace Platform.Local;

[Serializable]
[Preserve]
[DoNotTouchSerializableFlags]
public class UserIdentifierLocal : PlatformUserIdentifierAbs
{
	public readonly string PlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int hashcode;

	public override EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.Local;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.Local);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string ReadablePlatformUserIdentifier { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string CombinedString { get; }

	public UserIdentifierLocal(string _playername)
	{
		if (string.IsNullOrEmpty(_playername))
		{
			throw new ArgumentException("Playername must not be empty", "_playername");
		}
		PlayerName = _playername;
		ReadablePlatformUserIdentifier = _playername;
		CombinedString = PlatformIdentifierString + "_" + _playername;
		hashcode = _playername.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public override bool DecodeTicket(string _ticket)
	{
		return true;
	}

	public override bool Equals(PlatformUserIdentifierAbs _other)
	{
		if (_other == null)
		{
			return false;
		}
		if (this == _other)
		{
			return true;
		}
		if (!(_other is UserIdentifierLocal userIdentifierLocal))
		{
			return false;
		}
		return userIdentifierLocal.PlayerName == PlayerName;
	}

	public override int GetHashCode()
	{
		return hashcode;
	}
}
