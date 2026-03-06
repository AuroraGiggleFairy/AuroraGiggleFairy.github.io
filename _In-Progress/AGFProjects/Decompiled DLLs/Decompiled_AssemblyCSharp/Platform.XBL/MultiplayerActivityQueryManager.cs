using System;
using System.Collections.Generic;
using System.Threading;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public class MultiplayerActivityQueryManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Request
	{
		public int id;

		public ulong[] xuids;

		public OnGetActivityComplete callback;

		public List<XblMultiplayerActivityInfo> results = new List<XblMultiplayerActivityInfo>();

		public int batchesPending;

		public Request(ulong[] xuids, int batchCount, OnGetActivityComplete callback)
		{
			id = Interlocked.Increment(ref nextRequestId);
			this.xuids = xuids;
			batchesPending = batchCount;
			this.callback = callback;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RequestBatch(ulong[] xuidBatch, Request request)
	{
		public ulong[] xuids = xuidBatch;

		public Request request = request;
	}

	public delegate void OnGetActivityComplete(ulong[] requestedXuids, List<XblMultiplayerActivityInfo> results);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int batchMax = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int backoffSeconds = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int burstLimit = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextRequestId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Unity.XGamingRuntime.XblContextHandle xblContextHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public object pendingLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, Request> pendingRequests = new Dictionary<int, Request>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object retryLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<RequestBatch> retryQueue = new Queue<RequestBatch>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.TaskInfo retryTask;

	public MultiplayerActivityQueryManager(Unity.XGamingRuntime.XblContextHandle xblContextHandle)
	{
		this.xblContextHandle = xblContextHandle;
	}

	public void GetActivityAsync(ulong[] xuids, OnGetActivityComplete callback)
	{
		int num = (xuids.Length + 30 - 1) / 30;
		Request request = new Request(xuids, num, callback);
		lock (pendingLock)
		{
			if (!pendingRequests.TryAdd(request.id, request))
			{
				Log.Error("[XBL] could not start GetActivityAsync as request could not be enqueued");
				return;
			}
		}
		if (num == 1)
		{
			StartBatch(request, xuids);
			return;
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 30;
			int num3 = Math.Min(xuids.Length - num2, 30);
			ulong[] array = new ulong[num3];
			Array.Copy(xuids, num2, array, 0, num3);
			StartBatch(request, array);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartBatch(Request request, ulong[] xuids)
	{
		SDK.XBL.XblMultiplayerActivityGetActivityAsync(xblContextHandle, xuids, [PublicizedFrom(EAccessModifier.Internal)] (int hresult, XblMultiplayerActivityInfo[] results) =>
		{
			if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hresult))
			{
				if (hresult == -2145844819)
				{
					lock (retryLock)
					{
						retryQueue.Enqueue(new RequestBatch(xuids, request));
						if (retryTask == null)
						{
							retryTask = ThreadManager.AddSingleTask(RetryBatchesTask, null, RetryExitHandler);
						}
						return;
					}
				}
				XblHelpers.LogHR(hresult, "XblMultiplayerActivityGetActivityAsync");
				CompleteBatch(request, null);
			}
			else
			{
				CompleteBatch(request, results);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteBatch(Request request, XblMultiplayerActivityInfo[] batchResults)
	{
		if (batchResults != null && batchResults.Length != 0)
		{
			lock (request.results)
			{
				request.results.AddRange(batchResults);
			}
		}
		if (Interlocked.Decrement(ref request.batchesPending) == 0)
		{
			request.callback(request.xuids, request.results);
			lock (pendingLock)
			{
				pendingRequests.Remove(request.id, out var _);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RetryBatchesTask(ThreadManager.TaskInfo taskInfo)
	{
		while (true)
		{
			Log.Warning($"[XBL] too many multiplayer activity requests, will try again in {20}s");
			Thread.Sleep(20000);
			lock (retryLock)
			{
				int num = 0;
				RequestBatch result;
				while (retryQueue.TryDequeue(out result) && num < 20)
				{
					StartBatch(result.request, result.xuids);
					num++;
				}
				if (retryQueue.Count == 0)
				{
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RetryExitHandler(ThreadManager.TaskInfo _ti, Exception _e)
	{
		lock (retryLock)
		{
			retryTask = null;
		}
	}
}
