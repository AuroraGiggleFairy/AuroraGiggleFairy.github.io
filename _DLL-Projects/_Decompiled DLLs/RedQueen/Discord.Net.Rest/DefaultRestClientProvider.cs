using System;

namespace Discord.Net.Rest;

internal static class DefaultRestClientProvider
{
	public static readonly RestClientProvider Instance = Create();

	public static RestClientProvider Create(bool useProxy = false)
	{
		return delegate(string url)
		{
			try
			{
				return new DefaultRestClient(url, useProxy);
			}
			catch (PlatformNotSupportedException inner)
			{
				throw new PlatformNotSupportedException("The default RestClientProvider is not supported on this platform.", inner);
			}
		};
	}
}
