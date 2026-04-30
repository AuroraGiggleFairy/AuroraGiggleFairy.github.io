using System;
using System.Collections.Generic;

namespace Platform;

public interface IPlayerReporting
{
	public abstract class PlayerReportCategory
	{
		public abstract override string ToString();

		[PublicizedFrom(EAccessModifier.Protected)]
		public PlayerReportCategory()
		{
		}
	}

	void Init(IPlatform _owner);

	IList<PlayerReportCategory> ReportCategories();

	void ReportPlayer(PlatformUserIdentifierAbs _reportedUserCross, PlayerReportCategory _reportCategory, string _message, Action<bool> _reportCompleteCallback);

	PlayerReportCategory GetPlayerReportCategoryMapping(EnumReportCategory _reportCategory);
}
