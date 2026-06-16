namespace Platform;

public static class IPlatformUserBlockedDataExtensions
{
	public static bool IsBlocked(this IPlatformUserBlockedData blockedData)
	{
		return blockedData.State.IsBlocked();
	}
}
