using Steamworks;

namespace Platform.Steam;

public class AuthenticationServer : IAuthenticationServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<ValidateAuthTicketResponse_t> m_validateAuthTicketResponse;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GSClientGroupStatus_t> m_gsClientGroupStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public KickPlayerDelegate kickPlayerDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public SteamGroupStatusResponse groupStatusResponseDelegate;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Log.Out("[Steamworks.NET] Registering auth callbacks");
			if (m_validateAuthTicketResponse == null)
			{
				m_validateAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(ValidateAuthTicketResponse);
			}
			if (m_gsClientGroupStatus == null)
			{
				m_gsClientGroupStatus = Callback<GSClientGroupStatus_t>.CreateGameServer(GsClientGroupStatus);
			}
		};
	}

	public EBeginUserAuthenticationResult AuthenticateUser(ClientInfo _cInfo)
	{
		Log.Out("[Steamworks.NET] Auth.AuthenticateUser()");
		UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_cInfo.PlatformId;
		CSteamID cSteamID = new CSteamID(userIdentifierSteam.SteamId);
		byte[] ticket = userIdentifierSteam.Ticket;
		if (ticket == null || ticket.Length == 0)
		{
			return EBeginUserAuthenticationResult.InvalidTicket;
		}
		EBeginAuthSessionResult eBeginAuthSessionResult = SteamGameServer.BeginAuthSession(ticket, ticket.Length, cSteamID);
		string[] obj = new string[8] { "[Steamworks.NET] Authenticating player: ", _cInfo.playerName, " SteamId: ", null, null, null, null, null };
		CSteamID cSteamID2 = cSteamID;
		obj[3] = cSteamID2.ToString();
		obj[4] = " TicketLen: ";
		obj[5] = ticket.Length.ToString();
		obj[6] = " Result: ";
		obj[7] = eBeginAuthSessionResult.ToStringCached();
		Log.Out(string.Concat(obj));
		if (eBeginAuthSessionResult == EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
		{
			return EBeginUserAuthenticationResult.Ok;
		}
		SteamGameServer.EndAuthSession(cSteamID);
		return (EBeginUserAuthenticationResult)eBeginAuthSessionResult;
	}

	public void RemoveUser(ClientInfo _cInfo)
	{
		if (owner.ServerListAnnouncer.GameServerInitialized)
		{
			SteamGameServer.EndAuthSession(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateAuthTicketResponse(ValidateAuthTicketResponse_t _resp)
	{
		CSteamID steamID = _resp.m_SteamID;
		CSteamID ownerSteamID = _resp.m_OwnerSteamID;
		PlatformUserIdentifierAbs userIdentifier = new UserIdentifierSteam(steamID);
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifier);
		if (clientInfo == null)
		{
			Log.Warning($"[Steamworks.NET] Authentication callback failed: User not found. ID: {steamID}");
			return;
		}
		string playerName = clientInfo.playerName;
		((UserIdentifierSteam)clientInfo.PlatformId).OwnerId = new UserIdentifierSteam(ownerSteamID);
		Log.Out($"[Steamworks.NET] Authentication callback. ID: {steamID}, owner: {ownerSteamID}, result: {_resp.m_eAuthSessionResponse.ToStringCached()}");
		if (_resp.m_eAuthSessionResponse != EAuthSessionResponse.k_EAuthSessionResponseOK)
		{
			Log.Out($"[Steamworks.NET] Kick player for invalid login: {steamID} {playerName}");
			kickPlayerDelegate?.Invoke(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlatformAuthenticationFailed, (int)_resp.m_eAuthSessionResponse));
		}
		else
		{
			authSuccessfulDelegate(clientInfo);
		}
	}

	public void StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
	{
		authSuccessfulDelegate = _authSuccessfulDelegate;
		kickPlayerDelegate = _kickPlayerDelegate;
	}

	public void StartServerSteamGroups(SteamGroupStatusResponse _groupStatusResponseDelegate)
	{
		groupStatusResponseDelegate = _groupStatusResponseDelegate;
	}

	public void StopServer()
	{
		authSuccessfulDelegate = null;
		kickPlayerDelegate = null;
	}

	public bool RequestUserInGroupStatus(ClientInfo _cInfo, string _steamIdGroup)
	{
		UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_cInfo.PlatformId;
		CSteamID cSteamID = new CSteamID(userIdentifierSteam.SteamId);
		if (!ulong.TryParse(_steamIdGroup, out var result))
		{
			Log.Warning("Invalid Steam group ID '" + _steamIdGroup + "' (value out of range) in serveradmin.xml, ignoring");
			return false;
		}
		CSteamID cSteamID2 = new CSteamID(result);
		EAccountType eAccountType = cSteamID2.GetEAccountType();
		if (eAccountType != EAccountType.k_EAccountTypeClan)
		{
			if (eAccountType == EAccountType.k_EAccountTypeIndividual)
			{
				Log.Warning("Invalid Steam group ID '" + _steamIdGroup + "' (SteamID is for a user, not for a group) in serveradmin.xml, ignoring");
			}
			else
			{
				Log.Warning("Invalid Steam group ID '" + _steamIdGroup + "' (SteamID not valid for a Steam group but for " + eAccountType.ToStringCached() + ") in serveradmin.xml, ignoring");
			}
			return false;
		}
		bool num = SteamGameServer.RequestUserGroupStatus(cSteamID, cSteamID2);
		if (!num)
		{
			Log.Warning($"Failed requesting Steam group membership for user {cSteamID} and group {cSteamID2}");
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GsClientGroupStatus(GSClientGroupStatus_t _response)
	{
		CSteamID steamIDUser = _response.m_SteamIDUser;
		ulong steamID = _response.m_SteamIDGroup.m_SteamID;
		PlatformUserIdentifierAbs userIdentifier = new UserIdentifierSteam(steamIDUser);
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifier);
		if (clientInfo != null)
		{
			groupStatusResponseDelegate(clientInfo, steamID, _response.m_bMember, _response.m_bOfficer);
		}
	}
}
