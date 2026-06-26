public class PackageDestinationAllButEntityID : IPackageDestinationFilter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public PackageDestinationAllButEntityID(int _excludedEntityId)
	{
		entityId = _excludedEntityId;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		if (_cInfo.bAttachedToEntity)
		{
			return _cInfo.entityId == entityId;
		}
		return true;
	}
}
