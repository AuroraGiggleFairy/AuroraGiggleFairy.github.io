using System;
using UnityEngine;

public class vp_Weapon : vp_Component
{
	public new enum Type
	{
		Custom,
		Firearm,
		Melee,
		Thrown
	}

	public enum Grip
	{
		Custom,
		OneHanded,
		TwoHanded
	}

	public GameObject Weapon3rdPersonModel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject m_WeaponModel;

	public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);

	public Vector3 AimingPositionOffset = new Vector3(0f, 0f, 0f);

	public float PositionSpring2Stiffness = 0.95f;

	public float PositionSpring2Damping = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_PositionSpring2;

	public Vector3 RotationOffset = Vector3.zero;

	public float RotationSpring2Stiffness = 0.95f;

	public float RotationSpring2Damping = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring m_RotationSpring2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Wielded = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Weapon3rdPersonModelWakeUpTimer = new vp_Timer.Handle();

	public int AnimationType = 1;

	public int AnimationGrip = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_RotationSpring2DefaultRotation = Vector3.zero;

	public bool Wielded
	{
		get
		{
			if (m_Wielded)
			{
				return base.Rendering;
			}
			return false;
		}
		set
		{
			m_Wielded = value;
		}
	}

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null && base.EventHandler != null)
			{
				m_Player = (vp_PlayerEventHandler)base.EventHandler;
			}
			return m_Player;
		}
	}

	public Vector3 RotationSpring2DefaultRotation
	{
		get
		{
			return m_RotationSpring2DefaultRotation;
		}
		set
		{
			m_RotationSpring2DefaultRotation = value;
		}
	}

	public Vector3 Recoil => m_RotationSpring2.State;

	public virtual Vector3 OnValue_AimDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (Weapon3rdPersonModel.transform.position - Player.LookPoint.Get()).normalized;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		RotationOffset = base.transform.localEulerAngles;
		PositionOffset = base.transform.position;
		base.Transform.localEulerAngles = RotationOffset;
		if (base.transform.parent == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") Must not be placed in scene root. Disabling self.");
			vp_Utility.Activate(base.gameObject, activate: false);
		}
		else if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		RefreshWeaponModel();
		base.OnEnable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		RefreshWeaponModel();
		Activate3rdPersonModel(active: false);
		base.OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		m_PositionSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.PositionAdditiveSelf);
		m_PositionSpring2.MinVelocity = 1E-05f;
		m_RotationSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.RotationAdditiveGlobal);
		m_RotationSpring2.MinVelocity = 1E-05f;
		SnapSprings();
		Refresh();
		CacheRenderers();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Time.timeScale != 0f)
		{
			UpdateSprings();
		}
	}

	public virtual void AddForce2(Vector3 positional, Vector3 angular)
	{
		m_PositionSpring2.AddForce(positional);
		m_RotationSpring2.AddForce(angular);
	}

	public virtual void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSprings()
	{
		base.Transform.localPosition = Vector3.up;
		base.Transform.localRotation = Quaternion.identity;
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring2.FixedUpdate();
	}

	public override void Refresh()
	{
		if (Application.isPlaying)
		{
			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.Stiffness = new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
				m_PositionSpring2.Damping = Vector3.one - new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
				m_PositionSpring2.RestState = Vector3.zero;
			}
			if (m_RotationSpring2 != null)
			{
				m_RotationSpring2.Stiffness = new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
				m_RotationSpring2.Damping = Vector3.one - new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);
				m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		m_Wielded = true;
		base.Rendering = true;
	}

	public virtual void SnapSprings()
	{
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
			m_RotationSpring2.State = m_RotationSpring2DefaultRotation;
			m_RotationSpring2.Stop(includeSoftForce: true);
		}
	}

	public virtual void StopSprings()
	{
		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.Stop(includeSoftForce: true);
		}
		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.Stop(includeSoftForce: true);
		}
	}

	public virtual void Wield(bool isWielding = true)
	{
		m_Wielded = isWielding;
		Refresh();
		base.StateManager.CombineStates();
	}

	public virtual void RefreshWeaponModel()
	{
		if (!(Player == null))
		{
			_ = Player.IsFirstPerson;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Activate3rdPersonModel(bool active = true)
	{
		if (Weapon3rdPersonModel == null)
		{
			return;
		}
		if (active)
		{
			Weapon3rdPersonModel.GetComponent<Renderer>().enabled = true;
			vp_Utility.Activate(Weapon3rdPersonModel);
			return;
		}
		Weapon3rdPersonModel.GetComponent<Renderer>().enabled = false;
		vp_Timer.In(0.1f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (Weapon3rdPersonModel != null)
			{
				vp_Utility.Activate(Weapon3rdPersonModel, activate: false);
			}
		}, m_Weapon3rdPersonModelWakeUpTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (!Player.IsFirstPerson.Get())
		{
			base.Rendering = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		if (!Player.IsFirstPerson.Get())
		{
			base.Rendering = true;
		}
	}
}
