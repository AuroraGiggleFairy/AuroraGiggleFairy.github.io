using System;
using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnSelectionWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ESpawnWindowMode
	{
		Progress,
		SpawnSelection
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ESpawnWindowMode windowMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChooseSpawnPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnteringGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTimeSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHasBedroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHasBackpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayCountdownTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip spawnReadyClip;

	public SpawnMethod spawnMethod;

	public SpawnPosition spawnTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	public ESpawnWindowMode WindowMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return windowMode;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (windowMode != value)
			{
				windowMode = value;
				IsDirty = true;
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "spawn_sound")
		{
			base.xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _clip) =>
			{
				spawnReadyClip = _clip;
			});
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "buttonsVisible":
			_value = (WindowMode == ESpawnWindowMode.SpawnSelection).ToString();
			return true;
		case "enteringGame":
			_value = bEnteringGame.ToString();
			return true;
		case "firstTimeSpawn":
			_value = bFirstTimeSpawn.ToString();
			return true;
		case "showNearFriends":
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_value = false.ToString();
			}
			else if (XUiC_SpawnNearFriendsList.SpawnNearFriendMode == AllowSpawnNearFriend.Disabled)
			{
				_value = false.ToString();
			}
			else
			{
				_value = bFirstTimeSpawn.ToString();
			}
			return true;
		case "hasBackpack":
		{
			bool flag = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient?.GetValue(GameInfoBool.AllowSpawnNearBackpack) ?? true;
			_value = (flag && bHasBackpack).ToString();
			return true;
		}
		case "hasBedroll":
			_value = bHasBedroll.ToString();
			return true;
		case "progressVisible":
			_value = (WindowMode == ESpawnWindowMode.Progress).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		XUiV_Label label = (XUiV_Label)GetChildById("lblProgress").ViewComponent;
		ellipsisAnimator = new TextEllipsisAnimator(null, label);
		if (GetChildById("btnLeave") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				base.xui.playerUI.windowManager.Close(windowGroup.ID);
				GameManager.Instance.Disconnect();
			};
		}
		if (GetChildById("btnSpawnFirstTime") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SpawnButtonPressed(SpawnMethod.Invalid);
			};
		}
		if (GetChildById("btnSpawn") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SpawnButtonPressed(SpawnMethod.Invalid);
			};
		}
		if (GetChildById("btnRespawn") is XUiC_SimpleButton xUiC_SimpleButton4)
		{
			xUiC_SimpleButton4.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				if (GameStats.GetInt(EnumGameStats.DeathPenalty) == 3)
				{
					SpawnButtonPressed(SpawnMethod.NewRandomSpawn);
				}
				else
				{
					SpawnButtonPressed(SpawnMethod.NearDeath);
				}
			};
		}
		if (GetChildById("btnNearBackpack") is XUiC_SimpleButton xUiC_SimpleButton5)
		{
			xUiC_SimpleButton5.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SpawnButtonPressed(SpawnMethod.NearBackpack);
			};
		}
		if (GetChildById("btnOnBedroll") is XUiC_SimpleButton xUiC_SimpleButton6)
		{
			xUiC_SimpleButton6.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SpawnButtonPressed(SpawnMethod.OnBedRoll);
			};
		}
		if (GetChildById("btnNearBedroll") is XUiC_SimpleButton xUiC_SimpleButton7)
		{
			xUiC_SimpleButton7.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				SpawnButtonPressed(SpawnMethod.NearBedroll);
			};
		}
		XUiC_SpawnNearFriendsList childByType = GetChildByType<XUiC_SpawnNearFriendsList>();
		if (childByType != null)
		{
			childByType.SpawnClicked += [PublicizedFrom(EAccessModifier.Private)] (PersistentPlayerData _ppd) =>
			{
				SpawnButtonPressed(SpawnMethod.NearFriend, _ppd.EntityId);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtons()
	{
		if ((WindowMode == ESpawnWindowMode.Progress && !bEnteringGame) || bEnteringGame)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal == null)
		{
			Log.Warning("Refresh buttons cannot process without an EntityPlayerLocal");
			return;
		}
		SpawnPosition spawnPosition;
		Vector3i vector3i;
		if (entityPlayerLocal != null)
		{
			spawnPosition = entityPlayerLocal.GetSpawnPoint();
			vector3i = entityPlayerLocal.GetLastDroppedBackpackPosition();
		}
		else
		{
			spawnPosition = new SpawnPosition(Vector3.forward, 0f);
			vector3i = Vector3i.down;
		}
		bHasBedroll = !spawnPosition.IsUndef();
		bHasBackpack = vector3i != Vector3i.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnButtonPressed(SpawnMethod _method, int _nearEntityId = -1)
	{
		base.xui.playerUI.xui.PlayMenuConfirmSound();
		spawnMethod = _method;
		if (bEnteringGame)
		{
			WindowMode = ESpawnWindowMode.Progress;
			base.xui.playerUI.CursorController.SetNavigationTarget(null);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.canSpawnPlayer = true;
				return;
			}
			GameManager.Instance.RequestToSpawn(_nearEntityId);
			if (_method == SpawnMethod.NearFriend)
			{
				ThreadManager.StartCoroutine(sendFriendRequest(_nearEntityId));
			}
			return;
		}
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal == null)
		{
			Log.Warning("Spawn button cannot process without an EntityPlayerLocal");
			return;
		}
		SpawnPosition spawnPosition;
		Vector3i blockPos;
		Vector3 position;
		if (entityPlayerLocal != null)
		{
			spawnPosition = entityPlayerLocal.GetSpawnPoint();
			blockPos = entityPlayerLocal.GetLastDroppedBackpackPosition();
			position = entityPlayerLocal.position;
		}
		else
		{
			spawnPosition = new SpawnPosition(Vector3.forward, 0f);
			blockPos = Vector3i.down;
			position = Vector3.down;
		}
		SpawnPosition spawnPosition2 = new SpawnPosition(blockPos, 0f);
		SpawnPosition spawnPosition3 = new SpawnPosition(position, 0f);
		SpawnPosition randomSpawnPosition = GameManager.Instance.GetSpawnPointList().GetRandomSpawnPosition(entityPlayerLocal.world);
		spawnTarget = spawnMethod switch
		{
			SpawnMethod.Invalid => SpawnPosition.Undef, 
			SpawnMethod.OnBedRoll => spawnPosition, 
			SpawnMethod.NewRandomSpawn => randomSpawnPosition, 
			SpawnMethod.NearDeath => spawnPosition3, 
			SpawnMethod.NearBedroll => spawnPosition, 
			SpawnMethod.NearBackpack => spawnPosition2, 
			SpawnMethod.Unstuck => SpawnPosition.Undef, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator sendFriendRequest(int _entityId)
	{
		if (_entityId == -1)
		{
			yield break;
		}
		EntityPlayerLocal epl;
		do
		{
			if (GameManager.Instance.World == null)
			{
				yield break;
			}
			epl = GameManager.Instance.World.GetPrimaryPlayer();
			yield return null;
		}
		while (epl == null || !epl.Spawned);
		if (GameManager.Instance.World.GetEntity(_entityId) is EntityPlayer entityPlayer)
		{
			if (!entityPlayer.partyInvites.Contains(epl))
			{
				entityPlayer.AddPartyInvite(epl.entityId);
			}
			NetPackagePartyActions package = NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, epl.entityId, _entityId);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
			}
		}
		epl.PlayerUI.xui.GetChildByType<XUiC_PlayersList>().AddInvitePress(_entityId);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		ellipsisAnimator.GetNextAnimatedString(_dt);
		if (IsDirty)
		{
			RefreshButtons();
			RefreshBindings(_forceAll: true);
			if (base.xui.playerUI.CursorController.navigationTarget == null && !base.xui.playerUI.CursorController.CursorModeActive)
			{
				GetChildById("content").SelectCursorElement(_withDelay: true);
			}
			IsDirty = false;
		}
		updateLoadState();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		delayCountdownTime = 1f;
		spawnMethod = SpawnMethod.Invalid;
		ellipsisAnimator.SetBaseString(Localization.Get("msgBuildingEnvironment"));
		showSpawningComponents(ESpawnWindowMode.Progress);
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
				showSpawningComponents(ESpawnWindowMode.SpawnSelection);
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
			showSpawningComponents(ESpawnWindowMode.SpawnSelection);
		}
		else
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showSpawningComponents(ESpawnWindowMode _windowMode)
	{
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		if (!setCursor)
		{
			WindowMode = _windowMode;
			base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
			setCursor = true;
			if (WindowMode == ESpawnWindowMode.SpawnSelection)
			{
				Manager.PlayXUiSound(spawnReadyClip, 1f);
			}
		}
	}

	public static void Open(LocalPlayerUI _playerUi, bool _chooseSpawnPosition, bool _enteringGame, bool _firstTimeSpawn = false)
	{
		XUiC_SpawnSelectionWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SpawnSelectionWindow>();
		childByType.bChooseSpawnPosition = _chooseSpawnPosition;
		childByType.bEnteringGame = _enteringGame;
		childByType.bFirstTimeSpawn = _firstTimeSpawn;
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

	public static bool IsInSpawnSelection(LocalPlayerUI _playerUi)
	{
		if (IsOpenInUI(_playerUi))
		{
			return GetWindow(_playerUi).WindowMode == ESpawnWindowMode.SpawnSelection;
		}
		return false;
	}

	public static XUiC_SpawnSelectionWindow GetWindow(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SpawnSelectionWindow>();
	}
}
