using System;
using UnityEngine;

public class Detonator : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light _light;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve _timeRate;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve _lightIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _animTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _animTimeDetonator;

	public float PulseRateScale = 1f;

	public void StartCountdown()
	{
		if (!base.isActiveAndEnabled)
		{
			base.enabled = true;
			base.gameObject.SetActive(value: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		_animTime = 0f;
		_animTimeDetonator = 0f;
		base.gameObject.SetActive(value: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		base.gameObject.SetActive(value: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float deltaTime = Time.deltaTime;
		deltaTime *= PulseRateScale;
		_animTime += deltaTime;
		_animTimeDetonator += deltaTime * ((_timeRate != null) ? _timeRate.Evaluate(_animTime) : 1f);
		if (_light != null && _lightIntensity != null)
		{
			_light.intensity = _lightIntensity.Evaluate(_animTimeDetonator);
		}
	}
}
