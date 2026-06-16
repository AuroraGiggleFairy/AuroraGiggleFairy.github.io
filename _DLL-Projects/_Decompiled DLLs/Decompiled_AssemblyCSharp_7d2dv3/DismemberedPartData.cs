using UnityEngine;

public class DismemberedPartData
{
	public string propertyKey;

	public string prefabPath;

	public string targetBone;

	public string damageTypeKey;

	public bool isDetachable;

	public Vector3 pos;

	public Vector3 scale;

	public Vector3 offset;

	public bool attachToParent;

	public string[] particlePaths;

	public bool useMask;

	public bool isLinked;

	public bool scaleOutLimb;

	public string solTarget;

	public Vector3 solScale;

	public bool hasSolScale;

	public string childTargetObj;

	public string insertBoneObj;

	public string addScalePoint;

	public string maskScaleBlend;

	public string setFixedValues;

	public string dismemberMatPath;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rot
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool hasRotOffset
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Invalid { get; set; }

	public void SetRot(Vector3 _rot)
	{
		hasRotOffset = true;
		rot = _rot;
	}

	public string Log()
	{
		return $" property: {propertyKey} prefabPath: {prefabPath} target: {targetBone} damageTag {damageTypeKey}";
	}
}
