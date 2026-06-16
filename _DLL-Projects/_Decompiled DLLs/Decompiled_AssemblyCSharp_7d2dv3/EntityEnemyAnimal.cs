using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityEnemyAnimal : EntityEnemy
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if ((bool)ModelTransform)
		{
			animator = ModelTransform.GetComponentInChildren<Animator>();
		}
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

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		Entity entity = world.GetEntity(_sourceEntityId);
		if ((bool)entity && entity.entityClass == entityClass)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (Electrocuted)
		{
			SetMoveForward(0f);
			if ((bool)animator)
			{
				animator.enabled = false;
			}
		}
		else
		{
			if ((bool)animator)
			{
				animator.enabled = true;
			}
			base.updateTasks();
		}
	}
}
