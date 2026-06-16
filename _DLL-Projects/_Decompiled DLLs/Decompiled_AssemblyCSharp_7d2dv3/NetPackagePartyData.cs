using Audio;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePartyData : NetPackage
{
	public enum PartyActions
	{
		SendInvite,
		AcceptInvite,
		ChangeLead,
		LeaveParty,
		KickFromParty,
		Disconnected,
		AutoJoin,
		SetVoiceLobby
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int PartyID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int LeaderIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoiceLobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] partyMembers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int changedEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyActions partyAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disbandParty;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePartyData Setup(Party _party, int _changedEntityID, PartyActions _partyAction, bool _disbandParty = false)
	{
		PartyID = _party.PartyID;
		LeaderIndex = _party.LeaderIndex;
		VoiceLobbyId = _party.VoiceLobbyId;
		partyMembers = _party.GetMemberIdArray();
		changedEntityID = _changedEntityID;
		partyAction = _partyAction;
		disbandParty = _disbandParty;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		PartyID = _br.ReadInt32();
		LeaderIndex = _br.ReadByte();
		VoiceLobbyId = _br.ReadString();
		int num = _br.ReadInt32();
		partyMembers = new int[num];
		for (int i = 0; i < num; i++)
		{
			partyMembers[i] = _br.ReadInt32();
		}
		changedEntityID = _br.ReadInt32();
		partyAction = (PartyActions)_br.ReadByte();
		disbandParty = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(PartyID);
		_bw.Write((byte)LeaderIndex);
		_bw.Write(VoiceLobbyId ?? "");
		_bw.Write(partyMembers.Length);
		for (int i = 0; i < partyMembers.Length; i++)
		{
			_bw.Write(partyMembers[i]);
		}
		_bw.Write(changedEntityID);
		_bw.Write((byte)partyAction);
		_bw.Write(disbandParty);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		Party party = PartyManager.Current.GetParty(PartyID);
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Party party2 = null;
		if (primaryPlayer != null)
		{
			party2 = primaryPlayer.Party;
		}
		if (party == null)
		{
			party = PartyManager.Current.CreateClientParty(_world, PartyID, LeaderIndex, partyMembers, VoiceLobbyId);
		}
		else
		{
			party.LeaderIndex = LeaderIndex;
			party.VoiceLobbyId = VoiceLobbyId;
			party.UpdateMemberList(_world, partyMembers);
		}
		if (primaryPlayer != null)
		{
			EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(changedEntityID);
			if (primaryPlayer == entityPlayer)
			{
				switch (partyAction)
				{
				case PartyActions.KickFromParty:
					Manager.PlayInsidePlayerHead("party_leave");
					GameManager.ShowTooltip(primaryPlayer, Localization.Get("ttPartyKickedFromParty"));
					break;
				case PartyActions.LeaveParty:
					Manager.PlayInsidePlayerHead("party_leave");
					break;
				}
			}
			else if (party2 == party && changedEntityID != -1)
			{
				switch (partyAction)
				{
				case PartyActions.AcceptInvite:
					entityPlayer.RemoveAllPartyInvites();
					if (entityPlayer == primaryPlayer)
					{
						GameManager.Instance.RemovePartyInvitesFromAllPlayers(entityPlayer);
						Manager.PlayInsidePlayerHead("party_join");
					}
					else
					{
						Manager.PlayInsidePlayerHead("party_member_join");
					}
					break;
				case PartyActions.Disconnected:
					if (entityPlayer != primaryPlayer)
					{
						Manager.PlayInsidePlayerHead("party_member_leave");
						GameManager.ShowTooltip(primaryPlayer, string.Format(Localization.Get("ttPartyDisconnectedFromParty"), entityPlayer.PlayerDisplayName));
						primaryPlayer.QuestJournal.RemovePlayerFromSharedWiths(entityPlayer);
					}
					break;
				case PartyActions.KickFromParty:
					entityPlayer.Party = null;
					if (entityPlayer != primaryPlayer)
					{
						Manager.PlayInsidePlayerHead("party_member_leave");
						GameManager.ShowTooltip(primaryPlayer, string.Format(Localization.Get("ttPartyOtherKickedFromParty"), entityPlayer.PlayerDisplayName));
						primaryPlayer.QuestJournal.RemovePlayerFromSharedWiths(entityPlayer);
					}
					break;
				case PartyActions.LeaveParty:
					entityPlayer.Party = null;
					if (entityPlayer != primaryPlayer)
					{
						Manager.PlayInsidePlayerHead("party_member_leave");
						GameManager.ShowTooltip(primaryPlayer, string.Format(Localization.Get("ttPartyOtherLeftParty"), entityPlayer.PlayerDisplayName));
					}
					break;
				case PartyActions.SetVoiceLobby:
				{
					for (int i = 0; i < party.MemberList.Count; i++)
					{
						party.MemberList[i].HandleOnPartyChanged();
					}
					break;
				}
				}
			}
		}
		if (disbandParty)
		{
			party.Disband();
		}
	}

	public override int GetLength()
	{
		return 9;
	}
}
