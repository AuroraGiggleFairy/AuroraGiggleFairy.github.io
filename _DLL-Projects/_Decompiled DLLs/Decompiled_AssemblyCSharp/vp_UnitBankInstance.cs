using System;
using UnityEngine;

[Serializable]
public class vp_UnitBankInstance : vp_ItemInstance
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int UNLIMITED = -1;

	[SerializeField]
	public vp_UnitType UnitType;

	[SerializeField]
	public int Count;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_Capacity = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Inventory m_Inventory;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Result;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_PrevCount;

	public int Capacity
	{
		get
		{
			if (Type != null)
			{
				m_Capacity = ((vp_UnitBankType)Type).Capacity;
			}
			return m_Capacity;
		}
		set
		{
			m_Capacity = Mathf.Max(-1, value);
		}
	}

	public virtual bool IsInternal => Type == null;

	[SerializeField]
	public vp_UnitBankInstance(vp_UnitBankType unitBankType, int id, vp_Inventory inventory)
		: base(unitBankType, id)
	{
		UnitType = unitBankType.Unit;
		m_Inventory = inventory;
	}

	[SerializeField]
	public vp_UnitBankInstance(vp_UnitType unitType, vp_Inventory inventory)
		: base(null, 0)
	{
		UnitType = unitType;
		m_Inventory = inventory;
	}

	public virtual bool TryRemoveUnits(int amount)
	{
		if (Count <= 0)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (amount == 0)
		{
			return false;
		}
		Count = Mathf.Max(0, Count - amount);
		return true;
	}

	public virtual bool TryGiveUnits(int amount)
	{
		if (Type != null && !((vp_UnitBankType)Type).Reloadable)
		{
			return false;
		}
		if (Capacity != -1 && Count >= Capacity)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (amount == 0)
		{
			return false;
		}
		Count += amount;
		if (Count <= Capacity)
		{
			return true;
		}
		if (Capacity == -1)
		{
			return true;
		}
		Count = Capacity;
		return true;
	}

	public virtual bool DoAddUnits(int amount)
	{
		m_PrevCount = Count;
		m_Result = TryGiveUnits(amount);
		if (m_Inventory.SpaceEnabled && m_Result && !IsInternal && m_Inventory.SpaceMode == vp_Inventory.Mode.Weight)
		{
			m_Inventory.UsedSpace += (float)(Count - m_PrevCount) * UnitType.Space;
		}
		m_Inventory.SetDirty();
		return m_Result;
	}

	public virtual bool DoRemoveUnits(int amount)
	{
		m_PrevCount = Count;
		m_Result = TryRemoveUnits(amount);
		if (m_Inventory.SpaceEnabled && m_Result && !IsInternal && m_Inventory.SpaceMode == vp_Inventory.Mode.Weight)
		{
			m_Inventory.UsedSpace = Mathf.Max(0f, m_Inventory.UsedSpace - (float)(m_PrevCount - Count) * UnitType.Space);
		}
		m_Inventory.SetDirty();
		return m_Result;
	}

	public virtual int ClampToCapacity()
	{
		int count = Count;
		if (Capacity != -1)
		{
			Count = Mathf.Clamp(Count, 0, Capacity);
		}
		Count = Mathf.Max(0, Count);
		return count - Count;
	}
}
