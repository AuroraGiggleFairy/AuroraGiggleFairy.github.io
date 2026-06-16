using System;
using System.Collections.Generic;
using System.Linq;

public class LockManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxLocksPerPlayer = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float KeepOpenTimeoutSeconds = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float KeepOpenPeriodSeconds = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LockManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> playersWithLocks = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public OneToManyDictionary<int, LockEntry> singleLocks = new OneToManyDictionary<int, LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ManyToManyDictionary<int, LockEntry> sharedLocks = new ManyToManyDictionary<int, LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LockEntry> pendingClientLocks = new HashSet<LockEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, DateTime> keepOpenTimes = new Dictionary<int, DateTime>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastUpdate;

	public static LockManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new LockManager();
			}
			return _instance;
		}
	}

	public void Init()
	{
		if (pendingClientLocks.Count > 0 || singleLocks.CountKeys > 0 || sharedLocks.CountValues > 0)
		{
			Log.Warning("[LockManager] had locked entities from a previous session!");
			pendingClientLocks.Clear();
			singleLocks.Clear();
			sharedLocks.Clear();
		}
		playersWithLocks.Clear();
		keepOpenTimes.Clear();
		lastUpdate = DateTime.UtcNow;
	}

	public void Update()
	{
		if (GameManager.Instance.World == null || (DateTime.UtcNow - lastUpdate).TotalSeconds <= 2.5)
		{
			return;
		}
		lastUpdate = DateTime.UtcNow;
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			List<int> list = null;
			foreach (KeyValuePair<int, DateTime> keepOpenTime in keepOpenTimes)
			{
				if (keepOpenTime.Key != primaryPlayerId && (DateTime.UtcNow - keepOpenTime.Value).TotalSeconds > 10.0)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(keepOpenTime.Key);
				}
			}
			foreach (int playersWithLock in playersWithLocks)
			{
				if (!keepOpenTimes.ContainsKey(playersWithLock))
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(playersWithLock);
				}
			}
			if (list == null)
			{
				return;
			}
			{
				foreach (int item in list)
				{
					Log.Error(string.Format("[{0}] No keep-open packet received from player {1}.", "LockManager", item));
					ForceUnlockByPlayer(item);
				}
				return;
			}
		}
		if (playersWithLocks.Contains(primaryPlayerId))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageInventoryKeepOpen>().Setup());
		}
	}

	public bool IsLockedByLocalPlayer(ILockTarget _target, ushort _channel = 0)
	{
		if (_target == null)
		{
			return false;
		}
		LockEntry lockEntry = new LockEntry(_target, _channel);
		if (pendingClientLocks.Contains(lockEntry))
		{
			return true;
		}
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (primaryPlayerId == -1)
		{
			return false;
		}
		if (_target.IsSharedLock(_channel) && sharedLocks.TryGetByValue(lockEntry, out var keys))
		{
			foreach (int item in keys)
			{
				if (item == primaryPlayerId)
				{
					return true;
				}
			}
			return false;
		}
		if (!_target.IsSharedLock(_channel) && singleLocks.TryGetByValue(lockEntry, out var key))
		{
			return key == primaryPlayerId;
		}
		return false;
	}

	public bool IsLockedServer(ILockTarget _target, ushort _channel = 0)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients may only check local lock states by using IsLockedByLocalPlayer.");
			return false;
		}
		LockEntry value = new LockEntry(_target, _channel);
		if (_target.IsSharedLock(_channel) && sharedLocks.ContainsValue(value))
		{
			return true;
		}
		if (!_target.IsSharedLock(_channel) && singleLocks.ContainsValue(value))
		{
			return true;
		}
		return false;
	}

	public void LockRequestLocal(ILockTarget _target, ILockContext _context = null, ushort _channel = 0)
	{
		if (_target == null)
		{
			Log.Error("[LockManager] target supplied for locking was null.");
			return;
		}
		ReadOnlySpan<ILockTarget> targets = new ILockTarget[1] { _target };
		LockRequestLocal(targets, _context, _channel);
	}

	public void LockRequestLocal(ReadOnlySpan<ILockTarget> _targets, ILockContext _context = null, ushort _channel = 0)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			if (_targets == null || _targets.Length == 0)
			{
				Log.Error("[LockManager] no targets supplied for locking.");
				return;
			}
			if (pendingClientLocks.Count > 0)
			{
				Log.Error("[LockManager] lock request already in progress.");
				return;
			}
			for (int i = 0; i < _targets.Length; i++)
			{
				pendingClientLocks.Add(new LockEntry(_targets[i], _channel));
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageLockRequest>().Setup(_targets, _context, _channel));
		}
		else
		{
			LockRequestServer(_targets, GameManager.Instance.World.GetPrimaryPlayerId(), _context, _channel);
		}
	}

	public void LockRequestServer(ReadOnlySpan<ILockTarget> _targets, int _playerId, ILockContext _context = null, ushort _channel = 0)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients must only request a lock by using LockRequestLocal.");
			return;
		}
		if (singleLocks.ContainsKey(_playerId) || sharedLocks.ContainsKey(_playerId) || keepOpenTimes.ContainsKey(_playerId))
		{
			Log.Error(string.Format("[{0}] Invalid lock state for player {1} when calling {2}. The player tried requesting a lock despite having an existing lock state.", "LockManager", _playerId, "LockRequestServer"));
			ForceUnlockByPlayer(_playerId);
			return;
		}
		bool flag = false;
		string errorMsg = null;
		try
		{
			if (_targets.Length > 5)
			{
				errorMsg = string.Format("[{0}] Failed to lock targets. Requested {1} exceeds limit {2}.", "LockManager", _targets.Length, 5);
				return;
			}
			for (int i = 0; i < _targets.Length; i++)
			{
				ILockTarget lockTarget = _targets[i];
				LockEntry value = new LockEntry(lockTarget, _channel);
				if (!lockTarget.IsSharedLock(_channel) && singleLocks.ContainsValue(value))
				{
					errorMsg = string.Format("[{0}] Failed to lock {1} on channel {2}. It is already locked by another player.", "LockManager", lockTarget.GetType(), _channel);
					return;
				}
			}
			for (int j = 0; j < _targets.Length; j++)
			{
				ILockTarget lockTarget2 = _targets[j];
				if (!lockTarget2.CanLockOnServer(_playerId, _context, _channel))
				{
					errorMsg = string.Format("[{0}] Failed to lock {1} on channel {2}. Target refused to be locked on server.", "LockManager", lockTarget2.GetType(), _channel);
					return;
				}
			}
			flag = true;
		}
		finally
		{
			if (flag)
			{
				for (int k = 0; k < _targets.Length; k++)
				{
					ILockTarget lockTarget3 = _targets[k];
					LockEntry value2 = new LockEntry(lockTarget3, _channel);
					if (lockTarget3.IsSharedLock(_channel))
					{
						sharedLocks.Add(_playerId, value2);
					}
					else
					{
						singleLocks.Add(_playerId, value2);
					}
					RefreshPlayerActive(_playerId);
					keepOpenTimes[_playerId] = DateTime.UtcNow;
				}
				for (int l = 0; l < _targets.Length; l++)
				{
					_targets[l].OnLockedServer(_success: true, _playerId, _context, _channel);
				}
			}
			if (GameManager.Instance.World.GetPrimaryPlayerId() != _playerId)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLockResponse>().Setup(flag, errorMsg, _targets, _context, _channel), _onlyClientsAttachedToAnEntity: false, _playerId);
			}
			else
			{
				LockResponse(flag, errorMsg, _targets, _context, _channel);
			}
		}
	}

	public void LockResponse(bool _success, string _errorMsg, ReadOnlySpan<ILockTarget> _targets, ILockContext _context, ushort _channel)
	{
		pendingClientLocks.Clear();
		if (!string.IsNullOrEmpty(_errorMsg))
		{
			Log.Warning(_errorMsg);
		}
		if (GameManager.Instance.World == null)
		{
			Log.Error("[LockManager] World not fully initialized.");
			return;
		}
		for (int i = 0; i < _targets.Length; i++)
		{
			if (_targets[i] == null)
			{
				Log.Error("[LockManager] target supplied for locking was null.");
				return;
			}
		}
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (_success)
		{
			for (int j = 0; j < _targets.Length; j++)
			{
				ILockTarget lockTarget = _targets[j];
				if (!lockTarget.CanLockLocally(_context, _channel))
				{
					Log.Warning(string.Format("[{0}] Failed to lock {1} on channel {2}. Target refused to be locked locally.", "LockManager", lockTarget.GetType(), _channel));
					return;
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				for (int k = 0; k < _targets.Length; k++)
				{
					ILockTarget lockTarget2 = _targets[k];
					LockEntry value = new LockEntry(lockTarget2, _channel);
					if (lockTarget2.IsSharedLock(_channel))
					{
						sharedLocks.Add(primaryPlayerId, value);
					}
					else
					{
						singleLocks.Add(primaryPlayerId, value);
					}
					RefreshPlayerActive(primaryPlayerId);
				}
				keepOpenTimes[primaryPlayerId] = DateTime.UtcNow;
			}
		}
		for (int l = 0; l < _targets.Length; l++)
		{
			_targets[l].OnLockedLocal(_success, _context, _channel);
		}
	}

	public void UnlockRequestLocal()
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageLockRequest>().Setup());
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows();
		}
		else
		{
			UnlockRequestServer(primaryPlayerId, _isForceUnlocked: false);
		}
	}

	public void UnlockRequestServer(int _playerId, bool _isForceUnlocked)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Clients must unlock by using UnlockRequestLocal.");
			return;
		}
		bool success = false;
		string errorMsg = null;
		try
		{
			if (playersWithLocks.Contains(_playerId))
			{
				bool flag = false;
				if (singleLocks.TryGetByKey(_playerId, out var values) && values != null)
				{
					flag = true;
					LockEntry[] array = values.ToArray();
					for (int i = 0; i < array.Length; i++)
					{
						LockEntry value = array[i];
						singleLocks.RemoveByValue(value);
						value.Target.OnUnlockedServer(_playerId, value.Channel);
					}
					singleLocks.RemoveByKey(_playerId);
				}
				if (sharedLocks.TryGetByKey(_playerId, out var values2) && values2 != null)
				{
					flag = true;
					LockEntry[] array = values2.ToArray();
					for (int i = 0; i < array.Length; i++)
					{
						LockEntry value2 = array[i];
						sharedLocks.Remove(_playerId, value2);
						value2.Target.OnUnlockedServer(_playerId, value2.Channel);
					}
					sharedLocks.RemoveByKey(_playerId);
				}
				keepOpenTimes.Remove(_playerId);
				RefreshPlayerActive(_playerId);
				if (!flag)
				{
					errorMsg = string.Format("[{0}] Found no targets while trying to unlock for player {1}.", "LockManager", _playerId);
				}
				else
				{
					success = true;
				}
			}
			else
			{
				errorMsg = "[LockManager] Player entity has nothing to unlock.";
			}
		}
		finally
		{
			if (GameManager.Instance.World.GetPrimaryPlayerId() != _playerId)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLockResponse>().Setup(success, errorMsg, _isForceUnlocked), _onlyClientsAttachedToAnEntity: false, _playerId);
			}
			else
			{
				LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows();
				UnlockResponse(success, errorMsg, _isForceUnlocked);
			}
		}
	}

	public void UnlockResponse(bool _success, string _errorMsg, bool _isForceUnlocked = false)
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		singleLocks.RemoveByKey(primaryPlayerId);
		sharedLocks.RemoveByKey(primaryPlayerId);
		keepOpenTimes.Remove(primaryPlayerId);
		RefreshPlayerActive(primaryPlayerId);
		if (!_success && !string.IsNullOrEmpty(_errorMsg))
		{
			Log.Warning(_errorMsg);
		}
		if (_isForceUnlocked)
		{
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.CloseAllOpenModalWindows();
			pendingClientLocks.Clear();
		}
	}

	public void ForceUnlockAll()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockAll");
			return;
		}
		int[] array = playersWithLocks.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			ForceUnlockByPlayer(array[i]);
		}
	}

	public void ForceUnlockByPlayer(int _playerId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockByPlayer");
		}
		else
		{
			UnlockRequestServer(_playerId, _isForceUnlocked: true);
		}
	}

	public void ForceUnlockLockTarget(ILockTarget _target)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockLockTarget");
		}
		else
		{
			if (_target == null)
			{
				return;
			}
			List<int> list = new List<int>();
			foreach (LockEntry value in singleLocks.Values)
			{
				if (value.Target == _target && singleLocks.TryGetByValue(value, out var key) && !list.Contains(key))
				{
					list.Add(key);
				}
			}
			foreach (LockEntry value2 in sharedLocks.Values)
			{
				if (value2.Target != _target || !sharedLocks.TryGetByValue(value2, out var keys))
				{
					continue;
				}
				foreach (int item in keys)
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (int item2 in list)
				{
					UnlockRequestServer(item2, _isForceUnlocked: true);
				}
				return;
			}
			Log.Warning("[LockManager] Target is not locked.");
		}
	}

	public void ForceUnlockByChunk(HashSetLong chunks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("[LockManager] Only servers can use ForceUnlockByChunk");
			return;
		}
		List<ILockTarget> list = new List<ILockTarget>();
		foreach (long chunk5 in chunks)
		{
			foreach (LockEntry value in singleLocks.Values)
			{
				if (value.Target is TileEntity tileEntity)
				{
					Chunk chunk = tileEntity.GetChunk();
					if ((chunk == null || chunk.Key == chunk5) && !list.Contains(value.Target))
					{
						list.Add(value.Target);
					}
				}
				else if (value.Target is TEFeatureAbs tEFeatureAbs)
				{
					Chunk chunk2 = tEFeatureAbs.Parent?.GetChunk();
					if ((chunk2 == null || chunk2.Key == chunk5) && !list.Contains(value.Target))
					{
						list.Add(value.Target);
					}
				}
			}
			foreach (LockEntry value2 in sharedLocks.Values)
			{
				if (value2.Target is TileEntity tileEntity2)
				{
					Chunk chunk3 = tileEntity2.GetChunk();
					if ((chunk3 == null || chunk3.Key == chunk5) && !list.Contains(value2.Target))
					{
						list.Add(value2.Target);
					}
				}
				else if (value2.Target is TEFeatureAbs tEFeatureAbs2)
				{
					Chunk chunk4 = tEFeatureAbs2.Parent?.GetChunk();
					if ((chunk4 == null || chunk4.Key == chunk5) && !list.Contains(value2.Target))
					{
						list.Add(value2.Target);
					}
				}
			}
		}
		foreach (ILockTarget item in list)
		{
			ForceUnlockLockTarget(item);
		}
	}

	public void ProcessKeepOpen(int _playerId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!singleLocks.ContainsKey(_playerId) && !sharedLocks.ContainsKey(_playerId))
			{
				Log.Warning("[LockManager] Keep Open request received but player owns no locks.");
				keepOpenTimes.Remove(_playerId);
				ForceUnlockByPlayer(_playerId);
			}
			else
			{
				keepOpenTimes[_playerId] = DateTime.UtcNow;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshPlayerActive(int playerId)
	{
		if (singleLocks.ContainsKey(playerId) || sharedLocks.ContainsKey(playerId))
		{
			playersWithLocks.Add(playerId);
		}
		else
		{
			playersWithLocks.Remove(playerId);
		}
	}
}
