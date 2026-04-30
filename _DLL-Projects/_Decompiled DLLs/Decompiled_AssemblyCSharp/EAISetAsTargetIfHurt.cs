using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAISetAsTargetIfHurt : EAITarget
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct TargetClass
	{
		public Type type;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TargetClass> targetClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public float viewAngleSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewAngleRestoreCounter;

	public override void Init(EntityAlive _theEntity)
	{
		Init(_theEntity, 0f, _bNeedToSee: false);
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		targetClasses = new List<TargetClass>();
		if (data.TryGetValue("class", out var _value))
		{
			string[] array = _value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				TargetClass item = new TargetClass
				{
					type = EntityFactory.GetEntityType(array[i])
				};
				targetClasses.Add(item);
			}
		}
	}

	public override bool CanExecute()
	{
		EntityAlive revengeTarget = theEntity.GetRevengeTarget();
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if ((bool)revengeTarget && revengeTarget != attackTarget && revengeTarget.entityType != theEntity.entityType)
		{
			if (targetClasses != null)
			{
				bool flag = false;
				Type type = revengeTarget.GetType();
				for (int i = 0; i < targetClasses.Count; i++)
				{
					TargetClass targetClass = targetClasses[i];
					if (targetClass.type != null && targetClass.type.IsAssignableFrom(type))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (attackTarget != null && attackTarget.IsAlive() && base.RandomFloat < 0.66f)
			{
				theEntity.SetRevengeTarget(null);
				return false;
			}
			if (check(revengeTarget))
			{
				return true;
			}
			Vector3 vector = theEntity.position - revengeTarget.position;
			float searchRadius = EntityClass.list[theEntity.entityClass].SearchRadius;
			vector = revengeTarget.position + vector.normalized * (searchRadius * 0.35f);
			Vector2 vector2 = manager.random.RandomInsideUnitCircle * searchRadius;
			vector.x += vector2.x;
			vector.z += vector2.y;
			Vector3i vector3i = World.worldToBlockPos(vector);
			int height = theEntity.world.GetHeight(vector3i.x, vector3i.z);
			if (height > 0)
			{
				vector.y = height;
			}
			int ticks = theEntity.CalcInvestigateTicks(1200, revengeTarget);
			theEntity.SetInvestigatePosition(vector, ticks);
			theEntity.SetRevengeTarget(null);
		}
		return false;
	}

	public override void Start()
	{
		theEntity.SetAttackTarget(theEntity.GetRevengeTarget(), 400);
		viewAngleSave = theEntity.GetMaxViewAngle();
		theEntity.SetMaxViewAngle(270f);
		viewAngleRestoreCounter = 100;
		base.Start();
	}

	public override void Update()
	{
		if (viewAngleRestoreCounter > 0)
		{
			viewAngleRestoreCounter--;
			if (viewAngleRestoreCounter == 0)
			{
				restoreViewAngle();
			}
		}
	}

	public override bool Continue()
	{
		if (theEntity.GetRevengeTarget() != null && theEntity.GetAttackTarget() != theEntity.GetRevengeTarget())
		{
			return false;
		}
		return base.Continue();
	}

	public override void Reset()
	{
		base.Reset();
		restoreViewAngle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void restoreViewAngle()
	{
		if (viewAngleSave > 0f)
		{
			theEntity.SetMaxViewAngle(viewAngleSave);
			viewAngleSave = 0f;
			viewAngleRestoreCounter = 0;
		}
	}
}
