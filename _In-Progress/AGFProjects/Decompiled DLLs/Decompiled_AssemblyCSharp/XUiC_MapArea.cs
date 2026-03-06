using System;
using System.Collections.Generic;
using Audio;
using GUI_2;
using InControl;
using Platform;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_MapArea : XUiController
{
	public enum EStaticMapOverlay
	{
		None,
		Radiation,
		Biomes
	}

	public const int MapDrawnSizeInChunks = 128;

	public const int MapDrawnSize = 2048;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZoomScale = 0.7f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxZoomScale = 6.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cSpriteScale = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapOnScreenSize = 712;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cTexMiddle = new Vector2i(356, 356);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapSizeFull = 2048;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapSizeZoom1 = 336;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float factorScreenSizeToDTM = 2.1190476f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float dragFactorSizeOfMap = 0.47191012f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStaticData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] staticWorldTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticWorldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticWorldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapTop;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D mapTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte mapMaskTransparency = byte.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[][] fowChunkMaskAlphas = new byte[13][];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapScrollTextureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mapScrollTextureChunksOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mapScrollTextureChunksOffsetZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool existingWaypointsInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShouldRedrawMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeToRedrawMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFowMaskEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosPixel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosChunksToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float zoomScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetZoomScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture xuiTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 defaultColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 hoverColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 disabledColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<long, MapObject> keyToMapObject = new DictionarySave<long, MapObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<int, NavObject> keyToNavObject = new DictionarySave<int, NavObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<int, GameObject> keyToNavSprite = new DictionarySave<int, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<long, GameObject> keyToMapSprite = new DictionarySave<long, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong navObjectsOnMapAlive = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong mapObjectsOnMapAlive = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject closestMouseOverNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpriteStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpritePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpriteEntitySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformSpritesParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblPlayerPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblBedrollPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblCursorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblMapMarkerDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblMapMarkerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button switchStaticMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<EStaticMapOverlay> cbxStaticMapType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController mapView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite crosshair;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMouseOverMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 nextMarkerMousePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public string kilometers;

	[PublicizedFrom(EAccessModifier.Private)]
	public float mapScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapBGPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ZOOM_SPEED = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const KeyCode dragKey = KeyCode.Mouse0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxMapSymbolSize = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MinMapSymbolSize = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float mapSpriteMouseOverDistance = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject selectMapSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapCursorSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentWaypointIconChosen;

	public override void Init()
	{
		base.Init();
		if (mapTexture == null)
		{
			mapTexture = new Texture2D(2048, 2048, TextureFormat.RGBA32, mipChain: false);
			mapTexture.name = "XUiC_MapArea.mapTexture";
		}
		NativeArray<Color32> rawTextureData = mapTexture.GetRawTextureData<Color32>();
		Color32 value = new Color32(0, 0, 0, byte.MaxValue);
		for (int i = 0; i < rawTextureData.Length; i += 4)
		{
			rawTextureData[i] = value;
		}
		XUiController childById = GetChildById("mapViewTexture");
		xuiTexture = (XUiV_Texture)childById.ViewComponent;
		transformSpritesParent = GetChildById("clippingPanel").ViewComponent.UiTransform;
		mapView = GetChildById("mapView");
		mapView.OnDrag += onMapDragged;
		mapView.OnScroll += onMapScrolled;
		mapView.OnPress += onMapPressedLeft;
		mapView.OnRightPress += onMapPressed;
		mapView.OnHover += onMapHover;
		zoomScale = 1f;
		targetZoomScale = 1f;
		base.xui.LoadData("Prefabs/MapSpriteEntity", [PublicizedFrom(EAccessModifier.Private)] (GameObject o) =>
		{
			prefabMapSprite = o;
		});
		base.xui.LoadData("Prefabs/MapSpriteStartPoint", [PublicizedFrom(EAccessModifier.Private)] (GameObject o) =>
		{
			prefabMapSpriteStartPoint = o;
		});
		base.xui.LoadData("Prefabs/MapSpritePrefab", [PublicizedFrom(EAccessModifier.Private)] (GameObject o) =>
		{
			prefabMapSpritePrefab = o;
		});
		base.xui.LoadData("Prefabs/MapSpriteEntitySpawner", [PublicizedFrom(EAccessModifier.Private)] (GameObject o) =>
		{
			prefabMapSpriteEntitySpawner = o;
		});
		initFOWChunkMaskColors();
		GetChildById("playerIcon").OnPress += onPlayerIconPressed;
		uiLblPlayerPos = (XUiV_Label)GetChildById("playerPos").ViewComponent;
		uiLblCursorPos = (XUiV_Label)GetChildById("cursorPos").ViewComponent;
		GetChildById("bedrollIcon").OnPress += onBedrollIconPressed;
		uiLblBedrollPos = (XUiV_Label)GetChildById("bedrollPos").ViewComponent;
		GetChildById("waypointIcon").OnPress += onWaypointIconPressed;
		uiLblMapMarkerDistance = (XUiV_Label)GetChildById("waypointDistance").ViewComponent;
		switchStaticMap = (XUiV_Button)GetChildById("switchStaticMap").ViewComponent;
		GetChildById("switchStaticMap").OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _args) =>
		{
			showStaticData = !showStaticData;
			switchStaticMap.Selected = showStaticData;
			cbxStaticMapType.ViewComponent.IsVisible = showStaticData;
		};
		cbxStaticMapType = GetChildByType<XUiC_ComboBoxEnum<EStaticMapOverlay>>();
		cbxStaticMapType.OnValueChanged += CbxStaticMapType_OnValueChanged;
		bShouldRedrawMap = true;
		initMap();
		kilometers = Localization.Get("xuiKilometers");
		crosshair = GetChildById("crosshair").ViewComponent as XUiV_Sprite;
		NavObjectManager.Instance.OnNavObjectRemoved += Instance_OnNavObjectRemoved;
		if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
		{
			showStaticData = true;
			switchStaticMap.Selected = true;
			cbxStaticMapType.Value = EStaticMapOverlay.Biomes;
		}
		mapView.ViewComponent.IsSnappable = false;
		childById.ViewComponent.IsSnappable = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectRemoved(NavObject newNavObject)
	{
		UnityEngine.Object.Destroy(keyToNavSprite[newNavObject.Key]);
		keyToNavObject.Remove(newNavObject.Key);
		keyToNavSprite.Remove(newNavObject.Key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFOWChunkMaskColors()
	{
		base.xui.LoadData("@:Textures/UI/fow_chunkMask.png", [PublicizedFrom(EAccessModifier.Private)] (Texture2D o) =>
		{
			Color32[] pixels = o.GetPixels32();
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					byte[] array = new byte[256];
					int num = 0;
					for (int k = i * 16; k < (i + 1) * 16; k++)
					{
						for (int l = j * 16; l < (j + 1) * 16; l++)
						{
							array[num++] = pixels[k * o.width + l].r;
						}
					}
					fowChunkMaskAlphas[i * 3 + j] = array;
				}
			}
			int num2 = 3;
			for (int m = 0; m < 4; m++)
			{
				byte[] array2 = new byte[256];
				int num3 = 0;
				for (int n = num2 * 16; n < (num2 + 1) * 16; n++)
				{
					for (int num4 = m * 16; num4 < (m + 1) * 16; num4++)
					{
						array2[num3++] = pixels[n * o.width + num4].r;
					}
				}
				fowChunkMaskAlphas[num2 * 3 + m] = array2;
			}
		});
	}

	public override void OnOpen()
	{
		base.OnOpen();
		closeAllPopups();
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		if (!isOpen)
		{
			isOpen = true;
			localPlayer = base.xui.playerUI.entityPlayer;
			bFowMaskEnabled = !GameManager.Instance.IsEditMode();
			switchStaticMap.IsVisible = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (GameManager.Instance.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
			cbxStaticMapType.ViewComponent.IsVisible = switchStaticMap.IsVisible && showStaticData;
			initExistingWaypoints(localPlayer.Waypoints);
			initMap();
			positionMap();
			PositionMapAt(localPlayer.GetPosition());
			base.xui.playerUI.GetComponentInParent<LocalPlayerCamera>().PreRender += OnPreRender;
			base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStick, "igcoMapMoveNoHold", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoMapWaypoint", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoMapZoomIn", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoMapZoomOut", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("map");
			crosshair.IsVisible = false;
			windowGroup.isEscClosable = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMapCursor(bool _customCursor)
	{
		bMapCursorSet = _customCursor;
		if (_customCursor)
		{
			CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Map);
		}
		else
		{
			CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		if (isOpen)
		{
			isOpen = false;
			bShouldRedrawMap = false;
			if (bMapCursorSet)
			{
				SetMapCursor(_customCursor: false);
				base.xui.currentToolTip.ToolTip = string.Empty;
			}
			base.xui.playerUI.GetComponentInParent<LocalPlayerCamera>().PreRender -= OnPreRender;
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.playerUI.CursorController.Locked = false;
			SoftCursor.SetCursor(CursorControllerAbs.ECursorType.Default);
			closestMouseOverNavObject = null;
			LocalPlayerUI.IsOverPagingOverrideElement = false;
		}
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		crosshair.IsVisible = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
	}

	public override void OnCursorUnSelected()
	{
		base.OnCursorUnSelected();
		crosshair.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!windowGroup.isShowing || !XUi.IsGameRunning() || base.xui.playerUI.entityPlayer == null)
		{
			return;
		}
		if (!bMapInitialized)
		{
			initMap();
		}
		if (base.xui.playerUI.playerInput.GUIActions.LastDeviceClass == InputDeviceClass.Controller && !base.xui.GetWindow("mapAreaSetWaypoint").IsVisible && bMouseOverMap)
		{
			Vector2 vector = -base.xui.playerUI.playerInput.GUIActions.Camera.Vector;
			if (vector.sqrMagnitude > 0f)
			{
				DragMap(vector * 500f * _dt);
			}
		}
		updateMapOverlay();
		if (bShouldRedrawMap)
		{
			updateFullMap();
			bShouldRedrawMap = false;
		}
		if (timeToRedrawMap > 0f)
		{
			timeToRedrawMap -= _dt;
			if (timeToRedrawMap <= 0f)
			{
				bShouldRedrawMap = true;
			}
		}
		if (localPlayer.ChunkObserver.mapDatabase.IsNetworkDataAvail())
		{
			timeToRedrawMap = 0.5f;
			localPlayer.ChunkObserver.mapDatabase.ResetNetworkDataAvail();
		}
		uiLblPlayerPos.Text = ValueDisplayFormatters.WorldPos(base.xui.playerUI.entityPlayer.GetPosition());
		Vector3 pos = screenPosToWorldPos(base.xui.playerUI.CursorController.GetScreenPosition());
		uiLblCursorPos.Text = ValueDisplayFormatters.WorldPos(pos);
		uiLblCursorPos.UiTransform.gameObject.SetActive(bMouseOverMap);
		uiLblBedrollPos.Text = ((localPlayer.SpawnPoints.Count > 0) ? ValueDisplayFormatters.WorldPos(localPlayer.SpawnPoints[0].ToVector3()) : string.Empty);
		string text = string.Empty;
		if (localPlayer.markerPosition != Vector3i.zero)
		{
			text = string.Format("{0} {1}", ((localPlayer.position - localPlayer.markerPosition.ToVector3()).magnitude / 1000f).ToCultureInvariantString("0.0"), kilometers);
		}
		uiLblMapMarkerDistance.Text = text;
		float num = 5f * base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
		if (num != 0f)
		{
			targetZoomScale = Utils.FastClamp(targetZoomScale + num * _dt, 0.7f, 6.15f);
		}
		zoomScale = Mathf.Lerp(zoomScale, targetZoomScale, 5f * _dt);
		positionMap();
		updateMapObjects();
		UpdateWaypointSelection();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			XUiV_Window window = base.xui.GetWindow("mapAreaSetWaypoint");
			if (window.IsVisible)
			{
				window.IsVisible = false;
				base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
				base.xui.GetWindow("mapAreaEnterWaypointName").IsVisible = false;
				base.xui.playerUI.CursorController.SetNavigationTargetLater(GetChildById("MapView").ViewComponent);
			}
			else
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows(null, _fromEsc: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapOverlay()
	{
		if (showStaticData == (staticWorldTexture != null))
		{
			return;
		}
		bShouldRedrawMap = true;
		if (!showStaticData)
		{
			staticWorldTexture = null;
		}
		else
		{
			if (staticWorldTexture != null)
			{
				return;
			}
			World world = GameManager.Instance.World;
			if (!(world.ChunkCache.ChunkProvider is ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw))
			{
				return;
			}
			IBiomeProvider biomeProvider = chunkProviderGenerateWorldFromRaw.GetBiomeProvider();
			WorldDecoratorPOIFromImage poiFromImage = chunkProviderGenerateWorldFromRaw.poiFromImage;
			if (biomeProvider == null || poiFromImage == null)
			{
				return;
			}
			int splat3Width = poiFromImage.splat3Width;
			int splat3Height = poiFromImage.splat3Height;
			int num = splat3Width / 2;
			int num2 = splat3Height / 2;
			staticWorldWidth = splat3Width;
			staticWorldHeight = splat3Height;
			staticMapLeft = -staticWorldWidth / 2;
			staticMapRight = staticWorldWidth / 2 - 1;
			staticMapBottom = -staticWorldHeight / 2;
			staticMapTop = staticWorldHeight / 2 - 1;
			staticWorldTexture = new Color32[poiFromImage.m_Poi.DimX * poiFromImage.m_Poi.DimY];
			Color32 color = new Color32(0, 0, 0, byte.MaxValue);
			for (int i = 0; i < splat3Height; i++)
			{
				int num3 = i * splat3Width;
				ReadOnlySpan<ushort> span;
				using (chunkProviderGenerateWorldFromRaw.heightData.GetReadOnlySpan(num3, splat3Width, out span))
				{
					for (int j = 0; j < splat3Width; j++)
					{
						color.r = 0;
						color.g = 0;
						color.b = 0;
						byte value = poiFromImage.m_Poi.colors.GetValue(j, i);
						PoiMapElement poiForColor;
						if (value == 1)
						{
							color.r = (color.b = byte.MaxValue);
						}
						else if (value == 2)
						{
							color.r = byte.MaxValue;
						}
						else if (value == 3)
						{
							color.g = byte.MaxValue;
						}
						else if (value == 4)
						{
							color.b = byte.MaxValue;
						}
						else if (value != 0 && (poiForColor = world.Biomes.getPoiForColor(value)) != null && poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
						{
							color.b = byte.MaxValue;
						}
						else
						{
							byte b = (byte)((float)(int)span[j] * 0.0038910506f);
							color.r = (color.g = (color.b = b));
							if (cbxStaticMapType.Value == EStaticMapOverlay.Biomes)
							{
								BiomeDefinition biomeAt = biomeProvider.GetBiomeAt(j - num, i - num2);
								if (biomeAt != null)
								{
									color.r = (byte)Mathf.LerpUnclamped((int)color.r, (biomeAt.m_uiColor >> 16) & 0xFF, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((int)color.g, (biomeAt.m_uiColor >> 8) & 0xFF, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((int)color.b, biomeAt.m_uiColor & 0xFF, 0.25f);
								}
							}
							else if (cbxStaticMapType.Value == EStaticMapOverlay.Radiation)
							{
								float radiationAt = biomeProvider.GetRadiationAt(j - num, i - num2);
								if (radiationAt < 0.5f)
								{
									continue;
								}
								if (radiationAt < 1.5f)
								{
									color.r = (byte)Mathf.LerpUnclamped((int)color.r, 0f, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((int)color.g, 255f, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((int)color.b, 0f, 0.25f);
								}
								else if (radiationAt < 2.5f)
								{
									color.r = (byte)Mathf.LerpUnclamped((int)color.r, 0f, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((int)color.g, 0f, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((int)color.b, 255f, 0.25f);
								}
								else
								{
									color.r = (byte)Mathf.LerpUnclamped((int)color.r, 255f, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((int)color.g, 0f, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((int)color.b, 0f, 0.25f);
								}
							}
						}
						staticWorldTexture[num3 + j] = color;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initExistingWaypoints(WaypointCollection _waypoints)
	{
		if (existingWaypointsInitialized)
		{
			return;
		}
		foreach (Waypoint w in _waypoints.Collection.list)
		{
			if (w.bIsAutoWaypoint || w.bUsingLocalizationId)
			{
				w.navObject.name = Localization.Get(w.name.Text);
				continue;
			}
			GeneratedTextManager.GetDisplayText(w.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
			{
				w.navObject.name = _filtered;
			}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
		}
		existingWaypointsInitialized = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initMap()
	{
		if (!(base.xui.playerUI.entityPlayer == null))
		{
			localPlayer = base.xui.playerUI.entityPlayer;
			bMapInitialized = true;
			xuiTexture.Texture = mapTexture;
			xuiTexture.Size = new Vector2i(712, 712);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFullMap()
	{
		int mapStartX = (int)mapMiddlePosChunks.x - 1024;
		int mapEndX = (int)mapMiddlePosChunks.x + 1024;
		int mapStartZ = (int)mapMiddlePosChunks.y - 1024;
		int mapEndZ = (int)mapMiddlePosChunks.y + 1024;
		updateMapSection(mapStartX, mapStartZ, mapEndX, mapEndZ, 0, 0, 2048, 2048);
		mapScrollTextureOffset.x = 0f;
		mapScrollTextureOffset.y = 0f;
		mapScrollTextureChunksOffsetX = 0;
		mapScrollTextureChunksOffsetZ = 0;
		positionMap();
		mapTexture.Apply();
		SendMapPositionToServer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapForScroll(int deltaChunksX, int deltaChunksZ)
	{
		if (deltaChunksX != 0)
		{
			int num = Mathf.Abs(deltaChunksX);
			int num2 = mapScrollTextureChunksOffsetX * 16;
			int value = (mapScrollTextureChunksOffsetX + deltaChunksX) * 16;
			value = Utils.WrapInt(value, 0, 2048);
			int num3;
			int num4;
			if (deltaChunksX > 0)
			{
				if (num2 == 2048)
				{
					num2 = 0;
				}
				num3 = (int)mapMiddlePosChunks.x + 1024;
				num4 = num3 - num * 16;
			}
			else
			{
				if (num2 == 0)
				{
					num2 = 2048;
				}
				int num5 = num2;
				num2 = value;
				value = num5;
				num4 = (int)mapMiddlePosChunks.x - 1024;
				num3 = num4 + num * 16;
			}
			int index = (mapScrollTextureChunksOffsetZ + deltaChunksZ) * 16;
			index = Utils.WrapIndex(index, 2048);
			int drawnMapEndZ = Utils.WrapIndex(index - 1, 2048);
			int num6 = (int)mapMiddlePosChunks.y - 1024;
			int mapEndZ = num6 + 2048;
			updateMapSection(num4, num6, num3, mapEndZ, num2, index, value, drawnMapEndZ);
		}
		if (deltaChunksZ != 0)
		{
			int num7 = Mathf.Abs(deltaChunksZ);
			int index = mapScrollTextureChunksOffsetZ * 16;
			int drawnMapEndZ = (mapScrollTextureChunksOffsetZ + deltaChunksZ) * 16;
			drawnMapEndZ = Utils.WrapInt(drawnMapEndZ, 0, 2048);
			int mapEndZ;
			int num6;
			if (deltaChunksZ > 0)
			{
				if (index == 2048)
				{
					index = 0;
				}
				mapEndZ = (int)mapMiddlePosChunks.y + 1024;
				num6 = mapEndZ - num7 * 16;
			}
			else
			{
				if (index == 0)
				{
					index = 2048;
				}
				int num8 = index;
				index = drawnMapEndZ;
				drawnMapEndZ = num8;
				num6 = (int)mapMiddlePosChunks.y - 1024;
				mapEndZ = num6 + num7 * 16;
			}
			int num2 = (mapScrollTextureChunksOffsetX + deltaChunksX) * 16;
			num2 = Utils.WrapIndex(num2, 2048);
			int value = Utils.WrapIndex(num2 - 1, 2048);
			int num4 = (int)mapMiddlePosChunks.x - 1024;
			int num3 = num4 + 2048;
			updateMapSection(num4, num6, num3, mapEndZ, num2, index, value, drawnMapEndZ);
		}
		mapScrollTextureOffset.x += (float)(deltaChunksX * 16) / (float)mapTexture.width;
		mapScrollTextureOffset.y += (float)(deltaChunksZ * 16) / (float)mapTexture.width;
		mapScrollTextureOffset.x = Utils.WrapFloat(mapScrollTextureOffset.x, 0f, 1f);
		mapScrollTextureOffset.y = Utils.WrapFloat(mapScrollTextureOffset.y, 0f, 1f);
		mapScrollTextureChunksOffsetX += deltaChunksX;
		mapScrollTextureChunksOffsetZ += deltaChunksZ;
		mapScrollTextureChunksOffsetX = Utils.WrapIndex(mapScrollTextureChunksOffsetX, 128);
		mapScrollTextureChunksOffsetZ = Utils.WrapIndex(mapScrollTextureChunksOffsetZ, 128);
		positionMap();
		mapTexture.Apply();
		SendMapPositionToServer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
	{
		IMapChunkDatabase mapDatabase = localPlayer.ChunkObserver.mapDatabase;
		bool flag = showStaticData && staticWorldTexture != null;
		NativeArray<Color32> rawTextureData = mapTexture.GetRawTextureData<Color32>();
		int num = mapStartZ;
		int num2 = drawnMapStartZ;
		while (num < mapEndZ)
		{
			int num3 = mapStartX;
			int num4 = drawnMapStartX;
			while (num3 < mapEndX)
			{
				int num5 = World.toChunkXZ(num3);
				int num6 = World.toChunkXZ(num);
				if (flag)
				{
					int num7 = num5 << 4;
					int num8 = num6 << 4;
					for (int i = 0; i < 256; i++)
					{
						int num9 = i / 16;
						int num10 = i % 16;
						int num11 = (num2 + num9) * 2048;
						int num12 = num4 + num10;
						int index = num11 + num12;
						int num13 = num7 + num10;
						int num14 = num8 + num9;
						if (num13 < staticMapLeft || num13 > staticMapRight || num14 < staticMapBottom || num14 > staticMapTop)
						{
							rawTextureData[index] = new Color32(0, 0, 0, 0);
							continue;
						}
						int num15 = num13 - staticMapLeft + (num14 - staticMapBottom) * staticWorldWidth;
						Color32 color = staticWorldTexture[num15];
						if (color.a > 0)
						{
							rawTextureData[index] = new Color32(color.r, color.g, color.b, byte.MaxValue);
						}
						else
						{
							rawTextureData[index] = new Color32(0, 0, 0, 0);
						}
					}
				}
				else
				{
					long chunkKey = WorldChunkCache.MakeChunkKey(num5, num6);
					ushort[] mapColors = mapDatabase.GetMapColors(chunkKey);
					if (mapColors == null)
					{
						for (int j = 0; j < 256; j++)
						{
							int num16 = (num2 + j / 16) * 2048;
							int index2 = num4 + j % 16 + num16;
							rawTextureData[index2] = new Color32(0, 0, 0, 0);
						}
					}
					else
					{
						bool flag2 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5, num6 + 1));
						bool flag3 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5, num6 - 1));
						bool flag4 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6));
						bool flag5 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6));
						int num17 = 0;
						if (flag2 && flag3 && flag4 && flag5)
						{
							bool flag6 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6 + 1));
							bool flag7 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6 + 1));
							bool flag8 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 - 1, num6 - 1));
							bool flag9 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num5 + 1, num6 - 1));
							num17 = ((!flag6) ? 9 : ((!flag7) ? 10 : ((!flag9) ? 11 : (flag8 ? 4 : 12))));
						}
						else
						{
							if (flag3 && !flag2)
							{
								num17 += 6;
							}
							else if (flag3 && flag2)
							{
								num17 += 3;
							}
							if (flag5 && flag4)
							{
								num17++;
							}
							else if (flag4)
							{
								num17 += 2;
							}
						}
						byte[] array = fowChunkMaskAlphas[num17];
						if (!bFowMaskEnabled)
						{
							array = fowChunkMaskAlphas[4];
						}
						for (int k = 0; k < 256; k++)
						{
							int num18 = k / 16;
							int num19 = k % 16;
							int num20 = (num2 + num18) * 2048;
							int num21 = num4 + num19;
							int index3 = num20 + num21;
							int num22 = num18 * 16;
							byte b = array[num22 + num19];
							Color32 color2 = Utils.FromColor5To32(mapColors[k]);
							rawTextureData[index3] = new Color32(color2.r, color2.g, color2.b, (b < byte.MaxValue) ? b : byte.MaxValue);
						}
					}
				}
				num3 += 16;
				num4 = Utils.WrapIndex(num4 + 16, 2048);
			}
			num += 16;
			num2 = Utils.WrapIndex(num2 + 16, 2048);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendMapPositionToServer()
	{
		if (GameManager.Instance.World.IsRemote() && !mapMiddlePosChunksToServer.Equals(mapMiddlePosChunks))
		{
			mapMiddlePosChunksToServer = mapMiddlePosChunks;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageMapPosition>().Setup(localPlayer.entityId, new Vector2i(Utils.Fastfloor(mapMiddlePosChunks.x), Utils.Fastfloor(mapMiddlePosChunks.y))));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void positionMap()
	{
		float num = (2048f - 336f * zoomScale) / 2f;
		mapScale = 336f * zoomScale / 2048f;
		float num2 = (num + (mapMiddlePosPixel.x - mapMiddlePosChunks.x)) / 2048f;
		float num3 = (num + (mapMiddlePosPixel.y - mapMiddlePosChunks.y)) / 2048f;
		mapPos = new Vector3(num2 + mapScrollTextureOffset.x, num3 + mapScrollTextureOffset.y, 0f);
		mapBGPos.x = (num + mapMiddlePosPixel.x) / 2048f;
		mapBGPos.y = (num + mapMiddlePosPixel.y) / 2048f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender(LocalPlayerCamera _localPlayerCamera)
	{
		Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(mapPos.x, mapPos.y, mapScale, mapScale));
		Shader.SetGlobalVector("_MainMapBGPosAndScale", new Vector4(mapBGPos.x, mapBGPos.y, mapScale, mapScale));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (UICamera.currentKey == KeyCode.Mouse0)
		{
			if (base.xui.playerUI.playerInput.GUIActions.LastDeviceClass != InputDeviceClass.Controller)
			{
				DragMap(_mousePositionDelta);
			}
			closeAllPopups();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DragMap(Vector2 delta)
	{
		float pixelRatioFactor = base.xui.GetPixelRatioFactor();
		mapMiddlePosPixel -= delta * pixelRatioFactor * 0.47191012f * zoomScale;
		mapMiddlePosPixel = GameManager.Instance.World.ClampToValidWorldPosForMap(mapMiddlePosPixel);
		int num = 0;
		int num2 = 0;
		while (mapMiddlePosChunks.x - mapMiddlePosPixel.x >= 16f)
		{
			mapMiddlePosChunks.x -= 16f;
			num--;
		}
		while (mapMiddlePosChunks.x - mapMiddlePosPixel.x <= -16f)
		{
			mapMiddlePosChunks.x += 16f;
			num++;
		}
		while (mapMiddlePosChunks.y - mapMiddlePosPixel.y >= 16f)
		{
			mapMiddlePosChunks.y -= 16f;
			num2--;
		}
		while (mapMiddlePosChunks.y - mapMiddlePosPixel.y <= -16f)
		{
			mapMiddlePosChunks.y += 16f;
			num2++;
		}
		if (num != 0 || num2 != 0)
		{
			updateMapForScroll(num, num2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapScrolled(XUiController _sender, float _delta)
	{
		float num = 6f;
		if (InputUtils.ShiftKeyPressed)
		{
			num = 5f * zoomScale;
		}
		float min = 0.7f;
		float max = 6.15f;
		targetZoomScale = Utils.FastClamp(zoomScale - _delta * num, min, max);
		if (_delta < 0f)
		{
			Manager.PlayInsidePlayerHead("map_zoom_in");
		}
		else
		{
			Manager.PlayInsidePlayerHead("map_zoom_out");
		}
		closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapObject(EnumMapObjectType _type, long _key, string _name, Vector3 _position, Vector3 _size, GameObject _prefab)
	{
		if (!keyToMapSprite.TryGetValue(_key, out var _value))
		{
			_value = transformSpritesParent.gameObject.AddChild(_prefab);
			_value.GetComponent<UISprite>().depth = 20;
			_value.name = _name;
			_value.GetComponent<UISprite>().depth = 1;
			keyToMapObject[_key] = new MapObject(_type, _position, _key, null, _bSelectable: true);
			keyToMapSprite[_key] = _value;
		}
		if ((bool)_value)
		{
			float num = getSpriteZoomScaleFac() * 4.3f;
			UISprite component = _value.GetComponent<UISprite>();
			component.width = (int)(_size.x * num);
			component.height = (int)(_size.z * num);
			Transform transform = _value.transform;
			transform.localPosition = worldPosToScreenPos(_position);
			transform.localRotation = Quaternion.identity;
			mapObjectsOnMapAlive.Add(_key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getSpriteZoomScaleFac()
	{
		return Utils.FastClamp(1f / (zoomScale * 2f), 0.02f, 20f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapObjectList(List<MapObject> _mapObjectList, bool _bConsiderInOnMouseOverCursor = false)
	{
		bool flag = false;
		for (int i = 0; i < _mapObjectList.Count; i++)
		{
			MapObject mapObject = _mapObjectList[i];
			mapObject.RefreshData();
			long num = ((long)mapObject.type << 32) | mapObject.key;
			if (!mapObject.IsMapIconEnabled())
			{
				continue;
			}
			GameObject gameObject = null;
			UISprite uISprite = null;
			if (!keyToMapSprite.ContainsKey(num))
			{
				gameObject = transformSpritesParent.gameObject.AddChild(prefabMapSprite);
				uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
				string mapIcon = mapObject.GetMapIcon();
				uISprite.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)uISprite.atlas).name, mapIcon);
				uISprite.spriteName = mapIcon;
				uISprite.depth = mapObject.GetLayerForMapIcon();
				uISprite.gameObject.GetComponent<TweenAlpha>().enabled = mapObject.IsMapIconBlinking();
				keyToMapObject[num] = mapObject;
				keyToMapSprite[num] = gameObject;
			}
			else
			{
				gameObject = keyToMapSprite[num];
			}
			string text = (mapObject.IsShowName() ? mapObject.GetName() : null);
			if (text != null)
			{
				UILabel component = gameObject.transform.Find("Name").GetComponent<UILabel>();
				component.text = text;
				component.gameObject.SetActive(value: true);
				component.color = mapObject.GetMapIconColor();
			}
			float spriteZoomScaleFac = getSpriteZoomScaleFac();
			uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
			Vector3 vector = mapObject.GetMapIconScale() * spriteZoomScaleFac;
			uISprite.width = (int)((float)cSpriteScale * vector.x);
			uISprite.height = (int)((float)cSpriteScale * vector.y);
			uISprite.color = mapObject.GetMapIconColor();
			uISprite.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f - mapObject.GetRotation().y);
			gameObject.transform.localPosition = worldPosToScreenPos(mapObject.GetPosition());
			if (mapObject.IsCenterOnLeftBottomCorner())
			{
				gameObject.transform.localPosition += new Vector3(uISprite.width / 2, uISprite.height / 2, 0f);
			}
			if (_bConsiderInOnMouseOverCursor)
			{
				Vector3 vector2 = worldPosToScreenPos(mapObject.GetPosition());
				vector2.y = 0f - vector2.y;
				Vector3 vector3 = mousePosToWindowPos(base.xui.playerUI.CursorController.GetScreenPosition());
				if (vector2.x > 0f && vector2.x < 712f && vector2.y > 0f && vector2.y < 712f && Utils.FastAbs((vector2 - vector3).magnitude) < 30f)
				{
					if (!bMapCursorSet)
					{
						SetMapCursor(_customCursor: true);
					}
					if (base.xui.currentToolTip.ToolTip != mapObject.GetName() && !string.IsNullOrEmpty(mapObject.GetName()))
					{
						base.xui.currentToolTip.ToolTip = mapObject.GetName();
						if (mapObject is MapObjectWaypoint)
						{
							selectWaypoint(((MapObjectWaypoint)mapObject).waypoint);
						}
					}
					flag = true;
				}
			}
			mapObjectsOnMapAlive.Add(num);
		}
		if (_bConsiderInOnMouseOverCursor && !flag && bMapCursorSet)
		{
			SetMapCursor(_customCursor: false);
			base.xui.currentToolTip.ToolTip = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjectList()
	{
		bool flag = true;
		bool flag2 = false;
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		navObjectsOnMapAlive.Clear();
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			if (navObject.hiddenOnMap)
			{
				continue;
			}
			NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
			if (currentMapSettings != null)
			{
				int key = navObject.Key;
				GameObject gameObject = null;
				UISprite uISprite = null;
				if (!keyToNavObject.ContainsKey(key))
				{
					gameObject = transformSpritesParent.gameObject.AddChild(prefabMapSprite);
					uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
					string spriteName = navObject.GetSpriteName(currentMapSettings);
					uISprite.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)uISprite.atlas).name, spriteName);
					uISprite.spriteName = spriteName;
					uISprite.depth = currentMapSettings.Layer;
					keyToNavObject[key] = navObject;
					keyToNavSprite[key] = gameObject;
				}
				else
				{
					gameObject = keyToNavSprite[key];
				}
				string text = ((navObject.TrackedEntity is EntityPlayer entityPlayer) ? entityPlayer.PlayerDisplayName : navObject.DisplayName);
				if (!string.IsNullOrEmpty(text))
				{
					UILabel component = gameObject.transform.Find("Name").GetComponent<UILabel>();
					component.text = text;
					component.font = base.xui.GetUIFontByName("ReferenceFont");
					component.gameObject.SetActive(value: true);
					component.color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color);
				}
				else
				{
					gameObject.transform.Find("Name").GetComponent<UILabel>().text = "";
				}
				float spriteZoomScaleFac = getSpriteZoomScaleFac();
				uISprite = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
				Vector3 vector = currentMapSettings.IconScaleVector * spriteZoomScaleFac;
				uISprite.width = Mathf.Clamp((int)((float)cSpriteScale * vector.x), 9, 100);
				uISprite.height = Mathf.Clamp((int)((float)cSpriteScale * vector.y), 9, 100);
				uISprite.color = (navObject.hiddenOnCompass ? Color.grey : (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color));
				uISprite.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f - navObject.Rotation.y);
				gameObject.transform.localPosition = worldPosToScreenPos(navObject.GetPosition() + Origin.position);
				if (currentMapSettings.AdjustCenter)
				{
					gameObject.transform.localPosition += new Vector3(uISprite.width / 2, uISprite.height / 2, 0f);
				}
				navObjectsOnMapAlive.Add(key);
			}
		}
		if (flag && !flag2 && bMapCursorSet)
		{
			SetMapCursor(_customCursor: false);
			base.xui.currentToolTip.ToolTip = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateMapObjects()
	{
		World world = GameManager.Instance.World;
		navObjectsOnMapAlive.Clear();
		mapObjectsOnMapAlive.Clear();
		if (world.IsEditor() || showStaticData)
		{
			SpawnPointList spawnPointList = GameManager.Instance.GetSpawnPointList();
			for (int i = 0; i < spawnPointList.Count; i++)
			{
				SpawnPoint spawnPoint = spawnPointList[i];
				updateMapObject(EnumMapObjectType.StartPoint, ((long)spawnPoint.GetHashCode() << 32) | 0xFFFFFFFFu, "SpawnPoint", spawnPoint.spawnPosition.position, Vector3.one * 30f, prefabMapSpriteStartPoint);
			}
			List<PrefabInstance> list = GameManager.Instance.GetDynamicPrefabDecorator()?.GetDynamicPrefabs();
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					PrefabInstance prefabInstance = list[j];
					Vector3 position = prefabInstance.boundingBoxPosition.ToVector3() + prefabInstance.boundingBoxSize.ToVector3() * 0.5f;
					updateMapObject(EnumMapObjectType.Prefab, ((long)prefabInstance.id << 32) | 0xFFFFFFFDu, prefabInstance.name, position, prefabInstance.boundingBoxSize.ToVector3(), prefabMapSpritePrefab);
				}
			}
		}
		updateNavObjectList();
		foreach (KeyValuePair<int, NavObject> item in keyToNavObject.Dict)
		{
			if (!navObjectsOnMapAlive.Contains(item.Key))
			{
				keyToNavObject.MarkToRemove(item.Key);
				keyToNavSprite.MarkToRemove(item.Key);
			}
		}
		foreach (KeyValuePair<long, MapObject> item2 in keyToMapObject.Dict)
		{
			if (!mapObjectsOnMapAlive.Contains(item2.Key))
			{
				keyToMapObject.MarkToRemove(item2.Key);
				keyToMapSprite.MarkToRemove(item2.Key);
			}
		}
		keyToNavObject.RemoveAllMarked([PublicizedFrom(EAccessModifier.Private)] (int _key) =>
		{
			keyToNavObject.Remove(_key);
		});
		keyToNavSprite.RemoveAllMarked([PublicizedFrom(EAccessModifier.Private)] (int _key) =>
		{
			UnityEngine.Object.Destroy(keyToNavSprite[_key]);
			keyToNavSprite.Remove(_key);
		});
		keyToMapObject.RemoveAllMarked([PublicizedFrom(EAccessModifier.Private)] (long _key) =>
		{
			keyToMapObject.Remove(_key);
		});
		keyToMapSprite.RemoveAllMarked([PublicizedFrom(EAccessModifier.Private)] (long _key) =>
		{
			UnityEngine.Object.Destroy(keyToMapSprite[_key]);
			keyToMapSprite.Remove(_key);
		});
		localPlayer.selectedSpawnPointKey = -1L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWaypointSelection()
	{
		Vector2 screenPosition = base.xui.playerUI.CursorController.GetScreenPosition();
		GameObject gameObject = null;
		float num = float.MaxValue;
		closestMouseOverNavObject = null;
		foreach (NavObject navObject in NavObjectManager.Instance.NavObjectList)
		{
			if (navObject == null || navObject.NavObjectClass == null || (navObject.TrackedEntity != null && navObject.TrackedEntity.entityId == GameManager.Instance.World.GetPrimaryPlayerId()))
			{
				continue;
			}
			GameObject gameObject2 = keyToNavSprite[navObject.Key];
			if (gameObject2 != null)
			{
				Vector3 b = base.xui.playerUI.uiCamera.cachedCamera.WorldToScreenPoint(gameObject2.transform.position);
				float num2 = Vector3.Distance(screenPosition, b);
				if (num2 <= 20f && (closestMouseOverNavObject == null || (closestMouseOverNavObject != null && num2 < num)))
				{
					closestMouseOverNavObject = navObject;
					num = num2;
					gameObject = gameObject2;
				}
			}
		}
		if (closestMouseOverNavObject != null)
		{
			if (selectMapSprite != gameObject)
			{
				if (selectMapSprite != null)
				{
					selectMapSprite.transform.localScale = Vector3.one;
				}
				selectMapSprite = gameObject;
				selectMapSprite.transform.localScale = Vector3.one * 1.5f;
			}
		}
		else if (selectMapSprite != null)
		{
			selectMapSprite.transform.localScale = Vector3.one;
			selectMapSprite = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPosToScreenPos(Vector3 _worldPos)
	{
		return new Vector3((_worldPos.x - mapMiddlePosPixel.x) * 2.1190476f / zoomScale + (float)cTexMiddle.x, (_worldPos.z - mapMiddlePosPixel.y) * 2.1190476f / zoomScale - (float)cTexMiddle.y, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mousePosToWindowPos(Vector3 _mousePos)
	{
		Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
		Vector3 vector = new Vector3(mouseXUIPosition.x, mouseXUIPosition.y, 0f);
		vector.x += 217f;
		vector.y -= 362f;
		vector.y = 0f - vector.y;
		return vector * 0.9493333f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 screenPosToWorldPos(Vector3 _mousePos, bool needY = false)
	{
		Vector3 vector = _mousePos;
		Vector3 vector2 = base.xui.playerUI.camera.WorldToScreenPoint(xuiTexture.UiTransform.position);
		vector.x -= vector2.x;
		vector.y -= vector2.y;
		vector.y *= -1f;
		Bounds xUIWindowScreenBounds = base.xui.GetXUIWindowScreenBounds(xuiTexture.UiTransform);
		Vector3 vector3 = xUIWindowScreenBounds.max - xUIWindowScreenBounds.min;
		float num = vector3.x / 336f;
		float num2 = (vector.x - vector3.x / 2f) / num * zoomScale + mapMiddlePosPixel.x;
		float num3 = (0f - (vector.y - vector3.y / 2f)) / num * zoomScale + mapMiddlePosPixel.y;
		float y = 0f;
		if (needY)
		{
			y = GameManager.Instance.World.GetHeightAt(num2, num3);
		}
		return new Vector3(num2, y, num3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportPlayerOnMap(Vector3 _screenPosition)
	{
		Vector3 vector = screenPosToWorldPos(_screenPosition);
		localPlayer.Teleport(new Vector3(vector.x, 180f, vector.z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapPressedLeft(XUiController _sender, int _mouseButton)
	{
		closeAllPopups();
		if (closestMouseOverNavObject != null)
		{
			closestMouseOverNavObject.hiddenOnCompass = !closestMouseOverNavObject.hiddenOnCompass;
			if (closestMouseOverNavObject.hiddenOnCompass)
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), Localization.Get("compassWaypointHiddenTooltip"));
			}
			if (closestMouseOverNavObject.NavObjectClass.NavObjectClassName == "quick_waypoint")
			{
				base.xui.playerUI.entityPlayer.navMarkerHidden = closestMouseOverNavObject.hiddenOnCompass;
				return;
			}
			Waypoint waypointForNavObject = base.xui.playerUI.entityPlayer.Waypoints.GetWaypointForNavObject(closestMouseOverNavObject);
			if (waypointForNavObject != null)
			{
				waypointForNavObject.hiddenOnCompass = closestMouseOverNavObject.hiddenOnCompass;
			}
		}
		else if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			OpenWaypointPopup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			return;
		}
		closeAllPopups();
		_ = base.xui.playerUI.uiCamera;
		if (InputUtils.ControlKeyPressed)
		{
			if (GameStats.GetBool(EnumGameStats.IsTeleportEnabled) || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				teleportPlayerOnMap(base.xui.playerUI.CursorController.GetScreenPosition());
			}
		}
		else
		{
			OpenWaypointPopup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenWaypointPopup()
	{
		nextMarkerMousePosition = base.xui.playerUI.CursorController.GetScreenPosition();
		base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
		XUiV_Window window = base.xui.GetWindow("mapAreaSetWaypoint");
		window.Position = base.xui.GetMouseXUIPosition();
		window.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationTargetLater(window.Controller.GetChildById("opt1").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapHover(XUiController _sender, bool _isOver)
	{
		bMouseOverMap = _isOver;
		LocalPlayerUI.IsOverPagingOverrideElement = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPlayerIconPressed(XUiController _sender, int _mouseButton)
	{
		PositionMapAt(localPlayer.GetPosition());
		closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBedrollIconPressed(XUiController _sender, int _mouseButton)
	{
		if (localPlayer.SpawnPoints.Count != 0)
		{
			PositionMapAt(localPlayer.SpawnPoints[0].ToVector3());
			closeAllPopups();
		}
	}

	public void OnSetWaypoint()
	{
		localPlayer.navMarkerHidden = false;
		localPlayer.markerPosition = World.worldToBlockPos(screenPosToWorldPos(nextMarkerMousePosition, needY: true));
		closeAllPopups();
	}

	public void OnWaypointEntryChosen(string _iconName)
	{
		currentWaypointIconChosen = _iconName;
	}

	public void OnWaypointCreated(string _name)
	{
		Waypoint w = new Waypoint();
		w.pos = World.worldToBlockPos(screenPosToWorldPos(nextMarkerMousePosition, needY: true));
		w.icon = currentWaypointIconChosen;
		w.name.Update(_name, PlatformManager.MultiPlatform.User.PlatformUserId);
		base.xui.playerUI.entityPlayer.Waypoints.Collection.Add(w);
		closeAllPopups();
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
		w.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", w.pos.ToVector3(), w.icon);
		w.navObject.IsActive = false;
		w.navObject.usingLocalizationId = w.bUsingLocalizationId;
		if (w.bIsAutoWaypoint || w.bUsingLocalizationId)
		{
			w.navObject.name = Localization.Get(w.name.Text);
		}
		else
		{
			GeneratedTextManager.GetDisplayText(w.name, [PublicizedFrom(EAccessModifier.Internal)] (string _filtered) =>
			{
				w.navObject.name = _filtered;
			}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
		}
		selectWaypoint(w);
		Manager.PlayInsidePlayerHead("ui_waypoint_add");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxStaticMapType_OnValueChanged(XUiController _sender, EStaticMapOverlay _oldvalue, EStaticMapOverlay _newvalue)
	{
		staticWorldTexture = null;
	}

	public void RefreshVehiclePositionWaypoint(EntityVehicle _vehicle, bool _unloaded)
	{
		Log.Out("Refresh Vehicle Position Waypoint {0} {1}", _vehicle.entityId, _unloaded);
		Waypoint waypoint = new Waypoint();
		waypoint.pos = World.worldToBlockPos(_vehicle.position);
		waypoint.icon = _vehicle.GetMapIcon();
		waypoint.ownerId = _vehicle.GetVehicle().OwnerId;
		waypoint.name.Update(Localization.Get(_vehicle.EntityName), PlatformManager.MultiPlatform.User.PlatformUserId);
		waypoint.lastKnownPositionEntityType = eLastKnownPositionEntityType.Vehicle;
		waypoint.lastKnownPositionEntityId = _vehicle.entityId;
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!entityPlayer.Waypoints.ContainsLastKnownPositionWaypoint(_vehicle.entityId))
		{
			entityPlayer.Waypoints.Collection.Add(waypoint);
			if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
			{
				((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
				waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos.ToVector3(), waypoint.icon);
				waypoint.navObject.IsActive = false;
				waypoint.navObject.OverrideSpriteName = _vehicle.GetMapIcon();
				waypoint.navObject.name = waypoint.name.Text;
				waypoint.navObject.usingLocalizationId = waypoint.bUsingLocalizationId;
			}
			else
			{
				RemoveVehicleLastKnownWaypoint(_vehicle);
			}
		}
		else if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
		{
			entityPlayer.Waypoints.UpdateEntityVehicleWayPoint(_vehicle, _unloaded);
		}
		else
		{
			RemoveVehicleLastKnownWaypoint(_vehicle);
		}
	}

	public void RemoveVehicleLastKnownWaypoint(EntityVehicle _vehicle)
	{
		if (base.xui.playerUI.entityPlayer.Waypoints.TryRemoveLastKnownPositionWaypoint(_vehicle.entityId))
		{
			((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
		}
	}

	public void RefreshDronePositionWaypoint(EntityDrone _drone, bool _unloaded)
	{
		Log.Out("Refresh Drone Position Waypoint {0} {1}", _drone.entityId, _unloaded);
		Waypoint waypoint = new Waypoint();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		waypoint.pos = World.worldToBlockPos(_drone.position);
		waypoint.icon = _drone.GetMapIcon();
		waypoint.ownerId = _drone.OwnerID;
		waypoint.name.Update(Localization.Get(_drone.EntityName), PlatformManager.MultiPlatform.User.PlatformUserId);
		waypoint.lastKnownPositionEntityType = eLastKnownPositionEntityType.Drone;
		waypoint.lastKnownPositionEntityId = _drone.entityId;
		if (!entityPlayer.Waypoints.ContainsLastKnownPositionWaypoint(_drone.entityId))
		{
			entityPlayer.Waypoints.Collection.Add(waypoint);
			if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
			{
				((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
				waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos.ToVector3(), waypoint.icon);
				waypoint.navObject.IsActive = false;
				waypoint.navObject.OverrideSpriteName = _drone.GetMapIcon();
				waypoint.navObject.name = waypoint.name.Text;
				waypoint.navObject.usingLocalizationId = waypoint.bUsingLocalizationId;
			}
			else
			{
				RemoveDronePositionWaypoint(_drone.entityId);
			}
		}
		else if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
		{
			entityPlayer.Waypoints.UpdateEntityDroneWayPoint(_drone, _drone.OrderState == EntityDrone.Orders.Follow, _unloaded);
		}
		else
		{
			RemoveDronePositionWaypoint(_drone.entityId);
		}
	}

	public void RefreshDronePositionWaypoint(int _entityId, Vector3i _pos, bool _unloaded)
	{
		Waypoint waypoint = new Waypoint();
		EntityClass entityClass = EntityClass.list[EntityClass.junkDroneClass];
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		waypoint.pos = _pos;
		waypoint.icon = entityClass.Properties.GetString(EntityClass.PropMapIcon);
		waypoint.ownerId = PlatformManager.MultiPlatform.User.PlatformUserId;
		waypoint.name.Update(Localization.Get(entityClass.entityClassName), PlatformManager.MultiPlatform.User.PlatformUserId);
		waypoint.lastKnownPositionEntityType = eLastKnownPositionEntityType.Drone;
		waypoint.lastKnownPositionEntityId = _entityId;
		if (!entityPlayer.Waypoints.ContainsLastKnownPositionWaypoint(_entityId))
		{
			entityPlayer.Waypoints.Collection.Add(waypoint);
			if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
			{
				((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
				waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos.ToVector3(), waypoint.icon);
				waypoint.navObject.IsActive = false;
				waypoint.navObject.OverrideSpriteName = waypoint.icon;
				waypoint.navObject.name = waypoint.name.Text;
				waypoint.navObject.usingLocalizationId = waypoint.bUsingLocalizationId;
			}
			else
			{
				RemoveDronePositionWaypoint(_entityId);
			}
		}
	}

	public void RemoveDronePositionWaypoint(int _entityId)
	{
		if (base.xui.playerUI.entityPlayer.Waypoints.TryRemoveLastKnownPositionWaypoint(_entityId))
		{
			((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onWaypointIconPressed(XUiController _sender, int _mouseButton)
	{
		_ = base.xui.playerUI.entityPlayer;
		if (localPlayer.markerPosition == Vector3i.zero)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_waypoint_delete");
		}
		localPlayer.markerPosition = Vector3i.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWaypoint(Waypoint _w)
	{
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).SelectWaypoint(_w);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWaypoint(NavObject _nav)
	{
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).SelectWaypoint(_nav);
	}

	public void closeAllPopups()
	{
		base.xui.GetWindow("mapAreaSetWaypoint").IsVisible = false;
		base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
		base.xui.GetWindow("mapAreaEnterWaypointName").IsVisible = false;
		base.xui.GetWindow("mapTrackingPopup").IsVisible = false;
		mapView.SelectCursorElement();
	}

	public void PositionMapAt(Vector3 _worldPos)
	{
		int num = (int)_worldPos.x;
		int num2 = (int)_worldPos.z;
		mapMiddlePosChunks = new Vector2(World.toChunkXZ(num - 1024) * 16 + 1024, World.toChunkXZ(num2 - 1024) * 16 + 1024);
		mapMiddlePosPixel = mapMiddlePosChunks;
		mapMiddlePosPixel = GameManager.Instance.World.ClampToValidWorldPosForMap(mapMiddlePosPixel);
		updateFullMap();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		UnityEngine.Object.Destroy(mapTexture);
	}
}
