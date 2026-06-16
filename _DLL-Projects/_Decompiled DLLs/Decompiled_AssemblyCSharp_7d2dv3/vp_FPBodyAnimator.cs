using System;
using System.Collections;
using UnityEngine;

public class vp_FPBodyAnimator : vp_BodyAnimator
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPController m_FPController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_FPCamera;

	public Vector3 EyeOffset = new Vector3(0f, -0.08f, -0.1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_WasFirstPersonLastFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_DefaultCamHeight;

	public float LookDownZoomFactor = 15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float LookDownForwardOffset = 0.05f;

	public bool ShowUnarmedArms = true;

	public Material InvisibleMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Material[] m_FirstPersonMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Material[] m_FirstPersonWithArmsMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Material[] m_ThirdPersonMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Material[] m_InvisiblePersonMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPWeaponShooter m_CurrentShooter;

	public vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
			{
				m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
			}
			return m_FPCamera;
		}
	}

	public vp_FPController FPController
	{
		get
		{
			if (m_FPController == null)
			{
				m_FPController = base.transform.root.GetComponent<vp_FPController>();
			}
			return m_FPController;
		}
	}

	public vp_FPWeaponShooter CurrentShooter
	{
		get
		{
			if ((m_CurrentShooter == null || (m_CurrentShooter != null && (!m_CurrentShooter.enabled || !vp_Utility.IsActive(m_CurrentShooter.gameObject)))) && base.WeaponHandler != null && base.WeaponHandler.CurrentWeapon != null)
			{
				m_CurrentShooter = base.WeaponHandler.CurrentWeapon.GetComponentInChildren<vp_FPWeaponShooter>();
			}
			return m_CurrentShooter;
		}
	}

	public float DefaultCamHeight
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_DefaultCamHeight == 0f)
			{
				if (FPCamera != null && FPCamera.DefaultState != null && FPCamera.DefaultState.Preset != null)
				{
					m_DefaultCamHeight = ((Vector3)FPCamera.DefaultState.Preset.GetFieldValue("PositionOffset")).y;
				}
				else
				{
					m_DefaultCamHeight = 1.75f;
				}
			}
			return m_DefaultCamHeight;
		}
	}

	public override Vector3 OnValue_LookPoint
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetLookPoint();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		InitMaterials();
		m_WasFirstPersonLastFrame = base.Player.IsFirstPerson.Get();
		FPCamera.HasCollision = true;
		base.Player.IsFirstPerson.Set(o: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		base.LateUpdate();
		if (Time.timeScale != 0f)
		{
			if (base.Player.IsFirstPerson.Get())
			{
				UpdatePosition();
				UpdateCameraPosition();
				UpdateCameraCollision();
			}
			else
			{
				FPCamera.DoCameraCollision();
			}
			UpdateFirePosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator RefreshMaterialsOnEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RefreshMaterials()
	{
		if (InvisibleMaterial == null)
		{
			Debug.LogWarning("Warning (" + this?.ToString() + ") No invisible material has been set. Head and arms will look buggy in first person.");
		}
		else if (!base.Player.IsFirstPerson.Get())
		{
			if (m_ThirdPersonMaterials != null)
			{
				base.Renderer.materials = m_ThirdPersonMaterials;
			}
		}
		else if (!base.Player.Dead.Active)
		{
			if (ShowUnarmedArms && base.Player.CurrentWeaponIndex.Get() < 1 && !base.Player.Climb.Active)
			{
				if (m_FirstPersonWithArmsMaterials != null)
				{
					base.Renderer.materials = m_FirstPersonWithArmsMaterials;
				}
			}
			else if (m_FirstPersonMaterials != null)
			{
				base.Renderer.materials = m_FirstPersonMaterials;
			}
		}
		else if (m_InvisiblePersonMaterials != null)
		{
			base.Renderer.materials = m_InvisiblePersonMaterials;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdatePosition()
	{
		base.Transform.position = FPController.SmoothPosition + FPController.SkinWidth * Vector3.down;
		if (base.Player.IsFirstPerson.Get() && !base.Player.Climb.Active)
		{
			if (m_HeadLookBones != null && m_HeadLookBones.Count > 0)
			{
				base.Transform.position = Vector3.Lerp(base.Transform.position, base.Transform.position + (FPCamera.Transform.position - m_HeadLookBones[0].transform.position), Mathf.Lerp(1f, 0f, Mathf.Max(0f, base.Player.Rotation.Get().x / 60f)));
			}
			else
			{
				Debug.LogWarning("Warning (" + this?.ToString() + ") No headlookbones have been assigned!");
			}
		}
		else
		{
			base.Transform.localPosition = Vector3.Scale(base.Transform.localPosition, Vector3.right + Vector3.up);
		}
		if (base.Player.Climb.Active)
		{
			base.Transform.localPosition += ClimbOffset;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCameraPosition()
	{
		FPCamera.transform.position = m_HeadLookBones[0].transform.position;
		float t = Mathf.Max(0f, (base.Player.Rotation.Get().x - 45f) / 45f);
		t = Mathf.SmoothStep(0f, 1f, t);
		FPCamera.transform.localPosition = new Vector3(FPCamera.transform.localPosition.x, FPCamera.transform.localPosition.y, FPCamera.transform.localPosition.z + t * (base.Player.Crouch.Active ? 0f : LookDownForwardOffset));
		FPCamera.Transform.localPosition -= EyeOffset;
		FPCamera.ZoomOffset = (0f - LookDownZoomFactor) * t;
		FPCamera.RefreshZoom();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCameraCollision()
	{
		FPCamera.DoCameraCollision();
		if (FPCamera.CollisionVector != Vector3.zero)
		{
			base.Transform.position += FPCamera.CollisionVector;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateGrounding()
	{
		m_Grounded = FPController.Grounded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateDebugInfo()
	{
		if (ShowDebugObjects)
		{
			base.DebugLookTarget.transform.position = FPCamera.LookPoint;
			base.DebugLookArrow.transform.LookAt(base.DebugLookTarget.transform.position);
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateFirePosition()
	{
		if (!(CurrentShooter == null) && !(CurrentShooter.ProjectileSpawnPoint == null))
		{
			CurrentShooter.FirePosition = CurrentShooter.ProjectileSpawnPoint.transform.position;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitMaterials()
	{
		if (InvisibleMaterial == null)
		{
			Debug.LogWarning("Warning () No invisible material has been set.");
			return;
		}
		m_FirstPersonMaterials = new Material[base.Renderer.materials.Length];
		m_FirstPersonWithArmsMaterials = new Material[base.Renderer.materials.Length];
		m_ThirdPersonMaterials = new Material[base.Renderer.materials.Length];
		m_InvisiblePersonMaterials = new Material[base.Renderer.materials.Length];
		for (int i = 0; i < base.Renderer.materials.Length; i++)
		{
			m_ThirdPersonMaterials[i] = base.Renderer.materials[i];
			if (base.Renderer.materials[i].name.ToLower().Contains("head") || base.Renderer.materials[i].name.ToLower().Contains("arm"))
			{
				m_FirstPersonMaterials[i] = InvisibleMaterial;
			}
			else
			{
				m_FirstPersonMaterials[i] = base.Renderer.materials[i];
			}
			if (base.Renderer.materials[i].name.ToLower().Contains("head"))
			{
				m_FirstPersonWithArmsMaterials[i] = InvisibleMaterial;
			}
			else
			{
				m_FirstPersonWithArmsMaterials[i] = base.Renderer.materials[i];
			}
			m_InvisiblePersonMaterials[i] = InvisibleMaterial;
		}
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetIsMoving()
	{
		return Vector3.Scale(base.Player.MotorThrottle.Get(), Vector3.right + Vector3.forward).magnitude > 0.01f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetLookPoint()
	{
		return FPCamera.LookPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnMessage_CameraToggle3rdPerson()
	{
		base.OnMessage_CameraToggle3rdPerson();
		StartCoroutine(RefreshMaterialsOnEndOfFrame());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Climb()
	{
		base.OnStart_Climb();
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Climb()
	{
		RefreshMaterials();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Dead()
	{
		base.OnStart_Dead();
		RefreshMaterials();
	}
}
