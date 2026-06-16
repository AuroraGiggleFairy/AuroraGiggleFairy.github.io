using System;
using UnityEngine.Scripting;

namespace Platform.PSN;

[Serializable]
[Preserve]
[DoNotTouchSerializableFlags]
public class UserIdentifierPSN : PlatformUserIdentifierAbs
{
	public override EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.PSN;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.PSN);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string ReadablePlatformUserIdentifier { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string CombinedString { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ulong AccountId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public UserIdentifierPSN(ulong _accountId)
	{
		AccountId = _accountId;
		ReadablePlatformUserIdentifier = AccountId.ToString();
		CombinedString = PlatformIdentifierString + "_" + ReadablePlatformUserIdentifier;
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
		if (!(_other is UserIdentifierPSN))
		{
			return false;
		}
		return AccountId == (_other as UserIdentifierPSN).AccountId;
	}

	public override int GetHashCode()
	{
		return AccountId.GetHashCode();
	}
}
