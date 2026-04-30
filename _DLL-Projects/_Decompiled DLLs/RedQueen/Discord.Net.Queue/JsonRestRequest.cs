using System.Threading.Tasks;
using Discord.Net.Rest;

namespace Discord.Net.Queue;

internal class JsonRestRequest : RestRequest
{
	public string Json { get; }

	public JsonRestRequest(IRestClient client, string method, string endpoint, string json, RequestOptions options)
		: base(client, method, endpoint, options)
	{
		Json = json;
	}

	public override async Task<RestResponse> SendAsync()
	{
		return await base.Client.SendAsync(base.Method, base.Endpoint, Json, base.Options.CancelToken, base.Options.HeaderOnly, base.Options.AuditLogReason).ConfigureAwait(continueOnCapturedContext: false);
	}
}
