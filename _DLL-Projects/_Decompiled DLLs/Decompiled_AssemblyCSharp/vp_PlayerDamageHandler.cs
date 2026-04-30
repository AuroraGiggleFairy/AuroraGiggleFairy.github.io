using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PlayerDamageHandler : vp_DamageHandler
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_PlayerInventory m_Inventory;

	public bool AllowFallDamage = true;

	public float FallImpactThreshold = 0.15f;

	public bool DeathOnFallImpactThreshold;

	public Vector2 FallImpactPitch = new Vector2(1f, 1.5f);

	public List<AudioClip> FallImpactSounds = new List<AudioClip>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FallImpactMultiplier = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_InventoryWasEnabledAtStart = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Collider> m_Colliders;

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null)
			{
				m_Player = base.transform.GetComponent<vp_PlayerEventHandler>();
			}
			return m_Player;
		}
	}

	public vp_PlayerInventory Inventory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Inventory == null)
			{
				m_Inventory = base.transform.root.GetComponentInChildren<vp_PlayerInventory>();
			}
			return m_Inventory;
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
					if (collider.gameObject.layer == 23)
					{
						m_Colliders.Add(collider);
					}
				}
			}
			return m_Colliders;
		}
	}

	public virtual float OnValue_Health
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return CurrentHealth;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			CurrentHealth = Mathf.Min(value, MaxHealth);
		}
	}

	public virtual float OnValue_MaxHealth
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return MaxHealth;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		if (Player != null)
		{
			Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (Inventory != null)
		{
			m_InventoryWasEnabledAtStart = Inventory.enabled;
		}
	}

	public override void Die()
	{
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(DeathSound);
		}
		GameObject[] deathSpawnObjects = DeathSpawnObjects;
		foreach (GameObject gameObject in deathSpawnObjects)
		{
			if (gameObject != null)
			{
				vp_Utility.Instantiate(gameObject, base.transform.position, base.transform.rotation);
			}
		}
		foreach (Collider collider in Colliders)
		{
			collider.enabled = false;
		}
		if (Inventory != null && Inventory.enabled)
		{
			Inventory.enabled = false;
		}
		Player.SetWeapon.Argument = 0;
		Player.SetWeapon.Start();
		Player.Dead.Start();
		Player.Run.Stop();
		Player.Jump.Stop();
		Player.Crouch.Stop();
		Player.Zoom.Stop();
		Player.Attack.Stop();
		Player.Reload.Stop();
		Player.Climb.Stop();
		Player.Interact.Stop();
		if (vp_Gameplay.isMultiplayer && vp_Gameplay.isMaster)
		{
			vp_GlobalEvent<Transform>.Send("Kill", base.transform.root);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Reset()
	{
		base.Reset();
		if (!Application.isPlaying)
		{
			return;
		}
		Player.Dead.Stop();
		Player.Stop.Send();
		foreach (Collider collider in Colliders)
		{
			collider.enabled = true;
		}
		if (Inventory != null && !Inventory.enabled)
		{
			Inventory.enabled = m_InventoryWasEnabledAtStart;
		}
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(RespawnSound);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		if (!Player.Dead.Active && AllowFallDamage && !(impact <= FallImpactThreshold))
		{
			vp_AudioUtility.PlayRandomSound(m_Audio, FallImpactSounds, FallImpactPitch);
			float damage = Mathf.Abs(DeathOnFallImpactThreshold ? MaxHealth : (MaxHealth * impact));
			Damage(damage);
		}
	}
}
