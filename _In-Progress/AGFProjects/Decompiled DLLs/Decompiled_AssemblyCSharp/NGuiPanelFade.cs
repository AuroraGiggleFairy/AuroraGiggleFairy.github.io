using System;
using UnityEngine;

public class NGuiPanelFade : MonoBehaviour
{
	public float duration = 0.3f;

	public bool bFadeInWhenEnabled = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mStart;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIWidget[] mWidgets;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] alpha;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFadeIn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFadeOut;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		mWidgets = GetComponentsInChildren<UIWidget>();
		alpha = new float[mWidgets.Length];
		for (int i = 0; i < mWidgets.Length; i++)
		{
			alpha[i] = mWidgets[i].color.a;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		bFadeOut = false;
		init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void reset()
	{
		for (int i = 0; i < mWidgets.Length; i++)
		{
			Color color = mWidgets[i].color;
			color.a = alpha[i];
			mWidgets[i].color = color;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		mStart = Time.time;
		bFadeIn = bFadeInWhenEnabled;
		mWidgets = GetComponentsInChildren<UIWidget>();
		if (alpha.Length != mWidgets.Length)
		{
			alpha = new float[mWidgets.Length];
		}
		for (int i = 0; i < mWidgets.Length; i++)
		{
			Color color = mWidgets[i].color;
			if (color.a != 0f)
			{
				alpha[i] = color.a;
			}
			if (bFadeIn)
			{
				color.a = 0f;
				mWidgets[i].color = color;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		float num = 0f;
		if (bFadeIn)
		{
			num = ((duration > 0f) ? Mathf.Clamp01((Time.time - mStart) / duration) : 1f);
		}
		else
		{
			if (!bFadeOut)
			{
				return;
			}
			num = ((duration > 0f) ? (1f - Mathf.Clamp01((Time.realtimeSinceStartup - mStart) / duration)) : 0f);
		}
		for (int i = 0; i < mWidgets.Length; i++)
		{
			Color color = mWidgets[i].color;
			color.a = num * alpha[i];
			mWidgets[i].color = color;
		}
		if (bFadeOut && num <= 0.001f)
		{
			reset();
			base.gameObject.SetActive(value: false);
			bFadeOut = false;
		}
		if (bFadeIn && num >= 1f)
		{
			bFadeIn = false;
		}
	}

	public void StartFadeOut()
	{
		bFadeOut = true;
		mStart = Time.time;
	}
}
