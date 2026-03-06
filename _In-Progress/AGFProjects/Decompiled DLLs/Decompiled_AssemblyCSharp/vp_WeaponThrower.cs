using System;
using UnityEngine;

public class vp_WeaponThrower : MonoBehaviour
{
	public float AttackMinDuration = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_OriginalAttackMinDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Root;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Weapon m_Weapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_WeaponShooter m_Shooter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_UnitBankType m_UnitBankType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_UnitBankInstance m_UnitBank;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemIdentifier m_ItemIdentifier;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_PlayerInventory m_Inventory;

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

	public Transform Root
	{
		get
		{
			if (m_Root == null)
			{
				m_Root = Transform.root;
			}
			return m_Root;
		}
	}

	public vp_Weapon Weapon
	{
		get
		{
			if (m_Weapon == null)
			{
				m_Weapon = (vp_Weapon)Transform.GetComponent(typeof(vp_Weapon));
			}
			return m_Weapon;
		}
	}

	public vp_WeaponShooter Shooter
	{
		get
		{
			if (m_Shooter == null)
			{
				m_Shooter = (vp_WeaponShooter)Transform.GetComponent(typeof(vp_WeaponShooter));
			}
			return m_Shooter;
		}
	}

	public vp_UnitBankType UnitBankType
	{
		get
		{
			if (ItemIdentifier == null)
			{
				return null;
			}
			vp_ItemType itemType = m_ItemIdentifier.GetItemType();
			if (itemType == null)
			{
				return null;
			}
			vp_UnitBankType vp_UnitBankType2 = itemType as vp_UnitBankType;
			if (vp_UnitBankType2 == null)
			{
				return null;
			}
			return vp_UnitBankType2;
		}
	}

	public vp_UnitBankInstance UnitBank
	{
		get
		{
			if (m_UnitBank == null && UnitBankType != null && Inventory != null)
			{
				foreach (vp_UnitBankInstance unitBankInstance in Inventory.UnitBankInstances)
				{
					if (unitBankInstance.UnitType == UnitBankType.Unit)
					{
						m_UnitBank = unitBankInstance;
					}
				}
			}
			return m_UnitBank;
		}
	}

	public vp_ItemIdentifier ItemIdentifier
	{
		get
		{
			if (m_ItemIdentifier == null)
			{
				m_ItemIdentifier = (vp_ItemIdentifier)Transform.GetComponent(typeof(vp_ItemIdentifier));
			}
			return m_ItemIdentifier;
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

	public vp_PlayerInventory Inventory
	{
		get
		{
			if (m_Inventory == null)
			{
				m_Inventory = (vp_PlayerInventory)Root.GetComponentInChildren(typeof(vp_PlayerInventory));
			}
			return m_Inventory;
		}
	}

	public bool HaveAmmoForCurrentWeapon
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (Player.CurrentWeaponAmmoCount.Get() <= 0)
			{
				return Player.CurrentWeaponClipCount.Get() > 0;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (!(Player == null))
		{
			Player.Register(this);
			TryStoreAttackMinDuration();
			Inventory.SetItemCap(ItemIdentifier.Type, 1, clamp: true);
			Inventory.CapsEnabled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		TryRestoreAttackMinDuration();
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		TryStoreAttackMinDuration();
		if (Weapon == null)
		{
			Debug.LogError("Throwing weapon setup error (" + this?.ToString() + ") requires a vp_Weapon or vp_FPWeapon component.");
			return;
		}
		if (UnitBankType == null)
		{
			Debug.LogError("Throwing weapon setup error (" + this?.ToString() + ") requires a vp_ItemIdentifier component with a valid UnitBank.");
			return;
		}
		if (Weapon.AnimationType != 3)
		{
			Debug.LogError("Throwing weapon setup error (" + this?.ToString() + ") Please set 'Animation -> Type' of '" + Weapon?.ToString() + "' item type to 'Thrown'.");
		}
		if (UnitBankType.Capacity != 1)
		{
			Debug.LogError("Throwing weapon setup error (" + this?.ToString() + ") Please set 'Capacity' for the '" + UnitBankType.name + "' item type to '1'.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryStoreAttackMinDuration()
	{
		if (Player.Attack != null && m_OriginalAttackMinDuration != 0f)
		{
			m_OriginalAttackMinDuration = Player.Attack.MinDuration;
			Player.Attack.MinDuration = AttackMinDuration;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryRestoreAttackMinDuration()
	{
		if (Player.Attack != null && m_OriginalAttackMinDuration == 0f)
		{
			Player.Attack.MinDuration = m_OriginalAttackMinDuration;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool TryReload()
	{
		if (UnitBank == null)
		{
			return false;
		}
		return Inventory.TryReload(UnitBank);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Attack()
	{
		if (!Player.IsFirstPerson.Get())
		{
			vp_Timer.In(Shooter.ProjectileSpawnDelay, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Weapon.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = false;
			});
			vp_Timer.In(Shooter.ProjectileSpawnDelay + 1f, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				if (HaveAmmoForCurrentWeapon)
				{
					Weapon.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = true;
				}
			});
		}
		if (Player.CurrentWeaponAmmoCount.Get() < 1)
		{
			TryReload();
		}
		vp_Timer.In(Shooter.ProjectileSpawnDelay + 0.5f, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Player.Attack.Stop();
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Reload()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Attack()
	{
		TryReload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		m_UnitBank = null;
	}
}
