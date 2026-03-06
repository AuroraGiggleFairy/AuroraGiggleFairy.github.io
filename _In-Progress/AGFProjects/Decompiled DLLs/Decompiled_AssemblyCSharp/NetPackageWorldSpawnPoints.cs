using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldSpawnPoints : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnPointList spawnPoints;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWorldSpawnPoints Setup(SpawnPointList _spawnPoints)
	{
		spawnPoints = _spawnPoints;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		spawnPoints = new SpawnPointList();
		spawnPoints.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		spawnPoints.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.SetSpawnPointList(spawnPoints);
	}

	public override int GetLength()
	{
		if (spawnPoints == null)
		{
			return 0;
		}
		return spawnPoints.Count * 20;
	}
}
