using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Spin")]
public class Spin : MonoBehaviour
{
	public Vector3 rotationsPerSecond = new Vector3(0f, 0.1f, 0f);

	public bool ignoreTimeScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody mRb;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mTrans = base.transform;
		mRb = GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (mRb == null)
		{
			ApplyDelta(ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (mRb != null)
		{
			ApplyDelta(Time.deltaTime);
		}
	}

	public void ApplyDelta(float delta)
	{
		delta *= 360f;
		Quaternion quaternion = Quaternion.Euler(rotationsPerSecond * delta);
		if (mRb == null)
		{
			mTrans.rotation *= quaternion;
		}
		else
		{
			mRb.MoveRotation(mRb.rotation * quaternion);
		}
	}
}
