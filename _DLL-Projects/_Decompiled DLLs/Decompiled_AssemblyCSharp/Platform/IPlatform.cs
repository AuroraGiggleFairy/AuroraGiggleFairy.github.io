using System.Collections.Generic;

namespace Platform;

public interface IPlatform
{
	bool AsServerOnly { get; set; }

	bool IsCrossplatform { get; set; }

	EPlatformIdentifier PlatformIdentifier { get; }

	string PlatformDisplayName { get; }

	IPlatformApi Api { get; }

	IUserClient User { get; }

	IAuthenticationClient AuthenticationClient { get; }

	IAuthenticationServer AuthenticationServer { get; }

	IList<IServerListInterface> ServerListInterfaces { get; }

	IServerListInterface ServerLookupInterface { get; }

	IMasterServerAnnouncer ServerListAnnouncer { get; }

	IList<IJoinSessionGameInviteListener> InviteListeners { get; }

	IMultiplayerInvitationDialog MultiplayerInvitationDialog { get; }

	ILobbyHost LobbyHost { get; }

	IPlayerInteractionsRecorder PlayerInteractionsRecorder { get; }

	IGameplayNotifier GameplayNotifier { get; }

	IPartyVoice PartyVoice { get; }

	IUtils Utils { get; }

	IPlatformMemory Memory { get; }

	IAntiCheatClient AntiCheatClient { get; }

	IAntiCheatServer AntiCheatServer { get; }

	IUserIdentifierMappingService IdMappingService { get; }

	IUserDetailsService UserDetailsService { get; }

	IPlayerReporting PlayerReporting { get; }

	ITextCensor TextCensor { get; }

	IRemoteFileStorage RemoteFileStorage { get; }

	IRemotePlayerFileStorage RemotePlayerFileStorage { get; }

	IList<IEntitlementValidator> EntitlementValidators { get; }

	PlayerInputManager Input { get; }

	IVirtualKeyboard VirtualKeyboard { get; }

	IAchievementManager AchievementManager { get; }

	IRichPresence RichPresence { get; }

	IApplicationStateController ApplicationState { get; }

	void CreateInstances();

	void Init();

	bool HasNetworkingEnabled(IList<string> _disabledProtocolNames);

	INetworkServer GetNetworkingServer(ProtocolManager _protocolManager);

	INetworkClient GetNetworkingClient(ProtocolManager _protocolManager);

	void UserAdded(PlatformUserIdentifierAbs _id, bool _isPrimary);

	string[] GetArgumentsForRelaunch();

	void Update();

	void LateUpdate();

	void Destroy();
}
