using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Look At Target")]
public class LookAtTarget : MonoBehaviour
{
	public int level;

	public Transform target;

	public float speed = 8f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (target != null)
		{
			Vector3 forward = target.position - mTrans.position;
			if (forward.magnitude > 0.001f)
			{
				Quaternion b = Quaternion.LookRotation(forward);
				mTrans.rotation = Quaternion.Slerp(mTrans.rotation, b, Mathf.Clamp01(speed * Time.deltaTime));
			}
		}
	}
}
