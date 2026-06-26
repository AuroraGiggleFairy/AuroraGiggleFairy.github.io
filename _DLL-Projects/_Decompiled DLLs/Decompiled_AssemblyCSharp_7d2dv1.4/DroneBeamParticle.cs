using System;
using UnityEngine;

public class DroneBeamParticle : MonoBehaviour
{
	public GameObject root;

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
			return;
		}
		displayTime -= Time.deltaTime;
		EntityAlive attackTargetLocal = drone.GetAttackTargetLocal();
		if ((bool)attackTargetLocal)
		{
			Vector3 chestPosition = attackTargetLocal.getChestPosition();
			root.transform.rotation = Quaternion.LookRotation(chestPosition - drone.HealArmPosition);
			if (displayTime <= 0f || ((bool)attackTargetLocal && attackTargetLocal.IsDead()))
			{
				UnityEngine.Object.Destroy(root);
			}
		}
	}

	public void SetDisplayTime(float time)
	{
		displayTime = time;
	}
}
