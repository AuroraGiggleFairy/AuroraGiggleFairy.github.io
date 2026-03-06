using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Platform.MultiPlatform;

[Preserve]
public class Factory : IPlatform
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IServerListInterface> serverListInterfaces;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AsServerOnly { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsCrossplatform { get; set; }

	public EPlatformIdentifier PlatformIdentifier => EPlatformIdentifier.None;

	public string PlatformDisplayName => null;

	public IPlatformApi Api => null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IUserClient User
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IAuthenticationClient AuthenticationClient => null;

	public IAuthenticationServer AuthenticationServer => null;

	public IList<IServerListInterface> ServerListInterfaces
	{
		get
		{
			if (serverListInterfaces != null)
			{
				return serverListInterfaces;
			}
			serverListInterfaces = new List<IServerListInterface>();
			IList<IServerListInterface> list = PlatformManager.CrossplatformPlatform?.ServerListInterfaces;
			if (list != null)
			{
				serverListInterfaces.AddRange(list);
			}
			list = PlatformManager.NativePlatform.ServerListInterfaces;
			if (list != null)
			{
				serverListInterfaces.AddRange(list);
			}
			return serverListInterfaces;
		}
	}

	public IServerListInterface ServerLookupInterface => null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IMasterServerAnnouncer ServerListAnnouncer
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ILobbyHost LobbyHost => null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlayerInteractionsRecorder PlayerInteractionsRecorder
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IGameplayNotifier GameplayNotifier
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IList<IJoinSessionGameInviteListener> InviteListeners => PlatformManager.NativePlatform.InviteListeners;

	public IMultiplayerInvitationDialog MultiplayerInvitationDialog => PlatformManager.NativePlatform.MultiplayerInvitationDialog;

	public IPartyVoice PartyVoice => PlatformManager.CrossplatformPlatform?.PartyVoice ?? PlatformManager.NativePlatform.PartyVoice;

	public IUtils Utils => null;

	public IPlatformMemory Memory => PlatformManager.NativePlatform.Memory;

	public IAntiCheatClient AntiCheatClient => PlatformManager.CrossplatformPlatform?.AntiCheatClient ?? PlatformManager.NativePlatform.AntiCheatClient;

	public IAntiCheatServer AntiCheatServer => PlatformManager.CrossplatformPlatform?.AntiCheatServer ?? PlatformManager.NativePlatform.AntiCheatServer;

	public IUserIdentifierMappingService IdMappingService => null;

	public IUserDetailsService UserDetailsService => null;

	public IPlayerReporting PlayerReporting => PlatformManager.CrossplatformPlatform?.PlayerReporting ?? PlatformManager.NativePlatform.PlayerReporting;

	public ITextCensor TextCensor
	{
		get
		{
			object obj = PlatformManager.CrossplatformPlatform?.TextCensor;
			if (obj == null)
			{
				IPlatform nativePlatform = PlatformManager.NativePlatform;
				if (nativePlatform == null)
				{
					return null;
				}
				obj = nativePlatform.TextCensor;
			}
			return (ITextCensor)obj;
		}
	}

	public IRemoteFileStorage RemoteFileStorage => PlatformManager.CrossplatformPlatform?.RemoteFileStorage ?? PlatformManager.NativePlatform.RemoteFileStorage;

	public IRemotePlayerFileStorage RemotePlayerFileStorage => PlatformManager.CrossplatformPlatform?.RemotePlayerFileStorage ?? PlatformManager.NativePlatform.RemotePlayerFileStorage;

	public IList<IEntitlementValidator> EntitlementValidators => PlatformManager.NativePlatform.EntitlementValidators;

	public IPlatformNetworkServer NetworkServer => null;

	public PlayerInputManager Input => null;

	public IVirtualKeyboard VirtualKeyboard => null;

	public IAchievementManager AchievementManager => null;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IRichPresence RichPresence
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public IApplicationStateController ApplicationState => PlatformManager.NativePlatform.ApplicationState;

	public void Init()
	{
		User.Init(this);
		ServerListAnnouncer.Init(this);
		RichPresence.Init(this);
	}

	public bool HasNetworkingEnabled(IList<string> _disabledProtocolNames)
	{
		return false;
	}

	public INetworkServer GetNetworkingServer(ProtocolManager _protocolManager)
	{
		throw new NotImplementedException();
	}

	public INetworkClient GetNetworkingClient(ProtocolManager _protocolManager)
	{
		throw new NotImplementedException();
	}

	public void UserAdded(PlatformUserIdentifierAbs _id, bool _isPrimary)
	{
		PlatformManager.NativePlatform.UserAdded(_id, _isPrimary);
		PlatformManager.CrossplatformPlatform?.UserAdded(_id, _isPrimary);
	}

	public string[] GetArgumentsForRelaunch()
	{
		return new string[0];
	}

	public void CreateInstances()
	{
		User = new User();
		ServerListAnnouncer = new ServerListAnnouncer();
		RichPresence = new RichPresence();
		PlayerInteractionsRecorder = new PlayerInteractionsRecorderMulti();
	}

	public void Update()
	{
	}

	public void LateUpdate()
	{
	}

	public void Destroy()
	{
		ServerListAnnouncer = null;
		User?.Destroy();
		User = null;
	}
}
