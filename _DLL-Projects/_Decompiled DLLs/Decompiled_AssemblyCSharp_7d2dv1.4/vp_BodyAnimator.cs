using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class vp_BodyAnimator : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_IsValid = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_ValidLookPoint = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_ValidLookPointForward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool HeadPointDirty = true;

	public GameObject HeadBone;

	public GameObject LowestSpineBone;

	[Range(0f, 90f)]
	public float HeadPitchCap = 45f;

	[Range(2f, 20f)]
	public float HeadPitchSpeed = 7f;

	[Range(0.2f, 20f)]
	public float HeadYawSpeed = 2f;

	[Range(0f, 1f)]
	public float LeaningFactor = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<GameObject> m_HeadLookBones = new List<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3> m_ReferenceUpDirs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3> m_ReferenceLookDirs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentHeadLookYaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentHeadLookPitch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<float> m_HeadLookFalloffs = new List<float>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<float> m_HeadLookCurrentFalloffs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<float> m_HeadLookTargetFalloffs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_HeadLookTargetWorldDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_HeadLookCurrentWorldDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_HeadLookBackup = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_LookPoint = Vector3.zero;

	public float FeetAdjustAngle = 80f;

	public float FeetAdjustSpeedStanding = 10f;

	public float FeetAdjustSpeedMoving = 12f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_PrevBodyYaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_BodyYaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentBodyYawTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastYaw;

	public Vector3 ClimbOffset = Vector3.forward * 0.6f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentForward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentStrafe;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentTurn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentTurnTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MaxWalkSpeed = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MaxRunSpeed = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MaxCrouchSpeed = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_WasMoving;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_GroundHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Grounded = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_AttackDoneTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NextAllowedUpdateTurnTargetTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float TURNMODIFIER = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float CROUCHTURNMODIFIER = 100f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float MOVEMODIFIER = 100f;

	public bool ShowDebugObjects;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int ForwardAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int PitchAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int StrafeAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int TurnAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int VerticalMoveAmount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsAttacking;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsClimbing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsCrouching;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsGrounded;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsMoving;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsOutOfControl;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsReloading;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsRunning;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsSettingWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int IsZooming;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int StartClimb;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int StartOutOfControl;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int StartReload;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int WeaponGripIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int WeaponTypeIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_WeaponHandler m_WeaponHandler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public SkinnedMeshRenderer m_Renderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator m_Animator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_HeadPoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_DebugLookTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_DebugLookArrow;

	public vp_WeaponHandler WeaponHandler
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_WeaponHandler == null)
			{
				m_WeaponHandler = (vp_WeaponHandler)base.transform.root.GetComponentInChildren(typeof(vp_WeaponHandler));
			}
			return m_WeaponHandler;
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

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null)
			{
				m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
			}
			return m_Player;
		}
	}

	public SkinnedMeshRenderer Renderer
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Renderer == null)
			{
				m_Renderer = base.transform.root.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			return m_Renderer;
		}
	}

	public Animator Animator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
			}
			return m_Animator;
		}
	}

	public Vector3 m_LocalVelocity
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_MathUtility.SnapToZero(Transform.root.InverseTransformDirection(Player.Velocity.Get()) / m_MaxSpeed);
		}
	}

	public float m_MaxSpeed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (Player.Run.Active)
			{
				return m_MaxRunSpeed;
			}
			if (Player.Crouch.Active)
			{
				return m_MaxCrouchSpeed;
			}
			return m_MaxWalkSpeed;
		}
	}

	public GameObject HeadPoint
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_HeadPoint == null)
			{
				m_HeadPoint = new GameObject("HeadPoint");
				m_HeadPoint.transform.parent = m_HeadLookBones[0].transform;
				m_HeadPoint.transform.localPosition = Vector3.zero;
				HeadPoint.transform.eulerAngles = Player.Rotation.Get();
			}
			return m_HeadPoint;
		}
	}

	public GameObject DebugLookTarget
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_DebugLookTarget == null)
			{
				m_DebugLookTarget = vp_3DUtility.DebugBall();
			}
			return m_DebugLookTarget;
		}
	}

	public GameObject DebugLookArrow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_DebugLookArrow == null)
			{
				m_DebugLookArrow = vp_3DUtility.DebugPointer();
				m_DebugLookArrow.transform.parent = HeadPoint.transform;
				m_DebugLookArrow.transform.localPosition = Vector3.zero;
				m_DebugLookArrow.transform.localRotation = Quaternion.identity;
				return m_DebugLookArrow;
			}
			return m_DebugLookArrow;
		}
	}

	public virtual float OnValue_BodyYaw
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Transform.eulerAngles.y;
		}
	}

	public virtual Vector3 OnValue_HeadLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (Player.LookPoint.Get() - HeadPoint.transform.position).normalized;
		}
	}

	public virtual Vector3 OnValue_LookPoint
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetLookPoint();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (Player != null)
		{
			Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (IsValidSetup())
		{
			InitHashIDs();
			InitHeadLook();
			InitMaxSpeeds();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (Time.timeScale != 0f)
		{
			if (!m_IsValid)
			{
				base.enabled = false;
				return;
			}
			UpdatePosition();
			UpdateGrounding();
			UpdateBody();
			UpdateSpine();
			UpdateAnimationSpeeds();
			UpdateAnimator();
			UpdateDebugInfo();
			UpdateHeadPoint();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAnimationSpeeds()
	{
		if (Time.time > m_NextAllowedUpdateTurnTargetTime)
		{
			m_CurrentTurnTarget = Mathf.DeltaAngle(m_PrevBodyYaw, m_BodyYaw) * (Player.Crouch.Active ? 100f : 0.2f);
			m_NextAllowedUpdateTurnTargetTime = Time.time + 0.1f;
		}
		m_CurrentTurn = Mathf.Lerp(m_CurrentTurn, m_CurrentTurnTarget, Time.deltaTime);
		if (Mathf.Round(Transform.root.eulerAngles.y) == Mathf.Round(m_LastYaw))
		{
			m_CurrentTurn *= 0.6f;
		}
		m_LastYaw = Transform.root.eulerAngles.y;
		m_CurrentTurn = vp_MathUtility.SnapToZero(m_CurrentTurn);
		m_CurrentForward = Mathf.Lerp(m_CurrentForward, m_LocalVelocity.z, Time.deltaTime * 100f);
		m_CurrentForward = ((Mathf.Abs(m_CurrentForward) > 0.03f) ? m_CurrentForward : 0f);
		if (Player.Crouch.Active)
		{
			if (Mathf.Abs(GetStrafeDirection()) < Mathf.Abs(m_CurrentTurn))
			{
				m_CurrentStrafe = Mathf.Lerp(m_CurrentStrafe, m_CurrentTurn, Time.deltaTime * 5f);
			}
			else
			{
				m_CurrentStrafe = Mathf.Lerp(m_CurrentStrafe, GetStrafeDirection(), Time.deltaTime * 5f);
			}
		}
		else
		{
			m_CurrentStrafe = Mathf.Lerp(m_CurrentStrafe, GetStrafeDirection(), Time.deltaTime * 5f);
		}
		m_CurrentStrafe = ((Mathf.Abs(m_CurrentStrafe) > 0.03f) ? m_CurrentStrafe : 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetStrafeDirection()
	{
		if (Player.InputMoveVector.Get().x < 0f)
		{
			return -1f;
		}
		if (Player.InputMoveVector.Get().x > 0f)
		{
			return 1f;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAnimator()
	{
		Animator.SetBool(IsRunning, Player.Run.Active && GetIsMoving());
		Animator.SetBool(IsCrouching, Player.Crouch.Active);
		Animator.SetInteger(WeaponTypeIndex, Player.CurrentWeaponType.Get());
		Animator.SetInteger(WeaponGripIndex, Player.CurrentWeaponGrip.Get());
		Animator.SetBool(IsSettingWeapon, Player.SetWeapon.Active);
		Animator.SetBool(IsReloading, Player.Reload.Active);
		Animator.SetBool(IsOutOfControl, Player.OutOfControl.Active);
		Animator.SetBool(IsClimbing, Player.Climb.Active);
		Animator.SetBool(IsZooming, Player.Zoom.Active);
		Animator.SetBool(IsGrounded, m_Grounded);
		Animator.SetBool(IsMoving, GetIsMoving());
		Animator.SetFloat(TurnAmount, m_CurrentTurn);
		Animator.SetFloat(ForwardAmount, m_CurrentForward);
		Animator.SetFloat(StrafeAmount, m_CurrentStrafe);
		Animator.SetFloat(PitchAmount, (0f - Player.Rotation.Get().x) / 90f);
		if (m_Grounded)
		{
			Animator.SetFloat(VerticalMoveAmount, 0f);
		}
		else if (Player.Velocity.Get().y < 0f)
		{
			Animator.SetFloat(VerticalMoveAmount, Mathf.Lerp(Animator.GetFloat(VerticalMoveAmount), -1f, Time.deltaTime * 3f));
		}
		else
		{
			Animator.SetFloat(VerticalMoveAmount, Player.MotorThrottle.Get().y * 10f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateDebugInfo()
	{
		if (ShowDebugObjects)
		{
			DebugLookTarget.transform.position = m_HeadLookBones[0].transform.position + HeadPoint.transform.forward * 1000f;
			DebugLookArrow.transform.LookAt(DebugLookTarget.transform.position);
			if (!vp_Utility.IsActive(m_DebugLookTarget))
			{
				vp_Utility.Activate(m_DebugLookTarget);
			}
			if (!vp_Utility.IsActive(m_DebugLookArrow))
			{
				vp_Utility.Activate(m_DebugLookArrow);
			}
		}
		else
		{
			if (m_DebugLookTarget != null)
			{
				vp_Utility.Activate(m_DebugLookTarget, activate: false);
			}
			if (m_DebugLookArrow != null)
			{
				vp_Utility.Activate(m_DebugLookArrow, activate: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadPoint()
	{
		if (HeadPointDirty)
		{
			HeadPoint.transform.eulerAngles = Player.Rotation.Get();
			HeadPointDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePosition()
	{
		if (!Player.IsFirstPerson.Get() && Player.Climb.Active)
		{
			Transform.localPosition += ClimbOffset;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBody()
	{
		m_PrevBodyYaw = m_BodyYaw;
		m_BodyYaw = Mathf.LerpAngle(m_BodyYaw, m_CurrentBodyYawTarget, Time.deltaTime * ((Player.Velocity.Get().magnitude > 0.1f) ? FeetAdjustSpeedMoving : FeetAdjustSpeedStanding));
		m_BodyYaw = ((m_BodyYaw < -360f) ? (m_BodyYaw += 360f) : m_BodyYaw);
		m_BodyYaw = ((m_BodyYaw > 360f) ? (m_BodyYaw -= 360f) : m_BodyYaw);
		Transform.eulerAngles = m_BodyYaw * Vector3.up;
		m_CurrentHeadLookYaw = Mathf.DeltaAngle(Player.Rotation.Get().y, Transform.eulerAngles.y);
		if (Mathf.Max(0f, m_CurrentHeadLookYaw - 90f) > 0f)
		{
			Transform.eulerAngles = Vector3.up * (Transform.root.eulerAngles.y + 90f);
			m_BodyYaw = (m_CurrentBodyYawTarget = Transform.eulerAngles.y);
		}
		else if (Mathf.Min(0f, m_CurrentHeadLookYaw - -90f) < 0f)
		{
			Transform.eulerAngles = Vector3.up * (Transform.root.eulerAngles.y - 90f);
			m_BodyYaw = (m_CurrentBodyYawTarget = Transform.eulerAngles.y);
		}
		if (Mathf.Abs(Player.Rotation.Get().y - m_BodyYaw) > 180f)
		{
			if (m_BodyYaw > 0f)
			{
				m_BodyYaw -= 360f;
				m_PrevBodyYaw -= 360f;
			}
			else if (m_BodyYaw < 0f)
			{
				m_BodyYaw += 360f;
				m_PrevBodyYaw += 360f;
			}
		}
		if (m_CurrentHeadLookYaw > FeetAdjustAngle || m_CurrentHeadLookYaw < 0f - FeetAdjustAngle || Player.Velocity.Get().magnitude > 0.1f)
		{
			m_CurrentBodyYawTarget = Mathf.LerpAngle(m_CurrentBodyYawTarget, Transform.root.eulerAngles.y, 0.1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSpine()
	{
		for (int i = 0; i < m_HeadLookBones.Count; i++)
		{
			if (Player.IsFirstPerson.Get() || Animator.GetBool(IsAttacking) || Animator.GetBool(IsZooming))
			{
				m_HeadLookTargetFalloffs[i] = m_HeadLookFalloffs[m_HeadLookFalloffs.Count - 1 - i];
			}
			else
			{
				m_HeadLookTargetFalloffs[i] = m_HeadLookFalloffs[i];
			}
			if (m_WasMoving && !Animator.GetBool(IsMoving))
			{
				m_HeadLookCurrentFalloffs[i] = m_HeadLookTargetFalloffs[i];
			}
			m_HeadLookCurrentFalloffs[i] = Mathf.SmoothStep(m_HeadLookCurrentFalloffs[i], Mathf.LerpAngle(m_HeadLookCurrentFalloffs[i], m_HeadLookTargetFalloffs[i], Time.deltaTime * 10f), Time.deltaTime * 20f);
			if (Player.IsFirstPerson.Get())
			{
				m_HeadLookTargetWorldDir = GetLookPoint() - m_HeadLookBones[0].transform.position;
				m_HeadLookCurrentWorldDir = Vector3.Slerp(m_HeadLookTargetWorldDir, vp_3DUtility.HorizontalVector(m_HeadLookTargetWorldDir), m_HeadLookCurrentFalloffs[i] / m_HeadLookFalloffs[0]);
			}
			else
			{
				m_ValidLookPoint = GetLookPoint();
				m_ValidLookPointForward = Transform.InverseTransformDirection(m_ValidLookPoint - m_HeadLookBones[0].transform.position).z;
				if (m_ValidLookPointForward < 0f)
				{
					m_ValidLookPoint += Transform.forward * (0f - m_ValidLookPointForward);
				}
				m_HeadLookTargetWorldDir = Vector3.Slerp(m_HeadLookTargetWorldDir, m_ValidLookPoint - m_HeadLookBones[0].transform.position, Time.deltaTime * HeadYawSpeed);
				m_HeadLookCurrentWorldDir = Vector3.Slerp(m_HeadLookCurrentWorldDir, vp_3DUtility.HorizontalVector(m_HeadLookTargetWorldDir), m_HeadLookCurrentFalloffs[i] / m_HeadLookFalloffs[0]);
			}
			m_HeadLookBones[i].transform.rotation = vp_3DUtility.GetBoneLookRotationInWorldSpace(m_HeadLookBones[i].transform.rotation, m_HeadLookBones[m_HeadLookBones.Count - 1].transform.parent.rotation, m_HeadLookCurrentWorldDir, m_HeadLookCurrentFalloffs[i], m_ReferenceLookDirs[i], m_ReferenceUpDirs[i], Quaternion.identity);
			if (!Player.IsFirstPerson.Get())
			{
				m_CurrentHeadLookPitch = Mathf.SmoothStep(m_CurrentHeadLookPitch, Mathf.Clamp(Player.Rotation.Get().x, 0f - HeadPitchCap, HeadPitchCap), Time.deltaTime * HeadPitchSpeed);
				m_HeadLookBones[i].transform.Rotate(HeadPoint.transform.right, m_CurrentHeadLookPitch * Mathf.Lerp(m_HeadLookFalloffs[i], m_HeadLookCurrentFalloffs[i], LeaningFactor), Space.World);
			}
		}
		m_WasMoving = Animator.GetBool(IsMoving);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool GetIsMoving()
	{
		return Vector3.Scale(Player.Velocity.Get(), Vector3.right + Vector3.forward).magnitude > 0.01f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetLookPoint()
	{
		m_HeadLookBackup = HeadPoint.transform.eulerAngles;
		HeadPoint.transform.eulerAngles = Player.Rotation.Get();
		m_LookPoint = HeadPoint.transform.position + HeadPoint.transform.forward * 1000f;
		HeadPoint.transform.eulerAngles = m_HeadLookBackup;
		return m_LookPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual List<float> CalculateBoneFalloffs(List<GameObject> boneList)
	{
		List<float> list = new List<float>();
		float num = 0f;
		for (int num2 = boneList.Count - 1; num2 > -1; num2--)
		{
			if (boneList[num2] == null)
			{
				boneList.RemoveAt(num2);
			}
			else
			{
				float num3 = Mathf.Lerp(0f, 1f, (float)(num2 + 1) / (float)boneList.Count);
				list.Add(num3 * num3 * num3);
				num += num3 * num3 * num3;
			}
		}
		if (boneList.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i] *= 1f / num;
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StoreReferenceDirections()
	{
		for (int i = 0; i < m_HeadLookBones.Count; i++)
		{
			Quaternion quaternion = Quaternion.Inverse(m_HeadLookBones[m_HeadLookBones.Count - 1].transform.parent.rotation);
			m_ReferenceLookDirs.Add(quaternion * Transform.rotation * Vector3.forward);
			m_ReferenceUpDirs.Add(quaternion * Transform.rotation * Vector3.up);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateGrounding()
	{
		Physics.SphereCast(new Ray(Transform.position + Vector3.up * 0.5f, Vector3.down), 0.4f, out m_GroundHit, 1f, 1084850176);
		m_Grounded = m_GroundHit.collider != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshWeaponStates()
	{
		if (!(WeaponHandler == null) && !(WeaponHandler.CurrentWeapon == null))
		{
			WeaponHandler.CurrentWeapon.SetState("Attack", Player.Attack.Active);
			WeaponHandler.CurrentWeapon.SetState("Zoom", Player.Zoom.Active);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMaxSpeeds()
	{
		vp_FPController vp_FPController2 = UnityEngine.Object.FindObjectOfType<vp_FPController>();
		if (vp_FPController2 != null)
		{
			m_MaxWalkSpeed = vp_FPController2.CalculateMaxSpeed();
			m_MaxRunSpeed = vp_FPController2.CalculateMaxSpeed("Run");
			m_MaxCrouchSpeed = vp_FPController2.CalculateMaxSpeed("Crouch");
		}
		else
		{
			m_MaxWalkSpeed = 3.999999f;
			m_MaxRunSpeed = 10.08f;
			m_MaxCrouchSpeed = 1.44f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHashIDs()
	{
		ForwardAmount = Animator.StringToHash("Forward");
		PitchAmount = Animator.StringToHash("Pitch");
		StrafeAmount = Animator.StringToHash("Strafe");
		TurnAmount = Animator.StringToHash("Turn");
		VerticalMoveAmount = Animator.StringToHash("VerticalMove");
		IsAttacking = Animator.StringToHash("IsAttacking");
		IsClimbing = Animator.StringToHash("IsClimbing");
		IsCrouching = Animator.StringToHash("IsCrouching");
		IsGrounded = Animator.StringToHash("IsGrounded");
		IsMoving = Animator.StringToHash("IsMoving");
		IsOutOfControl = Animator.StringToHash("IsOutOfControl");
		IsReloading = Animator.StringToHash("IsReloading");
		IsRunning = Animator.StringToHash("IsRunning");
		IsSettingWeapon = Animator.StringToHash("IsSettingWeapon");
		IsZooming = Animator.StringToHash("IsZooming");
		StartClimb = Animator.StringToHash("StartClimb");
		StartOutOfControl = Animator.StringToHash("StartOutOfControl");
		StartReload = Animator.StringToHash("StartReload");
		WeaponGripIndex = Animator.StringToHash("WeaponGrip");
		WeaponTypeIndex = Animator.StringToHash("WeaponType");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHeadLook()
	{
		if (m_IsValid)
		{
			m_HeadLookBones.Clear();
			GameObject headBone = HeadBone;
			while (headBone != LowestSpineBone.transform.parent.gameObject)
			{
				m_HeadLookBones.Add(headBone);
				headBone = headBone.transform.parent.gameObject;
			}
			m_ReferenceUpDirs = new List<Vector3>();
			m_ReferenceLookDirs = new List<Vector3>();
			m_HeadLookFalloffs = CalculateBoneFalloffs(m_HeadLookBones);
			m_HeadLookCurrentFalloffs = new List<float>(m_HeadLookFalloffs);
			m_HeadLookTargetFalloffs = new List<float>(m_HeadLookFalloffs);
			StoreReferenceDirections();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidSetup()
	{
		if (HeadBone == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") No gameobject has been assigned for 'HeadBone'.");
		}
		else if (LowestSpineBone == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") No gameobject has been assigned for 'LowestSpineBone'.");
		}
		else if (!vp_Utility.IsDescendant(HeadBone.transform, base.transform.root))
		{
			NotInSameHierarchyError(HeadBone);
		}
		else if (!vp_Utility.IsDescendant(LowestSpineBone.transform, base.transform.root))
		{
			NotInSameHierarchyError(LowestSpineBone);
		}
		else
		{
			if (vp_Utility.IsDescendant(HeadBone.transform, LowestSpineBone.transform))
			{
				return true;
			}
			Debug.LogError("Error (" + this?.ToString() + ") 'HeadBone' must be a child or descendant of 'LowestSpineBone'.");
		}
		m_IsValid = false;
		base.enabled = false;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NotInSameHierarchyError(GameObject o)
	{
		Debug.LogError("Error '" + o?.ToString() + "' can not be used as a bone for  " + this?.ToString() + " because it is not part of the same hierarchy.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_Attack()
	{
		m_AttackDoneTimer.Cancel();
		Animator.SetBool(IsAttacking, value: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Attack()
	{
		vp_Timer.In(0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Animator.SetBool(IsAttacking, value: false);
			RefreshWeaponStates();
		}, m_AttackDoneTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Zoom()
	{
		vp_Timer.In(0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (!Player.Attack.Active)
			{
				Animator.SetBool(IsAttacking, value: false);
			}
			RefreshWeaponStates();
		}, m_AttackDoneTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Reload()
	{
		Animator.SetTrigger(StartReload);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_OutOfControl()
	{
		Animator.SetTrigger(StartOutOfControl);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Climb()
	{
		Animator.SetTrigger(StartClimb);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (m_AttackDoneTimer.Active)
		{
			m_AttackDoneTimer.Execute();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		HeadPointDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		m_WasMoving = !m_WasMoving;
		HeadPointDirty = true;
	}
}
