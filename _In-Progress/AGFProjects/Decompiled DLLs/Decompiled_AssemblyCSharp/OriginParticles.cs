using UnityEngine;

public class OriginParticles : MonoBehaviour
{
	public void OnEnable()
	{
		Origin.particleSystemTs.Add(base.transform);
	}

	public void OnDisable()
	{
		Origin.particleSystemTs.Remove(base.transform);
	}
}
