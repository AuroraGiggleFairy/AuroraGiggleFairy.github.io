using System;
using System.Threading;
using UnityEngine;

public class WaitForTargetFPS : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetFPS = 20;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timePerFrame;

	public bool SkipSleepThisFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float sleepLastFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	public int TargetFPS
	{
		get
		{
			return m_targetFPS;
		}
		set
		{
			if (value != m_targetFPS)
			{
				m_targetFPS = value;
				timePerFrame = 1f / (float)m_targetFPS;
				base.enabled = m_targetFPS > 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		timePerFrame = 1f / (float)TargetFPS;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		float num = 0f;
		float num2 = Time.realtimeSinceStartup - lastUpdateTime;
		lastUpdateTime = Time.realtimeSinceStartup;
		if (!SkipSleepThisFrame)
		{
			float num3 = num2 - sleepLastFrame;
			if (num3 < timePerFrame)
			{
				num = Math.Min(timePerFrame - num3, 1f);
				Thread.Sleep((int)(num * 1000f));
			}
		}
		sleepLastFrame = num;
		SkipSleepThisFrame = false;
	}
}
