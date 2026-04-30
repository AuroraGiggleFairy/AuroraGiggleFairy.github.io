using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Audio;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace LockableWorkstations
{
	internal sealed class LockState
	{
		public bool IsLocked;
		public PlatformUserIdentifierAbs OwnerId;
		public List<PlatformUserIdentifierAbs> AllowedUserIds = new List<PlatformUserIdentifierAbs>();
		public string PasswordHash = string.Empty;
	}

	internal sealed class TileEntityLockAdapter : ILockable
	{
		private readonly TileEntity _tileEntity;
		private readonly LockState _state;

		public TileEntityLockAdapter(TileEntity tileEntity, LockState state)
		{
			_tileEntity = tileEntity;
			_state = state;
		}

		public int EntityId
		{
			get => _tileEntity.entityId;
			set => _tileEntity.entityId = value;
		}

		public bool IsLocked()
		{
			return _state.IsLocked;
		}

		public void SetLocked(bool _isLocked)
		{
			_state.IsLocked = _isLocked;
			_tileEntity.SetModified();
			LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
		}

		public PlatformUserIdentifierAbs GetOwner()
		{
			return _state.OwnerId;
		}

		public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
		{
			_state.OwnerId = _userIdentifier;
			_tileEntity.SetModified();
			LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
		}

		public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
		{
			if (_userIdentifier == null)
			{
				return false;
			}

			if ((_state.OwnerId != null && _state.OwnerId.Equals(_userIdentifier)) || _state.AllowedUserIds.Contains(_userIdentifier))
			{
				return true;
			}

			return false;
		}

		public List<PlatformUserIdentifierAbs> GetUsers()
		{
			return _state.AllowedUserIds;
		}

		public bool LocalPlayerIsOwner()
		{
			return IsOwner(PlatformManager.InternalLocalUserIdentifier);
		}

		public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
		{
			return _userIdentifier != null && _state.OwnerId != null && _state.OwnerId.Equals(_userIdentifier);
		}

		public bool HasPassword()
		{
			return !string.IsNullOrEmpty(_state.PasswordHash);
		}

		public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
		{
			changed = false;
			string hashed = Utils.HashString(_password ?? string.Empty);

			if (!LockableWorkstationHelpers.IsServer())
			{
				if (IsOwner(_userIdentifier))
				{
					if (!string.Equals(_state.PasswordHash, hashed, StringComparison.Ordinal))
					{
						changed = true;
						_state.PasswordHash = hashed;
						_state.AllowedUserIds.Clear();
					}

					LockableWorkstationHelpers.SendKeypadServerRequest(_tileEntity, "lw-setcode", hashed);
					_tileEntity.SetModified();
					return true;
				}

				LockableWorkstationHelpers.SendKeypadServerRequest(_tileEntity, "lw-allow", hashed);

				if (string.Equals(_state.PasswordHash, hashed, StringComparison.Ordinal))
				{
					if (_userIdentifier != null && !_state.AllowedUserIds.Contains(_userIdentifier))
					{
						_state.AllowedUserIds.Add(_userIdentifier);
					}
					_tileEntity.SetModified();
					return true;
				}

				// Server is authoritative for password checks in multiplayer.
				// Avoid false local rejection from stale client cache.
				return true;
			}

			if (IsOwner(_userIdentifier))
			{
				if (!string.Equals(_state.PasswordHash, hashed, StringComparison.Ordinal))
				{
					changed = true;
					_state.PasswordHash = hashed;
					_state.AllowedUserIds.Clear();
					_tileEntity.SetModified();
					LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
				}

				return true;
			}

			if (string.Equals(_state.PasswordHash, hashed, StringComparison.Ordinal))
			{
				if (_userIdentifier != null && !_state.AllowedUserIds.Contains(_userIdentifier))
				{
					_state.AllowedUserIds.Add(_userIdentifier);
					_tileEntity.SetModified();
					LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
				}

				return true;
			}

			return false;
		}

		public string GetPassword()
		{
			return _state.PasswordHash;
		}

		public void ApplyServerPasswordHash(string passwordHash)
		{
			string normalized = passwordHash ?? string.Empty;
			if (string.Equals(_state.PasswordHash, normalized, StringComparison.Ordinal) && _state.AllowedUserIds.Count == 0)
			{
				return;
			}

			_state.PasswordHash = normalized;
			_state.AllowedUserIds.Clear();
			_tileEntity.SetModified();
			LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
		}

		public void AddAllowedUserServer(PlatformUserIdentifierAbs userIdentifier)
		{
			if (userIdentifier == null || _state.AllowedUserIds.Contains(userIdentifier))
			{
				return;
			}

			_state.AllowedUserIds.Add(userIdentifier);
			_tileEntity.SetModified();
			LockableWorkstationHelpers.NotifyStateChanged(_tileEntity, _state);
		}
	}

	internal static class LockableWorkstationHelpers
	{
		private const int SaveFormatVersion = 1;
		private const string SaveFileName = "lockableWorkstations.dat";
		private static readonly TimeSpan AutosaveInterval = TimeSpan.FromSeconds(5.0);
		private static readonly Dictionary<string, LockState> RuntimeStateByPosition = new Dictionary<string, LockState>();
		private static bool _serverPersistenceLoaded;
		private static bool _serverPersistenceDirty;
		private static DateTime _nextAutosaveUtc = DateTime.MinValue;

		public static bool TryGetAdapter(WorldBase world, int clrIdx, Vector3i blockPos, out TileEntity tileEntity, out TileEntityLockAdapter adapter)
		{
			adapter = null;
			tileEntity = null;
			EnsureServerPersistenceLoaded();

			if (world == null)
			{
				return false;
			}

			tileEntity = world.GetTileEntity(clrIdx, blockPos) ?? world.GetTileEntity(blockPos);
			if (tileEntity == null)
			{
				return false;
			}

			if (!IsSupportedTileEntity(tileEntity))
			{
				return false;
			}

			string positionKey = ResolvePositionKey(tileEntity, blockPos);
			LockState state = GetOrCreateState(tileEntity, positionKey, blockPos);
			adapter = new TileEntityLockAdapter(tileEntity, state);
			return true;
		}

		public static bool TryGetAdapter(WorldBase world, TileEntity tileEntity, out TileEntityLockAdapter adapter)
		{
			adapter = null;
			if (world == null || tileEntity == null || !IsSupportedTileEntity(tileEntity))
			{
				return false;
			}

			return TryGetAdapter(world, tileEntity.GetClrIdx(), tileEntity.ToWorldPos(), out _, out adapter);
		}

		public static void NotifyStateChanged(TileEntity tileEntity, LockState state)
		{
			if (tileEntity == null || state == null || !IsServerContext())
			{
				return;
			}

			string positionKey = ResolvePositionKey(tileEntity, null);
			if (string.IsNullOrEmpty(positionKey))
			{
				return;
			}

			RuntimeStateByPosition[positionKey] = CloneState(state);
			MarkPersistenceDirty();
			TrySaveServerStateImmediate();
			BroadcastState(tileEntity, state);
		}

		public static void ApplySyncedState(int clrIdx, Vector3i blockPos, bool isLocked, string ownerCombined, string passwordHash, List<string> allowedUsersCombined)
		{
			_ = clrIdx;
			EnsureServerPersistenceLoaded();
			string key = BuildPositionKey(blockPos);
			LockState state = new LockState
			{
				IsLocked = isLocked,
				OwnerId = ParseUser(ownerCombined),
				PasswordHash = passwordHash ?? string.Empty,
				AllowedUserIds = new List<PlatformUserIdentifierAbs>()
			};

			if (allowedUsersCombined != null)
			{
				for (int i = 0; i < allowedUsersCombined.Count; i++)
				{
					PlatformUserIdentifierAbs parsed = ParseUser(allowedUsersCombined[i]);
					if (parsed != null)
					{
						state.AllowedUserIds.Add(parsed);
					}
				}
			}

			RuntimeStateByPosition[key] = state;
			if (IsServerContext())
			{
				MarkPersistenceDirty();
			}
		}

		public static bool IsAccessDenied(TileEntityLockAdapter adapter)
		{
			return IsAccessDenied(null, adapter, PlatformManager.InternalLocalUserIdentifier);
		}

		public static bool IsAccessDenied(TileEntityLockAdapter adapter, PlatformUserIdentifierAbs userId)
		{
			return IsAccessDenied(null, adapter, userId);
		}

		public static bool IsAccessDenied(WorldBase world, TileEntityLockAdapter adapter, PlatformUserIdentifierAbs userId)
		{
			if (adapter == null)
			{
				return false;
			}

			if (!adapter.IsLocked())
			{
				return false;
			}

			if (world != null && IsAdminUser(world, userId))
			{
				return false;
			}

			return !adapter.IsUserAllowed(userId);
		}

		public static bool IsOwnerOrAcl(WorldBase world, TileEntityLockAdapter adapter, PlatformUserIdentifierAbs requestUser)
		{
			if (adapter == null)
			{
				return false;
			}

			if (adapter.IsOwner(requestUser))
			{
				return true;
			}

			PlatformUserIdentifierAbs owner = adapter.GetOwner();
			if (owner == null || world == null)
			{
				return false;
			}

			PersistentPlayerData ownerData = world.GetGameManager()?.GetPersistentPlayerList()?.GetPlayerData(owner);
			return ownerData != null && ownerData.ACL != null && ownerData.ACL.Contains(requestUser);
		}

		public static bool IsAdminEntityId(int entityId)
		{
			if (entityId < 0)
			{
				return true;
			}

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager?.Clients == null)
			{
				return false;
			}

			ClientInfo clientInfo = manager.Clients.ForEntityId(entityId);
			return IsAdminClientInfo(clientInfo);
		}

		public static bool IsAdminUser(WorldBase world, PlatformUserIdentifierAbs userId)
		{
			_ = world;
			if (userId == null)
			{
				return false;
			}

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager?.Clients == null)
			{
				return false;
			}

			foreach (ClientInfo clientInfo in manager.Clients.List)
			{
				if (clientInfo?.InternalId != null && clientInfo.InternalId.Equals(userId) && IsAdminClientInfo(clientInfo))
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsAdminClientInfo(ClientInfo clientInfo)
		{
			if (clientInfo == null)
			{
				return false;
			}

			Type type = clientInfo.GetType();

			if (TryGetBoolean(type, clientInfo, "IsAdmin", out bool isAdmin) && isAdmin)
			{
				return true;
			}

			if (TryGetBoolean(type, clientInfo, "isAdmin", out bool isAdminField) && isAdminField)
			{
				return true;
			}

			if (TryGetInt(type, clientInfo, "PermissionLevel", out int permissionLevel) && permissionLevel > 0)
			{
				return true;
			}

			if (TryGetInt(type, clientInfo, "permissionLevel", out int permissionLevelField) && permissionLevelField > 0)
			{
				return true;
			}

			return false;
		}

		public static bool IsServer()
		{
			return SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		}

		public static void SendKeypadServerRequest(TileEntity tileEntity, string action, string payload)
		{
			if (tileEntity == null || IsServer())
			{
				return;
			}

			EntityPlayerLocal localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (localPlayer == null)
			{
				return;
			}

			string customUi = action + ":" + (payload ?? string.Empty);
			GameManager.Instance.World.GetGameManager()?.TELockServer(
				tileEntity.GetClrIdx(),
				tileEntity.ToWorldPos(),
				tileEntity.entityId,
				localPlayer.entityId,
				customUi);
		}

		public static PlatformUserIdentifierAbs ResolveUserIdentifier(EntityAlive entity)
		{
			_ = entity;
			return PlatformManager.InternalLocalUserIdentifier;
		}

		public static PlatformUserIdentifierAbs ResolveUserIdentifier(WorldBase world, int entityId)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager != null && manager.IsServer && manager.Clients != null)
			{
				ClientInfo clientInfo = manager.Clients.ForEntityId(entityId);
				if (clientInfo != null && clientInfo.InternalId != null)
				{
					return clientInfo.InternalId;
				}
			}

			_ = world;
			return PlatformManager.InternalLocalUserIdentifier;
		}

		public static string BuildStorageStyleActivationText(WorldBase world, BlockValue blockValue, int clrIdx, Vector3i blockPos, EntityAlive focusingEntity)
		{
			if (!(focusingEntity is EntityPlayerLocal localPlayer))
			{
				return string.Empty;
			}

			if (!TryGetAdapter(world, clrIdx, blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return string.Empty;
			}

			PlayerActionsLocal playerInput = localPlayer.playerInput;
			string hotkey = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string blockName = blockValue.Block.GetLocalizedBlockName();
			PlatformUserIdentifierAbs requestUser = ResolveUserIdentifier(focusingEntity);

			if (!adapter.IsLocked())
			{
				return string.Format(Localization.Get("tooltipUnlocked"), hotkey, blockName) + " " + Localization.Get("xuiAccessibleAGF");
			}

			if (IsAccessDenied(adapter, requestUser))
			{
				return string.Format(Localization.Get("tooltipJammed"), hotkey, blockName) + " " + Localization.Get("xuiDeniedAGF");
			}

			return string.Format(Localization.Get("tooltipLocked"), hotkey, blockName) + " " + Localization.Get("xuiSecuredAGF");
		}

		public static void AppendLockStateSuffix(ref string activationText, WorldBase world, int clrIdx, Vector3i blockPos)
		{
			if (!TryGetAdapter(world, clrIdx, blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			string suffix;
			if (!adapter.IsLocked())
			{
				suffix = " " + Localization.Get("xuiAccessibleAGF");
			}
			else if (IsAccessDenied(adapter))
			{
				suffix = " " + Localization.Get("xuiDeniedAGF");
			}
			else
			{
				suffix = " " + Localization.Get("xuiSecuredAGF");
			}

			string text = activationText ?? string.Empty;
			if (text.Contains(Localization.Get("xuiSecuredAGF")) || text.Contains(Localization.Get("xuiAccessibleAGF")) || text.Contains(Localization.Get("xuiDeniedAGF")))
			{
				activationText = text;
				return;
			}

			activationText = text.TrimEnd() + suffix;
		}

		public static BlockActivationCommand[] BuildCommands(WorldBase world, TileEntityLockAdapter adapter, BlockActivationCommand[] existingCommands)
		{
			if (adapter == null)
			{
				return existingCommands;
			}

			PlatformUserIdentifierAbs localUser = PlatformManager.InternalLocalUserIdentifier;
			bool owner = adapter.LocalPlayerIsOwner();
			bool keypadEnabled = adapter.IsLocked() || owner;

			BlockActivationCommand lockCommand;
			if (adapter.IsLocked())
			{
				lockCommand = new BlockActivationCommand("unlock", "unlock", _enabled: true);
			}
			else
			{
				lockCommand = new BlockActivationCommand("lock", "lock", _enabled: true);
			}

			BlockActivationCommand keypadCommand = new BlockActivationCommand("keypad", "keypad", keypadEnabled);

			int existingCount = existingCommands?.Length ?? 0;
			BlockActivationCommand[] commands = new BlockActivationCommand[existingCount + 2];
			if (existingCount > 0)
			{
				Array.Copy(existingCommands, commands, existingCount);
			}

			commands[existingCount] = lockCommand;
			commands[existingCount + 1] = keypadCommand;
			return commands;
		}

		public static bool HandleLockCommand(string commandName, WorldBase world, int clrIdx, Vector3i blockPos, EntityPlayerLocal player)
		{
			if (!TryGetAdapter(world, clrIdx, blockPos, out TileEntity tileEntity, out TileEntityLockAdapter adapter))
			{
				return false;
			}

			if (!string.Equals(commandName, "lock", StringComparison.Ordinal) &&
				!string.Equals(commandName, "unlock", StringComparison.Ordinal) &&
				!string.Equals(commandName, "keypad", StringComparison.Ordinal))
			{
				return false;
			}

			if (string.Equals(commandName, "keypad", StringComparison.Ordinal))
			{
				LocalPlayerUI playerUi = LocalPlayerUI.GetUIForPlayer(player);
				if (playerUi != null)
				{
					XUiC_KeypadWindow.Open(playerUi, adapter);
				}

				return true;
			}

			if (!IsServerContext())
			{
				string customAction = string.Equals(commandName, "lock", StringComparison.Ordinal) ? "lw-lock" : "lw-unlock";
				Vector3i targetPos = tileEntity?.ToWorldPos() ?? blockPos;
				int targetClr = tileEntity?.GetClrIdx() ?? clrIdx;
				int targetEntityId = tileEntity?.entityId ?? -1;
				world.GetGameManager()?.TELockServer(targetClr, targetPos, targetEntityId, player.entityId, customAction);
				return true;
			}

			PlatformUserIdentifierAbs requestUser = IsServerContext()
				? ResolveUserIdentifier(world, player != null ? player.entityId : -1)
				: ResolveUserIdentifier(player);
			bool isAdmin = IsServerContext() && player != null && IsAdminEntityId(player.entityId);
			bool canChangeLock = adapter.IsOwner(requestUser) || adapter.GetOwner() == null || isAdmin;
			if (!canChangeLock)
			{
				Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				return true;
			}

			if (adapter.GetOwner() == null)
			{
				adapter.SetOwner(requestUser);
			}

			if (string.Equals(commandName, "lock", StringComparison.Ordinal))
			{
				adapter.SetLocked(_isLocked: true);
				Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
				GameManager.ShowTooltip(player, "containerLocked");
			}
			else
			{
				adapter.SetLocked(_isLocked: false);
				Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
				GameManager.ShowTooltip(player, "containerUnlocked");
			}

			tileEntity.SetModified();
			return true;
		}

		public static Vector3i ResolveParentIfChild(WorldBase world, int clrIdx, Vector3i blockPos)
		{
			if (world == null)
			{
				return blockPos;
			}

			BlockValue block = world.GetBlock(clrIdx, blockPos);
			if (!block.ischild)
			{
				return blockPos;
			}

			return block.Block.multiBlockPos.GetParentPos(blockPos, block);
		}

		public static void EnsureOwnerOnPlace(WorldBase world, int clrIdx, Vector3i blockPos, EntityAlive placingEntity)
		{
			if (!(placingEntity is EntityPlayerLocal) || !TryGetAdapter(world, clrIdx, blockPos, out TileEntity tileEntity, out TileEntityLockAdapter adapter))
			{
				return;
			}

			if (adapter.GetOwner() == null)
			{
				adapter.SetOwner(ResolveUserIdentifier(placingEntity));
				if (!adapter.IsLocked())
				{
					adapter.SetLocked(_isLocked: true);
				}
			}
			tileEntity.SetModified();
		}

		public static void EnsureStateForAddedBlock(WorldBase world, int clrIdx, Vector3i blockPos, PlatformUserIdentifierAbs addedByPlayer)
		{
			if (!TryGetAdapter(world, clrIdx, blockPos, out TileEntity tileEntity, out TileEntityLockAdapter adapter))
			{
				return;
			}

			if (addedByPlayer != null && adapter.GetOwner() == null)
			{
				adapter.SetOwner(addedByPlayer);
				if (!adapter.IsLocked())
				{
					adapter.SetLocked(_isLocked: true);
				}
				tileEntity.SetModified();
				MarkPersistenceDirty();
				TrySaveServerStateImmediate();
			}
		}

		public static void RemoveState(TileEntity tileEntity)
		{
			if (tileEntity == null)
			{
				return;
			}

			string positionKey = ResolvePositionKey(tileEntity, null);
			if (string.IsNullOrEmpty(positionKey))
			{
				return;
			}

			RuntimeStateByPosition.Remove(positionKey);
			MarkPersistenceDirty();
			TrySaveServerStateImmediate();
		}

		public static void CopyState(TileEntity fromTileEntity, TileEntity toTileEntity)
		{
			if (fromTileEntity == null || toTileEntity == null)
			{
				return;
			}

			string fromPositionKey = ResolvePositionKey(fromTileEntity, null);
			if (string.IsNullOrEmpty(fromPositionKey) || !RuntimeStateByPosition.TryGetValue(fromPositionKey, out LockState sourceState))
			{
				return;
			}

			LockState clonedState = CloneState(sourceState);
			string toPositionKey = ResolvePositionKey(toTileEntity, null);
			if (string.IsNullOrEmpty(toPositionKey))
			{
				return;
			}

			RuntimeStateByPosition[toPositionKey] = CloneState(clonedState);
			MarkPersistenceDirty();
			TrySaveServerStateImmediate();
		}

		private static LockState GetOrCreateState(TileEntity tileEntity, string positionKey, Vector3i? legacyPos = null)
		{
			if (string.IsNullOrEmpty(positionKey))
			{
				positionKey = BuildPositionKey(tileEntity.ToWorldPos());
			}

			if (legacyPos.HasValue)
			{
				string legacyKey = BuildPositionKey(legacyPos.Value);
				if (!string.IsNullOrEmpty(legacyKey) && !string.Equals(legacyKey, positionKey, StringComparison.Ordinal) &&
					!RuntimeStateByPosition.ContainsKey(positionKey) && RuntimeStateByPosition.TryGetValue(legacyKey, out LockState legacyState) && legacyState != null)
				{
					RuntimeStateByPosition.Remove(legacyKey);
					RuntimeStateByPosition[positionKey] = legacyState;
					MarkPersistenceDirty();
					TrySaveServerStateImmediate();
				}
			}

			if (!RuntimeStateByPosition.TryGetValue(positionKey, out LockState state))
			{
				state = new LockState
				{
					IsLocked = false
				};

				RuntimeStateByPosition[positionKey] = state;
			}

			return state;
		}

		private static bool IsServerContext()
		{
			return IsServer();
		}

		private static void BroadcastState(TileEntity tileEntity, LockState state)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (tileEntity == null || state == null || manager == null || manager.Clients == null)
			{
				return;
			}

			List<string> allowed = new List<string>();
			for (int i = 0; i < state.AllowedUserIds.Count; i++)
			{
				PlatformUserIdentifierAbs user = state.AllowedUserIds[i];
				if (user != null)
				{
					allowed.Add(user.CombinedString);
				}
			}

			for (int i = 0; i < manager.Clients.List.Count; i++)
			{
				ClientInfo clientInfo = manager.Clients.List[i];
				if (clientInfo == null || clientInfo.entityId < 0 || !LockableWorkstationHybridRouting.HasClientCapability(clientInfo))
				{
					continue;
				}

				manager.SendPackage(
					NetPackageManager.GetPackage<NetPackageLockableWorkstationState>().Setup(
						tileEntity.GetClrIdx(),
						tileEntity.ToWorldPos(),
						state.IsLocked,
						state.OwnerId?.CombinedString ?? string.Empty,
						state.PasswordHash ?? string.Empty,
						allowed),
					_onlyClientsAttachedToAnEntity: false,
					clientInfo.entityId);
			}
		}

		private static bool IsSupportedTileEntity(TileEntity tileEntity)
		{
			return tileEntity is TileEntityWorkstation
				|| tileEntity is TileEntityCollector
				|| tileEntity is TileEntityPowerSource
				|| tileEntity is TileEntityPoweredRangedTrap;
		}

		private static string BuildPositionKey(Vector3i blockPos)
		{
			return blockPos.x + "," + blockPos.y + "," + blockPos.z;
		}

		private static bool TryParsePositionKey(string key, out Vector3i blockPos)
		{
			blockPos = Vector3i.zero;
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}

			string[] parts = key.Split(',');
			if (parts.Length != 3)
			{
				return false;
			}

			if (!int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y) || !int.TryParse(parts[2], out int z))
			{
				return false;
			}

			blockPos = new Vector3i(x, y, z);
			return true;
		}

		private static string ResolvePositionKey(TileEntity tileEntity, Vector3i? fallbackPos)
		{
			if (tileEntity != null)
			{
				return BuildPositionKey(tileEntity.ToWorldPos());
			}

			if (fallbackPos.HasValue)
			{
				return BuildPositionKey(fallbackPos.Value);
			}

			return null;
		}

		public static void InitializeServerPersistence()
		{
			if (!IsServerContext())
			{
				return;
			}

			_serverPersistenceLoaded = false;
			_serverPersistenceDirty = false;
			_nextAutosaveUtc = DateTime.UtcNow + AutosaveInterval;
			EnsureServerPersistenceLoaded();
		}

		public static void FlushServerPersistence()
		{
			if (!IsServerContext())
			{
				return;
			}

			EnsureServerPersistenceLoaded();
			SaveServerState();
		}

		public static void TickServerPersistence()
		{
			if (!IsServerContext())
			{
				return;
			}

			EnsureServerPersistenceLoaded();
			if (!_serverPersistenceDirty)
			{
				return;
			}

			DateTime now = DateTime.UtcNow;
			if (now < _nextAutosaveUtc)
			{
				return;
			}

			SaveServerState();
			_nextAutosaveUtc = now + AutosaveInterval;
		}

		public static void SyncAllStatesToPlayer(WorldBase world, int entityId)
		{
			if (!IsServerContext() || world == null || entityId < 0 || !LockableWorkstationHybridRouting.HasClientCapabilityEntityId(entityId))
			{
				return;
			}

			EnsureServerPersistenceLoaded();

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null)
			{
				return;
			}

			foreach (KeyValuePair<string, LockState> entry in RuntimeStateByPosition)
			{
				if (entry.Value == null || !TryParsePositionKey(entry.Key, out Vector3i pos))
				{
					continue;
				}

				TileEntity tileEntity = world.GetTileEntity(pos);
				int clrIdx = tileEntity != null ? tileEntity.GetClrIdx() : -1;

				List<string> allowed = new List<string>();
				for (int i = 0; i < entry.Value.AllowedUserIds.Count; i++)
				{
					PlatformUserIdentifierAbs user = entry.Value.AllowedUserIds[i];
					if (user != null)
					{
						allowed.Add(user.CombinedString);
					}
				}

				manager.SendPackage(
					NetPackageManager.GetPackage<NetPackageLockableWorkstationState>().Setup(
						clrIdx,
						pos,
						entry.Value.IsLocked,
						entry.Value.OwnerId?.CombinedString ?? string.Empty,
						entry.Value.PasswordHash ?? string.Empty,
						allowed),
					_onlyClientsAttachedToAnEntity: false,
					entityId);
			}
		}

		private static void LoadServerState()
		{
			RuntimeStateByPosition.Clear();

			string path = GetPersistencePath();
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return;
			}

			try
			{
				using FileStream stream = File.OpenRead(path);
				using BinaryReader reader = new BinaryReader(stream);

				int version = reader.ReadInt32();
				if (version != SaveFormatVersion)
				{
					return;
				}

				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					string positionKey = reader.ReadString();
					LockState state = new LockState
					{
						IsLocked = reader.ReadBoolean(),
						OwnerId = ParseUser(reader.ReadString()),
						PasswordHash = reader.ReadString(),
						AllowedUserIds = new List<PlatformUserIdentifierAbs>()
					};

					int allowedCount = reader.ReadInt32();
					for (int j = 0; j < allowedCount; j++)
					{
						PlatformUserIdentifierAbs parsed = ParseUser(reader.ReadString());
						if (parsed != null)
						{
							state.AllowedUserIds.Add(parsed);
						}
					}

					if (!string.IsNullOrEmpty(positionKey))
					{
						RuntimeStateByPosition[positionKey] = state;
					}
				}
			}
			catch (Exception ex)
			{
				RuntimeStateByPosition.Clear();
				Console.WriteLine("LockableWorkstations: Failed loading persistence file: " + ex.Message);
			}
		}

		private static void TrySaveServerStateImmediate()
		{
			if (!IsServerContext())
			{
				return;
			}

			EnsureServerPersistenceLoaded();
			SaveServerState();
		}

		private static void SaveServerState()
		{
			if (!IsServerContext())
			{
				return;
			}

			string path = GetPersistencePath();
			if (string.IsNullOrEmpty(path))
			{
				_serverPersistenceDirty = true;
				return;
			}

			try
			{
				string directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				string tempPath = path + ".tmp";
				List<KeyValuePair<string, LockState>> entries = new List<KeyValuePair<string, LockState>>();
				foreach (KeyValuePair<string, LockState> entry in RuntimeStateByPosition)
				{
					if (!string.IsNullOrEmpty(entry.Key) && entry.Value != null)
					{
						entries.Add(entry);
					}
				}

				using (FileStream stream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(SaveFormatVersion);
					writer.Write(entries.Count);

					for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
					{
						KeyValuePair<string, LockState> entry = entries[entryIndex];
						LockState state = entry.Value;

						writer.Write(entry.Key ?? string.Empty);
						writer.Write(state.IsLocked);
						writer.Write(state.OwnerId?.CombinedString ?? string.Empty);
						writer.Write(state.PasswordHash ?? string.Empty);

						int allowedCount = 0;
						for (int i = 0; i < state.AllowedUserIds.Count; i++)
						{
							if (state.AllowedUserIds[i] != null)
							{
								allowedCount++;
							}
						}

						writer.Write(allowedCount);
						for (int i = 0; i < state.AllowedUserIds.Count; i++)
						{
							PlatformUserIdentifierAbs user = state.AllowedUserIds[i];
							if (user != null)
							{
								writer.Write(user.CombinedString);
							}
						}
					}
				}

				if (File.Exists(path))
				{
					File.Delete(path);
				}

				File.Move(tempPath, path);
				_serverPersistenceDirty = false;
			}
			catch (Exception ex)
			{
				_serverPersistenceDirty = true;
				Console.WriteLine("LockableWorkstations: Failed writing persistence file: " + ex.Message);
			}
		}

		private static void EnsureServerPersistenceLoaded()
		{
			if (!IsServerContext() || _serverPersistenceLoaded)
			{
				return;
			}

			LoadServerState();
			_serverPersistenceLoaded = true;
		}

		private static void MarkPersistenceDirty()
		{
			if (!IsServerContext())
			{
				return;
			}

			_serverPersistenceDirty = true;
		}

		private static string GetPersistencePath()
		{
			try
			{
				string saveDir = GameIO.GetSaveGameDir();
				if (string.IsNullOrEmpty(saveDir))
				{
					return null;
				}

				return Path.Combine(saveDir, SaveFileName);
			}
			catch
			{
				return null;
			}
		}

		private static PlatformUserIdentifierAbs ParseUser(string combined)
		{
			if (string.IsNullOrEmpty(combined))
			{
				return null;
			}

			return PlatformUserIdentifierAbs.FromCombinedString(combined, _logErrors: false);
		}

		private static bool TryGetBoolean(Type type, object instance, string memberName, out bool value)
		{
			value = false;
			PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.PropertyType == typeof(bool))
			{
				value = (bool)property.GetValue(instance, null);
				return true;
			}

			FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && field.FieldType == typeof(bool))
			{
				value = (bool)field.GetValue(instance);
				return true;
			}

			return false;
		}

		private static bool TryGetInt(Type type, object instance, string memberName, out int value)
		{
			value = 0;
			PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.PropertyType == typeof(int))
			{
				value = (int)property.GetValue(instance, null);
				return true;
			}

			FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && field.FieldType == typeof(int))
			{
				value = (int)field.GetValue(instance);
				return true;
			}

			return false;
		}

		private static LockState CloneState(LockState source)
		{
			return new LockState
			{
				IsLocked = source.IsLocked,
				OwnerId = source.OwnerId,
				PasswordHash = source.PasswordHash,
				AllowedUserIds = new List<PlatformUserIdentifierAbs>(source.AllowedUserIds)
			};
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.GetActivationText))]
	public static class Patch_BlockWorkstation_GetActivationText
	{
		[HarmonyPriority(Priority.High)]
		public static void Postfix(ref string __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.GetActivationText))]
	public static class Patch_BlockCollector_GetActivationText
	{
		[HarmonyPriority(Priority.High)]
		public static void Postfix(ref string __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockForge), nameof(BlockForge.GetActivationText))]
	[HarmonyPriority(Priority.High)]
	[HarmonyBefore("com.agfprojects.expandedinteractionprompts")]
	public static class Patch_BlockForge_GetActivationText
	{
		public static void Postfix(ref string __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockCampfire), nameof(BlockCampfire.GetActivationText))]
	[HarmonyPriority(Priority.High)]
	[HarmonyBefore("com.agfprojects.expandedinteractionprompts")]
	public static class Patch_BlockCampfire_GetActivationText
	{
		public static void Postfix(ref string __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockPowerSource), nameof(BlockPowerSource.GetActivationText))]
	[HarmonyPriority(Priority.High)]
	public static class Patch_BlockPowerSource_GetActivationText
	{
		public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockLauncher), nameof(BlockLauncher.GetActivationText))]
	[HarmonyPriority(Priority.High)]
	public static class Patch_BlockLauncher_GetActivationText
	{
		public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockRanged), nameof(BlockRanged.GetActivationText))]
	[HarmonyPriority(Priority.High)]
	public static class Patch_BlockRanged_GetActivationText
	{
		public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			LockableWorkstationHelpers.AppendLockStateSuffix(ref __result, _world, _clrIdx, _blockPos);
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.GetBlockActivationCommands))]
	public static class Patch_BlockWorkstation_GetBlockActivationCommands
	{
		public static void Postfix(ref BlockActivationCommand[] __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			if (!LockableWorkstationHelpers.TryGetAdapter(_world, _clrIdx, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			__result = LockableWorkstationHelpers.BuildCommands(_world, adapter, __result);
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.GetBlockActivationCommands))]
	public static class Patch_BlockCollector_GetBlockActivationCommands
	{
		public static void Postfix(ref BlockActivationCommand[] __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
		{
			if (!LockableWorkstationHelpers.TryGetAdapter(_world, _clrIdx, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			__result = LockableWorkstationHelpers.BuildCommands(_world, adapter, __result);
		}
	}

	[HarmonyPatch(typeof(BlockPowerSource), nameof(BlockPowerSource.GetBlockActivationCommands))]
	public static class Patch_BlockPowerSource_GetBlockActivationCommands
	{
		public static void Postfix(ref BlockActivationCommand[] __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			if (!LockableWorkstationHelpers.TryGetAdapter(_world, _clrIdx, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			__result = LockableWorkstationHelpers.BuildCommands(_world, adapter, __result);
		}
	}

	[HarmonyPatch(typeof(BlockLauncher), nameof(BlockLauncher.GetBlockActivationCommands))]
	public static class Patch_BlockLauncher_GetBlockActivationCommands
	{
		public static void Postfix(ref BlockActivationCommand[] __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			if (!LockableWorkstationHelpers.TryGetAdapter(_world, _clrIdx, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			__result = LockableWorkstationHelpers.BuildCommands(_world, adapter, __result);
		}
	}

	[HarmonyPatch(typeof(BlockRanged), nameof(BlockRanged.GetBlockActivationCommands))]
	public static class Patch_BlockRanged_GetBlockActivationCommands
	{
		public static void Postfix(ref BlockActivationCommand[] __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			_ = _blockValue;
			_ = _entityFocusing;
			if (!LockableWorkstationHelpers.TryGetAdapter(_world, _clrIdx, _blockPos, out _, out TileEntityLockAdapter adapter))
			{
				return;
			}

			__result = LockableWorkstationHelpers.BuildCommands(_world, adapter, __result);
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.OnBlockActivated), typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockWorkstation_OnBlockActivated_Command
	{
		public static bool Prefix(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, EntityPlayerLocal _player, ref bool __result)
		{
			if (LockableWorkstationHelpers.HandleLockCommand(_commandName, _world, _cIdx, _blockPos, _player))
			{
				__result = true;
				return false;
			}

			if (LockableWorkstationHelpers.IsServer() &&
				string.Equals(_commandName, "open", StringComparison.Ordinal) &&
				LockableWorkstationHelpers.TryGetAdapter(_world, _cIdx, _blockPos, out _, out TileEntityLockAdapter adapter) &&
				LockableWorkstationHelpers.IsAccessDenied(_world, adapter, LockableWorkstationHelpers.ResolveUserIdentifier(_world, _player != null ? _player.entityId : -1)))
			{
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.OnBlockActivated), typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockCollector_OnBlockActivated_Command
	{
		public static bool Prefix(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, EntityPlayerLocal _player, ref bool __result)
		{
			if (LockableWorkstationHelpers.HandleLockCommand(_commandName, _world, _cIdx, _blockPos, _player))
			{
				__result = true;
				return false;
			}

			if (LockableWorkstationHelpers.IsServer() &&
				string.Equals(_commandName, "Search", StringComparison.Ordinal) &&
				LockableWorkstationHelpers.TryGetAdapter(_world, _cIdx, _blockPos, out _, out TileEntityLockAdapter adapter) &&
				LockableWorkstationHelpers.IsAccessDenied(_world, adapter, LockableWorkstationHelpers.ResolveUserIdentifier(_world, _player != null ? _player.entityId : -1)))
			{
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockPowerSource), nameof(BlockPowerSource.OnBlockActivated), typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockPowerSource_OnBlockActivated_Command
	{
		public static bool Prefix(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, EntityPlayerLocal _player, ref bool __result)
		{
			Vector3i targetPos = LockableWorkstationHelpers.ResolveParentIfChild(_world, _cIdx, _blockPos);

			if (LockableWorkstationHelpers.HandleLockCommand(_commandName, _world, _cIdx, targetPos, _player))
			{
				__result = true;
				return false;
			}

			if (LockableWorkstationHelpers.IsServer() &&
				string.Equals(_commandName, "open", StringComparison.Ordinal) &&
				LockableWorkstationHelpers.TryGetAdapter(_world, _cIdx, targetPos, out _, out TileEntityLockAdapter adapter) &&
				LockableWorkstationHelpers.IsAccessDenied(_world, adapter, LockableWorkstationHelpers.ResolveUserIdentifier(_world, _player != null ? _player.entityId : -1)))
			{
				Manager.BroadcastPlayByLocalPlayer(targetPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockLauncher), nameof(BlockLauncher.OnBlockActivated), typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockLauncher_OnBlockActivated_Command
	{
		public static bool Prefix(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, EntityPlayerLocal _player, ref bool __result)
		{
			Vector3i targetPos = LockableWorkstationHelpers.ResolveParentIfChild(_world, _cIdx, _blockPos);

			if (LockableWorkstationHelpers.HandleLockCommand(_commandName, _world, _cIdx, targetPos, _player))
			{
				__result = true;
				return false;
			}

			if (LockableWorkstationHelpers.IsServer() &&
				string.Equals(_commandName, "options", StringComparison.Ordinal) &&
				LockableWorkstationHelpers.TryGetAdapter(_world, _cIdx, targetPos, out _, out TileEntityLockAdapter adapter) &&
				LockableWorkstationHelpers.IsAccessDenied(_world, adapter, LockableWorkstationHelpers.ResolveUserIdentifier(_world, _player != null ? _player.entityId : -1)))
			{
				Manager.BroadcastPlayByLocalPlayer(targetPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockRanged), nameof(BlockRanged.OnBlockActivated), typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockRanged_OnBlockActivated_Command
	{
		public static bool Prefix(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, EntityPlayerLocal _player, ref bool __result)
		{
			Vector3i targetPos = LockableWorkstationHelpers.ResolveParentIfChild(_world, _cIdx, _blockPos);

			if (LockableWorkstationHelpers.HandleLockCommand(_commandName, _world, _cIdx, targetPos, _player))
			{
				__result = true;
				return false;
			}

			if (LockableWorkstationHelpers.IsServer() &&
				string.Equals(_commandName, "options", StringComparison.Ordinal) &&
				LockableWorkstationHelpers.TryGetAdapter(_world, _cIdx, targetPos, out _, out TileEntityLockAdapter adapter) &&
				LockableWorkstationHelpers.IsAccessDenied(_world, adapter, LockableWorkstationHelpers.ResolveUserIdentifier(_world, _player != null ? _player.entityId : -1)))
			{
				Manager.BroadcastPlayByLocalPlayer(targetPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.OnBlockActivated), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockWorkstation_OnBlockActivated_Main
	{
		public static bool Prefix(WorldBase _world, int _cIdx, Vector3i _blockPos, ref bool __result)
		{
			_ = _world;
			_ = _cIdx;
			_ = _blockPos;
			_ = __result;
			return true;
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.OnBlockActivated), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal))]
	public static class Patch_BlockCollector_OnBlockActivated_Main
	{
		public static bool Prefix(WorldBase _world, int _cIdx, Vector3i _blockPos, ref bool __result)
		{
			_ = _world;
			_ = _cIdx;
			_ = _blockPos;
			_ = __result;
			return true;
		}
	}

	[HarmonyPatch(typeof(GameManager), nameof(GameManager.TELockServer), typeof(int), typeof(Vector3i), typeof(int), typeof(int), typeof(string))]
	public static class Patch_GameManager_TELockServer
	{
		public static bool Prefix(GameManager __instance, int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt, string _customUi)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return true;
			}

			if (string.IsNullOrEmpty(_customUi))
			{
				return true;
			}

			bool isLockToggle = string.Equals(_customUi, "lw-lock", StringComparison.Ordinal) || string.Equals(_customUi, "lw-unlock", StringComparison.Ordinal);
			bool isSetCode = _customUi.StartsWith("lw-setcode:", StringComparison.Ordinal);
			bool isAllowCode = _customUi.StartsWith("lw-allow:", StringComparison.Ordinal);
			TileEntity tileEntity = (_lootEntityId != -1)
				? __instance.World.GetTileEntity(_lootEntityId)
				: __instance.World.GetTileEntity(_clrIdx, _blockPos);

			if (!isLockToggle && !isSetCode && !isAllowCode)
			{
				if (tileEntity == null || !LockableWorkstationHelpers.TryGetAdapter(__instance.World, tileEntity, out TileEntityLockAdapter openAdapter))
				{
					return true;
				}

				PlatformUserIdentifierAbs openRequestUser = LockableWorkstationHelpers.ResolveUserIdentifier(__instance.World, _entityIdThatOpenedIt);
				bool ownerOrAcl = LockableWorkstationHelpers.IsOwnerOrAcl(__instance.World, openAdapter, openRequestUser);
				bool isAdmin = LockableWorkstationHelpers.IsAdminEntityId(_entityIdThatOpenedIt);
				if (openAdapter.IsLocked() && !openAdapter.IsUserAllowed(openRequestUser) && !ownerOrAcl && !isAdmin)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTELock>()
						.Setup(NetPackageTELock.TELockType.DeniedAccess, _clrIdx, _blockPos, _lootEntityId, _entityIdThatOpenedIt, _customUi), _onlyClientsAttachedToAnEntity: false, _entityIdThatOpenedIt);
					return false;
				}

				return true;
			}

			if (tileEntity == null || !LockableWorkstationHelpers.TryGetAdapter(__instance.World, tileEntity, out TileEntityLockAdapter adapter))
			{
				return false;
			}

			PlatformUserIdentifierAbs requestUser = LockableWorkstationHelpers.ResolveUserIdentifier(__instance.World, _entityIdThatOpenedIt);
			bool isAdminUser = LockableWorkstationHelpers.IsAdminEntityId(_entityIdThatOpenedIt);
			bool canChangeLock = adapter.IsOwner(requestUser) || adapter.GetOwner() == null || isAdminUser;

			if (isSetCode)
			{
				if (!canChangeLock)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTELock>()
						.Setup(NetPackageTELock.TELockType.DeniedAccess, _clrIdx, _blockPos, _lootEntityId, _entityIdThatOpenedIt, _customUi), _onlyClientsAttachedToAnEntity: false, _entityIdThatOpenedIt);
					return false;
				}

				string hashed = _customUi.Substring("lw-setcode:".Length);
				if (adapter.GetOwner() == null && requestUser != null)
				{
					adapter.SetOwner(requestUser);
				}
				adapter.ApplyServerPasswordHash(hashed);

				return false;
			}

			if (isAllowCode)
			{
				string hashed = _customUi.Substring("lw-allow:".Length);
				if (string.Equals(adapter.GetPassword(), hashed, StringComparison.Ordinal) && requestUser != null)
				{
					adapter.AddAllowedUserServer(requestUser);
				}

				return false;
			}

			if (!canChangeLock)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTELock>()
					.Setup(NetPackageTELock.TELockType.DeniedAccess, _clrIdx, _blockPos, _lootEntityId, _entityIdThatOpenedIt, _customUi), _onlyClientsAttachedToAnEntity: false, _entityIdThatOpenedIt);
				return false;
			}

			if (adapter.GetOwner() == null && requestUser != null)
			{
				adapter.SetOwner(requestUser);
			}

			bool lockNow = string.Equals(_customUi, "lw-lock", StringComparison.Ordinal);
			adapter.SetLocked(lockNow);
			Vector3i soundPos = tileEntity.ToWorldPos();
			Manager.BroadcastPlayByLocalPlayer(soundPos.ToVector3() + Vector3.one * 0.5f, lockNow ? "Misc/locking" : "Misc/unlocking");
			return false;
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.PlaceBlock))]
	public static class Patch_BlockWorkstation_PlaceBlock
	{
		public static void Postfix(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
		{
			LockableWorkstationHelpers.EnsureOwnerOnPlace(_world, _result.clrIdx, _result.blockPos, _ea);
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.PlaceBlock))]
	public static class Patch_BlockCollector_PlaceBlock
	{
		public static void Postfix(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
		{
			LockableWorkstationHelpers.EnsureOwnerOnPlace(_world, _result.clrIdx, _result.blockPos, _ea);
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.OnBlockAdded))]
	public static class Patch_BlockWorkstation_OnBlockAdded
	{
		public static void Postfix(WorldBase world, Chunk _chunk, Vector3i _blockPos, PlatformUserIdentifierAbs _addedByPlayer)
		{
			LockableWorkstationHelpers.EnsureStateForAddedBlock(world, _chunk.ClrIdx, _blockPos, _addedByPlayer);
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.OnBlockAdded))]
	public static class Patch_BlockCollector_OnBlockAdded
	{
		public static void Postfix(WorldBase world, Chunk _chunk, Vector3i _blockPos, PlatformUserIdentifierAbs _addedByPlayer)
		{
			LockableWorkstationHelpers.EnsureStateForAddedBlock(world, _chunk.ClrIdx, _blockPos, _addedByPlayer);
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.OnBlockRemoved))]
	public static class Patch_BlockWorkstation_OnBlockRemoved
	{
		public static void Prefix(WorldBase world, Chunk _chunk, Vector3i _blockPos)
		{
			TileEntity tileEntity = world.GetTileEntity(_chunk.ClrIdx, _blockPos);
			LockableWorkstationHelpers.RemoveState(tileEntity);
		}
	}

	[HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.OnBlockRemoved))]
	public static class Patch_BlockCollector_OnBlockRemoved
	{
		public static void Prefix(WorldBase world, Chunk _chunk, Vector3i _blockPos)
		{
			TileEntity tileEntity = world.GetTileEntity(_chunk.ClrIdx, _blockPos);
			LockableWorkstationHelpers.RemoveState(tileEntity);
		}
	}

}
