using UnityEngine;

public abstract class EntityEnemy : EntityAlive
{
	public bool IsHordeZombie;

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
	}

	public override void PostInit()
	{
		base.PostInit();
		if (!isEntityRemote)
		{
			IsBloodMoon = world.aiDirector.BloodMoonComponent.BloodMoonActive;
		}
	}

	public override bool IsDrawMapIcon()
	{
		return true;
	}

	public override Vector3 GetMapIconScale()
	{
		return new Vector3(0.75f, 0.75f, 1f);
	}

	public override bool IsSavedToFile()
	{
		if (GetSpawnerSource() != EnumSpawnerSource.Dynamic || IsDead())
		{
			return base.IsSavedToFile();
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		if (!IsHordeZombie || world.GetPlayers().Count == 0)
		{
			return base.canDespawn();
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
		if (!isEntityRemote && GetSpawnerSource() != EnumSpawnerSource.Dynamic && target is EntityPlayer)
		{
			world.aiDirector.NotifyIntentToAttack(this, target as EntityPlayer);
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityEnemy()
	{
	}
}
