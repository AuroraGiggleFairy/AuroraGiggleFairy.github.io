using UnityEngine;

namespace ShinyScreenSpaceRaytracedReflections;

public class Rotate : MonoBehaviour
{
	public Vector3 axis = Vector3.up;

	public float speed = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		base.transform.Rotate(axis * (Time.deltaTime * speed));
	}
}
