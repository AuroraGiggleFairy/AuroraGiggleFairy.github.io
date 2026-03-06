using UnityEngine.Scripting;

[Preserve]
public class NetPackagePartyActions : NetPackage
{
	public enum PartyActions
	{
		SendInvite,
		AcceptInvite,
		ChangeLead,
		LeaveParty,
		KickFromParty,
		Disconnected,
		JoinAutoParty,
		SetVoiceLobby
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int invitedByEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int invitedEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string voiceLobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyActions currentOperation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] partyMembers;

	public NetPackagePartyActions Setup(PartyActions _operation, int _invitedByEntityID, int _invitedEntityID, int[] _partyMembers = null, string _voiceLobbyId = null)
	{
		currentOperation = _operation;
		invitedByEntityID = _invitedByEntityID;
		invitedEntityID = _invitedEntityID;
		partyMembers = _partyMembers;
		voiceLobbyId = _voiceLobbyId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		currentOperation = (PartyActions)_br.ReadByte();
		invitedByEntityID = _br.ReadInt32();
		invitedEntityID = _br.ReadInt32();
		voiceLobbyId = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)currentOperation);
		_bw.Write(invitedByEntityID);
		_bw.Write(invitedEntityID);
		_bw.Write(voiceLobbyId ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(invitedEntityID) as EntityPlayer;
		EntityPlayer entityPlayer2 = _world.GetEntity(invitedByEntityID) as EntityPlayer;
		if (entityPlayer == null || entityPlayer2 == null)
		{
			return;
		}
		switch (currentOperation)
		{
		case PartyActions.SendInvite:
			if (entityPlayer2.HasPendingPartyInvite(invitedEntityID))
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					if (!entityPlayer2.IsInParty())
					{
						Party.ServerHandleAcceptInvite(entityPlayer, entityPlayer2);
					}
					else if (!entityPlayer.IsInParty())
					{
						Party.ServerHandleAcceptInvite(entityPlayer2, entityPlayer);
					}
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(PartyActions.AcceptInvite, invitedEntityID, invitedByEntityID));
				}
			}
			else if (!entityPlayer.IsInParty())
			{
				entityPlayer.AddPartyInvite(invitedByEntityID);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(PartyActions.SendInvite, invitedByEntityID, invitedEntityID));
				}
				if (entityPlayer is EntityPlayerLocal player)
				{
					GameManager.ShowTooltip(player, string.Format(Localization.Get("ttPartyInviteReceived"), entityPlayer2.PlayerDisplayName), (string)null, "party_invite_receive", (ToolTipEvent)null, false, false, 0f);
				}
			}
			break;
		case PartyActions.AcceptInvite:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party == null)
			{
				Party.ServerHandleAcceptInvite(entityPlayer2, entityPlayer);
			}
			break;
		case PartyActions.ChangeLead:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleChangeLead(entityPlayer);
			}
			break;
		case PartyActions.LeaveParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleLeaveParty(entityPlayer, invitedEntityID);
			}
			break;
		case PartyActions.KickFromParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleKickParty(invitedEntityID);
			}
			break;
		case PartyActions.Disconnected:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityPlayer.Party != null)
			{
				Party.ServerHandleDisconnectParty(entityPlayer);
			}
			break;
		case PartyActions.JoinAutoParty:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleAutoJoinParty(entityPlayer);
			}
			break;
		case PartyActions.SetVoiceLobby:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Party.ServerHandleSetVoiceLoby(entityPlayer, voiceLobbyId);
			}
			break;
		}
	}

	public override int GetLength()
	{
		return 9;
	}
}
