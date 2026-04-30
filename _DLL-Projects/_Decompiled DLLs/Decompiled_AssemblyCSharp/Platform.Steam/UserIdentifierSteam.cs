using System;
using Steamworks;
using UnityEngine.Scripting;

namespace Platform.Steam;

[Serializable]
[Preserve]
[DoNotTouchSerializableFlags]
public class UserIdentifierSteam : PlatformUserIdentifierAbs
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] ticket;

	public UserIdentifierSteam OwnerId;

	public readonly ulong SteamId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int hashcode;

	public override EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.Steam;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.Steam);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string ReadablePlatformUserIdentifier { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string CombinedString { get; }

	public byte[] Ticket
	{
		get
		{
			return ticket;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			ticket = value;
		}
	}

	public UserIdentifierSteam(string _steamId)
	{
		if (_steamId.Length != 17 || !ulong.TryParse(_steamId, out var result))
		{
			throw new ArgumentException("Not a valid SteamID: " + _steamId, "_steamId");
		}
		SteamId = result;
		ReadablePlatformUserIdentifier = _steamId;
		CombinedString = PlatformIdentifierString + "_" + _steamId;
		hashcode = result.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public UserIdentifierSteam(ulong _steamId)
	{
		if (_steamId < 10000000000000000L || _steamId > 99999999999999999L)
		{
			throw new ArgumentException("Not a valid SteamID: " + _steamId, "_steamId");
		}
		SteamId = _steamId;
		ReadablePlatformUserIdentifier = _steamId.ToString();
		CombinedString = PlatformIdentifierString + "_" + _steamId;
		hashcode = _steamId.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public UserIdentifierSteam(CSteamID _steamId)
	{
		SteamId = _steamId.m_SteamID;
		ReadablePlatformUserIdentifier = _steamId.ToString();
		string platformIdentifierString = PlatformIdentifierString;
		CSteamID cSteamID = _steamId;
		CombinedString = platformIdentifierString + "_" + cSteamID.ToString();
		hashcode = _steamId.m_SteamID.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public override bool DecodeTicket(string _ticket)
	{
		if (string.IsNullOrEmpty(_ticket))
		{
			return false;
		}
		try
		{
			Ticket = Convert.FromBase64String(_ticket);
		}
		catch (FormatException ex)
		{
			Log.Error("Convert.FromBase64String: " + ex.Message);
			Log.Exception(ex);
			return false;
		}
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
		if (!(_other is UserIdentifierSteam userIdentifierSteam))
		{
			return false;
		}
		return userIdentifierSteam.SteamId == SteamId;
	}

	public override int GetHashCode()
	{
		return hashcode;
	}
}
