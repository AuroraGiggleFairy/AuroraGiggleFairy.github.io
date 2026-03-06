using System;

namespace Discord.Utils;

internal static class UrlValidation
{
	public static bool Validate(string url, bool allowAttachments = false)
	{
		if (string.IsNullOrEmpty(url))
		{
			return false;
		}
		if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && (!allowAttachments || !url.StartsWith("attachment://", StringComparison.Ordinal)))
		{
			throw new InvalidOperationException("The url " + url + " must include a protocol (either " + (allowAttachments ? "HTTP, HTTPS, or ATTACHMENT" : "HTTP or HTTPS") + ")");
		}
		return true;
	}

	public static bool ValidateButton(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return false;
		}
		if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("discord://", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("The url " + url + " must include a protocol (either HTTP, HTTPS, or DISCORD)");
		}
		return true;
	}
}
