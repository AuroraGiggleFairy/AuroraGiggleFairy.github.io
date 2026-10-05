using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class PartyRange
	{
		internal static bool TreatAsNear(EntityPlayer a, EntityPlayer b)
		{
			if (a == null || b == null)
			{
				return false;
			}

			return PartyHud.InLevel(a) || PartyHud.InLevel(b);
		}
	}

	[HarmonyPatch(typeof(Party), "MemberCountInRange")]
	internal static class Patch_Party_MemberCountInRange
	{
		private static bool Prefix(EntityPlayer player, ref int __result)
		{
			if (player?.Party?.MemberList == null)
			{
				__result = 0;
				return false;
			}

			int range = GameStats.GetInt(EnumGameStats.PartySharedKillRange);
			int count = 0;
			for (int i = 0; i < player.Party.MemberList.Count; i++)
			{
				EntityPlayer other = player.Party.MemberList[i];
				if (other == null || other == player)
				{
					continue;
				}

				if (PartyRange.TreatAsNear(player, other) || Vector3.Distance(player.position, other.position) < range)
				{
					count++;
				}
			}

			__result = count;
			return false;
		}
	}

	[HarmonyPatch(typeof(Party), "MemberCountNotInRange")]
	internal static class Patch_Party_MemberCountNotInRange
	{
		private static bool Prefix(EntityPlayer player, ref int __result)
		{
			if (player?.Party?.MemberList == null)
			{
				__result = 0;
				return false;
			}

			int count = 0;
			for (int i = 0; i < player.Party.MemberList.Count; i++)
			{
				EntityPlayer other = player.Party.MemberList[i];
				if (other == null || other == player)
				{
					continue;
				}

				if (PartyRange.TreatAsNear(player, other))
				{
					continue;
				}

				if (Vector3.Distance(player.position, other.position) >= 15f)
				{
					count++;
				}
			}

			__result = count;
			return false;
		}
	}
}
