using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord.Net.Rest;

internal sealed class DefaultRestClient : IRestClient, IDisposable
{
	private const int HR_SECURECHANNELFAILED = -2146233079;

	private readonly HttpClient _client;

	private readonly string _baseUrl;

	private readonly JsonSerializer _errorDeserializer;

	private CancellationToken _cancelToken;

	private bool _isDisposed;

	private static readonly HttpMethod Patch = new HttpMethod("PATCH");

	public DefaultRestClient(string baseUrl, bool useProxy = false)
	{
		_baseUrl = baseUrl;
		_client = new HttpClient(new HttpClientHandler
		{
			AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate),
			UseCookies = false,
			UseProxy = useProxy
		});
		SetHeader("accept-encoding", "gzip, deflate");
		_cancelToken = CancellationToken.None;
		_errorDeserializer = new JsonSerializer();
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_client.Dispose();
			}
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void SetHeader(string key, string value)
	{
		_client.DefaultRequestHeaders.Remove(key);
		if (value != null)
		{
			_client.DefaultRequestHeaders.Add(key, value);
		}
	}

	public void SetCancelToken(CancellationToken cancelToken)
	{
		_cancelToken = cancelToken;
	}

	public async Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null)
	{
		string requestUri = Path.Combine(_baseUrl, endpoint);
		using HttpRequestMessage restRequest = new HttpRequestMessage(GetMethod(method), requestUri);
		if (reason != null)
		{
			restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
		}
		if (requestHeaders != null)
		{
			foreach (KeyValuePair<string, IEnumerable<string>> requestHeader in requestHeaders)
			{
				restRequest.Headers.Add(requestHeader.Key, requestHeader.Value);
			}
		}
		return await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null)
	{
		string requestUri = Path.Combine(_baseUrl, endpoint);
		using HttpRequestMessage restRequest = new HttpRequestMessage(GetMethod(method), requestUri);
		if (reason != null)
		{
			restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
		}
		if (requestHeaders != null)
		{
			foreach (KeyValuePair<string, IEnumerable<string>> requestHeader in requestHeaders)
			{
				restRequest.Headers.Add(requestHeader.Key, requestHeader.Value);
			}
		}
		restRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
		return await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken, bool headerOnly, string reason = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = null)
	{
		string requestUri = Path.Combine(_baseUrl, endpoint);
		using HttpRequestMessage restRequest = new HttpRequestMessage(GetMethod(method), requestUri);
		if (reason != null)
		{
			restRequest.Headers.Add("X-Audit-Log-Reason", Uri.EscapeDataString(reason));
		}
		if (requestHeaders != null)
		{
			foreach (KeyValuePair<string, IEnumerable<string>> requestHeader in requestHeaders)
			{
				restRequest.Headers.Add(requestHeader.Key, requestHeader.Value);
			}
		}
		MultipartFormDataContent content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
		MemoryStream memoryStream = null;
		if (multipartParams != null)
		{
			foreach (KeyValuePair<string, object> p in multipartParams)
			{
				object value = p.Value;
				if (!(value is string content2))
				{
					if (!(value is byte[] content3))
					{
						if (!(value is Stream content4))
						{
							if (!(value is MultipartFile { Stream: var stream } fileValue))
							{
								throw new InvalidOperationException("Unsupported param type \"" + p.Value.GetType().Name + "\".");
							}
							if (!stream.CanSeek)
							{
								memoryStream = new MemoryStream();
								await stream.CopyToAsync(memoryStream).ConfigureAwait(continueOnCapturedContext: false);
								memoryStream.Position = 0L;
								stream = memoryStream;
							}
							StreamContent streamContent = new StreamContent(stream);
							fileValue.Filename.Split('.').Last();
							if (fileValue.ContentType != null)
							{
								streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileValue.ContentType);
							}
							content.Add(streamContent, p.Key, fileValue.Filename);
						}
						else
						{
							content.Add(new StreamContent(content4), p.Key);
						}
					}
					else
					{
						content.Add(new ByteArrayContent(content3), p.Key);
					}
				}
				else
				{
					content.Add(new StringContent(content2, Encoding.UTF8, "text/plain"), p.Key);
				}
			}
		}
		restRequest.Content = content;
		RestResponse result = await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(continueOnCapturedContext: false);
		memoryStream?.Dispose();
		return result;
	}

	private async Task<RestResponse> SendInternalAsync(HttpRequestMessage request, CancellationToken cancelToken, bool headerOnly)
	{
		using CancellationTokenSource cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken, cancelToken);
		cancelToken = cancelTokenSource.Token;
		HttpResponseMessage response = await _client.SendAsync(request, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		Dictionary<string, string> headers = response.Headers.ToDictionary((KeyValuePair<string, IEnumerable<string>> x) => x.Key, (KeyValuePair<string, IEnumerable<string>> x) => x.Value.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
		Stream stream = ((headerOnly && response.IsSuccessStatusCode) ? null : (await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false)));
		Stream stream2 = stream;
		return new RestResponse(response.StatusCode, headers, stream2);
	}

	private HttpMethod GetMethod(string method)
	{
		return method switch
		{
			"DELETE" => HttpMethod.Delete, 
			"GET" => HttpMethod.Get, 
			"PATCH" => Patch, 
			"POST" => HttpMethod.Post, 
			"PUT" => HttpMethod.Put, 
			_ => throw new ArgumentOutOfRangeException("method", "Unknown HttpMethod: " + method), 
		};
	}
}
