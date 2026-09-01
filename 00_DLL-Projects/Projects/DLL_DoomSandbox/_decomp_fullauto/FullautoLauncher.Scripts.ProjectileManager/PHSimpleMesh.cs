using UnityEngine;

namespace FullautoLauncher.Scripts.ProjectileManager;

public class PHSimpleMesh : ParameterHolderAbs
{
	public Matrix4x4 finalMat;

	public RenderParams renderParams;

	private SimpleMeshTransformData data;

	public PHSimpleMesh(ProjectileParams par, SimpleMeshTransformData data, in RenderParams renderPar)
		: base(par)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		this.data = data;
		renderParams = renderPar;
	}

	public override void Fire()
	{
		UpdatePosition();
	}

	public override void UpdatePosition()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		finalMat = Matrix4x4.Translate(par.renderPosition - Origin.position) * Matrix4x4.Rotate(Quaternion.LookRotation(par.moveDir)) * data.TRS;
		Bounds bounds = data.bounds;
		((Bounds)(ref bounds)).center = ((Matrix4x4)(ref finalMat)).MultiplyPoint3x4(((Bounds)(ref bounds)).center);
		((RenderParams)(ref renderParams)).worldBounds = bounds;
	}
}
