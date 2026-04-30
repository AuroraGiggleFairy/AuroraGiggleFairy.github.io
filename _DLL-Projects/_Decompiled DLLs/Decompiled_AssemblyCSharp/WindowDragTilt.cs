using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Window Drag Tilt")]
public class WindowDragTilt : MonoBehaviour
{
	public int updateOrder;

	public float degrees = 30f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mLastPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		mTrans = base.transform;
		mLastPos = mTrans.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Vector3 vector = mTrans.position - mLastPos;
		mLastPos = mTrans.position;
		mAngle += vector.x * degrees;
		mAngle = NGUIMath.SpringLerp(mAngle, 0f, 20f, Time.deltaTime);
		mTrans.localRotation = Quaternion.Euler(0f, 0f, 0f - mAngle);
	}
}
