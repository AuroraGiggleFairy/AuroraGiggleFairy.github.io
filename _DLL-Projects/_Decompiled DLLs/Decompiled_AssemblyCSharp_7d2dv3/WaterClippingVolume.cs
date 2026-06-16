using UnityEngine;

public class WaterClippingVolume
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Plane waterClipPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] intersectionPoints = new Vector3[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSliced;

	public void Prepare(Plane waterClipPlane)
	{
		this.waterClipPlane = waterClipPlane;
		isSliced = WaterClippingUtils.GetCubePlaneIntersectionEdgeLoop(waterClipPlane, ref intersectionPoints, out count);
	}

	public void ApplyClipping(ref Vector3 vertLocalPos)
	{
		if (isSliced && waterClipPlane.GetDistanceToPoint(vertLocalPos) > 0f)
		{
			vertLocalPos = waterClipPlane.ClosestPointOnPlane(vertLocalPos);
			if (!WaterClippingUtils.CubeBounds.Contains(vertLocalPos))
			{
				vertLocalPos = GeometryUtils.NearestPointOnEdgeLoop(vertLocalPos, intersectionPoints, count);
			}
		}
	}
}
