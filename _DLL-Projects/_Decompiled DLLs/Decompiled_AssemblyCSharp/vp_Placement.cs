using UnityEngine;

public class vp_Placement
{
	public Vector3 Position = Vector3.zero;

	public Quaternion Rotation = Quaternion.identity;

	public static bool AdjustPosition(vp_Placement p, float physicsRadius, int attempts = 1000)
	{
		attempts--;
		if (attempts > 0)
		{
			if (p.IsObstructed(physicsRadius))
			{
				Vector3 insideUnitSphere = Random.insideUnitSphere;
				p.Position.x += insideUnitSphere.x;
				p.Position.z += insideUnitSphere.z;
				AdjustPosition(p, physicsRadius, attempts);
			}
			return true;
		}
		Debug.LogWarning("(vp_Placement.AdjustPosition) Failed to find valid placement.");
		return false;
	}

	public virtual bool IsObstructed(float physicsRadius = 1f)
	{
		if (Physics.CheckSphere(Position, physicsRadius, 2260992))
		{
			return true;
		}
		return false;
	}

	public static void SnapToGround(vp_Placement p, float radius, float snapDistance)
	{
		if (snapDistance != 0f)
		{
			Physics.SphereCast(new Ray(p.Position + Vector3.up * snapDistance, Vector3.down), radius, out var hitInfo, snapDistance * 2f, 1084850176);
			if (hitInfo.collider != null)
			{
				p.Position.y = hitInfo.point.y + 0.05f;
			}
		}
	}
}
