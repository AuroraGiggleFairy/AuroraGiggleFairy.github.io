using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRandomizeRotation : ActionBaseClientAction
{
	public override void OnClientPerform(Entity target)
	{
		if (!(target != null))
		{
			return;
		}
		Entity attachedToEntity = target.AttachedToEntity;
		float num = GameEventManager.Current.Random.RandomRange(45, 315);
		if ((bool)attachedToEntity)
		{
			Transform physicsTransform = attachedToEntity.PhysicsTransform;
			Quaternion rotation = physicsTransform.rotation;
			rotation = Quaternion.AngleAxis(num, physicsTransform.up) * rotation;
			attachedToEntity.SetRotation(rotation.eulerAngles);
			physicsTransform.rotation = rotation;
			EntityVehicle entityVehicle = attachedToEntity as EntityVehicle;
			if ((bool)entityVehicle)
			{
				entityVehicle.CameraChangeRotation(num);
			}
		}
		else
		{
			Vector3 rotation2 = target.rotation;
			rotation2.y += num;
			target.SetRotation(rotation2);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRandomizeRotation
		{
			targetGroup = targetGroup
		};
	}
}
