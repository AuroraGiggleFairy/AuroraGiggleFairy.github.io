using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPWeaponMeleeAttack : vp_Component
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPWeapon m_Weapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPController m_Controller;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_Camera;

	public string WeaponStatePull = "Pull";

	public string WeaponStateSwing = "Swing";

	public float SwingDelay = 0.5f;

	public float SwingDuration = 0.5f;

	public float SwingRate = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NextAllowedSwingTime;

	public int SwingSoftForceFrames = 50;

	public Vector3 SwingPositionSoftForce = new Vector3(-0.5f, -0.1f, 0.3f);

	public Vector3 SwingRotationSoftForce = new Vector3(50f, -25f, 0f);

	public float ImpactTime = 0.11f;

	public Vector3 ImpactPositionSpringRecoil = new Vector3(0.01f, 0.03f, -0.05f);

	public Vector3 ImpactPositionSpring2Recoil = Vector3.zero;

	public Vector3 ImpactRotationSpringRecoil = Vector3.zero;

	public Vector3 ImpactRotationSpring2Recoil = new Vector3(0f, 0f, 10f);

	public string DamageMethodName = "Damage";

	public float Damage = 5f;

	public float DamageRadius = 0.3f;

	public float DamageRange = 2f;

	public float DamageForce = 1000f;

	public bool AttackPickRandomState = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_AttackCurrent;

	public float SparkFactor = 0.1f;

	public GameObject m_DustPrefab;

	public GameObject m_SparkPrefab;

	public GameObject m_DebrisPrefab;

	public List<UnityEngine.Object> SoundSwing = new List<UnityEngine.Object>();

	public List<UnityEngine.Object> SoundImpact = new List<UnityEngine.Object>();

	public Vector2 SoundSwingPitch = new Vector2(0.5f, 1.5f);

	public Vector2 SoundImpactPitch = new Vector2(1f, 1.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle SwingDelayTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle ImpactTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle SwingDurationTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Timer.Handle ResetTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Player;

	public vp_FPPlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		m_Controller = (vp_FPController)base.Root.GetComponent(typeof(vp_FPController));
		m_Camera = (vp_FPCamera)base.Root.GetComponentInChildren(typeof(vp_FPCamera));
		m_Weapon = (vp_FPWeapon)base.Transform.GetComponent(typeof(vp_FPWeapon));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		UpdateAttack();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateAttack()
	{
		if (!Player.Attack.Active || Player.SetWeapon.Active || m_Weapon == null || !m_Weapon.Wielded || Time.time < m_NextAllowedSwingTime)
		{
			return;
		}
		m_NextAllowedSwingTime = Time.time + SwingRate;
		if (AttackPickRandomState)
		{
			PickAttack();
		}
		m_Weapon.SetState(WeaponStatePull);
		m_Weapon.Refresh();
		vp_Timer.In(SwingDelay, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (SoundSwing.Count > 0)
			{
				base.Audio.pitch = UnityEngine.Random.Range(SoundSwingPitch.x, SoundSwingPitch.y) * Time.timeScale;
				base.Audio.clip = (AudioClip)SoundSwing[UnityEngine.Random.Range(0, SoundSwing.Count)];
				base.Audio.Play();
			}
			m_Weapon.SetState(WeaponStatePull, enabled: false);
			m_Weapon.SetState(WeaponStateSwing);
			m_Weapon.Refresh();
			m_Weapon.AddSoftForce(SwingPositionSoftForce, SwingRotationSoftForce, SwingSoftForceFrames);
			vp_Timer.In(ImpactTime, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Physics.SphereCast(new Ray(new Vector3(m_Controller.Transform.position.x, m_Camera.Transform.position.y, m_Controller.Transform.position.z), m_Camera.Transform.forward), DamageRadius, out var hitInfo, DamageRange, -538750981);
				if (hitInfo.collider != null)
				{
					SpawnImpactFX(hitInfo);
					ApplyDamage(hitInfo);
					ApplyRecoil();
				}
				else
				{
					vp_Timer.In(SwingDuration - ImpactTime, [PublicizedFrom(EAccessModifier.Private)] () =>
					{
						m_Weapon.StopSprings();
						Reset();
					}, SwingDurationTimer);
				}
			}, ImpactTimer);
		}, SwingDelayTimer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PickAttack()
	{
		int num = States.Count - 1;
		do
		{
			num = UnityEngine.Random.Range(0, States.Count - 1);
		}
		while (States.Count > 1 && num == m_AttackCurrent && UnityEngine.Random.value < 0.5f);
		m_AttackCurrent = num;
		SetState(States[m_AttackCurrent].Name);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Attack()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnImpactFX(RaycastHit hit)
	{
		Quaternion rotation = Quaternion.LookRotation(hit.normal);
		if (m_DustPrefab != null)
		{
			vp_Utility.Instantiate(m_DustPrefab, hit.point, rotation);
		}
		if (m_SparkPrefab != null && UnityEngine.Random.value < SparkFactor)
		{
			vp_Utility.Instantiate(m_SparkPrefab, hit.point, rotation);
		}
		if (m_DebrisPrefab != null)
		{
			vp_Utility.Instantiate(m_DebrisPrefab, hit.point, rotation);
		}
		if (SoundImpact.Count > 0)
		{
			base.Audio.pitch = UnityEngine.Random.Range(SoundImpactPitch.x, SoundImpactPitch.y) * Time.timeScale;
			base.Audio.PlayOneShot((AudioClip)SoundImpact[UnityEngine.Random.Range(0, SoundImpact.Count)]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDamage(RaycastHit hit)
	{
		hit.collider.SendMessage(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(m_Camera.Transform.forward * DamageForce / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, hit.point);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRecoil()
	{
		m_Weapon.StopSprings();
		m_Weapon.AddForce(ImpactPositionSpringRecoil, ImpactRotationSpringRecoil);
		m_Weapon.AddForce2(ImpactPositionSpring2Recoil, ImpactRotationSpring2Recoil);
		Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		vp_Timer.In(0.05f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (m_Weapon != null)
			{
				m_Weapon.SetState(WeaponStatePull, enabled: false);
				m_Weapon.SetState(WeaponStateSwing, enabled: false);
				m_Weapon.Refresh();
				if (AttackPickRandomState)
				{
					ResetState();
				}
			}
		}, ResetTimer);
	}
}
