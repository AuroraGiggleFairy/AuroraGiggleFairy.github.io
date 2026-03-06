using UnityEngine;

public class WorldRayHitInfo
{
	public Ray ray;

	public bool bHitValid;

	public string tag;

	public Transform transform;

	public HitInfoDetails hit;

	public HitInfoDetails fmcHit;

	public Vector3i lastBlockPos;

	public Collider hitCollider;

	public int hitTriangleIdx;

	public virtual void Clear()
	{
		ray = new Ray(Vector3.zero, Vector3.zero);
		bHitValid = false;
		tag = string.Empty;
		transform = null;
		lastBlockPos = Vector3i.zero;
		hit.Clear();
		fmcHit.Clear();
		hitCollider = null;
		hitTriangleIdx = 0;
	}

	public virtual void CopyFrom(WorldRayHitInfo _other)
	{
		ray = _other.ray;
		bHitValid = _other.bHitValid;
		tag = _other.tag;
		transform = _other.transform;
		lastBlockPos = _other.lastBlockPos;
		hit.CopyFrom(_other.hit);
		fmcHit.CopyFrom(_other.fmcHit);
		hitCollider = _other.hitCollider;
		hitTriangleIdx = _other.hitTriangleIdx;
	}

	public virtual WorldRayHitInfo Clone()
	{
		return new WorldRayHitInfo
		{
			ray = ray,
			bHitValid = bHitValid,
			tag = tag,
			transform = transform,
			lastBlockPos = lastBlockPos,
			hit = hit.Clone(),
			fmcHit = fmcHit.Clone(),
			hitCollider = hitCollider,
			hitTriangleIdx = hitTriangleIdx
		};
	}
}
