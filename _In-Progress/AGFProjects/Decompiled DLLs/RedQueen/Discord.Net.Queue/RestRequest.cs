using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Net.Rest;

namespace Discord.Net.Queue;

internal class RestRequest : IRequest
{
	public IRestClient Client { get; }

	public string Method { get; }

	public string Endpoint { get; }

	public DateTimeOffset? TimeoutAt { get; }

	public TaskCompletionSource<Stream> Promise { get; }

	public RequestOptions Options { get; }

	public RestRequest(IRestClient client, string method, string endpoint, RequestOptions options)
	{
		Preconditions.NotNull(options, "options");
		Client = client;
		Method = method;
		Endpoint = endpoint;
		Options = options;
		TimeoutAt = (options.Timeout.HasValue ? new DateTimeOffset?(DateTimeOffset.UtcNow.AddMilliseconds(options.Timeout.Value)) : ((DateTimeOffset?)null));
		Promise = new TaskCompletionSource<Stream>();
	}

	public virtual async Task<RestResponse> SendAsync()
	{
		return await Client.SendAsync(Method, Endpoint, Options.CancelToken, Options.HeaderOnly, Options.AuditLogReason, Options.RequestHeaders).ConfigureAwait(continueOnCapturedContext: false);
	}
}
