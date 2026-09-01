using UnityEngine;

namespace PartyGroupPlus
{
	public static class PartyGroupPlusXP
	{
		public static int CountNearbyOthers(EntityPlayer killer)
		{
			if (killer == null || killer.Party == null)
			{
				return 0;
			}

			int range = GameStats.GetInt(EnumGameStats.PartySharedKillRange);
			int count = 0;
			for (int i = 0; i < killer.Party.MemberList.Count; i++)
			{
				EntityPlayer member = killer.Party.MemberList[i];
				if (member != null && member != killer
					&& Vector3.Distance(killer.position, member.position) < range)
				{
					count++;
				}
			}

			return count;
		}

		/// <summary>
		/// Vanilla taxes 10% per nearby other. At 7 others (8 in range) that is 30%.
		/// Extra bodies in range do not raise the tax further.
		/// </summary>
		public const int MaxVanillaTaxOthers = 7;

		public static int SplitKillXP(EntityPlayer killer, int startingXP)
		{
			if (startingXP <= 0)
			{
				return 0;
			}

			int nearbyOthers = CountNearbyOthers(killer);
			if (nearbyOthers > MaxVanillaTaxOthers)
			{
				nearbyOthers = MaxVanillaTaxOthers;
			}

			return (int)((float)startingXP * (1f - 0.1f * nearbyOthers));
		}
	}
}
