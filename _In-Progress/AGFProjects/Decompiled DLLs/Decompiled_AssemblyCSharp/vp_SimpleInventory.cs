using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vp_SimpleInventory : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class InventoryWeaponStatusComparer : IComparer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		int IComparer.Compare(object x, object y)
		{
			return new CaseInsensitiveComparer().Compare(((InventoryWeaponStatus)x).Name, ((InventoryWeaponStatus)y).Name);
		}
	}

	[Serializable]
	public class InventoryItemStatus
	{
		public string Name = "Unnamed";

		public int Have;

		public int CanHave = 1;

		public bool ClearOnDeath = true;
	}

	[Serializable]
	public class InventoryWeaponStatus : InventoryItemStatus
	{
		public string ClipType = "";

		public int LoadedAmmo;

		public int MaxAmmo = 10;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<InventoryItemStatus> m_ItemTypes;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<InventoryWeaponStatus> m_WeaponTypes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, InventoryItemStatus> m_ItemStatusDictionary;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public InventoryWeaponStatus m_CurrentWeaponStatus;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_RefreshWeaponStatusIterations;

	public InventoryWeaponStatus CurrentWeaponStatus
	{
		get
		{
			return m_CurrentWeaponStatus;
		}
		set
		{
			m_CurrentWeaponStatus = value;
		}
	}

	public List<InventoryItemStatus> Weapons
	{
		get
		{
			List<InventoryItemStatus> list = new List<InventoryItemStatus>();
			foreach (InventoryWeaponStatus weaponType in m_WeaponTypes)
			{
				list.Add(weaponType);
			}
			return list;
		}
	}

	public List<InventoryItemStatus> EquippedWeapons
	{
		get
		{
			List<InventoryItemStatus> list = new List<InventoryItemStatus>();
			foreach (InventoryItemStatus value in m_ItemStatusDictionary.Values)
			{
				if (value.GetType() == typeof(InventoryWeaponStatus) && value.Have == 1)
				{
					list.Add(value);
				}
			}
			return list;
		}
	}

	public Dictionary<string, InventoryItemStatus> ItemStatusDictionary
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_ItemStatusDictionary == null)
			{
				m_ItemStatusDictionary = new Dictionary<string, InventoryItemStatus>();
				for (int num = m_ItemTypes.Count - 1; num > -1; num--)
				{
					if (!m_ItemStatusDictionary.ContainsKey(m_ItemTypes[num].Name))
					{
						m_ItemStatusDictionary.Add(m_ItemTypes[num].Name, m_ItemTypes[num]);
					}
					else
					{
						m_ItemTypes.Remove(m_ItemTypes[num]);
					}
				}
				for (int num2 = m_WeaponTypes.Count - 1; num2 > -1; num2--)
				{
					if (!m_ItemStatusDictionary.ContainsKey(m_WeaponTypes[num2].Name))
					{
						m_ItemStatusDictionary.Add(m_WeaponTypes[num2].Name, m_WeaponTypes[num2]);
					}
					else
					{
						m_WeaponTypes.Remove(m_WeaponTypes[num2]);
					}
				}
			}
			return m_ItemStatusDictionary;
		}
	}

	public virtual int OnValue_CurrentWeaponAmmoCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_CurrentWeaponStatus == null)
			{
				return 0;
			}
			return m_CurrentWeaponStatus.LoadedAmmo;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (m_CurrentWeaponStatus != null)
			{
				m_CurrentWeaponStatus.LoadedAmmo = value;
			}
		}
	}

	public virtual int OnValue_CurrentWeaponClipCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_CurrentWeaponStatus == null)
			{
				return 0;
			}
			if (!ItemStatusDictionary.TryGetValue(m_CurrentWeaponStatus.ClipType, out var value))
			{
				return 0;
			}
			return value.Have;
		}
	}

	public virtual string OnValue_CurrentWeaponClipType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_CurrentWeaponStatus == null)
			{
				return "";
			}
			return m_CurrentWeaponStatus.ClipType;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Player = (vp_FPPlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));
		IComparer comparer = new InventoryWeaponStatusComparer();
		m_WeaponTypes.Sort(comparer.Compare);
	}

	public bool HaveItem(object name)
	{
		if (!ItemStatusDictionary.TryGetValue((string)name, out var value))
		{
			return false;
		}
		if (value.Have < 1)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InventoryItemStatus GetItemStatus(string name)
	{
		if (!ItemStatusDictionary.TryGetValue(name, out var value))
		{
			Debug.LogError("Error: (" + this?.ToString() + "). Unknown item type: '" + name + "'.");
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InventoryWeaponStatus GetWeaponStatus(string name)
	{
		if (name == null)
		{
			return null;
		}
		if (!ItemStatusDictionary.TryGetValue(name, out var value))
		{
			Debug.LogError("Error: (" + this?.ToString() + "). Unknown item type: '" + name + "'.");
			return null;
		}
		if (value.GetType() != typeof(InventoryWeaponStatus))
		{
			Debug.LogError("Error: (" + this?.ToString() + "). Item is not a weapon: '" + name + "'.");
			return null;
		}
		return (InventoryWeaponStatus)value;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RefreshWeaponStatus()
	{
		if (!m_Player.CurrentWeaponWielded.Get() && m_RefreshWeaponStatusIterations < 50)
		{
			m_RefreshWeaponStatusIterations++;
			vp_Timer.In(0.1f, RefreshWeaponStatus);
			return;
		}
		m_RefreshWeaponStatusIterations = 0;
		string value = m_Player.CurrentWeaponName.Get();
		if (!string.IsNullOrEmpty(value))
		{
			m_CurrentWeaponStatus = GetWeaponStatus(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int OnMessage_GetItemCount(string name)
	{
		if (!ItemStatusDictionary.TryGetValue(name, out var value))
		{
			return 0;
		}
		return value.Have;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_DepleteAmmo()
	{
		if (m_CurrentWeaponStatus == null)
		{
			return false;
		}
		if (m_CurrentWeaponStatus.LoadedAmmo < 1)
		{
			if (m_CurrentWeaponStatus.MaxAmmo == 0)
			{
				return true;
			}
			return false;
		}
		m_CurrentWeaponStatus.LoadedAmmo--;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_AddAmmo(object arg)
	{
		object[] array = (object[])arg;
		string text = (string)array[0];
		int num = ((array.Length == 2) ? ((int)array[1]) : (-1));
		InventoryWeaponStatus weaponStatus = GetWeaponStatus(text);
		if (weaponStatus == null)
		{
			return false;
		}
		if (num == -1)
		{
			weaponStatus.LoadedAmmo = weaponStatus.MaxAmmo;
		}
		else
		{
			weaponStatus.LoadedAmmo = Mathf.Min(weaponStatus.LoadedAmmo + num, weaponStatus.MaxAmmo);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_AddItem(object args)
	{
		object[] array = (object[])args;
		string text = (string)array[0];
		int num = ((array.Length != 2) ? 1 : ((int)array[1]));
		InventoryItemStatus itemStatus = GetItemStatus(text);
		if (itemStatus == null)
		{
			return false;
		}
		itemStatus.CanHave = Mathf.Max(1, itemStatus.CanHave);
		if (itemStatus.Have >= itemStatus.CanHave)
		{
			return false;
		}
		itemStatus.Have = Mathf.Min(itemStatus.Have + num, itemStatus.CanHave);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_RemoveItem(object args)
	{
		object[] array = (object[])args;
		string text = (string)array[0];
		int num = ((array.Length != 2) ? 1 : ((int)array[1]));
		InventoryItemStatus itemStatus = GetItemStatus(text);
		if (itemStatus == null)
		{
			return false;
		}
		if (itemStatus.Have <= 0)
		{
			return false;
		}
		itemStatus.Have = Mathf.Max(itemStatus.Have - num, 0);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_RemoveClip()
	{
		if (m_CurrentWeaponStatus == null)
		{
			return false;
		}
		if (GetItemStatus(m_CurrentWeaponStatus.ClipType) == null)
		{
			return false;
		}
		if (m_CurrentWeaponStatus.LoadedAmmo >= m_CurrentWeaponStatus.MaxAmmo)
		{
			return false;
		}
		if (!m_Player.RemoveItem.Try(new object[1] { m_CurrentWeaponStatus.ClipType }))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_SetWeapon()
	{
		int num = (int)m_Player.SetWeapon.Argument;
		if (num == 0)
		{
			return true;
		}
		if (num < 0 || num > m_WeaponTypes.Count)
		{
			return false;
		}
		return HaveItem(m_WeaponTypes[num - 1].Name);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		RefreshWeaponStatus();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (m_ItemStatusDictionary == null)
		{
			return;
		}
		foreach (InventoryItemStatus value in m_ItemStatusDictionary.Values)
		{
			if (value.ClearOnDeath)
			{
				value.Have = 0;
				if (value.GetType() == typeof(InventoryWeaponStatus))
				{
					((InventoryWeaponStatus)value).LoadedAmmo = 0;
				}
			}
		}
	}
}
