using System;
using System.Text.RegularExpressions;
using Epic.OnlineServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Platform.EOS;

[Serializable]
[Preserve]
[DoNotTouchSerializableFlags]
public class UserIdentifierEos : PlatformUserIdentifierAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex puidMatcher = new Regex("^[0-9a-fA-F]{8,32}$", RegexOptions.Compiled);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string ticket;

	[PublicizedFrom(EAccessModifier.Private)]
	public string productUserIdString;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ProductUserId productUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int hashcode;

	public override EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.EOS;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string PlatformIdentifierString { get; } = PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS);

	public override string ReadablePlatformUserIdentifier => ProductUserIdString;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public override string CombinedString { get; }

	public string ProductUserIdString => productUserIdString ?? (productUserIdString = CreateStringFromPuid(productUserId));

	public ProductUserId ProductUserId => productUserId ?? (productUserId = CreatePuidFromString(productUserIdString));

	public string Ticket
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

	public static string CreateCombinedString(string _puidString)
	{
		return PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS) + "_" + _puidString;
	}

	public static string CreateCombinedString(ProductUserId _puid)
	{
		return PlatformManager.PlatformStringFromEnum(EPlatformIdentifier.EOS) + "_" + CreateStringFromPuid(_puid);
	}

	public UserIdentifierEos(string _puid)
	{
		if (string.IsNullOrEmpty(_puid))
		{
			throw new ArgumentException("Empty or null PUID", "_puid");
		}
		if (!puidMatcher.IsMatch(_puid))
		{
			throw new ArgumentException("Invalid PUID '" + _puid + "'", "_puid");
		}
		productUserIdString = _puid;
		CombinedString = CreateCombinedString(_puid);
		hashcode = ProductUserIdString.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public UserIdentifierEos(ProductUserId _puid)
	{
		if (_puid == null)
		{
			throw new ArgumentException("Null PUID", "_puid");
		}
		productUserId = _puid;
		CombinedString = CreateCombinedString(ProductUserIdString);
		hashcode = ProductUserIdString.GetHashCode() ^ ((int)PlatformIdentifier * 397);
	}

	public static string CreateStringFromPuid(ProductUserId _puid)
	{
		if (!ThreadManager.IsMainThread())
		{
			Log.Warning("CreateStringFromPuid NOT ON MAIN THREAD! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
		}
		if (_puid == null)
		{
			Log.Error("CreateStringFromPuid with null PUID! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
			return null;
		}
		return _puid.ToString();
	}

	public static ProductUserId CreatePuidFromString(string _puidString)
	{
		if (_puidString == null)
		{
			Log.Error("CreatePuidFromString with null PUID string! From:\n" + StackTraceUtility.ExtractStackTrace() + "\n");
			return null;
		}
		return ProductUserId.FromString(_puidString);
	}

	public override bool DecodeTicket(string _ticket)
	{
		if (string.IsNullOrEmpty(_ticket))
		{
			return false;
		}
		Ticket = _ticket;
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
		if (!(_other is UserIdentifierEos userIdentifierEos))
		{
			return false;
		}
		return string.Equals(userIdentifierEos.ProductUserIdString, ProductUserIdString, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		return hashcode;
	}
}
