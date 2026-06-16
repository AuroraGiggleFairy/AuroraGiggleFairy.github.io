using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using UnityEngine;

namespace Platform.EOS;

public class SessionsClient : IServerListInterface
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ESessionSearchType
	{
		Single,
		NoCrossplatform,
		OnlyCrossplatform
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SessionSearchArgs
	{
		public readonly SessionSearch SearchHandle;

		public readonly MicroStopwatch Stopwatch;

		public readonly GameServerFoundCallback Callback;

		public readonly EServerRelationType RelationType;

		public readonly bool CallbackOnFailure;

		public readonly bool UpdateRefreshing;

		public readonly ESessionSearchType SearchType;

		public SessionSearchArgs(SessionSearch _searchHandle, MicroStopwatch _stopwatch, GameServerFoundCallback _callback, EServerRelationType _relation, bool _callbackOnFailure, bool _updateRefreshing, ESessionSearchType _searchType)
		{
			SearchHandle = _searchHandle;
			Stopwatch = _stopwatch;
			Callback = _callback;
			RelationType = _relation;
			CallbackOnFailure = _callbackOnFailure;
			UpdateRefreshing = _updateRefreshing;
			SearchType = _searchType;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public SessionsInterface sessionsInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<ESessionSearchType> refreshingSearchTypes = new HashSet<ESessionSearchType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerFoundCallback gameServerFoundCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public MaxResultsReachedCallback maxResultsCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public ServerSearchErrorCallback sessionSearchErrorCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string compatibilityVersionString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pingCoroutineStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int concurrentPingRequests = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pingRequestsInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<GameServerInfo> serverPingsToGet = new Queue<GameServerInfo>();

	public bool IsPrefiltered => true;

	public bool IsRefreshing => refreshingSearchTypes.Count > 0;

	public SessionsClient()
	{
		compatibilityVersionString = Constants.cVersionInformation.SerializableString;
		compatibilityVersionString = compatibilityVersionString.Substring(0, compatibilityVersionString.LastIndexOf('.') + 1);
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += apiInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void apiInitialized()
	{
		lock (AntiCheatCommon.LockObject)
		{
			sessionsInterface = ((Api)owner.Api).PlatformInterface.GetSessionsInterface();
		}
	}

	public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
	{
		gameServerFoundCallback = _serverFound;
		maxResultsCallback = _maxResultsCallback;
		sessionSearchErrorCallback = _sessionSearchErrorCallback;
	}

	public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		if (!pingCoroutineStarted)
		{
			ThreadManager.StartCoroutine(getServerPingsCo());
		}
		EosHelpers.AssertMainThread("SeCl.Start");
		bool flag = PermissionsManager.IsCrossplayAllowed();
		List<IServerListInterface.ServerFilter> list = new List<IServerListInterface.ServerFilter>();
		bool? flag2 = null;
		foreach (IServerListInterface.ServerFilter _activeFilter in _activeFilters)
		{
			if (_activeFilter.Name.ContainsCaseInsensitive(GameInfoString.PlayGroup.ToStringCached()))
			{
				Log.Error("[EOS] Play group should not be filterable by the user. It is for internal use only. Ignoring.");
			}
			else if (_activeFilter.Name.ContainsCaseInsensitive(GameInfoBool.AllowCrossplay.ToStringCached()))
			{
				flag2 = _activeFilter.BoolValue;
			}
			else
			{
				list.Add(_activeFilter);
			}
		}
		if (flag)
		{
			if (flag2.HasValue)
			{
				AddCrossplayFiltersAndSearch(list, flag2.Value);
				return;
			}
			List<IServerListInterface.ServerFilter> filters = new List<IServerListInterface.ServerFilter>(list);
			AddCrossplayFiltersAndSearch(list, _allowCrossplay: false);
			AddCrossplayFiltersAndSearch(filters, _allowCrossplay: true);
		}
		else if (flag2 == true)
		{
			Log.Warning("[EOS] Active filter set for servers that allow crossplay, but client does not have crossplay permissions. No work to do.");
		}
		else
		{
			AddCrossplayFiltersAndSearch(list, _allowCrossplay: false);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void AddCrossplayFiltersAndSearch(List<IServerListInterface.ServerFilter> _filters, bool _allowCrossplay)
		{
			_filters.Add(new IServerListInterface.ServerFilter(GameInfoBool.AllowCrossplay.ToStringCached(), IServerListInterface.ServerFilter.EServerFilterType.BoolValue, 0, 0, _allowCrossplay));
			if (_allowCrossplay)
			{
				Log.Out("[EOS] Searching for servers that allow crossplay.");
				_filters.Add(new IServerListInterface.ServerFilter(GameInfoBool.SanctionsIgnored.ToStringCached(), IServerListInterface.ServerFilter.EServerFilterType.BoolValue));
				_filters.Add(new IServerListInterface.ServerFilter(GameInfoInt.MaxPlayers.ToStringCached(), IServerListInterface.ServerFilter.EServerFilterType.IntMax, 0, 8));
				Log.Out("[EOS] Searching for servers that have crossplay compatible settings.");
			}
			else
			{
				string text = EPlayGroupExtensions.Current.ToStringCached();
				_filters.Add(new IServerListInterface.ServerFilter(GameInfoString.PlayGroup.ToStringCached(), IServerListInterface.ServerFilter.EServerFilterType.StringValue, 0, 0, _boolValue: false, text));
				Log.Out("[EOS] Searching for servers that do not allow crossplay and are in the play group '" + text + "'.");
			}
			StartSearchInternal(_filters, (!_allowCrossplay) ? ESessionSearchType.NoCrossplatform : ESessionSearchType.OnlyCrossplatform);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSearchInternal(IList<IServerListInterface.ServerFilter> _activeFilters, ESessionSearchType searchType)
	{
		CreateSessionSearchOptions options = new CreateSessionSearchOptions
		{
			MaxSearchResults = 200u
		};
		Result result;
		SessionSearch outSessionSearchHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = sessionsInterface.CreateSessionSearch(ref options, out outSessionSearchHandle);
		}
		if (result != Result.Success)
		{
			sessionSearchErrorCallback?.Invoke(Localization.Get("xuiServerBrowserSearchErrorEOS"));
			Log.Error("[EOS] Failed creating sessions search: " + result.ToStringCached());
			return;
		}
		if (!setSearchParameters(outSessionSearchHandle, _activeFilters))
		{
			lock (AntiCheatCommon.LockObject)
			{
				outSessionSearchHandle.Release();
				return;
			}
		}
		MicroStopwatch stopwatch = new MicroStopwatch(_bStart: true);
		Log.Out($"[EOS] Starting session search with {_activeFilters.Count} filters");
		SessionSearchFindOptions options2 = new SessionSearchFindOptions
		{
			LocalUserId = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId
		};
		refreshingSearchTypes.Add(searchType);
		lock (AntiCheatCommon.LockObject)
		{
			outSessionSearchHandle.Find(ref options2, new SessionSearchArgs(outSessionSearchHandle, stopwatch, gameServerFoundCallback, EServerRelationType.Internet, _callbackOnFailure: false, _updateRefreshing: true, searchType), searchFinishedCallback);
		}
	}

	public void StopSearch()
	{
		refreshingSearchTypes.Clear();
		serverPingsToGet.Clear();
	}

	public void Disconnect()
	{
		StopSearch();
		gameServerFoundCallback = null;
		maxResultsCallback = null;
	}

	public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
	{
		string value = _serverInfo.GetValue(GameInfoString.UniqueId);
		if (string.IsNullOrEmpty(value))
		{
			Log.Error("[EOS] No session to search for in server info");
			_callback(owner, null, _relation);
			return;
		}
		EosHelpers.AssertMainThread("SeCl.Single");
		CreateSessionSearchOptions options = new CreateSessionSearchOptions
		{
			MaxSearchResults = 200u
		};
		Result result;
		SessionSearch outSessionSearchHandle;
		lock (AntiCheatCommon.LockObject)
		{
			result = sessionsInterface.CreateSessionSearch(ref options, out outSessionSearchHandle);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed creating sessions search: " + result.ToStringCached());
			_callback(owner, null, _relation);
			return;
		}
		SessionSearchSetSessionIdOptions options2 = new SessionSearchSetSessionIdOptions
		{
			SessionId = value
		};
		lock (AntiCheatCommon.LockObject)
		{
			result = outSessionSearchHandle.SetSessionId(ref options2);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed setting search session: " + result.ToStringCached());
			outSessionSearchHandle.Release();
			_callback(owner, null, _relation);
			return;
		}
		MicroStopwatch stopwatch = new MicroStopwatch(_bStart: true);
		SessionSearchFindOptions options3 = new SessionSearchFindOptions
		{
			LocalUserId = ((UserIdentifierEos)owner.User.PlatformUserId).ProductUserId
		};
		refreshingSearchTypes.Add(ESessionSearchType.Single);
		lock (AntiCheatCommon.LockObject)
		{
			outSessionSearchHandle.Find(ref options3, new SessionSearchArgs(outSessionSearchHandle, stopwatch, _callback, _relation, _callbackOnFailure: true, _updateRefreshing: false, ESessionSearchType.Single), searchFinishedCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setSearchParameters(SessionSearch _searchHandle, IList<IServerListInterface.ServerFilter> _activeFilters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!setSingleSearchParameter(_searchHandle, stringBuilder, GameInfoString.ServerVersion.ToStringCached(), AttributeType.String, ComparisonOp.Contains, ":", 0, _boolValue: false, compatibilityVersionString))
		{
			return false;
		}
		string matchmakingGroupTag = SessionsHost.GetMatchmakingGroupTag(PlatformManager.MultiPlatform.User.GetMatchmakingGroup());
		setSingleSearchParameter(_searchHandle, stringBuilder, SessionsInterface.SEARCH_BUCKET_ID, AttributeType.String, ComparisonOp.Equal, "=", 0, _boolValue: false, matchmakingGroupTag);
		if (_activeFilters.Count == 0)
		{
			Log.Warning("[EOS] Session search started without any filters from: " + StackTraceUtility.ExtractStackTrace());
			return setSingleSearchParameter(_searchHandle, stringBuilder, GameInfoString.LevelName.ToStringCached(), AttributeType.String, ComparisonOp.Contains, ":", 0, _boolValue: false, "");
		}
		foreach (IServerListInterface.ServerFilter _activeFilter in _activeFilters)
		{
			bool flag;
			switch (_activeFilter.Type)
			{
			case IServerListInterface.ServerFilter.EServerFilterType.BoolValue:
			{
				string stringValue = "," + _activeFilter.Name + "=" + (_activeFilter.BoolValue ? "1" : "0") + ",";
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, "-BoolValues-", AttributeType.String, ComparisonOp.Contains, ":", 0, _boolValue: false, stringValue);
				break;
			}
			case IServerListInterface.ServerFilter.EServerFilterType.IntValue:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Equal, "=", _activeFilter.IntMinValue);
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.IntNotValue:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Notequal, "!=", _activeFilter.IntMinValue);
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.IntMin:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Greaterthanorequal, ">=", _activeFilter.IntMinValue);
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.IntMax:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Lessthanorequal, "<=", _activeFilter.IntMaxValue);
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.IntRange:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Greaterthanorequal, ">=", _activeFilter.IntMinValue);
				if (flag)
				{
					flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.Int64, ComparisonOp.Lessthanorequal, "<=", _activeFilter.IntMaxValue);
				}
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.StringValue:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.String, ComparisonOp.Contains, "=", 0, _boolValue: false, "~$#$~" + _activeFilter.StringNeedle.ToLowerInvariant());
				break;
			case IServerListInterface.ServerFilter.EServerFilterType.StringContains:
				flag = setSingleSearchParameter(_searchHandle, stringBuilder, _activeFilter.Name, AttributeType.String, ComparisonOp.Contains, ":", 0, _boolValue: false, _activeFilter.StringNeedle.ToLowerInvariant());
				break;
			default:
				throw new ArgumentOutOfRangeException("Type", _activeFilter.Type, null);
			case IServerListInterface.ServerFilter.EServerFilterType.Any:
				continue;
			}
			if (!flag)
			{
				return false;
			}
		}
		Log.Out("[EOS] Session search filters: " + stringBuilder);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setSingleSearchParameter(SessionSearch _searchHandle, StringBuilder _sb, string _key, AttributeType _type, ComparisonOp _comparison, string _comparisonString, int _intValue = 0, bool _boolValue = false, string _stringValue = null)
	{
		AttributeDataValue value = _type switch
		{
			AttributeType.String => _stringValue, 
			AttributeType.Boolean => _boolValue, 
			AttributeType.Int64 => _intValue, 
			AttributeType.Double => throw new NotSupportedException("[EOS] Session attribute search type Double not supported!"), 
			_ => throw new ArgumentOutOfRangeException("_type", _type, null), 
		};
		SessionSearchSetParameterOptions options = new SessionSearchSetParameterOptions
		{
			Parameter = new AttributeData
			{
				Key = _key,
				Value = value
			},
			ComparisonOp = _comparison
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = _searchHandle.SetParameter(ref options);
		}
		string text = _type switch
		{
			AttributeType.String => _stringValue, 
			AttributeType.Boolean => _boolValue.ToString(), 
			AttributeType.Int64 => _intValue.ToString(), 
			AttributeType.Double => throw new NotSupportedException("[EOS] Session attribute search type Double not supported!"), 
			_ => throw new ArgumentOutOfRangeException("_type", _type, null), 
		};
		if (result != Result.Success)
		{
			Log.Error("[EOS] Failed setting search param '" + _key + "' to '" + text + "': " + result.ToStringCached());
		}
		_sb.Append(_key);
		_sb.Append(_comparisonString);
		_sb.Append(text);
		_sb.Append(", ");
		return result == Result.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void searchFinishedCallback(ref SessionSearchFindCallbackInfo _callbackData)
	{
		SessionSearchArgs sessionSearchArgs = (SessionSearchArgs)_callbackData.ClientData;
		sessionSearchArgs.Stopwatch.Stop();
		Log.Out($"[EOS] Search took: {sessionSearchArgs.Stopwatch.ElapsedMilliseconds} ms");
		if (_callbackData.ResultCode != Result.Success)
		{
			sessionSearchErrorCallback?.Invoke(Localization.Get("xuiServerBrowserSearchErrorEOS"));
			Log.Error("[EOS] Failed searching sessions on backend: " + _callbackData.ResultCode.ToStringCached());
			lock (AntiCheatCommon.LockObject)
			{
				sessionSearchArgs.SearchHandle.Release();
			}
			if (sessionSearchArgs.UpdateRefreshing)
			{
				refreshingSearchTypes.Remove(sessionSearchArgs.SearchType);
			}
			if (sessionSearchArgs.CallbackOnFailure)
			{
				sessionSearchArgs.Callback?.Invoke(owner, null, sessionSearchArgs.RelationType);
			}
			return;
		}
		SessionSearchGetSearchResultCountOptions options = default(SessionSearchGetSearchResultCountOptions);
		uint searchResultCount;
		lock (AntiCheatCommon.LockObject)
		{
			searchResultCount = sessionSearchArgs.SearchHandle.GetSearchResultCount(ref options);
		}
		if (sessionSearchArgs.SearchType != ESessionSearchType.Single)
		{
			maxResultsCallback?.Invoke(owner, searchResultCount >= 200, 200);
		}
		Log.Out("[EOS] Sessions received: " + searchResultCount);
		for (uint num = 0u; num < searchResultCount; num++)
		{
			SessionSearchCopySearchResultByIndexOptions options2 = new SessionSearchCopySearchResultByIndexOptions
			{
				SessionIndex = num
			};
			Result result;
			SessionDetails outSessionHandle;
			lock (AntiCheatCommon.LockObject)
			{
				result = sessionSearchArgs.SearchHandle.CopySearchResultByIndex(ref options2, out outSessionHandle);
			}
			if (result != Result.Success)
			{
				Log.Error($"[EOS] Failed getting session {num} data: {result.ToStringCached()}");
				continue;
			}
			SessionDetailsCopyInfoOptions options3 = default(SessionDetailsCopyInfoOptions);
			SessionDetailsInfo? outSessionInfo;
			lock (AntiCheatCommon.LockObject)
			{
				result = outSessionHandle.CopyInfo(ref options3, out outSessionInfo);
			}
			if (result != Result.Success)
			{
				Log.Error($"[EOS] Failed getting session {num} data details: {result.ToStringCached()}");
				outSessionHandle.Release();
				continue;
			}
			string text = null;
			text = GameUtils.GetLaunchArgument("debugsessions");
			if (text != null && !debugLogSessionInfo(num, outSessionHandle, outSessionInfo.Value, text == "verbose"))
			{
				outSessionHandle.Release();
				continue;
			}
			GameServerInfo gameServerInfo = parseSession(outSessionHandle, outSessionInfo.Value);
			if (gameServerInfo != null)
			{
				serverPingsToGet.Enqueue(gameServerInfo);
				sessionSearchArgs.Callback?.Invoke(owner, gameServerInfo, sessionSearchArgs.RelationType);
			}
			outSessionHandle.Release();
		}
		if (searchResultCount == 0 && sessionSearchArgs.CallbackOnFailure)
		{
			sessionSearchArgs.Callback?.Invoke(owner, null, sessionSearchArgs.RelationType);
		}
		sessionSearchArgs.SearchHandle.Release();
		if (sessionSearchArgs.UpdateRefreshing)
		{
			refreshingSearchTypes.Remove(sessionSearchArgs.SearchType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo parseSession(SessionDetails _sessionDetails, SessionDetailsInfo _sessionDetailsInfo)
	{
		GameServerInfo gameServerInfo = new GameServerInfo();
		GameServerInfo gameServerInfo2 = gameServerInfo;
		gameServerInfo2.SetValue(GameInfoInt.ServerVisibility, _sessionDetailsInfo.Settings.Value.PermissionLevel switch
		{
			OnlineSessionPermissionLevel.PublicAdvertised => 2, 
			OnlineSessionPermissionLevel.JoinViaPresence => 1, 
			_ => 0, 
		});
		gameServerInfo.SetValue(GameInfoInt.MaxPlayers, (int)_sessionDetailsInfo.Settings.Value.NumPublicConnections);
		gameServerInfo.SetValue(GameInfoInt.CurrentPlayers, (int)(_sessionDetailsInfo.Settings.Value.NumPublicConnections - _sessionDetailsInfo.NumOpenPublicConnections));
		gameServerInfo.SetValue(GameInfoString.IP, _sessionDetailsInfo.HostAddress);
		if (_sessionDetailsInfo.OwnerUserId != null)
		{
			gameServerInfo.SetValue(GameInfoString.CombinedPrimaryId, UserIdentifierEos.CreateCombinedString(_sessionDetailsInfo.OwnerUserId));
		}
		SessionDetailsGetSessionAttributeCountOptions options = default(SessionDetailsGetSessionAttributeCountOptions);
		uint sessionAttributeCount;
		lock (AntiCheatCommon.LockObject)
		{
			sessionAttributeCount = _sessionDetails.GetSessionAttributeCount(ref options);
		}
		for (uint num = 0u; num < sessionAttributeCount; num++)
		{
			SessionDetailsCopySessionAttributeByIndexOptions options2 = new SessionDetailsCopySessionAttributeByIndexOptions
			{
				AttrIndex = num
			};
			Result result;
			SessionDetailsAttribute? outSessionAttribute;
			lock (AntiCheatCommon.LockObject)
			{
				result = _sessionDetails.CopySessionAttributeByIndex(ref options2, out outSessionAttribute);
			}
			if (result != Result.Success)
			{
				Log.Error($"[EOS] Failed getting session attribute {num}: {result.ToStringCached()}");
				return null;
			}
			AttributeData value = outSessionAttribute.Value.Data.Value;
			string text = value.Key;
			switch (value.Value.ValueType)
			{
			case AttributeType.Boolean:
				if (!gameServerInfo.Parse(text, value.Value.AsBool == true))
				{
					return null;
				}
				break;
			case AttributeType.Int64:
				if (!gameServerInfo.Parse(text, (int)value.Value.AsInt64.GetValueOrDefault()))
				{
					return null;
				}
				break;
			case AttributeType.String:
				if (text.EqualsCaseInsensitive("-BoolValues-"))
				{
					string text2 = value.Value.AsUtf8;
					if (text2 == "##EMPTY##")
					{
						text2 = "";
					}
					string[] array = text2.Split(',');
					foreach (string text3 in array)
					{
						if (text3.Length <= 0)
						{
							continue;
						}
						int num2 = text3.IndexOf('=');
						if (num2 <= 0 || num2 >= text3.Length - 1)
						{
							Log.Warning("Session attribute " + text + " has invalid content for bool set: '" + text3 + "' (total: '" + text2 + "')");
						}
						else
						{
							string key = text3.Substring(0, num2);
							bool value2 = text3[num2 + 1] == '1';
							if (!gameServerInfo.Parse(key, value2))
							{
								return null;
							}
						}
					}
				}
				else
				{
					string text4 = value.Value.AsUtf8;
					if (text4 == "##EMPTY##")
					{
						text4 = "";
					}
					int num3 = text4.IndexOf("~$#$~", StringComparison.Ordinal);
					if (num3 >= 0)
					{
						text4 = text4.Substring(0, num3);
					}
					if (!gameServerInfo.Parse(text, text4))
					{
						return null;
					}
				}
				break;
			case AttributeType.Double:
				Log.Error($"Session attribute '{value.Key}' is of unsupported type double ({value.Value.AsDouble})");
				return null;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (ServerInfoCache.Instance.IsFavorite(gameServerInfo))
		{
			gameServerInfo.IsFavorite = true;
		}
		gameServerInfo.LastPlayedLinux = (int)ServerInfoCache.Instance.IsHistory(gameServerInfo);
		return gameServerInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool debugLogSessionInfo(uint _sessionIndex, SessionDetails _sessionDetails, SessionDetailsInfo _sessionDetailsInfo, bool _logAttributes = false)
	{
		SessionDetailsSettings value = _sessionDetailsInfo.Settings.Value;
		Log.Out($"Session {_sessionIndex}:");
		Log.Out("    Details:");
		Log.Out("        Settings:");
		Log.Out($"            BucketId: {value.BucketId}");
		Log.Out($"            InvitesAllowed: {value.InvitesAllowed}");
		Log.Out($"            PermissionLevel: {value.PermissionLevel}");
		Log.Out($"            SanctionsEnabled: {value.SanctionsEnabled}");
		Log.Out($"            NumPublicConnections: {value.NumPublicConnections}");
		Log.Out($"            AllowJoinInProgress: {value.AllowJoinInProgress}");
		Log.Out($"        Address: {_sessionDetailsInfo.HostAddress}");
		Log.Out($"        SessionId: {_sessionDetailsInfo.SessionId}");
		Log.Out($"        NumOpenPublicConnections: {_sessionDetailsInfo.NumOpenPublicConnections}");
		SessionDetailsGetSessionAttributeCountOptions options = default(SessionDetailsGetSessionAttributeCountOptions);
		uint sessionAttributeCount;
		lock (AntiCheatCommon.LockObject)
		{
			sessionAttributeCount = _sessionDetails.GetSessionAttributeCount(ref options);
		}
		Log.Out($"    Attributes: {sessionAttributeCount}");
		for (uint num = 0u; num < sessionAttributeCount; num++)
		{
			SessionDetailsCopySessionAttributeByIndexOptions options2 = new SessionDetailsCopySessionAttributeByIndexOptions
			{
				AttrIndex = num
			};
			Result result;
			SessionDetailsAttribute? outSessionAttribute;
			lock (AntiCheatCommon.LockObject)
			{
				result = _sessionDetails.CopySessionAttributeByIndex(ref options2, out outSessionAttribute);
			}
			if (result != Result.Success)
			{
				Log.Error($"[EOS] Failed getting session {_sessionIndex} attribute {num}: {result.ToStringCached()}");
				return false;
			}
			AttributeData value2 = outSessionAttribute.Value.Data.Value;
			if (!_logAttributes)
			{
				string a = value2.Key;
				if (!a.ContainsCaseInsensitive(GameInfoBool.EACEnabled.ToStringCached()) && !a.ContainsCaseInsensitive(GameInfoBool.SanctionsIgnored.ToStringCached()) && !a.ContainsCaseInsensitive(GameInfoBool.IsPasswordProtected.ToStringCached()) && !a.ContainsCaseInsensitive("-BoolValues-"))
				{
					continue;
				}
			}
			switch (value2.Value.ValueType)
			{
			case AttributeType.Boolean:
				Log.Out($"    Attr {value2.Key}: {value2.Value.AsBool}");
				break;
			case AttributeType.Int64:
				Log.Out($"    Attr {value2.Key}: {value2.Value.AsInt64}");
				break;
			case AttributeType.Double:
				Log.Out($"    Attr {value2.Key}: {value2.Value.AsDouble}");
				break;
			case AttributeType.String:
				Log.Out($"    Attr {value2.Key}: {value2.Value.AsUtf8}");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator getServerPingsCo()
	{
		pingCoroutineStarted = true;
		while (true)
		{
			if (pingRequestsInProgress < 8 && serverPingsToGet.Count > 0)
			{
				GameServerInfo gsi = serverPingsToGet.Dequeue();
				pingRequestsInProgress++;
				ServerInformationTcpClient.RequestRules(gsi, _ignoreTimeouts: true, serverPingCallback);
			}
			yield return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverPingCallback(bool _success, string _message, GameServerInfo _gsi)
	{
		pingRequestsInProgress--;
	}
}
