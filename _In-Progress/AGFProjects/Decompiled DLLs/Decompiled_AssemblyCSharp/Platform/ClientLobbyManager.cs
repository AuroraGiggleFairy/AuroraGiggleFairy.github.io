using System.Collections.Generic;

namespace Platform;

public class ClientLobbyManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Lobby
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PlatformLobbyId id;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<ClientInfo> clients = new List<ClientInfo>();

		public PlatformLobbyId Id => id;

		public bool IsEmpty => clients.Count == 0;

		public IReadOnlyList<ClientInfo> Clients => clients;

		public Lobby(PlatformLobbyId id)
		{
			this.id = id;
		}

		public Lobby(EPlatformIdentifier platform, string lobbyId)
		{
			id = new PlatformLobbyId(platform, lobbyId);
		}

		public void AddClient(ClientInfo client)
		{
			clients.Add(client);
			Log.Out($"[ClientLobbyManager] registered member {client.playerName} for client lobby {id.PlatformIdentifier} : {id.LobbyId}. Total members: {clients.Count}");
		}

		public void RemoveClient(ClientInfo client)
		{
			if (clients.Remove(client))
			{
				Log.Out($"[ClientLobbyManager] removed member {client.playerName} from client lobby {id.PlatformIdentifier} : {id.LobbyId}. Total members: {clients.Count}");
			}
			else
			{
				Log.Warning($"[ClientLobbyManager] remove member {client.playerName} from client lobby {id.PlatformIdentifier} : {id.LobbyId} failed. They are not a member");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<EPlatformIdentifier, Lobby> lobbies = new Dictionary<EPlatformIdentifier, Lobby>();

	public ClientLobbyManager()
	{
		ConnectionManager.OnClientDisconnected += OnClientDisconnected;
	}

	public bool TryGetLobbyId(EPlatformIdentifier platform, out PlatformLobbyId lobbyId)
	{
		lock (lockObj)
		{
			if (lobbies.TryGetValue(platform, out var value))
			{
				lobbyId = value.Id;
				return true;
			}
			lobbyId = null;
			return false;
		}
	}

	public void RegisterLobbyClient(PlatformLobbyId platformLobbyId, ClientInfo client, bool overwrite = false)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.Contains(client))
		{
			Log.Warning($"[ClientLobbyManager] could not register {client.playerName} for client lobby {platformLobbyId.PlatformIdentifier} : {platformLobbyId.LobbyId} as they are no longer connected");
			return;
		}
		lock (lockObj)
		{
			if (!lobbies.TryGetValue(platformLobbyId.PlatformIdentifier, out var value))
			{
				Log.Out($"[ClientLobbyManager] registering new lobby for client platform {platformLobbyId.PlatformIdentifier} : {platformLobbyId.LobbyId}");
				value = new Lobby(platformLobbyId);
				value.AddClient(client);
				lobbies.Add(platformLobbyId.PlatformIdentifier, value);
			}
			else if (value.Id.LobbyId.Equals(platformLobbyId.LobbyId))
			{
				value.AddClient(client);
			}
			else if (overwrite)
			{
				Log.Warning($"[ClientLobbyManager] overwriting existing lobby for {platformLobbyId.PlatformIdentifier}");
				Lobby lobby = new Lobby(platformLobbyId);
				lobby.AddClient(client);
				foreach (ClientInfo client2 in value.Clients)
				{
					client2.SendPackage(NetPackageManager.GetPackage<NetPackageLobbyJoin>().Setup(platformLobbyId));
					lobby.AddClient(client2);
				}
				lobbies[platformLobbyId.PlatformIdentifier] = lobby;
			}
			else
			{
				Log.Warning($"[ClientLobbyManager] a different client lobby already registered for {platformLobbyId.PlatformIdentifier}, sending to client");
				client.SendPackage(NetPackageManager.GetPackage<NetPackageLobbyJoin>().Setup(value.Id));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientDisconnected(ClientInfo client)
	{
		lock (lockObj)
		{
			if (lobbies.TryGetValue(client.PlatformId.PlatformIdentifier, out var value))
			{
				value.RemoveClient(client);
				if (value.IsEmpty)
				{
					Log.Out($"[ClientLobbyManager] removing registered lobby {value.Id.PlatformIdentifier} : {value.Id.LobbyId}");
					lobbies.Remove(client.PlatformId.PlatformIdentifier);
				}
			}
		}
	}
}
