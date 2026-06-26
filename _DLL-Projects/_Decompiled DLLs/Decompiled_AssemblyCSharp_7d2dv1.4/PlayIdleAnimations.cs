using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Play Idle Animations")]
public class PlayIdleAnimations : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animation mAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationClip mIdle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AnimationClip> mBreaks = new List<AnimationClip>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mNextBreak;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int mLastIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		mAnim = GetComponentInChildren<Animation>();
		if (mAnim == null)
		{
			Debug.LogWarning(NGUITools.GetHierarchy(base.gameObject) + " has no Animation component");
			UnityEngine.Object.Destroy(this);
			return;
		}
		foreach (AnimationState item in mAnim)
		{
			if (item.clip.name == "idle")
			{
				item.layer = 0;
				mIdle = item.clip;
				mAnim.Play(mIdle.name);
			}
			else if (item.clip.name.StartsWith("idle"))
			{
				item.layer = 1;
				mBreaks.Add(item.clip);
			}
		}
		if (mBreaks.Count == 0)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!(mNextBreak < Time.time))
		{
			return;
		}
		if (mBreaks.Count == 1)
		{
			AnimationClip animationClip = mBreaks[0];
			mNextBreak = Time.time + animationClip.length + UnityEngine.Random.Range(5f, 15f);
			mAnim.CrossFade(animationClip.name);
			return;
		}
		int num = UnityEngine.Random.Range(0, mBreaks.Count - 1);
		if (mLastIndex == num)
		{
			num++;
			if (num >= mBreaks.Count)
			{
				num = 0;
			}
		}
		mLastIndex = num;
		AnimationClip animationClip2 = mBreaks[num];
		mNextBreak = Time.time + animationClip2.length + UnityEngine.Random.Range(2f, 8f);
		mAnim.CrossFade(animationClip2.name);
	}
}
