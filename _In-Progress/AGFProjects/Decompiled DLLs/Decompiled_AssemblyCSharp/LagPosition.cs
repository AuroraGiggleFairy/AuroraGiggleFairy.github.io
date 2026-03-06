using System;
using UnityEngine;

public class LagPosition : MonoBehaviour
{
	public Vector3 speed = new Vector3(10f, 10f, 10f);

	public bool ignoreTimeScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mRelative;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mAbsolute;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool mStarted;

	public void OnRepositionEnd()
	{
		Interpolate(1000f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Interpolate(float delta)
	{
		Transform parent = mTrans.parent;
		if (parent != null)
		{
			Vector3 vector = parent.position + parent.rotation * mRelative;
			mAbsolute.x = Mathf.Lerp(mAbsolute.x, vector.x, Mathf.Clamp01(delta * speed.x));
			mAbsolute.y = Mathf.Lerp(mAbsolute.y, vector.y, Mathf.Clamp01(delta * speed.y));
			mAbsolute.z = Mathf.Lerp(mAbsolute.z, vector.z, Mathf.Clamp01(delta * speed.z));
			mTrans.position = mAbsolute;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (mStarted)
		{
			ResetPosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mStarted = true;
		ResetPosition();
	}

	public void ResetPosition()
	{
		mAbsolute = mTrans.position;
		mRelative = mTrans.localPosition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Interpolate(ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
	}
}
