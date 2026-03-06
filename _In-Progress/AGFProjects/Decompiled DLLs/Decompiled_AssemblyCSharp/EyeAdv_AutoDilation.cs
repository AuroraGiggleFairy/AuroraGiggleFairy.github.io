using System;
using UnityEngine;

public class EyeAdv_AutoDilation : MonoBehaviour
{
	public bool enableAutoDilation = true;

	public Transform sceneLightObject;

	public float lightSensitivity = 1f;

	public float dilationSpeed = 0.1f;

	public float maxDilation = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light sceneLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dilateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float pupilDilation = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currTargetDilation = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetDilation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currLightSensitivity = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer eyeRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		eyeRenderer = base.gameObject.GetComponent<Renderer>();
		if (sceneLightObject != null)
		{
			sceneLight = sceneLightObject.GetComponent<Light>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (!(sceneLight != null))
		{
			return;
		}
		lightIntensity = sceneLight.intensity;
		if (enableAutoDilation)
		{
			if (currTargetDilation != targetDilation || currLightSensitivity != lightSensitivity)
			{
				dilateTime = 0f;
				currTargetDilation = targetDilation;
				currLightSensitivity = lightSensitivity;
			}
			lightAngle = Vector3.Angle(sceneLightObject.transform.forward, base.transform.forward) / 180f;
			targetDilation = Mathf.Lerp(1f, 0f, lightAngle * lightIntensity * lightSensitivity);
			dilateTime += Time.deltaTime * dilationSpeed;
			pupilDilation = Mathf.Clamp(pupilDilation, 0f, maxDilation);
			pupilDilation = Mathf.Lerp(pupilDilation, targetDilation, dilateTime);
			eyeRenderer.sharedMaterial.SetFloat("_pupilSize", pupilDilation);
		}
	}
}
