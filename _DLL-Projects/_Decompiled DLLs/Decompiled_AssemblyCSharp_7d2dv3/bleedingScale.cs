using UnityEngine;

public class bleedingScale : MonoBehaviour
{
	public GameObject parentObject;

	public float minParticleScale;

	public float maxParticleScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float x = parentObject.transform.lossyScale.x;
		ParticleSystem.MainModule main = base.gameObject.GetComponent<ParticleSystem>().main;
		ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(minParticleScale * x, maxParticleScale * x);
		startSize.mode = ParticleSystemCurveMode.TwoConstants;
		main.startSize = startSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}
}
