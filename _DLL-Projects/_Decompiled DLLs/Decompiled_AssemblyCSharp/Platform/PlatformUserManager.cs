using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Platform;

public static class PlatformUserManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class PlatformUserData : IPlatformUserData, IPlatformUser
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumDictionary<EBlockType, PlatformUserBlockedData> m_userBlockedStates;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> m_userBlockedStatesReadOnly;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public PlatformUserIdentifierAbs PrimaryId { get; }

		public PlatformUserIdentifierAbs NativeId
		{
			get
			{
				using (s_nativeIdToPrimaryIdsLock.ReadLockScope())
				{
					PlatformUserIdentifierAbs key;
					return s_nativeIdToPrimaryIds.TryGetByValue(PrimaryId, out key) ? key : null;
				}
			}
			set
			{
				if (value == null)
				{
					return;
				}
				using (s_nativeIdToPrimaryIdsLock.UpgradableReadLockScope())
				{
					if (s_nativeIdToPrimaryIds.TryGetByValue(PrimaryId, out var key))
					{
						if (key.Equals(value))
						{
							return;
						}
						using (s_nativeIdToPrimaryIdsLock.WriteLockScope())
						{
							s_nativeIdToPrimaryIds.RemoveByValue(PrimaryId);
							s_nativeIdToPrimaryIds.Add(value, PrimaryId);
							LogError($"Primary ID '{PrimaryId}' was be remapped from Native ID '{key}' to Native ID '{value}'.");
						}
					}
					else
					{
						using (s_nativeIdToPrimaryIdsLock.WriteLockScope())
						{
							s_nativeIdToPrimaryIds.Add(value, PrimaryId);
						}
					}
				}
				bool flag;
				using (s_nativeUserIdsSeenLock.UpgradableReadLockScope())
				{
					if (s_nativeUserIdsSeen.Contains(value))
					{
						flag = false;
					}
					else
					{
						using (s_nativeUserIdsSeenLock.WriteLockScope())
						{
							flag = s_nativeUserIdsSeen.Add(value);
						}
					}
				}
				if (flag)
				{
					OnUserAdded(value, isPrimary: false);
					RequestUserDetailsUpdate();
				}
			}
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string Name { get; set; }

		public IReadOnlyDictionary<EBlockType, PlatformUserBlockedData> Blocked => m_userBlockedStates;

		IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> IPlatformUserData.Blocked
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return m_userBlockedStatesReadOnly;
			}
		}

		public PlatformUserData(PlatformUserIdentifierAbs primaryId)
		{
			PrimaryId = primaryId;
			m_userBlockedStates = new EnumDictionary<EBlockType, PlatformUserBlockedData>();
			m_userBlockedStatesReadOnly = new ReadOnlyDictionaryWrapper<EBlockType, PlatformUserBlockedData, IPlatformUserBlockedData>(m_userBlockedStates);
			foreach (EBlockType item in EnumUtils.Values<EBlockType>())
			{
				m_userBlockedStates[item] = new PlatformUserBlockedData(this, item);
			}
			RequestUserDetailsUpdate();
		}

		public override string ToString()
		{
			return string.Format("{0}[PrimaryId={1}, NativeId={2}, Name={3}, {4}]", "PlatformUserData", PrimaryId, NativeId, Name, string.Join(", ", Blocked.Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<EBlockType, PlatformUserBlockedData> kv) => $"Blocked[{kv.Key}]={kv.Value}")));
		}

		public void RequestUserDetailsUpdate()
		{
			if (!CanCheckUserDetails())
			{
				return;
			}
			using (s_userDetailsToUpdateLock.WriteLockScope())
			{
				s_userDetailsToUpdate.Add(this);
			}
		}

		public void MarkBlockedStateChanged()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			using (s_blockedUsersToUpdateLock.UpgradableReadLockScope())
			{
				if (s_blockedUsersToUpdate.Contains(this))
				{
					return;
				}
				using (s_blockedUsersToUpdateLock.WriteLockScope())
				{
					s_blockedUsersToUpdate.Add(this);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class PlatformUserBlockedData : IPlatformUserBlockedData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PlatformUserData m_userData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_blockedLocally;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public EBlockType Type { get; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public EUserBlockState State
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool Locally
		{
			get
			{
				return m_blockedLocally;
			}
			set
			{
				m_blockedLocally = value;
				RefreshBlockedState(State == EUserBlockState.ByPlatform);
			}
		}

		public PlatformUserBlockedData(PlatformUserData userData, EBlockType blockType)
		{
			m_userData = userData;
			Type = blockType;
			m_blockedLocally = false;
			State = EUserBlockState.NotBlocked;
		}

		public override string ToString()
		{
			return string.Format("{0}[Type={1}, State={2}, Locally={3}]", "PlatformUserBlockedData", Type, State, Locally);
		}

		public void RefreshBlockedState(bool isBlockedByPlatform)
		{
			EUserBlockState state = State;
			EUserBlockState eUserBlockState = (isBlockedByPlatform ? EUserBlockState.ByPlatform : (Locally ? EUserBlockState.InGame : EUserBlockState.NotBlocked));
			if (eUserBlockState == EUserBlockState.ByPlatform)
			{
				m_blockedLocally = false;
			}
			State = eUserBlockState;
			if (state != eUserBlockState)
			{
				OnBlockedStateChanged(m_userData, Type, eUserBlockState);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class PlatformUserBlockedResults : IPlatformUserBlockedResults
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PlatformUserData m_userData;

		public PlatformUserData User => m_userData;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public EnumDictionary<EBlockType, bool> IsBlocked { get; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool HasErrored
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		IPlatformUser IPlatformUserBlockedResults.User
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return m_userData;
			}
		}

		public PlatformUserBlockedResults(PlatformUserData userData)
		{
			m_userData = userData;
			IsBlocked = new EnumDictionary<EBlockType, bool>();
			foreach (EBlockType item in EnumUtils.Values<EBlockType>())
			{
				IsBlocked[item] = false;
			}
			HasErrored = false;
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}, HasErrored={2}, {3}.{4}={5}, {6}.{7}={8}]", "PlatformUserBlockedResults", string.Join(", ", IsBlocked.Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<EBlockType, bool> kv) => $"IsBlocked[{kv.Key}]={kv.Value}")), HasErrored, "User", "PrimaryId", User.PrimaryId, "User", "NativeId", User.NativeId);
		}

		public void Block(EBlockType blockType)
		{
			IsBlocked[blockType] = true;
		}

		public void Error()
		{
			HasErrored = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class PlatformUserDetailsResult
	{
		public readonly PlatformUserData UserData;

		public string Name;

		public PlatformUserDetailsResult(PlatformUserData userData)
		{
			UserData = userData;
		}
	}

	public const int PrimaryIdsPerNativeIdLimit = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<PlatformUserIdentifierAbs, PlatformUserData> s_primaryIdToPlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReaderWriterLockSlim s_primaryIdToPlatformLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BiMultiDictionary<PlatformUserIdentifierAbs, PlatformUserIdentifierAbs> s_nativeIdToPrimaryIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReaderWriterLockSlim s_nativeIdToPrimaryIdsLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<PlatformUserIdentifierAbs> s_nativeUserIdsSeen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReaderWriterLockSlim s_nativeUserIdsSeenLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PermissionFrameFrequency = 60;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EUserPerms s_lastPermissions;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PersistentFrameFrequency = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PersistentPlayerList s_persistentPlayerListLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<PlatformUserIdentifierAbs> s_persistentIdsTemp;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<PlatformUserIdentifierAbs> s_persistentIdsLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<PlatformUserData> s_blockedUsersToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReaderWriterLockSlim s_blockedUsersToUpdateLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<PlatformUserBlockedResults> s_blockedDataCurrentlyUpdating;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IReadOnlyList<IPlatformUserBlockedResults> s_blockedDataCurrentlyUpdatingReadOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<PlatformUserData> s_userDetailsToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<PlatformUserDetailsResult> s_userDetailsCurrentlyUpdating;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReaderWriterLockSlim s_userDetailsToUpdateLock;

	public static event PlatformUserBlockedStateChangedHandler BlockedStateChanged;

	public static event PlatformUserDetailsUpdatedHandler DetailsUpdated;

	[Conditional("PLATFORM_USER_MANAGER_DEBUG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogTrace(string message)
	{
		Log.Out("[PlatformUserManager] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string message)
	{
		Log.Out("[PlatformUserManager] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string message)
	{
		Log.Warning("[PlatformUserManager] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogError(string message)
	{
		Log.Error("[PlatformUserManager] " + message);
	}

	public static void Init()
	{
		s_primaryIdToPlatform = new Dictionary<PlatformUserIdentifierAbs, PlatformUserData>();
		s_primaryIdToPlatformLock = new ReaderWriterLockSlim();
		s_nativeIdToPrimaryIds = new BiMultiDictionary<PlatformUserIdentifierAbs, PlatformUserIdentifierAbs>();
		s_nativeIdToPrimaryIdsLock = new ReaderWriterLockSlim();
		s_nativeUserIdsSeen = new HashSet<PlatformUserIdentifierAbs>();
		s_nativeUserIdsSeenLock = new ReaderWriterLockSlim();
		s_lastPermissions = EUserPerms.All;
		s_persistentPlayerListLast = null;
		s_persistentIdsTemp = new HashSet<PlatformUserIdentifierAbs>();
		s_persistentIdsLast = new HashSet<PlatformUserIdentifierAbs>();
		s_blockedUsersToUpdate = new HashSet<PlatformUserData>();
		s_blockedUsersToUpdateLock = new ReaderWriterLockSlim();
		s_blockedDataCurrentlyUpdating = new List<PlatformUserBlockedResults>();
		s_blockedDataCurrentlyUpdatingReadOnly = new ReadOnlyListWrapper<PlatformUserBlockedResults, IPlatformUserBlockedResults>(s_blockedDataCurrentlyUpdating);
		PlatformManager.MultiPlatform.User.UserBlocksChanged += OnPlatformUserBlocksChanged;
		s_userDetailsToUpdate = new HashSet<PlatformUserData>();
		s_userDetailsCurrentlyUpdating = new List<PlatformUserDetailsResult>();
		s_userDetailsToUpdateLock = new ReaderWriterLockSlim();
		s_enabled = true;
	}

	public static void Destroy()
	{
		s_enabled = false;
		s_userDetailsToUpdateLock = null;
		s_userDetailsCurrentlyUpdating = null;
		s_userDetailsToUpdate = null;
		PlatformManager.MultiPlatform.User.UserBlocksChanged -= OnPlatformUserBlocksChanged;
		s_blockedDataCurrentlyUpdatingReadOnly = null;
		s_blockedDataCurrentlyUpdating = null;
		s_blockedUsersToUpdateLock?.Dispose();
		s_blockedUsersToUpdateLock = null;
		s_blockedUsersToUpdate = null;
		s_persistentIdsLast = null;
		s_persistentIdsTemp = null;
		s_persistentPlayerListLast = null;
		s_lastPermissions = (EUserPerms)0;
		s_nativeUserIdsSeenLock?.Dispose();
		s_nativeUserIdsSeenLock = null;
		s_nativeUserIdsSeen = null;
		s_nativeIdToPrimaryIdsLock?.Dispose();
		s_nativeIdToPrimaryIdsLock = null;
		s_nativeIdToPrimaryIds = null;
		s_primaryIdToPlatformLock?.Dispose();
		s_primaryIdToPlatformLock = null;
		s_primaryIdToPlatform = null;
	}

	public static void Update()
	{
		if (!s_enabled)
		{
			return;
		}
		try
		{
			UpdatePermissions();
			UpdatePersistentIds();
			UpdateUserDetails();
			UpdateBlockedStates();
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	public static IPlatformUserData GetOrCreate(PlatformUserIdentifierAbs primaryId)
	{
		if (primaryId == null)
		{
			return null;
		}
		PlatformUserData platformUserData;
		using (s_primaryIdToPlatformLock.UpgradableReadLockScope())
		{
			if (s_primaryIdToPlatform.TryGetValue(primaryId, out var value))
			{
				return value;
			}
			using (s_primaryIdToPlatformLock.WriteLockScope())
			{
				platformUserData = new PlatformUserData(primaryId);
				s_primaryIdToPlatform.Add(primaryId, platformUserData);
			}
		}
		OnUserAdded(primaryId, isPrimary: true);
		return platformUserData;
	}

	public static bool TryGetNativePlatform(PlatformUserIdentifierAbs primaryId, out EPlatformIdentifier platform)
	{
		if (primaryId == null)
		{
			platform = EPlatformIdentifier.None;
			return false;
		}
		using (s_primaryIdToPlatformLock.ReadLockScope())
		{
			if (!s_primaryIdToPlatform.TryGetValue(primaryId, out var value))
			{
				platform = EPlatformIdentifier.None;
				return false;
			}
			PlatformUserIdentifierAbs nativeId = value.NativeId;
			if (nativeId == null)
			{
				platform = EPlatformIdentifier.None;
				return false;
			}
			platform = nativeId.PlatformIdentifier;
			return true;
		}
	}

	public static int TryGetByNative(PlatformUserIdentifierAbs nativeId, Span<PlatformUserIdentifierAbs> primaryIds)
	{
		if (nativeId == null)
		{
			return 0;
		}
		int num;
		using (s_nativeIdToPrimaryIdsLock.ReadLockScope())
		{
			num = s_nativeIdToPrimaryIds.TryGetByKey(nativeId, primaryIds);
		}
		if (num >= 3)
		{
			LogWarning($"Expected number of values returned {num} to be less than the limit of PrimaryIds per NativeId ({3}).");
		}
		return num;
	}

	public static IEnumerator ResolveUserBlockedCoroutine(IPlatformUserData data)
	{
		while (true)
		{
			using (s_blockedUsersToUpdateLock.ReadLockScope())
			{
				if (!s_blockedUsersToUpdate.Contains((PlatformUserData)data))
				{
					break;
				}
			}
			yield return null;
		}
	}

	public static IEnumerator ResolveUserDetailsCoroutine(IPlatformUserData data)
	{
		while (true)
		{
			using (s_userDetailsToUpdateLock.ReadLockScope())
			{
				if (!s_userDetailsToUpdate.Contains((PlatformUserData)data))
				{
					break;
				}
			}
			yield return null;
		}
	}

	public static bool AreUsersPendingResolve(IReadOnlyList<IPlatformUserData> users)
	{
		if (users == null || users.Count == 0)
		{
			return false;
		}
		using (s_blockedUsersToUpdateLock.ReadLockScope())
		{
			for (int i = 0; i < users.Count; i++)
			{
				if (s_blockedUsersToUpdate.Contains((PlatformUserData)users[i]))
				{
					return true;
				}
			}
		}
		using (s_userDetailsToUpdateLock.ReadLockScope())
		{
			for (int j = 0; j < users.Count; j++)
			{
				if (s_userDetailsToUpdate.Contains((PlatformUserData)users[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static IEnumerator ResolveUserBlocksCoroutine(IReadOnlyList<IPlatformUserData> users)
	{
		if (users == null || users.Count == 0)
		{
			yield break;
		}
		foreach (IPlatformUserData user in users)
		{
			yield return ResolveUserBlockedCoroutine(user);
		}
	}

	public static IEnumerator ResolveUsersDetailsCoroutine(IReadOnlyList<IPlatformUserData> users)
	{
		if (users == null || users.Count == 0)
		{
			yield break;
		}
		foreach (IPlatformUserData user in users)
		{
			yield return ResolveUserDetailsCoroutine(user);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnUserAdded(PlatformUserIdentifierAbs userId, bool isPrimary)
	{
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("PlatformUserManager.OnUserAdded", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				OnUserAdded(userId, isPrimary);
			});
		}
		else
		{
			PlatformManager.MultiPlatform.UserAdded(userId, isPrimary);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnBlockedStateChanged(PlatformUserData userData, EBlockType type, EUserBlockState nextBlockState)
	{
		if (ThreadManager.IsMainThread())
		{
			PlatformUserManager.BlockedStateChanged?.Invoke(userData, type, nextBlockState);
			return;
		}
		ThreadManager.AddSingleTaskMainThread("PlatformUserManager.OnBlockedStateChanged", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
		{
			PlatformUserManager.BlockedStateChanged?.Invoke(userData, type, nextBlockState);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdatePermissions()
	{
		if (Time.frameCount % 60 == 0)
		{
			EUserPerms permissions = PermissionsManager.GetPermissions();
			if ((s_lastPermissions ^ permissions).HasCommunication())
			{
				MarkBlockedStateChangedAll();
			}
			s_lastPermissions = permissions;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdatePersistentIds()
	{
		if (Time.frameCount % 300 != 0)
		{
			return;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		if (persistentPlayers == null)
		{
			return;
		}
		if (s_persistentPlayerListLast != persistentPlayers)
		{
			s_persistentIdsLast.Clear();
			s_persistentPlayerListLast = persistentPlayers;
		}
		ICollection<PlatformUserIdentifierAbs> players = persistentPlayers.Players.Keys;
		s_persistentIdsLast.RemoveWhere([PublicizedFrom(EAccessModifier.Internal)] (PlatformUserIdentifierAbs last) => !players.Contains(last));
		s_persistentIdsTemp.Clear();
		foreach (PlatformUserIdentifierAbs item in players)
		{
			if (!s_persistentIdsLast.Contains(item))
			{
				s_persistentIdsLast.Add(item);
				s_persistentIdsTemp.Add(item);
			}
		}
		foreach (PlatformUserIdentifierAbs item2 in s_persistentIdsTemp)
		{
			IPlatformUserData orCreate = GetOrCreate(item2);
			foreach (IPlatformUserBlockedData value in orCreate.Blocked.Values)
			{
				value.Locally = false;
			}
			orCreate.MarkBlockedStateChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateUserDetails()
	{
		if (s_userDetailsCurrentlyUpdating.Count > 0)
		{
			return;
		}
		using (s_userDetailsToUpdateLock.UpgradableReadLockScope())
		{
			if (s_userDetailsToUpdate.Count <= 0)
			{
				return;
			}
			foreach (PlatformUserData item in s_userDetailsToUpdate)
			{
				s_userDetailsCurrentlyUpdating.Add(new PlatformUserDetailsResult(item));
			}
		}
		if (s_userDetailsCurrentlyUpdating.Count > 0)
		{
			ThreadManager.StartCoroutine(ResolveUserDetailsCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ResolveUserDetailsCoroutine()
	{
		try
		{
			if (PlatformManager.CrossplatformPlatform?.UserDetailsService != null)
			{
				List<UserDetailsRequest> list = null;
				List<int> list2 = null;
				for (int i = 0; i < s_userDetailsCurrentlyUpdating.Count; i++)
				{
					PlatformUserDetailsResult platformUserDetailsResult = s_userDetailsCurrentlyUpdating[i];
					if (platformUserDetailsResult.UserData.NativeId != null)
					{
						if (list == null)
						{
							list = new List<UserDetailsRequest>();
						}
						if (list2 == null)
						{
							list2 = new List<int>();
						}
						list.Add(new UserDetailsRequest(platformUserDetailsResult.UserData.PrimaryId, platformUserDetailsResult.UserData.NativeId.PlatformIdentifier));
						list2.Add(i);
					}
				}
				if (list != null)
				{
					yield return ResolveUserDetails(PlatformManager.CrossplatformPlatform.UserDetailsService, list, list2, s_userDetailsCurrentlyUpdating);
				}
			}
			if (PlatformManager.NativePlatform.UserDetailsService != null)
			{
				List<UserDetailsRequest> list3 = null;
				List<int> list4 = null;
				for (int j = 0; j < s_userDetailsCurrentlyUpdating.Count; j++)
				{
					PlatformUserDetailsResult platformUserDetailsResult2 = s_userDetailsCurrentlyUpdating[j];
					if (platformUserDetailsResult2.UserData.NativeId != null)
					{
						if (list3 == null)
						{
							list3 = new List<UserDetailsRequest>();
						}
						if (list4 == null)
						{
							list4 = new List<int>();
						}
						list3.Add(new UserDetailsRequest(platformUserDetailsResult2.UserData.NativeId));
						list4.Add(j);
					}
				}
				if (list3 != null)
				{
					yield return ResolveUserDetails(PlatformManager.NativePlatform.UserDetailsService, list3, list4, s_userDetailsCurrentlyUpdating);
				}
			}
			foreach (PlatformUserDetailsResult item in s_userDetailsCurrentlyUpdating)
			{
				if (!string.IsNullOrEmpty(item.Name))
				{
					item.UserData.Name = item.Name;
					PlatformUserManager.DetailsUpdated?.Invoke(item.UserData, item.Name);
				}
			}
			using (s_userDetailsToUpdateLock.WriteLockScope())
			{
				foreach (PlatformUserDetailsResult item2 in s_userDetailsCurrentlyUpdating)
				{
					s_userDetailsToUpdate.Remove(item2.UserData);
				}
			}
		}
		finally
		{
			s_userDetailsCurrentlyUpdating.Clear();
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static IEnumerator ResolveUserDetails(IUserDetailsService service, IReadOnlyList<UserDetailsRequest> requests, IReadOnlyList<int> resultsIndices, List<PlatformUserDetailsResult> results)
		{
			bool inProgress = true;
			service.RequestUserDetailsUpdate(requests, OnComplete);
			while (inProgress)
			{
				yield return true;
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void OnComplete(IReadOnlyList<UserDetailsRequest> completedRequests)
			{
				for (int k = 0; k < requests.Count; k++)
				{
					UserDetailsRequest userDetailsRequest = completedRequests[k];
					int index = resultsIndices[k];
					if (userDetailsRequest.IsSuccess)
					{
						results[index].Name = userDetailsRequest.details.name;
					}
				}
				inProgress = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateBlockedStates()
	{
		if (s_blockedDataCurrentlyUpdating.Count > 0 || s_blockedUsersToUpdate.Count <= 0)
		{
			return;
		}
		bool flag;
		PlatformUserData value;
		using (s_primaryIdToPlatformLock.ReadLockScope())
		{
			flag = s_primaryIdToPlatform.TryGetValue(PlatformManager.MultiPlatform.User.PlatformUserId, out value);
		}
		using (s_blockedUsersToUpdateLock.UpgradableReadLockScope())
		{
			if (flag)
			{
				using (s_blockedUsersToUpdateLock.WriteLockScope())
				{
					s_blockedUsersToUpdate.Remove(value);
				}
			}
			foreach (PlatformUserData item in s_blockedUsersToUpdate)
			{
				s_blockedDataCurrentlyUpdating.Add(new PlatformUserBlockedResults(item));
			}
		}
		if (s_blockedDataCurrentlyUpdating.Count > 0)
		{
			ThreadManager.StartCoroutine(UpdateBlockedStatesCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator UpdateBlockedStatesCoroutine()
	{
		try
		{
			yield return PlatformManager.MultiPlatform.User.ResolveUserBlocks(s_blockedDataCurrentlyUpdatingReadOnly);
			if (BlockedPlayerList.Instance != null)
			{
				yield return ResolveUserBlocksFromBlockList(s_blockedDataCurrentlyUpdatingReadOnly);
			}
			foreach (PlatformUserBlockedResults item in s_blockedDataCurrentlyUpdating)
			{
				if (item.HasErrored)
				{
					continue;
				}
				foreach (EBlockType item2 in EnumUtils.Values<EBlockType>())
				{
					item.User.Blocked[item2].RefreshBlockedState(item.IsBlocked[item2]);
				}
			}
			using (s_blockedUsersToUpdateLock.WriteLockScope())
			{
				foreach (PlatformUserBlockedResults item3 in s_blockedDataCurrentlyUpdating)
				{
					s_blockedUsersToUpdate.Remove(item3.User);
				}
			}
		}
		finally
		{
			s_blockedDataCurrentlyUpdating.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void MarkBlockedStateChangedAll()
	{
		using (s_primaryIdToPlatformLock.ReadLockScope())
		{
			foreach (PlatformUserData value in s_primaryIdToPlatform.Values)
			{
				value.MarkBlockedStateChanged();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnPlatformUserBlocksChanged(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds)
	{
		if (userIds == null)
		{
			MarkBlockedStateChangedAll();
			return;
		}
		using (s_primaryIdToPlatformLock.ReadLockScope())
		{
			foreach (PlatformUserIdentifierAbs userId in userIds)
			{
				if (s_primaryIdToPlatform.TryGetValue(userId, out var value))
				{
					value.MarkBlockedStateChanged();
				}
			}
		}
		using (s_nativeIdToPrimaryIdsLock.ReadLockScope())
		{
			foreach (PlatformUserIdentifierAbs userId2 in userIds)
			{
				if (!s_nativeIdToPrimaryIds.TryGetByKey(userId2, out var values))
				{
					continue;
				}
				using (s_primaryIdToPlatformLock.ReadLockScope())
				{
					foreach (PlatformUserIdentifierAbs item in values)
					{
						s_primaryIdToPlatform[item].MarkBlockedStateChanged();
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanCheckUserDetails()
	{
		if (PlatformManager.CrossplatformPlatform?.UserDetailsService == null)
		{
			return PlatformManager.NativePlatform.UserDetailsService != null;
		}
		return true;
	}

	public static IEnumerator ResolveUserBlocksFromBlockList(IReadOnlyList<IPlatformUserBlockedResults> _results)
	{
		if (BlockedPlayerList.Instance == null)
		{
			yield break;
		}
		while (BlockedPlayerList.Instance.PendingResolve())
		{
			yield return null;
		}
		foreach (BlockedPlayerList.ListEntry item in BlockedPlayerList.Instance.GetEntriesOrdered(_blocked: true, _resolveRequired: false))
		{
			PlatformUserIdentifierAbs primaryId = item.PlayerData.PrimaryId;
			foreach (IPlatformUserBlockedResults _result in _results)
			{
				if (_result.User.PrimaryId.Equals(primaryId))
				{
					_result.BlockAll();
					break;
				}
			}
		}
	}
}
