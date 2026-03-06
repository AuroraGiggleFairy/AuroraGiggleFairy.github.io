using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class vp_RigidbodyImpulse_random : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_Rigidbody;

	public float minRigidBodySpin = 0.2f;

	public float maxRigidBodySpin = 0.2f;

	public Vector3 minForce = new Vector3(0f, 0f, 0f);

	public Vector3 maxForce = new Vector3(0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (!(m_Rigidbody == null))
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(minForce.x, maxForce.x), UnityEngine.Random.Range(minForce.y, maxForce.y), UnityEngine.Random.Range(minForce.z, maxForce.z));
			float num = UnityEngine.Random.Range(minRigidBodySpin, maxRigidBodySpin);
			if (vector != Vector3.zero)
			{
				m_Rigidbody.AddForce(vector, ForceMode.Impulse);
			}
			if (num != 0f)
			{
				m_Rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles * num);
			}
		}
	}
}
