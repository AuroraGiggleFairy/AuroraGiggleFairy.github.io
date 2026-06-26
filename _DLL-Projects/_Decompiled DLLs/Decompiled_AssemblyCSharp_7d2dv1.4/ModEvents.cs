using System.Collections.Generic;
using System.Text;

public static class ModEvents
{
	public static readonly ModEvent GameAwake = new ModEvent
	{
		eventName = "GameAwake"
	};

	public static readonly ModEvent GameStartDone = new ModEvent
	{
		eventName = "GameStartDone"
	};

	public static readonly ModEvent GameUpdate = new ModEvent
	{
		eventName = "GameUpdate"
	};

	public static readonly ModEvent WorldShuttingDown = new ModEvent
	{
		eventName = "WorldShuttingDown"
	};

	public static readonly ModEvent GameShutdown = new ModEvent
	{
		eventName = "GameShutdown"
	};

	public static readonly ModEvent UnityUpdate = new ModEvent
	{
		eventName = "UnityUpdate"
	};

	public static readonly ModEventInterruptible<ClientInfo, string, StringBuilder> PlayerLogin = new ModEventInterruptible<ClientInfo, string, StringBuilder>
	{
		eventName = "PlayerLogin"
	};

	public static readonly ModEvent<ClientInfo, int, PlayerProfile> PlayerSpawning = new ModEvent<ClientInfo, int, PlayerProfile>
	{
		eventName = "PlayerSpawning"
	};

	public static readonly ModEvent<ClientInfo, RespawnType, Vector3i> PlayerSpawnedInWorld = new ModEvent<ClientInfo, RespawnType, Vector3i>
	{
		eventName = "PlayerSpawnedInWorld"
	};

	public static readonly ModEvent<ClientInfo, bool> PlayerDisconnected = new ModEvent<ClientInfo, bool>
	{
		eventName = "PlayerDisconnected"
	};

	public static readonly ModEvent<ClientInfo, PlayerDataFile> SavePlayerData = new ModEvent<ClientInfo, PlayerDataFile>
	{
		eventName = "SavePlayerData"
	};

	public static readonly ModEventInterruptible<ClientInfo, EnumGameMessages, string, string, string> GameMessage = new ModEventInterruptible<ClientInfo, EnumGameMessages, string, string, string>
	{
		eventName = "GameMessage"
	};

	public static readonly ModEventInterruptible<ClientInfo, EChatType, int, string, string, List<int>> ChatMessage = new ModEventInterruptible<ClientInfo, EChatType, int, string, string, List<int>>
	{
		eventName = "ChatMessage"
	};

	public static readonly ModEvent<Chunk> CalcChunkColorsDone = new ModEvent<Chunk>
	{
		eventName = "CalcChunkColorsDone"
	};

	public static readonly ModEvent<Entity, Entity> EntityKilled = new ModEvent<Entity, Entity>
	{
		eventName = "EntityKilled"
	};
}
