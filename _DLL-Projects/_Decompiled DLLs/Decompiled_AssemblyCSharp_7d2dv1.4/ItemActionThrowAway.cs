using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionThrowAway : ItemAction
{
	public class MyInventoryData : ItemActionData
	{
		public float m_ActivateTime;

		public bool m_bActivated;

		public bool m_bReleased;

		public float m_LastThrowTime;

		public float m_ThrowStrength;

		public bool m_bCanceled;

		public bool isCooldown;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const float SHORT_CLICK_TIME = 0.2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float defaultThrowStrength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxThrowStrength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxStrainTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle progressBarStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalType;

	public ItemActionThrowAway()
	{
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, new Color(0f, 1f, 0f, 0.35f));
		texture2D.Apply();
		progressBarStyle = new GUIStyle();
		progressBarStyle.normal.background = texture2D;
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		defaultThrowStrength = 1.1f;
		_props.ParseFloat("Throw_strength_default", ref defaultThrowStrength);
		maxThrowStrength = 5f;
		_props.ParseFloat("Throw_strength_max", ref maxThrowStrength);
		maxStrainTime = 2f;
		_props.ParseFloat("Max_strain_time", ref maxStrainTime);
		if (_props.Values.ContainsKey("Sound_start"))
		{
			soundStart = _props.Values["Sound_start"];
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void OnScreenOverlay(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		LocalPlayerUI playerUI = ((EntityPlayerLocal)myInventoryData.invData.holdingEntity).PlayerUI;
		if (!myInventoryData.isCooldown && myInventoryData.m_bActivated && Time.time - myInventoryData.m_ActivateTime > 0.2f)
		{
			float currentPower = Mathf.Min(maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / maxStrainTime;
			XUiC_ThrowPower.Status(playerUI, currentPower);
		}
		else
		{
			XUiC_ThrowPower.Status(playerUI);
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		originalType = _data.invData.holdingEntity.inventory.holdingItemItemValue.type;
		base.StartHolding(_data);
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		MyInventoryData obj = (MyInventoryData)_actionData;
		obj.m_bActivated = false;
		EntityPlayerLocal entityPlayerLocal = obj.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			XUiC_ThrowPower.Status(entityPlayerLocal.PlayerUI);
		}
	}

	public override bool AllowConcurrentActions()
	{
		return false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (!_bReleased)
		{
			if (!myInventoryData.m_bActivated)
			{
				myInventoryData.m_bActivated = true;
				myInventoryData.m_ActivateTime = Time.time;
			}
		}
		else
		{
			if (!myInventoryData.m_bActivated || myInventoryData.isCooldown)
			{
				return;
			}
			myInventoryData.m_bReleased = true;
			if (Time.time - myInventoryData.m_ActivateTime < 0.2f || maxStrainTime == 0f)
			{
				myInventoryData.m_ThrowStrength = defaultThrowStrength;
			}
			else
			{
				myInventoryData.m_ThrowStrength = Mathf.Min(maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / maxStrainTime * maxThrowStrength;
			}
			if (holdingEntity.inventory.holdingItemItemValue.Meta == 0 && EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
			{
				myInventoryData.m_LastThrowTime = Time.time + 1f;
				myInventoryData.m_bActivated = false;
				Manager.PlayInsidePlayerHead("twitch_no_attack");
				return;
			}
			myInventoryData.m_LastThrowTime = Time.time;
			myInventoryData.m_bActivated = false;
			myInventoryData.invData.holdingEntity.RightArmAnimationAttack = true;
			if (soundStart != null)
			{
				myInventoryData.invData.holdingEntity.PlayOneShot(soundStart);
			}
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.m_bActivated || (myInventoryData.m_LastThrowTime > 0f && Time.time - myInventoryData.m_LastThrowTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast))
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (myInventoryData.isCooldown)
		{
			myInventoryData.isCooldown = Time.time - myInventoryData.m_LastThrowTime < Delay;
			if (myInventoryData.m_bActivated)
			{
				myInventoryData.m_ActivateTime = Time.time;
			}
		}
		if (_actionData.invData.holdingEntity.inventory.holdingItemItemValue.type != originalType)
		{
			myInventoryData.m_bActivated = false;
			myInventoryData.m_bReleased = false;
		}
		else if (myInventoryData.m_bReleased)
		{
			float rayCast = AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
			if (!(myInventoryData.m_LastThrowTime <= 0f) && !(Time.time - myInventoryData.m_LastThrowTime < rayCast))
			{
				myInventoryData.m_LastThrowTime = Time.time;
				myInventoryData.m_bReleased = false;
				throwAway(myInventoryData);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void throwAway(MyInventoryData _actionData)
	{
		ItemInventoryData invData = _actionData.invData;
		EntityAlive holdingEntity = invData.holdingEntity;
		if (holdingEntity.inventory.holdingItemItemValue.Meta == 0 && EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.m_bActivated = false;
			return;
		}
		Vector3 lookVector = holdingEntity.GetLookVector();
		Vector3 headPosition = holdingEntity.getHeadPosition();
		Vector3 crosshairPosition3D = ((EntityPlayerLocal)holdingEntity).GetCrosshairPosition3D(0f, 0f, headPosition);
		if (!Physics.Raycast(new Ray(crosshairPosition3D - Origin.position, lookVector), out var _, 0.28f, -555274245))
		{
			crosshairPosition3D += 0.23f * lookVector;
			crosshairPosition3D -= headPosition;
			invData.gameManager.ItemDropServer(new ItemStack(holdingEntity.inventory.holdingItemItemValue, 1), crosshairPosition3D, Vector3.zero, lookVector * _actionData.m_ThrowStrength, holdingEntity.entityId, 60f, _bDropPosIsRelativeToHead: true, -1);
			holdingEntity.inventory.DecHoldingItem(1);
		}
		_actionData.invData.holdingEntity.emodel.avatarController.TriggerEvent("ItemThrownTrigger");
	}
}
