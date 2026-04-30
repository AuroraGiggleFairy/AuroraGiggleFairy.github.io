using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorWanderingHordeComponent : AIDirectorHordeComponent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNextHourMin = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlaytest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIWanderingHordeSpawner> spawners = new List<AIWanderingHordeSpawner>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong BanditNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong HordeNextTime;

	public bool HasAnySpawns => spawners.Count != 0;

	public bool OtherHordesAreActive
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!SkyManager.IsBloodMoonVisible())
			{
				return Director.GetComponent<AIDirectorChunkEventComponent>().HasAnySpawns;
			}
			return true;
		}
	}

	public override void InitNewGame()
	{
		isPlaytest = GameUtils.IsPlaytesting();
		BanditNextTime = 0uL;
		HordeNextTime = 0uL;
	}

	public override void Tick(double _dt)
	{
		if (!isPlaytest)
		{
			base.Tick(_dt);
			TickActiveSpawns((float)_dt);
			TickNextTime(ref HordeNextTime, AIWanderingHordeSpawner.SpawnType.Horde);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickActiveSpawns(float dt)
	{
		for (int num = spawners.Count - 1; num >= 0; num--)
		{
			AIWanderingHordeSpawner aIWanderingHordeSpawner = spawners[num];
			if (aIWanderingHordeSpawner.Update(Director.World, dt))
			{
				AIDirector.LogAIExtra("Wandering spawner finished {0}", aIWanderingHordeSpawner.spawnType);
				aIWanderingHordeSpawner.Cleanup();
				spawners.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickNextTime(ref ulong _nextTime, AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		if (!GameStats.GetBool(EnumGameStats.ZombieHordeMeter) || !GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			_nextTime = 0uL;
			return;
		}
		if (_nextTime == 0L)
		{
			if (Director.World.worldTime > 28000)
			{
				ChooseNextTime(_spawnType);
			}
			return;
		}
		int num = (int)(_nextTime - Director.World.worldTime);
		int num2 = num / 1000;
		if (num2 >= 7)
		{
			return;
		}
		if (OtherHordesAreActive)
		{
			_nextTime += (uint)((7 - num2) * 1000);
		}
		else if (num <= 0)
		{
			if (Director.World.Players.Count > 0)
			{
				StartSpawning(_spawnType);
			}
			else
			{
				ChooseNextTime(_spawnType);
			}
		}
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		HordeNextTime = _stream.ReadUInt64();
		if (_version > 3)
		{
			BanditNextTime = _stream.ReadUInt64();
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(HordeNextTime);
		_stream.Write(BanditNextTime);
	}

	public void StartSpawning(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		AIDirector.LogAI("Wandering StartSpawning {0}", _spawnType);
		CleanupType(_spawnType);
		bool flag = false;
		DictionaryList<int, AIDirectorPlayerState> trackedPlayers = Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers;
		for (int i = 0; i < trackedPlayers.list.Count; i++)
		{
			if (!trackedPlayers.list[i].Dead)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AIDirector.LogAI("Spawn {0}, no living players, wait 4 hours", _spawnType);
			SetNextTime(_spawnType, Director.World.worldTime + 4000);
			return;
		}
		List<AIDirectorPlayerState> list = new List<AIDirectorPlayerState>();
		Vector3 startPos;
		Vector3 pitStop;
		Vector3 endPos;
		uint num = FindTargets(out startPos, out pitStop, out endPos, list);
		if (num != 0)
		{
			AIDirector.LogAI("Spawn {0}, find targets, wait {1} hours", _spawnType, num);
			SetNextTime(_spawnType, Director.World.worldTime + 1000 * num);
		}
		else
		{
			ChooseNextTime(_spawnType);
			spawners.Add(new AIWanderingHordeSpawner(Director, _spawnType, null, list, Director.World.worldTime + 12000, startPos, pitStop, endPos));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupType(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		for (int num = spawners.Count - 1; num >= 0; num--)
		{
			AIWanderingHordeSpawner aIWanderingHordeSpawner = spawners[num];
			if (aIWanderingHordeSpawner.spawnType == _spawnType)
			{
				aIWanderingHordeSpawner.Cleanup();
				spawners.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChooseNextTime(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		switch (_spawnType)
		{
		case AIWanderingHordeSpawner.SpawnType.Bandits:
			BanditNextTime = Director.World.worldTime + (ulong)base.Random.RandomRange(12000, 24000);
			BanditNextTime += 2000uL;
			break;
		case AIWanderingHordeSpawner.SpawnType.Horde:
			HordeNextTime = Director.World.worldTime + (ulong)base.Random.RandomRange(12000, 24000);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNextTime(AIWanderingHordeSpawner.SpawnType _spawnType, ulong _time)
	{
		switch (_spawnType)
		{
		case AIWanderingHordeSpawner.SpawnType.Bandits:
			BanditNextTime = _time;
			break;
		case AIWanderingHordeSpawner.SpawnType.Horde:
			HordeNextTime = _time;
			break;
		}
	}

	public void LogTimes()
	{
		AIDirector.LogAI("Next wandering - bandit {0}, horde {1}", GameUtils.WorldTimeToString(BanditNextTime), GameUtils.WorldTimeToString(HordeNextTime));
	}
}
