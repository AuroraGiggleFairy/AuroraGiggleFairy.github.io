using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Follow Target")]
public class NGuiUIFollowTarget : MonoBehaviour
{
	public Transform target;

	public Camera gameCamera;

	public Camera uiCamera;

	public bool disableIfInvisible = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool mIsVisible;

	public Vector3 offset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (target != null)
		{
			if (gameCamera == null)
			{
				gameCamera = NGUITools.FindCameraForLayer(target.gameObject.layer);
			}
			if (uiCamera == null)
			{
				uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			}
			SetVisible(val: false);
		}
		else
		{
			Log.Error("Expected to have 'target' set to a valid transform", this);
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetVisible(bool val)
	{
		mIsVisible = val;
		int i = 0;
		for (int childCount = mTrans.childCount; i < childCount; i++)
		{
			NGUITools.SetActive(mTrans.GetChild(i).gameObject, val);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (!(target == null) && !(gameCamera == null))
		{
			Vector3 position = gameCamera.WorldToViewportPoint(target.position + offset);
			bool flag = (gameCamera.orthographic || position.z > 0f) && (!disableIfInvisible || (position.x > 0f && position.x < 1f && position.y > 0f && position.y < 1f));
			if (mIsVisible != flag)
			{
				SetVisible(flag);
			}
			if (flag)
			{
				base.transform.position = uiCamera.ViewportToWorldPoint(position);
				position = mTrans.localPosition;
				position.x = Mathf.FloorToInt(position.x);
				position.y = Mathf.FloorToInt(position.y);
				position.z = 0f;
				mTrans.localPosition = position;
			}
			OnUpdate(flag);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnUpdate(bool isVisible)
	{
	}
}
