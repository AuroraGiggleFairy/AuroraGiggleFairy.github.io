using System.Collections.Generic;
using Platform.Shared;
using UnityEngine.Scripting;

namespace Platform.EOS;

[Preserve]
[PlatformFactory(EPlatformIdentifier.EOS)]
public class Factory : AbsPlatform
{
	public override string NetworkProtocolName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return "EosNetworking";
		}
	}

	public override void CreateInstances()
	{
		EosBindingsUnityEditor.Init();
		base.Api = new Api();
		base.AuthenticationServer = new AuthServer();
		base.ServerListAnnouncer = new SessionsHost();
		IAntiCheatServer antiCheatServer2;
		if (!GameManager.IsDedicatedServer)
		{
			IAntiCheatServer antiCheatServer = new AntiCheatServerP2P();
			antiCheatServer2 = antiCheatServer;
		}
		else
		{
			IAntiCheatServer antiCheatServer = new AntiCheatServer();
			antiCheatServer2 = antiCheatServer;
		}
		base.AntiCheatServer = antiCheatServer2;
		if (!base.AsServerOnly && !GameManager.IsDedicatedServer)
		{
			base.User = new User();
			base.AuthenticationClient = new AuthClient();
			base.AntiCheatClient = new AntiCheatClientManager();
			base.PlayerReporting = new PlayerReporting();
			SessionsClient sessionsClient = new SessionsClient();
			base.ServerListInterfaces = new List<IServerListInterface>
			{
				sessionsClient,
				new FavoriteServers()
			};
			base.ServerLookupInterface = sessionsClient;
			base.PartyVoice = new Voice();
			base.RemotePlayerFileStorage = new RemotePlayerFileStorage();
			base.IdMappingService = new EosUserIdMapper((Api)base.Api, (User)base.User);
			base.UserDetailsService = new UserDetailsServiceEos();
		}
		if (!base.AsServerOnly)
		{
			base.RemoteFileStorage = new RemoteFileStorage();
		}
		AntiCheatCommon.Init();
	}

	public override bool HasNetworkingEnabled(IList<string> _disabledProtocolNames)
	{
		if (!GameManager.IsDedicatedServer && base.HasNetworkingEnabled(_disabledProtocolNames) && PlatformManager.NativePlatform.User.UserStatus == EUserStatus.LoggedIn && PlatformManager.NativePlatform.User.Permissions.HasMultiplayer())
		{
			return ((User)base.User)?.Permissions.HasMultiplayer() ?? false;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IPlatformNetworkServer instantiateNetworkServer(ProtocolManager _protocolManager)
	{
		return new NetworkServerEos(this, _protocolManager);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IPlatformNetworkClient instantiateNetworkClient(ProtocolManager _protocolManager)
	{
		return new NetworkClientEos(this, _protocolManager);
	}

	public override void Destroy()
	{
		base.Destroy();
		EosBindingsUnityEditor.Shutdown();
	}
}
