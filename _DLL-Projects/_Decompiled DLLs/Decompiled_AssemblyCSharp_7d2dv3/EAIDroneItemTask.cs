using System;
using System.Collections.Generic;
using GamePath;
using RaycastPathing;
using UnityEngine;

public class EAIDroneItemTask : EAIItemTask
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityDrone drone;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 currentPathTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 currentPathDest;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool weaponDischarged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackEnterTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackExitTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackEnterTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackExitTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fleeDist;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.PathTracker pathTracker = new EntityDrone.PathTracker();

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		drone = _theEntity as EntityDrone;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		GetData(data, "enterTime", ref attackEnterTime);
		GetData(data, "actionTime", ref actionTime);
		GetData(data, "exitTime", ref attackExitTime);
		attackEnterTimer = attackEnterTime;
		GetData(data, "fleeDist", ref fleeDist);
	}

	public override bool CanExecute()
	{
		if (!drone)
		{
			return false;
		}
		if (PathFinderThread.Instance == null)
		{
			return false;
		}
		return base.CanExecute();
	}

	public override bool Continue()
	{
		if (!drone)
		{
			return false;
		}
		return base.Continue();
	}

	public override void Update()
	{
		if (!drone)
		{
			return;
		}
		base.Update();
		if (attackEnterTimer > 0f)
		{
			attackEnterTimer -= 0.05f;
			if (attackEnterTimer <= 0f)
			{
				OnAttackEnter();
			}
		}
		if (actionTimer > 0f)
		{
			actionTimer -= 0.05f;
			if (actionTimer <= 0f)
			{
				OnAttackAction();
			}
		}
		if (attackExitTimer > 0f)
		{
			attackExitTimer -= 0.05f;
			if (attackExitTimer <= 0f)
			{
				OnAttackExit();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackEnter()
	{
		actionTimer = actionTime;
		if (drone.activeWeapon != null && !string.IsNullOrEmpty(drone.activeWeapon.soundWeaponUseBark))
		{
			drone.PlayVO(drone.activeWeapon.soundWeaponUseBark, _hasPriority: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackAction()
	{
		attackExitTimer = attackExitTime;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackExit()
	{
		attackEnterTimer = attackEnterTime;
		actionTimer = 0f;
		attackExitTimer = 0f;
		drone.SetRevengeTarget(null);
		drone.SetAttackTarget(null, 0);
		weaponDischarged = false;
		ClearPath();
		drone.SetState((drone.OrderState == EntityDrone.Orders.Stay) ? EntityDrone.State.Sentry : EntityDrone.State.Idle, sync: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool GetPath(Vector3 seekPos, float seekDist, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		if (EntityDrone.GetPath(currentPath, drone, drone.position, seekPos, drone.SpeedFlying, this, seekDist, pointRadius, debugDraw, duration))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] GetGroupPositions(EntityAlive target, float dist, bool debugDraw = false, float duration = 0f)
	{
		return EntityDrone.GetGroupPositions(target, dist, debugDraw, duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 1073807360, bool debugDraw = false, float duration = 0f)
	{
		return EntityDrone.IsPositionBlocked(start, end, layerMask, debugDraw);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ClearPath()
	{
		currentPath.Clear();
		drone.OnPathInterupted();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FollowPlannedPath(float speed, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		if (currentPath.Count <= 0)
		{
			return;
		}
		currentPathTarget = currentPath[0];
		drone.RotateTo((currentPathTarget - drone.position).normalized);
		drone.Move(currentPathTarget, pointRadius);
		if (drone.IsInRange(currentPathTarget, pointRadius))
		{
			currentPath.RemoveAt(0);
			return;
		}
		if (debugDraw && currentPath.Count > 1)
		{
			RaycastPathUtils.DrawLine(currentPath[0], currentPath[1], Color.green);
		}
		if (pathTracker.IsStuck(drone.position, currentPath[0]))
		{
			if (currentPath.Count > 1)
			{
				drone.TeleportToPosition(currentPath[1]);
				currentPath.RemoveRange(0, 2);
			}
			else
			{
				drone.TeleportToPosition(currentPath[0]);
				currentPath.RemoveAt(0);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool DoMoveIntoAtkPos(EntityAlive avaliableTarget, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 position = drone.position;
		Vector3 chestPosition = avaliableTarget.getChestPosition();
		Vector3 normalized = (chestPosition - position).normalized;
		bool flag = Vector3.Dot(seekForward, normalized) >= 0.9f;
		float magnitude = (chestPosition - position).magnitude;
		Utils.DrawCircleLinesHorzontal(position - Origin.position, seekDist, Color.white, Color.red, 24, 0.05f);
		if (currentPath.Count == 0)
		{
			Vector3[] groupPositions = GetGroupPositions(avaliableTarget, 1.414f);
			Array.Sort(groupPositions, [PublicizedFrom(EAccessModifier.Internal)] (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
			Vector3 seekPos = groupPositions[0];
			GetPath(seekPos, seekDist, pointRadius, debugDraw, duration);
			if (currentPath.Count > 0)
			{
				currentPathDest = currentPath[currentPath.Count - 1];
			}
			else
			{
				currentPathDest = seekPos;
			}
		}
		else
		{
			if ((chestPosition - currentPathDest).magnitude > seekDist)
			{
				ClearPath();
			}
			if (IsPositionBlocked(position, chestPosition) || (float)currentPath.Count > seekDist + 1f || (currentPath.Count > 0 && magnitude > seekDist + 1.414f))
			{
				FollowPlannedPath(drone.SpeedFlying, pointRadius);
			}
			else if (magnitude >= seekDist)
			{
				drone.RotateTo(normalized);
				drone.Move(chestPosition, pointRadius);
			}
		}
		if (!IsPositionBlocked(position, chestPosition) && magnitude <= seekDist)
		{
			drone.RotateTo(normalized);
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool DoFleeFromTargetEntity(EntityAlive avaliableTarget, float seekDist, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 position = drone.position;
		avaliableTarget.getChestPosition();
		if (currentPath.Count == 0)
		{
			Vector3[] groupPositions = GetGroupPositions(avaliableTarget, (fleeDist > 0f) ? fleeDist : 10f);
			Vector3 seekPos = groupPositions[drone.rand.RandomRange(0, groupPositions.Length)];
			GetPath(seekPos, seekDist, pointRadius, debugDraw, duration);
			if (currentPath.Count > 0)
			{
				currentPathDest = currentPath[currentPath.Count - 1];
			}
			else
			{
				currentPathDest = seekPos;
			}
		}
		else
		{
			bool flag = !EntityDrone.IsPositionBlocked(position, currentPathDest, 1073807360);
			float magnitude = (currentPathDest - position).magnitude;
			if (magnitude > 5f || !flag)
			{
				FollowPlannedPath(drone.SpeedFlying, pointRadius);
			}
			else
			{
				drone.RotateTo((currentPathDest - position).normalized);
				drone.Move(currentPathDest, pointRadius);
			}
			if (magnitude <= seekDist)
			{
				return true;
			}
		}
		return false;
	}
}
