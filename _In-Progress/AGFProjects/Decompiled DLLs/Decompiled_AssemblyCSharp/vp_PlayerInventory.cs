using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PlayerInventory : vp_Inventory
{
	[Serializable]
	public class AutoWieldSection
	{
		public bool Always;

		public bool IfUnarmed = true;

		public bool IfOutOfAmmo = true;

		public bool IfNotPresent = true;

		public bool FirstTimeOnly = true;
	}

	[Serializable]
	public class MiscSection
	{
		public bool ResetOnRespawn = true;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_ItemType, object> m_PreviouslyOwnedItems = new Dictionary<vp_ItemType, object>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemIdentifier m_WeaponIdentifierResult;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string m_MissingHandlerError = "Error (vp_PlayerInventory) this component must be on the same transform as a vp_PlayerEventHandler + vp_WeaponHandler.";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_UnitBankInstance, vp_Weapon> m_UnitBankWeapons;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_ItemInstance, vp_Weapon> m_ItemWeapons;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_Weapon, vp_ItemIdentifier> m_WeaponIdentifiers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<vp_UnitType, List<vp_Weapon>> m_WeaponsByUnit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemInstance m_CurrentWeaponInstance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_PlayerEventHandler m_Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_WeaponHandler m_WeaponHandler;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AutoWieldSection m_AutoWield;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public MiscSection m_Misc;

	public Dictionary<vp_Weapon, vp_ItemIdentifier> WeaponIdentifiers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_WeaponIdentifiers == null)
			{
				m_WeaponIdentifiers = new Dictionary<vp_Weapon, vp_ItemIdentifier>();
				foreach (vp_Weapon weapon in WeaponHandler.Weapons)
				{
					vp_ItemIdentifier component = weapon.GetComponent<vp_ItemIdentifier>();
					if (component != null)
					{
						m_WeaponIdentifiers.Add(weapon, component);
					}
				}
			}
			return m_WeaponIdentifiers;
		}
	}

	public Dictionary<vp_UnitType, List<vp_Weapon>> WeaponsByUnit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_WeaponsByUnit == null)
			{
				m_WeaponsByUnit = new Dictionary<vp_UnitType, List<vp_Weapon>>();
				foreach (vp_Weapon weapon in WeaponHandler.Weapons)
				{
					if (!WeaponIdentifiers.TryGetValue(weapon, out var value) || !(value != null))
					{
						continue;
					}
					vp_UnitBankType vp_UnitBankType2 = value.Type as vp_UnitBankType;
					if (!(vp_UnitBankType2 != null) || !(vp_UnitBankType2.Unit != null))
					{
						continue;
					}
					if (m_WeaponsByUnit.TryGetValue(vp_UnitBankType2.Unit, out var value2))
					{
						if (value2 == null)
						{
							value2 = new List<vp_Weapon>();
						}
						m_WeaponsByUnit.Remove(vp_UnitBankType2.Unit);
					}
					else
					{
						value2 = new List<vp_Weapon>();
					}
					value2.Add(weapon);
					m_WeaponsByUnit.Add(vp_UnitBankType2.Unit, value2);
				}
			}
			return m_WeaponsByUnit;
		}
	}

	public virtual vp_ItemInstance CurrentWeaponInstance
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (Application.isPlaying && WeaponHandler.CurrentWeaponIndex == 0)
			{
				m_CurrentWeaponInstance = null;
				return null;
			}
			if (m_CurrentWeaponInstance == null)
			{
				if (CurrentWeaponIdentifier == null)
				{
					MissingIdentifierError();
					m_CurrentWeaponInstance = null;
					return null;
				}
				m_CurrentWeaponInstance = GetItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID);
			}
			return m_CurrentWeaponInstance;
		}
	}

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

	public vp_WeaponHandler WeaponHandler
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_WeaponHandler == null)
			{
				m_WeaponHandler = base.transform.GetComponent<vp_WeaponHandler>();
			}
			return m_WeaponHandler;
		}
	}

	public vp_ItemIdentifier CurrentWeaponIdentifier
	{
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			return GetWeaponIdentifier(WeaponHandler.CurrentWeapon);
		}
	}

	public virtual int OnValue_CurrentWeaponAmmoCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return vp_UnitBankInstance2.Count;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2)
			{
				vp_UnitBankInstance2.TryGiveUnits(value);
			}
		}
	}

	public virtual int OnValue_CurrentWeaponMaxAmmoCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return vp_UnitBankInstance2.Capacity;
		}
	}

	public virtual int OnValue_CurrentWeaponClipCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2))
			{
				return 0;
			}
			return GetUnitCount(vp_UnitBankInstance2.UnitType);
		}
	}

	public virtual Texture2D OnValue_CurrentAmmoIcon
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (CurrentWeaponInstance == null)
			{
				return null;
			}
			if (CurrentWeaponInstance.Type == null)
			{
				return null;
			}
			vp_UnitBankType vp_UnitBankType2 = CurrentWeaponInstance.Type as vp_UnitBankType;
			if (vp_UnitBankType2 == null)
			{
				return null;
			}
			if (vp_UnitBankType2.Unit == null)
			{
				return null;
			}
			return vp_UnitBankType2.Unit.Icon;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual vp_ItemIdentifier GetWeaponIdentifier(vp_Weapon weapon)
	{
		if (!Application.isPlaying)
		{
			return null;
		}
		if (weapon == null)
		{
			return null;
		}
		if (!WeaponIdentifiers.TryGetValue(weapon, out m_WeaponIdentifierResult))
		{
			if (weapon == null)
			{
				return null;
			}
			m_WeaponIdentifierResult = weapon.GetComponent<vp_ItemIdentifier>();
			if (m_WeaponIdentifierResult == null)
			{
				return null;
			}
			if (m_WeaponIdentifierResult.Type == null)
			{
				return null;
			}
			WeaponIdentifiers.Add(weapon, m_WeaponIdentifierResult);
		}
		return m_WeaponIdentifierResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (Player == null || WeaponHandler == null)
		{
			Debug.LogError(m_MissingHandlerError);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		if (Player != null)
		{
			Player.Register(this);
		}
		UnwieldMissingWeapon();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool MissingIdentifierError(int weaponIndex = 0)
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		if (weaponIndex < 1)
		{
			return false;
		}
		if (WeaponHandler == null)
		{
			return false;
		}
		if (WeaponHandler.Weapons.Count <= weaponIndex - 1)
		{
			return false;
		}
		Debug.LogWarning(string.Format("Warning: Weapon gameobject '" + WeaponHandler.Weapons[weaponIndex - 1].name + "' lacks a properly set up vp_ItemIdentifier component!"));
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DoAddItem(vp_ItemType type, int id)
	{
		bool alreadyHaveIt = (vp_Gameplay.isMultiplayer ? HaveItem(type) : HaveItem(type, id));
		base.DoAddItem(type, id);
		TryWieldNewItem(type, alreadyHaveIt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DoRemoveItem(vp_ItemInstance item)
	{
		Unwield(item);
		base.DoRemoveItem(item);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DoAddUnitBank(vp_UnitBankType unitBankType, int id, int unitsLoaded)
	{
		bool alreadyHaveIt = (vp_Gameplay.isMultiplayer ? HaveItem(unitBankType) : HaveItem(unitBankType, id));
		base.DoAddUnitBank(unitBankType, id, unitsLoaded);
		TryWieldNewItem(unitBankType, alreadyHaveIt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TryWieldNewItem(vp_ItemType type, bool alreadyHaveIt)
	{
		bool flag = m_PreviouslyOwnedItems.ContainsKey(type);
		if (!flag)
		{
			m_PreviouslyOwnedItems.Add(type, null);
		}
		if (!m_AutoWield.Always && (!m_AutoWield.IfUnarmed || WeaponHandler.CurrentWeaponIndex >= 1) && (!m_AutoWield.IfOutOfAmmo || WeaponHandler.CurrentWeaponIndex <= 0 || WeaponHandler.CurrentWeapon.AnimationType == 2 || m_Player.CurrentWeaponAmmoCount.Get() >= 1) && (!m_AutoWield.IfNotPresent || m_AutoWield.FirstTimeOnly || alreadyHaveIt) && (!m_AutoWield.FirstTimeOnly || flag))
		{
			return;
		}
		if (type is vp_UnitBankType)
		{
			TryWield(GetItem(type));
			return;
		}
		if (type is vp_UnitType)
		{
			TryWieldByUnit(type as vp_UnitType);
			return;
		}
		if ((object)type != null)
		{
			TryWield(GetItem(type));
			return;
		}
		Type type2 = type.GetType();
		if (!(type2 == null))
		{
			type2 = type2.BaseType;
			if (type2 == typeof(vp_UnitBankType))
			{
				TryWield(GetItem(type));
			}
			else if (type2 == typeof(vp_UnitType))
			{
				TryWieldByUnit(type as vp_UnitType);
			}
			else if (type2 == typeof(vp_ItemType))
			{
				TryWield(GetItem(type));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DoRemoveUnitBank(vp_UnitBankInstance bank)
	{
		Unwield(bank);
		base.DoRemoveUnitBank(bank);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual vp_Weapon GetWeaponOfItemInstance(vp_ItemInstance itemInstance)
	{
		if (m_ItemWeapons == null)
		{
			m_ItemWeapons = new Dictionary<vp_ItemInstance, vp_Weapon>();
		}
		m_ItemWeapons.TryGetValue(itemInstance, out var value);
		if (value != null)
		{
			return value;
		}
		try
		{
			for (int i = 0; i < WeaponHandler.Weapons.Count; i++)
			{
				vp_ItemInstance itemInstanceOfWeapon = GetItemInstanceOfWeapon(WeaponHandler.Weapons[i]);
				Debug.Log("weapon with index: " + i + ", item instance: " + ((itemInstanceOfWeapon == null) ? "(have none)" : itemInstanceOfWeapon.Type.ToString()));
				if (itemInstanceOfWeapon != null && itemInstanceOfWeapon.Type == itemInstance.Type)
				{
					value = WeaponHandler.Weapons[i];
					m_ItemWeapons.Add(itemInstanceOfWeapon, value);
					return value;
				}
			}
		}
		catch
		{
			Debug.LogError("Exception " + this?.ToString() + " Crashed while trying to get item instance for a weapon. Likely a nullreference.");
		}
		return null;
	}

	public override bool DoAddUnits(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null)
		{
			return false;
		}
		int unitCount = GetUnitCount(bank.UnitType);
		bool flag = base.DoAddUnits(bank, amount);
		if (flag && bank.IsInternal)
		{
			try
			{
				TryWieldNewItem(bank.UnitType, unitCount != 0);
			}
			catch
			{
			}
			if ((!Application.isPlaying || WeaponHandler.CurrentWeaponIndex != 0) && CurrentWeaponInstance is vp_UnitBankInstance vp_UnitBankInstance2 && bank.UnitType == vp_UnitBankInstance2.UnitType && vp_UnitBankInstance2.Count == 0)
			{
				Player.AutoReload.Try();
			}
		}
		return flag;
	}

	public override bool DoRemoveUnits(vp_UnitBankInstance bank, int amount)
	{
		bool result = base.DoRemoveUnits(bank, amount);
		if (bank.Count == 0)
		{
			vp_Timer.In(0.3f, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Player.AutoReload.Try();
			});
		}
		return result;
	}

	public vp_UnitBankInstance GetUnitBankInstanceOfWeapon(vp_Weapon weapon)
	{
		return GetItemInstanceOfWeapon(weapon) as vp_UnitBankInstance;
	}

	public vp_ItemInstance GetItemInstanceOfWeapon(vp_Weapon weapon)
	{
		vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(weapon);
		if (weaponIdentifier == null)
		{
			return null;
		}
		return GetItem(weaponIdentifier.Type);
	}

	public int GetAmmoInWeapon(vp_Weapon weapon)
	{
		return GetUnitBankInstanceOfWeapon(weapon)?.Count ?? 0;
	}

	public int GetExtraAmmoForWeapon(vp_Weapon weapon)
	{
		vp_UnitBankInstance unitBankInstanceOfWeapon = GetUnitBankInstanceOfWeapon(weapon);
		if (unitBankInstanceOfWeapon == null)
		{
			return 0;
		}
		return GetUnitCount(unitBankInstanceOfWeapon.UnitType);
	}

	public int GetAmmoInCurrentWeapon()
	{
		return OnValue_CurrentWeaponAmmoCount;
	}

	public int GetExtraAmmoForCurrentWeapon()
	{
		return OnValue_CurrentWeaponClipCount;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UnwieldMissingWeapon()
	{
		if (Application.isPlaying && WeaponHandler.CurrentWeaponIndex >= 1 && (!(CurrentWeaponIdentifier != null) || !HaveItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID)))
		{
			if (CurrentWeaponIdentifier == null)
			{
				MissingIdentifierError(WeaponHandler.CurrentWeaponIndex);
			}
			Player.SetWeapon.TryStart(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool TryWieldByUnit(vp_UnitType unitType)
	{
		if (WeaponsByUnit.TryGetValue(unitType, out var value) && value != null && value.Count > 0)
		{
			foreach (vp_Weapon item in value)
			{
				if (m_Player.SetWeapon.TryStart(WeaponHandler.Weapons.IndexOf(item) + 1))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TryWield(vp_ItemInstance item)
	{
		if (!Application.isPlaying || Player.Dead.Active || !WeaponHandler.enabled)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			if (num < WeaponHandler.Weapons.Count + 1)
			{
				vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(WeaponHandler.Weapons[num - 1]);
				if (!(weaponIdentifier == null) && !(item.Type != weaponIdentifier.Type) && (weaponIdentifier.ID == 0 || item.ID == weaponIdentifier.ID))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		Player.SetWeapon.TryStart(num);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Unwield(vp_ItemInstance item)
	{
		if (!Application.isPlaying || WeaponHandler.CurrentWeaponIndex == 0)
		{
			return;
		}
		if (CurrentWeaponIdentifier == null)
		{
			MissingIdentifierError();
		}
		else
		{
			if (item.Type != CurrentWeaponIdentifier.Type || (CurrentWeaponIdentifier.ID != 0 && item.ID != CurrentWeaponIdentifier.ID))
			{
				return;
			}
			Player.SetWeapon.Start();
			if (!Player.Dead.Active)
			{
				vp_Timer.In(0.35f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					Player.SetNextWeapon.Try();
				});
			}
			vp_Timer.In(1f, UnwieldMissingWeapon);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		UnwieldMissingWeapon();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_SetWeapon()
	{
		int num = (int)Player.SetWeapon.Argument;
		if (num == 0)
		{
			return true;
		}
		if (num < 1 || num > WeaponHandler.Weapons.Count)
		{
			return false;
		}
		vp_ItemIdentifier weaponIdentifier = GetWeaponIdentifier(WeaponHandler.Weapons[num - 1]);
		if (weaponIdentifier == null)
		{
			return MissingIdentifierError(num);
		}
		bool flag = HaveItem(weaponIdentifier.Type, weaponIdentifier.ID);
		if (flag && WeaponHandler.Weapons[num - 1].AnimationType == 3 && GetAmmoInWeapon(WeaponHandler.Weapons[num - 1]) < 1)
		{
			vp_UnitBankType vp_UnitBankType2 = weaponIdentifier.Type as vp_UnitBankType;
			if (vp_UnitBankType2 == null)
			{
				Debug.LogError("Error (" + this?.ToString() + ") Tried to wield thrown weapon " + WeaponHandler.Weapons[num - 1]?.ToString() + " but its item identifier does not point to a UnitBank.");
				return false;
			}
			if (!TryReload(vp_UnitBankType2, weaponIdentifier.ID))
			{
				return false;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_DepleteAmmo()
	{
		if (CurrentWeaponIdentifier == null)
		{
			return MissingIdentifierError();
		}
		if (WeaponHandler.CurrentWeapon.AnimationType == 3)
		{
			TryReload(CurrentWeaponInstance as vp_UnitBankInstance);
		}
		return TryDeduct(CurrentWeaponIdentifier.Type as vp_UnitBankType, CurrentWeaponIdentifier.ID, 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_RefillCurrentWeapon()
	{
		if (CurrentWeaponIdentifier == null)
		{
			return MissingIdentifierError();
		}
		return TryReload(CurrentWeaponIdentifier.Type as vp_UnitBankType, CurrentWeaponIdentifier.ID);
	}

	public override void Reset()
	{
		m_PreviouslyOwnedItems.Clear();
		m_CurrentWeaponInstance = null;
		if (m_Misc.ResetOnRespawn)
		{
			base.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int OnMessage_GetItemCount(string itemTypeObjectName)
	{
		vp_ItemInstance item = GetItem(itemTypeObjectName);
		if (item == null)
		{
			return 0;
		}
		if (item is vp_UnitBankInstance { IsInternal: not false } vp_UnitBankInstance2)
		{
			return GetItemCount(vp_UnitBankInstance2.UnitType);
		}
		return GetItemCount(item.Type);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_AddItem(object args)
	{
		object[] array = (object[])args;
		vp_ItemType vp_ItemType2 = array[0] as vp_ItemType;
		if (vp_ItemType2 == null)
		{
			return false;
		}
		int amount = ((array.Length != 2) ? 1 : ((int)array[1]));
		if (vp_ItemType2 is vp_UnitType)
		{
			return TryGiveUnits(vp_ItemType2 as vp_UnitType, amount);
		}
		return TryGiveItems(vp_ItemType2, amount);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_RemoveItem(object args)
	{
		object[] array = (object[])args;
		vp_ItemType vp_ItemType2 = array[0] as vp_ItemType;
		if (vp_ItemType2 == null)
		{
			return false;
		}
		int amount = ((array.Length != 2) ? 1 : ((int)array[1]));
		if (vp_ItemType2 is vp_UnitType)
		{
			return TryRemoveUnits(vp_ItemType2 as vp_UnitType, amount);
		}
		return TryRemoveItems(vp_ItemType2, amount);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		m_CurrentWeaponInstance = null;
	}
}
