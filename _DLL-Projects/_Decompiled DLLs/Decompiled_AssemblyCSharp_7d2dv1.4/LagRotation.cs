using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Lag Rotation")]
public class LagRotation : MonoBehaviour
{
	public float speed = 10f;

	public bool ignoreTimeScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion mRelative;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion mAbsolute;

	public void OnRepositionEnd()
	{
		Interpolate(1000f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Interpolate(float delta)
	{
		if (mTrans != null)
		{
			Transform parent = mTrans.parent;
			if (parent != null)
			{
				mAbsolute = Quaternion.Slerp(mAbsolute, parent.rotation * mRelative, delta * speed);
				mTrans.rotation = mAbsolute;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mTrans = base.transform;
		mRelative = mTrans.localRotation;
		mAbsolute = mTrans.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Interpolate(ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
	}
}
