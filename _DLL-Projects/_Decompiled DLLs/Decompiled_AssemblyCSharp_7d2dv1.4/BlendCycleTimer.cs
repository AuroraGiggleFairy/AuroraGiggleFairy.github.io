public class BlendCycleTimer
{
	public enum Dir
	{
		In,
		Hold,
		Out,
		Done
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlendTimer m_blendTimer = new BlendTimer(0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_inTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_outTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_holdTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_time;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dir m_dir;

	public Dir Direction => m_dir;

	public float Value => m_blendTimer.Value;

	public BlendCycleTimer(float inTime, float holdTime, float outTime)
	{
		m_inTime = inTime;
		m_outTime = outTime;
		m_holdTime = holdTime;
		m_time = 0f;
		m_dir = Dir.Done;
	}

	public void Tick(float dt)
	{
		m_blendTimer.Tick(dt);
		switch (m_dir)
		{
		case Dir.In:
			m_time += dt;
			if (m_time >= m_inTime)
			{
				m_dir = Dir.Hold;
				m_time = 0f;
			}
			break;
		case Dir.Out:
			m_time += dt;
			if (m_time >= m_outTime)
			{
				m_dir = Dir.Done;
			}
			break;
		case Dir.Hold:
			if (m_holdTime != -1f)
			{
				m_time += dt;
				if (m_time >= m_holdTime)
				{
					m_dir = Dir.Out;
					m_time = 0f;
					m_blendTimer.BlendTo(0f, m_outTime);
				}
			}
			break;
		case Dir.Done:
			break;
		}
	}

	public void FadeIn()
	{
		m_dir = Dir.In;
		m_time = Value * m_inTime;
		m_blendTimer.BlendTo(1f, m_inTime);
	}

	public void FadeOut()
	{
		m_dir = Dir.Out;
		m_time = (1f - Value) * m_outTime;
		m_blendTimer.BlendTo(0f, m_outTime);
	}

	public void Restart()
	{
		m_time = 0f;
		m_dir = Dir.In;
		m_blendTimer.BlendTo(0f, 0f);
		m_blendTimer.BlendTo(1f, m_inTime);
	}
}
