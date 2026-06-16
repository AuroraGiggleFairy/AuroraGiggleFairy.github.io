using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

namespace Platform.EOS;

public class PlayerReporting : IPlayerReporting
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class PlayerReportCategoryEos : IPlayerReporting.PlayerReportCategory
	{
		public readonly PlayerReportsCategory Category;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayString;

		public PlayerReportCategoryEos(PlayerReportsCategory _category, string _displayString)
		{
			Category = _category;
			displayString = _displayString;
		}

		public override string ToString()
		{
			return displayString;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory> reportCategories;

	public ReportsInterface reportsInterface
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((Api)owner.Api).PlatformInterface.GetReportsInterface();
		}
	}

	public ProductUserId localProductUserId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId;
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public IList<IPlayerReporting.PlayerReportCategory> ReportCategories()
	{
		if (reportCategories != null)
		{
			return reportCategories.list;
		}
		reportCategories = new DictionaryList<PlayerReportsCategory, IPlayerReporting.PlayerReportCategory>();
		foreach (PlayerReportsCategory item in EnumUtils.Values<PlayerReportsCategory>())
		{
			if (item != PlayerReportsCategory.Invalid)
			{
				reportCategories.Add(item, new PlayerReportCategoryEos(item, Localization.Get("xuiCategoryPlayerReport" + item.ToStringCached())));
			}
		}
		return reportCategories.list;
	}

	public void ReportPlayer(PlatformUserIdentifierAbs _reportedUserCross, IPlayerReporting.PlayerReportCategory _reportCategory, string _message, Action<bool> _reportCompleteCallback)
	{
		if (_message != null && _message.Length > 256)
		{
			Log.Out("[EOS-Report] Long message, might get truncated");
		}
		EosHelpers.AssertMainThread("PRep.Send");
		SendPlayerBehaviorReportOptions options = new SendPlayerBehaviorReportOptions
		{
			ReporterUserId = localProductUserId,
			ReportedUserId = ((UserIdentifierEos)_reportedUserCross).ProductUserId,
			Category = ((PlayerReportCategoryEos)_reportCategory).Category,
			Message = _message
		};
		lock (AntiCheatCommon.LockObject)
		{
			reportsInterface.SendPlayerBehaviorReport(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref SendPlayerBehaviorReportCompleteCallbackInfo _callbackData) =>
			{
				if (_callbackData.ResultCode != Result.Success)
				{
					Log.Error("[EOS-Report] Reporting player failed: " + _callbackData.ResultCode.ToStringCached());
					_reportCompleteCallback(obj: false);
				}
				else
				{
					Log.Out("[EOS-Report] Sent player report");
					_reportCompleteCallback(obj: true);
				}
			});
		}
	}

	public IPlayerReporting.PlayerReportCategory GetPlayerReportCategoryMapping(EnumReportCategory _reportCategory)
	{
		if (!reportCategories.dict.TryGetValue(_reportCategory switch
		{
			EnumReportCategory.Cheating => PlayerReportsCategory.Cheating, 
			EnumReportCategory.VerbalAbuse => PlayerReportsCategory.VerbalAbuse, 
			_ => PlayerReportsCategory.Other, 
		}, out var value))
		{
			return null;
		}
		return value;
	}
}
