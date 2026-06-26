using UnityEngine;

public class DismemberedPartData
{
	public string propertyKey;

	public string prefabPath;

	public string targetBone;

	public string damageTypeKey;

	public bool isDetachable;

	public Vector3 scale;

	public Vector3 offset;

	public bool attachToParent;

	public bool alignToBone;

	public bool snapToChild;

	public string[] particlePaths;

	public bool overrideAnimationState;

	public bool useMask;

	public bool maskOverride;

	public Vector3 tscale;

	public bool isLinked;

	public bool scaleOutLimb;

	public string solTarget;

	public Vector3 solScale;

	public bool hasSolScale;

	public string childTargetObj;

	public string insertBoneObj;

	public bool updateWithAnim;

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
		return $"property: {propertyKey} prefabPath: {prefabPath} target: {targetBone} damageTag {damageTypeKey}";
	}
}
