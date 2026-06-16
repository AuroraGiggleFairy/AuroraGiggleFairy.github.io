using System;
using UnityEngine;

namespace JBooth.MicroSplat;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class GlitterLight : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light lght;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		lght = GetComponent<Light>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		lght = GetComponent<Light>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Shader.SetGlobalVector("_gGlitterLightDir", -base.transform.forward);
		Shader.SetGlobalVector("_gGlitterLightWorldPos", base.transform.position);
		if (lght != null)
		{
			Shader.SetGlobalColor("_gGlitterLightColor", lght.color);
		}
	}
}
