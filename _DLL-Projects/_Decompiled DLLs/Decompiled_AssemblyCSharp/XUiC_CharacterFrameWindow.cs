using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterFrameWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> displayInfoEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> currentMovementTag = EntityAlive.MovementTagRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform characterPivot;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DragRotateSpeed = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ControllerRotateSpeed = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMouseOverPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lastMousePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RotationStickDeadzone = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool InvertRotationStick = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cursorLockActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lockedCursorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool prevCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerDeathsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerFoodFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerWaterFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerFoodMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerWaterMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerItemsCraftedFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerPvpKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerZombieKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerXpToNextLevelFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerArmorRatingFormatter = new CachedStringFormatterInt();

	public override void Init()
	{
		base.Init();
		tabs = GetChildByType<XUiC_TabSelector>();
		previewFrame = GetChildById("previewFrameSDCS");
		if (previewFrame != null)
		{
			previewFrame.OnPress += PreviewFrame_OnPress;
			previewFrame.OnHover += PreviewFrame_OnHover;
		}
		lblLevel = (XUiV_Label)GetChildById("levelNumber").ViewComponent;
		lblName = (XUiV_Label)GetChildById("characterName").ViewComponent;
		textPreview = (XUiV_Texture)GetChildById("playerPreviewSDCS").ViewComponent;
		isDirty = true;
		if (GetChildById("coreStatsMovementModeToggle") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += coreStatsMovementModeToggle_OnPress;
		}
		if (GetChildById("btnCosmetics") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += btnCosmetic_OnPress;
		}
		XUiC_EquipmentStackGrid childByType = GetChildByType<XUiC_EquipmentStackGrid>();
		if (childByType != null)
		{
			XUiController childById = GetChildById("badgeSlots");
			if (childById != null)
			{
				childByType.RegisterExtraSlots(childById.GetChildrenByType<XUiC_EquipmentStack>());
			}
			XUiController childById2 = GetChildById("clothingSlots");
			if (childById2 != null)
			{
				childByType.RegisterExtraSlots(childById2.GetChildrenByType<XUiC_EquipmentStack>());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCosmetic_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_CharacterCosmeticWindowGroup.Open(base.xui, EquipmentSlots.Head);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void coreStatsMovementModeToggle_OnPress(XUiController _sender, int _mouseButton)
	{
		if (currentMovementTag.Equals(EntityAlive.MovementTagIdle))
		{
			currentMovementTag = EntityAlive.MovementTagWalking;
		}
		else if (currentMovementTag.Equals(EntityAlive.MovementTagWalking))
		{
			currentMovementTag = EntityAlive.MovementTagRunning;
		}
		else
		{
			currentMovementTag = EntityAlive.MovementTagIdle;
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		isMouseOverPreview = _isOver;
		LocalPlayerUI.IsOverPagingOverrideElement = _isOver;
		if (_isOver)
		{
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
		}
		else
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.dragAndDrop.CurrentStack != ItemStack.Empty)
		{
			ItemStack itemStack = base.xui.PlayerEquipment.EquipItem(base.xui.dragAndDrop.CurrentStack);
			if (base.xui.dragAndDrop.CurrentStack != itemStack)
			{
				base.xui.dragAndDrop.CurrentStack = itemStack;
				base.xui.dragAndDrop.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
			}
		}
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		string text = tabs?.SelectedTab?.TabKey;
		if (text != "armor" && text != "clothing" && text != "gear" && Time.time > updateTime)
		{
			updateTime = Time.time + 0.25f;
			RefreshBindings(isDirty);
		}
		if (isDirty)
		{
			if (player == null)
			{
				return;
			}
			lblLevel.Text = string.Format(Localization.Get("lblLevel"), player.Progression.GetLevel());
			lblName.Text = player.PlayerDisplayName;
			isDirty = false;
			RefreshBindings();
		}
		if (isPreviewDirty)
		{
			MakePreview();
		}
		textPreview.Texture = renderTextureSystem.RenderTex;
		if (characterPivot != null)
		{
			PlayerActionsGUI playerActionsGUI = base.xui?.playerUI?.playerInput?.GUIActions;
			if (playerActionsGUI != null)
			{
				bool num = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
				base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
				bool flag = false;
				if (num && isMouseOverPreview)
				{
					flag = true;
					float x = playerActionsGUI.Camera.Value.x;
					if (Mathf.Abs(x) > 0.15f)
					{
						float num2 = -1f;
						characterPivot.Rotate(Vector3.up, num2 * x * 200f * _dt, Space.World);
					}
				}
				if (cursorLockActive)
				{
					if (flag)
					{
						MaintainCursorLock();
					}
					else
					{
						EndCursorLock();
					}
				}
			}
			bool mouseButton = base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			bool mouseButton2 = base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (mouseButton || mouseButton2)
			{
				if (!isDragging && isMouseOverPreview)
				{
					isDragging = true;
					lastMousePos = Input.mousePosition;
				}
				if (isDragging)
				{
					Vector2 vector = Input.mousePosition;
					Vector2 vector2 = vector - lastMousePos;
					lastMousePos = vector;
					if (Mathf.Abs(vector2.x) > 0.01f)
					{
						float num3 = vector2.x * 0.4f;
						characterPivot.Rotate(Vector3.up, 0f - num3, Space.World);
					}
				}
			}
			else
			{
				isDragging = false;
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
		isPreviewDirty = true;
		player = base.xui.playerUI.entityPlayer;
		if (previewFrame != null)
		{
			previewFrame.OnPress -= PreviewFrame_OnPress;
			previewFrame.OnHover -= PreviewFrame_OnHover;
		}
		previewFrame = GetChildById("previewFrameSDCS");
		if (previewFrame != null)
		{
			previewFrame.OnPress += PreviewFrame_OnPress;
			previewFrame.OnHover += PreviewFrame_OnHover;
		}
		if (renderTextureSystem.ParentGO == null)
		{
			renderTextureSystem.Create("playerpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), textPreview.Size, _isAA: true);
		}
		renderCamera = renderTextureSystem.CameraGO.GetComponent<Camera>();
		renderCamera.transform.localPosition = new Vector3(0f, 1.5f, 0f);
		renderCamera.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
		renderCamera.orthographic = false;
		renderCamera.fieldOfView = 54f;
		renderCamera.renderingPath = RenderingPath.DeferredShading;
		if (characterPivot == null)
		{
			GameObject gameObject = new GameObject("CharacterPivot");
			characterPivot = gameObject.transform;
			characterPivot.SetParent(renderTextureSystem.ParentGO.transform, worldPositionStays: false);
			characterPivot.localPosition = new Vector3(0f, 0f, 2.15f);
			characterPivot.localRotation = Quaternion.AngleAxis(-30f, Vector3.up);
		}
		renderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
		GameObject gameObject2 = new GameObject("Key Light", typeof(Light));
		gameObject2.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject2.transform.SetPositionAndRotation(new Vector3(1.5f, 2.5f, -1.5f), Quaternion.Euler(20f, -20f, 0f));
		Light component = gameObject2.GetComponent<Light>();
		component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
		component.type = LightType.Spot;
		component.range = 20f;
		component.spotAngle = 60f;
		component.intensity = 1.5f;
		component.shadows = LightShadows.Hard;
		component.shadowStrength = 0.2f;
		component.shadowBias = 0.005f;
		component.cullingMask = 2048;
		GameObject gameObject3 = new GameObject("Fill Light", typeof(Light));
		gameObject3.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject3.transform.SetPositionAndRotation(new Vector3(-2f, 3f, 0f), Quaternion.Euler(35f, 45f, 0f));
		Light component2 = gameObject3.GetComponent<Light>();
		component2.color = new Color(1f, 1f, 1f, 1f);
		component2.type = LightType.Spot;
		component2.range = 20f;
		component2.spotAngle = 60f;
		component2.intensity = 0.5f;
		component2.shadows = LightShadows.Hard;
		component2.shadowStrength = 0.2f;
		component2.shadowBias = 0.005f;
		component2.cullingMask = 2048;
		GameObject gameObject4 = new GameObject("Fill 2 Light", typeof(Light));
		gameObject4.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject4.transform.SetPositionAndRotation(new Vector3(0f, -0.5f, -1.5f), Quaternion.Euler(-20f, 0f, 0f));
		Light component3 = gameObject4.GetComponent<Light>();
		component3.color = new Color(1f, 1f, 1f, 1f);
		component3.type = LightType.Spot;
		component3.range = 20f;
		component3.spotAngle = 60f;
		component3.intensity = 0.5f;
		component3.shadows = LightShadows.Hard;
		component3.shadowStrength = 0.2f;
		component3.shadowBias = 0.005f;
		component3.cullingMask = 2048;
		GameObject gameObject5 = new GameObject("Back Light", typeof(Light));
		gameObject5.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject5.transform.SetPositionAndRotation(new Vector3(-2f, 5f, 2f), Quaternion.Euler(60f, 105f, 0f));
		Light component4 = gameObject5.GetComponent<Light>();
		component4.color = new Color(0.4f, 0.75f, 1f, 1f);
		component4.type = LightType.Spot;
		component4.spotAngle = 60f;
		component4.range = 20f;
		component4.intensity = 1.5f;
		component4.shadows = LightShadows.Hard;
		component4.shadowStrength = 0.2f;
		component4.shadowBias = 0.005f;
		component4.cullingMask = 2048;
		displayInfoEntries = UIDisplayInfoManager.Current.GetCharacterDisplayInfo();
		if (player as EntityPlayerLocal != null && player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
		}
		XUiM_PlayerEquipment.SlotChanged += XUiM_PlayerEquipmentOnSlotChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipmentOnSlotChanged(XUiM_PlayerEquipment _playerEquipment, EquipmentSlots _slot, ItemStack _newStack)
	{
		if (tabs != null)
		{
			if (_slot <= EquipmentSlots.Feet)
			{
				tabs.SelectTabByName("armor");
			}
			else if (_slot >= EquipmentSlots.BiomeBadge && _slot <= EquipmentSlots.BiomeBadge4)
			{
				tabs.SelectTabByName("gear");
			}
			else if (_slot >= EquipmentSlots.ClothingHead && _slot <= EquipmentSlots.ClothingFeet)
			{
				tabs.SelectTabByName("clothing");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
		if (base.IsOpen)
		{
			MakePreview();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
		XUiM_PlayerEquipment.SlotChanged -= XUiM_PlayerEquipmentOnSlotChanged;
		SDCSUtils.DestroyViz(previewSDCSObj);
		renderTextureSystem.Cleanup();
		EndCursorLock();
		characterPivot = null;
		isMouseOverPreview = false;
		isDragging = false;
		LocalPlayerUI.IsOverPagingOverrideElement = false;
		if (previewFrame != null)
		{
			previewFrame.OnPress -= PreviewFrame_OnPress;
			previewFrame.OnHover -= PreviewFrame_OnHover;
			previewFrame = null;
		}
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	public void MakePreview()
	{
		if (!(player == null) && !(player.emodel == null) && player.emodel is EModelSDCS eModelSDCS)
		{
			isPreviewDirty = false;
			SDCSUtils.CreateVizUI(eModelSDCS.Archetype, ref previewSDCSObj, ref transformCatalog, player, useTempCosmetics: false);
			Utils.SetLayerRecursively(previewSDCSObj, 11);
			Transform transform = previewSDCSObj.transform;
			transform.SetParent((characterPivot != null) ? characterPivot : renderTextureSystem.ParentGO.transform, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			CharacterGazeController componentInChildren = previewSDCSObj.GetComponentInChildren<CharacterGazeController>();
			if ((bool)componentInChildren && renderCamera != null)
			{
				componentInChildren.LookAtTransformOverride = renderCamera.transform;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BeginCursorLock()
	{
		if (!cursorLockActive)
		{
			cursorLockActive = true;
			lockedCursorPos = base.xui.playerUI.CursorController.GetScreenPosition();
			prevCursorHidden = base.xui.playerUI.CursorController.VirtualCursorHidden;
			base.xui.playerUI.CursorController.VirtualCursorHidden = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaintainCursorLock()
	{
		base.xui.playerUI.CursorController.SetScreenPosition(lockedCursorPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndCursorLock()
	{
		if (cursorLockActive)
		{
			cursorLockActive = false;
			base.xui.playerUI.CursorController.VirtualCursorHidden = prevCursorHidden;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "movementmode":
			_value = (currentMovementTag.Equals(EntityAlive.MovementTagIdle) ? "Idle" : (currentMovementTag.Equals(EntityAlive.MovementTagWalking) ? "Walking" : "Running"));
			return true;
		case "playercoretemp":
			_value = ((player != null) ? XUiM_Player.GetCoreTemp(player) : "");
			return true;
		case "playercoretemptitle":
			_value = Localization.Get("xuiFeelsLike");
			return true;
		case "playerdeaths":
			_value = ((player != null) ? playerDeathsFormatter.Format(XUiM_Player.GetDeaths(player)) : "");
			return true;
		case "playerdeathstitle":
			_value = Localization.Get("xuiDeaths");
			return true;
		case "playerhealth":
			_value = ((player != null) ? playerHealthFormatter.Format((int)XUiM_Player.GetHealth(player)) : "");
			return true;
		case "playermaxhealth":
			_value = ((player != null) ? playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(player)) : "");
			return true;
		case "playerhealthtitle":
			_value = Localization.Get("lblHealth");
			return true;
		case "playermaxstamina":
			_value = ((player != null) ? playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(player)) : "");
			return true;
		case "playerstaminatitle":
			_value = Localization.Get("lblStamina");
			return true;
		case "playerstamina":
			_value = ((player != null) ? playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(player)) : "");
			return true;
		case "playerwater":
			_value = ((player != null) ? playerWaterFormatter.Format(XUiM_Player.GetWater(player)) : "");
			return true;
		case "playermodifiedcurrentwater":
			_value = ((player != null) ? playerWaterFormatter.Format(XUiM_Player.GetModifiedCurrentWater(player)) : "");
			return true;
		case "playerwatermax":
			_value = ((player != null) ? playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(player)) : "");
			return true;
		case "playerwatertitle":
			_value = Localization.Get("xuiWater");
			return true;
		case "playerfood":
			_value = ((player != null) ? playerFoodFormatter.Format(XUiM_Player.GetFood(player)) : "");
			return true;
		case "playermodifiedcurrentfood":
			_value = ((player != null) ? playerFoodFormatter.Format(XUiM_Player.GetModifiedCurrentFood(player)) : "");
			return true;
		case "playerfoodmax":
			_value = ((player != null) ? playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(player)) : "");
			return true;
		case "playerfoodtitle":
			_value = Localization.Get("xuiFood");
			return true;
		case "playeritemscrafted":
			_value = ((player != null) ? playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(player)) : "");
			return true;
		case "playeritemscraftedtitle":
			_value = Localization.Get("xuiItemsCrafted");
			return true;
		case "playerlongestlife":
			_value = ((player != null) ? XUiM_Player.GetLongestLife(player) : "");
			return true;
		case "playerlongestlifetitle":
			_value = Localization.Get("xuiLongestLife");
			return true;
		case "playercurrentlife":
			_value = ((player != null) ? XUiM_Player.GetCurrentLife(player) : "");
			return true;
		case "playercurrentlifetitle":
			_value = Localization.Get("xuiCurrentLife");
			return true;
		case "playerpvpkills":
			_value = ((player != null) ? playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(player)) : "");
			return true;
		case "playerpvpkillstitle":
			_value = Localization.Get("xuiPlayerKills");
			return true;
		case "playertravelled":
			_value = ((player != null) ? XUiM_Player.GetKMTraveled(player) : "");
			return true;
		case "playertravelledtitle":
			_value = Localization.Get("xuiKMTravelled");
			return true;
		case "playerxptonextlevel":
			_value = ((player != null) ? playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(player) + player.Progression.ExpDeficit) : "");
			return true;
		case "playerxptonextleveltitle":
			_value = Localization.Get("xuiXPToNextLevel");
			return true;
		case "playerzombiekills":
			_value = ((player != null) ? playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(player)) : "");
			return true;
		case "playerzombiekillstitle":
			_value = Localization.Get("xuiZombieKills");
			return true;
		case "playerarmorratingtitle":
			_value = Localization.Get("statPhysicalDamageResist");
			return true;
		case "playerarmorrating":
			_value = ((player != null) ? playerArmorRatingFormatter.Format((int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, player)) : "");
			return true;
		case "playergamestagetitle":
			_value = Localization.Get("xuiGameStage");
			return true;
		case "playergamestage":
			_value = ((player != null) ? player.gameStage.ToString() : "");
			return true;
		case "playerlootstagetitle":
			_value = Localization.Get("xuiLootstage");
			return true;
		case "playerlootstage":
			_value = ((player != null) ? player.GetLootStage(0f, 0f).ToString() : "");
			return true;
		default:
			if (_bindingName.StartsWith("playerstattitle"))
			{
				if (player != null)
				{
					int index = Convert.ToInt32(_bindingName.Replace("playerstattitle", "")) - 1;
					_value = GetStatTitle(index);
				}
				else
				{
					_value = "";
				}
				return true;
			}
			if (_bindingName.StartsWith("playerstat"))
			{
				if (player != null)
				{
					int index2 = Convert.ToInt32(_bindingName.Replace("playerstat", "")) - 1;
					_value = GetStatValue(index2);
				}
				else
				{
					_value = "";
				}
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int _index)
	{
		if (displayInfoEntries.Count <= _index)
		{
			return "";
		}
		if (displayInfoEntries[_index].TitleOverride != null)
		{
			return displayInfoEntries[_index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(displayInfoEntries[_index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int _index)
	{
		if (displayInfoEntries.Count <= _index)
		{
			return "";
		}
		DisplayInfoEntry displayInfoEntry = displayInfoEntries[_index];
		return XUiM_Player.GetStatValue(displayInfoEntry.StatType, base.xui.playerUI.entityPlayer, displayInfoEntry, currentMovementTag);
	}
}
