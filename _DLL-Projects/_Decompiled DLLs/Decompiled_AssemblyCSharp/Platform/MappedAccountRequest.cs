namespace Platform;

public class MappedAccountRequest
{
	public readonly PlatformUserIdentifierAbs Id;

	public readonly EPlatformIdentifier Platform;

	public string MappedAccountId;

	public string DisplayName;

	public MappedAccountQueryResult Result;

	public MappedAccountRequest(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform)
	{
		Id = _id;
		Platform = _platform;
	}
}
