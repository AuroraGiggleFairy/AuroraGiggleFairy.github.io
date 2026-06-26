using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorAirDropComponent : AIDirectorComponent
{
	[Preserve]
	public class SupplyCrateCache
	{
		public int entityId;

		public Vector3i blockPos;

		public SupplyCrateCache(int id, Vector3i blockPos)
		{
			entityId = id;
			this.blockPos = blockPos;
		}
	}

	public List<SupplyCrateCache> supplyCrates = new List<SupplyCrateCache>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AIAirDrop activeAirDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAirDropTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastAirdropCheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastFrequency;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] crateTypes = new string[1] { "sc_General" };

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong calcNextAirdrop(ulong currentTime, ulong dropFrequency)
	{
		ulong num = currentTime / 1000;
		if (dropFrequency % 24 == 0L)
		{
			return (dropFrequency + num) / 24 * 24000 + 12000;
		}
		return (dropFrequency + num) * 1000;
	}

	public override void Connect()
	{
		base.Connect();
	}

	public override void InitNewGame()
	{
		base.InitNewGame();
		activeAirDrop = null;
		lastAirdropCheckTime = Director.World.worldTime;
		nextAirDropTime = calcNextAirdrop(lastAirdropCheckTime, (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency));
		supplyCrates.Clear();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		if (activeAirDrop != null)
		{
			if (activeAirDrop.Tick((float)_dt))
			{
				activeAirDrop = null;
			}
			return;
		}
		ulong num = (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency);
		ulong worldTime = Director.World.worldTime;
		if (GameUtils.IsPlaytesting() || num == 0)
		{
			return;
		}
		if (worldTime >= nextAirDropTime)
		{
			if (SpawnAirDrop())
			{
				lastAirdropCheckTime = worldTime;
				nextAirDropTime = calcNextAirdrop(worldTime, num);
			}
		}
		else if (num != lastFrequency || worldTime < lastAirdropCheckTime)
		{
			nextAirDropTime = calcNextAirdrop(worldTime, num);
			lastFrequency = num;
			lastAirdropCheckTime = worldTime;
		}
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		nextAirDropTime = _stream.ReadUInt64();
		if (_version >= 9)
		{
			lastFrequency = _stream.ReadUInt64();
		}
		else
		{
			lastFrequency = (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency);
		}
		supplyCrates.Clear();
		int num = _stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = _stream.ReadInt32();
			if (num2 > lastID)
			{
				lastID = num2;
			}
			Vector3i blockPos = StreamUtils.ReadVector3i(_stream);
			supplyCrates.Add(new SupplyCrateCache(num2, blockPos));
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(nextAirDropTime);
		_stream.Write(lastFrequency);
		_stream.Write(supplyCrates.Count);
		for (int i = 0; i < supplyCrates.Count; i++)
		{
			_stream.Write(supplyCrates[i].entityId);
			StreamUtils.Write(_stream, supplyCrates[i].blockPos);
		}
	}

	public bool SpawnAirDrop()
	{
		bool result = false;
		if (activeAirDrop == null)
		{
			List<EntityPlayer> list = new List<EntityPlayer>();
			DictionaryList<int, AIDirectorPlayerState> trackedPlayers = Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers;
			for (int i = 0; i < trackedPlayers.list.Count; i++)
			{
				AIDirectorPlayerState aIDirectorPlayerState = trackedPlayers.list[i];
				if (!aIDirectorPlayerState.Player.IsDead())
				{
					list.Add(aIDirectorPlayerState.Player);
				}
			}
			if (list.Count > 0)
			{
				activeAirDrop = new AIAirDrop(this, Director.World, list);
				result = true;
			}
		}
		return result;
	}

	public void RemoveSupplyCrate(int entityId)
	{
		int num = -1;
		for (int i = 0; i < supplyCrates.Count; i++)
		{
			if (supplyCrates[i].entityId == entityId)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			supplyCrates.RemoveAt(num);
			return;
		}
		Log.Error("AIDirectorAirDropComponent: Attempted to remove supply crate cache with missing entityID {0}", entityId);
	}

	public void SetSupplyCratePosition(int entityId, Vector3i blockPos)
	{
		foreach (SupplyCrateCache supplyCrate in supplyCrates)
		{
			if (supplyCrate.entityId == entityId)
			{
				supplyCrate.blockPos = blockPos;
				break;
			}
		}
	}

	public EntitySupplyCrate SpawnSupplyCrate(Vector3 spawnPos)
	{
		if (Director.World == null)
		{
			return null;
		}
		if (supplyCrates.Count >= 12)
		{
			Entity entity = Director.World.GetEntity(supplyCrates[0].entityId);
			if (entity != null)
			{
				entity.MarkToUnload();
			}
			supplyCrates.RemoveAt(0);
		}
		Entity entity2 = EntityFactory.CreateEntity(EntityClass.FromString(crateTypes[base.Random.RandomRange(0, crateTypes.Length)]), spawnPos, new Vector3(base.Random.RandomFloat * 360f, 0f, 0f));
		Director.World.SpawnEntityInWorld(entity2);
		supplyCrates.Add(new SupplyCrateCache(entity2.entityId, World.worldToBlockPos(entity2.position)));
		return entity2 as EntitySupplyCrate;
	}
}
