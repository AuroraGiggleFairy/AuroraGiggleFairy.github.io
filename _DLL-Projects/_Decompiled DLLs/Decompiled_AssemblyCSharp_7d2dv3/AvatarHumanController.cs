using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class AvatarHumanController : AvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cOverrideLayerIndex = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cFullBodyLayerIndex = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cHitLayerIndex = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarRootMotion rootMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform modelT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform bipedT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightHandT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo baseStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo overrideStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo fullBodyStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo hitStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isVisibleInit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isVisible;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int jumpState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isJumpStarted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isCrawler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float crawlerTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isSuppressPain;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		modelT = EModelBase.FindModel(base.transform);
		assignStates();
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (!bipedT)
		{
			hitLayerIndex = 3;
			bipedT = entity.emodel.GetModelTransform();
			rightHandT = FindTransform(entity.GetRightHandTransformName());
			SetAnimator(bipedT);
			if (entity.RootMotion)
			{
				rootMotion = bipedT.gameObject.AddComponent<AvatarRootMotion>();
				rootMotion.Init(this, anim);
			}
		}
		SetWalkType(entity.GetWalkType());
		_setBool(AvatarController.isDeadHash, entity.IsDead());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform FindTransform(string _name)
	{
		return bipedT.FindInChildren(_name);
	}

	public override void SetVisible(bool _b)
	{
		if (isVisible == _b && isVisibleInit)
		{
			return;
		}
		isVisible = _b;
		isVisibleInit = true;
		Transform transform = bipedT;
		if ((bool)transform)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = _b;
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return modelT;
	}

	public override Transform GetRightHandTransform()
	{
		return rightHandT;
	}

	public override void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (returningToSleep)
		{
			base.TriggerSleeperPose(pose, returningToSleep);
		}
		else if ((bool)anim)
		{
			_setInt(AvatarController.sleeperPoseHash, pose);
			switch (pose)
			{
			case 0:
				anim.Play(AvatarController.sleeperIdleSitHash);
				break;
			case 1:
				anim.Play(AvatarController.sleeperIdleSideRightHash);
				break;
			case 2:
				anim.Play(AvatarController.sleeperIdleSideLeftHash);
				break;
			case 3:
				anim.Play(AvatarController.sleeperIdleBackHash);
				break;
			case 4:
				anim.Play(AvatarController.sleeperIdleStomachHash);
				break;
			case 5:
				anim.Play(AvatarController.sleeperIdleStandHash);
				break;
			case -2:
				anim.CrossFadeInFixedTime("Crouch Walk 8", 0.25f);
				break;
			case -1:
				_setTrigger(AvatarController.sleeperTriggerHash);
				break;
			}
		}
	}

	public override void TurnIntoCrawler()
	{
		isCrawler = true;
		crawlerTime = Time.time;
		isSuppressPain = true;
		_setInt(AvatarController.hitBodyPartHash, 0);
		SetWalkType(21);
		_setTrigger(AvatarController.toCrawlerTriggerHash);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		if (_propertyHash != AvatarController.walkTypeHash || _value != 5 || entity.GetWalkType() != 21)
		{
			base._setInt(_propertyHash, _value, _netsync);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarHumanController()
	{
	}
}
