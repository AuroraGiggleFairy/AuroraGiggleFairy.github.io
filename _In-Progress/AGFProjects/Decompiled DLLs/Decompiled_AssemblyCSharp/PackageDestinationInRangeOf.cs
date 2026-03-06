public class PackageDestinationInRangeOf : IPackageDestinationFilter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int range;

	public PackageDestinationInRangeOf(int _entityId, int _range)
	{
		entityId = _entityId;
		range = _range;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		return !GameManager.Instance.World.IsEntityInRange(entityId, _cInfo.entityId, range);
	}
}
