namespace Platform.XBL;

public class XuidResolveRequest
{
	public readonly PlatformUserIdentifierAbs Id;

	public bool IsSuccess;

	public ulong Xuid;

	public XuidResolveRequest(PlatformUserIdentifierAbs id)
	{
		Id = id;
	}
}
