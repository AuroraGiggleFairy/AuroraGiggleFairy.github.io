using System.Collections.Generic;
using FullautoLauncher.Scripts.ProjectileManager;
using UnityEngine;
using UnityEngine.Rendering;
using XMLData.Item;

public class PIGSimpleMesh : ProjectileItemGroupAbs<PHSimpleMesh>
{
	private Transform renderTrans;

	private Mesh mesh;

	private RenderParams renderParams;

	private SimpleMeshTransformData data;

	public PIGSimpleMesh(ItemClass item)
		: base(item)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		renderTrans = CloneModelAccess.TransformOf(item, new ItemValue(((ItemData)item).Id, false), (Transform)null);
		MeshFilter componentInChildren = ((Component)renderTrans).GetComponentInChildren<MeshFilter>();
		mesh = componentInChildren.sharedMesh;
		MeshRenderer componentInChildren2 = ((Component)renderTrans).GetComponentInChildren<MeshRenderer>();
		RenderParams val = default(RenderParams);
		((RenderParams)(ref val))._002Ector(((Renderer)componentInChildren2).material);
		((RenderParams)(ref val)).layer = ((Component)componentInChildren2).gameObject.layer;
		((RenderParams)(ref val)).lightProbeUsage = (LightProbeUsage)0;
		((RenderParams)(ref val)).shadowCastingMode = (ShadowCastingMode)0;
		((RenderParams)(ref val)).rendererPriority = ((Renderer)componentInChildren2).rendererPriority;
		((RenderParams)(ref val)).renderingLayerMask = ((Renderer)componentInChildren2).renderingLayerMask;
		((RenderParams)(ref val)).motionVectorMode = (MotionVectorGenerationMode)2;
		renderParams = val;
		data = new SimpleMeshTransformData(((Renderer)componentInChildren2).localToWorldMatrix, mesh.bounds);
	}

	public override void Update()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		foreach (HashSet<PHSimpleMesh> value in dict_fired_projectiles.Values)
		{
			foreach (PHSimpleMesh item in value)
			{
				Graphics.RenderMesh(ref item.renderParams, mesh, 0, item.finalMat, (Matrix4x4?)null);
			}
		}
	}

	protected override PHSimpleMesh Create(ProjectileParams par)
	{
		return new PHSimpleMesh(par, data, in renderParams);
	}
}
