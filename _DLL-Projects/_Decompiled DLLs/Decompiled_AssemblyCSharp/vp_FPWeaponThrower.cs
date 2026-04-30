using System;
using UnityEngine;

public class vp_FPWeaponThrower : vp_WeaponThrower
{
	public Vector3 FirePositionOffset = new Vector3(0.35f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_OriginalLookDownActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer1 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer2 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer3 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer4 = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPWeapon m_FPWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPWeaponShooter m_FPWeaponShooter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_FirePosition;

	public vp_FPWeapon FPWeapon
	{
		get
		{
			if (m_FPWeapon == null)
			{
				m_FPWeapon = (vp_FPWeapon)base.Transform.GetComponent(typeof(vp_FPWeapon));
			}
			return m_FPWeapon;
		}
	}

	public vp_FPWeaponShooter FPWeaponShooter
	{
		get
		{
			if (m_FPWeaponShooter == null)
			{
				m_FPWeaponShooter = (vp_FPWeaponShooter)base.Transform.GetComponent(typeof(vp_FPWeaponShooter));
			}
			return m_FPWeaponShooter;
		}
	}

	public Transform FirePosition
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_FirePosition == null)
			{
				GameObject gameObject = new GameObject("ThrownWeaponFirePosition");
				m_FirePosition = gameObject.transform;
				m_FirePosition.parent = Camera.main.transform;
			}
			return m_FirePosition;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		m_OriginalLookDownActive = FPWeapon.LookDownActive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RewindAnimation()
	{
		if (base.Player.IsFirstPerson.Get() && !(FPWeapon == null) && !(FPWeapon.WeaponModel == null) && !(FPWeapon.WeaponModel.GetComponent<Animation>() == null) && !(FPWeaponShooter == null) && !(FPWeaponShooter.AnimationFire == null))
		{
			FPWeapon.WeaponModel.GetComponent<Animation>()[FPWeaponShooter.AnimationFire.name].time = 0f;
			FPWeapon.WeaponModel.GetComponent<Animation>().Play();
			FPWeapon.WeaponModel.GetComponent<Animation>().Sample();
			FPWeapon.WeaponModel.GetComponent<Animation>().Stop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Attack()
	{
		base.OnStart_Attack();
		if (base.Player.IsFirstPerson.Get())
		{
			base.Shooter.m_ProjectileSpawnPoint = FirePosition.gameObject;
			FirePosition.localPosition = FirePositionOffset;
			FirePosition.localEulerAngles = Vector3.zero;
		}
		else
		{
			base.Shooter.m_ProjectileSpawnPoint = base.Weapon.Weapon3rdPersonModel;
		}
		FPWeapon.LookDownActive = false;
		vp_Timer.In(base.Shooter.ProjectileSpawnDelay, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!base.HaveAmmoForCurrentWeapon)
			{
				FPWeapon.SetState("ReWield");
				FPWeapon.Refresh();
				vp_Timer.In(1f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					if (!base.Player.SetNextWeapon.Try())
					{
						vp_Timer.In(0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
						{
							RewindAnimation();
							base.Player.SetWeapon.Start();
						}, m_Timer2);
					}
				});
			}
			else if (base.Player.IsFirstPerson.Get())
			{
				FPWeapon.SetState("ReWield");
				FPWeapon.Refresh();
				vp_Timer.In(1f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					RewindAnimation();
					FPWeapon.Rendering = true;
					FPWeapon.SetState("ReWield", enabled: false);
					FPWeapon.Refresh();
				}, m_Timer3);
			}
			else
			{
				vp_Timer.In(0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					base.Player.Attack.Stop();
				}, m_Timer4);
			}
		}, m_Timer1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_SetWeapon()
	{
		RewindAnimation();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStop_Attack()
	{
		base.OnStop_Attack();
		FPWeapon.LookDownActive = m_OriginalLookDownActive;
	}
}
