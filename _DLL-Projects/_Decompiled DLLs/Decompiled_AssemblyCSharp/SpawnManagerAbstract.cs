public abstract class SpawnManagerAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	public SpawnManagerAbstract(World _world)
	{
		world = _world;
	}

	public abstract void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData);
}
