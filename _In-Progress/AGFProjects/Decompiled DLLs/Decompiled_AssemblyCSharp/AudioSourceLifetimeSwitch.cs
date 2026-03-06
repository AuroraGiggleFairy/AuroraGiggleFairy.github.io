using System;
using UnityEngine;

public class AudioSourceLifetimeSwitch : MonoBehaviour
{
	public float TurnOffDelayAfterEntityDies = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform entityRoot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Entity entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource audio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float delay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstUpdate = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		entityRoot = RootTransformRefEntity.FindEntityUpwards(base.transform);
		audio = base.transform.GetComponent<AudioSource>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (bFirstUpdate)
		{
			bFirstUpdate = false;
			entity = ((entityRoot != null) ? entityRoot.GetComponent<Entity>() : null);
			if ((bool)entity && entity.IsDead())
			{
				if ((bool)audio)
				{
					audio.enabled = false;
					audio = null;
				}
				entity = null;
			}
		}
		if ((bool)audio && !entity)
		{
			delay -= Time.deltaTime;
			if (delay <= 0f)
			{
				audio.enabled = false;
				audio = null;
			}
		}
		if ((bool)entity && entity.IsDead())
		{
			delay = TurnOffDelayAfterEntityDies;
			entity = null;
		}
	}
}
