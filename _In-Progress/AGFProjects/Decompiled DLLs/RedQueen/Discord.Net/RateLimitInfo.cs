using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Discord.API;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.Net;

internal struct RateLimitInfo : IRateLimitInfo
{
	public bool IsGlobal
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public int? Limit
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public int? Remaining
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public int? RetryAfter
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public DateTimeOffset? Reset
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public TimeSpan? ResetAfter
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
		private set; }

	public string Bucket
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public TimeSpan? Lag
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public string Endpoint
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	internal RateLimitInfo(Dictionary<string, string> headers, string endpoint)
	{
		Endpoint = endpoint;
		bool result = default(bool);
		IsGlobal = headers.TryGetValue("X-RateLimit-Global", out var value) && bool.TryParse(value, out result) && result;
		Limit = ((headers.TryGetValue("X-RateLimit-Limit", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result2)) ? new int?(result2) : ((int?)null));
		Remaining = ((headers.TryGetValue("X-RateLimit-Remaining", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result3)) ? new int?(result3) : ((int?)null));
		Reset = ((headers.TryGetValue("X-RateLimit-Reset", out value) && double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result4)) ? new DateTimeOffset?(DateTimeOffset.FromUnixTimeMilliseconds((long)(result4 * 1000.0))) : ((DateTimeOffset?)null));
		RetryAfter = ((headers.TryGetValue("Retry-After", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result5)) ? new int?(result5) : ((int?)null));
		ResetAfter = ((headers.TryGetValue("X-RateLimit-Reset-After", out value) && double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result6)) ? new TimeSpan?(TimeSpan.FromSeconds(result6)) : ((TimeSpan?)null));
		Bucket = (headers.TryGetValue("X-RateLimit-Bucket", out value) ? value : null);
		Lag = ((headers.TryGetValue("Date", out value) && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result7)) ? new TimeSpan?(DateTimeOffset.UtcNow - result7) : ((TimeSpan?)null));
	}

	internal Ratelimit ReadRatelimitPayload(Stream response)
	{
		if (response != null && response.Length != 0L)
		{
			using (TextReader reader = new StreamReader(response))
			{
				using JsonReader reader2 = new JsonTextReader(reader);
				return DiscordRestClient.Serializer.Deserialize<Ratelimit>(reader2);
			}
		}
		return null;
	}
}
