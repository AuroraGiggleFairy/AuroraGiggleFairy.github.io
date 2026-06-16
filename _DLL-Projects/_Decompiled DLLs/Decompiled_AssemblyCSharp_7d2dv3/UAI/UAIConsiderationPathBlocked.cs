using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationPathBlocked : UAIConsiderationBase
{
	public override float GetScore(Context _context, object target)
	{
		Vector3i attackPos = Vector3i.zero;
		if (IsPathUsageBlocked(_context) && CanAttackBlocks(_context.Self, out attackPos))
		{
			_context.ConsiderationData.WaypointTargets.Add(attackPos.ToVector3());
			return 1f;
		}
		return 0f;
	}

	public bool IsPathUsageBlocked(Context _context)
	{
		EntityAlive self = _context.Self;
		if (self.getNavigator() == null)
		{
			return false;
		}
		if (self.getNavigator().getPath() == null)
		{
			return false;
		}
		Vector3 targetPos = GetTargetPos(self);
		Vector3i vector3i = World.worldToBlockPos(targetPos);
		float distanceSq = self.getNavigator().getPath().GetEndPoint()
			.GetDistanceSq(vector3i.x, vector3i.y, vector3i.z);
		if (distanceSq < 2.1f)
		{
			return false;
		}
		float distanceSq2 = self.GetDistanceSq(targetPos);
		if (self.GetDistanceSq(targetPos) >= 256f)
		{
			return distanceSq > distanceSq2;
		}
		return true;
	}

	public static bool CanAttackBlocks(EntityAlive theEntity, out Vector3i attackPos)
	{
		float attackAngle;
		BlockValue attackBlockValue;
		return CanAttackBlocks(theEntity, GetTargetYaw(theEntity), out attackAngle, out attackPos, out attackBlockValue);
	}

	public static bool CanAttackBlocks(EntityAlive theEntity, float yawAngle, out float attackAngle, out Vector3i attackAddPos, out BlockValue attackBlockValue)
	{
		int num = Utils.Fastfloor(theEntity.position.x);
		int num2 = Utils.Fastfloor(theEntity.position.y + 0.5f);
		int num3 = Utils.Fastfloor(theEntity.position.z);
		attackAddPos = new Vector3i(0, 1, 0);
		bool flag = isPosBlocked(theEntity, num, num2 + 1, num3, out attackBlockValue);
		attackAngle = 0f;
		if (!flag)
		{
			attackAddPos = new Vector3i(0, 0, 0);
			flag = isPosBlocked(theEntity, num, num2, num3, out attackBlockValue);
			attackAngle = -65f;
		}
		if (!flag)
		{
			int num4 = 0;
			int num5 = 0;
			float f = 0f - Mathf.Sin(yawAngle * 0.0175f - MathF.PI);
			float f2 = 0f - Mathf.Cos(yawAngle * 0.0175f - MathF.PI);
			if (Mathf.Abs(f) > 0.1f)
			{
				num4 = (int)Mathf.Sign(f);
			}
			if (Mathf.Abs(f2) > 0.1f)
			{
				num5 = (int)Mathf.Sign(f2);
			}
			attackAddPos = new Vector3i(num4, 1, num5);
			flag = isPosBlocked(theEntity, num + num4, num2 + 1, num3 + num5, out attackBlockValue);
			attackAngle = 0f;
			if (!flag)
			{
				attackAddPos = new Vector3i(num4, 0, num5);
				flag = isPosBlocked(theEntity, num + num4, num2, num3 + num5, out attackBlockValue);
				attackAngle = -45f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(2 * num4, 1, 2 * num5);
				flag = isPosBlocked(theEntity, num + 2 * num4, num2 + 1, num3 + 2 * num5, out attackBlockValue);
				attackAngle = 0f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(2 * num4, 0, 2 * num5);
				flag = isPosBlocked(theEntity, num + 2 * num4, num2, num3 + 2 * num5, out attackBlockValue);
				attackAngle = -45f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(num4, 0, 0);
				flag = isPosBlocked(theEntity, num + num4, num2, num3, out attackBlockValue);
				attackAngle = -45f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(2 * num4, 1, 0);
				flag = isPosBlocked(theEntity, num + 2 * num4, num2 + 1, num3, out attackBlockValue);
				attackAngle = 0f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(0, 0, num5);
				flag = isPosBlocked(theEntity, num, num2, num3 + num5, out attackBlockValue);
				attackAngle = -45f;
			}
			if (!flag)
			{
				attackAddPos = new Vector3i(0, 1, 2 * num5);
				flag = isPosBlocked(theEntity, num, num2 + 1, num3 + 2 * num5, out attackBlockValue);
				attackAngle = 0f;
			}
		}
		attackAddPos += new Vector3i(num, num2, num3);
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isPosBlocked(EntityAlive theEntity, int _x, int _y, int _z, out BlockValue attackBlockValue)
	{
		attackBlockValue = theEntity.world.GetBlock(_x, _y, _z);
		return attackBlockValue.Block.IsMovementBlocked(theEntity.world, new Vector3i(_x, _y, _z), attackBlockValue, BlockFace.None);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetTargetYaw(EntityAlive theEntity)
	{
		if (theEntity.GetAttackTarget() != null)
		{
			return theEntity.YawForTarget(theEntity.GetAttackTarget());
		}
		return theEntity.YawForTarget(GetTargetPos(theEntity));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector3 GetTargetPos(EntityAlive theEntity)
	{
		if (theEntity.GetAttackTarget() != null)
		{
			return theEntity.GetAttackTarget().GetPosition();
		}
		return theEntity.InvestigatePosition;
	}
}
