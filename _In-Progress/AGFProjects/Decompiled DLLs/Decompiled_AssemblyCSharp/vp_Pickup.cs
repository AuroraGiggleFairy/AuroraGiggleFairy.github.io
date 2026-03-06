using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public abstract class vp_Pickup : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody m_Rigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public string InventoryName = "Unnamed";

	public List<string> RecipientTags = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Collider m_LastCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Recipient;

	public string GiveMessage = "Picked up an item";

	public string FailMessage = "You currently can't pick up this item!";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_SpawnPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_SpawnScale = Vector3.zero;

	public bool Billboard;

	public Vector3 Spin = Vector3.zero;

	public float BobAmp;

	public float BobRate;

	public float BobOffset = -1f;

	public Vector3 RigidbodyForce = Vector3.zero;

	public float RigidbodySpin;

	public float RespawnDuration = 10f;

	public float RespawnScaleUpDuration;

	public float RemoveDuration;

	public AudioClip PickupSound;

	public AudioClip PickupFailSound;

	public AudioClip RespawnSound;

	public bool PickupSoundSlomo = true;

	public bool FailSoundSlomo = true;

	public bool RespawnSoundSlomo = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Depleted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_AlreadyFailed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_RespawnTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_CameraMainTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Transform = base.transform;
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Audio = GetComponent<AudioSource>();
		if (Camera.main != null)
		{
			m_CameraMainTransform = Camera.main.transform;
		}
		GetComponent<Collider>().isTrigger = true;
		m_Audio.clip = PickupSound;
		m_Audio.playOnAwake = false;
		m_Audio.minDistance = 3f;
		m_Audio.maxDistance = 150f;
		m_Audio.rolloffMode = AudioRolloffMode.Linear;
		m_Audio.dopplerLevel = 0f;
		m_SpawnPosition = m_Transform.position;
		m_SpawnScale = m_Transform.localScale;
		RespawnScaleUpDuration = ((m_Rigidbody == null) ? Mathf.Abs(RespawnScaleUpDuration) : 0f);
		if (BobOffset == -1f)
		{
			BobOffset = UnityEngine.Random.value;
		}
		if (RecipientTags.Count == 0)
		{
			RecipientTags.Add("Player");
		}
		if (RemoveDuration != 0f)
		{
			vp_Timer.In(RemoveDuration, Remove);
		}
		if (m_Rigidbody != null)
		{
			if (RigidbodyForce != Vector3.zero)
			{
				m_Rigidbody.AddForce(RigidbodyForce, ForceMode.Impulse);
			}
			if (RigidbodySpin != 0f)
			{
				m_Rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles * RigidbodySpin);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		UpdateMotion();
		if (m_Depleted && !m_Audio.isPlaying)
		{
			Remove();
		}
		if (m_Depleted || !(m_Rigidbody != null) || !m_Rigidbody.IsSleeping() || m_Rigidbody.isKinematic)
		{
			return;
		}
		m_Rigidbody.isKinematic = true;
		Collider[] components = GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger)
			{
				collider.enabled = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateMotion()
	{
		if (m_Rigidbody != null)
		{
			return;
		}
		if (Billboard)
		{
			if (m_CameraMainTransform != null)
			{
				m_Transform.localEulerAngles = m_CameraMainTransform.eulerAngles;
			}
		}
		else
		{
			m_Transform.localEulerAngles += Spin * Time.deltaTime;
		}
		if (BobRate != 0f && BobAmp != 0f)
		{
			m_Transform.position = m_SpawnPosition + Vector3.up * (Mathf.Cos((Time.time + BobOffset) * (BobRate * 10f)) * BobAmp);
		}
		if (m_Transform.localScale != m_SpawnScale)
		{
			m_Transform.localScale = Vector3.Lerp(m_Transform.localScale, m_SpawnScale, Time.deltaTime / RespawnScaleUpDuration);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (m_Depleted)
		{
			return;
		}
		using (List<string>.Enumerator enumerator = RecipientTags.GetEnumerator())
		{
			string current;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					continue;
				}
				return;
			}
			while (!(col.gameObject.tag == current));
		}
		if (col != m_LastCollider)
		{
			m_Recipient = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();
		}
		if (!(m_Recipient == null))
		{
			if (TryGive(m_Recipient))
			{
				m_Audio.pitch = (PickupSoundSlomo ? Time.timeScale : 1f);
				m_Audio.Play();
				GetComponent<Renderer>().enabled = false;
				m_Depleted = true;
				m_Recipient.HUDText.Send(GiveMessage);
			}
			else if (!m_AlreadyFailed)
			{
				m_Audio.pitch = (FailSoundSlomo ? Time.timeScale : 1f);
				m_Audio.PlayOneShot(PickupFailSound);
				m_AlreadyFailed = true;
				m_Recipient.HUDText.Send(FailMessage);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerExit(Collider col)
	{
		m_AlreadyFailed = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (!player.AddItem.Try(new object[2] { InventoryName, 1 }))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Remove()
	{
		if (!(this == null))
		{
			if (RespawnDuration == 0f)
			{
				vp_Utility.Destroy(base.gameObject);
			}
			else if (!m_RespawnTimer.Active)
			{
				vp_Utility.Activate(base.gameObject, activate: false);
				vp_Timer.In(RespawnDuration, Respawn, m_RespawnTimer);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Respawn()
	{
		if (m_Transform == null)
		{
			return;
		}
		if (Camera.main != null)
		{
			m_CameraMainTransform = Camera.main.transform;
		}
		m_RespawnTimer.Cancel();
		m_Transform.position = m_SpawnPosition;
		if (m_Rigidbody == null && RespawnScaleUpDuration > 0f)
		{
			m_Transform.localScale = Vector3.zero;
		}
		GetComponent<Renderer>().enabled = true;
		vp_Utility.Activate(base.gameObject);
		m_Audio.pitch = (RespawnSoundSlomo ? Time.timeScale : 1f);
		m_Audio.PlayOneShot(RespawnSound);
		m_Depleted = false;
		if (BobOffset == -1f)
		{
			BobOffset = UnityEngine.Random.value;
		}
		if (!(m_Rigidbody != null))
		{
			return;
		}
		m_Rigidbody.isKinematic = false;
		Collider[] components = GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (!collider.isTrigger)
			{
				collider.enabled = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Pickup()
	{
	}
}
