using System;
using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Sanctions;
using JetBrains.Annotations;
using Pathfinding.Util;

namespace Platform.EOS;

public class SanctionsCheck
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ProductUserId productUserId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SanctionsInterface sanctionsInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool queryInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EOSSanction> CurrentCheckSanctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WaitUntilSanctionsCheck(CoroutineCancellationToken _cancellationToken = null)
	{
		while (queryInProgress && !(_cancellationToken?.IsCancelled() ?? false))
		{
			yield return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public IEnumerator CheckSanctionsEnumerator(SanctionsInterface _sanctionsInterface, ProductUserId _productUserId, [CanBeNull] ProductUserId _localUser, Action<SanctionsCheckResult> callback, CoroutineCancellationToken _cancellationToken = null)
	{
		if (queryInProgress)
		{
			yield return WaitUntilSanctionsCheck(_cancellationToken);
		}
		if (_cancellationToken?.IsCancelled() ?? false)
		{
			yield break;
		}
		queryInProgress = true;
		sanctionsInterface = _sanctionsInterface;
		productUserId = _productUserId;
		QueryActivePlayerSanctionsOptions options = new QueryActivePlayerSanctionsOptions
		{
			TargetUserId = productUserId,
			LocalUserId = _localUser
		};
		lock (AntiCheatCommon.LockObject)
		{
			sanctionsInterface.QueryActivePlayerSanctions(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref QueryActivePlayerSanctionsCallbackInfo data) =>
			{
				CoroutineCancellationToken coroutineCancellationToken = _cancellationToken;
				if (coroutineCancellationToken == null || !coroutineCancellationToken.IsCancelled())
				{
					OnSanctionsQueryResolveAndGatherSanctions(ref data, callback);
				}
			});
		}
		yield return WaitUntilSanctionsCheck(_cancellationToken);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void CheckSanctions(SanctionsInterface _sanctionsInterface, ProductUserId _productUserId, [CanBeNull] ProductUserId _localUser, Action<SanctionsCheckResult> callback)
	{
		if (queryInProgress)
		{
			ThreadManager.StartCoroutine(CheckSanctionsEnumerator(_sanctionsInterface, _productUserId, _localUser, callback));
			return;
		}
		queryInProgress = true;
		sanctionsInterface = _sanctionsInterface;
		productUserId = _productUserId;
		QueryActivePlayerSanctionsOptions options = new QueryActivePlayerSanctionsOptions
		{
			TargetUserId = productUserId,
			LocalUserId = _localUser
		};
		lock (AntiCheatCommon.LockObject)
		{
			sanctionsInterface.QueryActivePlayerSanctions(ref options, null, [PublicizedFrom(EAccessModifier.Internal)] (ref QueryActivePlayerSanctionsCallbackInfo data) =>
			{
				OnSanctionsQueryResolveAndGatherSanctions(ref data, callback);
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSanctionsQueryResolveAndGatherSanctions(ref QueryActivePlayerSanctionsCallbackInfo data, Action<SanctionsCheckResult> callback)
	{
		if (data.ResultCode == Result.NotFound)
		{
			Log.Out("[EOS] Player has no sanctions");
			queryInProgress = false;
			CurrentCheckSanctions?.ClearFast();
			callback?.Invoke(new SanctionsCheckResult(null));
			return;
		}
		if (data.ResultCode != Result.Success)
		{
			Log.Out($"[EOS] Sanctions query failed {data.ResultCode}");
			if (data.ResultCode == Result.OperationWillRetry)
			{
				return;
			}
			queryInProgress = false;
			CurrentCheckSanctions?.ClearFast();
			callback?.Invoke(new SanctionsCheckResult(default(DateTime), GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 5, data.ResultCode.ToStringCached()));
		}
		else
		{
			Log.Out("[EOS] Player may have active sanctions");
			GetPlayerSanctionCountOptions options = new GetPlayerSanctionCountOptions
			{
				TargetUserId = productUserId
			};
			uint playerSanctionCount;
			lock (AntiCheatCommon.LockObject)
			{
				playerSanctionCount = sanctionsInterface.GetPlayerSanctionCount(ref options);
			}
			CurrentCheckSanctions = new List<EOSSanction>();
			for (uint num = 0u; num < playerSanctionCount; num++)
			{
				CopyPlayerSanctionByIndexOptions options2 = new CopyPlayerSanctionByIndexOptions
				{
					SanctionIndex = num,
					TargetUserId = productUserId
				};
				lock (AntiCheatCommon.LockObject)
				{
					if (sanctionsInterface.CopyPlayerSanctionByIndex(ref options2, out var outSanction) != Result.Success)
					{
						continue;
					}
					Log.Error("[EOS] Sanction found: " + (outSanction?.Action ?? ((Utf8String)"Empty Action")).ToString());
					if (outSanction.HasValue && outSanction.GetValueOrDefault().Action.ToString().Equals("RESTRICT_MATCHMAKING"))
					{
						Log.Error("[EOS] Sanction in place");
						if (outSanction.Value.TimeExpires == 0L)
						{
							Log.Out("[EOS] Sanctioned Until: Forever");
							CurrentCheckSanctions.Add(new EOSSanction(DateTime.MaxValue, outSanction.Value.ReferenceId));
							break;
						}
						DateTime value = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(outSanction.Value.TimeExpires).ToLocalTime();
						Log.Out("[EOS] Sanctioned Until: " + value.ToLongDateString());
						CurrentCheckSanctions.Add(new EOSSanction(value, outSanction.Value.ReferenceId));
					}
				}
			}
		}
		Log.Out($"[EOS] SanctionsQuery finished with: {CurrentCheckSanctions?.Count ?? 0} sanctions");
		queryInProgress = false;
		callback?.Invoke(new SanctionsCheckResult(CurrentCheckSanctions));
	}
}
