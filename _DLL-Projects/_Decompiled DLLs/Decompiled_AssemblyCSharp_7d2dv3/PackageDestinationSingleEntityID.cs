public class PackageDestinationSingleEntityID : IPackageDestinationFilter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public PackageDestinationSingleEntityID(int _entityId)
	{
		entityId = _entityId;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		if (_cInfo.bAttachedToEntity)
		{
			return _cInfo.entityId != entityId;
		}
		return true;
	}
}
