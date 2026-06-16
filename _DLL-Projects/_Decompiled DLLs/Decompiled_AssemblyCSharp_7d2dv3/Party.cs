using System.Collections.Generic;
using Audio;
using Twitch;
using UnityEngine;

public class Party
{
	public int LeaderIndex;

	public int PartyID = -1;

	public string VoiceLobbyId;

	public List<EntityPlayer> MemberList = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> changedPlayers = new List<EntityPlayer>();

	public EntityPlayer Leader
	{
		get
		{
			if (LeaderIndex >= MemberList.Count)
			{
				return null;
			}
			return MemberList[LeaderIndex];
		}
	}

	public int GameStage
	{
		get
		{
			List<int> list = new List<int>();
			for (int i = 0; i < MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = MemberList[i];
				list.Add(entityPlayer.gameStage);
			}
			return GameStageDefinition.CalcPartyLevel(list);
		}
	}

	public int HighestGameStage
	{
		get
		{
			int num = 0;
			for (int i = 0; i < MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = MemberList[i];
				num = Mathf.Max(num, entityPlayer.gameStage);
			}
			return num;
		}
	}

	public bool HasTwitchMember
	{
		get
		{
			for (int i = 0; i < MemberList.Count; i++)
			{
				if (MemberList[i].TwitchEnabled)
				{
					return true;
				}
			}
			return false;
		}
	}

	public TwitchVoteLockTypes HasTwitchVoteLock
	{
		get
		{
			for (int i = 0; i < MemberList.Count; i++)
			{
				if (MemberList[i].TwitchVoteLock != TwitchVoteLockTypes.None)
				{
					return MemberList[i].TwitchVoteLock;
				}
			}
			return TwitchVoteLockTypes.None;
		}
	}

	public event OnPartyMembersChanged PartyMemberAdded;

	public event OnPartyMembersChanged PartyMemberRemoved;

	public event OnPartyChanged PartyLeaderChanged;

	public int GetHighestLootStage(float containerMod, float containerBonus)
	{
		int num = 0;
		for (int i = 0; i < MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = MemberList[i];
			num = Mathf.Max(num, entityPlayer.GetLootStage(containerMod, containerBonus));
		}
		return num;
	}

	public bool AddPlayer(EntityPlayer player)
	{
		if (MemberList.Contains(player))
		{
			return false;
		}
		if (MemberList.Count == 8)
		{
			return false;
		}
		MemberList.Add(player);
		player.Party = this;
		player.RemoveAllPartyInvites();
		bool isInPartyOfLocalPlayer = false;
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (MemberList[i] is EntityPlayerLocal)
			{
				isInPartyOfLocalPlayer = true;
				break;
			}
		}
		if (this.PartyMemberAdded != null)
		{
			this.PartyMemberAdded(player);
		}
		for (int j = 0; j < MemberList.Count; j++)
		{
			MemberList[j].IsInPartyOfLocalPlayer = isInPartyOfLocalPlayer;
			MemberList[j].HandleOnPartyJoined();
			if (MemberList[j].NavObject != null)
			{
				MemberList[j].NavObject.UseOverrideColor = true;
				MemberList[j].NavObject.OverrideColor = Constants.TrackedFriendColors[j % Constants.TrackedFriendColors.Length];
				MemberList[j].NavObject.name = MemberList[j].PlayerDisplayName;
			}
		}
		return true;
	}

	public bool KickPlayer(EntityPlayer player)
	{
		if (!MemberList.Contains(player))
		{
			return false;
		}
		if (player.NavObject != null)
		{
			player.NavObject.UseOverrideColor = false;
		}
		MemberList.Remove(player);
		if (this.PartyMemberRemoved != null)
		{
			this.PartyMemberRemoved(player);
		}
		player.LeaveParty();
		player.IsInPartyOfLocalPlayer = false;
		if (MemberList.Count == 1)
		{
			MemberList[0].LeaveParty();
		}
		return true;
	}

	public bool RemovePlayer(EntityPlayer player)
	{
		if (!MemberList.Contains(player))
		{
			return false;
		}
		if (player.NavObject != null)
		{
			player.NavObject.UseOverrideColor = false;
		}
		MemberList.Remove(player);
		player.IsInPartyOfLocalPlayer = false;
		if (this.PartyMemberRemoved != null)
		{
			this.PartyMemberRemoved(player);
		}
		if (MemberList.Count != 1)
		{
			return true;
		}
		if (GameStats.GetBool(EnumGameStats.AutoParty) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && MemberList[0].entityId == GameManager.Instance.World.GetPrimaryPlayerId())
		{
			return true;
		}
		MemberList[0].LeaveParty();
		return true;
	}

	public bool ContainsMember(EntityPlayer player)
	{
		if (MemberList == null)
		{
			return false;
		}
		return MemberList.Contains(player);
	}

	public bool ContainsMember(int entityID)
	{
		if (MemberList == null)
		{
			return false;
		}
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (MemberList[i].entityId == entityID)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public int MemberCountInRange(EntityPlayer player)
	{
		int num = 0;
		for (int i = 0; i < player.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = player.Party.MemberList[i];
			if (!(entityPlayer == player) && Vector3.Distance(player.position, entityPlayer.position) < (float)GameStats.GetInt(EnumGameStats.PartySharedKillRange))
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public int MemberCountNotInRange(EntityPlayer player)
	{
		int num = 0;
		for (int i = 0; i < player.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = player.Party.MemberList[i];
			if (!(entityPlayer == player) && Vector3.Distance(player.position, entityPlayer.position) >= 15f)
			{
				num++;
			}
		}
		return num;
	}

	public int MemberCountNotWithin(EntityPlayer player, Rect poiRect)
	{
		int num = 0;
		for (int i = 0; i < player.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = player.Party.MemberList[i];
			if (!(entityPlayer == player))
			{
				Vector3 position = entityPlayer.position;
				position.y = position.z;
				if (!poiRect.Contains(position))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetPartyXP(EntityPlayer player, int startingXP)
	{
		int num = MemberCountInRange(player);
		return (int)((float)startingXP * (1f - 0.1f * (float)num));
	}

	public bool IsLocalParty()
	{
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (MemberList[i] is EntityPlayerLocal)
			{
				return true;
			}
		}
		return false;
	}

	public int[] GetMemberIdArray()
	{
		int[] array = new int[MemberList.Count];
		for (int i = 0; i < MemberList.Count; i++)
		{
			array[i] = MemberList[i].entityId;
		}
		return array;
	}

	public List<int> GetMemberIdList(EntityPlayer exclude)
	{
		List<int> list = null;
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (!(MemberList[i] == exclude))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(MemberList[i].entityId);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void UpdateMemberList(World world, int[] partyMembers)
	{
		EntityPlayerLocal entityPlayerLocal = null;
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		if (localPlayers != null && localPlayers.Count > 0)
		{
			entityPlayerLocal = localPlayers[0];
		}
		changedPlayers.Clear();
		bool flag = false;
		for (int i = 0; i < MemberList.Count; i++)
		{
			flag = false;
			for (int j = 0; j < partyMembers.Length; j++)
			{
				if (MemberList[i].entityId == partyMembers[j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				changedPlayers.Add(MemberList[i]);
			}
		}
		for (int k = 0; k < changedPlayers.Count; k++)
		{
			changedPlayers[k].Party = null;
			changedPlayers[k].IsInPartyOfLocalPlayer = false;
			changedPlayers[k].HandleOnPartyLeave(this);
			if (changedPlayers[k].NavObject != null && changedPlayers[k] != entityPlayerLocal)
			{
				changedPlayers[k].NavObject.UseOverrideColor = false;
			}
			if (entityPlayerLocal != null && entityPlayerLocal.Party == this)
			{
				entityPlayerLocal.QuestJournal.RemoveSharedQuestForOwner(changedPlayers[k].entityId);
				entityPlayerLocal.QuestJournal.RemoveSharedQuestEntryByOwner(changedPlayers[k].entityId);
			}
		}
		bool isInPartyOfLocalPlayer = false;
		changedPlayers.Clear();
		for (int l = 0; l < MemberList.Count; l++)
		{
			changedPlayers.Add(MemberList[l]);
		}
		MemberList.Clear();
		int index = 0;
		for (int m = 0; m < partyMembers.Length; m++)
		{
			flag = false;
			for (int n = 0; n < changedPlayers.Count; n++)
			{
				if (changedPlayers[n].entityId == partyMembers[m])
				{
					MemberList.Add(changedPlayers[n]);
					MemberList[index].Party = this;
					if (MemberList[index] is EntityPlayerLocal)
					{
						isInPartyOfLocalPlayer = true;
					}
					MemberList[index++].RemoveAllPartyInvites();
					changedPlayers.RemoveAt(n);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			EntityPlayer entityPlayer = world.GetEntity(partyMembers[m]) as EntityPlayer;
			if (entityPlayer != null)
			{
				MemberList.Add(entityPlayer);
				MemberList[index].Party = this;
				if (MemberList[index] is EntityPlayerLocal)
				{
					isInPartyOfLocalPlayer = true;
				}
				MemberList[index++].RemoveAllPartyInvites();
			}
		}
		for (int num = 0; num < MemberList.Count; num++)
		{
			if (entityPlayerLocal != null && num != LeaderIndex)
			{
				entityPlayerLocal.RemovePartyInvite(MemberList[num].entityId);
			}
			if (MemberList[num].NavObject != null && entityPlayerLocal.Party == MemberList[num].Party)
			{
				MemberList[num].NavObject.UseOverrideColor = true;
				MemberList[num].NavObject.OverrideColor = Constants.TrackedFriendColors[num % Constants.TrackedFriendColors.Length];
				MemberList[num].NavObject.name = MemberList[num].PlayerDisplayName;
			}
			MemberList[num].IsInPartyOfLocalPlayer = isInPartyOfLocalPlayer;
			MemberList[num].HandleOnPartyJoined();
		}
	}

	public void Disband()
	{
		for (int i = 0; i < MemberList.Count; i++)
		{
			MemberList[i].LeaveParty();
		}
		PartyManager.Current.RemoveParty(this);
	}

	public static void ServerHandleAcceptInvite(EntityPlayer invitedBy, EntityPlayer invitedEntity)
	{
		if (invitedBy.Party == null)
		{
			PartyManager.Current.CreateParty().AddPlayer(invitedBy);
		}
		invitedBy.Party.AddPlayer(invitedEntity);
		invitedEntity.RemoveAllPartyInvites();
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		if (localPlayers != null && localPlayers.Count > 0)
		{
			EntityPlayerLocal entityPlayerLocal = localPlayers[0];
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.RemovePartyInvite(invitedEntity.entityId);
				GameManager.Instance.RemovePartyInvitesFromAllPlayers(entityPlayerLocal);
				if (entityPlayerLocal != invitedEntity && entityPlayerLocal.Party != null && entityPlayerLocal.Party == invitedEntity.Party)
				{
					Manager.PlayInsidePlayerHead("party_member_join");
				}
				else if (entityPlayerLocal == invitedEntity)
				{
					Manager.PlayInsidePlayerHead("party_join");
				}
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(invitedBy.Party, invitedEntity.entityId, NetPackagePartyData.PartyActions.AcceptInvite));
	}

	public static void ServerHandleChangeLead(EntityPlayer newHost)
	{
		newHost.Party.SetLeader(newHost);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(newHost.Party, newHost.entityId, NetPackagePartyData.PartyActions.ChangeLead));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLeader(EntityPlayer newHost)
	{
		LeaderIndex = MemberList.IndexOf(newHost);
		for (int i = 0; i < MemberList.Count; i++)
		{
			MemberList[i].HandleOnPartyChanged();
		}
		if (this.PartyLeaderChanged != null)
		{
			this.PartyLeaderChanged(this, newHost);
		}
	}

	public static void ServerHandleKickParty(int entityID)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityID) as EntityPlayer;
		if (entityPlayer.Party == null)
		{
			return;
		}
		Party party = entityPlayer.Party;
		EntityPlayer leader = party.Leader;
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			if (primaryPlayer == entityPlayer)
			{
				Manager.PlayInsidePlayerHead("party_leave");
			}
			else if (primaryPlayer.Party == party)
			{
				primaryPlayer.QuestJournal.RemoveSharedQuestForOwner(entityPlayer.entityId);
				primaryPlayer.QuestJournal.RemoveSharedQuestEntryByOwner(entityPlayer.entityId);
				primaryPlayer.QuestJournal.RemovePlayerFromSharedWiths(entityPlayer);
				Manager.PlayInsidePlayerHead("party_member_leave");
			}
		}
		party.KickPlayer(entityPlayer);
		entityPlayer.LeaveParty();
		if (leader == entityPlayer)
		{
			party.LeaderIndex = 0;
		}
		else
		{
			party.SetLeader(leader);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(party, entityID, NetPackagePartyData.PartyActions.KickFromParty, party.MemberList.Count == 0));
	}

	public static void ServerHandleLeaveParty(EntityPlayer player, int entityID)
	{
		if (player.Party == null)
		{
			return;
		}
		Party party = player.Party;
		EntityPlayer leader = party.Leader;
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null && primaryPlayer == player)
		{
			party.ClearAllNavObjectColors();
		}
		if (primaryPlayer != null)
		{
			if (primaryPlayer == player)
			{
				Manager.PlayInsidePlayerHead("party_leave");
			}
			else if (primaryPlayer.Party == party)
			{
				primaryPlayer.QuestJournal.RemoveSharedQuestForOwner(player.entityId);
				primaryPlayer.QuestJournal.RemoveSharedQuestEntryByOwner(player.entityId);
				primaryPlayer.QuestJournal.RemovePlayerFromSharedWiths(player);
				Manager.PlayInsidePlayerHead("party_member_leave");
			}
		}
		party.RemovePlayer(player);
		player.LeaveParty();
		if (leader == player)
		{
			party.LeaderIndex = 0;
		}
		else
		{
			party.SetLeader(leader);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(party, entityID, NetPackagePartyData.PartyActions.LeaveParty, party.MemberList.Count == 0));
	}

	public static void ServerHandleDisconnectParty(EntityPlayer player)
	{
		if (player.Party == null)
		{
			return;
		}
		Party party = player.Party;
		EntityPlayer leader = party.Leader;
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			if (primaryPlayer == player)
			{
				Manager.PlayInsidePlayerHead("party_leave");
			}
			else if (primaryPlayer.Party == party)
			{
				primaryPlayer.QuestJournal.RemoveSharedQuestForOwner(player.entityId);
				primaryPlayer.QuestJournal.RemoveSharedQuestEntryByOwner(player.entityId);
				primaryPlayer.QuestJournal.RemovePlayerFromSharedWiths(player);
				Manager.PlayInsidePlayerHead("party_member_leave");
			}
		}
		party.RemovePlayer(player);
		player.LeaveParty();
		if (leader == player)
		{
			party.LeaderIndex = 0;
		}
		else
		{
			party.SetLeader(leader);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(party, player.entityId, NetPackagePartyData.PartyActions.Disconnected, party.MemberList.Count == 0));
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool IsFull()
	{
		return MemberList.Count == 8;
	}

	public static void ServerHandleAutoJoinParty(EntityPlayer joiningEntity)
	{
		Party party = PartyManager.Current.GetParty(1);
		if (party == null)
		{
			party = PartyManager.Current.CreateParty();
		}
		if (party.AddPlayer(joiningEntity))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(party, joiningEntity.entityId, NetPackagePartyData.PartyActions.AutoJoin));
		}
	}

	public static void ServerHandleSetVoiceLoby(EntityPlayer player, string voiceLobbyId)
	{
		if (player.Party != null)
		{
			Party party = player.Party;
			party.VoiceLobbyId = voiceLobbyId;
			for (int i = 0; i < party.MemberList.Count; i++)
			{
				party.MemberList[i].HandleOnPartyChanged();
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyData>().Setup(party, player.entityId, NetPackagePartyData.PartyActions.SetVoiceLobby, party.MemberList.Count == 0));
		}
	}

	public void ClearAllNavObjectColors()
	{
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (MemberList[i].NavObject != null)
			{
				MemberList[i].NavObject.UseOverrideColor = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public EntityPlayer GetMemberAtIndex(int index, EntityPlayer excludePlayer)
	{
		int num = 0;
		for (int i = 0; i < MemberList.Count; i++)
		{
			if (MemberList[i] != excludePlayer)
			{
				num++;
			}
			if (num == index)
			{
				return MemberList[i];
			}
		}
		return null;
	}
}
