using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAnimal : EntityAlive
{
	public override void OnUpdateLive()
	{
		GetEntitySenses().Clear();
		base.OnUpdateLive();
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAnimal()
	{
	}
}
