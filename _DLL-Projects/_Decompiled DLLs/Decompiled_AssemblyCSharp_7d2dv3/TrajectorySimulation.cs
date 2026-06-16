using System;
using System.Collections.Generic;
using UnityEngine;

public class TrajectorySimulation
{
	public enum State
	{
		Init,
		Invalid,
		ReadyToSimulate,
		Simulating,
		DoneWithSolution,
		DoneWithoutSolution
	}

	public struct ProjectileInfo
	{
		public float ProjectileSpeed;

		public float ProjectileGravity;

		public float ProjectileRadius;

		public float ProjectileTargetRadius;
	}

	public State TrajectoryState;

	public readonly EntityAlive Owner;

	public readonly EntityAlive Target;

	public readonly float MaxSimTime;

	public readonly ProjectileInfo Projectile;

	public readonly float RenderDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<TrajectorySimulation> OnSimulationComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LaunchPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 TargetPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Trajectory.AimTrajectory[] SimTrajectories = new Trajectory.AimTrajectory[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public int SimTrajectoryIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int SimTrajectoryCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float SimTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimCurrentPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Plane CullPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxOwnerDelta = 16f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxTargetDelta = 16f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSimInterval = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Queue<TrajectorySimulation> Trajectories = new Queue<TrajectorySimulation>();

	public bool TryGetSolution(out Trajectory.AimTrajectory trajectory)
	{
		if (TrajectoryState == State.DoneWithSolution)
		{
			trajectory = SimTrajectories[SimTrajectoryIndex];
			return true;
		}
		trajectory = default(Trajectory.AimTrajectory);
		return false;
	}

	public TrajectorySimulation(EntityAlive owner, EntityAlive target, float maxSimTime, ProjectileInfo projectile, float renderDuration, Action<TrajectorySimulation> onSimulationComplete)
	{
		TrajectoryState = State.ReadyToSimulate;
		Owner = owner;
		Target = target;
		MaxSimTime = maxSimTime;
		Projectile = projectile;
		RenderDuration = renderDuration;
		LaunchPosition = Owner.position;
		TargetPosition = Target.position;
		OnSimulationComplete = onSimulationComplete;
		CullPlane = new Plane(Vector3.up, TargetPosition - Vector3.up * Projectile.ProjectileTargetRadius);
	}

	public bool IsValid()
	{
		if (TrajectoryState == State.Invalid)
		{
			return false;
		}
		if (Owner != null && Target != null)
		{
			Vector3 position = Owner.transform.position;
			Vector3 position2 = Target.transform.position;
			float sqrMagnitude = (position - LaunchPosition).sqrMagnitude;
			float sqrMagnitude2 = (position2 - TargetPosition).sqrMagnitude;
			if (sqrMagnitude > 16f)
			{
				TrajectoryState = State.Invalid;
				return false;
			}
			if (sqrMagnitude2 > 16f)
			{
				TrajectoryState = State.Invalid;
				return false;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickSimulation(int maxIterations = 20)
	{
		if (Owner == null || Target == null)
		{
			TrajectoryState = State.Invalid;
			OnSimulationComplete(this);
			return;
		}
		if (TrajectoryState == State.ReadyToSimulate)
		{
			LaunchPosition = Owner.getHeadPosition();
			TargetPosition = Target.position;
			SimTrajectoryIndex = 0;
			SimTrajectoryCount = Trajectory.Calculate(LaunchPosition, TargetPosition, Projectile.ProjectileSpeed, Projectile.ProjectileGravity, SimTrajectories);
			if (SimTrajectoryCount == 0)
			{
				TrajectoryState = State.DoneWithoutSolution;
				OnSimulationComplete(this);
				return;
			}
			SimTime = 0f;
			SimVelocity = SimTrajectories[SimTrajectoryIndex].LaunchVelocity;
			SimCurrentPos = (SimLastPos = Owner.getHeadPosition());
			TrajectoryState = State.Simulating;
		}
		if (SimTrajectoryIndex >= SimTrajectoryCount)
		{
			TrajectoryState = State.DoneWithoutSolution;
			OnSimulationComplete(this);
		}
		else
		{
			if (TrajectoryState != State.Simulating)
			{
				return;
			}
			World world = GameManager.Instance.World;
			while (SimTime < MaxSimTime)
			{
				if (maxIterations <= 0)
				{
					return;
				}
				if (CullPlane.GetDistanceToPoint(SimCurrentPos) < 0f)
				{
					break;
				}
				if (SimVelocity.magnitude < 1f)
				{
					if (RenderDuration > 0f)
					{
						Utils.DrawBounds(new Bounds(SimCurrentPos, Vector3.one * Projectile.ProjectileRadius), Color.magenta, RenderDuration);
					}
					break;
				}
				Vector3 vector = SimVelocity * 0.05f;
				SimCurrentPos += vector;
				SimVelocity.y += Projectile.ProjectileGravity * 0.05f;
				if (RenderDuration > 0f)
				{
					Debug.DrawLine(SimLastPos, SimCurrentPos, Color.green, RenderDuration);
				}
				float magnitude;
				Vector3 vector2 = vector.NormalizeReturnMagnitude(out magnitude);
				Ray ray = new Ray(SimCurrentPos, vector2);
				if (Voxel.Raycast(world, ray, magnitude, 1082195968, 8191, Projectile.ProjectileRadius))
				{
					if (Voxel.phyxRaycastHit.distance == 0f)
					{
						if (RenderDuration > 0f)
						{
							Utils.DrawBounds(new Bounds(Voxel.phyxRaycastHit.point, Vector3.one * Projectile.ProjectileRadius), Color.red, RenderDuration);
						}
						break;
					}
					SimCurrentPos += ray.GetPoint(Voxel.phyxRaycastHit.distance);
					SimVelocity = Vector3.Reflect(SimVelocity, Voxel.phyxRaycastHit.normal);
					SimVelocity *= 0.25f;
					float num = magnitude - Voxel.phyxRaycastHit.distance;
					if (num > 0f)
					{
						SimCurrentPos = Voxel.phyxRaycastHit.point + Vector3.Reflect(vector2, Voxel.phyxRaycastHit.normal) * num;
					}
					if (RenderDuration > 0f)
					{
						Debug.DrawRay(Voxel.phyxRaycastHit.point, Voxel.phyxRaycastHit.normal * 0.2f, Color.yellow, RenderDuration);
					}
					if (Vector3.Dot(Voxel.phyxRaycastHit.normal, Vector3.up) > 0.707f)
					{
						break;
					}
				}
				SimLastPos = SimCurrentPos;
				SimTime += 0.05f;
				maxIterations--;
			}
			if (MathUtils.DistanceToSegment(SimCurrentPos, TargetPosition, Target.getChestPosition()) < Projectile.ProjectileTargetRadius)
			{
				TrajectoryState = State.DoneWithSolution;
				if (RenderDuration > 0f)
				{
					Utils.DrawBounds(new Bounds(SimCurrentPos, new Vector3(1f, 0f, 1f) * Projectile.ProjectileRadius), Color.blue, RenderDuration);
				}
				OnSimulationComplete(this);
				return;
			}
			SimTrajectoryIndex++;
			if (SimTrajectoryIndex < SimTrajectoryCount)
			{
				SimTime = 0f;
				SimVelocity = SimTrajectories[SimTrajectoryIndex].LaunchVelocity;
				SimCurrentPos = (SimLastPos = Owner.getHeadPosition());
				TrajectoryState = State.Simulating;
			}
			else
			{
				TrajectoryState = State.DoneWithoutSolution;
				OnSimulationComplete(this);
			}
		}
	}

	public static void Cleanup()
	{
		Trajectories.Clear();
	}

	public static void Queue(TrajectorySimulation trajectory)
	{
		if (!Trajectories.Contains(trajectory))
		{
			Trajectories.Enqueue(trajectory);
		}
		trajectory.TrajectoryState = State.ReadyToSimulate;
	}

	public static void UpdateSimulationQueue()
	{
		if (Trajectories.TryPeek(out var result))
		{
			result.TickSimulation();
			if (result.TrajectoryState != State.Simulating)
			{
				Trajectories.Dequeue();
			}
		}
	}
}
