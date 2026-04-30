using System;
using UnityEngine;

[Serializable]
public class vp_UnitBankType : vp_ItemType
{
	[SerializeField]
	public vp_UnitType Unit;

	[SerializeField]
	public int Capacity = 10;

	[SerializeField]
	public bool Reloadable = true;

	[SerializeField]
	public bool RemoveWhenDepleted;
}
