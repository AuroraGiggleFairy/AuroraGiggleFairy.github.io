using System;
using UnityEngine;

public class ParticleLifetimeSwitch : MonoBehaviour
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
	public ParticleSystem particles;

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
		particles = base.transform.GetComponent<ParticleSystem>();
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
				if (particles != null)
				{
					ParticleSystem.EmissionModule emission = particles.emission;
					emission.enabled = false;
					particles = null;
				}
				entity = null;
			}
		}
		if ((bool)particles && !entity)
		{
			delay -= Time.deltaTime;
			if (delay <= 0f)
			{
				ParticleSystem.EmissionModule emission2 = particles.emission;
				emission2.enabled = false;
				particles = null;
			}
		}
		if ((bool)entity && entity.IsDead())
		{
			delay = TurnOffDelayAfterEntityDies;
			entity = null;
		}
	}
}
