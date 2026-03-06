using System;
using UnityEngine;

public class AvatarControllerDummy : LegacyAvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public new bool bSpecialAttackPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public new float timeSpecialAttack2Playing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeRagePlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int ragingTicks;

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (modelTransform != null)
		{
			bipedTransform = modelTransform.Find(_modelName + (_bFPV ? "_FP" : ""));
			if (bipedTransform != null && entity != null)
			{
				rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName());
				SetAnimator(bipedTransform);
			}
		}
	}

	public override Transform GetRightHandTransform()
	{
		return rightHand;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignStates()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpineRotation()
	{
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return bSpecialAttackPlaying;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
		idleTime = 0f;
		bSpecialAttackPlaying = _b;
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		idleTime = 0f;
		timeSpecialAttack2Playing = 0.3f;
	}

	public override bool IsAnimationRagingPlaying()
	{
		return timeRagePlaying > 0f;
	}

	public override void StartAnimationRaging()
	{
		idleTime = 0f;
		ragingTicks = 3;
		timeRagePlaying = 0.3f;
	}

	public override bool IsAnimationWithMotionRunning()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (timeAttackAnimationPlaying > 0f)
		{
			timeAttackAnimationPlaying -= Time.deltaTime;
		}
		if (timeUseAnimationPlaying > 0f)
		{
			timeUseAnimationPlaying -= Time.deltaTime;
		}
		if (timeRagePlaying > 0f)
		{
			timeRagePlaying -= Time.deltaTime;
		}
		if (timeSpecialAttack2Playing > 0f)
		{
			timeSpecialAttack2Playing -= Time.deltaTime;
		}
	}
}
