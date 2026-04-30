using System;
using UnityEngine;

[Serializable]
public class vp_ItemInstance
{
	[SerializeField]
	public vp_ItemType Type;

	[SerializeField]
	public int ID;

	[SerializeField]
	public vp_ItemInstance(vp_ItemType type, int id)
	{
		ID = id;
		Type = type;
	}

	public virtual void SetUniqueID()
	{
		ID = vp_Utility.UniqueID;
	}
}
