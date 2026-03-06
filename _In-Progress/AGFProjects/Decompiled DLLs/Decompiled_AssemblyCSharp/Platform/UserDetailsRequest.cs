namespace Platform;

public class UserDetailsRequest
{
	public readonly PlatformUserIdentifierAbs Id;

	public readonly EPlatformIdentifier NativePlatform;

	public PlatformUserDetails details;

	public bool IsSuccess;

	public UserDetailsRequest(PlatformUserIdentifierAbs id)
	{
		Id = id;
		NativePlatform = id.PlatformIdentifier;
	}

	public UserDetailsRequest(PlatformUserIdentifierAbs id, EPlatformIdentifier platform)
	{
		Id = id;
		NativePlatform = platform;
	}
}
