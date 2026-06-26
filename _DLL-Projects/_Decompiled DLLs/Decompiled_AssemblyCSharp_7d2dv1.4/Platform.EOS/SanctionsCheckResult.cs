using System;
using System.Collections.Generic;

namespace Platform.EOS;

[PublicizedFrom(EAccessModifier.Internal)]
public struct SanctionsCheckResult
{
	public GameUtils.KickPlayerData KickReason;

	public string ReasonForSanction;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public DateTime LongestExpiry { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasActiveSanctions { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Success { get; }

	public SanctionsCheckResult(List<EOSSanction> sanctions)
	{
		Success = true;
		LongestExpiry = DateTime.MinValue;
		ReasonForSanction = string.Empty;
		KickReason = default(GameUtils.KickPlayerData);
		if (sanctions == null || sanctions.Count == 0)
		{
			HasActiveSanctions = false;
			return;
		}
		HasActiveSanctions = true;
		EOSSanction eOSSanction = sanctions[0];
		foreach (EOSSanction sanction in sanctions)
		{
			if (sanction.expiry == DateTime.MaxValue || sanction.expiry == default(DateTime))
			{
				Log.Out("[EOS] Sanctioned Until: Forever");
				ReasonForSanction = GetReasonMessage(default(DateTime), GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, "Sanction: [" + sanction.ReferenceId + "]");
				ReasonForSanction = string.Format(Localization.Get("auth_banned_forever"));
				KickReason = default(GameUtils.KickPlayerData);
				LongestExpiry = DateTime.MaxValue;
				break;
			}
			if (sanction.expiry > eOSSanction.expiry)
			{
				eOSSanction = sanction;
			}
		}
		Log.Out("[EOS] Sanctioned Until: " + LongestExpiry.ToLongDateString());
		ReasonForSanction = GetReasonMessage(eOSSanction.expiry, GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, "Sanction: [" + eOSSanction.ReferenceId + "]");
		KickReason = new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, eOSSanction.expiry, "Sanction: [" + eOSSanction.ReferenceId + "]");
		LongestExpiry = eOSSanction.expiry;
	}

	public SanctionsCheckResult(DateTime banUntil, GameUtils.EKickReason reason, int apiResponseEnum, string customReason)
	{
		KickReason = new GameUtils.KickPlayerData(reason, apiResponseEnum, banUntil, customReason);
		LongestExpiry = default(DateTime);
		HasActiveSanctions = false;
		ReasonForSanction = GetReasonMessage(banUntil, reason, apiResponseEnum, customReason);
		Success = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetReasonMessage(DateTime banUntil, GameUtils.EKickReason reason, int apiResponseEnum, string customReason)
	{
		return reason switch
		{
			GameUtils.EKickReason.CrossPlatformAuthenticationFailed => GetAuthFailedMessage(banUntil, apiResponseEnum, customReason), 
			GameUtils.EKickReason.Banned => BannedMessage(banUntil, customReason), 
			GameUtils.EKickReason.PlatformAuthenticationFailed => GetAuthFailedMessage(banUntil, apiResponseEnum, customReason), 
			_ => Localization.Get("auth_unknown"), 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetAuthFailedMessage(DateTime banUntil, int apiResponseEnum, string customReason)
	{
		switch (apiResponseEnum)
		{
		case 1:
		case 2:
		case 3:
		case 4:
		case 6:
		case 7:
		case 8:
			return string.Format(Localization.Get("platformauth_" + ((EUserAuthenticationResult)apiResponseEnum).ToStringCached()), PlatformManager.NativePlatform.PlatformDisplayName);
		case 5:
			return string.Format(Localization.Get("auth_timeout"), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
		case 9:
			return BannedMessage(banUntil, customReason);
		default:
			return Localization.Get("auth_unknown");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BannedMessage(DateTime banUntil, string customReason)
	{
		if (!(banUntil == default(DateTime)) && !(banUntil == DateTime.MaxValue))
		{
			return string.Format("\n" + Localization.Get("auth_sanctioned"), banUntil.ToCultureInvariantString());
		}
		return Localization.Get("auth_sanctioned_forever");
	}
}
