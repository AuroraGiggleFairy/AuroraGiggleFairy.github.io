using UnityEngine;

namespace FullautoLauncher.Scripts.ProjectileManager;

public class SimpleMeshTransformData
{
	public readonly Matrix4x4 TRS;

	public readonly Bounds bounds;

	public SimpleMeshTransformData(Matrix4x4 TRS, Bounds bounds)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		this.TRS = TRS;
		this.bounds = bounds;
	}
}
