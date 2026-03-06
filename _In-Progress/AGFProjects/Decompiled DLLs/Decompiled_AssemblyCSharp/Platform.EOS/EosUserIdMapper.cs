using System;
using System.Collections.Generic;
using System.Threading;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS;

public class EosUserIdMapper : IUserIdentifierMappingService
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Api api;

	[PublicizedFrom(EAccessModifier.Private)]
	public User user;

	public EosUserIdMapper(Api _eosApi, User _eosUser)
	{
		api = _eosApi;
		user = _eosUser;
	}

	public bool CanQuery(PlatformUserIdentifierAbs _id)
	{
		return _id is UserIdentifierEos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryValidateUser(out ProductUserId loggedInUser)
	{
		if (!(user.PlatformUserId is UserIdentifierEos userIdentifierEos))
		{
			Log.Error($"[EOS] Cannot query mapped account details. EosUserIdMapper has wrong id type {user.PlatformUserId}");
			loggedInUser = null;
			return false;
		}
		loggedInUser = userIdentifierEos.ProductUserId;
		if (loggedInUser == null)
		{
			Log.Error($"[EOS] Cannot query mapped account details. {userIdentifierEos} is not logged in");
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryValidateRequest(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, out ProductUserId _puid)
	{
		if (!(_id is UserIdentifierEos userIdentifierEos))
		{
			Log.Error($"[EOS] Cannot retrieve mapped account details, {_id} is not an eos product user id");
			_puid = null;
			return false;
		}
		_puid = userIdentifierEos.ProductUserId;
		if (!EosHelpers.PlatformIdentifierMappings.ContainsKey(_platform))
		{
			Log.Error($"[EOS] Cannot retrieve mapped acount details, target platform {_platform} does not map to a known external account type");
			return false;
		}
		return true;
	}

	public void QueryMappedAccountDetails(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
	{
		if (TryValidateUser(out var loggedInUser) && TryValidateRequest(_id, _platform, out var _puid))
		{
			QueryMappedExternalAccount(loggedInUser, _puid, _platform, _callback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueryMappedExternalAccount(ProductUserId _loggedInUser, ProductUserId _puid, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
	{
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccount", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				QueryMappedExternalAccount(_loggedInUser, _puid, _platform, _callback);
			});
			return;
		}
		if (api.ConnectInterface == null)
		{
			Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
			_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
			return;
		}
		if (!EosHelpers.PlatformIdentifierMappings.TryGetValue(_platform, out var externalAccountType))
		{
			Log.Out($"[EOS] Unknown external account type for {_platform}");
			_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
			return;
		}
		QueryProductUserIdMappingsOptions options = new QueryProductUserIdMappingsOptions
		{
			LocalUserId = _loggedInUser,
			ProductUserIds = new ProductUserId[1] { _puid }
		};
		lock (AntiCheatCommon.LockObject)
		{
			api.ConnectInterface.QueryProductUserIdMappings(ref options, options.ProductUserIds, [PublicizedFrom(EAccessModifier.Internal)] (ref QueryProductUserIdMappingsCallbackInfo _response) =>
			{
				if (_response.ResultCode != Result.Success)
				{
					Log.Out($"[EOS] QueryProductUserIdMappings failed {_response.ResultCode}");
					_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				}
				else if (api.ConnectInterface == null)
				{
					Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
					_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				}
				else
				{
					ProductUserId targetUserId = ((ProductUserId[])_response.ClientData)[0];
					CopyProductUserExternalAccountByAccountTypeOptions copyOptions = new CopyProductUserExternalAccountByAccountTypeOptions
					{
						TargetUserId = targetUserId,
						AccountIdType = externalAccountType
					};
					if (!TryCopyResult(copyOptions, out var _externalAccountInfo))
					{
						_callback(MappedAccountQueryResult.MappingNotFound, _platform, null, null);
					}
					else
					{
						Log.Out($"[EOS] found external account for {_puid}: Type: {externalAccountType}, Id: {_externalAccountInfo.AccountId}");
						_callback(MappedAccountQueryResult.Success, _platform, _externalAccountInfo.AccountId, _externalAccountInfo.DisplayName);
					}
				}
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryCopyResult(CopyProductUserExternalAccountByAccountTypeOptions _copyOptions, out ExternalAccountInfo _externalAccountInfo)
	{
		Result result;
		ExternalAccountInfo? outExternalAccountInfo;
		lock (AntiCheatCommon.LockObject)
		{
			result = api.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref _copyOptions, out outExternalAccountInfo);
		}
		if (result != Result.Success)
		{
			Log.Out($"[EOS] {_copyOptions.TargetUserId} copy failed. Result: {result}");
			_externalAccountInfo = default(ExternalAccountInfo);
			return false;
		}
		if (!outExternalAccountInfo.HasValue)
		{
			Log.Out($"[EOS] {_copyOptions.TargetUserId} copy failed, null info");
			_externalAccountInfo = default(ExternalAccountInfo);
			return false;
		}
		_externalAccountInfo = outExternalAccountInfo.Value;
		return true;
	}

	public void QueryMappedAccountsDetails(IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
	{
		ProductUserId loggedInUser;
		if (_requests.Count == 0)
		{
			_callback(_requests);
		}
		else if (TryValidateUser(out loggedInUser))
		{
			QueryMappedExternalAccounts(loggedInUser, _requests, _callback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueryMappedExternalAccounts(ProductUserId _loggedInUser, IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
	{
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccounts", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				QueryMappedExternalAccounts(_loggedInUser, _requests, _callback);
			});
			return;
		}
		if (api.ConnectInterface == null)
		{
			Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
			foreach (MappedAccountRequest _request in _requests)
			{
				_request.Result = MappedAccountQueryResult.QueryFailed;
			}
			_callback(_requests);
			return;
		}
		List<ProductUserId> puids = null;
		List<int> requestIndices = null;
		for (int num = 0; num < _requests.Count; num++)
		{
			MappedAccountRequest mappedAccountRequest = _requests[num];
			if (!TryValidateRequest(mappedAccountRequest.Id, mappedAccountRequest.Platform, out var _puid))
			{
				mappedAccountRequest.Result = MappedAccountQueryResult.QueryFailed;
				continue;
			}
			if (puids == null)
			{
				puids = new List<ProductUserId>();
			}
			if (requestIndices == null)
			{
				requestIndices = new List<int>();
			}
			puids.Add(_puid);
			requestIndices.Add(num);
		}
		if (puids == null)
		{
			_callback(_requests);
			return;
		}
		QueryProductUserIdMappingsOptions options = new QueryProductUserIdMappingsOptions
		{
			LocalUserId = _loggedInUser,
			ProductUserIds = puids.ToArray()
		};
		lock (AntiCheatCommon.LockObject)
		{
			api.ConnectInterface.QueryProductUserIdMappings(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref QueryProductUserIdMappingsCallbackInfo _response) =>
			{
				if (_response.ResultCode != Result.Success)
				{
					Log.Out($"[EOS] QueryProductUserIdMappings failed {_response.ResultCode}");
					foreach (MappedAccountRequest _request2 in _requests)
					{
						_request2.Result = MappedAccountQueryResult.QueryFailed;
					}
					_callback(_requests);
				}
				else if (api.ConnectInterface == null)
				{
					Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
					foreach (MappedAccountRequest _request3 in _requests)
					{
						_request3.Result = MappedAccountQueryResult.QueryFailed;
					}
					_callback(_requests);
				}
				else
				{
					CopyProductUserExternalAccountByAccountTypeOptions copyOptions = default(CopyProductUserExternalAccountByAccountTypeOptions);
					for (int i = 0; i < puids.Count; i++)
					{
						int index = requestIndices[i];
						MappedAccountRequest mappedAccountRequest2 = _requests[index];
						ProductUserId arg = (copyOptions.TargetUserId = puids[i]);
						copyOptions.AccountIdType = EosHelpers.PlatformIdentifierMappings[mappedAccountRequest2.Platform];
						if (!TryCopyResult(copyOptions, out var _externalAccountInfo))
						{
							mappedAccountRequest2.Result = MappedAccountQueryResult.MappingNotFound;
						}
						else
						{
							mappedAccountRequest2.MappedAccountId = _externalAccountInfo.AccountId;
							mappedAccountRequest2.DisplayName = _externalAccountInfo.DisplayName;
							mappedAccountRequest2.Result = MappedAccountQueryResult.Success;
							Log.Out($"[EOS] found external account for {arg}: Type: {_externalAccountInfo.AccountIdType}, Id: {_externalAccountInfo.AccountId}");
						}
					}
					_callback(_requests);
				}
			});
		}
	}

	public bool CanReverseQuery(EPlatformIdentifier _platform, string _platformId)
	{
		return EosHelpers.PlatformIdentifierMappings.ContainsKey(_platform);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryValidateReverseRequest(EPlatformIdentifier _platform, out ExternalAccountType _externalAccountType)
	{
		if (!EosHelpers.PlatformIdentifierMappings.TryGetValue(_platform, out _externalAccountType))
		{
			Log.Error($"[EOS] Cannot retrieve reverse mapped account details, target platform {_platform} does not map to a known external account type");
			return false;
		}
		return true;
	}

	public void ReverseQueryMappedAccountDetails(EPlatformIdentifier _platform, string _platformId, MappedAccountReverseQueryCallback _callback)
	{
		ReverseQueryMappedAccountsDetails(new MappedAccountReverseRequest[1]
		{
			new MappedAccountReverseRequest(_platform, _platformId)
		}, Callback);
		[PublicizedFrom(EAccessModifier.Internal)]
		void Callback(IReadOnlyList<MappedAccountReverseRequest> _completedRequests)
		{
			MappedAccountReverseRequest mappedAccountReverseRequest = _completedRequests[0];
			_callback(mappedAccountReverseRequest.Result, mappedAccountReverseRequest.PlatformId);
		}
	}

	public void ReverseQueryMappedAccountsDetails(IReadOnlyList<MappedAccountReverseRequest> _requests, MappedAccountsReverseQueryCallback _callback)
	{
		ProductUserId loggedInUser;
		if (_requests.Count == 0)
		{
			_callback(_requests);
		}
		else if (TryValidateUser(out loggedInUser))
		{
			QueryExternalAccountMappings(loggedInUser, _requests, _callback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueryExternalAccountMappings(ProductUserId _loggedInUser, IReadOnlyList<MappedAccountReverseRequest> _requests, MappedAccountsReverseQueryCallback _callback)
	{
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("QueryEosExternalAccountMappings", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				QueryExternalAccountMappings(_loggedInUser, _requests, _callback);
			});
			return;
		}
		if (api.ConnectInterface == null)
		{
			Log.Out("[EOS] QueryExternalAccountMappings failed, connect interface null");
			foreach (MappedAccountReverseRequest _request in _requests)
			{
				_request.Result = MappedAccountQueryResult.QueryFailed;
			}
			_callback(_requests);
			return;
		}
		Dictionary<ExternalAccountType, List<MappedAccountReverseRequest>> requestsByType = null;
		foreach (MappedAccountReverseRequest _request2 in _requests)
		{
			if (!TryValidateReverseRequest(_request2.Platform, out var _externalAccountType))
			{
				_request2.Result = MappedAccountQueryResult.QueryFailed;
				continue;
			}
			if (requestsByType == null)
			{
				requestsByType = new Dictionary<ExternalAccountType, List<MappedAccountReverseRequest>>();
			}
			if (!requestsByType.TryGetValue(_externalAccountType, out var value))
			{
				value = new List<MappedAccountReverseRequest>();
				requestsByType[_externalAccountType] = value;
			}
			value.Add(_request2);
		}
		if (requestsByType == null)
		{
			_callback(_requests);
			return;
		}
		int requestsFinished = 0;
		foreach (var (externalAccountType2, requests) in requestsByType)
		{
			QueryExternalAccountMappings(_loggedInUser, externalAccountType2, requests, Callback);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void Callback()
		{
			int num = Interlocked.Increment(ref requestsFinished);
			if (num >= requestsByType.Count)
			{
				if (num > requestsByType.Count)
				{
					Log.Error($"[EOS] Expected {requestsByType.Count} external account callbacks but got {num}.");
				}
				else
				{
					_callback(_requests);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueryExternalAccountMappings(ProductUserId _loggedInUser, ExternalAccountType _externalAccountType, IReadOnlyList<MappedAccountReverseRequest> _requests, Action _callback)
	{
		Utf8String[] externalAccountIds = new Utf8String[_requests.Count];
		for (int i = 0; i < _requests.Count; i++)
		{
			externalAccountIds[i] = _requests[i].Id;
		}
		QueryExternalAccountMappingsOptions options = new QueryExternalAccountMappingsOptions
		{
			LocalUserId = _loggedInUser,
			AccountIdType = _externalAccountType,
			ExternalAccountIds = externalAccountIds
		};
		lock (AntiCheatCommon.LockObject)
		{
			api.ConnectInterface.QueryExternalAccountMappings(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref QueryExternalAccountMappingsCallbackInfo _response) =>
			{
				if (_response.ResultCode != Result.Success)
				{
					Log.Out($"[EOS] QueryProductUserIdMappings failed {_response.ResultCode}");
					foreach (MappedAccountReverseRequest _request in _requests)
					{
						_request.Result = MappedAccountQueryResult.QueryFailed;
					}
					_callback();
				}
				else if (api.ConnectInterface == null)
				{
					Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
					foreach (MappedAccountReverseRequest _request2 in _requests)
					{
						_request2.Result = MappedAccountQueryResult.QueryFailed;
					}
					_callback();
				}
				else
				{
					GetExternalAccountMappingsOptions options2 = new GetExternalAccountMappingsOptions
					{
						LocalUserId = _loggedInUser,
						AccountIdType = _externalAccountType
					};
					for (int j = 0; j < externalAccountIds.Length; j++)
					{
						options2.TargetExternalUserId = externalAccountIds[j];
						MappedAccountReverseRequest mappedAccountReverseRequest = _requests[j];
						ProductUserId externalAccountMapping;
						lock (AntiCheatCommon.LockObject)
						{
							externalAccountMapping = api.ConnectInterface.GetExternalAccountMapping(ref options2);
						}
						if (externalAccountMapping == null || !externalAccountMapping.IsValid())
						{
							mappedAccountReverseRequest.Result = MappedAccountQueryResult.MappingNotFound;
						}
						else
						{
							mappedAccountReverseRequest.Result = MappedAccountQueryResult.Success;
							mappedAccountReverseRequest.PlatformId = new UserIdentifierEos(externalAccountMapping);
							Log.Out($"[EOS] found EOS account {externalAccountMapping} from external account: Type: {_externalAccountType}, Id: {mappedAccountReverseRequest.Id}");
						}
					}
					_callback();
				}
			});
		}
	}
}
