using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class vp_RigidbodyImpulse : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_Rigidbody;

	public Vector3 RigidbodyForce = new Vector3(0f, 5f, 0f);

	public float RigidbodySpin = 0.2f;

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
			if (RigidbodyForce != Vector3.zero)
			{
				m_Rigidbody.AddForce(RigidbodyForce, ForceMode.Impulse);
			}
			if (RigidbodySpin != 0f)
			{
				m_Rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles * RigidbodySpin);
			}
		}
	}
}
