using System;
using UnityEngine;

[RequireComponent(typeof(vp_FPWeapon))]
public class vp_FPWeaponShooter : vp_WeaponShooter
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPWeapon m_FPWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_FPCamera;

	public float MotionPositionReset = 0.5f;

	public float MotionRotationReset = 0.5f;

	public float MotionPositionPause = 1f;

	public float MotionRotationPause = 1f;

	public float MotionRotationRecoilCameraFactor;

	public float MotionPositionRecoilCameraFactor;

	public AnimationClip AnimationFire;

	public new vp_FPPlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return (vp_FPPlayerEventHandler)m_Player;
		}
	}

	public new vp_FPWeapon Weapon
	{
		get
		{
			if (m_FPWeapon == null)
			{
				m_FPWeapon = base.transform.GetComponent<vp_FPWeapon>();
			}
			return m_FPWeapon;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
		if (m_ProjectileSpawnPoint == null)
		{
			m_ProjectileSpawnPoint = m_FPCamera.gameObject;
		}
		m_ProjectileDefaultSpawnpoint = m_ProjectileSpawnPoint;
		m_NextAllowedFireTime = Time.time;
		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, ProjectileFiringRate - 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		RefreshFirePoint();
		base.OnEnable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (ProjectileFiringRate == 0f && AnimationFire != null)
		{
			ProjectileFiringRate = AnimationFire.length;
		}
		if (ProjectileFiringRate == 0f && AnimationFire != null)
		{
			ProjectileFiringRate = AnimationFire.length;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Fire()
	{
		m_LastFireTime = Time.time;
		if (AnimationFire != null)
		{
			if (Weapon.WeaponModel.GetComponent<Animation>()[AnimationFire.name] == null)
			{
				Debug.LogError("Error (" + this?.ToString() + ") No animation named '" + AnimationFire.name + "' is listed in this prefab. Make sure the prefab has an 'Animation' component which references all the clips you wish to play on the weapon.");
			}
			else
			{
				Weapon.WeaponModel.GetComponent<Animation>()[AnimationFire.name].time = 0f;
				Weapon.WeaponModel.GetComponent<Animation>().Sample();
				Weapon.WeaponModel.GetComponent<Animation>().Play(AnimationFire.name);
			}
		}
		if (MotionRecoilDelay == 0f)
		{
			ApplyRecoil();
		}
		else
		{
			vp_Timer.In(MotionRecoilDelay, ApplyRecoil);
		}
		base.Fire();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ApplyRecoil()
	{
		Weapon.ResetSprings(MotionPositionReset, MotionRotationReset, MotionPositionPause, MotionRotationPause);
		if (MotionRotationRecoil.z == 0f)
		{
			Weapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);
			if (MotionPositionRecoilCameraFactor != 0f)
			{
				m_FPCamera.AddForce2(MotionPositionRecoil * MotionPositionRecoilCameraFactor);
			}
			return;
		}
		Weapon.AddForce2(MotionPositionRecoil, Vector3.Scale(MotionRotationRecoil, Vector3.one + Vector3.back) + ((UnityEngine.Random.value < 0.5f) ? Vector3.forward : Vector3.back) * UnityEngine.Random.Range(MotionRotationRecoil.z * MotionRotationRecoilDeadZone, MotionRotationRecoil.z));
		if (MotionPositionRecoilCameraFactor != 0f)
		{
			m_FPCamera.AddForce2(MotionPositionRecoil * MotionPositionRecoilCameraFactor);
		}
		if (MotionRotationRecoilCameraFactor != 0f)
		{
			m_FPCamera.AddRollForce(UnityEngine.Random.Range(MotionRotationRecoil.z * MotionRotationRecoilDeadZone, MotionRotationRecoil.z) * MotionRotationRecoilCameraFactor * ((UnityEngine.Random.value < 0.5f) ? 1f : (-1f)));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		RefreshFirePoint();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshFirePoint()
	{
		if (Player.IsFirstPerson == null)
		{
			return;
		}
		if (Player.IsFirstPerson.Get())
		{
			m_ProjectileSpawnPoint = m_FPCamera.gameObject;
			if (base.MuzzleFlash != null)
			{
				base.MuzzleFlash.layer = 10;
			}
			m_MuzzleFlashSpawnPoint = null;
			m_ShellEjectSpawnPoint = null;
			Refresh();
		}
		else
		{
			m_ProjectileSpawnPoint = m_ProjectileDefaultSpawnpoint;
			if (base.MuzzleFlash != null)
			{
				base.MuzzleFlash.layer = 0;
			}
			m_MuzzleFlashSpawnPoint = null;
			m_ShellEjectSpawnPoint = null;
			Refresh();
		}
		if (Player.CurrentWeaponName.Get() != base.name)
		{
			m_ProjectileSpawnPoint = m_FPCamera.gameObject;
		}
	}
}
