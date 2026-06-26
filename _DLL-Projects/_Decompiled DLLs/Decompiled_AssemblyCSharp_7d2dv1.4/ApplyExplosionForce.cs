using System;
using UnityEngine;

public class ApplyExplosionForce : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpwards = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxColliders = 1024;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Collider[] colliderList = new Collider[1024];

	public static void Explode(Vector3 explosionPos, float power, float radius)
	{
		explosionPos -= Origin.position;
		power *= 20f;
		radius *= 1.75f;
		int num = Physics.OverlapSphereNonAlloc(explosionPos, radius, colliderList);
		if (num > 1024)
		{
			num = 1024;
		}
		for (int i = 0; i < num; i++)
		{
			Rigidbody component = colliderList[i].GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.AddExplosionForce(power, explosionPos, radius, 3f);
			}
		}
	}
}
