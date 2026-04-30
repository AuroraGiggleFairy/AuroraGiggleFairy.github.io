using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]
[Preserve]
public class vp_FPCamera : vp_Component
{
	public delegate void BobStepDelegate();

	public Vector3 DrivingPosition;

	public vp_FPController FPController;

	public float RenderingFieldOfView = 60f;

	public float RenderingZoomDamping = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FinalZoomTime;

	public float ZoomOffset;

	public Vector3 PositionOffset = new Vector3(0f, 1.75f, 0.1f);

	public Vector3 AimingPositionOffset = Vector3.zero;

	public float PositionGroundLimit = 0.1f;

	public float PositionSpringStiffness = 0.01f;

	public float PositionSpringDamping = 0.25f;

	public float PositionSpring2Stiffness = 0.95f;

	public float PositionSpring2Damping = 0.25f;

	public float PositionKneeling = 0.025f;

	public int PositionKneelingSoftness = 1;

	public float PositionEarthQuakeFactor = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_PositionSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_PositionSpring2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_DrawCameraCollisionDebugLine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 PositionOnDeath = Vector3.zero;

	public Vector2 RotationPitchLimit = new Vector2(90f, -90f);

	public Vector2 RotationYawLimit = new Vector2(-360f, 360f);

	public float RotationSpringStiffness = 0.01f;

	public float RotationSpringDamping = 0.25f;

	public float RotationKneeling = 0.025f;

	public int RotationKneelingSoftness = 1;

	public float RotationStrafeRoll = 0.01f;

	public float RotationEarthQuakeFactor;

	public Vector3 LookPoint = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_Pitch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_Yaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_RotationSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_LookPointHit;

	public Vector3 Position3rdPersonOffset = new Vector3(0.5f, 0.1f, 0.75f);

	public bool Locked3rdPerson;

	public float m_Current3rdPersonBlend;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Final3rdPersonCameraOffset = Vector3.zero;

	public float ShakeSpeed;

	public Vector3 ShakeAmplitude = new Vector3(10f, 10f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Shake = Vector3.zero;

	public float ShakeSpeed2;

	public Vector3 ShakeAmplitude2 = new Vector3(10f, 10f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Shake2 = Vector3.zero;

	public Vector4 BobRate = new Vector4(0f, 1.4f, 0f, 0.7f);

	public Vector4 BobAmplitude = new Vector4(0f, 0.25f, 0f, 0.5f);

	public float BobInputVelocityScale = 1f;

	public float BobMaxInputVelocity = 100f;

	public bool BobRequireGroundContact = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastBobSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector4 m_CurrentBobAmp = Vector4.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector4 m_CurrentBobVal = Vector4.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_BobSpeed;

	public BobStepDelegate BobStepCallback;

	public float BobStepThreshold = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastUpBob;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_BobWasElevating;

	public bool HasCollision = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CollisionVector = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CameraCollisionStartPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CameraCollisionEndPos = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public RaycastHit m_CameraHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> cameraCollisionEndPosList = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody m_FirstRigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerRotationSpeed = 16f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCameraOffsetSpeed = 10f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 current3rdPersonOffset = Vector3.zero;

	public float yaw3P;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float pitch3P;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetRotation3P;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _cameraRelative3PMovement;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 shake3P;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Camera thisCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_IsFirstPerson = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasLateUpdateRan;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 lastPos;

	public bool DrawCameraCollisionDebugLine
	{
		get
		{
			return m_DrawCameraCollisionDebugLine;
		}
		set
		{
			m_DrawCameraCollisionDebugLine = value;
		}
	}

	public Vector3 CollisionVector => m_CollisionVector;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasOverheadSpace
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

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

	public Rigidbody FirstRigidBody
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_FirstRigidbody == null)
			{
				m_FirstRigidbody = base.Transform.root.GetComponentInChildren<Rigidbody>();
			}
			return m_FirstRigidbody;
		}
	}

	public Vector2 Angle
	{
		get
		{
			return new Vector2(m_Pitch, m_Yaw);
		}
		set
		{
			Pitch = value.x;
			Yaw = value.y;
		}
	}

	public Vector3 Forward => m_Transform.forward;

	public float Pitch
	{
		get
		{
			return m_Pitch;
		}
		set
		{
			if (value > 90f)
			{
				value -= 360f;
			}
			m_Pitch = value;
		}
	}

	public float Yaw
	{
		get
		{
			return m_Yaw;
		}
		set
		{
			m_Yaw = value;
		}
	}

	public virtual bool OnValue_CameraRelativeMovement3P
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return _cameraRelative3PMovement;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			_cameraRelative3PMovement = value;
		}
	}

	public float CurrentCameraDistance => Mathf.Min(current3rdPersonOffset.z, CameraCollisionDistance);

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float CameraCollisionDistance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual bool OnValue_IsFirstPerson
	{
		get
		{
			return m_IsFirstPerson;
		}
		set
		{
			if (m_IsFirstPerson == value)
			{
				return;
			}
			m_IsFirstPerson = value;
			if (m_IsFirstPerson)
			{
				m_Yaw = yaw3P - 180f;
				Quaternion quaternion = Quaternion.AngleAxis(m_Yaw, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(0f, Vector3.left);
				base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion * quaternion2, base.Parent.rotation);
				SnapSprings();
			}
			if (!m_IsFirstPerson)
			{
				current3rdPersonOffset = Vector3.Lerp(new Vector3(0f, Position3rdPersonOffset.y, 0f), Position3rdPersonOffset, 0.5f);
				pitch3P = (0f - base.transform.forward.y) * 57.29578f;
				if (Player.Driving.Active)
				{
					yaw3P = Mathf.Atan2(0f - base.transform.forward.x, 0f - base.transform.forward.z) * 57.29578f;
				}
				else
				{
					yaw3P = Mathf.Atan2(0f - base.Parent.transform.forward.x, 0f - base.Parent.transform.forward.z) * 57.29578f;
					targetRotation3P = base.transform.eulerAngles.y;
					Player.Rotation.Set(new Vector2(Pitch, targetRotation3P));
				}
				SnapSprings();
				shake3P = Vector2.zero;
			}
		}
	}

	public virtual Transform OnValue_CameraTransform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return base.transform;
		}
	}

	public virtual Vector3 OnValue_LookPoint => LookPoint;

	public virtual Vector3 OnValue_CameraLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (Player.LookPoint.Get() - base.Transform.position).normalized;
		}
	}

	public virtual Vector2 OnValue_Rotation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Angle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			Angle = value;
		}
	}

	public virtual bool OnValue_IsLocal
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		FPController = base.Transform.parent.GetComponent<vp_FPController>();
		SetRotation(new Vector2(base.Transform.eulerAngles.x, base.Transform.eulerAngles.y));
		thisCamera = GetComponent<Camera>();
		thisCamera.fieldOfView = Constants.cDefaultCameraFieldOfView;
		m_PositionSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Position, autoUpdate: false);
		m_PositionSpring.MinVelocity = 1E-05f;
		m_PositionSpring.RestState = PositionOffset + AimingPositionOffset;
		m_PositionSpring2 = new vp_Spring(base.Transform, vp_Spring.UpdateMode.PositionAdditiveLocal, autoUpdate: false);
		m_PositionSpring2.MinVelocity = 1E-05f;
		m_RotationSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.RotationAdditiveLocal, autoUpdate: false);
		m_RotationSpring.MinVelocity = 1E-05f;
		m_RotationSpring.SdtdStopping = true;
		HasOverheadSpace = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		vp_TargetEvent<float>.Register(base.Parent, "CameraBombShake", OnMessage_CameraBombShake);
		vp_TargetEvent<float>.Register(base.Parent, "CameraGroundStomp", OnMessage_CameraGroundStomp);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		vp_TargetEvent<float>.Unregister(base.Parent, "CameraBombShake", OnMessage_CameraBombShake);
		vp_TargetEvent<float>.Unregister(base.Parent, "CameraGroundStomp", OnMessage_CameraGroundStomp);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		Refresh();
		SnapSprings();
		SnapZoom();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (Time.timeScale == 0f)
		{
			return;
		}
		UpdateInput();
		if (Player.IsFirstPerson.Get())
		{
			return;
		}
		if (!Player.Driving.Active)
		{
			new Vector2(m_Pitch, base.Parent.transform.eulerAngles.y);
			float y = targetRotation3P;
			if (Player.CameraRelativeMovement3P.Get() || Vector3.Dot(base.Parent.transform.forward, new Vector3(base.transform.forward.x, 0f, base.transform.forward.z)) < 0.95f)
			{
				y = Mathf.LerpAngle(base.Parent.transform.eulerAngles.y, targetRotation3P, Time.deltaTime * 16f);
			}
			Vector2 o = new Vector2(m_Pitch, y);
			Player.Rotation.Set(o);
			base.Parent.rotation = Quaternion.Euler(new Vector2(0f, o.y));
		}
		current3rdPersonOffset = Vector3.Lerp(current3rdPersonOffset, Position3rdPersonOffset, Time.deltaTime * 10f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (!Player.IsFirstPerson.Get())
		{
			if (hasLateUpdateRan)
			{
				hasLateUpdateRan = false;
				if (Time.timeScale > 0f && !Locked3rdPerson)
				{
					UpdateShakes();
					UpdateSprings();
				}
			}
		}
		else if (hasLateUpdateRan)
		{
			hasLateUpdateRan = false;
			base.FixedUpdate();
			if (Time.timeScale != 0f && !Locked3rdPerson)
			{
				UpdateZoom();
				UpdateSwaying();
				UpdateBob();
				UpdateEarthQuake();
				UpdateShakes();
				UpdateSprings();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		if (!Player.IsFirstPerson.Get())
		{
			hasLateUpdateRan = true;
			Update3rdPerson();
			return;
		}
		hasLateUpdateRan = true;
		base.LateUpdate();
		bool active = Player.Driving.Active;
		if (Locked3rdPerson)
		{
			if (!active)
			{
				Quaternion quaternion = Quaternion.AngleAxis(m_Yaw, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(0f, Vector3.left);
				base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion * quaternion2, base.Parent.rotation);
			}
			return;
		}
		if (FPController.enabled)
		{
			if (Player.Driving.Active)
			{
				m_Transform.position = DrivingPosition;
			}
			else if (Player.IsFirstPerson.Get())
			{
				m_Transform.position = FPController.SmoothPosition;
			}
			else
			{
				m_Transform.position = FPController.transform.position;
			}
			if (Player.IsFirstPerson.Get())
			{
				m_Transform.localPosition += m_PositionSpring.State + m_PositionSpring2.State;
			}
			else if (!Player.Driving.Active)
			{
				m_Transform.localPosition += m_PositionSpring.State + Vector3.Scale(m_PositionSpring2.State, Vector3.up);
			}
			if (HasCollision)
			{
				DoCameraCollision();
			}
		}
		Quaternion quaternion3 = Quaternion.AngleAxis(m_Yaw, Vector3.up);
		if (Player.Driving.Active)
		{
			Quaternion quaternion4 = Quaternion.AngleAxis(0f - m_Pitch, Vector3.left);
			base.Transform.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion4, base.Transform.rotation);
		}
		else
		{
			Quaternion quaternion5 = Quaternion.AngleAxis(0f, Vector3.left);
			base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion5, base.Parent.rotation);
			quaternion5 = Quaternion.AngleAxis(0f - m_Pitch, Vector3.left);
			base.Transform.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion5, base.Transform.rotation);
		}
		base.Transform.localEulerAngles += vp_MathUtility.NaNSafeVector3(Vector3.forward * m_RotationSpring.State.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update3rdPerson()
	{
		if (Position3rdPersonOffset == Vector3.zero)
		{
			return;
		}
		if (PositionOnDeath != Vector3.zero)
		{
			base.Transform.position = PositionOnDeath;
			if (FirstRigidBody != null)
			{
				base.Transform.LookAt(FirstRigidBody.transform.position + Vector3.up);
			}
			else
			{
				base.Transform.LookAt(base.Root.position + Vector3.up);
			}
			return;
		}
		bool active = Player.Driving.Active;
		bool flag = Player.CameraRelativeMovement3P.Get();
		pitch3P = Mathf.Clamp(pitch3P + Player.InputSmoothLook.Get().y, -89.99f, 89.99f);
		m_Pitch = pitch3P;
		yaw3P += Player.InputSmoothLook.Get().x;
		if (Locked3rdPerson)
		{
			targetRotation3P += Player.InputSmoothLook.Get().x;
			yaw3P = targetRotation3P;
			return;
		}
		if (flag)
		{
			Vector2 vector = Player.InputMoveVector.Get();
			if (vector != Vector2.zero)
			{
				targetRotation3P = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
			}
		}
		else
		{
			targetRotation3P = base.transform.eulerAngles.y;
		}
		m_Final3rdPersonCameraOffset = base.Transform.position;
		pitch3P += shake3P.y;
		yaw3P += shake3P.x;
		shake3P = Vector2.zero;
		Vector3 normalized = (Quaternion.Euler(0f - pitch3P, yaw3P, 0f) * Vector3.forward).normalized;
		if (active)
		{
			FPController.localPlayer.RefreshDrivingCameraPositions();
		}
		Vector3 obj = (active ? DrivingPosition : FPController.SmoothPosition);
		Vector3 position = obj + normalized * current3rdPersonOffset.z;
		position += Vector3.up * current3rdPersonOffset.y;
		base.Transform.position = position;
		m_Final3rdPersonCameraOffset -= base.Transform.position;
		Vector3 worldPosition = obj + Vector3.up * current3rdPersonOffset.y;
		base.Transform.LookAt(worldPosition);
		base.Transform.position += base.Transform.TransformDirection(new Vector3(current3rdPersonOffset.x, 0f, 0f));
		float num = Pitch / 90f;
		float v = current3rdPersonOffset.z * num * 0.6f;
		v = Utils.FastMin(v, 0.7f);
		base.Transform.position += new Vector3(base.Transform.forward.x, 0f, base.Transform.forward.z).normalized * v;
		DoCameraCollision();
		LookPoint = GetLookPoint();
	}

	public void AlignCharacterToCamera()
	{
		targetRotation3P = base.Transform.eulerAngles.y;
	}

	public virtual void DoCameraCollision()
	{
		HasOverheadSpace = true;
		CameraCollisionDistance = float.MaxValue;
		m_CameraCollisionStartPos = FPController.Transform.TransformPoint(0f, PositionOffset.y + AimingPositionOffset.y, 0f) - (m_Player.IsFirstPerson.Get() ? Vector3.zero : (FPController.Transform.position - FPController.SmoothPosition));
		if (!m_Player.IsFirstPerson.Get())
		{
			m_CameraCollisionEndPos = base.Transform.position + (base.Transform.position - m_CameraCollisionStartPos).normalized * FPController.CharacterController.radius;
			m_CollisionVector = Vector3.zero;
			if (Physics.Linecast(m_CameraCollisionStartPos, m_CameraCollisionEndPos, out m_CameraHit, 1082195968) && !m_CameraHit.collider.isTrigger)
			{
				base.Transform.position = m_CameraHit.point - (m_CameraHit.point - m_CameraCollisionStartPos).normalized * FPController.CharacterController.radius;
				m_CollisionVector = m_CameraHit.point - m_CameraCollisionEndPos;
				CameraCollisionDistance = m_CameraHit.distance;
				return;
			}
			Camera playerCamera = GameManager.Instance.World.GetPrimaryPlayer().playerCamera;
			Vector3[] array = new Vector3[4];
			playerCamera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), playerCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, array);
			cameraCollisionEndPosList.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 direction = array[i] * 1.5f;
				m_CameraCollisionEndPos = base.Transform.position + base.Transform.TransformDirection(direction);
				m_CollisionVector = Vector3.zero;
				cameraCollisionEndPosList.Add(m_CameraCollisionEndPos);
				if (Physics.Linecast(m_CameraCollisionStartPos, m_CameraCollisionEndPos, out m_CameraHit, 1082195968) && !m_CameraHit.collider.isTrigger)
				{
					base.Transform.position = m_CameraHit.point - (m_CameraHit.point - m_CameraCollisionStartPos).normalized * FPController.CharacterController.radius;
					m_CollisionVector = m_CameraHit.point - m_CameraCollisionEndPos;
					CameraCollisionDistance = m_CameraHit.distance;
					break;
				}
			}
		}
		else
		{
			Vector3 vector = m_CameraCollisionStartPos - Vector3.up * (FPController.CharacterController.height * 0.5f + 0.05f);
			m_CameraCollisionEndPos = vector + Vector3.up * FPController.CharacterController.radius * 2.1f;
			if (Physics.SphereCast(vector, FPController.CharacterController.radius, Vector3.up, out m_CameraHit, FPController.CharacterController.radius * 2.1f, 1082195968) && !m_CameraHit.collider.isTrigger)
			{
				m_CollisionVector = m_CameraCollisionEndPos - m_CameraHit.point;
				base.Transform.position = vector + Vector3.up * FPController.CharacterController.radius + Vector3.down * m_CollisionVector.y;
				CameraCollisionDistance = m_CameraHit.distance;
				HasOverheadSpace = false;
			}
		}
	}

	public virtual void AddForce(Vector3 force)
	{
		m_PositionSpring.AddForce(force);
	}

	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce2(Vector3 force)
	{
		m_PositionSpring2.AddForce(force);
	}

	public void AddForce2(float x, float y, float z)
	{
		AddForce2(new Vector3(x, y, z));
	}

	public virtual void AddRollForce(float force)
	{
		m_RotationSpring.AddForce(Vector3.forward * force);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateInput()
	{
		if (!Player.Dead.Active && !(Player.InputSmoothLook.Get() == Vector2.zero))
		{
			m_Yaw += Player.InputSmoothLook.Get().x;
			m_Pitch += Player.InputSmoothLook.Get().y;
			m_Yaw = ((m_Yaw < -360f) ? (m_Yaw += 360f) : m_Yaw);
			m_Yaw = ((m_Yaw > 360f) ? (m_Yaw -= 360f) : m_Yaw);
			m_Yaw = Mathf.Clamp(m_Yaw, RotationYawLimit.x, RotationYawLimit.y);
			m_Pitch = ((m_Pitch < -360f) ? (m_Pitch += 360f) : m_Pitch);
			m_Pitch = ((m_Pitch > 360f) ? (m_Pitch -= 360f) : m_Pitch);
			m_Pitch = Mathf.Clamp(m_Pitch, 0f - RotationPitchLimit.x, 0f - RotationPitchLimit.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateZoom()
	{
		if (!(m_FinalZoomTime <= Time.time))
		{
			RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
			float t = 1f - (m_FinalZoomTime - Time.time) / RenderingZoomDamping;
			base.gameObject.GetComponent<Camera>().fieldOfView = Mathf.SmoothStep(base.gameObject.GetComponent<Camera>().fieldOfView, (float)Constants.cDefaultCameraFieldOfView + ZoomOffset, t);
		}
	}

	public void RefreshZoom()
	{
		float t = 1f - (m_FinalZoomTime - Time.time) / RenderingZoomDamping;
		base.gameObject.GetComponent<Camera>().fieldOfView = Mathf.SmoothStep(base.gameObject.GetComponent<Camera>().fieldOfView, (float)Constants.cDefaultCameraFieldOfView + ZoomOffset, t);
	}

	public virtual void Zoom()
	{
		m_FinalZoomTime = Time.time + RenderingZoomDamping;
	}

	public virtual void SnapZoom()
	{
		base.gameObject.GetComponent<Camera>().fieldOfView = (float)Constants.cDefaultCameraFieldOfView + ZoomOffset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateShakes()
	{
		if (ShakeSpeed != 0f)
		{
			m_Yaw -= m_Shake.y;
			m_Pitch -= m_Shake.x;
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
			if (float.IsNaN(m_Shake.x) || float.IsNaN(m_Shake.y) || float.IsNaN(m_Shake.z))
			{
				Log.Warning("Shake NaN {0}, time {1}, speed {2}, amp {3}", m_Shake, Time.time, ShakeSpeed, ShakeAmplitude);
				ShakeSpeed = 0f;
				m_Shake = Vector3.zero;
				m_Pitch += -1f;
			}
			m_Yaw += m_Shake.y;
			m_Pitch += m_Shake.x;
			m_RotationSpring.AddForce(Vector3.forward * m_Shake.z * Time.timeScale);
		}
		if (ShakeSpeed2 != 0f)
		{
			m_Yaw -= m_Shake2.y;
			m_Pitch -= m_Shake2.x;
			shake3P.x -= m_Shake2.x;
			shake3P.y -= m_Shake2.y;
			m_Shake2 = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed2), ShakeAmplitude2);
			if (float.IsNaN(m_Shake2.x) || float.IsNaN(m_Shake2.y) || float.IsNaN(m_Shake2.z))
			{
				Log.Warning("Shake2 NaN {0}, time {1}, speed {2}, amp {3}", m_Shake2, Time.time, ShakeSpeed2, ShakeAmplitude2);
				ShakeSpeed2 = 0f;
				m_Shake2 = Vector3.zero;
				m_Pitch += -1f;
			}
			m_Yaw += m_Shake2.y;
			m_Pitch += m_Shake2.x;
			m_RotationSpring.AddForce(Vector3.forward * m_Shake2.z * Time.timeScale);
			shake3P.x += m_Shake2.x;
			shake3P.y += m_Shake2.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBob()
	{
		if (!(BobAmplitude == Vector4.zero) && !(BobRate == Vector4.zero) && Player.IsFirstPerson.Get())
		{
			m_BobSpeed = ((BobRequireGroundContact && !FPController.Grounded) ? 0f : FPController.CharacterController.velocity.sqrMagnitude);
			m_BobSpeed = Mathf.Min(m_BobSpeed * BobInputVelocityScale, BobMaxInputVelocity);
			m_BobSpeed = Mathf.Round(m_BobSpeed * 1000f) / 1000f;
			if (m_BobSpeed == 0f)
			{
				m_BobSpeed = Mathf.Min(m_LastBobSpeed * 0.93f, BobMaxInputVelocity);
			}
			m_CurrentBobAmp.y = m_BobSpeed * (BobAmplitude.y * -0.0001f);
			m_CurrentBobVal.y = Mathf.Cos(Time.time * (BobRate.y * 10f)) * m_CurrentBobAmp.y;
			m_CurrentBobAmp.x = m_BobSpeed * (BobAmplitude.x * 0.0001f);
			m_CurrentBobVal.x = Mathf.Cos(Time.time * (BobRate.x * 10f)) * m_CurrentBobAmp.x;
			m_CurrentBobAmp.z = m_BobSpeed * (BobAmplitude.z * 0.0001f);
			m_CurrentBobVal.z = Mathf.Cos(Time.time * (BobRate.z * 10f)) * m_CurrentBobAmp.z;
			m_CurrentBobAmp.w = m_BobSpeed * (BobAmplitude.w * 0.0001f);
			m_CurrentBobVal.w = Mathf.Cos(Time.time * (BobRate.w * 10f)) * m_CurrentBobAmp.w;
			m_PositionSpring.AddForce((Vector3)m_CurrentBobVal * Time.timeScale);
			AddRollForce(m_CurrentBobVal.w * Time.timeScale);
			m_LastBobSpeed = m_BobSpeed;
			DetectBobStep(m_BobSpeed, m_CurrentBobVal.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DetectBobStep(float speed, float upBob)
	{
		if (BobStepCallback != null && !(speed < BobStepThreshold))
		{
			bool flag = ((m_LastUpBob < upBob) ? true : false);
			m_LastUpBob = upBob;
			if (flag && !m_BobWasElevating)
			{
				BobStepCallback();
			}
			m_BobWasElevating = flag;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSwaying()
	{
		AddRollForce((base.Transform.InverseTransformDirection(FPController.CharacterController.velocity * 0.016f) * Time.timeScale).x * RotationStrafeRoll);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateEarthQuake()
	{
		if (!(Player == null) && Player.CameraEarthQuake.Active)
		{
			if (m_PositionSpring.State.y >= m_PositionSpring.RestState.y)
			{
				Vector3 o = Player.CameraEarthQuakeForce.Get();
				o.y = 0f - o.y;
				Player.CameraEarthQuakeForce.Set(o);
			}
			m_PositionSpring.AddForce(Player.CameraEarthQuakeForce.Get() * PositionEarthQuakeFactor);
			m_RotationSpring.AddForce(Vector3.forward * ((0f - Player.CameraEarthQuakeForce.Get().x) * 2f) * RotationEarthQuakeFactor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSprings()
	{
		Vector3 position = base.Transform.position;
		m_PositionSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring.FixedUpdate();
		base.Transform.position = position;
	}

	public virtual void DoBomb(Vector3 positionForce, float minRollForce, float maxRollForce)
	{
		AddForce2(positionForce);
		float num = UnityEngine.Random.Range(minRollForce, maxRollForce);
		if (UnityEngine.Random.value > 0.5f)
		{
			num = 0f - num;
		}
		AddRollForce(num);
	}

	public override void Refresh()
	{
		if (Application.isPlaying)
		{
			if (m_PositionSpring != null)
			{
				m_PositionSpring.Stiffness = new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
				m_PositionSpring.Damping = Vector3.one - new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);
				m_PositionSpring.MinState.y = PositionGroundLimit;
				m_PositionSpring.RestState = PositionOffset + AimingPositionOffset;
			}
			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.Stiffness = new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
				m_PositionSpring2.Damping = Vector3.one - new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
				m_PositionSpring2.MinState.y = 0f - PositionOffset.y - AimingPositionOffset.y + PositionGroundLimit;
			}
			if (m_RotationSpring != null)
			{
				m_RotationSpring.Stiffness = new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
				m_RotationSpring.Damping = Vector3.one - new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);
			}
		}
	}

	public virtual void SnapSprings()
	{
		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset + AimingPositionOffset;
			m_PositionSpring.State = PositionOffset + AimingPositionOffset;
			m_PositionSpring.Stop(includeSoftForce: true);
		}
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = Vector3.zero;
			m_RotationSpring.State = Vector3.zero;
			m_RotationSpring.Stop(includeSoftForce: true);
		}
	}

	public virtual void StopSprings()
	{
		if (m_PositionSpring != null)
		{
			m_PositionSpring.Stop(includeSoftForce: true);
		}
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring != null)
		{
			m_RotationSpring.Stop(includeSoftForce: true);
		}
		m_BobSpeed = 0f;
		m_LastBobSpeed = 0f;
	}

	public virtual void Stop()
	{
		SnapSprings();
		SnapZoom();
		Refresh();
	}

	public virtual void SetRotation(Vector2 eulerAngles, bool stopZoomAndSprings)
	{
		Angle = eulerAngles;
		if (stopZoomAndSprings)
		{
			Stop();
		}
	}

	public virtual void SetRotation(Vector2 eulerAngles)
	{
		Angle = eulerAngles;
		Stop();
	}

	public virtual void SetRotation(Vector2 eulerAngles, bool stopZoomAndSprings, bool obsolete)
	{
		SetRotation(eulerAngles, stopZoomAndSprings);
	}

	public Vector3 GetLookPoint()
	{
		if (!Player.IsFirstPerson.Get() && Physics.Linecast(base.Transform.position, base.Transform.position + base.Transform.forward * 1000f, out m_LookPointHit, 1082195968) && !m_LookPointHit.collider.isTrigger && base.Root.InverseTransformPoint(m_LookPointHit.point).z > 0f)
		{
			return m_LookPointHit.point;
		}
		return base.Transform.position + base.Transform.forward * 1000f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		impact = Mathf.Abs(impact * 55f);
		float t = impact * PositionKneeling;
		float t2 = impact * RotationKneeling;
		t = Mathf.SmoothStep(0f, 1f, t);
		t2 = Mathf.SmoothStep(0f, 1f, t2);
		t2 = Mathf.SmoothStep(0f, 1f, t2);
		if (m_PositionSpring != null)
		{
			m_PositionSpring.AddSoftForce(Vector3.down * t, PositionKneelingSoftness);
		}
		if (m_RotationSpring != null)
		{
			float num = ((UnityEngine.Random.value > 0.5f) ? (t2 * 2f) : (0f - t2 * 2f));
			m_RotationSpring.AddSoftForce(Vector3.forward * num, RotationKneelingSoftness);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HeadImpact(float impact)
	{
		if (m_RotationSpring != null && Mathf.Abs(m_RotationSpring.State.z) < 30f)
		{
			m_RotationSpring.AddForce(Vector3.forward * (impact * 20f) * Time.timeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraGroundStomp(float impact)
	{
		AddForce2(new Vector3(0f, -1f, 0f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		DoBomb(new Vector3(1f, -10f, 1f) * impact, 1f, 2f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Zoom()
	{
		if (!(Player == null))
		{
			Player.Run.Stop();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Run()
	{
		if (Player == null)
		{
			return true;
		}
		if (Player.Zoom.Active)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Stop()
	{
		Stop();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (!Player.IsFirstPerson.Get())
		{
			PositionOnDeath = base.Transform.position - m_Final3rdPersonCameraOffset;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		PositionOnDeath = Vector3.zero;
		m_Current3rdPersonBlend = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		m_Player.IsFirstPerson.Set(!m_Player.IsFirstPerson.Get());
	}
}
