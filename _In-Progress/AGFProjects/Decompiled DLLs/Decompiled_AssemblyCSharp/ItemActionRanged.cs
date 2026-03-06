using System;
using System.Collections.Generic;
using System.Diagnostics;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionRanged : ItemActionAttack
{
	public class ItemActionDataRanged : ItemActionAttackData
	{
		public float m_LastShotTime;

		public int reloadAmount;

		public bool IsDoubleBarrel;

		public Transform muzzle;

		public Transform muzzle2;

		public Transform Laser;

		public ItemActionFiringState state;

		public float lastTimeTriggerPressed;

		public Vector3i currentDiggingLocation;

		public float curBlockDamagePerHit;

		public float curBlockDamage;

		public bool bReleased;

		public bool bPressed;

		public GameRandom rand;

		public PerlinNoise MeanderNoise;

		public byte curBurstCount;

		public float lastAccuracy;

		public float distance;

		public float damageFalloffPercent;

		public bool isReloadRequested;

		public bool isReloading;

		public bool isWeaponReloading;

		public bool isReloadCancelled;

		public bool isWeaponReloadCancelled;

		public bool wasReloadCancelled;

		public bool wasWeaponReloadCancelled;

		public bool wasAiming;

		public bool isChangingAmmoType;

		public Transform ScopeTransform;

		public Transform SideTransform;

		public Transform BarrelTransform;

		public bool hasScopeMod;

		public bool hasSideMod;

		public bool hasBarrelMod;

		public bool IsFlashSuppressed;

		public string SoundStart;

		public string SoundLoop;

		public string SoundEnd;

		public float Delay;

		public float OriginalDelay = -1f;

		public bool burstShotStarted;

		public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

		public ItemActionDataRanged(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			IsDoubleBarrel = _invData.item.ItemTags.Test_Bit(FastTags<TagGroup.Global>.GetBit("dBarrel"));
			if (IsDoubleBarrel)
			{
				muzzle2 = getModelChildTransformByName(_invData, "Muzzle_R");
				muzzle = getModelChildTransformByName(_invData, "Muzzle_L");
			}
			else
			{
				muzzle = getModelChildTransformByName(_invData, "Muzzle");
			}
			Laser = getModelChildTransformByName(_invData, "laser");
			ScopeTransform = getModelChildTransformByName(_invData, "Attachments/Scope");
			SideTransform = getModelChildTransformByName(_invData, "Attachments/Side");
			BarrelTransform = getModelChildTransformByName(_invData, "Attachments/Barrel");
			hasScopeMod = ScopeTransform != null && ScopeTransform.childCount > 0;
			hasSideMod = SideTransform != null && SideTransform.childCount > 0;
			hasBarrelMod = BarrelTransform != null && BarrelTransform.childCount > 0;
			Transform modelChildTransformByName = getModelChildTransformByName(_invData, "ironsight");
			if (modelChildTransformByName == null)
			{
				modelChildTransformByName = getModelChildTransformByName(_invData, "ironsights");
			}
			if (modelChildTransformByName != null)
			{
				modelChildTransformByName.gameObject.SetActive(!hasScopeMod);
			}
			Transform modelChildTransformByName2 = getModelChildTransformByName(_invData, "scope_rail");
			if (modelChildTransformByName2 != null)
			{
				modelChildTransformByName2.gameObject.SetActive(hasScopeMod);
			}
			Transform modelChildTransformByName3 = getModelChildTransformByName(_invData, "side_rail");
			if (modelChildTransformByName3 != null)
			{
				modelChildTransformByName3.gameObject.SetActive(hasSideMod);
			}
			Transform modelChildTransformByName4 = getModelChildTransformByName(_invData, "barrel_rail");
			if (modelChildTransformByName4 != null)
			{
				modelChildTransformByName4.gameObject.SetActive(hasBarrelMod);
			}
			m_LastShotTime = -1f;
			MeanderNoise = new PerlinNoise(_invData.holdingEntity.entityId + _invData.item.Id);
			rand = _invData.holdingEntity.rand;
			waterCollisionParticles = new CollisionParticleController();
			waterCollisionParticles.Init(_invData.holdingEntity.entityId, _invData.item.MadeOfMaterial.SurfaceCategory, "water", 16);
		}

		public static Transform getModelChildTransformByName(ItemInventoryData _invData, string _name)
		{
			if (_invData.model == null)
			{
				return null;
			}
			if (_name.Contains("/"))
			{
				return _invData.model.Find(_name);
			}
			return _invData.model.FindInChilds(_name);
		}
	}

	public static string scGunIsJammed = "GunIsJammed";

	public const float cUnderwaterDamageReductionMultipler = 0.25f;

	public const float cUnderwaterDamagePenalty = 0.75f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string bulletMaterialName;

	public bool bSupportHarvesting;

	public bool bUseMeleeCrosshair;

	[PublicizedFrom(EAccessModifier.Private)]
	public float originalDelay = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectShootDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectTriggerPullDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectShootXbox;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectTriggerPullXbox;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool rapidTrigger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool AutoReload = true;

	public int EntityPenetrationCount;

	public int BlockPenetrationFactor = 251;

	[PublicizedFrom(EAccessModifier.Private)]
	public float spreadVerticalOffset;

	public static float AccuracyUpdateDecayConstant = 9.1f;

	public static bool LogOldAccuracy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float _oldAccuracy;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataRanged(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		bulletMaterialName = "bullet";
		_props.ParseString("bullet_material", ref bulletMaterialName);
		_props.ParseBool("SupportHarvesting", ref bSupportHarvesting);
		_props.ParseBool("UseMeleeCrosshair", ref bUseMeleeCrosshair);
		EntityPenetrationCount = 0;
		_props.ParseInt("EntityPenetrationCount", ref EntityPenetrationCount);
		_props.ParseInt("BlockPenetrationFactor", ref BlockPenetrationFactor);
		_props.ParseBool("AutoReload", ref AutoReload);
		if (_props.Values.ContainsKey("triggerEffectShootDualsense"))
		{
			triggerEffectShootDualsense = _props.Values["triggerEffectShootDualsense"];
		}
		else
		{
			triggerEffectShootDualsense = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectTriggerPullDualsense"))
		{
			triggerEffectTriggerPullDualsense = _props.Values["triggerEffectTriggerPullDualsense"];
		}
		else
		{
			triggerEffectTriggerPullDualsense = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectShootXbox"))
		{
			triggerEffectShootXbox = _props.Values["triggerEffectShootXbox"];
		}
		else
		{
			triggerEffectShootXbox = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectTriggerPullXbox"))
		{
			triggerEffectTriggerPullXbox = _props.Values["triggerEffectTriggerPullXbox"];
		}
		else
		{
			triggerEffectTriggerPullXbox = string.Empty;
		}
		_props.ParseFloat("SpreadVerticalOffset", ref spreadVerticalOffset);
		_props.ParseBool("RapidTrigger", ref rapidTrigger);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData actionData)
	{
		return bSupportHarvesting;
	}

	public bool IsSingleMagazineUsage()
	{
		return AmmoIsPerMagazine;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		CharacterCameraAngleValid(_actionData, out var _result);
		switch (_result)
		{
		case eTPCameraCheckResult.Pass:
			if (!bUseMeleeCrosshair)
			{
				return ItemClass.EnumCrosshairType.Crosshair;
			}
			return ItemClass.EnumCrosshairType.Plus;
		case eTPCameraCheckResult.LineOfSightCheckFailed:
			return ItemClass.EnumCrosshairType.Blocked;
		default:
			return ItemClass.EnumCrosshairType.None;
		}
	}

	public override RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.None;
	}

	public override void OnModificationsChanged(ItemActionData _data)
	{
		ItemActionDataRanged itemActionDataRanged = _data as ItemActionDataRanged;
		if (itemActionDataRanged.OriginalDelay == -1f)
		{
			Properties.ParseFloat("Delay", ref itemActionDataRanged.OriginalDelay);
		}
		ItemValue itemValue = itemActionDataRanged.invData.itemValue;
		string optionalValue = "";
		Properties.ParseString("Sound_start", ref optionalValue);
		itemActionDataRanged.SoundStart = itemValue.GetPropertyOverride("Sound_start", optionalValue);
		optionalValue = "";
		Properties.ParseString("Sound_loop", ref optionalValue);
		itemActionDataRanged.SoundLoop = itemValue.GetPropertyOverride("Sound_loop", optionalValue);
		optionalValue = "";
		Properties.ParseString("Sound_end", ref optionalValue);
		itemActionDataRanged.SoundEnd = itemValue.GetPropertyOverride("Sound_end", optionalValue);
		if (soundStart != null && soundStart.Contains("silenced"))
		{
			itemActionDataRanged.IsFlashSuppressed = true;
		}
		itemActionDataRanged.Laser = ((itemActionDataRanged.invData.model != null) ? ItemActionDataRanged.getModelChildTransformByName(itemActionDataRanged.invData, "laser") : null);
		itemActionDataRanged.bReleased = true;
		if ((bool)itemActionDataRanged.ScopeTransform)
		{
			string optionalValue2 = "0,0,0";
			Properties.ParseString("ScopeOffset", ref optionalValue2);
			Vector3 vector = StringParsers.ParseVector3(itemValue.GetPropertyOverride("ScopeOffset", optionalValue2));
			if (itemActionDataRanged.ScopeTransform.localPosition != vector)
			{
				itemActionDataRanged.ScopeTransform.localPosition = vector;
			}
			optionalValue2 = "0,0,0";
			Properties.ParseString("ScopeRotation", ref optionalValue2);
			Vector3 vector2 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("ScopeRotation", optionalValue2));
			if (itemActionDataRanged.ScopeTransform.localRotation.eulerAngles != vector2)
			{
				itemActionDataRanged.ScopeTransform.localRotation = Quaternion.Euler(vector2);
			}
			optionalValue2 = "1,1,1";
			Properties.ParseString("ScopeScale", ref optionalValue2);
			Vector3 vector3 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("ScopeScale", optionalValue2));
			if (itemActionDataRanged.ScopeTransform.localScale != vector3)
			{
				itemActionDataRanged.ScopeTransform.localScale = vector3;
			}
		}
		if ((bool)itemActionDataRanged.SideTransform)
		{
			string optionalValue3 = "0,0,0";
			Properties.ParseString("SideOffset", ref optionalValue3);
			Vector3 vector4 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("SideOffset", optionalValue3));
			if (itemActionDataRanged.SideTransform.localPosition != vector4)
			{
				itemActionDataRanged.SideTransform.localPosition = vector4;
			}
			optionalValue3 = "0,0,0";
			Properties.ParseString("SideRotation", ref optionalValue3);
			Vector3 vector5 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("SideRotation", optionalValue3));
			if (itemActionDataRanged.SideTransform.localRotation.eulerAngles != vector5)
			{
				itemActionDataRanged.SideTransform.localRotation = Quaternion.Euler(vector5);
			}
			optionalValue3 = "1,1,1";
			Properties.ParseString("SideScale", ref optionalValue3);
			Vector3 vector6 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("SideScale", optionalValue3));
			if (itemActionDataRanged.SideTransform.localScale != vector6)
			{
				itemActionDataRanged.SideTransform.localScale = vector6;
			}
		}
		if ((bool)itemActionDataRanged.BarrelTransform)
		{
			string optionalValue4 = "0,0,0";
			Properties.ParseString("BarrelOffset", ref optionalValue4);
			Vector3 vector7 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("BarrelOffset", optionalValue4));
			if (itemActionDataRanged.BarrelTransform.localPosition != vector7)
			{
				itemActionDataRanged.BarrelTransform.localPosition = vector7;
			}
			optionalValue4 = "0,0,0";
			Properties.ParseString("BarrelRotation", ref optionalValue4);
			Vector3 vector8 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("BarrelRotation", optionalValue4));
			if (itemActionDataRanged.BarrelTransform.localRotation.eulerAngles != vector8)
			{
				itemActionDataRanged.BarrelTransform.localRotation = Quaternion.Euler(vector8);
			}
			optionalValue4 = "1,1,1";
			Properties.ParseString("BarrelScale", ref optionalValue4);
			Vector3 vector9 = StringParsers.ParseVector3(itemValue.GetPropertyOverride("BarrelScale", optionalValue4));
			if (itemActionDataRanged.BarrelTransform.localScale != vector9)
			{
				itemActionDataRanged.BarrelTransform.localScale = vector9;
			}
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		if (_data.invData.holdingEntity as EntityPlayerLocal != null)
		{
			TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect((triggerEffectTriggerPullDualsense, triggerEffectTriggerPullXbox));
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_data;
		if (itemActionDataRanged.state != ItemActionFiringState.Off)
		{
			itemActionDataRanged.state = ItemActionFiringState.Off;
			ItemActionEffects(GameManager.Instance, itemActionDataRanged, 0, Vector3.zero, Vector3.forward);
		}
		itemActionDataRanged.bReleased = true;
		itemActionDataRanged.lastAccuracy = 1f;
		if (_data.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffect);
		}
		stopParticles(itemActionDataRanged.muzzle);
		stopParticles(itemActionDataRanged.muzzle2);
		if (Manager.IsASequence(itemActionDataRanged.invData.holdingEntity, itemActionDataRanged.SoundStart))
		{
			Manager.StopSequence(itemActionDataRanged.invData.holdingEntity, itemActionDataRanged.SoundStart);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopParticles(Transform t)
	{
		if (t != null)
		{
			ParticleSystem[] componentsInChildren = t.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_actionData;
		if (Reloading(itemActionDataRanged))
		{
			return true;
		}
		if (rapidTrigger && itemActionDataRanged.bReleased && Time.time - itemActionDataRanged.m_LastShotTime > 0.25f)
		{
			return false;
		}
		if (Time.time - itemActionDataRanged.m_LastShotTime < itemActionDataRanged.Delay)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeCurrentlyLoadedAmmunition(ItemValue _gun, ItemValue _ammo, EntityAlive _entity)
	{
		ItemStack itemStack = new ItemStack(_ammo, _gun.Meta);
		int itemCount = _entity.bag.GetItemCount(_ammo);
		int itemCount2 = _entity.inventory.GetItemCount(_ammo);
		EntityPlayerLocal entityPlayerLocal = _entity as EntityPlayerLocal;
		if (itemStack.count > 0)
		{
			if (itemCount > 0)
			{
				if (!entityPlayerLocal.bag.AddItem(itemStack) && !entityPlayerLocal.inventory.AddItem(itemStack))
				{
					GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId);
					entityPlayerLocal.PlayOneShot("itemdropped");
				}
			}
			else if (itemCount2 > 0)
			{
				if (!entityPlayerLocal.inventory.AddItem(itemStack) && !entityPlayerLocal.bag.AddItem(itemStack))
				{
					GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId);
					entityPlayerLocal.PlayOneShot("itemdropped");
				}
			}
			else if (!entityPlayerLocal.bag.AddItem(itemStack) && !entityPlayerLocal.inventory.AddItem(itemStack))
			{
				GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId);
				entityPlayerLocal.PlayOneShot("itemdropped");
			}
		}
		_gun.Meta = 0;
		_entity.inventory.CallOnToolbeltChangedInternal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadNewAmmunition(ItemValue _gun, ItemValue _ammo, EntityAlive _entity)
	{
		ItemActionDataRanged obj = (ItemActionDataRanged)_entity.inventory.holdingItemData.actionData[0];
		if (_gun.SelectedAmmoTypeIndex == MagazineItemNames.Length)
		{
			_gun.SelectedAmmoTypeIndex = 0;
		}
		obj.isChangingAmmoType = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedAmmoById(int _ammoItemId, ItemValue _gun)
	{
		for (int i = 0; i < MagazineItemNames.Length; i++)
		{
			if (ItemClass.GetItem(MagazineItemNames[i]).type == _ammoItemId)
			{
				_gun.SelectedAmmoTypeIndex = (byte)i;
				break;
			}
		}
	}

	public virtual void SetAmmoType(EntityAlive _entity, ref ItemValue _gun, int _lastSelectedIndex, int _newSelectedIndex)
	{
		_gun.SelectedAmmoTypeIndex = (byte)_newSelectedIndex;
		if (_gun.Equals(_entity.inventory.holdingItemItemValue))
		{
			SwapAmmoType(_entity, ItemClass.GetItem(MagazineItemNames[_newSelectedIndex]).type);
			return;
		}
		ItemValue ammo = ItemClass.GetItem(MagazineItemNames[_lastSelectedIndex]);
		ItemClass.GetItem(MagazineItemNames[_newSelectedIndex]);
		removeCurrentlyLoadedAmmunition(_gun, ammo, _entity);
		ItemActionDataRanged adr = _entity.inventory.holdingItemData.actionData[0] as ItemActionDataRanged;
		requestReload(adr);
	}

	public void ReloadSuccess(ItemActionDataRanged _adr)
	{
		_adr?.invData.holdingEntity.inventory.holdingItemItemValue.RemoveMetaData(scGunIsJammed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void requestReload(ItemActionDataRanged _adr)
	{
		if (_adr != null)
		{
			_adr.isReloadRequested = true;
			GameManager.Instance.ItemReloadServer(_adr.invData.holdingEntity.entityId);
		}
	}

	public void SwapSelectedAmmo(EntityAlive _entity, int _ammoIndex)
	{
		if (_ammoIndex == _entity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex)
		{
			if (_entity.inventory.holdingItemData.actionData[0] is ItemActionDataRanged itemActionDataRanged && _entity.inventory.GetHoldingGun().CanReload(itemActionDataRanged))
			{
				requestReload(itemActionDataRanged);
			}
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(MagazineItemNames[_ammoIndex]);
		if (itemClass != null)
		{
			SwapAmmoType(_entity, itemClass.Id);
		}
	}

	public override void SwapAmmoType(EntityAlive _entity, int _ammoItemId = -1)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_entity.inventory.holdingItemData.actionData[0];
		CancelReload(itemActionDataRanged, holsterWeapon: true);
		ItemValue itemValue = itemActionDataRanged.invData.itemValue;
		EntityAlive holdingEntity = itemActionDataRanged.invData.holdingEntity;
		ItemValue ammo = ItemClass.GetItem(MagazineItemNames[itemValue.SelectedAmmoTypeIndex]);
		itemActionDataRanged.reloadAmount = 0;
		removeCurrentlyLoadedAmmunition(itemValue, ammo, holdingEntity);
		if (_ammoItemId == -1)
		{
			for (int i = 0; i < MagazineItemNames.Length; i++)
			{
				itemValue.SelectedAmmoTypeIndex++;
				if (itemValue.SelectedAmmoTypeIndex == MagazineItemNames.Length)
				{
					itemValue.SelectedAmmoTypeIndex = 0;
				}
				ItemValue itemValue2 = ItemClass.GetItem(MagazineItemNames[itemValue.SelectedAmmoTypeIndex]);
				if (itemActionDataRanged.invData.holdingEntity.inventory.GetItemCount(itemValue2) + itemActionDataRanged.invData.holdingEntity.bag.GetItemCount(itemValue2) + itemActionDataRanged.invData.itemValue.Meta > 0)
				{
					break;
				}
			}
		}
		else
		{
			setSelectedAmmoById(_ammoItemId, itemValue);
		}
		ItemValue ammo2 = ItemClass.GetItem(MagazineItemNames[itemValue.SelectedAmmoTypeIndex]);
		_entity.inventory.CallOnToolbeltChangedInternal();
		loadNewAmmunition(itemValue, ammo2, holdingEntity);
		EntityPlayerLocal entityPlayerLocal = itemActionDataRanged.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			itemActionDataRanged.invData.holdingEntity.ForceHoldingWeaponUpdate();
		}
		requestReload(itemActionDataRanged);
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.HolsterWeapon(holster: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CycleAmmoType(ItemActionData _actionData, bool excludeNonUnderwaterAmmoTypes)
	{
		if (MagazineItemNames.Length <= 1)
		{
			return;
		}
		int selectedAmmoTypeIndex = _actionData.invData.holdingEntity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex;
		int num = selectedAmmoTypeIndex;
		selectedAmmoTypeIndex--;
		while (selectedAmmoTypeIndex != num)
		{
			ItemValue itemValue = ItemClass.GetItem(MagazineItemNames[selectedAmmoTypeIndex]);
			if (excludeNonUnderwaterAmmoTypes && !itemValue.ItemClass.UsableUnderwater)
			{
				selectedAmmoTypeIndex--;
				continue;
			}
			if (_actionData.invData.holdingEntity.bag.GetItemCount(itemValue) > 0)
			{
				break;
			}
			selectedAmmoTypeIndex--;
			if (selectedAmmoTypeIndex < 0)
			{
				selectedAmmoTypeIndex = MagazineItemNames.Length - 1;
			}
		}
		SwapSelectedAmmo(_actionData.invData.holdingEntity, selectedAmmoTypeIndex);
	}

	public virtual bool IsAmmoUsableUnderwater(EntityAlive holdingEntity)
	{
		int selectedAmmoTypeIndex = holdingEntity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex;
		return ItemClass.GetItem(MagazineItemNames[selectedAmmoTypeIndex]).ItemClass.UsableUnderwater;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_actionData;
		itemActionDataRanged.Delay = 60f / EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemActionDataRanged.invData.itemValue, 60f / itemActionDataRanged.OriginalDelay, itemActionDataRanged.invData.holdingEntity);
		float lastUseTime = itemActionDataRanged.lastUseTime;
		itemActionDataRanged.lastUseTime = Time.time;
		float deltaTickTime = itemActionDataRanged.lastUseTime - lastUseTime;
		if (_actionData.invData.holdingEntity.isHeadUnderwater && _actionData.invData.itemValue.ItemClass != null && MagazineItemNames != null && !ItemClass.GetItemClass(MagazineItemNames[_actionData.invData.itemValue.SelectedAmmoTypeIndex]).UsableUnderwater)
		{
			CycleAmmoType(_actionData, excludeNonUnderwaterAmmoTypes: true);
			return;
		}
		if (itemActionDataRanged.state != ItemActionFiringState.Off && itemActionDataRanged.m_LastShotTime > 0f && Time.time > itemActionDataRanged.m_LastShotTime + itemActionDataRanged.Delay * 2f)
		{
			triggerReleased(itemActionDataRanged, _actionData.indexInEntityOfAction);
		}
		updateAccuracy(_actionData, _actionData.invData.holdingEntity.AimingGun, deltaTickTime);
		if ((bool)itemActionDataRanged.SideTransform && itemActionDataRanged.Laser == null && itemActionDataRanged.SideTransform.childCount > 0)
		{
			itemActionDataRanged.Laser = ItemActionDataRanged.getModelChildTransformByName(_actionData.invData, "laser");
		}
		if (ItemAction.ShowDistanceDebugInfo || (_actionData as ItemActionDataRanged).Laser != null)
		{
			GetExecuteActionTarget(_actionData);
		}
	}

	public static bool ReloadCancelled(ItemActionDataRanged actionData)
	{
		if (!actionData.isReloadCancelled)
		{
			return actionData.isWeaponReloadCancelled;
		}
		return true;
	}

	public static bool NotReloadCancelled(ItemActionDataRanged actionData)
	{
		if (actionData.isReloadCancelled)
		{
			return !actionData.isWeaponReloadCancelled;
		}
		return true;
	}

	public static bool Reloading(ItemActionDataRanged actionData)
	{
		if (!actionData.isReloading && !actionData.isWeaponReloading)
		{
			return actionData.isReloadRequested;
		}
		return true;
	}

	public static bool NotReloading(ItemActionDataRanged actionData)
	{
		if (!actionData.isReloading && !actionData.isWeaponReloading)
		{
			return !actionData.isReloadRequested;
		}
		return false;
	}

	public override void CancelReload(ItemActionData _data, bool holsterWeapon)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_data;
		bool num = NotReloading(itemActionDataRanged);
		if (num)
		{
			itemActionDataRanged.invData.holdingEntity.emodel.avatarController.SetReloadBool(value: false);
		}
		if (!num && !ReloadCancelled(itemActionDataRanged))
		{
			base.CancelReload(_data, holsterWeapon);
			itemActionDataRanged.isReloadCancelled = true;
			itemActionDataRanged.isWeaponReloadCancelled = itemActionDataRanged.invData.item.HasReloadAnim;
			itemActionDataRanged.isChangingAmmoType = false;
			itemActionDataRanged.invData.holdingEntity.emodel.avatarController.SetReloadBool(value: false);
			if (itemActionDataRanged.state != ItemActionFiringState.Off)
			{
				itemActionDataRanged.state = ItemActionFiringState.Off;
				ItemActionEffects(GameManager.Instance, itemActionDataRanged, 0, Vector3.zero, Vector3.forward);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJammed(ItemValue iv)
	{
		int value;
		return iv.TryGetMetadata(scGunIsJammed, out value);
	}

	public override bool CanReload(ItemActionData _actionData)
	{
		ItemActionDataRanged actionData = (ItemActionDataRanged)_actionData;
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		ItemValue itemValue = ItemClass.GetItem(MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex]);
		int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, holdingItemItemValue, BulletsPerMagazine, _actionData.invData.holdingEntity);
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (NotReloading(actionData) && (entityPlayerLocal == null || !entityPlayerLocal.CancellingInventoryActions) && (isJammed(holdingItemItemValue) || _actionData.invData.itemValue.Meta < num))
		{
			if (_actionData.invData.holdingEntity.inventory.GetItemCount(itemValue) <= 0 && _actionData.invData.holdingEntity.bag.GetItemCount(itemValue) <= 0)
			{
				return HasInfiniteAmmo(_actionData);
			}
			return true;
		}
		return false;
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		if (_actionData is ItemActionDataRanged itemActionDataRanged)
		{
			itemActionDataRanged.isReloadRequested = false;
			if (!itemActionDataRanged.invData.holdingEntity.isEntityRemote)
			{
				Manager.StopSequence(itemActionDataRanged.invData.holdingEntity, itemActionDataRanged.SoundStart);
				itemActionDataRanged.invData.holdingEntity.emodel.avatarController.CancelEvent("WeaponFire");
				itemActionDataRanged.invData.holdingEntity.OnReloadStart();
			}
		}
	}

	public override bool IsAimingGunPossible(ItemActionData _actionData)
	{
		return NotReloading((ItemActionDataRanged)_actionData);
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.AimingGun)
		{
			return EnumCameraShake.Small;
		}
		return EnumCameraShake.None;
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		return TriggerEffectManager.GetTriggerEffect((triggerEffectTriggerPullDualsense, triggerEffectTriggerPullXbox));
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		return TriggerEffectManager.GetTriggerEffect((triggerEffectShootDualsense, triggerEffectShootXbox));
	}

	public override bool AllowItemLoopingSound(ItemActionData _actionData)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_actionData;
		int burstCount = GetBurstCount(_actionData);
		if (_actionData.invData.itemValue.Meta > 0 && burstCount > 1 && itemActionDataRanged.curBurstCount < burstCount && !string.IsNullOrEmpty(soundRepeat))
		{
			return itemActionDataRanged.state == ItemActionFiringState.Loop;
		}
		return false;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		ItemActionDataRanged itemActionDataRanged = _actionData as ItemActionDataRanged;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			switch (itemActionDataRanged.state)
			{
			case ItemActionFiringState.Off:
			{
				TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect((triggerEffectTriggerPullDualsense, triggerEffectTriggerPullXbox));
				GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect);
				break;
			}
			default:
			{
				TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect((triggerEffectShootDualsense, triggerEffectShootXbox));
				GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect);
				break;
			}
			}
		}
		bool flag = false;
		if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
		{
			if (_firingState == 0 && itemActionDataRanged.invData.itemValue.Meta != 0)
			{
				if (!Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart))
				{
					if (itemActionDataRanged.state != ItemActionFiringState.Off)
					{
						Manager.Play(holdingEntity, itemActionDataRanged.SoundEnd);
					}
				}
				else if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
				{
					Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
				}
			}
			else if (_firingState != 0 && itemActionDataRanged.invData.itemValue.Meta == 0)
			{
				if (!Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart))
				{
					if (itemActionDataRanged.state != ItemActionFiringState.Off)
					{
						Manager.Play(holdingEntity, itemActionDataRanged.SoundStart);
						flag = true;
					}
				}
				else if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
				{
					Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
				}
			}
			else if (itemActionDataRanged.invData.itemValue.Meta == 0 && Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart) && (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote))
			{
				Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
			}
		}
		if (_firingState == 0)
		{
			return;
		}
		onHoldingEntityFired(_actionData);
		string text = ((_firingState == 1) ? itemActionDataRanged.SoundStart : itemActionDataRanged.SoundLoop);
		if (!string.IsNullOrEmpty(text))
		{
			if (!Manager.IsASequence(holdingEntity, text))
			{
				if (!flag || _firingState != 1)
				{
					Manager.Play(holdingEntity, text);
				}
			}
			else
			{
				Manager.PlaySequence(holdingEntity, text);
			}
		}
		if (holdingEntity.inventory.IsHUDDisabled() || itemActionDataRanged.IsFlashSuppressed || !itemActionDataRanged.muzzle)
		{
			return;
		}
		bool flag2 = (bool)entityPlayerLocal && entityPlayerLocal.bFirstPersonView;
		if (particlesMuzzleFire != null)
		{
			ParticleEffect pe = new ParticleEffect((flag2 && particlesMuzzleFireFpv != null) ? particlesMuzzleFireFpv : particlesMuzzleFire, Vector3.zero, 1f, Color.clear, null, itemActionDataRanged.muzzle, _OLDCreateColliders: false);
			Transform transform = _gameManager.SpawnParticleEffectClientForceCreation(pe, holdingEntity.entityId, _worldSpawn: false);
			if ((bool)transform)
			{
				if (itemActionDataRanged.IsDoubleBarrel && itemActionDataRanged.invData.itemValue.Meta == 0)
				{
					transform.SetParent(itemActionDataRanged.muzzle2, worldPositionStays: false);
				}
				else
				{
					transform.SetParent(itemActionDataRanged.muzzle, worldPositionStays: false);
				}
				if (transform.GetComponentsInChildren<ParticleSystem>().Length != 0 && entityPlayerLocal == GameManager.Instance.World.GetPrimaryPlayer() && entityPlayerLocal.vp_FPCamera.OnValue_IsFirstPerson)
				{
					Utils.SetLayerRecursively(transform.gameObject, 10);
				}
			}
		}
		if (particlesMuzzleSmoke != null)
		{
			float lightValue = _gameManager.World.GetLightBrightness(World.worldToBlockPos(itemActionDataRanged.muzzle.position)) / 2f;
			ParticleEffect pe2 = new ParticleEffect((flag2 && particlesMuzzleSmokeFpv != null) ? particlesMuzzleSmokeFpv : particlesMuzzleSmoke, Vector3.zero, lightValue, Color.clear, null, null, _OLDCreateColliders: false);
			Transform transform2 = _gameManager.SpawnParticleEffectClientForceCreation(pe2, holdingEntity.entityId, _worldSpawn: false);
			if ((bool)transform2 && entityPlayerLocal == GameManager.Instance.World.GetPrimaryPlayer() && entityPlayerLocal.vp_FPCamera.OnValue_IsFirstPerson)
			{
				transform2.gameObject.layer = 10;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onHoldingEntityFired(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			_actionData.invData.holdingEntity.emodel.avatarController.SetMeleeAttackSpeed(1f / ((ItemActionDataRanged)_actionData).Delay);
			_actionData.invData.holdingEntity.OnFired();
		}
		(_actionData as ItemActionDataRanged).lastAccuracy *= EffectManager.GetValue(PassiveEffects.IncrementalSpreadMultiplier, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity);
		(_actionData as ItemActionDataRanged).lastAccuracy = Mathf.Min((_actionData as ItemActionDataRanged).lastAccuracy, 5f);
		if (_actionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.GetTriggerEffect((triggerEffectShootDualsense, triggerEffectShootXbox)));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void triggerReleased(ItemActionDataRanged myActionData, int _idx)
	{
		myActionData.bPressed = false;
		myActionData.bReleased = true;
		myActionData.invData.gameManager.ItemActionEffectsServer(myActionData.invData.holdingEntity.entityId, myActionData.invData.slotIdx, _idx, 0, Vector3.zero, Vector3.zero);
		myActionData.state = ItemActionFiringState.Off;
		if (myActionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.GetTriggerEffect((triggerEffectTriggerPullDualsense, triggerEffectTriggerPullXbox)));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int getUserData(ItemActionData _actionData)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ConsumeAmmo(ItemActionData _actionData)
	{
		_actionData.invData.itemValue.Meta--;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_actionData;
		if (_actionData.invData.holdingEntity is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
			if (!CharacterCameraAngleValid(_actionData, out var _))
			{
				return;
			}
		}
		if (_bReleased)
		{
			itemActionDataRanged.bReleased = true;
			itemActionDataRanged.curBurstCount = 0;
			if (Manager.IsASequence(_actionData.invData.holdingEntity, itemActionDataRanged.SoundStart))
			{
				Manager.StopSequence(_actionData.invData.holdingEntity, itemActionDataRanged.SoundStart);
			}
			triggerReleased(itemActionDataRanged, _actionData.indexInEntityOfAction);
			return;
		}
		bool flag = !itemActionDataRanged.bPressed;
		bool flag2 = flag && rapidTrigger;
		itemActionDataRanged.bPressed = true;
		int burstCount = GetBurstCount(_actionData);
		bool flag3 = itemActionDataRanged.curBurstCount < burstCount;
		flag3 = flag3 || burstCount == -1;
		if (!flag2 && !flag3 && !itemActionDataRanged.bReleased)
		{
			return;
		}
		bool bReleased = itemActionDataRanged.bReleased;
		itemActionDataRanged.bReleased = false;
		if (Reloading(itemActionDataRanged))
		{
			itemActionDataRanged.m_LastShotTime = Time.time;
		}
		else
		{
			if (!flag2 && Time.time - itemActionDataRanged.m_LastShotTime < itemActionDataRanged.Delay)
			{
				return;
			}
			if (itemActionDataRanged.burstShotStarted)
			{
				itemActionDataRanged.burstShotStarted = false;
			}
			itemActionDataRanged.m_LastShotTime = Time.time;
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			holdingEntity.MinEventContext.Other = null;
			if (isJammed(holdingEntity.inventory.holdingItemItemValue))
			{
				itemActionDataRanged.m_LastShotTime = Time.time + 1f;
				if (flag)
				{
					holdingEntity.FireEvent(MinEventTypes.onSelfItemJammedUse);
				}
				return;
			}
			if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, itemActionDataRanged.invData.item.ItemTags) > 0f)
			{
				itemActionDataRanged.m_LastShotTime = Time.time + 1f;
				if (flag)
				{
					Manager.PlayInsidePlayerHead("twitch_no_attack");
				}
				return;
			}
			if (holdingEntity.isHeadUnderwater && !IsAmmoUsableUnderwater(holdingEntity))
			{
				if (flag)
				{
					GameManager.ShowTooltip(holdingEntity as EntityPlayerLocal, "ttCannotUseAtThisTime");
				}
				return;
			}
			if (itemActionDataRanged.invData.itemValue.PercentUsesLeft <= 0f)
			{
				if (flag)
				{
					EntityPlayerLocal player = holdingEntity as EntityPlayerLocal;
					if (item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
					{
						Manager.PlayInsidePlayerHead(item.Properties.Values[ItemClass.PropSoundJammed]);
					}
					GameManager.ShowTooltip(player, "ttItemNeedsRepair");
				}
				return;
			}
			itemActionDataRanged.invData.holdingEntity.MinEventContext.ItemValue = itemActionDataRanged.invData.holdingEntity.inventory.holdingItemItemValue;
			itemActionDataRanged.invData.holdingEntity.MinEventContext.ItemActionData = itemActionDataRanged.invData.actionData[0];
			itemActionDataRanged.curBurstCount++;
			if (!checkAmmo(itemActionDataRanged))
			{
				if (bReleased)
				{
					holdingEntity.PlayOneShot(soundEmpty);
					if (itemActionDataRanged.state != ItemActionFiringState.Off)
					{
						itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(itemActionDataRanged.invData.holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero);
					}
					itemActionDataRanged.state = ItemActionFiringState.Off;
					if (CanReload(itemActionDataRanged))
					{
						requestReload(itemActionDataRanged);
						itemActionDataRanged.invData.holdingEntitySoundID = -2;
					}
				}
				return;
			}
			itemActionDataRanged.burstShotStarted = true;
			itemActionDataRanged.invData.holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotEnd);
			itemActionDataRanged.invData.holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotStart);
			if (itemActionDataRanged.state == ItemActionFiringState.Off)
			{
				itemActionDataRanged.state = ItemActionFiringState.Start;
			}
			else
			{
				itemActionDataRanged.state = ItemActionFiringState.Loop;
			}
			if (!InfiniteAmmo)
			{
				ConsumeAmmo(_actionData);
			}
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2);
			Vector3 shotDirection = Vector3.zero;
			int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemActionDataRanged.invData.itemValue, 1f, itemActionDataRanged.invData.holdingEntity);
			bool flag4 = false;
			for (int i = 0; i < num; i++)
			{
				bool hitEntityFound = false;
				shotDirection = fireShot(i, itemActionDataRanged, ref hitEntityFound);
				if (hitEntityFound)
				{
					flag4 = true;
				}
			}
			if (!flag4 && holdingEntity != null)
			{
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionMissEntity : MinEventTypes.onSelfSecondaryActionMissEntity);
			}
			holdingEntity.SetModelLayer(modelLayer);
			Vector3 _startPos;
			Vector3 _direction;
			int actionEffectsValues = GetActionEffectsValues(_actionData, out _startPos, out _direction);
			itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, (int)itemActionDataRanged.state, _startPos, _direction, actionEffectsValues | getUserData(_actionData));
			if (itemActionDataRanged.invData.itemValue.MaxUseTimes > 0)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemActionDataRanged.invData.itemValue, 1f, itemActionDataRanged.invData.holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags);
				if (itemActionDataRanged.invData.itemValue.PercentUsesLeft == 0f)
				{
					itemActionDataRanged.state = ItemActionFiringState.Off;
				}
			}
			if (GetMaxAmmoCount(itemActionDataRanged) == 1 && itemActionDataRanged.invData.itemValue.Meta == 0)
			{
				if (itemActionDataRanged.state != ItemActionFiringState.Off)
				{
					itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero);
				}
				itemActionDataRanged.state = ItemActionFiringState.Off;
				item.StopHoldingAudio(itemActionDataRanged.invData);
				if (AutoReload && CanReload(itemActionDataRanged))
				{
					requestReload(itemActionDataRanged);
				}
			}
			Vector3 kickbackForce = GetKickbackForce(shotDirection);
			holdingEntity.motion += kickbackForce * (holdingEntity.AimingGun ? 0.2f : 0.5f);
			holdingEntity.inventory.CallOnToolbeltChangedInternal();
			HandleItemBreak(_actionData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool checkAmmo(ItemActionData _actionData)
	{
		if (!InfiniteAmmo)
		{
			return _actionData.invData.itemValue.Meta > 0;
		}
		return true;
	}

	public bool HasInfiniteAmmo(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.InfiniteAmmo, _actionData.invData.itemValue, 0f, _actionData.invData.holdingEntity) > 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float updateAccuracy(ItemActionData _actionData, bool _isAimingGun, float deltaTickTime)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float num = ((!_isAimingGun) ? EffectManager.GetValue(PassiveEffects.SpreadMultiplierHip, itemValue, 1f, holdingEntity) : EffectManager.GetValue(PassiveEffects.SpreadMultiplierAiming, itemValue, 0.1f, holdingEntity));
		num = ((holdingEntity.moveDirection == Vector3.zero) ? (num * EffectManager.GetValue(PassiveEffects.SpreadMultiplierIdle, itemValue, 0.1f, holdingEntity)) : (holdingEntity.MovementRunning ? (num * EffectManager.GetValue(PassiveEffects.SpreadMultiplierRunning, itemValue, 1f, holdingEntity)) : (num * EffectManager.GetValue(PassiveEffects.SpreadMultiplierWalking, itemValue, 1f, holdingEntity))));
		if (holdingEntity.IsCrouching)
		{
			num *= EffectManager.GetValue(PassiveEffects.SpreadMultiplierCrouching, itemValue, 1f, holdingEntity);
		}
		ItemActionDataRanged obj = (ItemActionDataRanged)_actionData;
		double num2 = Mathf.Clamp01(EffectManager.GetValue(PassiveEffects.WeaponHandling, itemValue, 0.1f, holdingEntity));
		obj.lastAccuracy = AccuracyExpDecay(obj.lastAccuracy, num, (double)AccuracyUpdateDecayConstant * num2, deltaTickTime);
		return obj.lastAccuracy;
		[PublicizedFrom(EAccessModifier.Internal)]
		static float AccuracyExpDecay(float initial, float target, double decay, float dt)
		{
			dt = Mathf.Clamp01(dt);
			float num3 = (float)((double)target + (double)(initial - target) * Math.Exp((0.0 - decay) * (double)dt));
			if (Math.Abs(num3 - initial) < 1E-05f)
			{
				return target;
			}
			return num3;
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	public static void ResetOldAccuracy()
	{
		_oldAccuracy = 1f;
	}

	public virtual float GetRange(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, _actionData.invData.itemValue, Range, _actionData.invData.holdingEntity);
	}

	public virtual int GetMaxAmmoCount(ItemActionData _actionData)
	{
		return (int)EffectManager.GetValue(PassiveEffects.MagazineSize, _actionData.invData.itemValue, BulletsPerMagazine, _actionData.invData.holdingEntity);
	}

	public virtual int GetBurstCount(ItemActionData _actionData)
	{
		return (int)EffectManager.GetValue(PassiveEffects.BurstRoundCount, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 getDirectionOffset(ItemActionDataRanged _actionData, Vector3 _forward, int _shotOffset = 0)
	{
		float value = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity);
		value *= _actionData.lastAccuracy;
		value *= (float)_actionData.MeanderNoise.Noise(Time.time, 0.0, _shotOffset) * 0.66f;
		float x = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity) * _actionData.lastAccuracy * ((float)_actionData.MeanderNoise.Noise(0.0, Time.time, _shotOffset) * 0.66f) + spreadVerticalOffset;
		Quaternion quaternion = Quaternion.LookRotation(_forward, Vector3.up);
		Vector3 vector = Quaternion.Euler(x, value, 0f) * Vector3.forward;
		return quaternion * vector;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 getDirectionRandomOffset(ItemActionDataRanged _actionData, Vector3 _forward)
	{
		float value = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity);
		value *= _actionData.lastAccuracy;
		value *= _actionData.rand.RandomFloat * 2f - 1f;
		float x = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity) * _actionData.lastAccuracy * (_actionData.rand.RandomFloat * 2f - 1f) + spreadVerticalOffset;
		Quaternion quaternion = Quaternion.LookRotation(_forward, Vector3.up);
		Vector3 vector = Quaternion.Euler(x, value, 0f) * Vector3.forward;
		return quaternion * vector;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 fireShot(int _shotIdx, ItemActionDataRanged _actionData, ref bool hitEntityFound)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float range = GetRange(_actionData);
		Ray lookRay = holdingEntity.GetLookRay();
		lookRay.direction = getDirectionOffset(_actionData, lookRay.direction, _shotIdx);
		_actionData.waterCollisionParticles.Reset();
		_actionData.waterCollisionParticles.CheckCollision(lookRay.origin, lookRay.direction, range, holdingEntity.entityId);
		int hitMask = ((hitmaskOverride == 0) ? 8 : hitmaskOverride);
		int num = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue, EntityPenetrationCount, holdingEntity, null, itemValue.ItemClass.ItemTags));
		num++;
		int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, itemValue, BlockPenetrationFactor, holdingEntity, null, itemValue.ItemClass.ItemTags));
		EntityAlive entityAlive = null;
		hitEntityFound = false;
		for (int i = 0; i < num; i++)
		{
			if (Voxel.Raycast(_actionData.invData.world, lookRay, range, -538750997, hitMask, 0f))
			{
				WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
				if (worldRayHitInfo.hit.distanceSq > range * range)
				{
					return lookRay.direction;
				}
				lookRay.origin = worldRayHitInfo.hit.pos;
				if (worldRayHitInfo.tag.StartsWith("E_"))
				{
					EntityDrone component = worldRayHitInfo.transform.GetComponent<EntityDrone>();
					if ((bool)component && component.isAlly(holdingEntity as EntityPlayer))
					{
						lookRay.origin = worldRayHitInfo.hit.pos + lookRay.direction * 0.1f;
						i--;
						continue;
					}
					string bodyPartName;
					EntityAlive entityAlive2 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out bodyPartName) as EntityAlive;
					if (entityAlive == entityAlive2)
					{
						lookRay.origin = worldRayHitInfo.hit.pos + lookRay.direction * 0.1f;
						i--;
						continue;
					}
					holdingEntity.MinEventContext.Other = entityAlive2;
					entityAlive = entityAlive2;
					hitEntityFound = true;
				}
				else
				{
					BlockValue blockHit = ItemActionAttack.GetBlockHit(_actionData.invData.world, worldRayHitInfo);
					i += Mathf.FloorToInt((float)blockHit.Block.MaxDamage / (float)num2);
					holdingEntity.MinEventContext.BlockValue = blockHit;
				}
				float num3 = 1f;
				float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, itemValue, range, holdingEntity);
				if (worldRayHitInfo.hit.distanceSq > value * value)
				{
					num3 = 1f - (worldRayHitInfo.hit.distanceSq - value * value) / (range * range - value * value);
				}
				_actionData.attackDetails.isCriticalHit = holdingEntity.AimingGun;
				_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.RangedTag;
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayHit : MinEventTypes.onSelfSecondaryActionRayHit);
				float num4 = 1f;
				World world = GameManager.Instance.World;
				Vector3i pos = World.worldToBlockPos(worldRayHitInfo.hit.pos);
				WaterValue water = world.GetWater(pos);
				if (water.HasMass())
				{
					Vector3i pos2 = new Vector3i(pos.x, pos.y + 1, pos.z);
					if (world.GetWater(pos2).GetMassPercent() > 0f)
					{
						num4 = 0.25f;
					}
					else
					{
						float num5 = worldRayHitInfo.hit.pos.y - (float)pos.y;
						float num6 = water.GetMassPercent() * 0.6f - num5;
						if (num6 > 0f)
						{
							num4 = 1f - 0.75f * num6;
						}
					}
				}
				ItemActionAttack.Hit(worldRayHitInfo, holdingEntity.entityId, (DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Piercing : DamageType, GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, worldRayHitInfo), holdingEntity) * num3 * num4, GetDamageEntity(itemValue, holdingEntity) * num3 * num4, 1f, itemValue.PercentUsesLeft, _actionData.invData.item.CritChance.Value, ItemAction.GetDismemberChance(_actionData, worldRayHitInfo), bulletMaterialName, damageMultiplier, getBuffActions(_actionData), _actionData.attackDetails, 0, ActionExp, ActionExpBonusMultiplier, null, ToolBonuses, bSupportHarvesting ? EnumAttackMode.RealAndHarvesting : EnumAttackMode.RealNoHarvesting);
				if (bSupportHarvesting)
				{
					GameUtils.HarvestOnAttack(_actionData, ToolBonuses);
				}
			}
			else
			{
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayMiss : MinEventTypes.onSelfSecondaryActionRayMiss);
			}
		}
		return lookRay.direction;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getDamageBlock(ItemActionDataRanged _actionData)
	{
		return GetDamageBlock(_actionData.invData.itemValue, BlockValue.Air, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getDamageEntity(ItemActionDataRanged _actionData)
	{
		return GetDamageEntity(_actionData.invData.itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int GetActionEffectsValues(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		_startPos = Vector3.zero;
		_direction = Vector3.zero;
		return 0;
	}

	public override int GetInitialMeta(ItemValue _itemValue)
	{
		return (int)EffectManager.GetValue(PassiveEffects.MagazineSize, _itemValue, BulletsPerMagazine);
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		ItemActionDataRanged itemActionDataRanged = (ItemActionDataRanged)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		float num = (itemActionDataRanged.distance = GetRange(_actionData));
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2);
		int hitMask = ((hitmaskOverride == 0) ? 8 : hitmaskOverride);
		Ray ray = holdingEntity.GetLookRay();
		bool flag = Reloading(itemActionDataRanged);
		if (holdingEntity is EntityPlayer entityPlayer && itemActionDataRanged.Laser != null)
		{
			if (itemActionDataRanged.Laser.gameObject.activeInHierarchy)
			{
				if (!CharacterCameraAngleValid(_actionData, out var _))
				{
					ray = new Ray(_actionData.invData.holdingEntity.inventory.GetHoldingItemTransform().position + Origin.position, _actionData.invData.holdingEntity.inventory.GetHoldingItemTransform().forward);
				}
				if (Voxel.Raycast(_actionData.invData.world, ray, num, -538750997, hitMask, 0f))
				{
					WorldRayHitInfo updatedHitInfo = _actionData.GetUpdatedHitInfo();
					entityPlayer.SetLaserSightData(!flag, updatedHitInfo.hit.pos);
				}
				else
				{
					entityPlayer.SetLaserSightData(_laserSightActive: false, ray.origin);
				}
			}
			else
			{
				entityPlayer.SetLaserSightData(_laserSightActive: false, ray.origin);
			}
		}
		ray.direction = getDirectionOffset(itemActionDataRanged, ray.direction);
		bool num2 = Voxel.Raycast(_actionData.invData.world, ray, num, -538750997, hitMask, 0f);
		holdingEntity.SetModelLayer(modelLayer);
		if (num2)
		{
			WorldRayHitInfo updatedHitInfo = _actionData.GetUpdatedHitInfo();
			itemActionDataRanged.distance = Mathf.Sqrt(updatedHitInfo.hit.distanceSq);
			itemActionDataRanged.damageFalloffPercent = 1f;
			if (itemActionDataRanged.Laser != null)
			{
				itemActionDataRanged.Laser.position = updatedHitInfo.hit.pos - Origin.position;
				itemActionDataRanged.Laser.gameObject.SetActive(!flag);
			}
			float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _actionData.invData.itemValue, num, holdingEntity);
			if (updatedHitInfo.hit.distanceSq > value * value)
			{
				itemActionDataRanged.damageFalloffPercent = 1f - (updatedHitInfo.hit.distanceSq - value * value) / (num * num - value * value);
			}
			return updatedHitInfo;
		}
		return null;
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		base.GetItemValueActionInfo(ref _infoList, _itemValue, _xui, 0);
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblHandling"), EffectManager.GetValue(PassiveEffects.WeaponHandling, _itemValue, 0.1f, _xui.playerUI.entityPlayer)));
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblRPM"), EffectManager.GetValue(PassiveEffects.RoundsPerMinute, _itemValue, 60f / originalDelay, _xui.playerUI.entityPlayer)));
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeFalloffRange"), string.Format("{0} / {1} {2}", EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _itemValue, 0f, _xui.playerUI.entityPlayer).ToCultureInvariantString(), EffectManager.GetValue(PassiveEffects.MaxRange, _itemValue, Range, _xui.playerUI.entityPlayer).ToCultureInvariantString(), Localization.Get("lblAttributeFalloffRangeText"))));
	}
}
