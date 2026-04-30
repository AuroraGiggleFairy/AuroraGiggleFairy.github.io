using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Platform.XBL;

public static class XblXuidMapper
{
	public delegate void XuidMappedHandler(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid);

	public delegate void UserIdentifierMappedHandler(ulong xuid, PlatformUserIdentifierAbs userId);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class XuidState
	{
		public int AttemptsCompleted;

		public bool InProgress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IUserIdentifierMappingService s_mappingService;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XuidState EmptyXuidState;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly BiMultiDictionary<XuidState, PlatformUserIdentifierAbs> s_xuidStateToUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly BiDictionary<XuidState, ulong> s_xuidStateToXuid;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ReaderWriterLockSlim s_xuidStateDictionaryLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<PlatformUserIdentifierAbs> s_userIdsWithNoXblMapping;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ReaderWriterLockSlim s_userIdsWithNoXblMappingLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSet<ulong> s_xuidsCheckedForUserIdentifiers;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ReaderWriterLockSlim s_xuidsCheckedForUserIdentifiersLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PlatformUserIdentifierAbs> s_xuidMappedResultTemp;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object s_xuidMappedResultTempLock;

	public static event XuidMappedHandler XuidMapped;

	public static event UserIdentifierMappedHandler UserIdentifierMapped;

	[PublicizedFrom(EAccessModifier.Private)]
	static XblXuidMapper()
	{
		EmptyXuidState = new XuidState();
		s_xuidStateToUserId = new BiMultiDictionary<XuidState, PlatformUserIdentifierAbs>();
		s_xuidStateToXuid = new BiDictionary<XuidState, ulong>();
		s_xuidStateDictionaryLock = new ReaderWriterLockSlim();
		s_userIdsWithNoXblMapping = new HashSet<PlatformUserIdentifierAbs>();
		s_userIdsWithNoXblMappingLock = new ReaderWriterLockSlim();
		s_xuidsCheckedForUserIdentifiers = new HashSet<ulong>();
		s_xuidsCheckedForUserIdentifiersLock = new ReaderWriterLockSlim();
		s_xuidMappedResultTemp = new List<PlatformUserIdentifierAbs>();
		s_xuidMappedResultTempLock = new object();
	}

	public static void Enable()
	{
		if (!s_enabled)
		{
			s_enabled = true;
			Log.Out("[XBL-XuidMapper] Enabled.");
		}
	}

	public static ulong GetXuid(PlatformUserIdentifierAbs userId)
	{
		if (!s_enabled)
		{
			return 0uL;
		}
		XuidState xuidState = GetXuidState(userId);
		using (s_xuidStateDictionaryLock.ReadLockScope())
		{
			if (s_xuidStateToXuid.TryGetByKey(xuidState, out var value))
			{
				return value;
			}
		}
		if (s_mappingService == null)
		{
			s_mappingService = PlatformManager.CrossplatformPlatform?.IdMappingService;
		}
		if (s_mappingService == null)
		{
			Log.Error("[XBL-XuidMapper] ID mapping service required to identify Xbl users");
			return 0uL;
		}
		ResolveXuid(xuidState);
		return 0uL;
	}

	public static void SetXuid(PlatformUserIdentifierAbs userId, ulong xuid)
	{
		if (xuid == 0L)
		{
			return;
		}
		XuidState xuidState = GetXuidState(userId);
		ulong value;
		using (s_xuidStateDictionaryLock.UpgradableReadLockScope())
		{
			if (!s_xuidStateToXuid.TryGetByKey(xuidState, out value))
			{
				value = 0uL;
			}
			if (value == xuid)
			{
				return;
			}
			using (s_xuidStateDictionaryLock.WriteLockScope())
			{
				s_xuidStateToXuid.RemoveByKey(xuidState);
				s_xuidStateToXuid.RemoveByValue(xuid);
				s_xuidStateToXuid.Add(xuidState, xuid);
			}
			lock (xuidState)
			{
				xuidState.InProgress = false;
				xuidState.AttemptsCompleted++;
			}
		}
		if (value != 0L)
		{
			using (s_xuidStateDictionaryLock.ReadLockScope())
			{
				s_xuidStateToUserId.TryGetByKey(xuidState, out var values);
				Log.Warning(string.Format("[XBL-XuidMapper] Unexpected mapping change Xuid changed from '{0}' to '{1}' for UserIds: {2}", value, xuid, string.Join(", ", values)));
			}
		}
		lock (s_xuidMappedResultTempLock)
		{
			s_xuidMappedResultTemp.Clear();
			using (s_xuidStateDictionaryLock.ReadLockScope())
			{
				s_xuidStateToUserId.TryGetByKey(xuidState, s_xuidMappedResultTemp);
			}
			XblXuidMapper.XuidMapped?.Invoke(s_xuidMappedResultTemp, xuid);
			s_xuidMappedResultTemp.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResolveXuid(XuidState xuidState)
	{
		List<PlatformUserIdentifierAbs> userIds = null;
		int attempt;
		lock (xuidState)
		{
			if (xuidState.InProgress)
			{
				return;
			}
			using (s_xuidStateDictionaryLock.ReadLockScope())
			{
				if (s_xuidStateToUserId.TryGetByKey(xuidState, out var values))
				{
					foreach (PlatformUserIdentifierAbs item in values)
					{
						if (!s_mappingService.CanQuery(item))
						{
							continue;
						}
						using (s_userIdsWithNoXblMappingLock.ReadLockScope())
						{
							if (s_userIdsWithNoXblMapping.Contains(item))
							{
								continue;
							}
						}
						if (userIds == null)
						{
							userIds = new List<PlatformUserIdentifierAbs>();
						}
						userIds.Add(item);
					}
				}
			}
			if (userIds == null || userIds.Count <= 0)
			{
				return;
			}
			attempt = xuidState.AttemptsCompleted + 1;
			xuidState.InProgress = true;
		}
		int userIdsIndex = -1;
		PlatformUserIdentifierAbs userId;
		ProcessNextId();
		[PublicizedFrom(EAccessModifier.Internal)]
		void AttemptCompleted()
		{
			lock (xuidState)
			{
				if (xuidState.AttemptsCompleted < attempt)
				{
					xuidState.InProgress = false;
					xuidState.AttemptsCompleted = attempt;
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void Callback(MappedAccountQueryResult _result, EPlatformIdentifier _mappedPlatform, string _mappedId, string _mappedName)
		{
			bool flag = false;
			switch (_result)
			{
			case MappedAccountQueryResult.Success:
				flag = true;
				break;
			case MappedAccountQueryResult.MappingNotFound:
				Log.Warning($"[XBL-XuidMapper] No xuid mapping for user id '{userId}'.");
				using (s_userIdsWithNoXblMappingLock.UpgradableReadLockScope())
				{
					if (!s_userIdsWithNoXblMapping.Contains(userId))
					{
						using (s_userIdsWithNoXblMappingLock.WriteLockScope())
						{
							s_userIdsWithNoXblMapping.Add(userId);
						}
					}
				}
				break;
			default:
				Log.Warning($"[XBL-XuidMapper] Mapped account query failed for user id '{userId}'. Result: {_result.ToStringCached()}");
				break;
			}
			ulong _result2;
			if (!flag)
			{
				HandleResult(0uL);
			}
			else if (string.IsNullOrEmpty(_mappedId))
			{
				Log.Warning($"[XBL-XuidMapper] Empty xuid mapping for user id '{userId}'.");
				HandleResult(0uL);
			}
			else if (!StringParsers.TryParseUInt64(_mappedId, out _result2))
			{
				Log.Warning($"[XBL-XuidMapper] Failed to parse xuid '{_mappedId}' for user id '{userId}'.");
				HandleResult(0uL);
			}
			else
			{
				Log.Out($"[XBL-XuidMapper] Mapped user id '{userId}' to xuid '{_result2}'.");
				HandleResult(_result2);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void HandleResult(ulong xuid)
		{
			if (xuid != 0L)
			{
				SetXuid(userId, xuid);
				AttemptCompleted();
			}
			else
			{
				ProcessNextId();
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void ProcessNextId()
		{
			userIdsIndex++;
			lock (xuidState)
			{
				if (!xuidState.InProgress || xuidState.AttemptsCompleted >= attempt || userIdsIndex >= userIds.Count)
				{
					AttemptCompleted();
					return;
				}
			}
			userId = userIds[userIdsIndex];
			Log.Out($"[XBL-XuidMapper] Mapping User Id '{userId}'...");
			PlatformManager.CrossplatformPlatform.IdMappingService.QueryMappedAccountDetails(userId, EPlatformIdentifier.XBL, Callback);
		}
	}

	public static void ResolveUserIdentifiers(IReadOnlyCollection<ulong> xuids)
	{
		if (s_mappingService == null)
		{
			s_mappingService = PlatformManager.CrossplatformPlatform?.IdMappingService;
		}
		if (s_mappingService == null)
		{
			return;
		}
		ulong[] xuidsToRequest;
		using (s_xuidsCheckedForUserIdentifiersLock.ReadLockScope())
		{
			xuidsToRequest = (from xuid in xuids
				where !s_xuidsCheckedForUserIdentifiers.Contains(xuid)
				where s_mappingService.CanReverseQuery(EPlatformIdentifier.XBL, xuid.ToString())
				select xuid).ToArray();
		}
		MappedAccountReverseRequest[] requests;
		if (xuidsToRequest.Length != 0)
		{
			requests = xuidsToRequest.Select([PublicizedFrom(EAccessModifier.Internal)] (ulong xuid) => new MappedAccountReverseRequest(EPlatformIdentifier.XBL, xuid.ToString())).ToArray();
			Log.Out("[XBL-XuidMapper] Reverse mapping XUID(s) '" + string.Join("','", xuids) + "'...");
			s_mappingService.ReverseQueryMappedAccountsDetails(requests, ResolveUserIdentifiersCallback);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void ResolveUserIdentifiersCallback(IReadOnlyList<MappedAccountReverseRequest> _)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				ulong num = xuidsToRequest[i];
				MappedAccountReverseRequest mappedAccountReverseRequest = requests[i];
				switch (mappedAccountReverseRequest.Result)
				{
				case MappedAccountQueryResult.QueryFailed:
					Log.Out($"[XBL-XuidMapper] Reverse query failed for {num}");
					break;
				case MappedAccountQueryResult.MappingNotFound:
					Log.Out($"[XBL-XuidMapper] Could not get user identifier for {num}");
					using (s_xuidsCheckedForUserIdentifiersLock.WriteLockScope())
					{
						s_xuidsCheckedForUserIdentifiers.Add(num);
					}
					break;
				case MappedAccountQueryResult.Success:
					Log.Out($"[XBL-XuidMapper] Resolved user identifier for {num}: {mappedAccountReverseRequest.PlatformId}");
					SetXuid(mappedAccountReverseRequest.PlatformId, num);
					using (s_xuidsCheckedForUserIdentifiersLock.WriteLockScope())
					{
						s_xuidsCheckedForUserIdentifiers.Add(num);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("Result");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XuidState GetXuidState(PlatformUserIdentifierAbs userId)
	{
		using (s_xuidStateDictionaryLock.UpgradableReadLockScope())
		{
			if (s_xuidStateToUserId.TryGetByValue(userId, out var key))
			{
				return key;
			}
			using (s_xuidStateDictionaryLock.WriteLockScope())
			{
				PlatformUserIdentifierAbs[] array = new PlatformUserIdentifierAbs[4];
				int num = PlatformUserManager.TryGetByNative(userId, array.AsSpan(0, array.Length - 1));
				array[num] = userId;
				int num2 = num + 1;
				for (int i = 0; i < num2 && !s_xuidStateToUserId.TryGetByValue(array[i], out key); i++)
				{
				}
				if (key == null)
				{
					key = new XuidState();
				}
				for (int j = 0; j < num2; j++)
				{
					PlatformUserIdentifierAbs value = array[j];
					if (!s_xuidStateToUserId.TryGetByValue(value, out var key2))
					{
						s_xuidStateToUserId.Add(key, value);
					}
					else if (key2 != key)
					{
						s_xuidStateToUserId.RemoveByValue(value);
						s_xuidStateToUserId.Add(key, value);
						Log.Error(string.Format("[XBL-XuidMapper] Unexpected state merge. UserId '{0}' already had state but has been merged with UserIds: '{1}'.", array[j], string.Join("', '", array.Take(num2))));
					}
				}
			}
			return key;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XuidState GetXuidState(ulong xuid)
	{
		if (xuid == 0L)
		{
			return EmptyXuidState;
		}
		using (s_xuidStateDictionaryLock.UpgradableReadLockScope())
		{
			if (s_xuidStateToXuid.TryGetByValue(xuid, out var key))
			{
				return key;
			}
			using (s_xuidStateDictionaryLock.WriteLockScope())
			{
				key = new XuidState();
				s_xuidStateToXuid.Add(key, xuid);
			}
			return key;
		}
	}

	public static void ResolveXuids(IReadOnlyList<XuidResolveRequest> _requests, Action<IReadOnlyList<XuidResolveRequest>> _onComplete)
	{
		IUserIdentifierMappingService userIdentifierMappingService = PlatformManager.CrossplatformPlatform?.IdMappingService;
		if (userIdentifierMappingService == null)
		{
			Log.Error("[XBL-XuidMapper] Cannot resolve xuids, no mapping service available");
			_onComplete(_requests);
			return;
		}
		List<MappedAccountRequest> list = null;
		List<int> requestIndices = null;
		for (int i = 0; i < _requests.Count; i++)
		{
			XuidResolveRequest xuidResolveRequest = _requests[i];
			XuidState xuidState = GetXuidState(xuidResolveRequest.Id);
			IReadOnlyCollection<PlatformUserIdentifierAbs> values;
			using (s_xuidStateDictionaryLock.ReadLockScope())
			{
				if (!s_xuidStateToXuid.TryGetByKey(xuidState, out var value))
				{
					value = 0uL;
				}
				if (value != 0L)
				{
					xuidResolveRequest.Xuid = value;
					xuidResolveRequest.IsSuccess = true;
					continue;
				}
				if (!s_xuidStateToUserId.TryGetByKey(xuidState, out values))
				{
					continue;
				}
			}
			foreach (PlatformUserIdentifierAbs item in values)
			{
				if (!userIdentifierMappingService.CanQuery(item))
				{
					continue;
				}
				using (s_userIdsWithNoXblMappingLock.ReadLockScope())
				{
					if (s_userIdsWithNoXblMapping.Contains(item))
					{
						continue;
					}
				}
				if (list == null)
				{
					list = new List<MappedAccountRequest>();
				}
				if (requestIndices == null)
				{
					requestIndices = new List<int>();
				}
				list.Add(new MappedAccountRequest(item, EPlatformIdentifier.XBL));
				requestIndices.Add(i);
				break;
			}
		}
		if (list == null)
		{
			_onComplete(_requests);
		}
		else
		{
			userIdentifierMappingService.QueryMappedAccountsDetails(list, Callback);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void Callback(IReadOnlyList<MappedAccountRequest> completedMappingRequests)
		{
			for (int j = 0; j < completedMappingRequests.Count; j++)
			{
				MappedAccountRequest mappedAccountRequest = completedMappingRequests[j];
				int index = requestIndices[j];
				XuidResolveRequest xuidResolveRequest2 = _requests[index];
				switch (mappedAccountRequest.Result)
				{
				case MappedAccountQueryResult.QueryFailed:
					Log.Out($"[XBL-XuidMapper] query failed for {mappedAccountRequest.Id}");
					break;
				case MappedAccountQueryResult.MappingNotFound:
					Log.Out($"[XBL-XuidMapper] could not get xuid for {mappedAccountRequest.Id}");
					using (s_userIdsWithNoXblMappingLock.UpgradableReadLockScope())
					{
						if (!s_userIdsWithNoXblMapping.Contains(mappedAccountRequest.Id))
						{
							using (s_userIdsWithNoXblMappingLock.WriteLockScope())
							{
								s_userIdsWithNoXblMapping.Add(mappedAccountRequest.Id);
							}
						}
					}
					break;
				case MappedAccountQueryResult.Success:
				{
					if (!StringParsers.TryParseUInt64(mappedAccountRequest.MappedAccountId, out var _result))
					{
						Log.Warning($"[XBL-XuidMapper] Failed to parse xuid '{mappedAccountRequest.MappedAccountId}' for user id '{mappedAccountRequest.Id}'.");
					}
					else
					{
						xuidResolveRequest2.IsSuccess = true;
						xuidResolveRequest2.Xuid = _result;
						SetXuid(mappedAccountRequest.Id, _result);
					}
					break;
				}
				}
			}
			_onComplete(_requests);
		}
	}
}
