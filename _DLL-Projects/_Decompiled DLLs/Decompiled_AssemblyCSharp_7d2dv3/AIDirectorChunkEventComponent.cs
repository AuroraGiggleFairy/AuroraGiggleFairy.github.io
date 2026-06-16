using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkEventComponent : AIDirectorHordeComponent
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class Horde : AIScoutHordeSpawner.IHorde
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public AIDirectorChunkEventComponent _outer;

		[PublicizedFrom(EAccessModifier.Private)]
		public AIHordeSpawner _horde;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 _pos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int _numSpawned;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool _destroy;

		public bool canSpawnMore => _numSpawned < 25;

		public bool isSpawning
		{
			get
			{
				if (_horde != null)
				{
					return _horde.isSpawning;
				}
				return false;
			}
		}

		public Horde(AIDirectorChunkEventComponent outer, Vector3 pos)
		{
			_outer = outer;
			_pos = pos;
		}

		public void SpawnMore(int size)
		{
			int num = _numSpawned + size;
			int num2 = num - _numSpawned;
			_numSpawned = num;
			if (_horde != null)
			{
				_horde.numToSpawn += num2;
				return;
			}
			_horde = new AIHordeSpawner(_outer.Director.World, "ScoutGSList", _pos, 30f);
			_horde.numToSpawn = num2;
		}

		public void SetSpawnPos(Vector3 pos)
		{
			if (_horde != null)
			{
				_horde.targetPos = pos;
			}
			_pos = pos;
		}

		public void Destroy()
		{
			if (_horde != null)
			{
				_horde.Cleanup();
			}
			_horde = null;
			_destroy = true;
		}

		public bool Tick(double dt)
		{
			if (_destroy)
			{
				return true;
			}
			if (_horde != null && _horde.Tick(dt))
			{
				_horde.Cleanup();
				_horde = null;
			}
			return false;
		}
	}

	public const int cVersion = 1;

	public const int cChunksPerArea = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cEventDelay = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityLevelToSpawn = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnChance = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, AIDirectorChunkData> activeChunks = new Dictionary<long, AIDirectorChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> removeChunks = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIScoutHordeSpawner> scoutSpawnList = new List<AIScoutHordeSpawner>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Horde> hordeSpawnList = new List<Horde>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorChunkData> checkChunks = new List<AIDirectorChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] neighbors = new int[16]
	{
		-1, 0, 1, 0, 0, -1, 0, 1, -1, -1,
		1, -1, -1, 1, 1, 1
	};

	public bool HasAnySpawns => hordeSpawnList.Count != 0;

	public void Clear()
	{
		activeChunks.Clear();
		checkChunks.Clear();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		float num = (float)_dt;
		spawnDelay -= num;
		if (spawnDelay <= 0f)
		{
			spawnDelay = 5f;
			CheckToSpawn();
			foreach (KeyValuePair<long, AIDirectorChunkData> activeChunk in activeChunks)
			{
				if (!activeChunk.Value.Tick(5f))
				{
					removeChunks.Add(activeChunk.Key);
				}
			}
			if (removeChunks.Count > 0)
			{
				for (int i = 0; i < removeChunks.Count; i++)
				{
					activeChunks.Remove(removeChunks[i]);
				}
				removeChunks.Clear();
			}
		}
		TickActiveSpawns(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickActiveSpawns(float dt)
	{
		for (int num = scoutSpawnList.Count - 1; num >= 0; num--)
		{
			if (scoutSpawnList[num].Update(Director.World, dt))
			{
				AIDirector.LogAIExtra("Scout horde spawn finished (all mobs spawned)");
				scoutSpawnList[num].Cleanup();
				scoutSpawnList.RemoveAt(num);
			}
		}
		for (int num2 = hordeSpawnList.Count - 1; num2 >= 0; num2--)
		{
			if (hordeSpawnList[num2].Tick(dt))
			{
				AIDirector.LogAIExtra("Scout triggered horde finished (all mobs spawned)");
				hordeSpawnList.RemoveAt(num2);
			}
		}
	}

	public override void Read(BinaryReader _stream, int _outerVersion)
	{
		if (_outerVersion >= 5)
		{
			activeChunks.Clear();
			int outerVersion = _stream.ReadInt32();
			int num = _stream.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				long key = _stream.ReadInt64();
				AIDirectorChunkData aIDirectorChunkData = new AIDirectorChunkData();
				aIDirectorChunkData.Read(_stream, outerVersion);
				activeChunks[key] = aIDirectorChunkData;
			}
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		_stream.Write(1);
		_stream.Write(activeChunks.Count);
		foreach (KeyValuePair<long, AIDirectorChunkData> activeChunk in activeChunks)
		{
			_stream.Write(activeChunk.Key);
			activeChunk.Value.Write(_stream);
		}
	}

	public int GetActiveCount()
	{
		return activeChunks.Count;
	}

	public AIDirectorChunkData GetChunkDataFromPosition(Vector3i _position, bool _createIfNeeded)
	{
		int x = World.toChunkXZ(_position.x) / 5;
		int y = World.toChunkXZ(_position.z) / 5;
		long key = WorldChunkCache.MakeChunkKey(x, y);
		if (activeChunks.TryGetValue(key, out var value))
		{
			return value;
		}
		if (_createIfNeeded)
		{
			value = new AIDirectorChunkData();
			activeChunks[key] = value;
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartCooldownOnNeighbors(Vector3i _position, bool _isLong)
	{
		int num = World.toChunkXZ(_position.x) / 5;
		int num2 = World.toChunkXZ(_position.z) / 5;
		for (int i = 0; i < neighbors.Length; i += 2)
		{
			long key = WorldChunkCache.MakeChunkKey(num + neighbors[i], num2 + neighbors[i + 1]);
			if (!activeChunks.TryGetValue(key, out var value))
			{
				value = new AIDirectorChunkData();
				activeChunks[key] = value;
			}
			value.StartNeighborCooldown(_isLong);
		}
	}

	public void NotifyEvent(AIDirectorChunkEvent _chunkEvent)
	{
		AIDirectorChunkData chunkDataFromPosition = GetChunkDataFromPosition(_chunkEvent.Position, _createIfNeeded: true);
		if (chunkDataFromPosition.IsReady)
		{
			chunkDataFromPosition.AddEvent(_chunkEvent);
			if (!checkChunks.Contains(chunkDataFromPosition))
			{
				checkChunks.Add(chunkDataFromPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckToSpawn()
	{
		if (checkChunks.Count > 0)
		{
			AIDirectorChunkData chunkData = checkChunks[0];
			checkChunks.RemoveAt(0);
			CheckToSpawn(chunkData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckToSpawn(AIDirectorChunkData _chunkData)
	{
		if (!GameStats.GetBool(EnumGameStats.ZombieHordeMeter) || !GameStats.GetBool(EnumGameStats.IsSpawnEnemies) || !(_chunkData.ActivityLevel >= 25f))
		{
			return;
		}
		AIDirectorChunkEvent aIDirectorChunkEvent = _chunkData.FindBestEventAndReset();
		if (aIDirectorChunkEvent != null)
		{
			bool flag = Director.random.RandomFloat < 0.2f && !GameUtils.IsPlaytesting();
			StartCooldownOnNeighbors(aIDirectorChunkEvent.Position, flag);
			if (flag)
			{
				_chunkData.SetLongDelay();
				SpawnScouts(aIDirectorChunkEvent.Position.ToVector3());
			}
		}
		else
		{
			AIDirector.LogAI("Chunk event not found!");
		}
	}

	public void SpawnScouts(Vector3 targetPos)
	{
		if (FindScoutStartPos(targetPos, out var startPos))
		{
			EntityPlayer closestPlayer = Director.World.GetClosestPlayer(targetPos, 120f, _isDead: false);
			if ((bool)closestPlayer)
			{
				int num = GameStageDefinition.CalcGameStageAround(closestPlayer);
				string text = "ScoutsRadiated";
				if (num < 45)
				{
					text = "Scouts1";
				}
				else if (num < 85)
				{
					text = "Scouts2";
				}
				else if (num < 125)
				{
					text = "ScoutsFeral";
				}
				EntitySpawner spawner = new EntitySpawner(text, Vector3i.zero, Vector3i.zero, 0);
				scoutSpawnList.Add(new AIScoutHordeSpawner(spawner, startPos, targetPos, _isBloodMoon: false));
				AIDirector.LogAI("Spawning {0} at {1}, to {2}", text, startPos.ToCultureInvariantString(), targetPos.ToCultureInvariantString());
			}
		}
		else
		{
			AIDirector.LogAI("Scout spawning failed");
		}
	}

	public AIScoutHordeSpawner.IHorde CreateHorde(Vector3 startPos)
	{
		Horde horde = new Horde(this, startPos);
		hordeSpawnList.Add(horde);
		return horde;
	}
}
