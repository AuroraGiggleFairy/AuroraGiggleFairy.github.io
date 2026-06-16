using UnityEngine;

public class BlendTimer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] m_value = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] m_time = new float[2];

	public float Value => m_value[0];

	public float Target => m_value[2];

	public BlendTimer()
		: this(1f)
	{
	}

	public BlendTimer(float initialValue)
	{
		m_time[0] = (m_time[1] = 0f);
		m_value[0] = (m_value[1] = (m_value[2] = initialValue));
	}

	public void Tick(float dt)
	{
		if (m_time[1] != 0f)
		{
			m_time[0] += dt;
			if (m_time[0] >= m_time[1])
			{
				m_value[0] = m_value[2];
				m_time[1] = 0f;
			}
			else
			{
				m_value[0] = Mathf.Lerp(m_value[1], m_value[2], m_time[0] / m_time[1]);
			}
		}
	}

	public void BlendTo(float value, float time)
	{
		if (time > 0f)
		{
			m_value[1] = m_value[0];
			m_value[2] = value;
			m_time[0] = 0f;
			m_time[1] = time;
		}
		else
		{
			m_value[0] = (m_value[1] = (m_value[2] = value));
			m_time[1] = 0f;
		}
	}

	public void BlendToRate(float value, float unitsPerSecond)
	{
		BlendTo(value, Mathf.Abs(value - m_value[0]) / unitsPerSecond);
	}
}
