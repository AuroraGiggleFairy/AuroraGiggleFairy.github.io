using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class ModEvents
{
	public struct SGameFocusData(bool _isFocused)
	{
		public readonly bool IsFocused = _isFocused;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SGameAwakeData
	{
	}

	public struct SGameStartingData(bool _asServer)
	{
		public readonly bool AsServer = _asServer;
	}

	public struct SMainMenuOpeningData(bool _openedBefore)
	{
		public readonly bool OpenedBefore = _openedBefore;
	}

	public struct SMainMenuOpenedData(bool _firstTimeOpen)
	{
		public readonly bool FirstTimeOpen = _firstTimeOpen;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SGameStartDoneData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SCreateWorldDoneData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SGameUpdateData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SWorldShuttingDownData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SGameShutdownData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SServerRegisteredData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SUnityUpdateData
	{
	}

	public struct SPlayerLoginData(ClientInfo _clientInfo, string _compatibilityVersion)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly string CompatibilityVersion = _compatibilityVersion;

		public string CustomMessage = null;
	}

	public struct SPlayerJoinedGameData(ClientInfo _clientInfo)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;
	}

	public readonly struct SPlayerSpawningData(ClientInfo _clientInfo, int _chunkViewDim, PlayerProfile _playerProfile)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly int ChunkViewDim = _chunkViewDim;

		public readonly PlayerProfile PlayerProfile = _playerProfile;
	}

	public struct SPlayerSpawnedInWorldData(ClientInfo _clientInfo, bool _isLocalPlayer, int _entityId, RespawnType _respawnType, Vector3i _position)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly bool IsLocalPlayer = _isLocalPlayer;

		public readonly int EntityId = _entityId;

		public readonly RespawnType RespawnType = _respawnType;

		public readonly Vector3i Position = _position;
	}

	public struct SPlayerDisconnectedData(ClientInfo _clientInfo, bool _gameShuttingDown)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly bool GameShuttingDown = _gameShuttingDown;
	}

	public struct SSavePlayerDataData(ClientInfo _clientInfo, PlayerDataFile _playerDataFile)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly PlayerDataFile PlayerDataFile = _playerDataFile;
	}

	public struct SGameMessageData(ClientInfo _clientInfo, EnumGameMessages _messageType, string _mainName, string _secondaryName)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly EnumGameMessages MessageType = _messageType;

		public readonly string MainName = _mainName;

		public readonly string SecondaryName = _secondaryName;
	}

	public struct SChatMessageData(ClientInfo _clientInfo, EChatType _chatType, int _senderEntityId, string _message, string _mainName, List<int> _recipientEntityIds)
	{
		public readonly ClientInfo ClientInfo = _clientInfo;

		public readonly EChatType ChatType = _chatType;

		public readonly int SenderEntityId = _senderEntityId;

		public readonly string Message = _message;

		public readonly string MainName = _mainName;

		public readonly List<int> RecipientEntityIds = _recipientEntityIds;
	}

	public struct SCalcChunkColorsDoneData(Chunk _chunk)
	{
		public readonly Chunk Chunk = _chunk;
	}

	public struct SEntityKilledData(Entity _killedEntitiy, Entity _killingEntity)
	{
		public readonly Entity KilledEntitiy = _killedEntitiy;

		public readonly Entity KillingEntity = _killingEntity;
	}

	public enum EModEventResult
	{
		Continue,
		StopHandlersRunVanilla,
		StopHandlersAndVanilla
	}

	public delegate void ModEventHandlerDelegate<TData>(ref TData _data) where TData : struct;

	public delegate EModEventResult ModEventInterruptibleHandlerDelegate<TData>(ref TData _data) where TData : struct;

	public abstract class ModEventAbs<TDelegate> where TDelegate : Delegate
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public class Receiver
		{
			public readonly Mod Mod;

			public readonly TDelegate DelegateFunc;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly bool coreGame;

			public string ModName
			{
				get
				{
					if (Mod != null)
					{
						return Mod.Name;
					}
					if (coreGame)
					{
						return "-GameCore-";
					}
					return "-UnknownMod-";
				}
			}

			public Receiver(Mod _mod, TDelegate _handler, bool _coreGame = false)
			{
				Mod = _mod;
				DelegateFunc = _handler;
				coreGame = _coreGame;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string eventName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly List<Receiver> receivers = new List<Receiver>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public ModEventAbs(string _eventName)
		{
			eventName = _eventName;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void RegisterHandler(TDelegate _handlerFunc)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			Assembly assembly = typeof(ModEvents).Assembly;
			bool coreGame = false;
			Mod mod = null;
			if (callingAssembly.Equals(assembly))
			{
				coreGame = true;
			}
			else
			{
				mod = ModManager.GetModForAssembly(callingAssembly);
				if (mod == null)
				{
					Log.Warning("[MODS] Could not find mod that tries to register a handler for event " + eventName);
				}
			}
			receivers.Add(new Receiver(mod, _handlerFunc, coreGame));
		}

		public void UnregisterHandler(TDelegate _handlerFunc)
		{
			for (int num = receivers.Count - 1; num >= 0; num--)
			{
				if (receivers[num].DelegateFunc.Equals(_handlerFunc))
				{
					receivers.RemoveAt(num);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void LogError(Exception _e, Receiver _currentMod)
		{
			Log.Error("[MODS] Error while executing ModEvent \"" + eventName + "\" on mod \"" + _currentMod.ModName + "\"");
			Log.Exception(_e);
		}
	}

	public class ModEvent<TData> : ModEventAbs<ModEventHandlerDelegate<TData>> where TData : struct
	{
		public ModEvent([CallerMemberName] string _eventName = null)
			: base(_eventName)
		{
		}

		public void Invoke(ref TData _data)
		{
			foreach (Receiver receiver in receivers)
			{
				try
				{
					receiver.DelegateFunc(ref _data);
				}
				catch (Exception e)
				{
					LogError(e, receiver);
				}
			}
		}
	}

	public class ModEventInterruptible<TData> : ModEventAbs<ModEventInterruptibleHandlerDelegate<TData>> where TData : struct
	{
		public ModEventInterruptible([CallerMemberName] string _eventName = null)
			: base(_eventName)
		{
		}

		public (EModEventResult, Mod) Invoke(ref TData _data)
		{
			foreach (Receiver receiver in receivers)
			{
				try
				{
					EModEventResult eModEventResult = receiver.DelegateFunc(ref _data);
					if (eModEventResult != EModEventResult.Continue)
					{
						return (eModEventResult, receiver.Mod);
					}
				}
				catch (Exception e)
				{
					LogError(e, receiver);
				}
			}
			return (EModEventResult.Continue, null);
		}
	}

	public static readonly ModEvent<SGameFocusData> GameFocus = new ModEvent<SGameFocusData>("GameFocus");

	public static readonly ModEvent<SGameAwakeData> GameAwake = new ModEvent<SGameAwakeData>("GameAwake");

	public static readonly ModEvent<SGameStartingData> GameStarting = new ModEvent<SGameStartingData>("GameStarting");

	public static readonly ModEventInterruptible<SMainMenuOpeningData> MainMenuOpening = new ModEventInterruptible<SMainMenuOpeningData>("MainMenuOpening");

	public static readonly ModEvent<SMainMenuOpenedData> MainMenuOpened = new ModEvent<SMainMenuOpenedData>("MainMenuOpened");

	public static readonly ModEvent<SGameStartDoneData> GameStartDone = new ModEvent<SGameStartDoneData>("GameStartDone");

	public static readonly ModEvent<SCreateWorldDoneData> CreateWorldDone = new ModEvent<SCreateWorldDoneData>("CreateWorldDone");

	public static readonly ModEvent<SGameUpdateData> GameUpdate = new ModEvent<SGameUpdateData>("GameUpdate");

	public static readonly ModEvent<SWorldShuttingDownData> WorldShuttingDown = new ModEvent<SWorldShuttingDownData>("WorldShuttingDown");

	public static readonly ModEvent<SGameShutdownData> GameShutdown = new ModEvent<SGameShutdownData>("GameShutdown");

	public static readonly ModEvent<SServerRegisteredData> ServerRegistered = new ModEvent<SServerRegisteredData>("ServerRegistered");

	public static readonly ModEvent<SUnityUpdateData> UnityUpdate = new ModEvent<SUnityUpdateData>("UnityUpdate");

	public static readonly ModEventInterruptible<SPlayerLoginData> PlayerLogin = new ModEventInterruptible<SPlayerLoginData>("PlayerLogin");

	public static readonly ModEvent<SPlayerJoinedGameData> PlayerJoinedGame = new ModEvent<SPlayerJoinedGameData>("PlayerJoinedGame");

	public static readonly ModEvent<SPlayerSpawningData> PlayerSpawning = new ModEvent<SPlayerSpawningData>("PlayerSpawning");

	public static readonly ModEvent<SPlayerSpawnedInWorldData> PlayerSpawnedInWorld = new ModEvent<SPlayerSpawnedInWorldData>("PlayerSpawnedInWorld");

	public static readonly ModEvent<SPlayerDisconnectedData> PlayerDisconnected = new ModEvent<SPlayerDisconnectedData>("PlayerDisconnected");

	public static readonly ModEvent<SSavePlayerDataData> SavePlayerData = new ModEvent<SSavePlayerDataData>("SavePlayerData");

	public static readonly ModEventInterruptible<SGameMessageData> GameMessage = new ModEventInterruptible<SGameMessageData>("GameMessage");

	public static readonly ModEventInterruptible<SChatMessageData> ChatMessage = new ModEventInterruptible<SChatMessageData>("ChatMessage");

	public static readonly ModEvent<SCalcChunkColorsDoneData> CalcChunkColorsDone = new ModEvent<SCalcChunkColorsDoneData>("CalcChunkColorsDone");

	public static readonly ModEvent<SEntityKilledData> EntityKilled = new ModEvent<SEntityKilledData>("EntityKilled");
}
