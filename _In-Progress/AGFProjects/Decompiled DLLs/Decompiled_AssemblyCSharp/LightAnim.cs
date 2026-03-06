using System;
using UnityEngine;

public class LightAnim : MonoBehaviour
{
	public float Duration = 0.5f;

	public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 1f), new Keyframe(1f, 0f));

	public bool DestroyAtEnd;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light LightRef;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float intensityStart;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float time;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Duration = Utils.FastMax(Duration, 0.001f);
		LightRef = GetComponent<Light>();
		intensityStart = LightRef.intensity;
		int length = IntensityCurve.length;
		if (length > 0)
		{
			timeEnd = IntensityCurve[length - 1].time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float num = Time.deltaTime / Duration;
		time += num;
		if (time < timeEnd)
		{
			LightRef.intensity = intensityStart * IntensityCurve.Evaluate(time);
		}
		else if (DestroyAtEnd)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
