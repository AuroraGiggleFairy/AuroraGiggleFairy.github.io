using System;
using System.Collections.Generic;
using InControl;

namespace Platform;

public abstract class AbsPlatform : IPlatform
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public IPlatformNetworkServer NetworkServer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IPlatformNetworkClient NetworkClient;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AsServerOnly { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsCrossplatform { get; set; }

	public string PlatformDisplayName => PlatformManager.GetPlatformDisplayName(PlatformIdentifier);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EPlatformIdentifier PlatformIdentifier { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlatformApi Api
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IUserClient User
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IAuthenticationClient AuthenticationClient
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IAuthenticationServer AuthenticationServer
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IList<IServerListInterface> ServerListInterfaces
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IServerListInterface ServerLookupInterface
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IMasterServerAnnouncer ServerListAnnouncer
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ILobbyHost LobbyHost
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlayerInteractionsRecorder PlayerInteractionsRecorder
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IGameplayNotifier GameplayNotifier
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IList<IJoinSessionGameInviteListener> InviteListeners
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IMultiplayerInvitationDialog MultiplayerInvitationDialog
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPartyVoice PartyVoice
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IUtils Utils
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlatformMemory Memory
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IAntiCheatClient AntiCheatClient
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IAntiCheatServer AntiCheatServer
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IUserIdentifierMappingService IdMappingService
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IUserDetailsService UserDetailsService
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IPlayerReporting PlayerReporting
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ITextCensor TextCensor
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IRemoteFileStorage RemoteFileStorage
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IRemotePlayerFileStorage RemotePlayerFileStorage
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IList<IEntitlementValidator> EntitlementValidators
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager Input
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IVirtualKeyboard VirtualKeyboard
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IAchievementManager AchievementManager
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IRichPresence RichPresence
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public IApplicationStateController ApplicationState
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual string NetworkProtocolName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsPlatform()
	{
		Type typeFromHandle = typeof(PlatformFactoryAttribute);
		object[] customAttributes = GetType().GetCustomAttributes(typeFromHandle, inherit: false);
		if (customAttributes.Length != 1)
		{
			throw new Exception("Platform has no PlatformFactory attribute");
		}
		PlatformFactoryAttribute platformFactoryAttribute = (PlatformFactoryAttribute)customAttributes[0];
		PlatformIdentifier = platformFactoryAttribute.TargetPlatform;
	}

	public virtual void Init()
	{
		Log.Out("[Platform] Initializing " + PlatformIdentifier);
		Api?.Init(this);
		User?.Init(this);
		AuthenticationClient?.Init(this);
		AuthenticationServer?.Init(this);
		if (ServerListInterfaces != null)
		{
			foreach (IServerListInterface serverListInterface in ServerListInterfaces)
			{
				serverListInterface.Init(this);
			}
		}
		ServerListAnnouncer?.Init(this);
		if (InviteListeners != null)
		{
			foreach (IJoinSessionGameInviteListener inviteListener in InviteListeners)
			{
				inviteListener?.Init(this);
			}
		}
		MultiplayerInvitationDialog?.Init(this);
		LobbyHost?.Init(this);
		PlayerInteractionsRecorder?.Init(this);
		GameplayNotifier?.Init(this);
		PartyVoice?.Init(this);
		Utils?.Init(this);
		Utils?.ClearTempFiles();
		if (Utils != null)
		{
			InputManager.OnDeviceDetached += Utils.ControllerDisconnected;
		}
		AntiCheatClient?.Init(this);
		AntiCheatServer?.Init(this);
		PlayerReporting?.Init(this);
		TextCensor?.Init(this);
		VirtualKeyboard?.Init(this);
		AchievementManager?.Init(this);
		RichPresence?.Init(this);
		RemoteFileStorage?.Init(this);
		RemotePlayerFileStorage?.Init(this);
		if (EntitlementValidators != null)
		{
			foreach (IEntitlementValidator entitlementValidator in EntitlementValidators)
			{
				entitlementValidator?.Init(this);
			}
		}
		ApplicationState?.Init(this);
		UserDetailsService?.Init(this);
		if (!GameManager.IsDedicatedServer && !AsServerOnly)
		{
			Api?.InitClientApis();
		}
		if (GameManager.IsDedicatedServer)
		{
			Api?.InitServerApis();
		}
	}

	public virtual bool HasNetworkingEnabled(IList<string> _disabledProtocolNames)
	{
		string networkProtocolName = NetworkProtocolName;
		if (string.IsNullOrEmpty(networkProtocolName))
		{
			return false;
		}
		string text = networkProtocolName.ToLowerInvariant();
		int num;
		if (GameUtils.GetLaunchArgument("no" + text) == null)
		{
			num = ((!_disabledProtocolNames.Contains(text)) ? 1 : 0);
			if (num != 0)
			{
				goto IL_004a;
			}
		}
		else
		{
			num = 0;
		}
		Log.Out("[NET] Disabling protocol: " + networkProtocolName);
		goto IL_004a;
		IL_004a:
		return (byte)num != 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IPlatformNetworkServer instantiateNetworkServer(ProtocolManager _protocolManager)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IPlatformNetworkClient instantiateNetworkClient(ProtocolManager _protocolManager)
	{
		return null;
	}

	public INetworkServer GetNetworkingServer(ProtocolManager _protocolManager)
	{
		return NetworkServer ?? (NetworkServer = instantiateNetworkServer(_protocolManager));
	}

	public INetworkClient GetNetworkingClient(ProtocolManager _protocolManager)
	{
		return NetworkClient ?? (NetworkClient = instantiateNetworkClient(_protocolManager));
	}

	public virtual void UserAdded(PlatformUserIdentifierAbs _id, bool _isPrimary)
	{
	}

	public virtual string[] GetArgumentsForRelaunch()
	{
		return new string[0];
	}

	public abstract void CreateInstances();

	public virtual void Update()
	{
		Api?.Update();
		ServerListAnnouncer?.Update();
		AntiCheatServer?.Update();
		Input?.Update();
		TextCensor?.Update();
		ApplicationState?.Update();
	}

	public void LateUpdate()
	{
	}

	public virtual void Destroy()
	{
		RichPresence = null;
		AchievementManager?.Destroy();
		AchievementManager = null;
		Input = null;
		ApplicationState?.Destroy();
		ApplicationState = null;
		VirtualKeyboard?.Destroy();
		VirtualKeyboard = null;
		NetworkClient = null;
		NetworkServer = null;
		PlayerReporting = null;
		TextCensor = null;
		AntiCheatServer?.Destroy();
		AntiCheatServer = null;
		AntiCheatClient?.Destroy();
		AntiCheatClient = null;
		Memory = null;
		Utils?.ClearTempFiles();
		Utils = null;
		PartyVoice?.Destroy();
		PartyVoice = null;
		LobbyHost = null;
		PlayerInteractionsRecorder?.Destroy();
		PlayerInteractionsRecorder = null;
		InviteListeners = null;
		ServerListAnnouncer = null;
		ServerListInterfaces = null;
		AuthenticationServer = null;
		AuthenticationClient?.Destroy();
		AuthenticationClient = null;
		User?.Destroy();
		User = null;
		Api?.Destroy();
		Api = null;
	}
}
