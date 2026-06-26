using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarCharacterController : AvatarMultiBodyController
{
	public class AnimationStates
	{
		public readonly HashSet<int> JumpStates;

		public readonly HashSet<int> DeathStates;

		public readonly HashSet<int> ReloadStates;

		public readonly HashSet<int> HitStates;

		public AnimationStates(HashSet<int> _jumpStates, HashSet<int> _deathStates, HashSet<int> _reloadStates, HashSet<int> _hitStates)
		{
			JumpStates = _jumpStates;
			DeathStates = _deathStates;
			ReloadStates = _reloadStates;
			HitStates = _hitStates;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BodyAnimator characterBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string modelName;

	public BodyAnimator CharacterBody => characterBody;

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (characterBody != null && modelName != _modelName)
		{
			if (characterBody.Parts.BodyObj != null)
			{
				characterBody.Parts.BodyObj.SetActive(value: false);
			}
			removeBodyAnimator(characterBody);
			characterBody = null;
		}
		if (characterBody == null)
		{
			modelName = _modelName;
			Transform transform = EModelBase.FindModel(base.transform);
			if (transform != null)
			{
				Transform transform2 = transform.Find(_modelName);
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(value: true);
					characterBody = addBodyAnimator(createCharacterBody(transform2));
				}
			}
		}
		if (characterBody != null)
		{
			initBodyAnimator(characterBody, _bFPV, _bMale);
		}
		base.SwitchModelAndView(_modelName, _bFPV, _bMale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void initBodyAnimator(BodyAnimator _body, bool _bFPV, bool _bMale)
	{
		_setBool("IsMale", _bMale);
		_setFloat("IsMaleFloat", _bMale ? 1f : 0f);
		SetWalkType(entity.GetWalkType());
		_setBool(AvatarController.isDeadHash, entity.IsDead());
		_setBool(AvatarController.isFPVHash, _bFPV);
		_setBool(AvatarController.isAliveHash, entity.IsAlive());
		if (_body == characterBody)
		{
			characterBody.State = (_bFPV ? BodyAnimator.EnumState.OnlyColliders : BodyAnimator.EnumState.Visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract BodyAnimator createCharacterBody(Transform _bodyTransform);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getJumpStates()
	{
		return new HashSet<int> { Animator.StringToHash("Base Layer.Jump") };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getDeathStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		GetFirstPersonDeathStates(hashSet);
		GetThirdPersonDeathStates(hashSet);
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getReloadStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		GetFirstPersonReloadStates(hashSet);
		GetThirdPersonReloadStates(hashSet);
		return hashSet;
	}

	public override Animator GetAnimator()
	{
		return characterBody.Animator;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getHitStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		GetThirdPersonHitStates(hashSet);
		return hashSet;
	}

	public static void GetFirstPersonReloadStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBlunderbussReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvPistolReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvMP5Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSniperRifleReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvM136Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvCrossbowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvHuntingRifleReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAugerReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvChainsawReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvMagnumReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvNailGunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAK47Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAK47Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvCompoundBowReload"));
	}

	public static void GetThirdPersonReloadStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleMP5Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSniperRifleReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleM136Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleCrossbowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleHuntingRifleReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleAugerReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleChainsawReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReloadIntro"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReloadExit"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.Female44MagnumReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleNailGunReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBlunderbussReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemalePistolReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleAk47Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleCompoundBowReload"));
	}

	public static void GetFirstPersonHitStates(HashSet<int> hashSet)
	{
	}

	public static void GetThirdPersonHitStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchHeadLeft"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchHeadRight"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchChestLeft"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchChestRight"));
	}

	public static void GetFirstPersonDeathStates(HashSet<int> hashSet)
	{
	}

	public static void GetThirdPersonDeathStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("Base Layer.generic"));
		hashSet.Add(Animator.StringToHash("Base Layer.FemaleDeath01"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingHead"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingChest"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingLeftArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingRightArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingRightLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadFront"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeft"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftA"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftB"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftC"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadRight"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningChestA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningChestB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningHeadA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningHeadB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftArmA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftArmB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightArmA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightArmB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingChestA"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingChestB"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingHeadA"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingHeadB"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingLeftArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingRightArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingRightLeg"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _pid, bool _netsync = true)
	{
		base._setTrigger(_pid, _netsync);
		if (characterBody != null)
		{
			Animator animator = characterBody.Animator;
			if (AvatarMultiBodyController.animatorIsValid(animator))
			{
				animator.SetTrigger(_pid);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _pid, bool _netsync = true)
	{
		base._resetTrigger(_pid, _netsync);
		if (characterBody != null)
		{
			Animator animator = characterBody.Animator;
			if ((bool)animator)
			{
				animator.ResetTrigger(_pid);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _pid, float _value, bool _netsync = true)
	{
		base._setFloat(_pid, _value, _netsync);
		if (characterBody != null)
		{
			Animator animator = characterBody.Animator;
			if ((bool)animator)
			{
				animator.SetFloat(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _pid, bool _value, bool _netsync = true)
	{
		base._setBool(_pid, _value, _netsync);
		if (characterBody != null)
		{
			Animator animator = characterBody.Animator;
			if ((bool)animator)
			{
				animator.SetBool(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _pid, int _value, bool _netsync = true)
	{
		base._setInt(_pid, _value, _netsync);
		if (characterBody != null)
		{
			Animator animator = characterBody.Animator;
			if ((bool)animator)
			{
				animator.SetInteger(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarCharacterController()
	{
	}
}
