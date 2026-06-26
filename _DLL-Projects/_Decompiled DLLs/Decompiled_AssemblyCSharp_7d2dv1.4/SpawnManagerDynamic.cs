using System.IO;
using System.Xml;
using UnityEngine;

public class SpawnManagerDynamic : SpawnManagerAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte CurrentFileVersion = 1;

	public const int cMinRange = 64;

	public const int cMaxRange = 96;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySpawner currentSpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDaySpawned;

	public SpawnManagerDynamic(World _world, XmlDocument _spawnXml)
		: base(_world)
	{
		lastDaySpawned = -1;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)1);
		_bw.Write(currentSpawner != null);
		if (currentSpawner != null)
		{
			_bw.Write(lastDaySpawned);
			currentSpawner.Write(_bw);
		}
	}

	public void Read(BinaryReader _br)
	{
		_br.ReadByte();
		if (_br.ReadBoolean())
		{
			currentSpawner = new EntitySpawner();
			lastDaySpawned = _br.ReadInt32();
			currentSpawner.Read(_br);
		}
	}

	public override void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData)
	{
		if (world.IsDaytime() || world.Players.list.Count == 0)
		{
			return;
		}
		int num = GameUtils.WorldTimeToDays(world.worldTime);
		if (num != lastDaySpawned || currentSpawner == null)
		{
			lastDaySpawned = num;
			EntitySpawner entitySpawner = currentSpawner;
			Log.Out("New ES '" + _spawnerName + "' for day: " + num);
			currentSpawner = new EntitySpawner(_spawnerName, Vector3i.zero, Vector3i.zero, 0, entitySpawner?.GetEntityIdsSpaned());
		}
		if (currentSpawner != null)
		{
			currentSpawner.SpawnManually(world, num, _bSpawnEnemyEntities, [PublicizedFrom(EAccessModifier.Internal)] (EntitySpawner _es, out EntityPlayer _outPlayerToAttack) =>
			{
				_outPlayerToAttack = null;
				return true;
			}, [PublicizedFrom(EAccessModifier.Private)] (EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos) => world.GetRandomSpawnPositionMinMaxToRandomPlayer(64, 96, _bConsiderBedrolls: true, out _outPlayerToAttack, out _pos), null, null);
		}
	}
}
