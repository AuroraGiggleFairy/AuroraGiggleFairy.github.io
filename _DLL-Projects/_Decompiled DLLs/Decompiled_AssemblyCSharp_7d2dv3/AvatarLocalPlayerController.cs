using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarLocalPlayerController : AvatarCharacterController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BodyAnimator fpsArms;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFPV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int tpvDisableInFrames;

	public BodyAnimator FPSArms => fpsArms;

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		base.SwitchModelAndView(_modelName, _bFPV, _bMale);
		if (entity is EntityPlayerLocal && (entity as EntityPlayerLocal).IsSpectator)
		{
			return;
		}
		if (fpsArms != null && isMale != _bMale)
		{
			if (fpsArms.Parts.BodyObj != null)
			{
				fpsArms.Parts.BodyObj.SetActive(value: false);
			}
			removeBodyAnimator(fpsArms);
			fpsArms = null;
		}
		if (fpsArms == null)
		{
			isMale = _bMale;
			Transform transform = (entity as EntityPlayerLocal)?.cameraTransform;
			if (transform != null)
			{
				Transform transform2 = transform.FindInChildren((entity.emodel is EModelSDCS) ? "baseRigFP" : (isMale ? "maleArms_fp" : "femaleArms_fp"));
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(value: true);
					fpsArms = addBodyAnimator(createFPSArms(transform2));
				}
			}
		}
		if (fpsArms != null)
		{
			initBodyAnimator(fpsArms, _bFPV, _bMale);
		}
		isFPV = _bFPV;
		if (_bFPV)
		{
			base.PrimaryBody = fpsArms;
			fpsArms.State = BodyAnimator.EnumState.Visible;
			base.CharacterBody.State = BodyAnimator.EnumState.OnlyColliders;
			return;
		}
		base.PrimaryBody = base.CharacterBody;
		if (fpsArms != null)
		{
			fpsArms.State = BodyAnimator.EnumState.Disabled;
		}
		base.CharacterBody.State = BodyAnimator.EnumState.Visible;
		if (base.HeldItemTransform != null)
		{
			Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 24, Utils.ExcludeLayerZoom);
		}
	}

	public void TPVResetAnimPose()
	{
		if ((bool)anim)
		{
			tpvDisableInFrames = 2;
			anim.enabled = true;
		}
	}

	public override void SetInRightHand(Transform _transform)
	{
		base.SetInRightHand(_transform);
		if (base.HeldItemTransform != null)
		{
			if (isFPV)
			{
				Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 10, Utils.ExcludeLayerZoom);
			}
			else
			{
				Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 24, Utils.ExcludeLayerZoom);
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		if (base.PrimaryBody == null || base.PrimaryBody.Parts == null)
		{
			return null;
		}
		return base.PrimaryBody.Parts.BodyObj.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void avatarVisibilityChanged(BodyAnimator _body, bool _bVisible)
	{
		if (!_bVisible)
		{
			base.avatarVisibilityChanged(_body, _bVisible);
		}
		else if (_body == fpsArms)
		{
			_body.State = ((!isFPV) ? BodyAnimator.EnumState.Disabled : BodyAnimator.EnumState.Visible);
		}
		else if (_body == base.CharacterBody)
		{
			_body.State = (isFPV ? BodyAnimator.EnumState.OnlyColliders : BodyAnimator.EnumState.Visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		base.LateUpdate();
		if (tpvDisableInFrames > 0 && --tpvDisableInFrames == 0 && (bool)anim && isFPV)
		{
			anim.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BodyAnimator createCharacterBody(Transform _bodyTransform)
	{
		AnimationStates animStates = new AnimationStates(getJumpStates(), getDeathStates(), getReloadStates(), getHitStates());
		return new UMACharacterBodyAnimator(base.Entity, animStates, _bodyTransform, BodyAnimator.EnumState.Disabled);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initBodyAnimator(BodyAnimator _body, bool _bFPV, bool _bMale)
	{
		base.initBodyAnimator(_body, _bFPV, _bMale);
		if (_body == fpsArms)
		{
			fpsArms.State = ((!_bFPV) ? BodyAnimator.EnumState.Disabled : BodyAnimator.EnumState.Visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual BodyAnimator createFPSArms(Transform _fpsArmsTransform)
	{
		AnimationStates animStates = new AnimationStates(getJumpStates(), getDeathStates(), getReloadStates(), getHitStates());
		return new FirstPersonAnimator(base.Entity, animStates, _fpsArmsTransform, BodyAnimator.EnumState.Disabled);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _pid, bool _netsync = true)
	{
		base._setTrigger(_pid, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _pid, bool _netsync = true)
	{
		base._resetTrigger(_pid, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.ResetTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _pid, float _value, bool _netsync = true)
	{
		base._setFloat(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetFloat(_pid, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _pid, bool _value, bool _netsync = true)
	{
		base._setBool(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetBool(_pid, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _pid, int _value, bool _netsync = true)
	{
		base._setInt(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetInteger(_pid, _value);
		}
	}
}
