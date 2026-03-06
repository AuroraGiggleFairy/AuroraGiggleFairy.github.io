using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorBloodMoonComponent : AIDirectorComponent
{
	public const int cPartyEnemyMax = 30;

	public const int cTimeStayAfterDeathScale = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnDelay = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDayLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDayNextOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dawnHour;

	[PublicizedFrom(EAccessModifier.Private)]
	public int duskHour;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextParty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBloodMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorBloodMoonParty> parties = new List<AIDirectorBloodMoonParty>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> players = new List<EntityPlayer>();

	public bool BloodMoonActive => isBloodMoon;

	public override void InitNewGame()
	{
		base.InitNewGame();
		int num = GameUtils.WorldTimeToDays(Director.World.worldTime);
		bmDayLast = (num - 1) / 7 * 7;
		CalcNextDay();
		ComputeDawnAndDuskTimes();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		World world = Director.World;
		bool flag = isBloodMoon;
		isBloodMoon = IsBloodMoonTime(world.worldTime);
		if (isBloodMoon != flag)
		{
			if (isBloodMoon)
			{
				StartBloodMoon();
			}
			else
			{
				EndBloodMoon();
			}
		}
		if (!isBloodMoon)
		{
			int num = GameStats.GetInt(EnumGameStats.BloodMoonDay);
			if (num != bmDay)
			{
				bmDay = num;
				bmDayLast = num - 1;
				Log.Warning("Blood Moon day stat changed {0}", num);
			}
		}
		if (!isBloodMoon || !GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		delay -= (float)_dt;
		for (int i = 0; i < players.Count; i++)
		{
			EntityPlayer entityPlayer = players[i];
			if (entityPlayer.bloodMoonParty == null && entityPlayer.IsSpawned())
			{
				AddPlayerToParty(entityPlayer);
			}
		}
		for (int j = 0; j < parties.Count; j++)
		{
			if (nextParty >= parties.Count)
			{
				nextParty = 0;
			}
			AIDirectorBloodMoonParty aIDirectorBloodMoonParty = parties[j];
			bool flag2 = j == nextParty && delay <= 0f;
			if (aIDirectorBloodMoonParty.IsEmpty)
			{
				aIDirectorBloodMoonParty.KillPartyZombies();
				if (flag2)
				{
					nextParty++;
				}
			}
			else if (aIDirectorBloodMoonParty.Tick(world, _dt, flag2) && flag2)
			{
				delay = 1f / (float)parties.Count;
				nextParty++;
			}
		}
	}

	public bool SetForToday(bool _keepNextDay)
	{
		int num = GameUtils.WorldTimeToDays(Director.World.worldTime);
		if (num == bmDay)
		{
			return false;
		}
		if (_keepNextDay)
		{
			bmDayNextOverride = bmDay;
		}
		SetDay(num);
		return true;
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		if (_version >= 8)
		{
			bmDayLast = _stream.ReadInt32();
			int day = _stream.ReadInt32();
			int num = _stream.ReadInt16();
			int num2 = _stream.ReadInt16();
			int num3 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
			int num4 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange);
			if (num3 != num || num4 != num2)
			{
				CalcNextDay();
			}
			else
			{
				SetDay(day);
			}
		}
		ComputeDawnAndDuskTimes();
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(bmDayLast);
		_stream.Write(bmDay);
		_stream.Write((short)GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency));
		_stream.Write((short)GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange));
	}

	public void AddPlayer(EntityPlayer _player)
	{
		players.Add(_player);
	}

	public void RemovePlayer(EntityPlayer _player)
	{
		if (players.Remove(_player))
		{
			for (int i = 0; i < parties.Count; i++)
			{
				parties[i].PlayerLoggedOut(_player);
			}
		}
	}

	public void TimeChanged(bool isSeek = false)
	{
		if (isBloodMoon && !IsBloodMoonTime(Director.World.worldTime))
		{
			EndBloodMoon();
		}
		if (bmDay != GameUtils.WorldTimeToElements(Director.World.worldTime).Days && !isBloodMoon && !IsBloodMoonTime(Director.World.worldTime))
		{
			CalcNextDay(isSeek);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartBloodMoon()
	{
		Log.Out("BloodMoon starting for day " + GameUtils.WorldTimeToDays(Director.World.worldTime));
		ClearParties();
		for (int i = 0; i < players.Count; i++)
		{
			players[i].IsBloodMoonDead = false;
		}
		delay = 0f;
		DictionaryList<int, Entity> entities = Director.World.Entities;
		for (int j = 0; j < entities.Count; j++)
		{
			EntityEnemy entityEnemy = entities.list[j] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.IsBloodMoon = true;
				entityEnemy.timeStayAfterDeath /= 3;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndBloodMoon()
	{
		Log.Out("Blood moon is over!");
		isBloodMoon = false;
		if (bmDayNextOverride > 0)
		{
			bmDay = bmDayNextOverride;
			bmDayNextOverride = 0;
			SetDay(bmDay);
		}
		if (GameUtils.WorldTimeToDays(Director.World.worldTime) > bmDay)
		{
			bmDayLast = bmDay;
			CalcNextDay();
		}
		ClearParties();
		DictionaryList<int, Entity> entities = Director.World.Entities;
		for (int i = 0; i < entities.Count; i++)
		{
			EntityEnemy entityEnemy = entities.list[i] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.bIsChunkObserver = false;
				entityEnemy.IsHordeZombie = false;
				entityEnemy.IsBloodMoon = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearParties()
	{
		nextParty = 0;
		parties.Clear();
		for (int i = 0; i < players.Count; i++)
		{
			players[i].bloodMoonParty = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddPlayerToParty(EntityPlayer _player)
	{
		for (int i = 0; i < parties.Count; i++)
		{
			AIDirectorBloodMoonParty aIDirectorBloodMoonParty = parties[i];
			if (aIDirectorBloodMoonParty.IsMemberOfParty(_player.entityId))
			{
				aIDirectorBloodMoonParty.AddPlayer(_player);
				break;
			}
		}
		if (_player.bloodMoonParty == null)
		{
			for (int j = 0; j < parties.Count && !parties[j].TryAddPlayer(_player); j++)
			{
			}
		}
		if (_player.bloodMoonParty == null)
		{
			CreateNewParty(_player);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateNewParty(EntityPlayer _player)
	{
		parties.Add(new AIDirectorBloodMoonParty(_player, this, GameStats.GetInt(EnumGameStats.BloodMoonEnemyCount)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComputeDawnAndDuskTimes()
	{
		(duskHour, dawnHour) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsBloodMoonTime(ulong worldTime)
	{
		return GameUtils.IsBloodMoonTime(worldTime, (duskHour: duskHour, dawnHour: dawnHour), bmDay);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcNextDay(bool isSeek = false)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
		int num2;
		if (num <= 0)
		{
			num2 = 0;
		}
		else
		{
			int num3 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange);
			int num4 = num + base.Random.RandomRange(0, num3 + 1);
			int num5 = GameUtils.WorldTimeToDays(Director.World.worldTime);
			while (num5 <= bmDayLast)
			{
				bmDayLast -= num4;
			}
			if (bmDayLast < 0)
			{
				bmDayLast = 0;
			}
			num2 = bmDayLast;
			do
			{
				num2 += num4;
			}
			while (num2 < num5);
			bmDayLast = num2 - num4;
			if (isSeek && bmDay > bmDayLast && bmDay <= bmDayLast + num + num3)
			{
				num2 = bmDay;
			}
		}
		SetDay(num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDay(int day)
	{
		if (GameManager.Instance != null && GameManager.Instance.gameStateManager != null)
		{
			GameManager.Instance.gameStateManager.SetBloodMoonDay(day);
		}
		if (bmDay != day)
		{
			bmDay = day;
			Log.Out("BloodMoon SetDay: day {0}, last day {1}, freq {2}, range {3}", bmDay, bmDayLast, GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency), GamePrefs.GetInt(EnumGamePrefs.BloodMoonRange));
		}
	}

	public void LogBM(string format, params object[] args)
	{
		format = $"{Time.frameCount} BM {format}";
		Log.Warning(format, args);
	}
}
