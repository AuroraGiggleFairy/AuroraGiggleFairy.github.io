using System;
using UnityEngine;

public class DismemberedPart : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody[] rigidbodies;

	public Vector3 initialForce;

	public bool useRandomForce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float halfMass;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHangTime = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float hangTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Vector3 zero = Vector3.zero;
		if (useRandomForce)
		{
			World world = GameManager.Instance.World;
			zero.x += world.RandomRange(-0.8f, 0.8f);
			zero.y += world.RandomRange(0f, 0.8f);
			zero.z += world.RandomRange(-0.8f, 0.8f);
		}
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			rigidbodies[i].AddForce(zero, ForceMode.Impulse);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!(hangTime >= 0f))
		{
			return;
		}
		hangTime -= Time.deltaTime;
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			Rigidbody obj = rigidbodies[i];
			obj.mass = Mathf.Lerp(obj.mass, 0.5f, hangTime / 0.1f);
		}
		if (DismembermentManager.DebugBulletTime)
		{
			Time.timeScale = Mathf.Lerp(0.25f, 1f, 1f - hangTime / 0.5f);
			if (hangTime <= 0f)
			{
				Time.timeScale = 1f;
			}
		}
	}
}
