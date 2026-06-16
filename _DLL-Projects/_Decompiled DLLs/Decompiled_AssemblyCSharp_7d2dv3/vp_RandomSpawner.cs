using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_RandomSpawner : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource m_Audio;

	public AudioClip Sound;

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public bool RandomAngle = true;

	public List<GameObject> SpawnObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (SpawnObjects == null)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, SpawnObjects.Count);
		if (!(SpawnObjects[index] == null))
		{
			((GameObject)vp_Utility.Instantiate(SpawnObjects[index], base.transform.position, base.transform.rotation)).transform.Rotate(UnityEngine.Random.rotation.eulerAngles);
			m_Audio = GetComponent<AudioSource>();
			m_Audio.playOnAwake = true;
			if (Sound != null)
			{
				m_Audio.rolloffMode = AudioRolloffMode.Linear;
				m_Audio.clip = Sound;
				m_Audio.pitch = UnityEngine.Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
				m_Audio.Play();
			}
		}
	}
}
