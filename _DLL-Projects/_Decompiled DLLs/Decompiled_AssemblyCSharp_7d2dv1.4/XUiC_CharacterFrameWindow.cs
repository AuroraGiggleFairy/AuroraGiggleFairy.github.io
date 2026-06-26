using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterFrameWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Tabs
	{
		Character,
		Stats,
		CoreStats
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController characterButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController coreStatsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Tabs currentTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer ep;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;

	public RuntimeAnimatorController animationController;

	public float atlasResolutionScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string levelLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> displayInfoEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack WeatherSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

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
		previewFrame = GetChildById("playerPreviewSDCS");
		previewFrame.OnPress += PreviewFrame_OnPress;
		previewFrame.OnHover += PreviewFrame_OnHover;
		lblLevel = (XUiV_Label)GetChildById("levelNumber").ViewComponent;
		lblName = (XUiV_Label)GetChildById("characterName").ViewComponent;
		textPreview = (XUiV_Texture)GetChildById("playerPreviewSDCS").ViewComponent;
		isDirty = true;
		characterButton = GetChildById("characterButton");
		if (characterButton != null)
		{
			characterButton.OnPress += CharacterButton_OnPress;
		}
		statsButton = GetChildById("statButton");
		if (statsButton != null)
		{
			statsButton.OnPress += StatsButton_OnPress;
		}
		coreStatsButton = GetChildById("coreStatButton");
		if (coreStatsButton != null)
		{
			coreStatsButton.OnPress += CoreStatsButton_OnPress;
		}
		XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
		levelLabel = Localization.Get("lblLevel");
		WeatherSlot = GetChildById("weatherSlot") as XUiC_EquipmentStack;
		XUiC_EquipmentStackGrid childByType = GetChildByType<XUiC_EquipmentStackGrid>();
		if (childByType != null)
		{
			childByType.ExtraSlot = WeatherSlot;
		}
		base.xui.playerUI.OnUIShutdown += HandleUIShutdown;
		base.xui.OnShutdown += HandleUIShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatsButton_OnPress(XUiController _sender, int _mouseButton)
	{
		currentTab = Tabs.Stats;
		if (characterButton != null)
		{
			((XUiV_Button)characterButton.ViewComponent).Selected = false;
		}
		if (statsButton != null)
		{
			((XUiV_Button)statsButton.ViewComponent).Selected = true;
		}
		if (coreStatsButton != null)
		{
			((XUiV_Button)coreStatsButton.ViewComponent).Selected = false;
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CharacterButton_OnPress(XUiController _sender, int _mouseButton)
	{
		currentTab = Tabs.Character;
		if (characterButton != null)
		{
			((XUiV_Button)characterButton.ViewComponent).Selected = true;
		}
		if (statsButton != null)
		{
			((XUiV_Button)statsButton.ViewComponent).Selected = false;
		}
		if (coreStatsButton != null)
		{
			((XUiV_Button)coreStatsButton.ViewComponent).Selected = false;
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CoreStatsButton_OnPress(XUiController _sender, int _mouseButton)
	{
		currentTab = Tabs.CoreStats;
		if (characterButton != null)
		{
			((XUiV_Button)characterButton.ViewComponent).Selected = false;
		}
		if (statsButton != null)
		{
			((XUiV_Button)statsButton.ViewComponent).Selected = false;
		}
		if (coreStatsButton != null)
		{
			((XUiV_Button)coreStatsButton.ViewComponent).Selected = true;
		}
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUIShutdown()
	{
		base.xui.playerUI.OnUIShutdown -= HandleUIShutdown;
		base.xui.OnShutdown -= HandleUIShutdown;
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		renderTextureSystem.RotateTarget(Time.deltaTime * 10f);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (ep == null)
		{
			ep = base.xui.playerUI.entityPlayer;
		}
		if (currentTab != Tabs.Character && Time.time > updateTime)
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
			if (WeatherSlot != null)
			{
				WeatherSlot.EquipSlot = EquipmentSlots.WeatherKit;
			}
			lblLevel.Text = string.Format(levelLabel, player.Progression.GetLevel());
			lblName.Text = player.PlayerDisplayName;
			isDirty = false;
			RefreshBindings();
		}
		if (isPreviewDirty)
		{
			MakePreview();
		}
		textPreview.Texture = renderTextureSystem.RenderTex;
		if (previewSDCSObj != null)
		{
			previewSDCSObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
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
		previewFrame.OnPress += PreviewFrame_OnPress;
		previewFrame.OnHover += PreviewFrame_OnHover;
		textPreview = (XUiV_Texture)GetChildById("playerPreviewSDCS").ViewComponent;
		if (renderTextureSystem.ParentGO == null)
		{
			renderTextureSystem.Create("playerpreview", new GameObject(), new Vector3(0f, -0.5f, 3f), new Vector3(0f, -0.2f, 7.5f), textPreview.Size, _isAA: true);
		}
		displayInfoEntries = UIDisplayInfoManager.Current.GetCharacterDisplayInfo();
		if (player as EntityPlayerLocal != null && player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment1(XUiM_PlayerEquipment playerEquipment)
	{
		if (base.IsOpen)
		{
			MakePreview();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment1;
		SDCSUtils.DestroyViz(previewSDCSObj);
		renderTextureSystem.Cleanup();
	}

	public void MakePreview()
	{
		if (!(ep == null) && !(ep.emodel == null) && ep.emodel is EModelSDCS eModelSDCS)
		{
			isPreviewDirty = false;
			SDCSUtils.CreateVizUI(eModelSDCS.Archetype, ref previewSDCSObj, ref transformCatalog, ep);
			Utils.SetLayerRecursively(previewSDCSObj, 11);
			Transform transform = previewSDCSObj.transform;
			transform.SetParent(renderTextureSystem.ParentGO.transform, worldPositionStays: false);
			transform.localPosition = new Vector3(0.022f, -2.9f, 12f);
			transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			renderTextureSystem.SetOrtho(enabled: true, 0.95f);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "showcharactersdcs":
			value = (currentTab == Tabs.Character && player != null && player.emodel as EModelSDCS != null).ToString();
			return true;
		case "showstats":
			value = (currentTab == Tabs.Stats).ToString();
			return true;
		case "showcore":
			value = (currentTab == Tabs.CoreStats).ToString();
			return true;
		case "playercoretemp":
			value = ((player != null) ? XUiM_Player.GetCoreTemp(player) : "");
			return true;
		case "playercoretemptitle":
			value = Localization.Get("xuiFeelsLike");
			return true;
		case "playerdeaths":
			value = ((player != null) ? playerDeathsFormatter.Format(XUiM_Player.GetDeaths(player)) : "");
			return true;
		case "playerdeathstitle":
			value = Localization.Get("xuiDeaths");
			return true;
		case "playerhealth":
			value = ((player != null) ? playerHealthFormatter.Format((int)XUiM_Player.GetHealth(player)) : "");
			return true;
		case "playermaxhealth":
			value = ((player != null) ? playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(player)) : "");
			return true;
		case "playerhealthtitle":
			value = Localization.Get("lblHealth");
			return true;
		case "playermaxstamina":
			value = ((player != null) ? playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(player)) : "");
			return true;
		case "playerstaminatitle":
			value = Localization.Get("lblStamina");
			return true;
		case "playerstamina":
			value = ((player != null) ? playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(player)) : "");
			return true;
		case "playerwater":
			value = ((player != null) ? playerWaterFormatter.Format(XUiM_Player.GetWater(player)) : "");
			return true;
		case "playermodifiedcurrentwater":
			value = ((player != null) ? playerWaterFormatter.Format(XUiM_Player.GetModifiedCurrentWater(player)) : "");
			return true;
		case "playerwatermax":
			value = ((player != null) ? playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(player)) : "");
			return true;
		case "playerwatertitle":
			value = Localization.Get("xuiWater");
			return true;
		case "playerfood":
			value = ((player != null) ? playerFoodFormatter.Format(XUiM_Player.GetFood(player)) : "");
			return true;
		case "playermodifiedcurrentfood":
			value = ((player != null) ? playerFoodFormatter.Format(XUiM_Player.GetModifiedCurrentFood(player)) : "");
			return true;
		case "playerfoodmax":
			value = ((player != null) ? playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(player)) : "");
			return true;
		case "playerfoodtitle":
			value = Localization.Get("xuiFood");
			return true;
		case "playeritemscrafted":
			value = ((player != null) ? playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(player)) : "");
			return true;
		case "playeritemscraftedtitle":
			value = Localization.Get("xuiItemsCrafted");
			return true;
		case "playerlongestlife":
			value = ((player != null) ? XUiM_Player.GetLongestLife(player) : "");
			return true;
		case "playerlongestlifetitle":
			value = Localization.Get("xuiLongestLife");
			return true;
		case "playercurrentlife":
			value = ((player != null) ? XUiM_Player.GetCurrentLife(player) : "");
			return true;
		case "playercurrentlifetitle":
			value = Localization.Get("xuiCurrentLife");
			return true;
		case "playerpvpkills":
			value = ((player != null) ? playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(player)) : "");
			return true;
		case "playerpvpkillstitle":
			value = Localization.Get("xuiPlayerKills");
			return true;
		case "playertravelled":
			value = ((player != null) ? XUiM_Player.GetKMTraveled(player) : "");
			return true;
		case "playertravelledtitle":
			value = Localization.Get("xuiKMTravelled");
			return true;
		case "playerxptonextlevel":
			value = ((player != null) ? playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(player) + player.Progression.ExpDeficit) : "");
			return true;
		case "playerxptonextleveltitle":
			value = Localization.Get("xuiXPToNextLevel");
			return true;
		case "playerzombiekills":
			value = ((player != null) ? playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(player)) : "");
			return true;
		case "playerzombiekillstitle":
			value = Localization.Get("xuiZombieKills");
			return true;
		case "playerarmorratingtitle":
			value = Localization.Get("statPhysicalDamageResist");
			return true;
		case "playerarmorrating":
			value = ((player != null) ? playerArmorRatingFormatter.Format((int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, player)) : "");
			return true;
		case "playerlootstagetitle":
			value = Localization.Get("xuiLootstage");
			return true;
		case "playerlootstage":
			value = ((player != null) ? player.GetHighestPartyLootStage(0f, 0f).ToString() : "");
			return true;
		default:
			if (bindingName.StartsWith("playerstattitle"))
			{
				if (player != null)
				{
					int index = Convert.ToInt32(bindingName.Replace("playerstattitle", "")) - 1;
					value = GetStatTitle(index);
				}
				else
				{
					value = "";
				}
				return true;
			}
			if (bindingName.StartsWith("playerstat"))
			{
				if (player != null)
				{
					int index2 = Convert.ToInt32(bindingName.Replace("playerstat", "")) - 1;
					value = GetStatValue(index2);
				}
				else
				{
					value = "";
				}
				return true;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (displayInfoEntries.Count <= index)
		{
			return "";
		}
		if (displayInfoEntries[index].TitleOverride != null)
		{
			return displayInfoEntries[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(displayInfoEntries[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (displayInfoEntries.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry displayInfoEntry = displayInfoEntries[index];
		return XUiM_Player.GetStatValue(displayInfoEntry.StatType, base.xui.playerUI.entityPlayer, displayInfoEntry);
	}
}
