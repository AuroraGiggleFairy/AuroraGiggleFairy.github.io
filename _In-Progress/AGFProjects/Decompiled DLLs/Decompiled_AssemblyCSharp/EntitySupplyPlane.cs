using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySupplyPlane : Entity
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksToFly;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayedSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mainCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter planeMF;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh planeMesh;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	public override bool IsDeadIfOutOfWorld()
	{
		return false;
	}

	public void SetDirectionToFly(Vector3 _directionToFly, int _ticksToFly)
	{
		ticksToFly = _ticksToFly;
		motion = _directionToFly * 6f;
		IsMovementReplicated = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveBoundsInsideFrustrum(Transform _parentT)
	{
		if ((bool)planeMesh)
		{
			float magnitude = (mainCamera.transform.position - _parentT.position).magnitude;
			Vector3 size = Vector3.one * (magnitude * 1.25f);
			Vector3 zero = Vector3.zero;
			planeMesh.bounds = new Bounds(zero, size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFarDraw()
	{
		if (!mainCamera)
		{
			mainCamera = Camera.main;
			if (!mainCamera)
			{
				return;
			}
		}
		if (!planeMesh)
		{
			planeMF = base.transform.GetComponentInChildren<MeshFilter>();
			if ((bool)planeMF)
			{
				planeMesh = planeMF.mesh;
			}
		}
		MoveBoundsInsideFrustrum(base.transform);
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		base.OnUpdatePosition(_partialTicks);
		UpdateFarDraw();
		interpolateTargetRot = 0;
		position += motion * _partialTicks;
		if (!isEntityRemote && --ticksToFly <= 0)
		{
			MarkToUnload();
		}
		if (!isPlayedSound)
		{
			Manager.Play(this, "SupplyDrops/Supply_Crate_Plane_lp");
			isPlayedSound = true;
		}
		SetAirBorne(_b: true);
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}
}
