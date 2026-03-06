using UnityEngine;

public class vp_ItemIdentifier : MonoBehaviour
{
	public vp_ItemType Type;

	public int ID;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_TargetEventReturn<vp_ItemType>.Register(base.transform, "GetItemType", GetItemType);
		vp_TargetEventReturn<int>.Register(base.transform, "GetItemID", GetItemID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
	}

	public virtual vp_ItemType GetItemType()
	{
		return Type;
	}

	public virtual int GetItemID()
	{
		return ID;
	}
}
