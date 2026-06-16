public abstract class DynamicMeshServerData : NetPackage
{
	public int X;

	public int Z;

	public abstract bool Prechecks();

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicMeshServerData()
	{
	}
}
