using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPInteractManager : MonoBehaviour
{
	public float InteractDistance = 2f;

	public float InteractDistance3rdPerson = 3f;

	public float MaxInteractDistance = 25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_Camera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Interactable m_CurrentInteractable;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Texture m_OriginalCrosshair;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Interactable m_LastInteractable;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Collider, vp_Interactable> m_Interactables = new Dictionary<Collider, vp_Interactable>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Interactable m_CurrentCrosshairInteractable;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_ShowTextTimer = new vp_Timer.Handle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_CanInteract;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float CrosshairTimeoutTimer { get; set; }

	public virtual vp_Interactable OnValue_Interactable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_CurrentInteractable;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_CurrentInteractable = value;
		}
	}

	public virtual bool OnValue_CanInteract
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_CanInteract;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_CanInteract = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Player = GetComponent<vp_FPPlayerEventHandler>();
		m_Camera = GetComponentInChildren<vp_FPCamera>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}

	public virtual void OnStart_Dead()
	{
		ShouldFinishInteraction();
	}

	public virtual void LateUpdate()
	{
		if (!m_Player.Dead.Active)
		{
			if (m_OriginalCrosshair == null && m_Player.Crosshair.Get() != null)
			{
				m_OriginalCrosshair = m_Player.Crosshair.Get();
			}
			InteractCrosshair();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Interact()
	{
		if (ShouldFinishInteraction())
		{
			return false;
		}
		if (m_Player.SetWeapon.Active)
		{
			return false;
		}
		vp_Interactable interactable = null;
		if (FindInteractable(out interactable))
		{
			if (interactable.InteractType != vp_Interactable.vp_InteractType.Normal)
			{
				return false;
			}
			if (!interactable.TryInteract(m_Player))
			{
				return false;
			}
			ResetCrosshair(reset: false);
			m_LastInteractable = interactable;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ShouldFinishInteraction()
	{
		if (m_Player.Interactable.Get() != null)
		{
			m_CurrentCrosshairInteractable = null;
			ResetCrosshair();
			m_Player.Interactable.Get().FinishInteraction();
			m_Player.Interactable.Set(null);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InteractCrosshair()
	{
		if (m_Player.Crosshair.Get() == null || m_Player.Interactable.Get() != null)
		{
			return;
		}
		vp_Interactable interactable = null;
		if (FindInteractable(out interactable))
		{
			if (!(interactable != m_CurrentCrosshairInteractable) || (CrosshairTimeoutTimer > Time.time && m_LastInteractable != null && interactable.GetType() == m_LastInteractable.GetType()))
			{
				return;
			}
			m_CanInteract = true;
			m_CurrentCrosshairInteractable = interactable;
			if (interactable.InteractText != "" && !m_ShowTextTimer.Active)
			{
				vp_Timer.In(interactable.DelayShowingText, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					m_Player.HUDText.Send(interactable.InteractText);
				}, m_ShowTextTimer);
			}
			if (!(interactable.m_InteractCrosshair == null))
			{
				m_Player.Crosshair.Set(interactable.m_InteractCrosshair);
			}
		}
		else
		{
			m_CanInteract = false;
			ResetCrosshair();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool FindInteractable(out vp_Interactable interactable)
	{
		interactable = null;
		if (Physics.Raycast(m_Camera.Transform.position, m_Camera.Transform.forward, out var hitInfo, MaxInteractDistance, -538750981))
		{
			if (!m_Interactables.TryGetValue(hitInfo.collider, out interactable))
			{
				m_Interactables.Add(hitInfo.collider, interactable = hitInfo.collider.GetComponent<vp_Interactable>());
			}
			if (interactable == null)
			{
				return false;
			}
			if (interactable.InteractDistance == 0f && hitInfo.distance >= (m_Player.IsFirstPerson.Get() ? InteractDistance : InteractDistance3rdPerson))
			{
				return false;
			}
			if (interactable.InteractDistance > 0f && hitInfo.distance >= interactable.InteractDistance)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ResetCrosshair(bool reset = true)
	{
		if (!(m_OriginalCrosshair == null) && !(m_Player.Crosshair.Get() == m_OriginalCrosshair))
		{
			m_ShowTextTimer.Cancel();
			if (reset)
			{
				m_Player.Crosshair.Set(m_OriginalCrosshair);
			}
			m_CurrentCrosshairInteractable = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			vp_Interactable value = null;
			if (!m_Interactables.TryGetValue(hit.collider, out value))
			{
				m_Interactables.Add(hit.collider, value = hit.collider.GetComponent<vp_Interactable>());
			}
			if (!(value == null) && value.InteractType == vp_Interactable.vp_InteractType.CollisionTrigger)
			{
				hit.gameObject.SendMessage("TryInteract", m_Player, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
