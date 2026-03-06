using System;
using UnityEngine;

public class vp_PulsingLight : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light m_Light;

	public float m_MinIntensity = 2f;

	public float m_MaxIntensity = 5f;

	public float m_Rate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_Light = GetComponent<Light>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!(m_Light == null))
		{
			m_Light.intensity = m_MinIntensity + Mathf.Abs(Mathf.Cos(Time.time * m_Rate) * (m_MaxIntensity - m_MinIntensity));
		}
	}
}
