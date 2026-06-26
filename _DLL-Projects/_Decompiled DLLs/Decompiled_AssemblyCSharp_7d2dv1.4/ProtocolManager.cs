using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class ProtocolManager : IProtocolManagerProtocolInterface
{
	public enum NetworkType
	{
		None,
		Client,
		Server,
		OfflineServer
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<INetworkClient> clients = new List<INetworkClient>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<INetworkServer> servers = new List<INetworkServer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo currentGameServerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentConnectionAttemptIndex;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasRunningServers
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public NetworkType CurrentMode
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsServer
	{
		get
		{
			if (CurrentMode != NetworkType.Server)
			{
				return CurrentMode == NetworkType.OfflineServer;
			}
			return true;
		}
	}

	public bool IsClient => CurrentMode == NetworkType.Client;

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupProtocols()
	{
		if (servers.Count != 0)
		{
			return;
		}
		string text = GamePrefs.GetString(EnumGamePrefs.ServerDisabledNetworkProtocols);
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(text))
		{
			list.AddRange(text.ToLower().Split(','));
		}
		if (GameUtils.GetLaunchArgument("nounet") != null)
		{
			list.Add("unet");
		}
		if (GameUtils.GetLaunchArgument("noraknet") != null)
		{
			list.Add("raknet");
		}
		if (GameUtils.GetLaunchArgument("nolitenetlib") != null)
		{
			list.Add("litenetlib");
		}
		if (!list.Contains("litenetlib"))
		{
			servers.Add(new NetworkServerLiteNetLib(this));
			clients.Add(new NetworkClientLiteNetLib(this));
		}
		else
		{
			Log.Out("[NET] Disabling protocol: LiteNetLib");
		}
		if (PlatformManager.NativePlatform.HasNetworkingEnabled(list))
		{
			servers.Add(PlatformManager.NativePlatform.GetNetworkingServer(this));
			if (!GameManager.IsDedicatedServer)
			{
				clients.Add(PlatformManager.NativePlatform.GetNetworkingClient(this));
			}
		}
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		if (crossplatformPlatform != null && crossplatformPlatform.HasNetworkingEnabled(list))
		{
			servers.Add(PlatformManager.CrossplatformPlatform.GetNetworkingServer(this));
			if (!GameManager.IsDedicatedServer)
			{
				clients.Add(PlatformManager.CrossplatformPlatform.GetNetworkingClient(this));
			}
		}
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (serverPlatform.Value.AsServerOnly && serverPlatform.Value.HasNetworkingEnabled(list))
			{
				servers.Add(serverPlatform.Value.GetNetworkingServer(this));
			}
		}
	}

	public string GetGamePortsString()
	{
		string text = "";
		string serverPorts = ServerInformationTcpProvider.Instance.GetServerPorts();
		if (!string.IsNullOrEmpty(serverPorts))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += serverPorts;
		}
		string text2 = PlatformManager.MultiPlatform.ServerListAnnouncer?.GetServerPorts();
		if (!string.IsNullOrEmpty(text2))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		int basePort = GamePrefs.GetInt(EnumGamePrefs.ServerPort);
		for (int i = 0; i < servers.Count; i++)
		{
			string serverPorts2 = servers[i].GetServerPorts(basePort);
			if (!string.IsNullOrEmpty(serverPorts2))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += serverPorts2;
			}
		}
		return text;
	}

	public void Update()
	{
		for (int i = 0; i < servers.Count; i++)
		{
			servers[i].Update();
		}
		for (int j = 0; j < clients.Count; j++)
		{
			clients[j].Update();
		}
	}

	public void LateUpdate()
	{
		for (int i = 0; i < servers.Count; i++)
		{
			servers[i].LateUpdate();
		}
		for (int j = 0; j < clients.Count; j++)
		{
			clients[j].LateUpdate();
		}
	}

	public void StartOfflineServer()
	{
		Log.Out("NET: Starting offline server.");
		CurrentMode = NetworkType.OfflineServer;
	}

	public NetworkConnectionError StartServers(string _password)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode || !PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer())
		{
			Log.Warning($"NET: User unable to create online server. User status: {PlatformManager.MultiPlatform.User.UserStatus}, Multiplayer allowed: {PermissionsManager.IsMultiplayerAllowed()}, Host Multiplayer allowed: {PermissionsManager.CanHostMultiplayer()}");
			StartOfflineServer();
			return NetworkConnectionError.NoError;
		}
		Log.Out("NET: Starting server protocols");
		SetupProtocols();
		CurrentMode = NetworkType.Server;
		NetworkConnectionError networkConnectionError = NetworkConnectionError.NoError;
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerPort);
		if (num < 1024 || num > 65530)
		{
			Log.Error($"NET: Starting server protocols failed: Invalid ServerPort {num}, must be within 1024 and 65530");
			return NetworkConnectionError.InvalidPort;
		}
		for (int i = 0; i < servers.Count; i++)
		{
			networkConnectionError = servers[i].StartServer(num, _password);
			if (networkConnectionError != NetworkConnectionError.NoError)
			{
				break;
			}
			HasRunningServers = true;
		}
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			for (int j = 0; j < servers.Count; j++)
			{
				servers[j].StopServer();
			}
			HasRunningServers = false;
			CurrentMode = NetworkType.None;
			Log.Error("NET: Starting server protocols failed: " + networkConnectionError.ToStringCached());
		}
		return networkConnectionError;
	}

	public void MakeServerOffline()
	{
		if (CurrentMode == NetworkType.Server)
		{
			StopServersOnly();
			CurrentMode = NetworkType.OfflineServer;
		}
	}

	public void SetServerPassword(string _password)
	{
		if (CurrentMode != NetworkType.Server)
		{
			return;
		}
		foreach (INetworkServer server in servers)
		{
			server.SetServerPassword(_password);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopServersOnly()
	{
		Log.Out("NET: Stopping server protocols");
		foreach (INetworkServer server in servers)
		{
			server.StopServer();
		}
		HasRunningServers = false;
	}

	public void StopServers()
	{
		StopServersOnly();
		ThreadManager.StartCoroutine(resetStateLater(0.25f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resetStateLater(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		CurrentMode = NetworkType.None;
	}

	public void ConnectToServer(GameServerInfo _gameServerInfo)
	{
		SetupProtocols();
		CurrentMode = NetworkType.Client;
		currentGameServerInfo = _gameServerInfo;
		clients[currentConnectionAttemptIndex].Connect(_gameServerInfo);
	}

	public void InvalidPasswordEv()
	{
		CurrentMode = NetworkType.None;
		currentGameServerInfo = null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_InvalidPassword();
	}

	public void ConnectionFailedEv(string _msg)
	{
		currentConnectionAttemptIndex++;
		if (currentConnectionAttemptIndex < clients.Count)
		{
			clients[currentConnectionAttemptIndex].Connect(currentGameServerInfo);
			return;
		}
		CurrentMode = NetworkType.None;
		currentConnectionAttemptIndex = 0;
		currentGameServerInfo = null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_ConnectionFailed(_msg);
	}

	public void DisconnectedFromServerEv(string _msg)
	{
		CurrentMode = NetworkType.None;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DisconnectedFromServer(_msg);
	}

	public void Disconnect()
	{
		currentConnectionAttemptIndex = 0;
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].Disconnect();
		}
		if (IsClient)
		{
			CurrentMode = NetworkType.None;
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectFromServer();
		}
	}

	public void SetLatencySimulation(bool _enable, int _min, int _max)
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].SetLatencySimulation(_enable, _min, _max);
		}
		for (int j = 0; j < servers.Count; j++)
		{
			servers[j].SetLatencySimulation(_enable, _min, _max);
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].SetPacketLossSimulation(_enable, _chance);
		}
		for (int j = 0; j < servers.Count; j++)
		{
			servers[j].SetPacketLossSimulation(_enable, _chance);
		}
	}

	public void EnableNetworkStatistics()
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].EnableStatistics();
		}
		for (int j = 0; j < servers.Count; j++)
		{
			servers[j].EnableStatistics();
		}
	}

	public void DisableNetworkStatistics()
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].DisableStatistics();
		}
		for (int j = 0; j < servers.Count; j++)
		{
			servers[j].DisableStatistics();
		}
	}

	public string PrintNetworkStatistics()
	{
		string text = "";
		for (int i = 0; i < clients.Count; i++)
		{
			text = text + "CLIENT " + i + "\n";
			text = text + clients[i].PrintNetworkStatistics() + "\n";
		}
		for (int j = 0; j < servers.Count; j++)
		{
			text = text + "SERVER " + j + "\n";
			text = text + servers[j].PrintNetworkStatistics() + "\n";
		}
		return text;
	}

	public void ResetNetworkStatistics()
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].ResetNetworkStatistics();
		}
		for (int j = 0; j < servers.Count; j++)
		{
			servers[j].ResetNetworkStatistics();
		}
	}
}
