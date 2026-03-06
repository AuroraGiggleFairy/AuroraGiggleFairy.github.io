using System.IO;

public class NetPackageLootRespawnTweak : NetPackage
{
	public int _lootRespawnDays = 0;

	public NetPackageLootRespawnTweak Setup(int lootRespawnDays)
	{
		_lootRespawnDays = lootRespawnDays;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		_lootRespawnDays = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		((BinaryWriter)_bw).Write(_lootRespawnDays);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GamePrefs.Set(EnumGamePrefs.LootRespawnDays, _lootRespawnDays);
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
