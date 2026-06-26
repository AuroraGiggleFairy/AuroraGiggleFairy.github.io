using System;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Interval
	{
		public float Time;

		public float Value;
	}

	public float MinLight;

	public float MaxLight;

	public float IntervalMin;

	public float IntervalMax;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_intervalIdx;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_time;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_baseLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Interval> m_steps;

	public void Start()
	{
		Init();
	}

	public void Update()
	{
		m_time += Time.deltaTime;
		Interval interval = null;
		while (true)
		{
			interval = m_steps[m_intervalIdx];
			if (m_time < interval.Time)
			{
				break;
			}
			m_time -= interval.Time;
			m_baseLight = interval.Value;
			m_intervalIdx++;
			if (m_intervalIdx >= m_steps.Count)
			{
				m_intervalIdx = 0;
			}
		}
		GetComponent<Light>().intensity = Mathf.Lerp(m_baseLight, interval.Value, m_time / interval.Time);
	}

	public void Reset()
	{
		MinLight = 0.1f;
		MaxLight = 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		m_intervalIdx = 0;
		m_time = 0f;
		m_steps = new List<Interval>();
		m_baseLight = MinLight;
		GetComponent<Light>().intensity = m_baseLight;
		float num = 0f;
		do
		{
			Interval interval = new Interval();
			interval.Time = UnityEngine.Random.Range(Mathf.Max(0.001f, IntervalMin), Mathf.Max(0.001f, IntervalMax));
			interval.Value = UnityEngine.Random.Range(MinLight, MaxLight);
			m_steps.Add(interval);
			num += interval.Time;
		}
		while (!(num >= 5f));
	}
}
