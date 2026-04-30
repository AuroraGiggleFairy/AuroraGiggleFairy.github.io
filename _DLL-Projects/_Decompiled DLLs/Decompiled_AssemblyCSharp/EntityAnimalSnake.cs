using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityAnimalSnake : EntityHuman
{
	public override Vector3 GetAttackTargetHitPosition()
	{
		Vector3 result = attackTarget.position;
		result.y += 0.5f;
		return result;
	}
}
