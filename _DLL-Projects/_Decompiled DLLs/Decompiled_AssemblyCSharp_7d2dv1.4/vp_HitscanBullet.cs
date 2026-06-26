using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_HitscanBullet : MonoBehaviour
{
	public bool IgnoreLocalPlayer = true;

	public float Range = 100f;

	public float Force = 100f;

	public float Damage = 1f;

	public string DamageMethodName = "Damage";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Source;

	public float m_SparkFactor = 0.5f;

	public GameObject m_ImpactPrefab;

	public GameObject m_DustPrefab;

	public GameObject m_SparkPrefab;

	public GameObject m_DebrisPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public List<AudioClip> m_ImpactSounds = new List<AudioClip>();

	public Vector2 SoundImpactPitch = new Vector2(1f, 1.5f);

	public int[] NoDecalOnTheseLayers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Renderer m_Renderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Transform = base.transform;
		m_Renderer = GetComponent<Renderer>();
		m_Audio = GetComponent<AudioSource>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_Initialized = true;
		DoHit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (m_Initialized)
		{
			DoHit();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoHit()
	{
		Ray ray = new Ray(m_Transform.position, m_Transform.forward);
		if (Physics.Raycast(ray, out var hitInfo, Range, IgnoreLocalPlayer ? (-538750981) : (-738197525)))
		{
			Vector3 localScale = m_Transform.localScale;
			m_Transform.parent = hitInfo.transform;
			m_Transform.localPosition = hitInfo.transform.InverseTransformPoint(hitInfo.point);
			m_Transform.rotation = Quaternion.LookRotation(hitInfo.normal);
			if (hitInfo.transform.lossyScale == Vector3.one)
			{
				m_Transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0, 360), Space.Self);
			}
			else
			{
				m_Transform.parent = null;
				m_Transform.localScale = localScale;
				m_Transform.parent = hitInfo.transform;
			}
			Rigidbody attachedRigidbody = hitInfo.collider.attachedRigidbody;
			if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
			{
				attachedRigidbody.AddForceAtPosition(ray.direction * Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, hitInfo.point);
			}
			if (m_ImpactPrefab != null)
			{
				vp_Utility.Instantiate(m_ImpactPrefab, m_Transform.position, m_Transform.rotation);
			}
			if (m_DustPrefab != null)
			{
				vp_Utility.Instantiate(m_DustPrefab, m_Transform.position, m_Transform.rotation);
			}
			if (m_SparkPrefab != null && UnityEngine.Random.value < m_SparkFactor)
			{
				vp_Utility.Instantiate(m_SparkPrefab, m_Transform.position, m_Transform.rotation);
			}
			if (m_DebrisPrefab != null)
			{
				vp_Utility.Instantiate(m_DebrisPrefab, m_Transform.position, m_Transform.rotation);
			}
			if (m_ImpactSounds.Count > 0)
			{
				m_Audio.pitch = UnityEngine.Random.Range(SoundImpactPitch.x, SoundImpactPitch.y) * Time.timeScale;
				m_Audio.clip = m_ImpactSounds[UnityEngine.Random.Range(0, m_ImpactSounds.Count)];
				m_Audio.Stop();
				m_Audio.Play();
			}
			if (m_Source != null)
			{
				hitInfo.collider.SendMessageUpwards(DamageMethodName, new vp_DamageInfo(Damage, m_Source), SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				hitInfo.collider.SendMessageUpwards(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);
			}
			if (NoDecalOnTheseLayers.Length != 0)
			{
				int[] noDecalOnTheseLayers = NoDecalOnTheseLayers;
				foreach (int num in noDecalOnTheseLayers)
				{
					if (hitInfo.transform.gameObject.layer == num)
					{
						m_Renderer.enabled = false;
						TryDestroy();
						return;
					}
				}
			}
			if (m_Renderer != null)
			{
				vp_DecalManager.Add(base.gameObject);
			}
			else
			{
				vp_Timer.In(1f, TryDestroy);
			}
		}
		else
		{
			vp_Utility.Destroy(base.gameObject);
		}
	}

	public void SetSource(Transform source)
	{
		m_Source = source;
		if (source.transform.root == Camera.main.transform.root)
		{
			IgnoreLocalPlayer = true;
		}
	}

	[Obsolete("Please use 'SetSource' instead.")]
	public void SetSender(Transform sender)
	{
		SetSource(sender);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryDestroy()
	{
		if (!(this == null))
		{
			if (!m_Audio.isPlaying)
			{
				m_Renderer.enabled = true;
				vp_Utility.Destroy(base.gameObject);
			}
			else
			{
				vp_Timer.In(1f, TryDestroy);
			}
		}
	}
}
