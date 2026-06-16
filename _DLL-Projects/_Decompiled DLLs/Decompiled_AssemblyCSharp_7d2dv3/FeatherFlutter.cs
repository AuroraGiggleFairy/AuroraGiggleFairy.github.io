using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FeatherFlutter : MonoBehaviour
{
	public float amplitude = 0.5f;

	public float frequency = 1.5f;

	public float phaseSpread = 6f;

	public float fallBlendSpeed = 0.5f;

	public float lateralDrag = 0.85f;

	public float zAmplitude = 0.3f;

	public float zFrequency = 1.1f;

	public float rotationInfluence = 25f;

	public float rotationDrag = 0.9f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem ps;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleSystem.Particle[] particles;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		ps = GetComponent<ParticleSystem>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		int particleCount = ps.particleCount;
		if (particleCount == 0)
		{
			return;
		}
		if (particles == null || particles.Length < particleCount)
		{
			particles = new ParticleSystem.Particle[particleCount];
		}
		ps.GetParticles(particles, particleCount);
		for (int i = 0; i < particleCount; i++)
		{
			if (!(particles[i].velocity.y >= 0f))
			{
				float num = (float)(particles[i].randomSeed % 1000) / 1000f * phaseSpread;
				float num2 = particles[i].startLifetime - particles[i].remainingLifetime;
				float num3 = Mathf.Clamp01((0f - particles[i].velocity.y) / fallBlendSpeed);
				float b = Mathf.Sin(num2 * frequency + num) * amplitude * num3;
				float b2 = Mathf.Sin(num2 * zFrequency + num + 1.5f) * zAmplitude * num3;
				Vector3 velocity = particles[i].velocity;
				velocity.x = Mathf.Lerp(velocity.x, b, 1f - lateralDrag);
				velocity.z = Mathf.Lerp(velocity.z, b2, 1f - lateralDrag);
				particles[i].velocity = velocity;
				float b3 = (0f - velocity.z) * rotationInfluence;
				float b4 = velocity.x * rotationInfluence;
				Vector3 rotation3D = particles[i].rotation3D;
				rotation3D.x = Mathf.Lerp(rotation3D.x, b3, 1f - rotationDrag);
				rotation3D.z = Mathf.Lerp(rotation3D.z, b4, 1f - rotationDrag);
				particles[i].rotation3D = rotation3D;
			}
		}
		ps.SetParticles(particles, particleCount);
	}
}
