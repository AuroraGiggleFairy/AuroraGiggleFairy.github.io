using System;
using UnityEngine.Scripting;

namespace Platform.XBL;

[Serializable]
[Preserve]
[DoNotTouchSerializableFlags]
public class UserIdentifierXbl : PlatformUserIdentifierAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string pxuid;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int hashcode;

	public override EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.XBL;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.XBL);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string ReadablePlatformUserIdentifier { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string CombinedString { get; }

	public ulong Xuid => XblXuidMapper.GetXuid(this);

	public UserIdentifierXbl(string _pxuid)
	{
		pxuid = _pxuid;
		ReadablePlatformUserIdentifier = _pxuid;
		CombinedString = PlatformIdentifierString + "_" + _pxuid;
		hashcode = _pxuid.GetHashCode() ^ ((int)PlatformIdentifier * 397);
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
		if (!(_other is UserIdentifierXbl userIdentifierXbl))
		{
			return false;
		}
		return string.Equals(userIdentifierXbl.pxuid, pxuid, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		return hashcode;
	}
}
