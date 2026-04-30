using System;
using System.Collections;
using Steamworks;

namespace Platform.Steam;

[PublicizedFrom(EAccessModifier.Internal)]
public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string LobbyMarker = "Lobby:";

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public Callback<GameServerChangeRequested_t> m_friends_serverchange;

	public void Init(IPlatform _owner)
	{
		if (m_friends_serverchange == null)
		{
			m_friends_serverchange = Callback<GameServerChangeRequested_t>.Create(Friends_GameServerChangeRequested);
		}
		_owner.Api.ClientApiInitialized += [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length - 1; i++)
			{
				if (commandLineArgs[i] == "+connect_lobby" && ulong.TryParse(commandLineArgs[i + 1], out var result))
				{
					SetLobby(new CSteamID(result));
				}
			}
		};
	}

	public (string invite, string password) TakePendingInvite()
	{
		string item = pendingInvite;
		pendingInvite = null;
		string item2 = pendingPassword;
		pendingPassword = null;
		return (invite: item, password: item2);
	}

	public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
	{
		if (string.IsNullOrEmpty(_invite))
		{
			_onFinished?.Invoke(obj: false);
			yield break;
		}
		if (_invite.StartsWith("Lobby:"))
		{
			PlatformManager.NativePlatform.LobbyHost?.JoinLobby(_invite.Substring("Lobby:".Length), null);
			_onFinished?.Invoke(obj: true);
			yield break;
		}
		string[] array = _invite.Split(':');
		string ip = "";
		int port = 0;
		if (array.Length == 2)
		{
			ip = array[0];
			port = Convert.ToInt32(array[1]);
		}
		yield return InviteManager.HandleIpPortInvite(ip, port, _password, _onFinished);
	}

	public string GetListenerIdentifier()
	{
		return "STM";
	}

	public void SetLobby(CSteamID _lobbyId)
	{
		pendingInvite = "Lobby:" + _lobbyId.m_SteamID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Friends_GameServerChangeRequested(GameServerChangeRequested_t _value)
	{
		Log.Out("[Steamworks.NET] Friends_GameServerChangeRequested");
		pendingInvite = _value.m_rgchServer;
		pendingPassword = _value.m_rgchPassword;
	}
}
