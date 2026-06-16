using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy;

public class ThreatLevelTracker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager dynamicMusicManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleeperIncrement = 1f / 32f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTargetIncrement = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAlertIncrement = 0.125f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cInactiveIncrement = 0.0625f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBaseIncrement = 0.0015f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal epLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal somePlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> enemies;

	[PublicizedFrom(EAccessModifier.Private)]
	public float threatLevelTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 boundingBoxRange = new Vector3(50f, 50f, 50f);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int DeadEnemies
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int InactiveEnemies
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SleepingEnemies
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int ActiveEnemies
	{
		get
		{
			if (enemies == null)
			{
				return 0;
			}
			return enemies.Count - DeadEnemies - InactiveEnemies - SleepingEnemies;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float NumericalThreatLevel
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ThreatLevelLegacyType ThreatLevel
	{
		get
		{
			if (NumericalThreatLevel < 0.25f)
			{
				return ThreatLevelLegacyType.Exploration;
			}
			return ThreatLevelLegacyType.Suspense;
		}
	}

	public bool IsMusicPlayingThisTick
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return dynamicMusicManager.IsMusicPlayingThisTick;
		}
	}

	public bool IsThreatLevelInExploration
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return NumericalThreatLevel < 0.25f;
		}
	}

	public bool IsTargetInExploration
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return threatLevelTarget <= 0.25f;
		}
	}

	public bool IsTargetAboveThreatLevel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return threatLevelTarget > NumericalThreatLevel;
		}
	}

	public static void Init(DynamicMusicManager _dmManager)
	{
		_dmManager.ThreatLevelTracker = new ThreatLevelTracker();
		_dmManager.ThreatLevelTracker.dynamicMusicManager = _dmManager;
		_dmManager.ThreatLevelTracker.somePlayer = GameManager.Instance.World.GetPrimaryPlayer();
		_dmManager.ThreatLevelTracker.epLocal = _dmManager.PrimaryLocalPlayer;
		_dmManager.ThreatLevelTracker.NumericalThreatLevel = 0f;
		_dmManager.ThreatLevelTracker.enemies = new List<Entity>();
	}

	public void Tick()
	{
		if (GameTimer.Instance.ticks % 20 == 0L)
		{
			TickTrackThreatLevel();
		}
		TickMoveThreatLevel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickMoveThreatLevel()
	{
		if (!IsMusicPlayingThisTick || (IsTargetInExploration && IsThreatLevelInExploration))
		{
			NumericalThreatLevel = threatLevelTarget;
		}
		else if (IsTargetAboveThreatLevel)
		{
			if (IsThreatLevelInExploration)
			{
				NumericalThreatLevel = 0.25f;
			}
			else
			{
				NumericalThreatLevel = Utils.FastClamp(NumericalThreatLevel + 0.0015f, 0f, threatLevelTarget);
			}
		}
		else
		{
			NumericalThreatLevel = Utils.FastClamp(NumericalThreatLevel - 0.003f, threatLevelTarget, NumericalThreatLevel);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickTrackThreatLevel()
	{
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityEnemy), new Bounds(epLocal.position, boundingBoxRange), enemies);
		int num = (InactiveEnemies = 0);
		int deadEnemies = (SleepingEnemies = num);
		DeadEnemies = deadEnemies;
		threatLevelTarget = 0f;
		for (int i = 0; i < enemies.Count; i++)
		{
			EntityEnemy entityEnemy = enemies[i] as EntityEnemy;
			if (entityEnemy.IsDead())
			{
				DeadEnemies++;
			}
			else if (entityEnemy.IsSleeping)
			{
				threatLevelTarget += 1f / 32f;
				SleepingEnemies++;
			}
			else if (EnemyIsTargetingPlayer(entityEnemy))
			{
				threatLevelTarget += 0.25f;
			}
			else if (entityEnemy.IsAlert)
			{
				threatLevelTarget += 0.125f;
			}
			else
			{
				threatLevelTarget += 0.0625f;
				InactiveEnemies++;
			}
		}
		threatLevelTarget = Utils.FastClamp(threatLevelTarget, 0f, 0.5f);
		enemies.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EnemyIsTargetingPlayer(EntityEnemy _enemy)
	{
		EntityAlive attackTarget = _enemy.GetAttackTarget();
		if (attackTarget != null && attackTarget.Equals(epLocal))
		{
			return true;
		}
		return false;
	}

	public void Event(MinEventTypes _eventType, MinEventParams _eventParms)
	{
	}
}
