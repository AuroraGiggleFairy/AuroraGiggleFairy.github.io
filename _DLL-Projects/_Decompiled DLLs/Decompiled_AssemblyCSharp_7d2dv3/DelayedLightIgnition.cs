using System;
using UnityEngine;

public class DelayedLightIgnition : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light myLight;

	public float delay = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timer;

	public void Awake()
	{
		myLight = GetComponent<Light>();
	}

	public void Start()
	{
		myLight.enabled = false;
		timer = delay;
	}

	public void Update()
	{
		if (myLight != null && !myLight.enabled)
		{
			if (timer <= 0f)
			{
				myLight.enabled = true;
			}
			timer -= Time.deltaTime;
		}
	}
}
