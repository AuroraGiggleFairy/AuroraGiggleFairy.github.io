namespace Platform;

public class MappedAccountReverseRequest
{
	public readonly EPlatformIdentifier Platform;

	public readonly string Id;

	public MappedAccountQueryResult Result;

	public PlatformUserIdentifierAbs PlatformId;

	public MappedAccountReverseRequest(EPlatformIdentifier _platform, string _id)
	{
		Platform = _platform;
		Id = _id;
	}
}
