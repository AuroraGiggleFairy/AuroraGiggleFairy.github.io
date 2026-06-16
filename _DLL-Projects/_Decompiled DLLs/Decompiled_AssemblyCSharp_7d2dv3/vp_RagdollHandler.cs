using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_RagdollHandler : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Collider> m_Colliders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Rigidbody> m_Rigidbodies;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Transform> m_Transforms;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator m_Animator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_BodyAnimator m_BodyAnimator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_FPCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public CharacterController m_CharacterController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_HeadRotationCorrection = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_TimeOfDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CameraFreezeAngle = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Transform, Quaternion> TransformRotations = new Dictionary<Transform, Quaternion>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Transform, Vector3> TransformPositions = new Dictionary<Transform, Vector3>();

	public float VelocityMultiplier = 30f;

	public float CameraFreezeDelay = 2.5f;

	public GameObject HeadBone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_Rot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Pos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_TriedToFetchPlayer;

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null && !m_TriedToFetchPlayer)
			{
				m_Player = base.transform.root.GetComponentInChildren<vp_PlayerEventHandler>();
				m_TriedToFetchPlayer = true;
			}
			return m_Player;
		}
	}

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

	public CharacterController CharacterController
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_CharacterController == null)
			{
				m_CharacterController = base.transform.root.GetComponentInChildren<CharacterController>();
			}
			return m_CharacterController;
		}
	}

	public List<Collider> Colliders
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Colliders == null)
			{
				m_Colliders = new List<Collider>();
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (collider.gameObject.layer != 23)
					{
						m_Colliders.Add(collider);
					}
				}
			}
			return m_Colliders;
		}
	}

	public List<Rigidbody> Rigidbodies
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Rigidbodies == null)
			{
				m_Rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
			}
			return m_Rigidbodies;
		}
	}

	public List<Transform> Transforms
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Transforms == null)
			{
				m_Transforms = new List<Transform>();
				foreach (Rigidbody rigidbody in Rigidbodies)
				{
					m_Transforms.Add(rigidbody.transform);
				}
			}
			return m_Transforms;
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

	public vp_BodyAnimator BodyAnimator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_BodyAnimator == null)
			{
				m_BodyAnimator = GetComponent<vp_BodyAnimator>();
			}
			return m_BodyAnimator;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (Colliders == null || Colliders.Count == 0 || Rigidbodies == null || Rigidbodies.Count == 0 || Transforms == null || Transforms.Count == 0 || Animator == null || BodyAnimator == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Could not be initialized. Please make sure hierarchy has ragdoll colliders, Animator and vp_BodyAnimator.");
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		SetRagdoll(enabled: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SaveStartPose()
	{
		foreach (Transform transform in Transforms)
		{
			if (!TransformRotations.ContainsKey(transform))
			{
				TransformRotations.Add(transform.transform, transform.localRotation);
			}
			if (!TransformPositions.ContainsKey(transform))
			{
				TransformPositions.Add(transform.transform, transform.localPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RestoreStartPose()
	{
		foreach (Transform transform in Transforms)
		{
			if (TransformRotations.TryGetValue(transform, out m_Rot))
			{
				transform.localRotation = m_Rot;
			}
			if (TransformPositions.TryGetValue(transform, out m_Pos))
			{
				transform.localPosition = m_Pos;
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		UpdateDeathCamera();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateDeathCamera()
	{
		if (!(Player == null) && Player.Dead.Active && !(HeadBone == null) && Player.IsFirstPerson.Get())
		{
			FPCamera.Transform.position = HeadBone.transform.position;
			m_HeadRotationCorrection = HeadBone.transform.localEulerAngles;
			if (Time.time - m_TimeOfDeath < CameraFreezeDelay)
			{
				FPCamera.Transform.localEulerAngles = (m_CameraFreezeAngle = new Vector3(0f - m_HeadRotationCorrection.z, 0f - m_HeadRotationCorrection.x, m_HeadRotationCorrection.y));
			}
			else
			{
				FPCamera.Transform.localEulerAngles = m_CameraFreezeAngle;
			}
		}
	}

	public virtual void SetRagdoll(bool enabled = true)
	{
		if (Animator != null)
		{
			Animator.enabled = !enabled;
		}
		if (BodyAnimator != null)
		{
			BodyAnimator.enabled = !enabled;
		}
		if (CharacterController != null)
		{
			CharacterController.enabled = !enabled;
		}
		foreach (Rigidbody rigidbody in Rigidbodies)
		{
			rigidbody.isKinematic = !enabled;
		}
		foreach (Collider collider in Colliders)
		{
			collider.enabled = enabled;
		}
		if (!enabled)
		{
			RestoreStartPose();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		SetRagdoll();
		foreach (Rigidbody rigidbody in Rigidbodies)
		{
			rigidbody.AddForce(Player.Velocity.Get() * VelocityMultiplier);
		}
		m_TimeOfDeath = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		SetRagdoll(enabled: false);
		Player.OutOfControl.Stop();
	}
}
