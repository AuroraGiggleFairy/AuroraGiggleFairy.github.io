using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class vp_Grab : vp_Interactable
{
	public string OnGrabText = "";

	public Texture GrabStateCrosshair;

	public float FootstepForce = 0.015f;

	public float Kneeling = 5f;

	public float Stiffness = 0.5f;

	public float ShakeSpeed = 0.1f;

	public Vector3 ShakeAmplitude = Vector3.one;

	public float ThrowStrength = 6f;

	public bool AllowThrowRotation = true;

	public float Burden;

	public int MaxCollisionCount = 20;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentShake = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentRotationSway = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_CurrentMouseMove;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentSwayForce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentSwayTorque;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentFootstepForce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentHoldAngle = Vector3.one;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Collider m_LastExternalCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_CollisionCount;

	public Vector3 CarryingOffset = new Vector3(0f, -0.5f, 1.5f);

	public float CameraPitchDownLimit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_TempCarryingOffset = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_IsFetching;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float duration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FetchProgress;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPInteractManager m_InteractManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_LastWeaponEquipped;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_IsGrabbed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_OriginalPitchDownLimit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_DefaultGravity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_DefaultDrag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_DefaultAngularDrag;

	public Vector2 SoundsPitch = new Vector2(1f, 1.5f);

	public List<AudioClip> GrabSounds = new List<AudioClip>();

	public List<AudioClip> DropSounds = new List<AudioClip>();

	public List<AudioClip> ThrowSounds = new List<AudioClip>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_DisableAngleSwayTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (!GetComponent<Rigidbody>() || !GetComponent<Collider>())
		{
			base.enabled = false;
		}
		if (GetComponent<Rigidbody>() != null)
		{
			m_DefaultGravity = GetComponent<Rigidbody>().useGravity;
			m_DefaultDrag = m_Transform.GetComponent<Rigidbody>().drag;
			m_DefaultAngularDrag = m_Transform.GetComponent<Rigidbody>().angularDrag;
		}
		InteractType = vp_InteractType.Normal;
		m_InteractManager = UnityEngine.Object.FindObjectOfType(typeof(vp_FPInteractManager)) as vp_FPInteractManager;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		if (m_IsGrabbed && !(m_Transform.parent == null))
		{
			UpdateShake();
			UpdatePosition();
			UpdateRotation();
			UpdateBurden();
			DampenForces();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (m_IsGrabbed && !(m_Transform.parent == null))
		{
			UpdateInput();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateInput()
	{
		m_CurrentMouseMove.x = m_Player.InputRawLook.Get().x * Time.timeScale;
		m_CurrentMouseMove.y = m_Player.InputRawLook.Get().y * Time.timeScale;
		if (m_Player.InputGetButtonDown.Send("Attack"))
		{
			m_Player.Interact.TryStart();
		}
		else if (m_Player.CurrentWeaponIndex.Get() != 0)
		{
			m_Player.SetWeapon.TryStart(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateShake()
	{
		m_CurrentShake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
		m_Transform.localEulerAngles += m_CurrentShake;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePosition()
	{
		m_CurrentSwayForce += m_Player.Velocity.Get() * 0.005f;
		m_CurrentSwayForce.y += m_CurrentFootstepForce;
		m_CurrentSwayForce += m_Camera.Transform.TransformDirection(new Vector3(m_CurrentMouseMove.x * 0.05f, (m_Player.Rotation.Get().x > m_Camera.RotationPitchLimit.y) ? (m_CurrentMouseMove.y * 0.015f) : (m_CurrentMouseMove.y * 0.05f), 0f));
		m_TempCarryingOffset = (m_Player.IsFirstPerson.Get() ? CarryingOffset : (CarryingOffset - m_Camera.Position3rdPersonOffset));
		m_Transform.position = Vector3.Lerp(m_Transform.position, m_Camera.Transform.position - m_CurrentSwayForce + m_Camera.Transform.right * m_TempCarryingOffset.x + m_Camera.Transform.up * m_Transform.localScale.y * (m_TempCarryingOffset.y + m_CurrentShake.y * 0.5f) + m_Camera.Transform.forward * m_TempCarryingOffset.z, (m_FetchProgress < 1f) ? m_FetchProgress : (Time.deltaTime * (Stiffness * 60f)));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateRotation()
	{
		m_Camera.RotationPitchLimit = Vector2.Lerp(m_Camera.RotationPitchLimit, new Vector2(m_Camera.RotationPitchLimit.x, CameraPitchDownLimit), m_FetchProgress);
		if (!m_DisableAngleSwayTimer.Active)
		{
			m_CurrentSwayTorque += m_Player.Velocity.Get() * 0.005f;
			m_CurrentRotationSway = m_Camera.Transform.InverseTransformDirection(m_CurrentSwayTorque * 1.5f);
			m_CurrentRotationSway.y = m_CurrentRotationSway.z;
			m_CurrentRotationSway.z = m_CurrentRotationSway.x;
			m_CurrentRotationSway.x = (0f - m_CurrentRotationSway.y) * 0.5f;
			m_CurrentRotationSway.y = 0f;
			Quaternion localRotation = m_Transform.localRotation;
			m_Transform.Rotate(m_Camera.transform.forward, m_CurrentRotationSway.z * -0.5f * Time.timeScale);
			m_Transform.Rotate(m_Camera.transform.right, m_CurrentRotationSway.x * -0.5f * Time.timeScale);
			Quaternion localRotation2 = m_Transform.localRotation;
			m_Transform.localRotation = localRotation;
			m_Transform.localRotation = Quaternion.Slerp(localRotation2, Quaternion.Euler(m_CurrentHoldAngle + m_CurrentShake * 50f), Time.deltaTime * (Stiffness * 60f));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBurden()
	{
		if (!(Burden <= 0f))
		{
			m_Player.MotorThrottle.Set(m_Player.MotorThrottle.Get() * (1f - Mathf.Clamp01(Burden)));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DampenForces()
	{
		m_CurrentSwayForce *= 0.9f;
		m_CurrentSwayTorque *= 0.9f;
		m_CurrentFootstepForce *= 0.9f;
	}

	public override bool TryInteract(vp_FPPlayerEventHandler player)
	{
		if (m_Player == null)
		{
			m_Player = player;
		}
		if (player == null)
		{
			return false;
		}
		if (m_Controller == null)
		{
			m_Controller = m_Player.GetComponent<vp_FPController>();
		}
		if (m_Controller == null)
		{
			return false;
		}
		if (m_Camera == null)
		{
			m_Camera = m_Player.GetComponentInChildren<vp_FPCamera>();
		}
		if (m_Camera == null)
		{
			return false;
		}
		if (m_WeaponHandler == null)
		{
			m_WeaponHandler = m_Player.GetComponentInChildren<vp_WeaponHandler>();
		}
		if (m_Audio == null)
		{
			m_Audio = m_Player.GetComponent<AudioSource>();
		}
		m_Player.Register(this);
		if (!m_IsGrabbed)
		{
			StartGrab();
		}
		else
		{
			StopGrab();
		}
		m_Player.Interactable.Set(this);
		if (GrabStateCrosshair != null)
		{
			m_Player.Crosshair.Set(GrabStateCrosshair);
		}
		else
		{
			m_Player.Crosshair.Set(new Texture2D(0, 0));
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartGrab()
	{
		vp_AudioUtility.PlayRandomSound(m_Audio, GrabSounds, SoundsPitch);
		if (!string.IsNullOrEmpty(OnGrabText))
		{
			m_Player.HUDText.Send(OnGrabText);
		}
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
		m_LastWeaponEquipped = m_Player.CurrentWeaponIndex.Get();
		m_OriginalPitchDownLimit = m_Camera.RotationPitchLimit.y;
		m_FetchProgress = 0f;
		if (m_LastWeaponEquipped != 0)
		{
			m_Player.SetWeapon.TryStart(0);
		}
		else if (!m_IsFetching)
		{
			StartCoroutine("Fetch");
		}
		if (m_Transform.GetComponent<Rigidbody>() != null)
		{
			m_Transform.GetComponent<Rigidbody>().useGravity = false;
			m_Transform.GetComponent<Rigidbody>().drag = Stiffness * 60f;
			m_Transform.GetComponent<Rigidbody>().angularDrag = Stiffness * 60f;
		}
		if (m_Controller.Transform.GetComponent<Collider>().enabled && m_Transform.GetComponent<Collider>().enabled)
		{
			Physics.IgnoreCollision(m_Controller.Transform.GetComponent<Collider>(), m_Transform.GetComponent<Collider>(), ignore: true);
		}
		m_Transform.parent = m_Camera.Transform;
		m_CurrentHoldAngle = m_Transform.localEulerAngles;
		m_IsGrabbed = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StopGrab()
	{
		m_IsGrabbed = false;
		m_FetchProgress = 1f;
		vp_FPCamera camera = m_Camera;
		camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Footstep));
		m_Player.SetWeapon.TryStart(m_LastWeaponEquipped);
		if (m_Transform.GetComponent<Rigidbody>() != null)
		{
			m_Transform.GetComponent<Rigidbody>().useGravity = m_DefaultGravity;
			m_Transform.GetComponent<Rigidbody>().drag = m_DefaultDrag;
			m_Transform.GetComponent<Rigidbody>().angularDrag = m_DefaultAngularDrag;
		}
		if (!m_Player.Dead.Active && vp_Utility.IsActive(m_Transform.gameObject) && m_Controller.Transform.GetComponent<Collider>().enabled && m_Transform.GetComponent<Collider>().enabled)
		{
			Physics.IgnoreCollision(m_Controller.Transform.GetComponent<Collider>(), m_Transform.GetComponent<Collider>(), ignore: false);
		}
		Vector3 eulerAngles = m_Transform.eulerAngles;
		m_Transform.parent = null;
		m_Transform.eulerAngles = eulerAngles;
		if (m_Transform.GetComponent<Rigidbody>() != null)
		{
			m_Transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
			m_Transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		if (m_Player.InputGetButtonDown.Send("Attack"))
		{
			vp_AudioUtility.PlayRandomSound(m_Audio, ThrowSounds, SoundsPitch);
			if (m_Transform.GetComponent<Rigidbody>() != null)
			{
				m_Transform.GetComponent<Rigidbody>().AddForce(m_Player.Velocity.Get() + m_Player.CameraLookDirection.Get() * ThrowStrength, ForceMode.Impulse);
				if (AllowThrowRotation)
				{
					m_Transform.GetComponent<Rigidbody>().AddTorque(m_Camera.Transform.forward * ((UnityEngine.Random.value > 0.5f) ? 0.5f : (-0.5f)) + m_Camera.Transform.right * ((UnityEngine.Random.value > 0.5f) ? 0.5f : (-0.5f)), ForceMode.Impulse);
				}
			}
		}
		else
		{
			vp_AudioUtility.PlayRandomSound(m_Audio, DropSounds, SoundsPitch);
			if (m_Transform.GetComponent<Rigidbody>() != null)
			{
				m_Transform.GetComponent<Rigidbody>().AddForce(m_Player.Velocity.Get() + m_Player.CameraLookDirection.Get(), ForceMode.Impulse);
			}
		}
		if (m_InteractManager == null)
		{
			m_InteractManager = UnityEngine.Object.FindObjectOfType(typeof(vp_FPInteractManager)) as vp_FPInteractManager;
		}
		m_InteractManager.CrosshairTimeoutTimer = Time.time + 0.5f;
		vp_Timer.In(0.1f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Camera.RotationPitchLimit.y = m_OriginalPitchDownLimit;
		});
	}

	public override void FinishInteraction()
	{
		if (m_IsGrabbed)
		{
			StopGrab();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator Fetch()
	{
		m_IsFetching = true;
		m_CurrentSwayForce = Vector3.zero;
		m_CurrentSwayTorque = Vector3.zero;
		m_CurrentFootstepForce = 0f;
		m_FetchProgress = 0f;
		duration = Vector3.Distance(m_Camera.Transform.position, m_Transform.position) * 0.5f;
		vp_Timer.In(duration + 1f, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		}, m_DisableAngleSwayTimer);
		while (m_FetchProgress < 1f)
		{
			m_FetchProgress += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
		m_IsFetching = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCollisionEnter(Collision col)
	{
		if (m_IsGrabbed)
		{
			if (m_FetchProgress < 1f)
			{
				m_FetchProgress *= 1.2f;
			}
			vp_Timer.In(2f, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
			}, m_DisableAngleSwayTimer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCollisionStay(Collision col)
	{
		if (!m_IsGrabbed || MaxCollisionCount == 0)
		{
			return;
		}
		if (col.collider != m_LastExternalCollider)
		{
			if (!col.collider.GetComponent<Rigidbody>() || col.collider.GetComponent<Rigidbody>().isKinematic)
			{
				m_LastExternalCollider = col.collider;
				m_CollisionCount = 1;
			}
			return;
		}
		if (Physics.Raycast(m_Transform.position, m_Camera.Transform.forward, out var hitInfo, 1f) && hitInfo.collider == m_LastExternalCollider && m_FetchProgress >= 1f)
		{
			m_CollisionCount = MaxCollisionCount;
		}
		m_CollisionCount++;
		if (m_CollisionCount > MaxCollisionCount && (!Physics.Raycast(col.contacts[0].point + Vector3.up * 0.1f, -Vector3.up, out hitInfo, 0.2f) || !(hitInfo.collider == m_LastExternalCollider)))
		{
			m_CollisionCount = 0;
			m_FetchProgress = 1f;
			m_LastExternalCollider = null;
			if (m_Player != null)
			{
				m_Player.Interact.TryStart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCollisionExit()
	{
		m_CurrentHoldAngle = m_Transform.localEulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Footstep()
	{
		m_CurrentFootstepForce += FootstepForce;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		m_CurrentSwayForce.y += impact * Kneeling;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		if (m_IsGrabbed && !m_IsFetching)
		{
			StartCoroutine("Fetch");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_SetWeapon()
	{
		int num = (int)m_Player.SetWeapon.Argument;
		if (!m_IsGrabbed || num == 0)
		{
			return true;
		}
		return false;
	}
}
