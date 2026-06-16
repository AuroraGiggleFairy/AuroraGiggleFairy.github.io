using Steamworks;

namespace Platform.Steam;

public class MultiplayerInvitationDialogSteam : IMultiplayerInvitationDialog
{
	[PublicizedFrom(EAccessModifier.Private)]
	public LobbyHost lobbyHost;

	public bool CanShow
	{
		get
		{
			if (lobbyHost != null)
			{
				return lobbyHost.IsInLobby;
			}
			return false;
		}
	}

	public void Init(IPlatform owner)
	{
		lobbyHost = (LobbyHost)owner.LobbyHost;
	}

	public void ShowInviteDialog()
	{
		if (lobbyHost == null)
		{
			Log.Error("[Steam] Cannot open invite dialog, lobby host is null");
			return;
		}
		string lobbyId = lobbyHost.LobbyId;
		ulong _result;
		if (string.IsNullOrEmpty(lobbyId))
		{
			Log.Error("[Steam] Cannot open invite dialog, no lobby id set");
		}
		else if (StringParsers.TryParseUInt64(lobbyId, out _result))
		{
			Log.Out($"[Steam] Opening invite dialog for lobby: {_result}");
			SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(_result));
		}
		else
		{
			Log.Error("[Steam] Cannot open invite dialog, could not parse Steam lobby id: " + lobbyId);
		}
	}
}
