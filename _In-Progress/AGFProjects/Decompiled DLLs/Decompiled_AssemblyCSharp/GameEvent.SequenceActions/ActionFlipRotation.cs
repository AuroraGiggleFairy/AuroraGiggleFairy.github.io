using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionFlipRotation : ActionBaseClientAction
{
	public override void OnClientPerform(Entity target)
	{
		if (!(target != null))
		{
			return;
		}
		Entity attachedToEntity = target.AttachedToEntity;
		if ((bool)attachedToEntity)
		{
			Transform physicsTransform = attachedToEntity.PhysicsTransform;
			Quaternion rotation = physicsTransform.rotation;
			rotation = Quaternion.AngleAxis(180f, physicsTransform.up) * rotation;
			attachedToEntity.SetRotation(rotation.eulerAngles);
			physicsTransform.rotation = rotation;
			EntityVehicle entityVehicle = attachedToEntity as EntityVehicle;
			if ((bool)entityVehicle)
			{
				entityVehicle.CameraChangeRotation(180f);
				entityVehicle.VelocityFlip();
			}
		}
		else
		{
			Vector3 rotation2 = target.rotation;
			rotation2.y += 180f;
			target.SetRotation(rotation2);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionFlipRotation
		{
			targetGroup = targetGroup
		};
	}
}
