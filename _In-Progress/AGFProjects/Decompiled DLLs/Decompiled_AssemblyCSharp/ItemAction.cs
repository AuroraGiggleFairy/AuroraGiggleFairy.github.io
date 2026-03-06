using System.Collections.Generic;
using System.Text;
using Audio;
using UnityEngine;
using XMLData.Item;

public abstract class ItemAction : XMLData.Item.ItemActionData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> headTag = FastTags<TagGroup.Global>.Parse("head");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> armTag = FastTags<TagGroup.Global>.Parse("arm");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> legTag = FastTags<TagGroup.Global>.Parse("leg");

	public static bool ShowDebugDisplayHit;

	public static float DebugDisplayHitSize = 0.005f;

	public static float DebugDisplayHitTime = 10f;

	public static bool ShowDistanceDebugInfo = false;

	public ItemClass item;

	public List<string> BuffActions;

	public new float Delay;

	public new float Range;

	public float SphereRadius;

	public DynamicProperties Properties = new DynamicProperties();

	public RequirementGroup ExecutionRequirements;

	public bool UseAnimation = true;

	public int ActionIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundStart;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Sound_in_head;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUseParticleHarvesting;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particleHarvestingCategory;

	public int ActionExp;

	public float ActionExpBonusMultiplier;

	public string GetSoundStart()
	{
		return soundStart;
	}

	public virtual ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		CharacterCameraAngleValid(_actionData, out var _result);
		return _result switch
		{
			eTPCameraCheckResult.Pass => ItemClass.EnumCrosshairType.Plus, 
			eTPCameraCheckResult.LineOfSightCheckFailed => ItemClass.EnumCrosshairType.Blocked, 
			_ => ItemClass.EnumCrosshairType.None, 
		};
	}

	public virtual bool IsEndDelayed()
	{
		return false;
	}

	public virtual void OnHUD(ItemActionData _actionData, int _x, int _y)
	{
	}

	public virtual void OnScreenOverlay(ItemActionData _data)
	{
	}

	public virtual bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	public virtual bool ConsumeCameraFunction(ItemActionData _actionData)
	{
		return false;
	}

	public virtual ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionData(_invData, _indexInEntityOfAction);
	}

	public virtual RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.None;
	}

	public virtual bool IsFocusBlockInside()
	{
		return true;
	}

	public virtual bool IsHUDDisabled(ItemActionData _data)
	{
		return false;
	}

	public virtual void StartHolding(ItemActionData _data)
	{
		OnModificationsChanged(_data);
	}

	public virtual void StopHolding(ItemActionData _data)
	{
		_data.invData.holdingEntity.StopAnimatorAudio(Entity.StopAnimatorAudioType.StopOnStopHolding);
	}

	public virtual void OnHoldingUpdate(ItemActionData _actionData)
	{
	}

	public virtual void ShowItem(ItemActionData _actionData, bool _show)
	{
	}

	public virtual void OnModificationsChanged(ItemActionData _data)
	{
	}

	public virtual bool CanCancel(ItemActionData _data)
	{
		return false;
	}

	public virtual void CancelReload(ItemActionData _actionData, bool holsterWeapon)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && holsterWeapon)
		{
			entityPlayerLocal.HolsterWeapon(holster: true);
		}
	}

	public virtual bool AllowConcurrentActions()
	{
		return false;
	}

	public virtual void Cleanup(ItemActionData _data)
	{
	}

	public virtual void ReadFrom(DynamicProperties _props)
	{
		Delay = 0f;
		_props.ParseFloat("Delay", ref Delay);
		_props.ParseString("Sound_start", ref soundStart);
		_props.ParseBool("Sound_in_head", ref Sound_in_head);
		if (_props.Values.ContainsKey("Particle_harvesting"))
		{
			bUseParticleHarvesting = StringParsers.ParseBool(_props.Values["Particle_harvesting"]);
			particleHarvestingCategory = _props.Params1["Particle_harvesting"];
		}
		ActionExp = 2;
		_props.ParseInt("ActionExp", ref ActionExp);
		ActionExpBonusMultiplier = 10f;
		_props.ParseFloat("ActionExpBonusMultiplier", ref ActionExpBonusMultiplier);
		_props.ParseBool("UseAnimation", ref UseAnimation);
		BuffActions = new List<string>();
		if (_props.Values.ContainsKey("Buff"))
		{
			if (_props.Values["Buff"].Contains(","))
			{
				string[] collection = _props.Values["Buff"].Replace(" ", "").Split(',');
				BuffActions.AddRange(collection);
			}
			else
			{
				BuffActions.Add(_props.Values["Buff"].Trim());
			}
		}
		else
		{
			ActionExpBonusMultiplier = 10f;
		}
		Properties = _props;
	}

	public string GetDescription()
	{
		return Properties.GetString("Description");
	}

	public virtual string CanInteract(ItemActionData _actionData)
	{
		return null;
	}

	public static void ExecuteBuffActions(List<string> actions, int instigatorId, EntityAlive target, bool isCritical, EnumBodyPartHit hitLocation, string context)
	{
		if (target == null)
		{
			return;
		}
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(instigatorId) as EntityAlive;
		if (entityAlive == null || actions == null)
		{
			return;
		}
		for (int i = 0; i < actions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(actions[i]);
			if (buff != null)
			{
				float originalValue = 1f;
				originalValue = EffectManager.GetValue(PassiveEffects.BuffProcChance, null, originalValue, entityAlive, null, FastTags<TagGroup.Global>.Parse(buff.Name));
				if (target.rand.RandomFloat <= originalValue)
				{
					target.Buffs.AddBuff(actions[i], entityAlive.entityId);
				}
			}
		}
	}

	public virtual bool IsActionRunning(ItemActionData _actionData)
	{
		return false;
	}

	public virtual bool CanExecute(ItemActionData _actionData)
	{
		if (ExecutionRequirements == null)
		{
			return true;
		}
		_actionData.invData.holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemValue;
		if (!ExecutionRequirements.IsValid(_actionData.invData.holdingEntity.MinEventContext))
		{
			return false;
		}
		return true;
	}

	public abstract void ExecuteAction(ItemActionData _actionData, bool _bReleased);

	public virtual bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		return false;
	}

	public virtual void CancelAction(ItemActionData _actionData)
	{
	}

	public virtual WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		return null;
	}

	public virtual void GetIronSights(ItemActionData _actionData, out float _fov)
	{
		_fov = 0f;
	}

	public virtual EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		return TriggerEffectManager.NoneEffect;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		return TriggerEffectManager.NoneEffect;
	}

	public virtual bool AllowItemLoopingSound(ItemActionData _actionData)
	{
		return true;
	}

	public virtual bool IsAimingGunPossible(ItemActionData _actionData)
	{
		return true;
	}

	public virtual void AimingSet(ItemActionData _actionData, bool _isAiming, bool _wasAiming)
	{
	}

	public virtual void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
	}

	public virtual void UpdateNozzleParticlesPosAndRot(ItemActionData _actionData)
	{
	}

	public virtual int GetInitialMeta(ItemValue _itemValue)
	{
		return 0;
	}

	public virtual void SwapAmmoType(EntityAlive _entity, int _ammoItemId = -1)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool canShowOverlay(ItemActionData actionData)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isShowOverlay(ItemActionData actionData)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void getOverlayData(ItemActionData actionData, out float _perc, out string _text)
	{
		_perc = 0f;
		_text = "";
	}

	public static float GetDismemberChance(ItemActionData _actionData, WorldRayHitInfo hitInfo)
	{
		FastTags<TagGroup.Global> actionTags = _actionData.ActionTags;
		if (hitInfo.tag == "E_BP_Head")
		{
			actionTags |= headTag;
		}
		else if (hitInfo.tag.ContainsCaseInsensitive("arm"))
		{
			actionTags |= armTag;
		}
		else if (hitInfo.tag.ContainsCaseInsensitive("leg"))
		{
			actionTags |= legTag;
		}
		return EffectManager.GetValue(PassiveEffects.DismemberChance, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, 0f, _actionData.invData.holdingEntity, null, actionTags | _actionData.invData.item.ItemTags);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual List<string> getBuffActions(ItemActionData _actionData)
	{
		if (BuffActions == null)
		{
			return new List<string>();
		}
		return BuffActions;
	}

	public virtual void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
	}

	public virtual bool IsEditingTool()
	{
		return false;
	}

	public virtual string GetStat(ItemActionData _data)
	{
		return string.Empty;
	}

	public virtual bool IsStatChanged()
	{
		return false;
	}

	public virtual bool HasRadial()
	{
		return false;
	}

	public virtual void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string StringFormatHandler(string title, object value)
	{
		return $"{title}: [REPLACE_COLOR]{value}[-]\n";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string BuffActionStrings(ItemAction itemAction, List<string> stringList)
	{
		if (itemAction.BuffActions == null || itemAction.BuffActions.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemAction.BuffActions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(itemAction.BuffActions[i]);
			if (buff != null && !string.IsNullOrEmpty(buff.Name))
			{
				stringList.Add(StringFormatHandler(Localization.Get("lblEffect"), $"{buff.Name}"));
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string getColoredItemStat(string _title, float _value)
	{
		if (_value > 0f)
		{
			return $"{_title}: [00ff00]+{_value.ToCultureInvariantString()}[-]";
		}
		if (_value < 0f)
		{
			return $"{_title}: [ff0000]{_value.ToCultureInvariantString()}[-]";
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string getColoredItemStatPercentage(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemBreak(ItemActionData _actionData)
	{
		if (_actionData.invData.itemValue.MaxUseTimes > 0 && _actionData.invData.itemValue.UseTimes >= (float)_actionData.invData.itemValue.MaxUseTimes)
		{
			Manager.BroadcastPlay(_actionData.invData.holdingEntity, "itembreak");
		}
	}

	public virtual bool CharacterCameraAngleValid(ItemActionData _actionData, out eTPCameraCheckResult _result)
	{
		_result = eTPCameraCheckResult.Pass;
		if (_actionData.invData.holdingEntity is EntityPlayerLocal epl)
		{
			return CharacterCameraAngleValid(epl, out _result);
		}
		return true;
	}

	public virtual bool CharacterCameraAngleValid(EntityPlayerLocal _epl, out eTPCameraCheckResult _result)
	{
		if (_epl.bFirstPersonView || _epl.vp_FPCamera.Locked3rdPerson)
		{
			_result = eTPCameraCheckResult.Pass;
			return true;
		}
		if (_epl.LineOfSightObstructed)
		{
			_result = eTPCameraCheckResult.LineOfSightCheckFailed;
			return false;
		}
		if (Vector3.Angle(to: new Vector3(_epl.cameraTransform.forward.x, 0f, _epl.cameraTransform.forward.z), from: _epl.transform.forward) > 20f)
		{
			_result = eTPCameraCheckResult.AngleCheckFailed;
			return false;
		}
		_result = eTPCameraCheckResult.Pass;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemAction()
	{
	}
}
