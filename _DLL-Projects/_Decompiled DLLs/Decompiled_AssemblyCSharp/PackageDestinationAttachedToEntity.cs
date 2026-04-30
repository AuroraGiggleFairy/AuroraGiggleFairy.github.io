public class PackageDestinationAttachedToEntity : IPackageDestinationFilter
{
	public bool Exclude(ClientInfo _cInfo)
	{
		return !_cInfo.bAttachedToEntity;
	}
}
