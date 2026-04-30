using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_FPWeapon : vp_Weapon
{
	public GameObject WeaponPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CharacterController Controller;

	public float RenderingZoomDamping = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FinalZoomTime;

	public float RenderingFieldOfView = 75f;

	public float originalRenderingFieldOfView;

	public Vector2 RenderingClippingPlanes = new Vector2(0.01f, 10f);

	public float RenderingZScale = 1f;

	public float PositionSpringStiffness = 0.01f;

	public float PositionSpringDamping = 0.25f;

	public float PositionFallRetract = 1f;

	public float PositionPivotSpringStiffness = 0.01f;

	public float PositionPivotSpringDamping = 0.25f;

	public float PositionKneeling = 0.06f;

	public int PositionKneelingSoftness = 1;

	public Vector3 PositionWalkSlide = new Vector3(0.5f, 0.75f, 0.5f);

	public Vector3 PositionPivot = Vector3.zero;

	public Vector3 RotationPivot = Vector3.zero;

	public float PositionInputVelocityScale = 1f;

	public float PositionMaxInputVelocity = 25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_PositionSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_PositionPivotSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_RotationPivotSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CameraMatrixOverride m_CameraMatrixOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_WeaponGroup;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_Pivot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_WeaponGroupTransform;

	public float RotationSpringStiffness = 0.01f;

	public float RotationSpringDamping = 0.25f;

	public float RotationPivotSpringStiffness = 0.01f;

	public float RotationPivotSpringDamping = 0.25f;

	public float RotationKneeling;

	public int RotationKneelingSoftness = 1;

	public Vector3 RotationLookSway = new Vector3(1f, 0.7f, 0f);

	public Vector3 RotationStrafeSway = new Vector3(0.3f, 1f, 1.5f);

	public Vector3 RotationFallSway = new Vector3(1f, -0.5f, -3f);

	public float RotationSlopeSway = 0.5f;

	public float RotationInputVelocityScale = 1f;

	public float RotationMaxInputVelocity = 15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_RotationSpring;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_SwayVel = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_FallSway = Vector3.zero;

	public float RetractionDistance;

	public Vector2 RetractionOffset = new Vector2(0f, 0f);

	public float RetractionRelaxSpeed = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_DrawRetractionDebugLine;

	public float ShakeSpeed = 0.05f;

	public Vector3 ShakeAmplitude = new Vector3(0.25f, 0f, 2f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Shake = Vector3.zero;

	public Vector4 BobRate = new Vector4(0.9f, 0.45f, 0f, 0f);

	public Vector4 BobAmplitude = new Vector4(0.35f, 0.5f, 0f, 0f);

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

	public Vector3 StepPositionForce = new Vector3(0f, -0.0012f, -0.0012f);

	public Vector3 StepRotationForce = new Vector3(0f, 0f, 0f);

	public int StepSoftness = 4;

	public float StepMinVelocity;

	public float StepPositionBalance;

	public float StepRotationBalance;

	public float StepForceScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastUpBob;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_BobWasElevating;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_PosStep = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_RotStep = Vector3.zero;

	public bool LookDownActive;

	public float LookDownYawLimit = 60f;

	public Vector3 LookDownPositionOffsetMiddle = new Vector3(0.32f, -0.37f, 0.78f);

	public Vector3 LookDownPositionOffsetLeft = new Vector3(0.27f, -0.31f, 0.7f);

	public Vector3 LookDownPositionOffsetRight = new Vector3(0.6f, -0.41f, 0.86f);

	public float LookDownPositionSpringPower = 1f;

	public Vector3 LookDownRotationOffsetMiddle = new Vector3(-3.9f, 2.24f, 4.69f);

	public Vector3 LookDownRotationOffsetLeft = new Vector3(-7f, -10.5f, 15.6f);

	public Vector3 LookDownRotationOffsetRight = new Vector3(-9.2f, -9.8f, 48.84f);

	public float LookDownRotationSpringPower = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentPosRestState = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CurrentRotRestState = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve m_LookDownCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.8f, 0.2f, 0.9f, 1.5f), new Keyframe(1f, 1f));

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LookDownPitch;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LookDownYaw;

	public AudioClip SoundWield;

	public AudioClip SoundUnWield;

	public AnimationClip AnimationWield;

	public AnimationClip AnimationUnWield;

	public List<UnityEngine.Object> AnimationAmbient = new List<UnityEngine.Object>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<bool> m_AmbAnimPlayed = new List<bool>();

	public Vector2 AmbientInterval = new Vector2(2.5f, 7.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_CurrentAmbientAnimation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_AnimationAmbientTimer = new vp_Timer.Handle();

	public Vector3 PositionExitOffset = new Vector3(0f, -1f, 0f);

	public Vector3 RotationExitOffset = new Vector3(40f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_LookInput = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float LOOKDOWNSPEED = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_FPPlayer;

	public CameraMatrixOverride CameraMatrixOverride => m_CameraMatrixOverride;

	public GameObject WeaponModel => m_WeaponModel;

	public Vector3 DefaultPosition => (Vector3)base.DefaultState.Preset.GetFieldValue("PositionOffset");

	public Vector3 DefaultRotation => (Vector3)base.DefaultState.Preset.GetFieldValue("RotationOffset");

	public bool DrawRetractionDebugLine
	{
		get
		{
			return m_DrawRetractionDebugLine;
		}
		set
		{
			m_DrawRetractionDebugLine = value;
		}
	}

	public vp_FPPlayerEventHandler FPPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_FPPlayer == null && base.EventHandler != null)
			{
				m_FPPlayer = base.EventHandler as vp_FPPlayerEventHandler;
			}
			return m_FPPlayer;
		}
	}

	public override Vector3 OnValue_AimDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (FPPlayer.IsFirstPerson.Get())
			{
				return FPPlayer.HeadLookDirection.Get();
			}
			if (Weapon3rdPersonModel == null)
			{
				return FPPlayer.HeadLookDirection.Get();
			}
			return (Weapon3rdPersonModel.transform.position - FPPlayer.LookPoint.Get()).normalized;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (base.transform.parent == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Must not be placed in scene root. Disabling self.");
			vp_Utility.Activate(base.gameObject, activate: false);
			return;
		}
		Controller = base.Transform.gameObject.GetComponentInParent<CharacterController>();
		if (Controller == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Could not find CharacterController. Disabling self.");
			vp_Utility.Activate(base.gameObject, activate: false);
			return;
		}
		base.Transform.eulerAngles = Vector3.zero;
		Transform parent = base.transform;
		while (parent != null && !parent.TryGetComponent<CameraMatrixOverride>(out m_CameraMatrixOverride))
		{
			m_CameraMatrixOverride = null;
			parent = parent.parent;
		}
		if (m_CameraMatrixOverride == null)
		{
			Debug.LogWarning("vp_FPWeapon could not find CameraMatrixOverride in hierarchy, some functionality will be unavailable");
		}
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		InstantiateWeaponModel();
		base.Start();
		originalRenderingFieldOfView = RenderingFieldOfView;
		Transform transform = ((base.Transform.parent != null && base.Transform.parent.gameObject.name == "UFPSRoot") ? base.Transform.parent : base.Transform.FindParent("UFPSRoot"));
		if (transform == null)
		{
			transform = new GameObject("UFPSRoot").transform;
			transform.parent = base.Transform.parent;
		}
		m_WeaponGroup = transform?.gameObject;
		m_WeaponGroupTransform = m_WeaponGroup.transform;
		m_WeaponGroupTransform.localPosition = PositionOffset;
		vp_Layer.Set(m_WeaponGroup, 10);
		base.Transform.parent = m_WeaponGroupTransform;
		base.Transform.localPosition = Vector3.zero;
		m_WeaponGroupTransform.localEulerAngles = RotationOffset;
		m_Pivot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		m_Pivot.name = "Pivot";
		m_Pivot.GetComponent<Collider>().enabled = false;
		m_Pivot.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		m_Pivot.transform.parent = m_WeaponGroupTransform;
		m_Pivot.transform.localPosition = Vector3.zero;
		m_Pivot.layer = 10;
		vp_Utility.Activate(m_Pivot.gameObject, activate: false);
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.color = new Color(0f, 0f, 1f, 0.5f);
		m_Pivot.GetComponent<Renderer>().material = material;
		m_PositionSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Position);
		m_PositionSpring.RestState = PositionOffset;
		m_PositionPivotSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Position);
		m_PositionPivotSpring.RestState = PositionPivot;
		m_PositionSpring2 = new vp_Spring(base.Transform, vp_Spring.UpdateMode.PositionAdditiveLocal);
		m_PositionSpring2.MinVelocity = 1E-05f;
		m_RotationSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Rotation);
		m_RotationSpring.RestState = RotationOffset;
		m_RotationPivotSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Rotation);
		m_RotationPivotSpring.RestState = RotationPivot;
		m_RotationSpring2 = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.RotationAdditiveLocal);
		m_RotationSpring2.MinVelocity = 1E-05f;
		SnapSprings();
		Refresh();
	}

	public virtual void InstantiateWeaponModel()
	{
		if (WeaponPrefab != null)
		{
			if (m_WeaponModel != null && m_WeaponModel != base.gameObject)
			{
				UnityEngine.Object.Destroy(m_WeaponModel);
			}
			m_WeaponModel = UnityEngine.Object.Instantiate(WeaponPrefab);
			m_WeaponModel.transform.parent = base.transform;
			m_WeaponModel.transform.localPosition = Vector3.zero;
			m_WeaponModel.transform.localScale = new Vector3(1f, 1f, RenderingZScale);
			m_WeaponModel.transform.localEulerAngles = Vector3.zero;
		}
		else
		{
			m_WeaponModel = base.gameObject;
		}
		CacheRenderers();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
		ScheduleAmbientAnimation();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (Time.timeScale != 0f)
		{
			UpdateInput();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateZoom();
			UpdateSwaying();
			UpdateBob();
			UpdateEarthQuake();
			UpdateStep();
			UpdateShakes();
			UpdateRetraction();
			UpdateSprings();
			UpdateLookDown();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
	}

	public virtual void AddForce(Vector3 force)
	{
		m_PositionSpring.AddForce(force);
	}

	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce(Vector3 positional, Vector3 angular)
	{
		m_PositionSpring.AddForce(positional);
		m_RotationSpring.AddForce(angular);
	}

	public virtual void AddForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}

	public virtual void AddSoftForce(Vector3 force, int frames)
	{
		m_PositionSpring.AddSoftForce(force, frames);
	}

	public virtual void AddSoftForce(float x, float y, float z, int frames)
	{
		AddSoftForce(new Vector3(x, y, z), frames);
	}

	public virtual void AddSoftForce(Vector3 positional, Vector3 angular, int frames)
	{
		m_PositionSpring.AddSoftForce(positional, frames);
		m_RotationSpring.AddSoftForce(angular, frames);
	}

	public virtual void AddSoftForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot, int frames)
	{
		AddSoftForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot), frames);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateInput()
	{
		if (!base.Player.Dead.Active)
		{
			m_LookInput = FPPlayer.InputRawLook.Get() / base.Delta * Time.timeScale * Time.timeScale;
			m_LookInput *= RotationInputVelocityScale;
			m_LookInput = Vector3.Min(m_LookInput, Vector3.one * RotationMaxInputVelocity);
			m_LookInput = Vector3.Max(m_LookInput, Vector3.one * (0f - RotationMaxInputVelocity));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateZoom()
	{
		if (!(m_FinalZoomTime <= Time.time) && m_Wielded)
		{
			RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
			float num = 1f - (m_FinalZoomTime - Time.time) / RenderingZoomDamping;
			if (m_CameraMatrixOverride != null)
			{
				m_CameraMatrixOverride.fov = Mathf.SmoothStep(m_CameraMatrixOverride.fov, RenderingFieldOfView, num * 15f);
			}
		}
	}

	public virtual void Zoom()
	{
		m_FinalZoomTime = Time.time + RenderingZoomDamping;
	}

	public virtual void SnapZoom()
	{
		if (m_CameraMatrixOverride != null)
		{
			m_CameraMatrixOverride.fov = RenderingFieldOfView;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateShakes()
	{
		if (ShakeSpeed != 0f)
		{
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
			m_RotationSpring.AddForce(m_Shake * Time.timeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateRetraction(bool firstIteration = true)
	{
		if (RetractionDistance != 0f)
		{
			Vector3 vector = WeaponModel.transform.TransformPoint(RetractionOffset);
			Vector3 end = vector + WeaponModel.transform.forward * RetractionDistance;
			if (Physics.Linecast(vector, end, out var hitInfo, 1084850176) && !hitInfo.collider.isTrigger)
			{
				WeaponModel.transform.position = hitInfo.point - (hitInfo.point - vector).normalized * (RetractionDistance * 0.99f);
				WeaponModel.transform.localPosition = Vector3.forward * Mathf.Min(WeaponModel.transform.localPosition.z, 0f);
			}
			else if (firstIteration && WeaponModel.transform.localPosition != Vector3.zero && WeaponModel != base.gameObject)
			{
				WeaponModel.transform.localPosition = Vector3.forward * Mathf.SmoothStep(WeaponModel.transform.localPosition.z, 0f, RetractionRelaxSpeed * Time.timeScale);
				UpdateRetraction(firstIteration: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBob()
	{
		if (!(BobAmplitude == Vector4.zero) && !(BobRate == Vector4.zero))
		{
			m_BobSpeed = ((BobRequireGroundContact && !Controller.isGrounded) ? 0f : Controller.velocity.sqrMagnitude);
			m_BobSpeed = Mathf.Min(m_BobSpeed * BobInputVelocityScale, BobMaxInputVelocity);
			m_BobSpeed = Mathf.Round(m_BobSpeed * 1000f) / 1000f;
			if (m_BobSpeed == 0f)
			{
				m_BobSpeed = Mathf.Min(m_LastBobSpeed * 0.93f, BobMaxInputVelocity);
			}
			m_CurrentBobAmp.x = m_BobSpeed * (BobAmplitude.x * -0.0001f);
			m_CurrentBobVal.x = Mathf.Cos(Time.time * (BobRate.x * 10f)) * m_CurrentBobAmp.x;
			m_CurrentBobAmp.y = m_BobSpeed * (BobAmplitude.y * 0.0001f);
			m_CurrentBobVal.y = Mathf.Cos(Time.time * (BobRate.y * 10f)) * m_CurrentBobAmp.y;
			m_CurrentBobAmp.z = m_BobSpeed * (BobAmplitude.z * 0.0001f);
			m_CurrentBobVal.z = Mathf.Cos(Time.time * (BobRate.z * 10f)) * m_CurrentBobAmp.z;
			m_CurrentBobAmp.w = m_BobSpeed * (BobAmplitude.w * 0.0001f);
			m_CurrentBobVal.w = Mathf.Cos(Time.time * (BobRate.w * 10f)) * m_CurrentBobAmp.w;
			m_RotationSpring.AddForce(m_CurrentBobVal * Time.timeScale);
			m_PositionSpring.AddForce(Vector3.forward * m_CurrentBobVal.w * Time.timeScale);
			m_LastBobSpeed = m_BobSpeed;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateEarthQuake()
	{
		if (!(FPPlayer == null) && FPPlayer.CameraEarthQuake.Active && Controller.isGrounded)
		{
			Vector3 vector = FPPlayer.CameraEarthQuakeForce.Get();
			AddForce(new Vector3(0f, 0f, (0f - vector.z) * 0.015f), new Vector3(vector.y * 2f, 0f - vector.x, vector.x * 2f));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateSprings()
	{
		m_PositionSpring.FixedUpdate();
		m_PositionPivotSpring.FixedUpdate();
		m_RotationPivotSpring.FixedUpdate();
		m_RotationSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring2.FixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookDown()
	{
		if (!LookDownActive || (FPPlayer.Rotation.Get().x < 0f && m_LookDownPitch == 0f && m_LookDownYaw == 0f))
		{
			return;
		}
		if (FPPlayer.Rotation.Get().x > 0f)
		{
			m_LookDownPitch = Mathf.Lerp(m_LookDownPitch, vp_MathUtility.SnapToZero(Mathf.Max(0f, FPPlayer.Rotation.Get().x / 90f)), Time.deltaTime * 2f);
			m_LookDownYaw = Mathf.Lerp(m_LookDownYaw, vp_MathUtility.SnapToZero(Mathf.DeltaAngle(FPPlayer.Rotation.Get().y, FPPlayer.BodyYaw.Get())) / 90f * vp_MathUtility.SnapToZero(Mathf.Max(0f, (FPPlayer.Rotation.Get().x - LookDownYawLimit) / (90f - LookDownYawLimit))), Time.deltaTime * 2f);
		}
		else
		{
			m_LookDownPitch *= 0.9f;
			m_LookDownYaw *= 0.9f;
			if (m_LookDownPitch < 0.01f)
			{
				m_LookDownPitch = 0f;
			}
			if (m_LookDownYaw < 0.01f)
			{
				m_LookDownYaw = 0f;
			}
		}
		m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_WeaponGroupTransform.localPosition, LookDownPositionOffsetMiddle, m_LookDownCurve.Evaluate(m_LookDownPitch)));
		m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(m_WeaponGroupTransform.localRotation, Quaternion.Euler(LookDownRotationOffsetMiddle), m_LookDownPitch));
		if (m_LookDownYaw > 0f)
		{
			m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_WeaponGroupTransform.localPosition, LookDownPositionOffsetLeft, Mathf.SmoothStep(0f, 1f, m_LookDownYaw)));
			m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(m_WeaponGroupTransform.localRotation, Quaternion.Euler(LookDownRotationOffsetLeft), m_LookDownYaw));
		}
		else
		{
			m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(m_WeaponGroupTransform.localPosition, LookDownPositionOffsetRight, Mathf.SmoothStep(0f, 1f, 0f - m_LookDownYaw)));
			m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(m_WeaponGroupTransform.localRotation, Quaternion.Euler(LookDownRotationOffsetRight), 0f - m_LookDownYaw));
		}
		m_CurrentPosRestState = Vector3.Lerp(m_CurrentPosRestState, m_PositionSpring.RestState, Time.fixedDeltaTime);
		m_CurrentRotRestState = Vector3.Lerp(m_CurrentRotRestState, m_RotationSpring.RestState, Time.fixedDeltaTime);
		m_WeaponGroupTransform.localPosition += vp_MathUtility.NaNSafeVector3((m_PositionSpring.State - m_CurrentPosRestState) * (m_LookDownPitch * LookDownPositionSpringPower));
		m_WeaponGroupTransform.localEulerAngles -= vp_MathUtility.NaNSafeVector3(new Vector3(Mathf.DeltaAngle(m_RotationSpring.State.x, m_CurrentRotRestState.x), Mathf.DeltaAngle(m_RotationSpring.State.y, m_CurrentRotRestState.y), Mathf.DeltaAngle(m_RotationSpring.State.z, m_CurrentRotRestState.z)) * (m_LookDownPitch * LookDownRotationSpringPower));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateStep()
	{
		if (StepMinVelocity <= 0f || (BobRequireGroundContact && !Controller.isGrounded) || Controller.velocity.sqrMagnitude < StepMinVelocity)
		{
			return;
		}
		bool flag = ((m_LastUpBob < m_CurrentBobVal.x) ? true : false);
		m_LastUpBob = m_CurrentBobVal.x;
		if (flag && !m_BobWasElevating)
		{
			if (Mathf.Cos(Time.time * (BobRate.x * 5f)) > 0f)
			{
				m_PosStep = StepPositionForce - StepPositionForce * StepPositionBalance;
				m_RotStep = StepRotationForce - StepPositionForce * StepRotationBalance;
			}
			else
			{
				m_PosStep = StepPositionForce + StepPositionForce * StepPositionBalance;
				m_RotStep = Vector3.Scale(StepRotationForce - StepPositionForce * StepRotationBalance, -Vector3.one + Vector3.right * 2f);
			}
			AddSoftForce(m_PosStep * StepForceScale, m_RotStep * StepForceScale, StepSoftness);
		}
		m_BobWasElevating = flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSwaying()
	{
		m_SwayVel = Controller.velocity * PositionInputVelocityScale;
		m_SwayVel = Vector3.Min(m_SwayVel, Vector3.one * PositionMaxInputVelocity);
		m_SwayVel = Vector3.Max(m_SwayVel, Vector3.one * (0f - PositionMaxInputVelocity));
		m_SwayVel *= Time.timeScale;
		Vector3 vector = base.Transform.InverseTransformDirection(m_SwayVel / 60f);
		m_RotationSpring.AddForce(new Vector3(m_LookInput.y * (RotationLookSway.x * 0.025f), m_LookInput.x * (RotationLookSway.y * -0.025f), m_LookInput.x * (RotationLookSway.z * -0.025f)));
		m_FallSway = RotationFallSway * (m_SwayVel.y * 0.005f);
		if (Controller.isGrounded)
		{
			m_FallSway *= RotationSlopeSway;
		}
		m_FallSway.z = Mathf.Max(0f, m_FallSway.z);
		m_RotationSpring.AddForce(m_FallSway);
		m_PositionSpring.AddForce(Vector3.forward * (0f - Mathf.Abs(m_SwayVel.y * (PositionFallRetract * 2.5E-05f))));
		m_PositionSpring.AddForce(new Vector3(vector.x * (PositionWalkSlide.x * 0.0016f), 0f - Mathf.Abs(vector.x * (PositionWalkSlide.y * 0.0016f)), (0f - vector.z) * (PositionWalkSlide.z * 0.0016f)));
		m_RotationSpring.AddForce(new Vector3(0f - Mathf.Abs(vector.x * (RotationStrafeSway.x * 0.16f)), 0f - vector.x * (RotationStrafeSway.y * 0.16f), vector.x * (RotationStrafeSway.z * 0.16f)));
	}

	public virtual void ResetSprings(float positionReset, float rotationReset, float positionPauseTime = 0f, float rotationPauseTime = 0f)
	{
		m_PositionSpring.State = Vector3.Lerp(m_PositionSpring.State, m_PositionSpring.RestState, positionReset);
		m_RotationSpring.State = Vector3.Lerp(m_RotationSpring.State, m_RotationSpring.RestState, rotationReset);
		m_PositionPivotSpring.State = Vector3.Lerp(m_PositionPivotSpring.State, m_PositionPivotSpring.RestState, positionReset);
		m_RotationPivotSpring.State = Vector3.Lerp(m_RotationPivotSpring.State, m_RotationPivotSpring.RestState, rotationReset);
		if (positionPauseTime != 0f)
		{
			m_PositionSpring.ForceVelocityFadeIn(positionPauseTime);
		}
		if (rotationPauseTime != 0f)
		{
			m_RotationSpring.ForceVelocityFadeIn(rotationPauseTime);
		}
		if (positionPauseTime != 0f)
		{
			m_PositionPivotSpring.ForceVelocityFadeIn(positionPauseTime);
		}
		if (rotationPauseTime != 0f)
		{
			m_RotationPivotSpring.ForceVelocityFadeIn(rotationPauseTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 SmoothStep(Vector3 from, Vector3 to, float amount)
	{
		return new Vector3(Mathf.SmoothStep(from.x, to.x, amount), Mathf.SmoothStep(from.y, to.y, amount), Mathf.SmoothStep(from.z, to.z, amount));
	}

	public override void Refresh()
	{
		if (Application.isPlaying)
		{
			float num = 1f - (m_FinalZoomTime - Time.time) / Mathf.Max(RenderingZoomDamping, 0.01f);
			if (m_Wielded)
			{
				PositionOffset = SmoothStep(PositionOffset, DefaultPosition + AimingPositionOffset, num * 15f);
			}
			else
			{
				PositionOffset = SmoothStep(PositionOffset, PositionExitOffset, num * 15f);
			}
			if (m_PositionSpring != null)
			{
				m_PositionSpring.Stiffness = new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
				m_PositionSpring.Damping = Vector3.one - new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);
				m_PositionSpring.RestState = PositionOffset - PositionPivot;
			}
			if (m_PositionPivotSpring != null)
			{
				m_PositionPivotSpring.Stiffness = new Vector3(PositionPivotSpringStiffness, PositionPivotSpringStiffness, PositionPivotSpringStiffness);
				m_PositionPivotSpring.Damping = Vector3.one - new Vector3(PositionPivotSpringDamping, PositionPivotSpringDamping, PositionPivotSpringDamping);
				m_PositionPivotSpring.RestState = PositionPivot;
			}
			if (m_RotationPivotSpring != null)
			{
				m_RotationPivotSpring.Stiffness = new Vector3(RotationPivotSpringStiffness, RotationPivotSpringStiffness, RotationPivotSpringStiffness);
				m_RotationPivotSpring.Damping = Vector3.one - new Vector3(RotationPivotSpringDamping, RotationPivotSpringDamping, RotationPivotSpringDamping);
				m_RotationPivotSpring.RestState = RotationPivot;
			}
			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.Stiffness = new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
				m_PositionSpring2.Damping = Vector3.one - new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
				m_PositionSpring2.RestState = Vector3.zero;
			}
			if (m_RotationSpring != null)
			{
				m_RotationSpring.Stiffness = new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
				m_RotationSpring.Damping = Vector3.one - new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);
				m_RotationSpring.RestState = RotationOffset;
			}
			if (m_RotationSpring2 != null)
			{
				m_RotationSpring2.Stiffness = new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
				m_RotationSpring2.Damping = Vector3.one - new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);
				m_RotationSpring2.RestState = Vector3.zero;
			}
			if (base.Rendering)
			{
				Zoom();
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		SnapZoom();
		if (m_WeaponGroup != null && !vp_Utility.IsActive(m_WeaponGroup))
		{
			vp_Utility.Activate(m_WeaponGroup);
		}
		SetPivotVisible(visible: false);
	}

	public override void Deactivate()
	{
		m_Wielded = false;
		if (m_WeaponGroup != null && vp_Utility.IsActive(m_WeaponGroup))
		{
			vp_Utility.Activate(m_WeaponGroup, activate: false);
		}
	}

	public virtual void SnapPivot()
	{
		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
		}
		if (m_WeaponGroup != null)
		{
			m_WeaponGroupTransform.localPosition = PositionOffset - PositionPivot;
		}
		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
		}
		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.RestState = RotationPivot;
			m_RotationPivotSpring.State = RotationPivot;
		}
		base.Transform.localPosition = PositionPivot;
		base.Transform.localEulerAngles = RotationPivot;
	}

	public virtual void SetPivotVisible(bool visible)
	{
		if (!(m_Pivot == null))
		{
			vp_Utility.Activate(m_Pivot.gameObject, visible);
		}
	}

	public virtual void SnapToExit()
	{
		RotationOffset = RotationExitOffset;
		PositionOffset = PositionExitOffset;
		SnapSprings();
		SnapPivot();
	}

	public override void SnapSprings()
	{
		base.SnapSprings();
		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
			m_PositionSpring.Stop(includeSoftForce: true);
		}
		if (m_WeaponGroup != null)
		{
			m_WeaponGroupTransform.localPosition = PositionOffset - PositionPivot;
		}
		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
			m_PositionPivotSpring.Stop(includeSoftForce: true);
		}
		base.Transform.localPosition = PositionPivot;
		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.RestState = RotationPivot;
			m_RotationPivotSpring.State = RotationPivot;
			m_RotationPivotSpring.Stop(includeSoftForce: true);
		}
		base.Transform.localEulerAngles = RotationPivot;
		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = RotationOffset;
			m_RotationSpring.State = RotationOffset;
			m_RotationSpring.Stop(includeSoftForce: true);
		}
	}

	public override void StopSprings()
	{
		if (m_PositionSpring != null)
		{
			m_PositionSpring.Stop(includeSoftForce: true);
		}
		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring != null)
		{
			m_RotationSpring.Stop(includeSoftForce: true);
		}
		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.Stop(includeSoftForce: true);
		}
	}

	public override void Wield(bool isWielding = true)
	{
		if (isWielding)
		{
			SnapToExit();
		}
		PositionOffset = (isWielding ? (DefaultPosition + AimingPositionOffset) : PositionExitOffset);
		RotationOffset = (isWielding ? DefaultRotation : RotationExitOffset);
		m_Wielded = isWielding;
		Refresh();
		base.StateManager.CombineStates();
		if (base.Audio != null && (isWielding ? SoundWield : SoundUnWield) != null && vp_Utility.IsActive(base.gameObject))
		{
			base.Audio.pitch = Time.timeScale;
			base.Audio.PlayOneShot(isWielding ? SoundWield : SoundUnWield);
		}
		if ((isWielding ? AnimationWield : AnimationUnWield) != null && vp_Utility.IsActive(base.gameObject))
		{
			if (isWielding)
			{
				m_WeaponModel.GetComponent<Animation>().CrossFade(AnimationWield.name);
			}
			else
			{
				m_WeaponModel.GetComponent<Animation>().CrossFade(AnimationUnWield.name);
			}
		}
	}

	public virtual void ScheduleAmbientAnimation()
	{
		if (AnimationAmbient.Count == 0 || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		vp_Timer.In(UnityEngine.Random.Range(AmbientInterval.x, AmbientInterval.y), [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (vp_Utility.IsActive(base.gameObject))
			{
				m_CurrentAmbientAnimation = UnityEngine.Random.Range(0, AnimationAmbient.Count);
				if (AnimationAmbient[m_CurrentAmbientAnimation] != null)
				{
					m_WeaponModel.GetComponent<Animation>().CrossFadeQueued(AnimationAmbient[m_CurrentAmbientAnimation].name);
					ScheduleAmbientAnimation();
				}
			}
		}, m_AnimationAmbientTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		if (m_PositionSpring != null)
		{
			m_PositionSpring.AddSoftForce(Vector3.down * impact * PositionKneeling, PositionKneelingSoftness);
		}
		if (m_RotationSpring != null)
		{
			m_RotationSpring.AddSoftForce(Vector3.right * impact * RotationKneeling, RotationKneelingSoftness);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HeadImpact(float impact)
	{
		AddForce(Vector3.zero, Vector3.forward * (impact * 20f) * Time.timeScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraGroundStomp(float impact)
	{
		AddForce(Vector3.zero, new Vector3(-0.25f, 0f, 0f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		AddForce(Vector3.zero, new Vector3(-0.3f, 0.1f, 0.5f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		RefreshWeaponModel();
	}
}
