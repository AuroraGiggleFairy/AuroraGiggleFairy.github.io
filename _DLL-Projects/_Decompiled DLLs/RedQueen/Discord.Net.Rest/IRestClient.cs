using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Net.Rest;

internal interface IRestClient : IDisposable
{
	void SetHeader(string key, string value);

	void SetCancelToken(CancellationToken cancelToken);

	Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly = false, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null);

	Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly = false, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null);

	Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken, bool headerOnly = false, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null);
}
