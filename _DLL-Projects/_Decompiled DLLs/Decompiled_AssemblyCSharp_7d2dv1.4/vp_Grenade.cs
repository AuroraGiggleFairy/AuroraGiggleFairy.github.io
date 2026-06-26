using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(vp_DamageHandler))]
public class vp_Grenade : MonoBehaviour
{
	public float LifeTime = 3f;

	public float RigidbodyForce = 10f;

	public float RigidbodySpin;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_Rigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Source;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_OriginalSource;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		vp_Timer.In(LifeTime, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			base.transform.SendMessage("DieBySources", new Transform[2] { m_Source, m_OriginalSource }, SendMessageOptions.DontRequireReceiver);
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (!(m_Rigidbody == null))
		{
			if (RigidbodyForce != 0f)
			{
				m_Rigidbody.AddForce(base.transform.forward * RigidbodyForce, ForceMode.Impulse);
			}
			if (RigidbodySpin != 0f)
			{
				m_Rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles * RigidbodySpin);
			}
		}
	}

	public void SetSource(Transform source)
	{
		m_Source = base.transform;
		m_OriginalSource = source;
	}
}
