using System;
using UnityEngine;

public class vp_AngleBob : MonoBehaviour
{
	public Vector3 BobAmp = new Vector3(0f, 0.1f, 0f);

	public Vector3 BobRate = new Vector3(0f, 4f, 0f);

	public float YOffset;

	public bool RandomizeBobOffset;

	public bool LocalMotion;

	public bool FadeToTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_InitialRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Offset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Transform = base.transform;
		m_InitialRotation = m_Transform.eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		m_Transform.eulerAngles = m_InitialRotation;
		if (RandomizeBobOffset)
		{
			YOffset = UnityEngine.Random.value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (BobRate.x != 0f && BobAmp.x != 0f)
		{
			m_Offset.x = vp_MathUtility.Sinus(BobRate.x, BobAmp.x);
		}
		if (BobRate.y != 0f && BobAmp.y != 0f)
		{
			m_Offset.y = vp_MathUtility.Sinus(BobRate.y, BobAmp.y);
		}
		if (BobRate.z != 0f && BobAmp.z != 0f)
		{
			m_Offset.z = vp_MathUtility.Sinus(BobRate.z, BobAmp.z);
		}
		if (!LocalMotion)
		{
			if (FadeToTarget)
			{
				m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, Quaternion.Euler(m_InitialRotation + m_Offset + Vector3.up * YOffset), Time.deltaTime);
			}
			else
			{
				m_Transform.eulerAngles = m_InitialRotation + m_Offset + Vector3.up * YOffset;
			}
		}
		else
		{
			m_Transform.eulerAngles = m_InitialRotation + Vector3.up * YOffset;
			m_Transform.localEulerAngles += m_Transform.TransformDirection(m_Offset);
		}
	}
}
