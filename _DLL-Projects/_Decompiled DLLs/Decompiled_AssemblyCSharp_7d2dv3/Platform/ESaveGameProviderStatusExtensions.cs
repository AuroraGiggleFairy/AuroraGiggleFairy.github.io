using System;

namespace Platform;

public static class ESaveGameProviderStatusExtensions
{
	public static bool IsTerminal(this ESaveGameProviderStatus status)
	{
		switch (status)
		{
		case ESaveGameProviderStatus.Uninitialized:
		case ESaveGameProviderStatus.TemporaryError:
			return false;
		case ESaveGameProviderStatus.Ok:
		case ESaveGameProviderStatus.PermanentError:
			return true;
		default:
			throw new ArgumentOutOfRangeException("status", status, null);
		}
	}
}
