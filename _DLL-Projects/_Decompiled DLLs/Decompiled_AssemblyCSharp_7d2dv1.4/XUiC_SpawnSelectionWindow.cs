using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnSelectionWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChooseSpawnPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnteringGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayCountdownTime;

	public SpawnMethod spawnMethod;

	public SpawnPosition spawnTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption3;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option1Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option2Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option3Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option1Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option2Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option3Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	public bool ShowButtons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (showButtons != value)
			{
				showButtons = value;
				IsDirty = true;
			}
		}
	}

	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			lblProgress.Text = value;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "buttonsVisible"))
		{
			if (_bindingName == "progressVisible")
			{
				_value = (!showButtons).ToString();
				return true;
			}
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = showButtons.ToString();
		return true;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		lblProgress = (XUiV_Label)GetChildById("lblProgress").ViewComponent;
		ellipsisAnimator = new TextEllipsisAnimator(null, lblProgress);
		btnOption1 = (XUiC_SimpleButton)GetChildById("btnOption1");
		btnOption2 = (XUiC_SimpleButton)GetChildById("btnOption2");
		btnOption3 = (XUiC_SimpleButton)GetChildById("btnOption3");
		btnOption1.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
		{
			SpawnButtonPressed(option1Method, option1Position);
		};
		btnOption2.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
		{
			SpawnButtonPressed(option2Method, option2Position);
		};
		btnOption3.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
		{
			SpawnButtonPressed(option3Method, option3Position);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtons()
	{
		if (!showButtons && !bEnteringGame)
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			return;
		}
		if (bEnteringGame)
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption1.Text = Localization.Get("lblSpawn");
			option1Method = SpawnMethod.Invalid;
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			Log.Warning("Refresh buttons cannot process without an EntityPlayerLocal");
			return;
		}
		SpawnPosition spawnPoint = primaryPlayer.GetSpawnPoint();
		bool flag = !primaryPlayer.GetSpawnPoint().IsUndef();
		Vector3i lastDroppedBackpackPosition = primaryPlayer.GetLastDroppedBackpackPosition();
		bool flag2 = lastDroppedBackpackPosition != Vector3i.zero;
		SpawnPosition spawnPosition = (flag2 ? new SpawnPosition(lastDroppedBackpackPosition, 0f) : SpawnPosition.Undef);
		option1Position = (option2Position = (option3Position = SpawnPosition.Undef));
		if (!flag && !flag2)
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption1.Text = Localization.Get("lblRespawn");
			option1Method = SpawnMethod.Invalid;
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		}
		else if (flag2 && !flag)
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption1.Text = Localization.Get("lblSpawnNearBackpack");
			option1Method = SpawnMethod.NearBackpack;
			option1Position = spawnPosition;
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		}
		else if (!flag2 && flag)
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption1.Text = Localization.Get("lblSpawnOnBedroll");
			option1Method = SpawnMethod.OnBedRoll;
			option1Position = spawnPoint;
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption2.Text = Localization.Get("lblSpawnNearBedroll");
			option2Method = SpawnMethod.NearBedroll;
			option2Position = spawnPosition;
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		}
		else
		{
			btnOption1.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption1.Text = Localization.Get("lblSpawnNearBackpack");
			option1Method = SpawnMethod.NearBackpack;
			option1Position = spawnPosition;
			btnOption2.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption2.Text = Localization.Get("lblSpawnOnBedroll");
			option2Method = SpawnMethod.OnBedRoll;
			option2Position = spawnPoint;
			btnOption3.ViewComponent.UiTransform.gameObject.SetActive(value: true);
			btnOption3.Text = Localization.Get("lblSpawnNearBedroll");
			option3Method = SpawnMethod.NearBedroll;
			option3Position = spawnPoint;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnButtonPressed(SpawnMethod _method, SpawnPosition _position)
	{
		base.xui.playerUI.xui.PlayMenuConfirmSound();
		if (bEnteringGame)
		{
			ShowButtons = false;
			base.xui.playerUI.CursorController.SetNavigationTarget(null);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.canSpawnPlayer = true;
			}
			else
			{
				GameManager.Instance.RequestToSpawn();
			}
		}
		else
		{
			spawnMethod = _method;
			spawnTarget = _position;
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		ellipsisAnimator.GetNextAnimatedString(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			RefreshButtons();
			IsDirty = false;
		}
		updateLoadState();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.World.GetPrimaryPlayer();
		delayCountdownTime = 1f;
		spawnMethod = SpawnMethod.Invalid;
		ellipsisAnimator.SetBaseString(Localization.Get("msgBuildingEnvironment"));
		showSpawningComponents(_spawning: false);
		RefreshBindings();
		((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
		setCursor = false;
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		bChooseSpawnPosition = false;
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		setCursor = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLoadState()
	{
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			if (bEnteringGame)
			{
				showSpawningComponents(_spawning: true);
			}
			return;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
			return;
		}
		if (delayCountdownTime > 0f)
		{
			delayCountdownTime -= Time.deltaTime;
			return;
		}
		int displayedChunkGameObjectsCount = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount();
		int viewDistance = GameUtils.GetViewDistance();
		int num = ((!GameManager.Instance.World.ChunkCache.IsFixedSize) ? (viewDistance * viewDistance - 10) : 0);
		if (displayedChunkGameObjectsCount < num)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ellipsisAnimator.SetBaseString(Localization.Get("msgStartingGame"));
				GameManager.Instance.World.GetPrimaryPlayer();
			}
		}
		else if (DistantTerrain.Instance != null && !DistantTerrain.Instance.IsTerrainReady)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				ellipsisAnimator.SetBaseString(Localization.Get("msgGeneratingDistantTerrain"));
			}
		}
		else if (!LocalPlayerUI.GetUIForPrimaryPlayer().xui.isReady)
		{
			ellipsisAnimator.SetBaseString(Localization.Get("msgLoadingUI"));
		}
		else if (bChooseSpawnPosition && GameManager.Instance.World.GetPrimaryPlayer() != null)
		{
			showSpawningComponents(_spawning: true);
		}
		else
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showSpawningComponents(bool _spawning)
	{
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		if (!setCursor)
		{
			ShowButtons = _spawning;
			base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
			btnOption1.SelectCursorElement(_withDelay: true);
			setCursor = true;
		}
	}

	public static void Open(LocalPlayerUI _playerUi, bool _chooseSpawnPosition, bool _enteringGame)
	{
		XUiC_SpawnSelectionWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SpawnSelectionWindow>();
		childByType.bChooseSpawnPosition = _chooseSpawnPosition;
		childByType.bEnteringGame = _enteringGame;
		_playerUi.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		_playerUi.windowManager.CloseIfOpen(ID);
	}

	public static bool IsOpenInUI(LocalPlayerUI _playerUi)
	{
		return _playerUi.windowManager.IsWindowOpen(ID);
	}

	public static XUiC_SpawnSelectionWindow GetWindow(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SpawnSelectionWindow>();
	}
}
