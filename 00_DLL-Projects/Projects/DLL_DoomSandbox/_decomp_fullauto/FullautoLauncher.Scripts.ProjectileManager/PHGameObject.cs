using System;
using UnityEngine;

namespace FullautoLauncher.Scripts.ProjectileManager;

public class PHGameObject : ParameterHolderAbs, IDisposable
{
	public Transform Transform { get; }

	public PHGameObject(Transform transform, ProjectileParams par)
		: base(par)
	{
		Transform = transform;
	}

	public override void Fire()
	{
		Transform.parent = null;
		((Component)Transform).gameObject.SetActive(true);
		UpdatePosition();
	}

	public override void UpdatePosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = par.renderPosition - Origin.position;
		Transform.position = val;
		Transform.LookAt(val + par.moveDir);
	}

	public void Dispose()
	{
		Object.Destroy((Object)(object)((Component)Transform).gameObject);
	}
}
