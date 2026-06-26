using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class vp_Shell : MonoBehaviour
{
	public delegate void RestAngleFunc();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody m_Rigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource m_Audio;

	public float LifeTime = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_RemoveTime;

	public float m_Persistence = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RestAngleFunc m_RestAngleFunc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_RestTime;

	public List<AudioClip> m_BounceSounds = new List<AudioClip>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Transform = base.transform;
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Audio = GetComponent<AudioSource>();
		GetComponent<AudioSource>().playOnAwake = false;
		GetComponent<AudioSource>().dopplerLevel = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		m_RestAngleFunc = null;
		m_RemoveTime = Time.time + LifeTime;
		m_RestTime = Time.time + LifeTime * 0.25f;
		m_Rigidbody.maxAngularVelocity = 100f;
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;
		m_Rigidbody.constraints = RigidbodyConstraints.None;
		GetComponent<Collider>().enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_RestAngleFunc == null)
		{
			if (Time.time > m_RestTime)
			{
				DecideRestAngle();
			}
		}
		else
		{
			m_RestAngleFunc();
		}
		if (Time.time > m_RemoveTime)
		{
			m_Transform.localScale = Vector3.Lerp(m_Transform.localScale, Vector3.zero, Time.deltaTime * 60f * 0.2f);
			if (Time.time > m_RemoveTime + 0.5f)
			{
				vp_Utility.Destroy(base.gameObject);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > 2f)
		{
			if (UnityEngine.Random.value > 0.5f)
			{
				m_Rigidbody.AddRelativeTorque(-UnityEngine.Random.rotation.eulerAngles * 0.15f);
			}
			else
			{
				m_Rigidbody.AddRelativeTorque(UnityEngine.Random.rotation.eulerAngles * 0.15f);
			}
			if (m_Audio != null && m_BounceSounds.Count > 0)
			{
				m_Audio.pitch = Time.timeScale;
				m_Audio.PlayOneShot(m_BounceSounds[UnityEngine.Random.Range(0, m_BounceSounds.Count)]);
			}
		}
		else if (UnityEngine.Random.value > m_Persistence)
		{
			GetComponent<Collider>().enabled = false;
			m_RemoveTime = Time.time + 0.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DecideRestAngle()
	{
		if (Mathf.Abs(m_Transform.eulerAngles.x - 270f) < 55f)
		{
			if (Physics.Raycast(new Ray(m_Transform.position, Vector3.down), out var hitInfo, 1f) && hitInfo.normal == Vector3.up)
			{
				m_RestAngleFunc = UpRight;
				m_Rigidbody.constraints = (RigidbodyConstraints)80;
			}
		}
		else
		{
			m_RestAngleFunc = TippedOver;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpRight()
	{
		m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, Quaternion.Euler(-90f, m_Transform.rotation.y, m_Transform.rotation.z), Time.time * (Time.deltaTime * 60f * 0.05f));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TippedOver()
	{
		m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, Quaternion.Euler(0f, m_Transform.localEulerAngles.y, m_Transform.localEulerAngles.z), Time.time * (Time.deltaTime * 60f * 0.005f));
	}
}
