using System;
using UnityEngine;

public class vp_3rdPersonWeaponAim : MonoBehaviour
{
	public GameObject Hand;

	[Range(0f, 360f)]
	public float AngleAdjustX;

	[Range(0f, 360f)]
	public float AngleAdjustY;

	[Range(0f, 360f)]
	public float AngleAdjustZ;

	[Range(0f, 5f)]
	public float RecoilFactorX = 1f;

	[Range(0f, 5f)]
	public float RecoilFactorY = 1f;

	[Range(0f, 5f)]
	public float RecoilFactorZ = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_DefaultRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_ReferenceUpDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_ReferenceLookDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion m_HandBoneRotDif;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_WorldDir = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_WeaponHandler m_WeaponHandler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator m_Animator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_Root;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_LowerArmObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform m_HandObj;

	public Transform Transform
	{
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
		get
		{
			if (m_Player == null)
			{
				m_Player = (vp_PlayerEventHandler)Root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
			}
			return m_Player;
		}
	}

	public vp_WeaponHandler WeaponHandler
	{
		get
		{
			if (m_WeaponHandler == null)
			{
				m_WeaponHandler = (vp_WeaponHandler)Root.GetComponentInChildren(typeof(vp_WeaponHandler));
			}
			return m_WeaponHandler;
		}
	}

	public Animator Animator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Animator == null)
			{
				m_Animator = Root.GetComponentInChildren<Animator>();
			}
			return m_Animator;
		}
	}

	public Transform Root
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_Root == null)
			{
				m_Root = Transform.root;
			}
			return m_Root;
		}
	}

	public Transform LowerArmObj
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_LowerArmObj == null)
			{
				m_LowerArmObj = HandObj.parent;
			}
			return m_LowerArmObj;
		}
	}

	public Transform HandObj
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_HandObj == null)
			{
				if (Hand != null)
				{
					m_HandObj = Hand.transform;
				}
				else
				{
					m_HandObj = vp_Utility.GetTransformByNameInAncestors(Transform, "hand", includeInactive: true, subString: true);
					if (m_HandObj == null && Transform.parent != null)
					{
						m_HandObj = Transform.parent;
					}
					if (m_HandObj != null)
					{
						Hand = m_HandObj.gameObject;
					}
				}
			}
			return m_HandObj;
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
		m_DefaultRotation = Transform.localRotation;
		if (LowerArmObj == null || HandObj == null)
		{
			Debug.LogError("Hierarchy Error (" + this?.ToString() + ") This script should be placed on a 3rd person weapon gameobject childed to a hand bone in a rigged character.");
			base.enabled = false;
			return;
		}
		Quaternion quaternion = Quaternion.Inverse(LowerArmObj.rotation);
		m_ReferenceLookDir = quaternion * Root.rotation * Vector3.forward;
		m_ReferenceUpDir = quaternion * Root.rotation * Vector3.up;
		Quaternion rotation = HandObj.rotation;
		HandObj.rotation = Root.rotation;
		Quaternion rotation2 = HandObj.rotation;
		HandObj.rotation = rotation;
		m_HandBoneRotDif = Quaternion.Inverse(rotation2) * rotation;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateAiming();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAiming()
	{
		if (!(Animator == null))
		{
			if ((!Animator.GetBool("IsAttacking") && !Animator.GetBool("IsZooming")) || Animator.GetBool("IsReloading") || Animator.GetBool("IsOutOfControl") || Player.CurrentWeaponIndex.Get() == 0)
			{
				Transform.localRotation = m_DefaultRotation;
				return;
			}
			Quaternion rotation = Transform.rotation;
			Transform.rotation = Quaternion.LookRotation(Player.AimDirection.Get());
			m_WorldDir = Transform.forward;
			Transform.rotation = rotation;
			HandObj.rotation = vp_3DUtility.GetBoneLookRotationInWorldSpace(HandObj.rotation, LowerArmObj.rotation, m_WorldDir, 1f, m_ReferenceUpDir, m_ReferenceLookDir, m_HandBoneRotDif);
			HandObj.Rotate(Transform.forward, AngleAdjustZ + WeaponHandler.CurrentWeapon.Recoil.z * RecoilFactorZ, Space.World);
			HandObj.Rotate(Transform.up, AngleAdjustY + WeaponHandler.CurrentWeapon.Recoil.y * RecoilFactorY, Space.World);
			HandObj.Rotate(Transform.right, AngleAdjustX + WeaponHandler.CurrentWeapon.Recoil.x * RecoilFactorX, Space.World);
		}
	}
}
