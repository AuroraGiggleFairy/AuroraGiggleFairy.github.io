using System;
using UnityEngine;

public class vp_DoomsDayDevice : MonoBehaviour
{
	public AudioClip EarthQuakeSound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Initiated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_Button;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PulsingLight m_PulsingLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_PlayerAudioSource;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_DeviceAudioSource;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_OriginalButtonPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_OriginalButtonColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_OriginalPulsingLightMaxIntensity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Player = UnityEngine.Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		if (m_Player != null)
		{
			m_PlayerAudioSource = m_Player.GetComponent<AudioSource>();
		}
		m_DeviceAudioSource = GetComponent<AudioSource>();
		m_Button = GameObject.Find("ForbiddenButton");
		if (m_Button != null)
		{
			m_OriginalButtonPos = m_Button.transform.localPosition;
			m_OriginalButtonColor = m_Button.GetComponent<Renderer>().material.color;
		}
		m_PulsingLight = m_Button.GetComponentInChildren<vp_PulsingLight>();
		if (m_PulsingLight != null)
		{
			m_OriginalPulsingLightMaxIntensity = m_PulsingLight.m_MaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
		if (m_Button != null)
		{
			m_Button.transform.localPosition = m_OriginalButtonPos;
			m_Button.GetComponent<Renderer>().material.color = m_OriginalButtonColor;
		}
		if (m_DeviceAudioSource != null)
		{
			m_DeviceAudioSource.pitch = 1f;
			m_DeviceAudioSource.volume = 1f;
		}
		if (m_PulsingLight != null)
		{
			m_PulsingLight.m_MaxIntensity = m_OriginalPulsingLightMaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (Initiated)
		{
			if (m_Button != null)
			{
				m_Button.GetComponent<Renderer>().material.color = Color.Lerp(m_Button.GetComponent<Renderer>().material.color, m_OriginalButtonColor * 0.2f, Time.deltaTime * 1.5f);
			}
			if (m_DeviceAudioSource != null)
			{
				m_DeviceAudioSource.pitch -= Time.deltaTime * 0.35f;
			}
			if (m_PulsingLight != null)
			{
				m_PulsingLight.m_MaxIntensity = 2.5f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitiateDoomsDay()
	{
		if (!Initiated)
		{
			Initiated = true;
			if (m_Button != null)
			{
				m_Button.transform.localPosition += Vector3.down * 0.02f;
			}
			if (m_PlayerAudioSource != null)
			{
				m_PlayerAudioSource.PlayOneShot(EarthQuakeSound);
			}
			m_Player.CameraEarthQuake.TryStart(new Vector3(0.05f, 0.05f, 10f));
			vp_Timer.In(3f, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				SendMessage("Die");
			});
			vp_Timer.In(3f, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Initiated = false;
			});
		}
	}
}
