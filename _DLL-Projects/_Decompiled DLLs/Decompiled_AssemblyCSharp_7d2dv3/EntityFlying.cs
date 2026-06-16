using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFlying : EntityEnemy
{
	public override bool IsAirBorne()
	{
		return true;
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		if (AttachedToEntity != null)
		{
			return;
		}
		if (IsDead())
		{
			entityCollision(motion);
			motion.y -= 0.08f;
			motion.y *= 0.98f;
			motion.x *= 0.91f;
			motion.z *= 0.91f;
			return;
		}
		if (IsInWater())
		{
			Move(_direction, _isDirAbsolute, 0.02f, 1f);
			entityCollision(motion);
			motion *= 0.8f;
			return;
		}
		float num = 0.91f;
		if (onGround)
		{
			num = 0.55f;
			BlockValue block = world.GetBlock(Utils.Fastfloor(position.x), Utils.Fastfloor(boundingBox.min.y) - 1, Utils.Fastfloor(position.z));
			if (!block.isair)
			{
				num = Mathf.Clamp(block.Block.blockMaterial.Friction, 0.01f, 1f);
			}
		}
		float num2 = 0.163f / (num * num * num);
		Move(_direction, _isDirAbsolute, onGround ? (0.1f * num2) : 0.02f, 1f);
		entityCollision(motion);
		motion *= num;
	}
}
