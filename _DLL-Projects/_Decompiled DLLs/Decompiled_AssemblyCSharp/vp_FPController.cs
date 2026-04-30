using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(vp_FPPlayerEventHandler))]
[RequireComponent(typeof(CharacterController))]
[Preserve]
public class vp_FPController : vp_Component
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Player;

	public EntityPlayerLocal localPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterController m_CharacterController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_FixedPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_SmoothPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_SpeedModifier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Grounded;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_HeadContact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_GroundHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_LastGroundHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_CeilingHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_WallHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FallImpact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Terrain m_CurrentTerrain;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_SurfaceIdentifier m_CurrentSurface;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CollisionFlags LastMoveCollisionFlags;

	public float MotorAcceleration = 0.18f;

	public float MotorDamping = 0.17f;

	public float MotorBackwardsSpeed = 0.65f;

	public float MotorSidewaysSpeed = 0.65f;

	public float MotorAirSpeed = 0.35f;

	public float MotorSlopeSpeedUp = 1f;

	public float MotorSlopeSpeedDown = 1f;

	public bool MotorFreeFly;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_MoveDirection = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_SlopeFactor = 1f;

	public Vector3 m_MotorThrottle = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MotorAirSpeedModifier = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentAntiBumpOffset;

	public float originalMotorJumpForce = -1f;

	public float MotorJumpForce = 0.18f;

	public float MotorJumpForceDamping = 0.08f;

	public float MotorJumpForceHold = 0.003f;

	public float MotorJumpForceHoldDamping = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_MotorJumpForceHoldSkipFrames;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MotorJumpForceAcc;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_MotorJumpDone = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FallSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_MaxHeight = float.MinValue;

	public float m_MaxHeightInitialFallSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_GravityForce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 slideLastGroundN;

	public float PhysicsForceDamping = 0.05f;

	public float PhysicsPushForce = 5f;

	public float PhysicsGravityModifier = 0.2f;

	public float PhysicsSlopeSlideLimit = 30f;

	public float PhysicsSlopeSlidiness = 0.15f;

	public float PhysicsWallBounce;

	public float PhysicsWallFriction;

	public float PhysicsCrouchHeightModifier = 0.5f;

	public bool PhysicsHasCollisionTrigger = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_Trigger;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CapsuleCollider m_TriggerCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_ExternalForce = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] m_SmoothForceFrame = new Vector3[120];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Slide;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_SlideFast;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_SlideFallSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_OnSteepGroundSince;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_SlopeSlideSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PredictedPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PrevPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PrevDir = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_NewDir = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_ForceImpact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_ForceMultiplier;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 CapsuleBottom = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 CapsuleTop = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_StepHeight = 0.7f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_SkinWidth = 0.08f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Platform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PositionOnPlatform = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastPlatformAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_LastPlatformPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NormalHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_NormalCenter = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CrouchHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CrouchCenter = Vector3.zero;

	public vp_FPPlayerEventHandler Player
	{
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	public CharacterController CharacterController
	{
		get
		{
			if (m_CharacterController == null)
			{
				m_CharacterController = base.gameObject.GetComponent<CharacterController>();
			}
			return m_CharacterController;
		}
	}

	public Vector3 SmoothPosition => m_SmoothPosition;

	public Vector3 Velocity => m_CharacterController.velocity;

	public float SpeedModifier
	{
		get
		{
			return m_SpeedModifier;
		}
		set
		{
			m_SpeedModifier = value;
		}
	}

	public bool Grounded => m_Grounded;

	public bool HeadContact => m_HeadContact;

	public Vector3 GroundNormal => m_GroundHit.normal;

	public float GroundAngle => Vector3.Angle(m_GroundHit.normal, Vector3.up);

	public Transform GroundTransform => m_GroundHit.transform;

	public bool IsCollidingWall => m_WallHit.collider != null;

	public float ProjectedWallMove
	{
		get
		{
			if (!(m_WallHit.collider != null))
			{
				return 1f;
			}
			return m_ForceMultiplier;
		}
	}

	public Vector3 FixedPosition => m_FixedPosition;

	public float SkinWidth => m_SkinWidth;

	public virtual Vector3 OnValue_Position
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return base.Transform.position;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			SetPosition(value);
		}
	}

	public virtual Vector3 OnValue_Velocity
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CharacterController.velocity;
		}
	}

	public virtual float OnValue_StepOffset
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CharacterController.stepOffset;
		}
	}

	public virtual float OnValue_SlopeLimit
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CharacterController.slopeLimit;
		}
	}

	public virtual float OnValue_Radius
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CharacterController.radius;
		}
	}

	public virtual float OnValue_Height
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CharacterController.height;
		}
	}

	public virtual Vector3 OnValue_MotorThrottle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_MotorThrottle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_MotorThrottle = value;
		}
	}

	public virtual bool OnValue_MotorJumpDone
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_MotorJumpDone;
		}
	}

	public virtual float OnValue_FallSpeed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_FallSpeed;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_FallSpeed = value;
		}
	}

	public virtual Transform OnValue_Platform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_Platform;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_Platform = value;
		}
	}

	public virtual Texture OnValue_GroundTexture
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (GroundTransform == null)
			{
				return null;
			}
			if (GroundTransform.GetComponent<Renderer>() == null && m_CurrentTerrain == null)
			{
				return null;
			}
			int num = -1;
			if (m_CurrentTerrain != null)
			{
				num = vp_FootstepManager.GetMainTerrainTexture(Player.Position.Get(), m_CurrentTerrain);
				if (num > m_CurrentTerrain.terrainData.terrainLayers.Length - 1)
				{
					return null;
				}
			}
			if (!(m_CurrentTerrain == null))
			{
				return m_CurrentTerrain.terrainData.terrainLayers[num].diffuseTexture;
			}
			return GroundTransform.GetComponent<Renderer>().material.mainTexture;
		}
	}

	public virtual vp_SurfaceIdentifier OnValue_SurfaceType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_CurrentSurface;
		}
	}

	public void Reposition(Vector3 _deltaVec)
	{
		m_SmoothPosition += _deltaVec;
		SetPosition(m_SmoothPosition);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		SyncCharacterController();
		if (originalMotorJumpForce == -1f)
		{
			originalMotorJumpForce = MotorJumpForce;
		}
	}

	public void SyncCharacterController()
	{
		m_NormalHeight = CharacterController.height;
		CharacterController.center = (m_NormalCenter = new Vector3(0f, m_NormalHeight * 0.5f, 0f));
		CharacterController.radius = m_NormalHeight * 0.16666f;
		m_CrouchHeight = m_NormalHeight * PhysicsCrouchHeightModifier;
		m_CrouchCenter = m_NormalCenter * PhysicsCrouchHeightModifier;
		m_StepHeight = CharacterController.stepOffset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		vp_TargetEvent<Vector3>.Register(m_Transform, "ForceImpact", AddForce);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		vp_TargetEvent<Vector3>.Unregister(m_Root, "ForceImpact", AddForce);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		SetPosition(base.Transform.position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		SmoothMove();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			if (Player.Driving.Active)
			{
				m_PrevPos = m_FixedPosition;
				m_FixedPosition = base.Transform.position;
				return;
			}
			base.Transform.position = m_FixedPosition;
			UpdateMotor();
			UpdateJump();
			UpdateForces();
			UpdateSliding();
			UpdateOutOfControl();
			FixedMove();
			UpdateCollisions();
			UpdatePlatformMove();
			m_PrevPos = base.Transform.position;
			m_FixedPosition = base.Transform.position;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateMotor()
	{
		if (!MotorFreeFly)
		{
			UpdateThrottleWalk();
			m_MotorThrottle = vp_MathUtility.SnapToZero(m_MotorThrottle);
		}
		else
		{
			localPlayer.SwimModeUpdateThrottle();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateThrottleWalk()
	{
		UpdateSlopeFactor();
		m_MotorAirSpeedModifier = (m_Grounded ? 1f : MotorAirSpeed);
		Vector2 vector = Player.InputMoveVector.Get();
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		float num = 0.1f * MotorAcceleration * m_MotorAirSpeedModifier * m_SlopeFactor;
		if (!Player.IsFirstPerson.Get() && Player.CameraRelativeMovement3P.Get())
		{
			float z = vector.y * num;
			float x = vector.x * num;
			Vector3 vector2 = new Vector3(x, 0f, z);
			m_MotorThrottle += vector2;
		}
		else
		{
			float num2 = ((vector.y > 0f) ? vector.y : (vector.y * MotorBackwardsSpeed)) * num;
			float num3 = vector.x * MotorSidewaysSpeed * num;
			m_MotorThrottle += base.Transform.TransformDirection(Vector3.forward) * num2 + base.Transform.TransformDirection(Vector3.right) * num3;
		}
		m_MotorThrottle.x /= 1f + MotorDamping * m_MotorAirSpeedModifier * Time.timeScale;
		m_MotorThrottle.z /= 1f + MotorDamping * m_MotorAirSpeedModifier * Time.timeScale;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateThrottleFree()
	{
		Vector3 vector = Player.CameraLookDirection.Get();
		vector.y *= 2f;
		vector.Normalize();
		m_MotorThrottle += Player.InputMoveVector.Get().y * vector * (MotorAcceleration * 0.1f);
		m_MotorThrottle += Player.InputMoveVector.Get().x * base.Transform.TransformDirection(Vector3.right * (MotorAcceleration * 0.1f));
		m_MotorThrottle.x /= 1f + MotorDamping * Time.timeScale;
		m_MotorThrottle.z /= 1f + MotorDamping * Time.timeScale;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJump()
	{
		if (m_HeadContact)
		{
			Player.Jump.Stop(1f);
		}
		if (!MotorFreeFly)
		{
			UpdateJumpForceWalk();
		}
		else
		{
			UpdateJumpForceFree();
		}
		m_MotorThrottle.y += m_MotorJumpForceAcc * Time.timeScale;
		m_MotorJumpForceAcc /= 1f + MotorJumpForceHoldDamping * Time.timeScale;
		m_MotorThrottle.y /= 1f + MotorJumpForceDamping * Time.timeScale;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJumpForceWalk()
	{
		if (!Player.Jump.Active || m_Grounded)
		{
			return;
		}
		if (m_MotorJumpForceHoldSkipFrames > 2)
		{
			if (!(Player.Velocity.Get().y < 0f))
			{
				m_MotorJumpForceAcc += MotorJumpForceHold;
			}
		}
		else
		{
			m_MotorJumpForceHoldSkipFrames++;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJumpForceFree()
	{
		if (!Player.Jump.Active || !Player.Crouch.Active)
		{
			if (Player.Jump.Active)
			{
				m_MotorJumpForceAcc += MotorJumpForceHold;
			}
			else if (Player.Crouch.Active && Grounded && CharacterController.height == m_NormalHeight)
			{
				CharacterController.height = m_CrouchHeight;
				CharacterController.center = m_CrouchCenter;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateForces()
	{
		m_GravityForce = Physics.gravity.y * (PhysicsGravityModifier * 0.002f) * vp_TimeUtility.AdjustedTimeScale;
		if (m_Grounded && m_FallSpeed <= 0f)
		{
			m_FallSpeed = m_GravityForce;
		}
		else
		{
			m_FallSpeed += m_GravityForce;
		}
		if (m_SmoothForceFrame[0] != Vector3.zero)
		{
			AddForceInternal(m_SmoothForceFrame[0]);
			for (int i = 0; i < 120; i++)
			{
				m_SmoothForceFrame[i] = ((i < 119) ? m_SmoothForceFrame[i + 1] : Vector3.zero);
				if (m_SmoothForceFrame[i] == Vector3.zero)
				{
					break;
				}
			}
		}
		m_ExternalForce /= 1f + PhysicsForceDamping * vp_TimeUtility.AdjustedTimeScale;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSliding()
	{
		bool slideFast = m_SlideFast;
		bool slide = m_Slide;
		m_Slide = false;
		if (!m_Grounded)
		{
			m_OnSteepGroundSince = 0f;
			m_SlideFast = false;
		}
		else if (GroundAngle > PhysicsSlopeSlideLimit)
		{
			m_Slide = true;
			if (GroundAngle <= Player.SlopeLimit.Get())
			{
				m_SlopeSlideSpeed = Mathf.Max(m_SlopeSlideSpeed, PhysicsSlopeSlidiness * 0.01f);
				m_OnSteepGroundSince = 0f;
				m_SlideFast = false;
				m_SlopeSlideSpeed = ((Mathf.Abs(m_SlopeSlideSpeed) < 0.0001f) ? 0f : (m_SlopeSlideSpeed / (1f + 0.05f * vp_TimeUtility.AdjustedTimeScale)));
			}
			else
			{
				if (m_SlopeSlideSpeed > 0.01f)
				{
					m_SlideFast = true;
				}
				if (m_OnSteepGroundSince == 0f)
				{
					m_OnSteepGroundSince = Time.time;
					slideLastGroundN = Vector3.up;
				}
				m_SlopeSlideSpeed += PhysicsSlopeSlidiness * 0.01f * 5f * Time.deltaTime * vp_TimeUtility.AdjustedTimeScale;
				m_SlopeSlideSpeed *= 0.97f;
				float num = Vector3.Dot(GroundNormal, slideLastGroundN);
				slideLastGroundN = GroundNormal;
				if (num < 0.2f)
				{
					float num2 = 0.7f;
					if (num < -0.2f)
					{
						num2 = 0.2f;
					}
					m_SlopeSlideSpeed *= num2;
					m_ExternalForce *= num2;
				}
			}
			AddForce(Vector3.Cross(Vector3.Cross(GroundNormal, Vector3.down), GroundNormal) * m_SlopeSlideSpeed * vp_TimeUtility.AdjustedTimeScale);
		}
		else
		{
			m_OnSteepGroundSince = 0f;
			m_SlideFast = false;
			m_SlopeSlideSpeed = 0f;
		}
		if (m_MotorThrottle != Vector3.zero)
		{
			m_Slide = false;
		}
		if (m_SlideFast)
		{
			m_SlideFallSpeed = base.Transform.position.y;
		}
		else if (slideFast && !Grounded)
		{
			m_FallSpeed = Mathf.Min(0f, base.Transform.position.y - m_SlideFallSpeed);
		}
		if (slide != m_Slide)
		{
			Player.SetState("Slide", m_Slide);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOutOfControl()
	{
		if (m_ExternalForce.magnitude > 0.2f || m_FallSpeed < -0.2f || m_SlideFast)
		{
			Player.OutOfControl.Start();
		}
		else if (Player.OutOfControl.Active)
		{
			Player.OutOfControl.Stop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedMove()
	{
		Physics.SyncTransforms();
		m_MoveDirection = Vector3.zero;
		m_MoveDirection += m_ExternalForce;
		m_MoveDirection += m_MotorThrottle;
		m_MoveDirection.x *= m_SpeedModifier;
		m_MoveDirection.z *= m_SpeedModifier;
		m_MoveDirection.y += m_FallSpeed;
		if (MotorFreeFly && m_MoveDirection.y < 0f)
		{
			m_MotorThrottle.y += m_FallSpeed;
			m_FallSpeed = 0f;
		}
		if (MotorFreeFly)
		{
			m_MaxHeight = float.MinValue;
			m_MaxHeightInitialFallSpeed = 0f;
		}
		else
		{
			float num = base.Transform.position.y + Origin.position.y;
			if (m_Grounded || num > m_MaxHeight)
			{
				m_MaxHeight = num;
			}
		}
		bool flag = m_Grounded;
		if (flag)
		{
			Vector3 vector = m_MoveDirection * base.Delta * Time.timeScale;
			vector.y = 0f;
			if (vector != Vector3.zero)
			{
				vector = vector.normalized;
				float maxDistance = m_CharacterController.radius * 2f + 0.5f;
				if (Physics.SphereCast(new Ray(m_Transform.position + new Vector3(0f, m_CharacterController.height + 0.2f, 0f) - vector * m_CharacterController.radius, vector), 0.05f, maxDistance, 1073807360))
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			m_CharacterController.stepOffset = m_StepHeight;
		}
		else
		{
			m_CharacterController.stepOffset = 0.1f;
		}
		m_CurrentAntiBumpOffset = 0f;
		if (m_Grounded && m_MotorThrottle.y <= 0.001f)
		{
			m_CurrentAntiBumpOffset = Mathf.Max(0.1f, Vector3.Scale(m_MoveDirection, Vector3.one - Vector3.up).magnitude);
			m_MoveDirection.y -= m_CurrentAntiBumpOffset;
		}
		m_PredictedPos = base.Transform.position + vp_MathUtility.NaNSafeVector3(m_MoveDirection * base.Delta * Time.timeScale);
		if (m_Platform != null && m_PositionOnPlatform != Vector3.zero)
		{
			Player.Move.Send(vp_MathUtility.NaNSafeVector3(m_Platform.TransformPoint(m_PositionOnPlatform) - m_Transform.position));
		}
		Player.Move.Send(vp_MathUtility.NaNSafeVector3(m_MoveDirection * base.Delta * Time.timeScale));
		if (Player.Dead.Active)
		{
			Player.InputMoveVector.Set(Vector2.zero);
		}
		Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * m_CharacterController.radius, Vector3.down), m_CharacterController.radius, out m_GroundHit, m_SkinWidth + 0.001f, 1084850176);
		m_Grounded = m_GroundHit.collider != null;
		if (!m_Grounded && Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * m_CharacterController.radius, Vector3.down), m_CharacterController.radius, out var hitInfo, (m_CharacterController.skinWidth + 0.001f) * 4f, 1084850176) && hitInfo.collider is CharacterController)
		{
			m_Grounded = true;
			m_GroundHit = hitInfo;
		}
		if (!m_Grounded && Player.Velocity.Get().y > 0f)
		{
			Physics.SphereCast(new Ray(base.Transform.position, Vector3.up), m_CharacterController.radius, out m_CeilingHit, m_CharacterController.height - (m_CharacterController.radius - m_CharacterController.skinWidth) + 0.01f, 1084850176);
			m_HeadContact = m_CeilingHit.collider != null;
		}
		else
		{
			m_HeadContact = false;
		}
		if (m_GroundHit.transform == null && m_LastGroundHit.transform != null)
		{
			if (m_Platform != null && m_PositionOnPlatform != Vector3.zero)
			{
				AddForce(m_Platform.position - m_LastPlatformPos);
				m_Platform = null;
			}
			if (m_CurrentAntiBumpOffset != 0f)
			{
				Player.Move.Send(vp_MathUtility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * base.Delta * Time.timeScale);
				m_PredictedPos += vp_MathUtility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * base.Delta * Time.timeScale;
				m_MoveDirection.y += m_CurrentAntiBumpOffset;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SmoothMove()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (Player.Driving.Active)
		{
			m_FixedPosition = base.Transform.position;
			m_SmoothPosition = base.Transform.position;
			return;
		}
		m_FixedPosition = base.Transform.position;
		base.Transform.position = m_SmoothPosition;
		Physics.SyncTransforms();
		Player.Move.Send(vp_MathUtility.NaNSafeVector3(m_MoveDirection * base.Delta * Time.timeScale));
		m_SmoothPosition = base.Transform.position;
		if (Vector3.Distance(base.Transform.position, m_SmoothPosition) > Player.Radius.Get())
		{
			m_SmoothPosition = m_FixedPosition;
		}
		if (m_Platform != null && (m_LastPlatformPos.y < m_Platform.position.y || m_LastPlatformPos.y > m_Platform.position.y))
		{
			m_SmoothPosition.y = base.Transform.position.y;
		}
		m_SmoothPosition = Vector3.Lerp(m_SmoothPosition, m_FixedPosition, Time.deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCollisions()
	{
		if (m_GroundHit.transform != null && m_GroundHit.transform != m_LastGroundHit.transform)
		{
			if (m_LastGroundHit.transform == null)
			{
				float fallImpact = (0f - CharacterController.velocity.y * 0.01f) * Time.timeScale;
				float num = base.Transform.position.y + Origin.position.y;
				float num2 = Mathf.Round((m_MaxHeight - num) * 2f) / 2f;
				if (!MotorFreeFly && num2 >= 0f)
				{
					float num3 = Math.Abs(m_GravityForce);
					m_FallImpact = (float)Math.Sqrt(Math.Pow(m_MaxHeightInitialFallSpeed, 2.0) + (double)(2f * num3 * num2)) * 0.8677499f;
				}
				else
				{
					m_FallImpact = fallImpact;
				}
				m_MaxHeight = float.MinValue;
				m_MaxHeightInitialFallSpeed = 0f;
				m_SmoothPosition.y = base.Transform.position.y;
				DeflectDownForce();
				Player.FallImpact.Send(m_FallImpact);
				Player.FallImpact2.Send(m_FallImpact);
				m_MotorThrottle.y = 0f;
				m_MotorJumpForceAcc = 0f;
				m_MotorJumpForceHoldSkipFrames = 0;
			}
			if (m_GroundHit.collider.gameObject.layer == 28)
			{
				m_Platform = m_GroundHit.transform;
				m_LastPlatformAngle = m_Platform.eulerAngles.y;
			}
			else
			{
				m_Platform = null;
			}
			Terrain component = m_GroundHit.transform.GetComponent<Terrain>();
			if (component != null)
			{
				m_CurrentTerrain = component;
			}
			else
			{
				m_CurrentTerrain = null;
			}
			vp_SurfaceIdentifier component2 = m_GroundHit.transform.GetComponent<vp_SurfaceIdentifier>();
			if (component2 != null)
			{
				m_CurrentSurface = component2;
			}
			else
			{
				m_CurrentSurface = null;
			}
		}
		else
		{
			m_FallImpact = 0f;
		}
		m_LastGroundHit = m_GroundHit;
		if (m_PredictedPos.y > base.Transform.position.y && (m_ExternalForce.y > 0f || m_MotorThrottle.y > 0f))
		{
			DeflectUpForce();
		}
		if (m_PredictedPos.x != base.Transform.position.x || m_PredictedPos.z != base.Transform.position.z)
		{
			DeflectHorizontalForce();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSlopeFactor()
	{
		if (!m_Grounded)
		{
			m_SlopeFactor = 1f;
			return;
		}
		Vector3 motorThrottle = m_MotorThrottle;
		motorThrottle.y = 0f;
		float num = Vector3.Angle(m_GroundHit.normal, motorThrottle);
		m_SlopeFactor = 1f + (1f - num / 90f);
		if (Mathf.Abs(1f - m_SlopeFactor) < 0.25f)
		{
			m_SlopeFactor = 1f;
		}
		else if (m_SlopeFactor > 1f)
		{
			if (MotorSlopeSpeedDown == 1f)
			{
				m_SlopeFactor = 1f / m_SlopeFactor;
				m_SlopeFactor *= 1.2f;
			}
			else
			{
				m_SlopeFactor *= MotorSlopeSpeedDown;
			}
		}
		else
		{
			if (MotorSlopeSpeedUp == 1f)
			{
				m_SlopeFactor *= 1.2f;
			}
			else
			{
				m_SlopeFactor *= MotorSlopeSpeedUp;
			}
			m_SlopeFactor = ((GroundAngle > Player.SlopeLimit.Get()) ? 0f : m_SlopeFactor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePlatformMove()
	{
		if (!(m_Platform == null))
		{
			m_PositionOnPlatform = m_Platform.InverseTransformPoint(m_Transform.position);
			Player.Rotation.Set(new Vector2(Player.Rotation.Get().x, Player.Rotation.Get().y - Mathf.DeltaAngle(m_Platform.eulerAngles.y, m_LastPlatformAngle)));
			m_LastPlatformAngle = m_Platform.eulerAngles.y;
			m_LastPlatformPos = m_Platform.position;
			m_SmoothPosition = base.Transform.position;
		}
	}

	public virtual void SetPosition(Vector3 position)
	{
		base.Transform.position = position;
		m_PrevPos = position;
		m_SmoothPosition = position;
		m_FixedPosition = position;
		Physics.SyncTransforms();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddForceInternal(Vector3 force)
	{
		m_ExternalForce += force;
	}

	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce(Vector3 force)
	{
		if (Time.timeScale >= 1f)
		{
			AddForceInternal(force);
		}
		else
		{
			AddSoftForce(force, 1f);
		}
	}

	public virtual void AddSoftForce(Vector3 force, float frames)
	{
		force /= Time.timeScale;
		frames = Mathf.Clamp(frames, 1f, 120f);
		AddForceInternal(force / frames);
		for (int i = 0; i < Mathf.RoundToInt(frames) - 1; i++)
		{
			m_SmoothForceFrame[i] += force / frames;
		}
	}

	public virtual void StopSoftForce()
	{
		for (int i = 0; i < 120 && !(m_SmoothForceFrame[i] == Vector3.zero); i++)
		{
			m_SmoothForceFrame[i] = Vector3.zero;
		}
	}

	public virtual void Stop()
	{
		Player.Move.Send(Vector3.zero);
		m_MotorThrottle = Vector3.zero;
		m_MotorJumpDone = true;
		m_MotorJumpForceAcc = 0f;
		m_ExternalForce = Vector3.zero;
		StopSoftForce();
		Player.InputMoveVector.Set(Vector2.zero);
		m_FallSpeed = 0f;
		m_SmoothPosition = base.Transform.position;
		m_MaxHeight = float.MinValue;
		m_MaxHeightInitialFallSpeed = 0f;
	}

	public virtual void DeflectDownForce()
	{
		if (GroundAngle > PhysicsSlopeSlideLimit)
		{
			m_SlopeSlideSpeed = m_FallImpact * (0.25f * Time.timeScale);
		}
		if (GroundAngle > 85f)
		{
			m_MotorThrottle += vp_3DUtility.HorizontalVector(GroundNormal * m_FallImpact);
			m_Grounded = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DeflectUpForce()
	{
		if (m_HeadContact)
		{
			m_NewDir = Vector3.Cross(Vector3.Cross(m_CeilingHit.normal, Vector3.up), m_CeilingHit.normal);
			m_ForceImpact = m_MotorThrottle.y + m_ExternalForce.y;
			Vector3 vector = m_NewDir * (m_MotorThrottle.y + m_ExternalForce.y) * (1f - PhysicsWallFriction);
			m_ForceImpact -= vector.magnitude;
			AddForce(vector * Time.timeScale);
			m_MotorThrottle.y = 0f;
			m_ExternalForce.y = 0f;
			m_FallSpeed = 0f;
			m_NewDir.x = base.Transform.InverseTransformDirection(m_NewDir).x;
			Player.HeadImpact.Send((m_NewDir.x < 0f || (m_NewDir.x == 0f && UnityEngine.Random.value < 0.5f)) ? (0f - m_ForceImpact) : m_ForceImpact);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DeflectHorizontalForce()
	{
		m_PredictedPos.y = base.Transform.position.y;
		m_PrevPos.y = base.Transform.position.y;
		m_PrevDir = (m_PredictedPos - m_PrevPos).normalized;
		CapsuleBottom = m_PrevPos + Vector3.up * Player.Radius.Get();
		CapsuleTop = CapsuleBottom + Vector3.up * (Player.Height.Get() - Player.Radius.Get() * 2f);
		if (!Physics.CapsuleCast(CapsuleBottom, CapsuleTop, Player.Radius.Get(), m_PrevDir, out m_WallHit, Vector3.Distance(m_PrevPos, m_PredictedPos) + 0.07f, 1084850176))
		{
			return;
		}
		m_NewDir = Vector3.Cross(m_WallHit.normal, Vector3.up).normalized;
		if (Vector3.Dot(Vector3.Cross(m_WallHit.point - base.Transform.position, m_PrevPos - base.Transform.position), Vector3.up) > 0f)
		{
			m_NewDir = -m_NewDir;
		}
		m_ForceMultiplier = Mathf.Abs(Vector3.Dot(m_PrevDir, m_NewDir)) * (1f - PhysicsWallFriction);
		if (PhysicsWallBounce > 0f)
		{
			m_NewDir = Vector3.Lerp(m_NewDir, Vector3.Reflect(m_PrevDir, m_WallHit.normal), PhysicsWallBounce);
			m_ForceMultiplier = Mathf.Lerp(m_ForceMultiplier, 1f, PhysicsWallBounce * (1f - PhysicsWallFriction));
		}
		if (m_ExternalForce != Vector3.zero)
		{
			m_ForceImpact = 0f;
			float y = m_ExternalForce.y;
			m_ExternalForce.y = 0f;
			m_ForceImpact = m_ExternalForce.magnitude;
			m_ExternalForce = m_NewDir * m_ExternalForce.magnitude * m_ForceMultiplier;
			m_ForceImpact -= m_ExternalForce.magnitude;
			for (int i = 0; i < 120 && !(m_SmoothForceFrame[i] == Vector3.zero); i++)
			{
				m_SmoothForceFrame[i] = m_SmoothForceFrame[i].magnitude * m_NewDir * m_ForceMultiplier;
			}
			m_ExternalForce.y = y;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RefreshColliders()
	{
		if (Player.Crouch.Active && (!MotorFreeFly || Grounded))
		{
			CharacterController.height = m_CrouchHeight;
			CharacterController.center = m_CrouchCenter;
		}
		else
		{
			CharacterController.height = m_NormalHeight;
			CharacterController.center = m_NormalCenter;
		}
		if (m_TriggerCollider != null)
		{
			m_TriggerCollider.radius = CharacterController.radius + m_SkinWidth;
			m_TriggerCollider.height = CharacterController.height + m_SkinWidth * 2f;
			m_TriggerCollider.center = CharacterController.center;
		}
	}

	public float CalculateMaxSpeed(string stateName = "Default", float accelDuration = 5f)
	{
		if (stateName != "Default")
		{
			bool flag = false;
			foreach (vp_State state in States)
			{
				if (state.Name == stateName)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Debug.LogError("Error (" + this?.ToString() + ") Controller has no such state: '" + stateName + "'.");
				return 0f;
			}
		}
		Dictionary<vp_State, bool> dictionary = new Dictionary<vp_State, bool>();
		foreach (vp_State state2 in States)
		{
			dictionary.Add(state2, state2.Enabled);
			state2.Enabled = false;
		}
		base.StateManager.Reset();
		if (stateName != "Default")
		{
			SetState(stateName);
		}
		float num = 0f;
		float num2 = 5f;
		for (int i = 0; (float)i < 60f * num2; i++)
		{
			num += MotorAcceleration * 0.1f * 60f;
			num /= 1f + MotorDamping;
		}
		foreach (vp_State state3 in States)
		{
			dictionary.TryGetValue(state3, out var value);
			state3.Enabled = value;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic && !(hit.moveDirection.y < -0.3f))
		{
			Vector3 vector = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
			attachedRigidbody.velocity = vector * (PhysicsPushForce / attachedRigidbody.mass);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Jump()
	{
		if (MotorFreeFly)
		{
			return true;
		}
		if (!m_Grounded)
		{
			return false;
		}
		if (!m_MotorJumpDone)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Swim()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Swim()
	{
		m_ExternalForce.y += m_FallSpeed * 0.2f;
		m_FallSpeed = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Run()
	{
		if (Player.Crouch.Active)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Jump()
	{
		m_SlopeSlideSpeed *= 0.2f;
		m_MotorJumpDone = false;
		if (!MotorFreeFly || Grounded)
		{
			m_MotorThrottle.y = MotorJumpForce / Time.timeScale;
			m_SmoothPosition.y = base.Transform.position.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Jump()
	{
		m_MotorJumpDone = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStop_Crouch()
	{
		if (Physics.SphereCast(new Ray(base.Transform.position, Vector3.up), Player.Radius.Get(), m_NormalHeight - Player.Radius.Get() + 0.01f, 1084850176))
		{
			Player.Crouch.NextAllowedStopTime = Time.time + 0.1f;
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Crouch()
	{
		Player.Run.Stop();
		RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Crouch()
	{
		RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_ForceImpact(Vector3 force)
	{
		AddForce(force);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Stop()
	{
		Stop();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Move(Vector3 direction)
	{
		if (CharacterController.enabled)
		{
			LastMoveCollisionFlags = CharacterController.Move(direction);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		Player.OutOfControl.Stop();
	}

	public void ScaleFallSpeed(float scale)
	{
		m_FallSpeed *= scale;
		m_MaxHeightInitialFallSpeed *= scale;
		float num = base.Transform.position.y + Origin.position.y;
		m_MaxHeight = num + scale * scale * (m_MaxHeight - num);
	}
}
