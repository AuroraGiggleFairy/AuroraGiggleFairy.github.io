using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Terrain/Public/TerrainData.h")]
[NativeHeader("Modules/TerrainPhysics/TerrainCollider.h")]
public class TerrainCollider : Collider
{
	public extern TerrainData terrainData
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	private RaycastHit Raycast(Ray ray, float maxDistance, bool hitHoles, ref bool hasHit)
	{
		Raycast_Injected(ref ray, maxDistance, hitHoles, ref hasHit, out var ret);
		return ret;
	}

	internal bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, bool hitHoles)
	{
		bool hasHit = false;
		hitInfo = Raycast(ray, maxDistance, hitHoles, ref hasHit);
		return hasHit;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Raycast_Injected(ref Ray ray, float maxDistance, bool hitHoles, ref bool hasHit, out RaycastHit ret);
}
