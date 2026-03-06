using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Debris : MonoBehaviour
{
	public float Radius = 2f;

	public float Force = 10f;

	public float UpForce = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource m_Audio;

	public List<AudioClip> Sounds = new List<AudioClip>();

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public float LifeTime = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Destroy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider[] m_Colliders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Collider, Dictionary<string, object>> m_PiecesInitial = new Dictionary<Collider, Dictionary<string, object>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Audio = GetComponent<AudioSource>();
		m_Colliders = GetComponentsInChildren<Collider>();
		Collider[] colliders = m_Colliders;
		foreach (Collider collider in colliders)
		{
			if ((bool)collider.GetComponent<Rigidbody>())
			{
				m_PiecesInitial.Add(collider, new Dictionary<string, object>
				{
					{
						"Position",
						collider.transform.localPosition
					},
					{
						"Rotation",
						collider.transform.localRotation
					}
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		m_Destroy = false;
		m_Audio.playOnAwake = true;
		Collider[] colliders = m_Colliders;
		foreach (Collider collider in colliders)
		{
			if (!collider.GetComponent<Rigidbody>())
			{
				continue;
			}
			collider.transform.localPosition = (Vector3)m_PiecesInitial[collider]["Position"];
			collider.transform.localRotation = (Quaternion)m_PiecesInitial[collider]["Rotation"];
			collider.GetComponent<Rigidbody>().velocity = Vector3.zero;
			collider.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			collider.GetComponent<Rigidbody>().AddExplosionForce(Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, base.transform.position, Radius, UpForce);
			Collider c = collider;
			vp_Timer.In(UnityEngine.Random.Range(LifeTime * 0.5f, LifeTime * 0.95f), [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				if (c != null)
				{
					vp_Utility.Destroy(c.gameObject);
				}
			});
		}
		vp_Timer.In(LifeTime, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Destroy = true;
		});
		if (Sounds.Count > 0)
		{
			m_Audio.rolloffMode = AudioRolloffMode.Linear;
			m_Audio.clip = Sounds[UnityEngine.Random.Range(0, Sounds.Count)];
			m_Audio.pitch = UnityEngine.Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
			m_Audio.Play();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_Destroy && !GetComponent<AudioSource>().isPlaying)
		{
			vp_Utility.Destroy(base.gameObject);
		}
	}
}
