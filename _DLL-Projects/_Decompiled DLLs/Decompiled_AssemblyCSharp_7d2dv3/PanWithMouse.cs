using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Pan With Mouse")]
public class PanWithMouse : MonoBehaviour
{
	public Vector2 degrees = new Vector2(5f, 3f);

	public float range = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion mStart;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mRot = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mTrans = base.transform;
		mStart = mTrans.localRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float deltaTime = RealTime.deltaTime;
		Vector3 vector = UICamera.lastEventPosition;
		float num = (float)Screen.width * 0.5f;
		float num2 = (float)Screen.height * 0.5f;
		if (range < 0.1f)
		{
			range = 0.1f;
		}
		float x = Mathf.Clamp((vector.x - num) / num / range, -1f, 1f);
		float y = Mathf.Clamp((vector.y - num2) / num2 / range, -1f, 1f);
		mRot = Vector2.Lerp(mRot, new Vector2(x, y), deltaTime * 5f);
		mTrans.localRotation = mStart * Quaternion.Euler((0f - mRot.y) * degrees.y, mRot.x * degrees.x, 0f);
	}
}
