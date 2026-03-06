using System.Collections;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class ItemActionZoom : ItemAction
{
	public class ItemActionDataZoom : ItemActionData
	{
		public float CurrentZoom;

		public Transform Scope;

		public bool bZoomInProgress;

		public float timeZoomStarted;

		public int layerBeforeSwitch;

		public bool HasScope;

		public Vector3 SightsCameraOffset;

		public Vector3 ScopeCameraOffset;

		public Texture2D ZoomOverlay;

		public string ZoomOverlayName;

		public int MaxZoomIn;

		public int MaxZoomOut;

		public int BaseFOV;

		public Coroutine aimingCoroutine;

		public bool aimingValue;

		public ItemActionDataZoom(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			if (_invData.model != null)
			{
				Scope = _invData.model.FindInChilds("Attachments");
				if (Scope == null)
				{
					Log.Error("Transform 'Attachments' not found in weapon prefab for {0}.", _invData.model.name);
				}
				else
				{
					Scope = Scope.Find("Scope");
					HasScope = Scope.childCount > 0;
				}
			}
			layerBeforeSwitch = -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string zoomTriggerEffectPullDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string zoomTriggerEffectShootDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string zoomTriggerEffectPullXb;

	[PublicizedFrom(EAccessModifier.Private)]
	public string zoomTriggerEffectShootXb;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("zoomTriggerEffectPullDualsense"))
		{
			zoomTriggerEffectPullDualsense = _props.Values["zoomTriggerEffectPullDualsense"];
		}
		else
		{
			zoomTriggerEffectPullDualsense = string.Empty;
		}
		if (_props.Values.ContainsKey("zoomTriggerEffectPullXb"))
		{
			zoomTriggerEffectPullXb = _props.Values["zoomTriggerEffectPullXb"];
		}
		else
		{
			zoomTriggerEffectPullXb = string.Empty;
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataZoom(_invData, _indexInEntityOfAction);
	}

	public override void OnModificationsChanged(ItemActionData _data)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_data;
		if (Properties != null && Properties.Values.ContainsKey("Zoom_overlay"))
		{
			itemActionDataZoom.ZoomOverlayName = _data.invData.itemValue.GetPropertyOverride("Zoom_overlay", Properties.Values["Zoom_overlay"]);
		}
		else
		{
			itemActionDataZoom.ZoomOverlayName = _data.invData.itemValue.GetPropertyOverride("Zoom_overlay", "");
		}
		if (itemActionDataZoom.ZoomOverlayName != "")
		{
			itemActionDataZoom.ZoomOverlay = DataLoader.LoadAsset<Texture2D>(itemActionDataZoom.ZoomOverlayName);
		}
		if (itemActionDataZoom.invData.holdingEntity as EntityPlayerLocal != null)
		{
			itemActionDataZoom.BaseFOV = (int)(itemActionDataZoom.invData.holdingEntity as EntityPlayerLocal).playerCamera.fieldOfView;
			itemActionDataZoom.MaxZoomOut = itemActionDataZoom.BaseFOV;
		}
		if (Properties != null && Properties.Values.ContainsKey("Zoom_max_out"))
		{
			itemActionDataZoom.MaxZoomOut = StringParsers.ParseSInt32(_data.invData.itemValue.GetPropertyOverride("Zoom_max_out", Properties.Values["Zoom_max_out"]));
		}
		else
		{
			itemActionDataZoom.MaxZoomOut = StringParsers.ParseSInt32(_data.invData.itemValue.GetPropertyOverride("Zoom_max_out", itemActionDataZoom.MaxZoomOut.ToString()));
		}
		if (Properties != null && Properties.Values.ContainsKey("Zoom_max_in"))
		{
			itemActionDataZoom.MaxZoomIn = StringParsers.ParseSInt32(_data.invData.itemValue.GetPropertyOverride("Zoom_max_in", Properties.Values["Zoom_max_in"]));
		}
		else
		{
			itemActionDataZoom.MaxZoomIn = StringParsers.ParseSInt32(_data.invData.itemValue.GetPropertyOverride("Zoom_max_in", itemActionDataZoom.MaxZoomOut.ToString()));
		}
		if (Properties != null && Properties.Values.ContainsKey("SightsCameraOffset"))
		{
			itemActionDataZoom.SightsCameraOffset = StringParsers.ParseVector3(itemActionDataZoom.invData.itemValue.GetPropertyOverride("SightsCameraOffset", Properties.Values["SightsCameraOffset"]));
		}
		else
		{
			itemActionDataZoom.SightsCameraOffset = StringParsers.ParseVector3(itemActionDataZoom.invData.itemValue.GetPropertyOverride("SightsCameraOffset", "0,0,0"));
		}
		if (Properties != null && Properties.Values.ContainsKey("ScopeCameraOffset"))
		{
			itemActionDataZoom.ScopeCameraOffset = StringParsers.ParseVector3(itemActionDataZoom.invData.itemValue.GetPropertyOverride("ScopeCameraOffset", Properties.Values["ScopeCameraOffset"]));
		}
		else
		{
			itemActionDataZoom.ScopeCameraOffset = StringParsers.ParseVector3(itemActionDataZoom.invData.itemValue.GetPropertyOverride("ScopeCameraOffset", "0,0,0"));
		}
		itemActionDataZoom.CurrentZoom = itemActionDataZoom.MaxZoomOut;
		if (itemActionDataZoom.invData.model != null && itemActionDataZoom.Scope != null)
		{
			itemActionDataZoom.HasScope = itemActionDataZoom.Scope.childCount > 0;
		}
		else if (itemActionDataZoom.invData.model != null)
		{
			itemActionDataZoom.Scope = itemActionDataZoom.invData.model.FindInChilds("Attachments");
			itemActionDataZoom.Scope = itemActionDataZoom.Scope.Find("Scope");
			itemActionDataZoom.HasScope = itemActionDataZoom.Scope.childCount > 0;
		}
		if (!itemActionDataZoom.HasScope)
		{
			ItemValue[] modifications = itemActionDataZoom.invData.itemValue.Modifications;
			foreach (ItemValue obj in modifications)
			{
				if (obj != null && obj.ItemClass?.HasAllTags(FastTags<TagGroup.Global>.Parse("scope")) == true)
				{
					itemActionDataZoom.HasScope = true;
					break;
				}
			}
		}
		if (Properties != null && Properties.Values.ContainsKey("zoomTriggerEffectPullDualsense"))
		{
			zoomTriggerEffectPullDualsense = _data.invData.itemValue.GetPropertyOverride("zoomTriggerEffectPullDualsense", "NoEffect");
		}
		if (Properties != null && Properties.Values.ContainsKey("zoomTriggerEffectPullXb"))
		{
			zoomTriggerEffectPullXb = _data.invData.itemValue.GetPropertyOverride("zoomTriggerEffectPullXb", "NoEffect");
		}
		if (Properties != null && Properties.Values.ContainsKey("zoomTriggerEffectShootDualsense"))
		{
			zoomTriggerEffectShootDualsense = _data.invData.itemValue.GetPropertyOverride("zoomTriggerEffectShootDualsense", "NoEffect");
		}
		if (Properties != null && Properties.Values.ContainsKey("zoomTriggerEffectShootXb"))
		{
			zoomTriggerEffectShootXb = _data.invData.itemValue.GetPropertyOverride("zoomTriggerEffectShootXb", "NoEffect");
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		if (_data.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.GetTriggerEffect((zoomTriggerEffectPullDualsense, zoomTriggerEffectPullXb)));
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		EntityPlayerLocal entityPlayerLocal = _data.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.NoneEffect);
			if (_data is ItemActionDataZoom itemActionDataZoom && itemActionDataZoom.invData.holdingEntity.AimingGun)
			{
				entityPlayerLocal.cameraTransform.GetComponent<Camera>().fieldOfView = itemActionDataZoom.BaseFOV;
			}
		}
	}

	public override void OnScreenOverlay(ItemActionData _actionData)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataZoom.invData.holdingEntity as EntityPlayerLocal;
		if (!(entityPlayerLocal != null))
		{
			return;
		}
		if (itemActionDataZoom.ZoomOverlay != null && !itemActionDataZoom.bZoomInProgress && _actionData.invData.holdingEntity.AimingGun)
		{
			if (itemActionDataZoom.Scope != null && (bool)entityPlayerLocal.playerCamera)
			{
				entityPlayerLocal.playerCamera.cullingMask = entityPlayerLocal.playerCamera.cullingMask & -1025;
				if (itemActionDataZoom.invData.holdingEntity.GetModelLayer() != 10)
				{
					itemActionDataZoom.layerBeforeSwitch = itemActionDataZoom.invData.holdingEntity.GetModelLayer();
					itemActionDataZoom.invData.holdingEntity.SetModelLayer(10, force: false, Utils.ExcludeLayerZoom);
					return;
				}
			}
			float num = itemActionDataZoom.ZoomOverlay.width;
			float num2 = (float)Screen.height * 0.95f;
			num *= num2 / (float)itemActionDataZoom.ZoomOverlay.height;
			int num3 = (int)(((float)Screen.width - num) / 2f);
			int num4 = (int)(((float)Screen.height - num2) / 2f);
			GUIUtils.DrawFilledRect(new Rect(0f, 0f, Screen.width, num4), Color.black, _bDrawBorder: false, Color.black);
			GUIUtils.DrawFilledRect(new Rect(0f, 0f, num3, Screen.height), Color.black, _bDrawBorder: false, Color.black);
			GUIUtils.DrawFilledRect(new Rect((float)num3 + num, 0f, Screen.width, (float)num4 + num2), Color.black, _bDrawBorder: false, Color.black);
			GUIUtils.DrawFilledRect(new Rect(0f, (float)num4 + num2, Screen.width, Screen.height), Color.black, _bDrawBorder: false, Color.black);
			Graphics.DrawTexture(new Rect(num3, num4, num, num2), itemActionDataZoom.ZoomOverlay);
			if (entityPlayerLocal != null && !entityPlayerLocal.bFirstPersonView && entityPlayerLocal.emodel.visible)
			{
				entityPlayerLocal.emodel.SetVisible(_bVisible: false, _isKeepColliders: true);
			}
		}
		else if (!entityPlayerLocal.bFirstPersonView && !entityPlayerLocal.emodel.visible)
		{
			entityPlayerLocal.emodel.SetVisible(_bVisible: true, _isKeepColliders: true);
		}
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		if (!_actionData.invData.holdingEntity.AimingGun)
		{
			return false;
		}
		if (_scrollWheelInput == 0f)
		{
			return false;
		}
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		if (!itemActionDataZoom.bZoomInProgress)
		{
			itemActionDataZoom.CurrentZoom = Utils.FastClamp(itemActionDataZoom.CurrentZoom + _scrollWheelInput * -25f, itemActionDataZoom.MaxZoomIn, itemActionDataZoom.MaxZoomOut);
			((EntityPlayerLocal)_actionData.invData.holdingEntity).cameraTransform.GetComponent<Camera>().fieldOfView = (int)itemActionDataZoom.CurrentZoom;
		}
		return true;
	}

	public override bool ConsumeCameraFunction(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.AimingGun)
		{
			return false;
		}
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		if (itemActionDataZoom.MaxZoomIn == itemActionDataZoom.MaxZoomOut)
		{
			return false;
		}
		if (!itemActionDataZoom.bZoomInProgress)
		{
			if (itemActionDataZoom.CurrentZoom == (float)itemActionDataZoom.MaxZoomIn)
			{
				itemActionDataZoom.CurrentZoom = itemActionDataZoom.MaxZoomOut;
			}
			else
			{
				itemActionDataZoom.CurrentZoom = Utils.FastClamp(itemActionDataZoom.CurrentZoom - 5f, itemActionDataZoom.MaxZoomIn, itemActionDataZoom.MaxZoomOut);
			}
			((EntityPlayerLocal)_actionData.invData.holdingEntity).cameraTransform.GetComponent<Camera>().fieldOfView = (int)itemActionDataZoom.CurrentZoom;
			return true;
		}
		return false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		bool flag = !_bReleased && itemActionDataZoom.invData.holdingEntity.IsAimingGunPossible();
		EntityPlayerLocal entityPlayerLocal = itemActionDataZoom.invData.holdingEntity as EntityPlayerLocal;
		if ((bool)entityPlayerLocal)
		{
			if (!entityPlayerLocal.IsCameraAttachedToPlayerOrScope() && entityPlayerLocal.bFirstPersonView)
			{
				return;
			}
			if (entityPlayerLocal.movementInput.running && !_bReleased)
			{
				entityPlayerLocal.MoveController.ForceStopRunning();
			}
			bool flag2 = ((entityPlayerLocal.playerInput.LastDeviceClass == InputDeviceClass.Controller) ? GamePrefs.GetBool(EnumGamePrefs.OptionsControllerWeaponAiming) : GamePrefs.GetBool(EnumGamePrefs.OptionsWeaponAiming));
			if (_bReleased && flag2 && ((itemActionDataZoom.aimingCoroutine != null && itemActionDataZoom.aimingValue) || entityPlayerLocal.bLerpCameraFlag))
			{
				return;
			}
		}
		if (itemActionDataZoom.aimingCoroutine != null)
		{
			GameManager.Instance.StopCoroutine(itemActionDataZoom.aimingCoroutine);
			itemActionDataZoom.aimingCoroutine = null;
		}
		if (itemActionDataZoom.invData.holdingEntity.AimingGun != flag)
		{
			itemActionDataZoom.aimingValue = flag;
			itemActionDataZoom.aimingCoroutine = GameManager.Instance.StartCoroutine(startEndZoomLater(itemActionDataZoom));
			if (!_bReleased && (bool)entityPlayerLocal && entityPlayerLocal.movementInput.lastInputController)
			{
				entityPlayerLocal.MoveController.FindCameraSnapTarget(eCameraSnapMode.Zoom, 50f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startEndZoomLater(ItemActionDataZoom _actionData)
	{
		yield return new WaitForSecondsRealtime(0f);
		_actionData.invData.holdingEntity.AimingGun = _actionData.aimingValue;
	}

	public override void AimingSet(ItemActionData _actionData, bool _isAiming, bool _wasAiming)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		if (itemActionDataZoom.aimingCoroutine != null)
		{
			GameManager.Instance.StopCoroutine(itemActionDataZoom.aimingCoroutine);
			itemActionDataZoom.aimingCoroutine = null;
		}
		if (_isAiming != _wasAiming)
		{
			startEndZoom(itemActionDataZoom, _isAiming);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startEndZoom(ItemActionDataZoom _actionData, bool _isAiming)
	{
		if (_isAiming)
		{
			if (!_actionData.bZoomInProgress && !string.IsNullOrEmpty(_actionData.invData.item.soundSightIn))
			{
				Manager.BroadcastPlay(_actionData.invData.item.soundSightIn);
			}
			_actionData.timeZoomStarted = Time.time;
			_actionData.bZoomInProgress = true;
			return;
		}
		if (_actionData.layerBeforeSwitch != -1)
		{
			_actionData.invData.holdingEntity.SetModelLayer(_actionData.layerBeforeSwitch);
			_actionData.layerBeforeSwitch = -1;
		}
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_actionData.invData.holdingEntity;
		if (_actionData.Scope != null && (bool)entityPlayerLocal.playerCamera)
		{
			entityPlayerLocal.playerCamera.cullingMask = entityPlayerLocal.playerCamera.cullingMask | 0x400;
		}
		if (!_actionData.bZoomInProgress && !string.IsNullOrEmpty(_actionData.invData.item.soundSightOut))
		{
			Manager.BroadcastPlay(_actionData.invData.item.soundSightOut);
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		if (itemActionDataZoom.bZoomInProgress && Time.time - itemActionDataZoom.timeZoomStarted < 0.3f)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDataZoom itemActionDataZoom = (ItemActionDataZoom)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataZoom.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			bool flag = ((itemActionDataZoom.aimingCoroutine != null) ? itemActionDataZoom.aimingValue : entityPlayerLocal.AimingGun);
			if (!entityPlayerLocal.movementInput.running && !flag && !entityPlayerLocal.bLerpCameraFlag)
			{
				itemActionDataZoom.HasExecuted = false;
			}
			vp_FPWeapon vp_FPWeapon2 = entityPlayerLocal.vp_FPWeapon;
			if (vp_FPWeapon2 != null)
			{
				if (_actionData.invData.holdingEntity.AimingGun)
				{
					if (itemActionDataZoom.HasScope)
					{
						vp_FPWeapon2.AimingPositionOffset = itemActionDataZoom.ScopeCameraOffset;
					}
					else
					{
						vp_FPWeapon2.AimingPositionOffset = itemActionDataZoom.SightsCameraOffset;
					}
					vp_FPWeapon2.RenderingFieldOfView = StringParsers.ParseSInt32(_actionData.invData.itemValue.GetPropertyOverride("WeaponCameraFOV", vp_FPWeapon2.originalRenderingFieldOfView.ToCultureInvariantString()));
				}
				else
				{
					vp_FPWeapon2.AimingPositionOffset = Vector3.zero;
					vp_FPWeapon2.RenderingFieldOfView = vp_FPWeapon2.originalRenderingFieldOfView;
				}
				vp_FPWeapon2.Refresh();
			}
		}
		if (itemActionDataZoom.bZoomInProgress && !(Time.time - itemActionDataZoom.timeZoomStarted < 0.15f))
		{
			itemActionDataZoom.bZoomInProgress = false;
			if (_actionData.invData.holdingEntity.AimingGun && (bool)entityPlayerLocal)
			{
				entityPlayerLocal.cameraTransform.GetComponent<Camera>().fieldOfView = (int)itemActionDataZoom.CurrentZoom;
			}
		}
	}

	public override bool IsHUDDisabled(ItemActionData _data)
	{
		if (base.ZoomOverlay != null && !_data.invData.holdingEntity.isEntityRemote && _data.invData.holdingEntity.AimingGun)
		{
			return !((ItemActionDataZoom)_data).bZoomInProgress;
		}
		return false;
	}

	public override void GetIronSights(ItemActionData _actionData, out float _fov)
	{
		_fov = ((base.ZoomOverlay == null) ? ((ItemActionDataZoom)_actionData).CurrentZoom : 0f);
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		if (base.ZoomOverlay != null && _actionData.invData.holdingEntity.AimingGun)
		{
			return EnumCameraShake.Big;
		}
		if (_actionData.invData.holdingEntity.AimingGun)
		{
			return EnumCameraShake.Tiny;
		}
		return EnumCameraShake.Small;
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		return TriggerEffectManager.GetTriggerEffect((zoomTriggerEffectPullDualsense, zoomTriggerEffectPullXb));
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		return TriggerEffectManager.GetTriggerEffect((zoomTriggerEffectShootDualsense, zoomTriggerEffectShootXb));
	}

	public override bool AllowConcurrentActions()
	{
		return true;
	}
}
