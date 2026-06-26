using System;
using UnityEngine;

public class vp_Bob : MonoBehaviour
{
	public Vector3 BobAmp = new Vector3(0f, 0.1f, 0f);

	public Vector3 BobRate = new Vector3(0f, 4f, 0f);

	public float BobOffset;

	public float GroundOffset;

	public bool RandomizeBobOffset;

	public bool LocalMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_InitialPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Offset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Transform = base.transform;
		m_InitialPosition = m_Transform.position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		m_Transform.position = m_InitialPosition;
		if (RandomizeBobOffset)
		{
			BobOffset = UnityEngine.Random.value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (BobRate.x != 0f && BobAmp.x != 0f)
		{
			m_Offset.x = vp_MathUtility.Sinus(BobRate.x, BobAmp.x, BobOffset);
		}
		if (BobRate.y != 0f && BobAmp.y != 0f)
		{
			m_Offset.y = vp_MathUtility.Sinus(BobRate.y, BobAmp.y, BobOffset);
		}
		if (BobRate.z != 0f && BobAmp.z != 0f)
		{
			m_Offset.z = vp_MathUtility.Sinus(BobRate.z, BobAmp.z, BobOffset);
		}
		if (!LocalMotion)
		{
			m_Transform.position = m_InitialPosition + m_Offset + Vector3.up * GroundOffset;
			return;
		}
		m_Transform.position = m_InitialPosition + Vector3.up * GroundOffset;
		m_Transform.localPosition += m_Transform.TransformDirection(m_Offset);
	}
}
