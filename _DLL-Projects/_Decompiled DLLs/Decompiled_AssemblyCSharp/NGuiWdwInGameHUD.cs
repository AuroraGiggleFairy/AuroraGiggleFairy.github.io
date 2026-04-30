using System;
using UnityEngine;

public class NGuiWdwInGameHUD : MonoBehaviour
{
	public Texture2D[] overlayDamageTextures = new Texture2D[8];

	public Texture2D[] overlayDamageBloodDrops = new Texture2D[3];

	public Texture2D CrosshairTexture;

	public Texture2D CrosshairDamage;

	public Texture2D CrosshairUpgrade;

	public Texture2D CrosshairRepair;

	public Texture2D CrosshairAiming;

	public Texture2D CrosshairPowerSource;

	public Texture2D CrosshairPowerItem;

	public Texture2D[] StealthIcons = new Texture2D[5];

	public Texture2D[] StealthOverlays = new Texture2D[2];

	public Texture2D CrosshairBlocked;

	public Transform FocusCube;

	public float crosshairAlpha = 1f;

	public bool showCrosshair = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string wdwOpenedOnGameStatsShowWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal playerEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI;

	public GameManager gameManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		playerUI = GetComponentInParent<LocalPlayerUI>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		NGuiAction nGuiAction = new NGuiAction("DebugSpawn", PlayerActionsGlobal.Instance.DebugSpawn);
		nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			playerUI.windowManager.SwitchVisible(XUiC_SpawnMenu.ID);
		});
		nGuiAction.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nGuiAction2 = new NGuiAction("DebugGameEvent", PlayerActionsGlobal.Instance.DebugGameEvent);
		nGuiAction2.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			playerUI.windowManager.SwitchVisible(XUiC_GameEventMenu.ID);
		});
		nGuiAction2.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nGuiAction3 = new NGuiAction("SwitchHUD", PlayerActionsGlobal.Instance.SwitchHUD);
		nGuiAction3.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			playerUI.windowManager.ToggleHUDEnabled();
		});
		playerUI.windowManager.AddGlobalAction(nGuiAction);
		playerUI.windowManager.AddGlobalAction(nGuiAction2);
		playerUI.windowManager.AddGlobalAction(nGuiAction3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		playerEntity = playerUI.entityPlayer;
		playerUI.OnEntityPlayerLocalAssigned += HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		playerUI.OnEntityPlayerLocalAssigned -= HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEntityPlayerLocalAssigned(EntityPlayerLocal _entity)
	{
		playerEntity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!gameManager.gameStateManager.IsGameStarted())
		{
			return;
		}
		string text = GameStats.GetString(EnumGameStats.ShowWindow);
		if (!string.IsNullOrEmpty(text))
		{
			if (!playerUI.windowManager.IsWindowOpen(text))
			{
				playerUI.windowManager.Open(text, _bModal: false);
				wdwOpenedOnGameStatsShowWindow = text;
			}
		}
		else if (wdwOpenedOnGameStatsShowWindow != null)
		{
			playerUI.windowManager.Close(wdwOpenedOnGameStatsShowWindow);
			wdwOpenedOnGameStatsShowWindow = null;
		}
		if (playerEntity != null)
		{
			playerEntity.OnHUD();
		}
		if (playerUI.windowManager.IsHUDEnabled())
		{
			int num = GameStats.GetInt(EnumGameStats.GameState);
			if (num != 1)
			{
				_ = 2;
			}
		}
	}
}
