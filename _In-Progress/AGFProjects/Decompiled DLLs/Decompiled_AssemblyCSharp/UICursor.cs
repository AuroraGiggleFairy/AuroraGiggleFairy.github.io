using System;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/Examples/UI Cursor")]
public class UICursor : MonoBehaviour
{
	public static UICursor instance;

	public Camera uiCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform mTrans;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite mSprite;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public INGUIAtlas mAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string mSpriteName;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		instance = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mTrans = base.transform;
		mSprite = GetComponentInChildren<UISprite>();
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		if (mSprite != null)
		{
			mAtlas = mSprite.atlas;
			mSpriteName = mSprite.spriteName;
			if (mSprite.depth < 100)
			{
				mSprite.depth = 100;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (uiCamera != null)
		{
			mousePosition.x = Mathf.Clamp01(mousePosition.x / (float)Screen.width);
			mousePosition.y = Mathf.Clamp01(mousePosition.y / (float)Screen.height);
			mTrans.position = uiCamera.ViewportToWorldPoint(mousePosition);
			if (uiCamera.orthographic)
			{
				Vector3 localPosition = mTrans.localPosition;
				localPosition.x = Mathf.Round(localPosition.x);
				localPosition.y = Mathf.Round(localPosition.y);
				mTrans.localPosition = localPosition;
			}
		}
		else
		{
			mousePosition.x -= (float)Screen.width * 0.5f;
			mousePosition.y -= (float)Screen.height * 0.5f;
			mousePosition.x = Mathf.Round(mousePosition.x);
			mousePosition.y = Mathf.Round(mousePosition.y);
			mTrans.localPosition = mousePosition;
		}
	}

	public static void Clear()
	{
		if (instance != null && instance.mSprite != null)
		{
			Set(instance.mAtlas, instance.mSpriteName);
		}
	}

	public static void Set(INGUIAtlas atlas, string sprite)
	{
		if (instance != null && (bool)instance.mSprite)
		{
			instance.mSprite.atlas = atlas;
			instance.mSprite.spriteName = sprite;
			instance.mSprite.MakePixelPerfect();
			instance.Update();
		}
	}
}
