using UnityEngine;

namespace Twitch;

public class ViewerEntry
{
	public float SpecialPoints;

	public float StandardPoints;

	public int BitCredits;

	public int UserID = -1;

	public string UserColor = "FFFFFF";

	public float LastAction = -1f;

	public float addPointsUntil;

	public bool IsActive;

	public bool IsSub;

	public float CombinedPoints => SpecialPoints + StandardPoints;

	public void RemovePoints(float usedPoints, TwitchAction.PointTypes pointType, TwitchActionEntry entry)
	{
		switch (pointType)
		{
		case TwitchAction.PointTypes.SP:
			SpecialPoints -= usedPoints;
			entry.SpecialPointsUsed = (int)usedPoints;
			break;
		case TwitchAction.PointTypes.PP:
		{
			float num3 = Mathf.Min(usedPoints, StandardPoints);
			entry.StandardPointsUsed = (int)num3;
			StandardPoints -= num3;
			num3 = usedPoints - num3;
			if (num3 > 0f)
			{
				SpecialPoints -= num3;
				entry.SpecialPointsUsed = (int)num3;
			}
			break;
		}
		case TwitchAction.PointTypes.Bits:
		{
			int num = Utils.FastMin((int)usedPoints, (ExtensionManager.Version == "2.0.1") ? BitCredits : TwitchAction.GetAdjustedBitPriceFloor(BitCredits));
			BitCredits -= num;
			entry.CreditsUsed = num;
			entry.BitsUsed = (int)usedPoints;
			TwitchLeaderboardStats leaderboardStats = TwitchManager.LeaderboardStats;
			int num2 = ((ExtensionManager.Version == "2.0.1") ? (entry.BitsUsed - num) : TwitchAction.GetAdjustedBitPriceCeil(entry.BitsUsed - num));
			if (num2 > 0)
			{
				leaderboardStats.TotalBits += num2;
				leaderboardStats.CheckMostBitsSpent(leaderboardStats.AddBitsUsed(entry.UserName, UserColor, num2));
			}
			break;
		}
		}
	}
}
