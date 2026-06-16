using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAnimal : EntityAlive
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float minStressTime = 2.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxStressTime = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDistressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timer;

	public void SetDistressed(bool _isDistressed, float _minTime, float _maxTime, int _playerId)
	{
		isDistressed = _isDistressed;
		minStressTime = _minTime;
		maxStressTime = _maxTime;
		playerId = _playerId;
		timer = 0f;
	}

	public void ClearDistressed()
	{
		isDistressed = false;
		EntityPlayerLocal entityPlayerLocal = getEntityPlayerLocal();
		if ((bool)entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(entityId);
		}
	}

	public override void OnUpdateLive()
	{
		GetEntitySenses().Clear();
		base.OnUpdateLive();
		if (isDistressed && IsAlive())
		{
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				timer += rand.RandomRange(minStressTime, maxStressTime);
				GameManager.Instance.PlaySoundAtPositionServer(position, GetSoundHurt(), AudioRolloffMode.Linear, 1, entityId);
			}
			EntityPlayerLocal entityPlayerLocal = getEntityPlayerLocal();
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.Waypoints.UpdateEntityAnimalWayPoint(this, unloaded: true);
			}
		}
	}

	public override bool IsDrawMapIcon()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		return new Color(1f, 0.8235294f, 29f / 85f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float getNextStepSoundDistance()
	{
		return 0.8f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override void OnEntityDeath()
	{
		if ((bool)PhysicsTransform)
		{
			PhysicsTransform.gameObject.SetActive(value: false);
		}
		base.OnEntityDeath();
		EntityPlayerLocal entityPlayerLocal = getEntityPlayerLocal();
		if ((bool)entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(entityId);
		}
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		EntityPlayerLocal entityPlayerLocal = getEntityPlayerLocal();
		if ((bool)entityPlayerLocal)
		{
			entityPlayerLocal.Waypoints.TryRemoveLastKnownPositionWaypoint(entityId);
		}
	}

	public override void OnCollectServer(int _playerId)
	{
		GameManager.Instance.World.RemoveEntity(entityId, EnumRemoveEntityReason.Captured);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal getEntityPlayerLocal()
	{
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer();
		if ((bool)entityPlayerLocal && entityPlayerLocal.entityId != playerId)
		{
			entityPlayerLocal = null;
		}
		return entityPlayerLocal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAnimal()
	{
	}
}
