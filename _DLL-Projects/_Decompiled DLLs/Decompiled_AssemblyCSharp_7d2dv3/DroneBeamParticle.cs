using System;
using UnityEngine;

public class DroneBeamParticle : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone drone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float displayTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!drone)
		{
			drone = GetComponentInParent<EntityDrone>();
			Transform transform = base.transform.parent.FindInChilds("WristLeft");
			if ((bool)transform)
			{
				base.transform.SetParent(transform, worldPositionStays: false);
			}
		}
		else
		{
			if (!(displayTime > 0f))
			{
				return;
			}
			displayTime -= Time.deltaTime;
			EntityAlive attackTargetLocal = drone.GetAttackTargetLocal();
			if ((bool)attackTargetLocal)
			{
				base.transform.rotation = Quaternion.LookRotation(attackTargetLocal.getChestPosition() - drone.GetHealArmPosition());
				if (attackTargetLocal.IsDead())
				{
					displayTime = 0f;
				}
			}
			if (displayTime <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void SetDisplayTime(float time)
	{
		displayTime = time;
	}
}
