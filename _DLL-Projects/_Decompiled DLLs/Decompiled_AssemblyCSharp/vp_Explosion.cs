using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Explosion : MonoBehaviour
{
	public float Radius = 15f;

	public float Force = 1000f;

	public float UpForce = 10f;

	public float Damage = 10f;

	public bool AllowCover;

	public float CameraShake = 1f;

	public string DamageMessageName = "Damage";

	public bool RequireDamageHandler = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_HaveExploded;

	public AudioClip Sound;

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public List<GameObject> FXPrefabs = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Ray m_Ray;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_RaycastHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider m_TargetCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_TargetTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_TargetRigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_DistanceModifier;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_DamageHandler, object> m_DHandlersHitByThisExplosion = new Dictionary<vp_DamageHandler, object>(50);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static vp_DamageHandler m_TargetDHandler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Source;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_OriginalSource;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public float DistanceModifier
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_DistanceModifier == 0f)
			{
				m_DistanceModifier = 1f - Vector3.Distance(Transform.position, m_TargetTransform.position) / Radius;
			}
			return m_DistanceModifier;
		}
	}

	public Transform Transform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			return m_Transform;
		}
	}

	public Transform Source
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Source == null)
			{
				m_Source = base.transform;
			}
			return m_Source;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_Source = value;
		}
	}

	public Transform OriginalSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_OriginalSource == null)
			{
				m_OriginalSource = base.transform;
			}
			return m_OriginalSource;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_OriginalSource = value;
		}
	}

	public AudioSource Audio
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Audio == null)
			{
				m_Audio = GetComponent<AudioSource>();
			}
			return m_Audio;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		Source = base.transform;
		OriginalSource = null;
		vp_TargetEvent<Transform>.Register(base.transform, "SetSource", SetSource);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		Source = null;
		OriginalSource = null;
		vp_TargetEvent<Transform>.Unregister(base.transform, "SetSource", SetSource);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_HaveExploded)
		{
			if (!Audio.isPlaying)
			{
				vp_Utility.Destroy(base.gameObject);
			}
		}
		else
		{
			DoExplode();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoExplode()
	{
		m_HaveExploded = true;
		foreach (GameObject fXPrefab in FXPrefabs)
		{
			if (fXPrefab != null)
			{
				vp_Utility.Instantiate(fXPrefab, Transform.position, Transform.rotation);
			}
		}
		m_DHandlersHitByThisExplosion.Clear();
		Collider[] array = Physics.OverlapSphere(Transform.position, Radius, -738197525);
		foreach (Collider collider in array)
		{
			if (collider.gameObject.isStatic)
			{
				continue;
			}
			m_DistanceModifier = 0f;
			if (!(collider != null) || !(collider != GetComponent<Collider>()))
			{
				continue;
			}
			m_TargetCollider = collider;
			m_TargetTransform = collider.transform;
			AddUFPSCameraShake();
			if (!TargetInCover())
			{
				m_TargetRigidbody = collider.GetComponent<Rigidbody>();
				if (m_TargetRigidbody != null)
				{
					AddRigidbodyForce();
				}
				else
				{
					AddUFPSForce();
				}
				m_TargetDHandler = vp_DamageHandler.GetDamageHandlerOfCollider(m_TargetCollider);
				if (m_TargetDHandler != null)
				{
					DoUFPSDamage(DistanceModifier * Damage);
				}
				else if (!RequireDamageHandler)
				{
					DoUnityDamage(DistanceModifier * Damage);
				}
			}
		}
		Audio.clip = Sound;
		Audio.pitch = UnityEngine.Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
		if (!Audio.playOnAwake)
		{
			Audio.Play();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool TargetInCover()
	{
		if (!AllowCover)
		{
			return false;
		}
		m_Ray.origin = Transform.position;
		m_Ray.direction = (m_TargetCollider.bounds.center - Transform.position).normalized;
		if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1f) && m_RaycastHit.collider == m_TargetCollider)
		{
			return false;
		}
		m_Ray.direction = (vp_3DUtility.HorizontalVector(m_TargetCollider.bounds.center) + Vector3.up * m_TargetCollider.bounds.max.y - Transform.position).normalized;
		if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1f) && m_RaycastHit.collider == m_TargetCollider)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddRigidbodyForce()
	{
		if (!m_TargetRigidbody.isKinematic)
		{
			m_Ray.origin = m_TargetTransform.position;
			m_Ray.direction = -Vector3.up;
			if (!Physics.Raycast(m_Ray, out m_RaycastHit, 1f))
			{
				UpForce = 0f;
			}
			m_TargetRigidbody.AddExplosionForce(Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, Transform.position, Radius, UpForce);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddUFPSForce()
	{
		vp_TargetEvent<Vector3>.Send(m_TargetTransform.root, "ForceImpact", (m_TargetTransform.position - Transform.position).normalized * Force * 0.001f * DistanceModifier);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddUFPSCameraShake()
	{
		vp_TargetEvent<float>.Send(m_TargetTransform.root, "CameraBombShake", DistanceModifier * CameraShake);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DoUFPSDamage(float damage)
	{
		if (!m_DHandlersHitByThisExplosion.ContainsKey(m_TargetDHandler))
		{
			m_DHandlersHitByThisExplosion.Add(m_TargetDHandler, null);
			m_TargetDHandler.Damage(new vp_DamageInfo(damage, Source, OriginalSource));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoUnityDamage(float damage)
	{
		m_TargetCollider.gameObject.BroadcastMessage(DamageMessageName, damage, SendMessageOptions.DontRequireReceiver);
	}

	public void SetSource(Transform source)
	{
		m_OriginalSource = source;
	}
}
