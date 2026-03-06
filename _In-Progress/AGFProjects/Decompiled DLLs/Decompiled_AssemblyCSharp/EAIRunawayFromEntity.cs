using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayFromEntity : EAIRunAway
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Type> targetClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive avoidEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRunNoiseVolume = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public float safeDistance = 38f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minSneakDistance = 3.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> list = new List<Entity>();

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		targetClasses = new List<Type>();
		if (data.TryGetValue("class", out var _value))
		{
			string[] array = _value.Split(',');
			for (int i = 0; i < array.Length; i += 2)
			{
				Type entityType = EntityFactory.GetEntityType(array[i]);
				targetClasses.Add(entityType);
			}
		}
		GetData(data, "safeDistance", ref safeDistance);
		GetData(data, "minSneakDistance", ref minSneakDistance);
	}

	public override bool CanExecute()
	{
		FindEnemy();
		if (avoidEntity == null)
		{
			return false;
		}
		return base.CanExecute();
	}

	public void FindEnemy()
	{
		avoidEntity = null;
		if ((bool)theEntity.noisePlayer && theEntity.noisePlayerVolume >= 8f)
		{
			avoidEntity = theEntity.noisePlayer;
			return;
		}
		float seeDistance = theEntity.GetSeeDistance();
		Bounds bb = BoundsUtils.ExpandBounds(theEntity.boundingBox, seeDistance, seeDistance, seeDistance);
		for (int i = 0; i < targetClasses.Count; i++)
		{
			Type type = targetClasses[i];
			theEntity.world.GetEntitiesInBounds(type, bb, list);
			if (type == typeof(EntityPlayer))
			{
				float num = float.MaxValue;
				for (int j = 0; j < list.Count; j++)
				{
					EntityPlayer entityPlayer = list[j] as EntityPlayer;
					float seeDistance2 = manager.GetSeeDistance(entityPlayer);
					if (seeDistance2 < num && theEntity.CanSee(entityPlayer) && theEntity.CanSeeStealth(seeDistance2, entityPlayer.Stealth.lightLevel) && !entityPlayer.IsIgnoredByAI())
					{
						num = seeDistance2;
						avoidEntity = entityPlayer;
					}
				}
			}
			else
			{
				float num2 = float.MaxValue;
				for (int k = 0; k < list.Count; k++)
				{
					EntityAlive entityAlive = list[k] as EntityAlive;
					float distanceSq = theEntity.GetDistanceSq(entityAlive);
					if (distanceSq <= minSneakDistance * minSneakDistance)
					{
						avoidEntity = entityAlive;
						break;
					}
					if (distanceSq < num2 && theEntity.CanSee(entityAlive) && !entityAlive.IsIgnoredByAI())
					{
						num2 = distanceSq;
						avoidEntity = entityAlive;
					}
				}
			}
			list.Clear();
			if ((bool)avoidEntity)
			{
				break;
			}
		}
	}

	public override bool Continue()
	{
		if (theEntity.GetDistanceSq(avoidEntity) >= safeDistance * safeDistance)
		{
			return false;
		}
		return base.Continue();
	}

	public override void Reset()
	{
		avoidEntity = null;
	}

	public override void Update()
	{
		base.Update();
		theEntity.navigator.setMoveSpeed(theEntity.IsSwimming() ? theEntity.GetMoveSpeed() : theEntity.GetMoveSpeedPanic());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetFleeFromPos()
	{
		return avoidEntity.position;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}", base.ToString(), (avoidEntity != null) ? avoidEntity.GetDebugName() : "");
	}
}
