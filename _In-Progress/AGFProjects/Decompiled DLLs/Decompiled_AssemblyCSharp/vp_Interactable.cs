using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class vp_Interactable : MonoBehaviour
{
	public enum vp_InteractType
	{
		Normal,
		Trigger,
		CollisionTrigger
	}

	public vp_InteractType InteractType;

	public List<string> RecipientTags = new List<string>();

	public float InteractDistance;

	public Texture m_InteractCrosshair;

	public string InteractText = "";

	public float DelayShowingText = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPController m_Controller;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPCamera m_Camera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_WeaponHandler m_WeaponHandler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		m_Transform = base.transform;
		if (RecipientTags.Count == 0)
		{
			RecipientTags.Add("Player");
		}
		if (InteractType == vp_InteractType.Trigger && GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().isTrigger = true;
		}
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

	public virtual bool TryInteract(vp_FPPlayerEventHandler player)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (InteractType != vp_InteractType.Trigger)
		{
			return;
		}
		using (List<string>.Enumerator enumerator = RecipientTags.GetEnumerator())
		{
			string current;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					continue;
				}
				return;
			}
			while (!(col.gameObject.tag == current));
		}
		m_Player = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();
		if (!(m_Player == null))
		{
			TryInteract(m_Player);
		}
	}

	public virtual void FinishInteraction()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Interactable()
	{
	}
}
