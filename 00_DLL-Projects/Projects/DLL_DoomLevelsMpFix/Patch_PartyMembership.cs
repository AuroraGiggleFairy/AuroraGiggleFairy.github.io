using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class PartyIds
	{
		private static readonly Dictionary<int, int[]> Members = new Dictionary<int, int[]>();
		private static readonly Dictionary<int, int> Leaders = new Dictionary<int, int>();
		private static readonly Dictionary<int, string> Names = new Dictionary<int, string>();

		internal static void Save(Party party, int[] ids)
		{
			if (party == null || ids == null)
			{
				return;
			}

			int[] copy = new int[ids.Length];
			Array.Copy(ids, copy, ids.Length);
			Members[party.PartyID] = copy;
			int leader = -1;
			if (party.LeaderIndex >= 0 && party.LeaderIndex < ids.Length)
			{
				leader = ids[party.LeaderIndex];
			}
			else if (Leaders.TryGetValue(party.PartyID, out int previous))
			{
				leader = previous;
			}

			if (leader > 0)
			{
				Leaders[party.PartyID] = leader;
			}
		}

		internal static int[] For(Party party)
		{
			if (party != null && Members.TryGetValue(party.PartyID, out int[] ids) && ids != null && ids.Length > 0)
			{
				return ids;
			}

			return party?.GetMemberIdArray();
		}

		internal static int LeaderOf(Party party)
		{
			if (party != null && Leaders.TryGetValue(party.PartyID, out int id))
			{
				return id;
			}

			return -1;
		}

		internal static void RememberName(EntityPlayer player)
		{
			if (player == null || string.IsNullOrEmpty(player.PlayerDisplayName))
			{
				return;
			}

			Names[player.entityId] = player.PlayerDisplayName;
		}

		internal static string NameOf(int entityId)
		{
			if (Names.TryGetValue(entityId, out string cached) && !string.IsNullOrEmpty(cached))
			{
				return cached;
			}

			PersistentPlayerData data = GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(entityId);
			string name = data?.PlayerName?.DisplayName;
			if (!string.IsNullOrEmpty(name))
			{
				Names[entityId] = name;
				return name;
			}

			return null;
		}

		internal static int IndexOf(Party party, int entityId)
		{
			int[] ids = For(party);
			if (ids == null)
			{
				return -1;
			}

			for (int i = 0; i < ids.Length; i++)
			{
				if (ids[i] == entityId)
				{
					return i;
				}
			}

			return -1;
		}

		internal static Color32 ColorOf(Party party, int entityId)
		{
			int index = IndexOf(party, entityId);
			if (index < 0)
			{
				index = 0;
			}

			return Constants.TrackedFriendColors[index % Constants.TrackedFriendColors.Length];
		}

		internal static void Clear()
		{
			Members.Clear();
			Leaders.Clear();
			Names.Clear();
		}
	}

	internal static class PartyMembers
	{
		internal static EntityPlayer Find(World world, int entityId)
		{
			if (world == null || entityId <= 0)
			{
				return null;
			}

			if (world.Players?.dict != null && world.Players.dict.TryGetValue(entityId, out EntityPlayer mapped) && mapped != null)
			{
				return mapped;
			}

			if (world.Players?.list != null)
			{
				for (int i = 0; i < world.Players.list.Count; i++)
				{
					EntityPlayer player = world.Players.list[i];
					if (player != null && player.entityId == entityId)
					{
						return player;
					}
				}
			}

			return world.GetEntity(entityId) as EntityPlayer;
		}

		internal static void SyncLive(Party party, int[] ids, World world)
		{
			if (party?.MemberList == null || ids == null)
			{
				return;
			}

			List<EntityPlayer> next = new List<EntityPlayer>(ids.Length);
			for (int i = 0; i < ids.Length; i++)
			{
				EntityPlayer player = Find(world, ids[i]);
				if (player == null)
				{
					continue;
				}

				player.Party = party;
				PartyIds.RememberName(player);
				if (player.NavObject != null)
				{
					player.NavObject.UseOverrideColor = true;
					player.NavObject.OverrideColor = PartyIds.ColorOf(party, player.entityId);
					player.NavObject.name = player.PlayerDisplayName;
				}

				next.Add(player);
			}

			party.MemberList.Clear();
			party.MemberList.AddRange(next);
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.UpdateMemberList))]
	internal static class Patch_Party_UpdateMemberList
	{
		private static bool Prefix(Party __instance, World world, int[] partyMembers)
		{
			if (__instance?.MemberList == null || partyMembers == null || world == null)
			{
				return false;
			}

			int leaderId = -1;
			if (__instance.LeaderIndex >= 0 && __instance.LeaderIndex < partyMembers.Length)
			{
				leaderId = partyMembers[__instance.LeaderIndex];
			}

			PartyIds.Save(__instance, partyMembers);

			EntityPlayerLocal local = null;
			List<EntityPlayerLocal> locals = GameManager.Instance?.World?.GetLocalPlayers();
			if (locals != null && locals.Count > 0)
			{
				local = locals[0];
			}

			List<EntityPlayer> leaving = new List<EntityPlayer>();
			for (int i = 0; i < __instance.MemberList.Count; i++)
			{
				EntityPlayer member = __instance.MemberList[i];
				if (member == null)
				{
					continue;
				}

				bool stay = false;
				for (int j = 0; j < partyMembers.Length; j++)
				{
					if (member.entityId == partyMembers[j])
					{
						stay = true;
						break;
					}
				}

				if (!stay)
				{
					leaving.Add(member);
				}
			}

			for (int i = 0; i < leaving.Count; i++)
			{
				EntityPlayer member = leaving[i];
				member.Party = null;
				member.IsInPartyOfLocalPlayer = false;
				member.HandleOnPartyLeave(__instance);
				if (member.NavObject != null && member != local)
				{
					member.NavObject.UseOverrideColor = false;
				}

				if (local != null && local.Party == __instance && local.QuestJournal != null)
				{
					local.QuestJournal.RemoveSharedQuestForOwner(member.entityId);
					local.QuestJournal.RemoveSharedQuestEntryByOwner(member.entityId);
				}
			}

			List<EntityPlayer> kept = new List<EntityPlayer>(__instance.MemberList);
			__instance.MemberList.Clear();
			bool inLocalParty = false;
			for (int m = 0; m < partyMembers.Length; m++)
			{
				EntityPlayer found = null;
				for (int n = 0; n < kept.Count; n++)
				{
					EntityPlayer existing = kept[n];
					if (existing != null && existing.entityId == partyMembers[m])
					{
						found = existing;
						kept.RemoveAt(n);
						break;
					}
				}

				if (found == null)
				{
					found = PartyMembers.Find(world, partyMembers[m]);
				}

				if (found == null)
				{
					continue;
				}

				PartyIds.RememberName(found);
				__instance.MemberList.Add(found);
				found.Party = __instance;
				if (found is EntityPlayerLocal)
				{
					inLocalParty = true;
				}

				found.RemoveAllPartyInvites();
				PartyInvites.Forget(found.entityId);
			}

			if (leaderId > 0)
			{
				int at = __instance.MemberList.Count;
				for (int i = 0; i < __instance.MemberList.Count; i++)
				{
					EntityPlayer member = __instance.MemberList[i];
					if (member != null && member.entityId == leaderId)
					{
						at = i;
						break;
					}
				}

				__instance.LeaderIndex = at;
			}

			for (int num = 0; num < __instance.MemberList.Count; num++)
			{
				EntityPlayer member = __instance.MemberList[num];
				if (member == null)
				{
					continue;
				}

				if (local != null && num != __instance.LeaderIndex)
				{
					local.RemovePartyInvite(member.entityId);
				}

				if (member.NavObject != null && local != null && local.Party == member.Party)
				{
					member.NavObject.UseOverrideColor = true;
					member.NavObject.OverrideColor = PartyIds.ColorOf(__instance, member.entityId);
					member.NavObject.name = member.PlayerDisplayName;
				}

				member.IsInPartyOfLocalPlayer = inLocalParty;
				member.HandleOnPartyJoined();
			}

			if (local != null)
			{
				for (int i = 0; i < partyMembers.Length; i++)
				{
					if (partyMembers[i] == local.entityId)
					{
						local.Party = __instance;
						break;
					}
				}
			}

			PartyHud.RequestRefresh();
			return false;
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.IsPartyLead))]
	internal static class Patch_IsPartyLead
	{
		private static void Postfix(EntityPlayer __instance, ref bool __result)
		{
			if (__instance == null)
			{
				return;
			}

			int leaderId = PartyIds.LeaderOf(__instance.Party);
			if (leaderId > 0)
			{
				__result = __instance.entityId == leaderId;
				return;
			}

			if (__result || __instance.Party?.MemberList == null)
			{
				return;
			}

			int index = __instance.Party.LeaderIndex;
			if (index < 0 || index >= __instance.Party.MemberList.Count)
			{
				return;
			}

			EntityPlayer leader = __instance.Party.MemberList[index];
			__result = leader != null && leader.entityId == __instance.entityId;
		}
	}
}
