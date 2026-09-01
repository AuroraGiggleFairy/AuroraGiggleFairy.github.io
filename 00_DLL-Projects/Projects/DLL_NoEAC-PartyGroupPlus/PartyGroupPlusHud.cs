using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PartyGroupPlus
{
	public static class PartyGroupPlusHud
	{
		public const float RefreshSeconds = 10f;
		public const int BandCount = 4;

		static readonly float[] BandMaxMeters = { 150f, 500f, 1000f };

		public static float NextRefreshTime;

		static bool ColorHooked;
		static readonly FieldInfo EntryListField = AccessTools.Field(typeof(XUiC_PartyEntryList), "entryList");
		static readonly List<EntityPlayer> Ordered = new List<EntityPlayer>(32);
		static readonly Dictionary<int, int> LastBand = new Dictionary<int, int>();
		static readonly List<int> LastIds = new List<int>(32);
		static readonly Dictionary<int, EntityPlayer> LiveById = new Dictionary<int, EntityPlayer>(32);
		static readonly Dictionary<int, int> LiveBand = new Dictionary<int, int>(32);
		static readonly List<EntityPlayer>[] BandStayers = MakeBandLists();
		static readonly List<EntityPlayer>[] BandFromCloser = MakeBandLists();
		static readonly List<EntityPlayer>[] BandFromFarther = MakeBandLists();
		static readonly List<EntityPlayer>[] BandNew = MakeBandLists();
		static readonly List<MemberSeed> NewSeeds = new List<MemberSeed>(16);
		static readonly List<int> PrevIds = new List<int>(32);
		static readonly Dictionary<int, int> PrevBand = new Dictionary<int, int>(32);
		static readonly HashSet<int> PrevPriority = new HashSet<int>();
		static readonly HashSet<int> Placed = new HashSet<int>();
		public static XUiC_PartyEntryList CachedList;
		public static bool PriorityDirty;
		static int LastPartyId = int.MinValue;
		static int LastMemberKey;
		static Vector3 LiveOrigin;

		struct MemberSeed
		{
			public EntityPlayer Player;
			public float Dist;
		}

		static List<EntityPlayer>[] MakeBandLists()
		{
			List<EntityPlayer>[] lists = new List<EntityPlayer>[BandCount];
			for (int i = 0; i < BandCount; i++)
			{
				lists[i] = new List<EntityPlayer>(16);
			}

			return lists;
		}

		public static bool TryGetPartyCount(EntityPlayer localPlayer, out int count)
		{
			count = 0;
			if (localPlayer == null || localPlayer.IsDead())
			{
				return false;
			}

			Party party = localPlayer.Party;
			if (party == null)
			{
				return false;
			}

			List<EntityPlayer> members = party.MemberList;
			if (members == null)
			{
				return false;
			}

			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] != null)
				{
					count++;
				}
			}

			return count >= 2;
		}

		public static void HookColorRefresh()
		{
			if (ColorHooked)
			{
				return;
			}

			ColorHooked = true;
			PartyGroupPlusColorStore.Changed += RefreshEntryColors;
		}

		public static void RefreshEntryColors()
		{
			if (CachedList == null)
			{
				return;
			}

			List<XUiC_PartyEntry> entries = EntryListField.GetValue(CachedList) as List<XUiC_PartyEntry>;
			if (entries == null)
			{
				return;
			}

			for (int i = 0; i < entries.Count; i++)
			{
				XUiC_PartyEntry entry = entries[i];
				if (entry == null || entry.Player == null)
				{
					continue;
				}

				entry.IsDirty = true;
				entry.RefreshBindings();
			}
		}

		public static void TrySlowRefresh(XUiC_PartyWindow window)
		{
			HookColorRefresh();
			if (CachedList == null && window != null)
			{
				CachedList = window.GetChildByType<XUiC_PartyEntryList>();
			}

			if (Time.time < NextRefreshTime)
			{
				return;
			}

			if (CachedList != null)
			{
				CachedList.RefreshPartyList();
			}
		}

		public static void ApplySortedList(XUiC_PartyEntryList list)
		{
			CachedList = list;
			HookColorRefresh();
			List<XUiC_PartyEntry> entries = EntryListField.GetValue(list) as List<XUiC_PartyEntry>;
			if (entries == null || entries.Count == 0)
			{
				return;
			}

			int slots = entries.Count;
			EntityPlayer localPlayer = XUiC_PartyGroupPlusColorChoice.SafeLocal(list.xui);
			if (localPlayer == null || localPlayer.Party == null)
			{
				ClearOrder();
				for (int i = 0; i < slots; i++)
				{
					AssignSlot(entries, i, null);
				}

				return;
			}

			int memberKey = MemberKey(localPlayer.Party);
			bool partyChanged = localPlayer.Party.PartyID != LastPartyId || memberKey != LastMemberKey;
			bool timed = Ordered.Count == 0 || Time.time >= NextRefreshTime;
			if (timed || partyChanged || PriorityDirty)
			{
				RebuildOrder(localPlayer);
				if (timed)
				{
					NextRefreshTime = Time.time + RefreshSeconds;
				}

				PriorityDirty = false;
			}

			LastPartyId = localPlayer.Party.PartyID;
			LastMemberKey = memberKey;

			int shown = slots < Ordered.Count ? slots : Ordered.Count;
			for (int i = 0; i < shown; i++)
			{
				AssignSlot(entries, i, Ordered[i]);
			}

			for (int i = shown; i < slots; i++)
			{
				AssignSlot(entries, i, null);
			}
		}

		static void CollectLive(EntityPlayer localPlayer)
		{
			LiveById.Clear();
			LiveBand.Clear();
			LiveOrigin = localPlayer.position;
			List<EntityPlayer> members = localPlayer.Party.MemberList;
			for (int i = 0; i < members.Count; i++)
			{
				EntityPlayer member = members[i];
				if (member == null || member == localPlayer)
				{
					continue;
				}

				LiveById[member.entityId] = member;
				LiveBand[member.entityId] = BandForDistance(Vector3.Distance(LiveOrigin, member.position));
			}
		}

		static void RebuildOrder(EntityPlayer localPlayer)
		{
			CollectLive(localPlayer);
			PrevIds.Clear();
			for (int i = 0; i < LastIds.Count; i++)
			{
				PrevIds.Add(LastIds[i]);
			}

			PrevBand.Clear();
			foreach (KeyValuePair<int, int> pair in LastBand)
			{
				PrevBand[pair.Key] = pair.Value;
			}

			Ordered.Clear();
			LastBand.Clear();
			LastIds.Clear();
			FillGroup(true);
			FillGroup(false);

			PrevPriority.Clear();
			for (int i = 0; i < Ordered.Count; i++)
			{
				if (PartyGroupPlusPriorityStore.IsPriority(Ordered[i]))
				{
					PrevPriority.Add(Ordered[i].entityId);
				}
			}
		}

		static void FillGroup(bool wantPriority)
		{
			ClearBandLists();
			Placed.Clear();
			for (int i = 0; i < PrevIds.Count; i++)
			{
				int id = PrevIds[i];
				if (!LiveById.TryGetValue(id, out EntityPlayer player))
				{
					continue;
				}

				if (PartyGroupPlusPriorityStore.IsPriority(player) != wantPriority)
				{
					continue;
				}

				int band = LiveBand[id];
				bool wasThisGroup = PrevPriority.Contains(id) == wantPriority;
				if (!wasThisGroup)
				{
					BandNew[band].Add(player);
					Placed.Add(id);
					continue;
				}

				if (!PrevBand.TryGetValue(id, out int oldBand) || band == oldBand)
				{
					BandStayers[band].Add(player);
				}
				else if (oldBand < band)
				{
					BandFromCloser[band].Add(player);
				}
				else
				{
					BandFromFarther[band].Add(player);
				}

				Placed.Add(id);
			}

			foreach (KeyValuePair<int, EntityPlayer> pair in LiveById)
			{
				if (Placed.Contains(pair.Key))
				{
					continue;
				}

				if (PartyGroupPlusPriorityStore.IsPriority(pair.Value) != wantPriority)
				{
					continue;
				}

				BandNew[LiveBand[pair.Key]].Add(pair.Value);
			}

			SortNewcomersByDistance(LiveOrigin);
			for (int band = 0; band < BandCount; band++)
			{
				AppendBand(BandFromCloser[band], band);
				AppendBand(BandStayers[band], band);
				AppendBand(BandFromFarther[band], band);
				AppendBand(BandNew[band], band);
			}
		}

		static void SortNewcomersByDistance(Vector3 origin)
		{
			for (int band = 0; band < BandCount; band++)
			{
				List<EntityPlayer> list = BandNew[band];
				if (list.Count < 2)
				{
					continue;
				}

				NewSeeds.Clear();
				for (int i = 0; i < list.Count; i++)
				{
					EntityPlayer player = list[i];
					NewSeeds.Add(new MemberSeed
					{
						Player = player,
						Dist = Vector3.Distance(origin, player.position)
					});
				}

				NewSeeds.Sort(CompareNewSeeds);
				list.Clear();
				for (int i = 0; i < NewSeeds.Count; i++)
				{
					list.Add(NewSeeds[i].Player);
				}
			}
		}

		static int CompareNewSeeds(MemberSeed a, MemberSeed b)
		{
			int dist = a.Dist.CompareTo(b.Dist);
			if (dist != 0)
			{
				return dist;
			}

			return a.Player.entityId.CompareTo(b.Player.entityId);
		}

		static void AppendBand(List<EntityPlayer> players, int band)
		{
			for (int i = 0; i < players.Count; i++)
			{
				EntityPlayer player = players[i];
				Ordered.Add(player);
				LastBand[player.entityId] = band;
				LastIds.Add(player.entityId);
			}
		}

		static int BandForDistance(float meters)
		{
			for (int i = 0; i < BandMaxMeters.Length; i++)
			{
				if (meters <= BandMaxMeters[i])
				{
					return i;
				}
			}

			return BandCount - 1;
		}

		public static int MemberKey(Party party)
		{
			int key = party.PartyID * 397;
			List<EntityPlayer> members = party.MemberList;
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] != null)
				{
					key = (key * 31) + members[i].entityId;
				}
			}

			return key;
		}

		static void ClearOrder()
		{
			Ordered.Clear();
			LastBand.Clear();
			LastIds.Clear();
			LastPartyId = int.MinValue;
			LastMemberKey = 0;
			PrevPriority.Clear();
		}

		static void ClearBandLists()
		{
			for (int i = 0; i < BandCount; i++)
			{
				BandStayers[i].Clear();
				BandFromCloser[i].Clear();
				BandFromFarther[i].Clear();
				BandNew[i].Clear();
			}
		}

		static void AssignSlot(List<XUiC_PartyEntry> entries, int slot, EntityPlayer player)
		{
			if (entries[slot].Player == player)
			{
				return;
			}

			entries[slot].SetPlayer(player);
		}
	}
}
