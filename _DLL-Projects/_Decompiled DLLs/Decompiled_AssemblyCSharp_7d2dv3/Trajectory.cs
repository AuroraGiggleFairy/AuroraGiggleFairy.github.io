using System;
using UnityEngine;

public static class Trajectory
{
	public struct AimTrajectory
	{
		public Vector3 Direction;

		public Vector3 LaunchVelocity;

		public float Angle;

		public float Time;
	}

	public static int Calculate(Vector3 source, Vector3 target, float projectileSpeed, float gravity, Span<AimTrajectory> trajectories)
	{
		Span<float> span = stackalloc float[2];
		float magnitude = (new Vector2(target.x, target.z) - new Vector2(source.x, source.z)).magnitude;
		float num = target.y - source.y;
		float num2 = 4f * num * num + 4f * magnitude * magnitude;
		float num3 = -4f * projectileSpeed * projectileSpeed - 4f * num * gravity;
		float num4 = gravity * gravity;
		float num5 = num3 * num3 - 4f * num2 * num4;
		if (num5 <= 0f || num2 == 0f)
		{
			return 0;
		}
		float num6 = Mathf.Sqrt(num5);
		float num7 = 0.5f / num2;
		span[0] = (0f - num3 + num6) * num7;
		span[1] = (0f - num3 - num6) * num7;
		int num8 = 0;
		for (int i = 0; i < 2; i++)
		{
			if (!(span[i] <= 0f))
			{
				num5 = Mathf.Sqrt(span[i]);
				trajectories[num8].Angle = Mathf.Atan2(0.5f * (2f * num * span[i] - gravity) / num5, num5 * magnitude);
				trajectories[num8].Time = magnitude / (Mathf.Cos(trajectories[num8].Angle) * projectileSpeed);
				Vector3 vector = target;
				Vector3 vector2 = vector - source;
				vector.y = source.y + Mathf.Tan(trajectories[num8].Angle) * new Vector2(vector2.x, vector2.z).magnitude;
				trajectories[num8].Direction = vector - source;
				trajectories[num8].Direction.Normalize();
				trajectories[num8].LaunchVelocity = trajectories[num8].Direction * projectileSpeed;
				num8++;
			}
		}
		return num8;
	}

	public static bool SuggestVelocity_CustomArc(out Vector3 launchVelocity, Vector3 source, Vector3 target, float projectileGravity, float arcParam = 0.5f)
	{
		Vector3 value = target - source;
		float magnitude;
		Vector3 b = value.NormalizeReturnMagnitude(out magnitude);
		if (magnitude > float.Epsilon)
		{
			Vector3 normalized = Vector3.Lerp(Vector3.up, b, arcParam).normalized;
			float magnitude2 = new Vector2(value.x, value.z).magnitude;
			float y = value.y;
			float f = Mathf.Atan2(value.y, magnitude2) * 57.29578f;
			float num = Mathf.Cos(f);
			float num2 = projectileGravity * (magnitude2 * magnitude2) * 0.5f;
			float num3 = (y - magnitude2 * Mathf.Tan(f)) * num;
			float num4 = num2 / num3;
			if (num4 >= 0f)
			{
				float num5 = Mathf.Sqrt(num4);
				launchVelocity = normalized * num5;
				return true;
			}
		}
		launchVelocity = Vector3.zero;
		return false;
	}
}
