using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.Net;

namespace Discord;

internal class RequestOptions
{
	public static RequestOptions Default => new RequestOptions();

	public int? Timeout { get; set; }

	public CancellationToken CancelToken { get; set; } = CancellationToken.None;

	public RetryMode? RetryMode { get; set; }

	public bool HeaderOnly { get; internal set; }

	public string AuditLogReason { get; set; }

	public bool? UseSystemClock { get; set; }

	public Func<IRateLimitInfo, Task> RatelimitCallback { get; set; }

	internal bool IgnoreState { get; set; }

	internal BucketId BucketId { get; set; }

	internal bool IsClientBucket { get; set; }

	internal bool IsReactionBucket { get; set; }

	internal bool IsGatewayBucket { get; set; }

	internal IDictionary<string, IEnumerable<string>> RequestHeaders { get; }

	internal static RequestOptions CreateOrClone(RequestOptions options)
	{
		if (options == null)
		{
			return new RequestOptions();
		}
		return options.Clone();
	}

	internal void ExecuteRatelimitCallback(IRateLimitInfo info)
	{
		if (RatelimitCallback != null)
		{
			Task.Run(async delegate
			{
				await RatelimitCallback(info);
			});
		}
	}

	public RequestOptions()
	{
		Timeout = 15000;
		RequestHeaders = new Dictionary<string, IEnumerable<string>>();
	}

	public RequestOptions Clone()
	{
		return MemberwiseClone() as RequestOptions;
	}
}
