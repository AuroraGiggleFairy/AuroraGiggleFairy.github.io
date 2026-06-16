using System.Collections.Generic;
using Platform.LAN;
using Twitch;
using UnityEngine.Scripting;

namespace Platform.Steam;

[Preserve]
[PlatformFactory(EPlatformIdentifier.Steam)]
public class Factory : AbsPlatform
{
	public override string NetworkProtocolName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return "SteamNetworking";
		}
	}

	public override void CreateInstances()
	{
		base.Api = new Api();
		base.User = new User();
		base.AuthenticationServer = new AuthenticationServer();
		base.ServerListAnnouncer = new MasterServerAnnouncer();
		base.LobbyHost = new LobbyHost();
		base.MultiplayerInvitationDialog = new MultiplayerInvitationDialogSteam();
		base.Utils = new Utils();
		if (!base.AsServerOnly && !GameManager.IsDedicatedServer)
		{
			base.AchievementManager = new AchievementManager();
			base.RichPresence = new RichPresence();
			base.InviteListeners = new List<IJoinSessionGameInviteListener>
			{
				new JoinSessionGameInviteListener(),
				DiscordInviteListener.ListenerInstance
			};
			base.EntitlementValidators = new List<IEntitlementValidator>
			{
				new DownloadableContentValidator(),
				new TwitchEntitlementManager()
			};
			base.AuthenticationClient = new AuthenticationClient();
			base.ServerListInterfaces = new List<IServerListInterface>
			{
				new LobbyListInternet(),
				new LobbyListFriends(),
				new LANServerList()
			};
			if (PlatformManager.CrossplatformPlatform == null)
			{
				base.ServerListInterfaces.Add(new MasterServerList(EServerRelationType.Internet));
				base.ServerListInterfaces.Add(new MasterServerList(EServerRelationType.LAN));
				base.ServerListInterfaces.Add(new MasterServerList(EServerRelationType.Friends));
				base.ServerListInterfaces.Add(new MasterServerList(EServerRelationType.Favorites));
				base.ServerListInterfaces.Add(new MasterServerList(EServerRelationType.History));
			}
			base.VirtualKeyboard = new VirtualKeyboard();
		}
		if (!base.AsServerOnly)
		{
			base.Input = new PlayerInputManager();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IPlatformNetworkServer instantiateNetworkServer(ProtocolManager _protocolManager)
	{
		return new NetworkServerSteam(this, _protocolManager);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IPlatformNetworkClient instantiateNetworkClient(ProtocolManager _protocolManager)
	{
		return new NetworkClientSteam(this, _protocolManager);
	}
}
