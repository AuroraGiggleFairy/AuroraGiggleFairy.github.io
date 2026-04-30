using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionCatapult : ItemActionLauncher
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataCatapult : ItemActionDataLauncher
	{
		public bool m_bActivated;

		public bool m_bCanceled;

		public float m_ActivateTime;

		public float m_MaxStrainTime;

		public ItemActionDataCatapult(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle progressBarStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundDraw;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundCancel;

	public ItemActionCatapult()
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
		if (_props.Values.ContainsKey("Sound_draw"))
		{
			soundDraw = _props.Values["Sound_draw"];
		}
		if (_props.Values.ContainsKey("Sound_cancel"))
		{
			soundCancel = _props.Values["Sound_cancel"];
		}
	}

	public override void OnModificationsChanged(ItemActionData _data)
	{
		base.OnModificationsChanged(_data);
		if (Properties.Values.ContainsKey("Max_strain_time"))
		{
			((ItemActionDataCatapult)_data).m_MaxStrainTime = StringParsers.ParseFloat(_data.invData.itemValue.GetPropertyOverride("Max_strain_time", Properties.Values["Max_strain_time"]));
		}
		else
		{
			((ItemActionDataCatapult)_data).m_MaxStrainTime = StringParsers.ParseFloat(_data.invData.itemValue.GetPropertyOverride("Max_strain_time", "2"));
		}
		((ItemActionDataCatapult)_data).m_MaxStrainTime = 60f / EffectManager.GetValue(PassiveEffects.RoundsPerMinute, _data.invData.itemValue, ((ItemActionDataCatapult)_data).m_MaxStrainTime, _data.invData.holdingEntity);
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataCatapult(_invData, _indexInEntityOfAction);
	}

	public override void OnScreenOverlay(ItemActionData _actionData)
	{
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataCatapult.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			LocalPlayerUI playerUI = entityPlayerLocal.PlayerUI;
			float value = (Time.time - itemActionDataCatapult.m_ActivateTime) / itemActionDataCatapult.m_MaxStrainTime;
			if (itemActionDataCatapult.m_bActivated)
			{
				XUiC_ThrowPower.Status(playerUI, Mathf.Clamp01(value));
			}
			else
			{
				XUiC_ThrowPower.Status(playerUI);
			}
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataCatapult.invData.holdingEntity as EntityPlayerLocal;
		if (_bReleased)
		{
			itemActionDataCatapult.m_bCanceled = false;
			itemActionDataCatapult.invData.holdingEntity.SpecialAttack = false;
		}
		if (ItemActionRanged.Reloading(itemActionDataCatapult))
		{
			itemActionDataCatapult.m_LastShotTime = Time.time;
		}
		else
		{
			if (Time.time - itemActionDataCatapult.m_LastShotTime < itemActionDataCatapult.Delay)
			{
				return;
			}
			if (!InfiniteAmmo && itemActionDataCatapult.invData.itemValue.Meta == 0)
			{
				if (AutoReload && CanReload(itemActionDataCatapult))
				{
					itemActionDataCatapult.invData.gameManager.ItemReloadServer(itemActionDataCatapult.invData.holdingEntity.entityId);
					itemActionDataCatapult.invData.holdingEntitySoundID = -2;
				}
			}
			else if (!_bReleased)
			{
				if (!itemActionDataCatapult.m_bActivated)
				{
					itemActionDataCatapult.m_bActivated = true;
					itemActionDataCatapult.m_ActivateTime = Time.time;
					itemActionDataCatapult.invData.holdingEntity.SpecialAttack = true;
					if (soundDraw != null)
					{
						_actionData.invData.holdingEntity.PlayOneShot(soundDraw);
					}
					if (entityPlayerLocal != null)
					{
						entityPlayerLocal.StartTPCameraLockTimer();
					}
				}
			}
			else if (itemActionDataCatapult.m_bActivated)
			{
				itemActionDataCatapult.strainPercent = (Time.time - itemActionDataCatapult.m_ActivateTime) / itemActionDataCatapult.m_MaxStrainTime;
				if ((itemActionDataCatapult.invData.itemValue.MaxUseTimes > 0 && itemActionDataCatapult.invData.itemValue.UseTimes >= (float)itemActionDataCatapult.invData.itemValue.MaxUseTimes) || (itemActionDataCatapult.invData.itemValue.UseTimes == 0f && itemActionDataCatapult.invData.itemValue.MaxUseTimes == 0) || (entityPlayerLocal != null && !CharacterCameraAngleValid(entityPlayerLocal, out var _)))
				{
					CancelAction(_actionData);
					itemActionDataCatapult.m_bCanceled = false;
				}
				itemActionDataCatapult.m_bActivated = false;
				base.ExecuteAction(_actionData, false);
				base.ExecuteAction(_actionData, true);
			}
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		base.OnHoldingUpdate(_actionData);
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		if (itemActionDataCatapult.m_bActivated && itemActionDataCatapult.invData.holdingEntity is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		CancelAction(_data);
		ItemActionDataCatapult obj = (ItemActionDataCatapult)_data;
		obj.m_bCanceled = false;
		EntityPlayerLocal entityPlayerLocal = obj.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			XUiC_ThrowPower.Status(entityPlayerLocal.PlayerUI);
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		if (itemActionDataCatapult.m_bActivated)
		{
			itemActionDataCatapult.m_bActivated = false;
			itemActionDataCatapult.m_bCanceled = true;
			itemActionDataCatapult.bReleased = false;
			itemActionDataCatapult.invData.holdingEntity.SpecialAttack = false;
			itemActionDataCatapult.invData.holdingEntity.SpecialAttack2 = true;
			if (soundCancel != null)
			{
				_actionData.invData.holdingEntity.PlayOneShot(soundCancel);
			}
			triggerReleased(itemActionDataCatapult, _actionData.indexInEntityOfAction);
			if (itemActionDataCatapult.invData.slotIdx == itemActionDataCatapult.invData.holdingEntity.inventory.holdingItemIdx && itemActionDataCatapult.invData.item == itemActionDataCatapult.invData.holdingEntity.inventory.holdingItem)
			{
				_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
			}
		}
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		if (itemActionDataCatapult != null)
		{
			itemActionDataCatapult.isReloadRequested = false;
			Manager.StopSequence(itemActionDataCatapult.invData.holdingEntity, itemActionDataCatapult.SoundStart);
			if (!itemActionDataCatapult.invData.holdingEntity.isEntityRemote)
			{
				itemActionDataCatapult.invData.holdingEntity.OnReloadStart();
			}
		}
	}

	public float GetStrainPercent(ItemActionData _actionData)
	{
		if (_actionData is ItemActionDataLauncher itemActionDataLauncher)
		{
			return itemActionDataLauncher.lastAttackStrainPercent;
		}
		return 0f;
	}

	public override bool CanReload(ItemActionData _actionData)
	{
		ItemActionDataCatapult itemActionDataCatapult = (ItemActionDataCatapult)_actionData;
		if (itemActionDataCatapult != null && itemActionDataCatapult.m_bActivated)
		{
			CancelAction(_actionData);
		}
		return base.CanReload(_actionData);
	}
}
