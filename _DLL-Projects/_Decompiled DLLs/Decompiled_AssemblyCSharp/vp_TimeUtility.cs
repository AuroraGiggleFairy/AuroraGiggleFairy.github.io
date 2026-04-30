using UnityEngine;

public static class vp_TimeUtility
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_MinTimeScale = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_MaxTimeScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool m_Paused = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_TimeScaleOnPause = 1f;

	public static float InitialFixedTimeStep = Time.fixedDeltaTime;

	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			value = ClampTimeScale(value);
			Time.timeScale = value;
			Time.fixedDeltaTime = InitialFixedTimeStep * Time.timeScale;
		}
	}

	public static float AdjustedTimeScale => 1f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));

	public static bool Paused
	{
		get
		{
			return m_Paused;
		}
		set
		{
			if (value)
			{
				if (!m_Paused)
				{
					m_Paused = true;
					m_TimeScaleOnPause = Time.timeScale;
					Time.timeScale = 0f;
				}
			}
			else if (m_Paused)
			{
				m_Paused = false;
				Time.timeScale = m_TimeScaleOnPause;
				m_TimeScaleOnPause = 1f;
			}
		}
	}

	public static void FadeTimeScale(float targetTimeScale, float fadeSpeed)
	{
		if (TimeScale != targetTimeScale)
		{
			targetTimeScale = ClampTimeScale(targetTimeScale);
			TimeScale = Mathf.Lerp(TimeScale, targetTimeScale, Time.deltaTime * 60f * fadeSpeed);
			if (Mathf.Abs(TimeScale - targetTimeScale) < 0.01f)
			{
				TimeScale = targetTimeScale;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ClampTimeScale(float t)
	{
		if (t < m_MinTimeScale || t > m_MaxTimeScale)
		{
			t = Mathf.Clamp(t, m_MinTimeScale, m_MaxTimeScale);
			Debug.LogWarning("Warning: (vp_TimeUtility) TimeScale was clamped to within the supported range (" + m_MinTimeScale.ToCultureInvariantString() + " - " + m_MaxTimeScale.ToCultureInvariantString() + ").");
		}
		return t;
	}
}
