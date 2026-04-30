using System;
using UnityEngine;

public class vp_WeaponReloader : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Weapon m_Weapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public AudioClip SoundReload;

	public float ReloadDuration = 1f;

	public virtual float OnValue_CurrentWeaponReloadDuration
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return ReloadDuration;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Audio = GetComponent<AudioSource>();
		m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Weapon = base.transform.GetComponent<vp_Weapon>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
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
	public virtual bool CanStart_Reload()
	{
		if (!m_Player.CurrentWeaponWielded.Get())
		{
			return false;
		}
		if (m_Player.CurrentWeaponMaxAmmoCount.Get() != 0 && m_Player.CurrentWeaponAmmoCount.Get() == m_Player.CurrentWeaponMaxAmmoCount.Get())
		{
			return false;
		}
		if (m_Player.CurrentWeaponClipCount.Get() < 1)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Reload()
	{
		m_Player.Reload.AutoDuration = m_Player.CurrentWeaponReloadDuration.Get();
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(SoundReload);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Reload()
	{
		m_Player.RefillCurrentWeapon.Try();
	}
}
