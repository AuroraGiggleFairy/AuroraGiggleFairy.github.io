using UnityEngine;

namespace WaterClippingTool;

public class WaterClippingPlanePlacer : MonoBehaviour
{
	public static readonly Plane DisabledPlane = new Plane(Vector3.up, 1000f);

	public static readonly Vector4 DisabledPlaneVec = new Vector4(0f, 1f, 0f, 1000f);

	public static readonly Vector3 DefaultModelOffset = new Vector3(1f, 0f, 1f);

	public ShapeSettings liveSettings;

	public Plane GetPlane()
	{
		return new Plane(base.transform.forward, base.transform.position);
	}
}
