using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform;

public class BlockedPlayerList : IRemotePlayerStorageObject
{
	public class ListEntry
	{
		public readonly PlayerData PlayerData;

		public readonly DateTime LastSeen;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool ResolvedOnce
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool Blocked
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public ListEntry(PlayerData _playerData, DateTime _lastSeen, bool _blockState)
		{
			PlayerData = _playerData;
			LastSeen = _lastSeen;
			Blocked = _blockState;
		}

		public static ListEntry Read(BinaryReader _reader)
		{
			PlayerData playerData = PlayerData.Read(_reader);
			DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(_reader.ReadInt64()).UtcDateTime;
			bool blockState = _reader.ReadBoolean();
			return new ListEntry(playerData, utcDateTime, blockState);
		}

		public void Write(BinaryWriter _writer)
		{
			PlayerData.Write(_writer);
			long value = new DateTimeOffset(LastSeen).ToUnixTimeSeconds();
			_writer.Write(value);
			_writer.Write(Blocked);
		}

		public void SetResolvedOnce()
		{
			ResolvedOnce = true;
		}

		public (bool, string) SetBlockState(bool _blockState)
		{
			if (Blocked == _blockState)
			{
				return (false, null);
			}
			if (PlatformManager.NativePlatform.User.CanShowProfile(PlayerData.NativeId))
			{
				Blocked = false;
				Log.Warning($"[BlockedPlayerList] Cannot change block state of native user {PlayerData.NativeId} through the block list");
				return (false, null);
			}
			if (_blockState && Instance.EntryCount(_blocked: true, _resolveRequired: false) >= 500)
			{
				return (false, Localization.Get("xuiBlockedPlayersCantAddMessage"));
			}
			PlayerData.PlatformData.MarkBlockedStateChanged();
			Instance.MarkForWrite();
			Blocked = _blockState;
			return (true, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockedPlayerList instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan WriteThreshold = TimeSpan.FromMinutes(10.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan WriteRequestDelay = TimeSpan.FromSeconds(5.0);

	public const int MaxBlockedPlayerEntries = 500;

	public const int MaxRecentPlayerEntries = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilePath = "BlockedPlayerList";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TimeoutHours = 168;

	[PublicizedFrom(EAccessModifier.Private)]
	public object bplLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<PlatformUserIdentifierAbs, ListEntry> playerStates = new DictionaryList<PlatformUserIdentifierAbs, ListEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastWriteTime = DateTime.Now;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime? writeRequestTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ERoutineState readStorageState;

	[PublicizedFrom(EAccessModifier.Private)]
	public IRemotePlayerFileStorage.CallbackResult readStorageResult = IRemotePlayerFileStorage.CallbackResult.Other;

	[PublicizedFrom(EAccessModifier.Private)]
	public ERoutineState writeToStorageState;

	[PublicizedFrom(EAccessModifier.Private)]
	public ERoutineState resolveState;

	public static BlockedPlayerList Instance
	{
		get
		{
			if (instance == null && PlatformManager.MultiPlatform?.RemotePlayerFileStorage != null)
			{
				instance = new BlockedPlayerList();
				PlayerInteractions.Instance.OnNewPlayerInteraction += instance.OnPlayerInteraction;
			}
			return instance;
		}
	}

	public void Update()
	{
		if (writeRequestTime.HasValue)
		{
			DateTime now = DateTime.Now;
			DateTime? dateTime = writeRequestTime;
			if (now - dateTime >= WriteRequestDelay)
			{
				goto IL_007f;
			}
		}
		if (!(DateTime.Now - lastWriteTime >= WriteThreshold))
		{
			return;
		}
		goto IL_007f;
		IL_007f:
		WriteToStorage();
		lastWriteTime = DateTime.Now;
		writeRequestTime = null;
	}

	public void UpdatePlayersSeenInWorld(World _world)
	{
		if (_world?.Players == null)
		{
			return;
		}
		foreach (EntityPlayer item in _world.Players.list)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(item.entityId);
			AddOrUpdatePlayer(playerDataFromEntityID.PlayerData, DateTime.UtcNow);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ListEntry AddOrUpdatePlayer(PlayerData _playerData, DateTime _timeStamp, bool? _blocked = null, bool _ignoreLimit = false)
	{
		if (_playerData == null || _playerData.PrimaryId.Equals(PlatformManager.MultiPlatform.User.PlatformUserId))
		{
			return null;
		}
		DateTime dateTime = DateTime.UtcNow.AddHours(-168.0);
		if (_blocked == false && dateTime >= _timeStamp)
		{
			return null;
		}
		lock (bplLock)
		{
			ListEntry valueOrDefault = playerStates.dict.GetValueOrDefault(_playerData.PrimaryId);
			if (!_ignoreLimit && _blocked == true && (valueOrDefault == null || !valueOrDefault.Blocked) && EntryCount(_blocked: true, _resolveRequired: false) >= 500)
			{
				return null;
			}
			ListEntry listEntry = null;
			listEntry = ((_blocked.HasValue || valueOrDefault == null) ? new ListEntry(_playerData, _timeStamp, _blocked == true) : new ListEntry(_playerData, _timeStamp, valueOrDefault.Blocked));
			playerStates.Set(_playerData.PrimaryId, listEntry);
			return listEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerInteraction(PlayerInteraction _interaction)
	{
		ListEntry listEntry = AddOrUpdatePlayer(_interaction.PlayerData, DateTime.UtcNow);
		if (listEntry != null)
		{
			MarkForWrite();
			listEntry.SetResolvedOnce();
		}
	}

	public int EntryCount(bool _blocked, bool _resolveRequired)
	{
		return playerStates.list.Count([PublicizedFrom(EAccessModifier.Internal)] (ListEntry entry) => entry.Blocked == _blocked && (!_resolveRequired || entry.ResolvedOnce));
	}

	public IEnumerable<ListEntry> GetEntriesOrdered(bool _blocked, bool _resolveRequired)
	{
		lock (bplLock)
		{
			SortPlayerStates();
			for (int i = 0; i < playerStates.list.Count; i++)
			{
				ListEntry listEntry = playerStates.list[i];
				if (listEntry.Blocked == _blocked && (!_resolveRequired || listEntry.ResolvedOnce))
				{
					yield return listEntry;
				}
			}
		}
	}

	public ListEntry GetPlayerStateInfo(PlatformUserIdentifierAbs _primaryId)
	{
		lock (bplLock)
		{
			if (playerStates.dict.TryGetValue(_primaryId, out var value))
			{
				return value;
			}
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortPlayerStates()
	{
		playerStates.list.Sort([PublicizedFrom(EAccessModifier.Internal)] (ListEntry p1, ListEntry p2) =>
		{
			DateTime lastSeen = p2.LastSeen;
			return lastSeen.CompareTo(p1.LastSeen);
		});
	}

	public IEnumerator ReadStorageAndResolve()
	{
		readStorageState = ERoutineState.Running;
		lock (bplLock)
		{
			BlockedPlayerList blockedPlayerList = IRemotePlayerFileStorage.ReadCachedObject<BlockedPlayerList>(PlatformManager.MultiPlatform.User, "BlockedPlayerList");
			if (blockedPlayerList != null)
			{
				playerStates = blockedPlayerList.playerStates;
			}
		}
		bool callbackComplete = false;
		IRemotePlayerFileStorage remotePlayerFileStorage = PlatformManager.MultiPlatform.RemotePlayerFileStorage;
		if (remotePlayerFileStorage != null)
		{
			remotePlayerFileStorage.ReadRemoteObject<BlockedPlayerList>("BlockedPlayerList", _overwriteCache: true, ReadRPFSCallback);
			while (!callbackComplete)
			{
				yield return null;
			}
		}
		if (playerStates.Count > 0)
		{
			yield return ResolveUserDetails();
		}
		readStorageState = ERoutineState.Succeeded;
		[PublicizedFrom(EAccessModifier.Internal)]
		void ReadRPFSCallback(IRemotePlayerFileStorage.CallbackResult _result, BlockedPlayerList _storedBpl)
		{
			readStorageResult = _result;
			callbackComplete = true;
			lock (bplLock)
			{
				if (_result != IRemotePlayerFileStorage.CallbackResult.Success || _storedBpl == null)
				{
					return;
				}
				foreach (ListEntry item in _storedBpl.playerStates.list)
				{
					if (!playerStates.dict.ContainsKey(item.PlayerData.PrimaryId))
					{
						AddOrUpdatePlayer(item.PlayerData, item.LastSeen, item.Blocked);
					}
					else if (DateTime.Compare(item.LastSeen, playerStates.dict[item.PlayerData.PrimaryId].LastSeen) > 0)
					{
						AddOrUpdatePlayer(item.PlayerData, item.LastSeen, item.Blocked);
					}
				}
			}
			callbackComplete = true;
		}
	}

	public void ReadInto(BinaryReader _reader)
	{
		lock (bplLock)
		{
			playerStates.Clear();
			_reader.ReadInt32();
			int num = _reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ListEntry listEntry = ListEntry.Read(_reader);
				AddOrUpdatePlayer(listEntry.PlayerData, listEntry.LastSeen, listEntry.Blocked);
			}
		}
	}

	public void MarkForWrite()
	{
		if (!writeRequestTime.HasValue)
		{
			writeRequestTime = DateTime.Now;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteToStorage()
	{
		if (writeToStorageState == ERoutineState.Running)
		{
			Log.Warning("[BlockedPlayerList] Tried to write to storage while another write is already in progress.");
			return;
		}
		if (readStorageResult != IRemotePlayerFileStorage.CallbackResult.Success && readStorageResult != IRemotePlayerFileStorage.CallbackResult.MalformedData && readStorageResult != IRemotePlayerFileStorage.CallbackResult.FileNotFound)
		{
			Log.Out("[BlockedPlayerList] Error when processing remote list. Saving to local cache only.");
			if (!IRemotePlayerFileStorage.WriteCachedObject(PlatformManager.MultiPlatform.User, "BlockedPlayerList", this))
			{
				Log.Warning("[BlockedPlayerList] Failed to write to local cache.");
			}
			return;
		}
		if (readStorageResult == IRemotePlayerFileStorage.CallbackResult.MalformedData)
		{
			Log.Out("[BlockedPlayerList] Previous remote list was malformed so it will be overwritten.");
		}
		writeToStorageState = ERoutineState.Running;
		PlatformManager.MultiPlatform.RemotePlayerFileStorage.WriteRemoteObject("BlockedPlayerList", this, _overwriteCache: true, WriteRPFSCallback);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteRPFSCallback(IRemotePlayerFileStorage.CallbackResult _result)
	{
		writeToStorageState = ERoutineState.NotStarted;
		if (_result != IRemotePlayerFileStorage.CallbackResult.Success)
		{
			Log.Warning("[BlockedPlayerList] Recent Player List failed to write to remote storage.");
		}
	}

	public void WriteFrom(BinaryWriter _writer)
	{
		lock (bplLock)
		{
			_writer.Write(1);
			SortPlayerStates();
			int num = EntryCount(_blocked: true, _resolveRequired: false);
			int num2 = Math.Min(EntryCount(_blocked: false, _resolveRequired: false), 100);
			_writer.Write(num + num2);
			for (int i = 0; i < num + num2; i++)
			{
				playerStates.list[i].Write(_writer);
			}
		}
	}

	public bool PendingResolve()
	{
		if (readStorageState == ERoutineState.Succeeded)
		{
			return resolveState == ERoutineState.Running;
		}
		return true;
	}

	public IEnumerator ResolveUserDetails()
	{
		while (resolveState == ERoutineState.Running)
		{
			yield return null;
		}
		try
		{
			resolveState = ERoutineState.Running;
			List<IPlatformUserData> dataList = new List<IPlatformUserData>();
			lock (bplLock)
			{
				foreach (ListEntry item in playerStates.list)
				{
					item.PlayerData.PlatformData.RequestUserDetailsUpdate();
					dataList.Add(item.PlayerData.PlatformData);
				}
			}
			yield return PlatformUserManager.ResolveUsersDetailsCoroutine(dataList);
			foreach (IPlatformUserData item2 in dataList)
			{
				AuthoredText playerName = playerStates.dict[item2.PrimaryId].PlayerData.PlayerName;
				if (item2.Name != null && item2.Name != playerName.Text)
				{
					playerName.Update(item2.Name, playerName.Author);
					GeneratedTextManager.PrefilterText(playerName);
				}
				playerStates.dict[item2.PrimaryId].SetResolvedOnce();
			}
			resolveState = ERoutineState.Succeeded;
		}
		finally
		{
			BlockedPlayerList blockedPlayerList = this;
			if (blockedPlayerList.resolveState != ERoutineState.Succeeded)
			{
				blockedPlayerList.resolveState = ERoutineState.Failed;
			}
		}
	}
}
