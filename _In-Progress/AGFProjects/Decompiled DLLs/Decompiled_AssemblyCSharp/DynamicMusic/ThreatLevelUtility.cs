using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine;

namespace DynamicMusic;

public static class ThreatLevelUtility
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ZOMBIE_COMBAT_QUANTITY = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PLAYER_HOME_MINIMUM_DISTANCE = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float THREAT_PER_ENEMY = 1f / 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> enemies = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 boundingBoxRange = new Vector3(50f, 50f, 50f);

	public static int Zombies;

	public static int Targeting;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<float> threatLevels = new Queue<float>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int LOOKBACK = 300;

	public static float GetThreatLevelOn(EntityPlayerLocal _player)
	{
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityEnemy), new Bounds(_player.position, boundingBoxRange), enemies);
		float num = 0f;
		int num2 = (Zombies = zombiesContributingThreat());
		int num3 = (Targeting = EnemiesTargeting());
		if (num3 > 0)
		{
			_player.LastTargetEventTime = Time.time;
		}
		if ((num2 >= 4 && num3 > 0) || (_player.ThreatLevel.Category == ThreatLevelType.Panicked && num2 > 0 && Time.time - _player.LastTargetEventTime < 15f) || GameUtils.IsBloodMoonTime(GameManager.Instance.World.worldTime, GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength)), GameStats.GetInt(EnumGameStats.BloodMoonDay)))
		{
			int num4 = GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount) * ((_player.Party == null) ? 1 : _player.Party.MemberList.Count);
			float num5 = 0.3f / (float)num4;
			float num6 = 0.15f / (float)num4;
			float item = MathUtils.Clamp(0.7f + num5 * (float)num3 + num6 * (float)(num2 - num3), 0.7f, 1f);
			threatLevels.Enqueue(item);
			if (threatLevels.Count > 300)
			{
				threatLevels.Dequeue();
			}
			num = MathUtils.Clamp(threatLevels.Average(), 0.7f, 1f);
		}
		else
		{
			num += (GameManager.Instance.World.IsDark() ? 0.1f : 0f);
			num += (isPlayerInUnclearedPOI(_player) ? 0.2f : 0f);
			num += (IsPlayerHome(_player) ? 0f : 0.2f);
			num += (IsPlayerInSpookyBiome(_player) ? 0.1f : 0f);
			num += (float)num2 * (1f / 30f);
		}
		enemies.Clear();
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isPlayerInUnclearedPOI(EntityPlayerLocal _player)
	{
		if (_player.PlayerStats.LightInsidePer > 0.2f)
		{
			if (GamePrefs.GetString(EnumGamePrefs.GameWorld).Equals("Playtesting"))
			{
				return true;
			}
			if (_player.prefab != null)
			{
				foreach (SleeperVolume sleeperVolume in _player.prefab.sleeperVolumes)
				{
					if (!sleeperVolume.wasCleared)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int zombiesContributingThreat()
	{
		int num = 0;
		for (int i = 0; i < enemies.Count; i++)
		{
			EntityEnemy entityEnemy = enemies[i] as EntityEnemy;
			if (entityEnemy.IsAlive() && !entityEnemy.IsSleeping)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int EnemiesTargeting()
	{
		int num = 0;
		for (int i = 0; i < enemies.Count; i++)
		{
			EntityEnemy entityEnemy = enemies[i] as EntityEnemy;
			if (entityEnemy != null && entityEnemy.IsAlive() && entityEnemy.GetAttackTargetLocal() as EntityPlayer != null)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsPlayerHome(EntityPlayerLocal _player)
	{
		SpawnPosition spawnPoint = _player.GetSpawnPoint();
		if (spawnPoint.IsUndef())
		{
			return false;
		}
		if ((spawnPoint.position - _player.position).magnitude > 50f)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsPlayerInSpookyBiome(EntityPlayerLocal _player)
	{
		if (_player.biomeStandingOn == null)
		{
			return false;
		}
		if (!_player.biomeStandingOn.m_sBiomeName.Equals("burnt_forest"))
		{
			return _player.biomeStandingOn.m_sBiomeName.Equals("wasteland");
		}
		return true;
	}
}
