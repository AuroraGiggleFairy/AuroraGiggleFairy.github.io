using UnityEngine;

public class DismembermentPart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DismemberedPartData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject obj;

	public DismemberedPartData Data => data;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform objT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform targetT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public uint bodyDamageFlag
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageTypes damageType
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string prefabPath
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool useMask => data.useMask;

	public DismembermentPart(DismemberedPartData _data, uint _bodyDamageFlag, EnumDamageTypes _damageType)
	{
		data = _data;
		prefabPath = data.prefabPath;
		bodyDamageFlag = _bodyDamageFlag;
		damageType = _damageType;
	}

	public void SetObj(Transform _t)
	{
		objT = _t;
		obj = _t.gameObject;
	}

	public void SetTarget(Transform _t)
	{
		targetT = _t;
	}

	public void Hide()
	{
		obj.SetActive(value: false);
	}

	public void LateUpdate()
	{
		if ((bool)objT && (bool)targetT && data.updateWithAnim)
		{
			if (!data.attachToParent)
			{
				objT.position = targetT.position;
				objT.rotation = targetT.rotation;
			}
			else
			{
				Transform parent = targetT.parent;
				objT.position = parent.position;
				objT.rotation = parent.rotation;
			}
		}
	}
}
