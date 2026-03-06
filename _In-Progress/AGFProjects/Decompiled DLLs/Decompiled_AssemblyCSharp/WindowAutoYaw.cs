using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Window Auto-Yaw")]
public class WindowAutoYaw : MonoBehaviour
{
	public int updateOrder;

	public Camera uiCamera;

	public float yawAmount = 20f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		mTrans.localRotation = Quaternion.identity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (uiCamera != null)
		{
			Vector3 vector = uiCamera.WorldToViewportPoint(mTrans.position);
			mTrans.localRotation = Quaternion.Euler(0f, (vector.x * 2f - 1f) * yawAmount, 0f);
		}
	}
}
