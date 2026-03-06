using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class XUiC_LevelToolsHelpers
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesUnpaintable = new string[5] { "_BlockEntities", "models", "modelsCollider", "cutout", "cutoutCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesPaintable = new string[2] { "opaque", "opaqueCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesTerrain = new string[2] { "terrain", "terrainCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool wasShowingImposterBeforeUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float screenshotBorderPercentage = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool screenshot4To3 = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool drawingScreenshotGuide;

	public static NGuiAction BuildAction(string _functionName, string _captionOverride, bool _forToggle)
	{
		if (_functionName.IndexOf(':') < 0)
		{
			return null;
		}
		NGuiAction nGuiAction = null;
		if (_functionName.StartsWith("SBM:"))
		{
			nGuiAction = createSelectionBoxAction(_functionName.Substring("SBM:".Length));
		}
		else if (_functionName.StartsWith("BTS:"))
		{
			nGuiAction = createBlockToolSelectionAction(_functionName.Substring("BTS:".Length));
		}
		else if (_functionName.StartsWith("Special:"))
		{
			nGuiAction = createSpecialAction(_functionName.Substring("Special:".Length));
		}
		if (nGuiAction == null)
		{
			Log.Error("Function " + _functionName + " for LevelTools UI not found");
			return null;
		}
		if (!string.IsNullOrEmpty(_captionOverride))
		{
			nGuiAction.SetText(_captionOverride);
		}
		if (_forToggle != nGuiAction.IsToggle())
		{
			Log.Error(_forToggle ? ("Function " + _functionName + " for LevelTools UI is not a toggle action, but bound to a toggle button") : ("Function " + _functionName + " for LevelTools UI is a toggle action, but bound to a regular button"));
			return null;
		}
		return nGuiAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createSelectionBoxAction(string _categoryName)
	{
		SelectionCategory selectionCategory = SelectionBoxManager.Instance.GetCategory(_categoryName);
		if (selectionCategory == null)
		{
			return null;
		}
		NGuiAction nGuiAction = new NGuiAction(Localization.Get("selectionCategory" + _categoryName), null, _isToggle: true);
		nGuiAction.SetDescription(_categoryName);
		nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SelectionCategory selectionCategory2 = selectionCategory;
			selectionCategory2.SetVisible(!selectionCategory2.IsVisible());
		});
		nGuiAction.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => selectionCategory.IsVisible());
		return nGuiAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createBlockToolSelectionAction(string _actionName)
	{
		BlockToolSelection blockToolSelection = (BlockToolSelection)((GameManager.Instance.GetActiveBlockTool() is BlockToolSelection) ? GameManager.Instance.GetActiveBlockTool() : null);
		if (blockToolSelection == null)
		{
			return null;
		}
		blockToolSelection.GetActions().TryGetValue(_actionName, out var value);
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createSpecialAction(string _substring)
	{
		switch (_substring)
		{
		case "CompositionGrid":
		{
			NGuiAction nGuiAction31 = new NGuiAction(Localization.Get("leveltoolsShowCompositionGrid"), null, _isToggle: true);
			nGuiAction31.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.ToggleCompositionGrid();
			});
			nGuiAction31.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsCompositionGrid());
			return nGuiAction31;
		}
		case "ShowChunkBorders":
		{
			NGuiAction nGuiAction29 = new NGuiAction(Localization.Get("leveltoolsShowChunkBorders"), null, _isToggle: true);
			nGuiAction29.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PlayerMoveController moveController = LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer.MoveController;
				moveController.drawChunkMode = (moveController.drawChunkMode + 1) % 2;
			});
			nGuiAction29.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer.MoveController.drawChunkMode > 0);
			return nGuiAction29;
		}
		case "Unpaintable":
		{
			NGuiAction nGuiAction26 = new NGuiAction(Localization.Get("leveltoolsShowUnpaintable"), null, _isToggle: true);
			nGuiAction26.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowUnpaintables = !GameManager.bShowUnpaintables;
				setChunkPartVisible(goNamesUnpaintable, GameManager.bShowUnpaintables);
			});
			nGuiAction26.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowUnpaintables);
			return nGuiAction26;
		}
		case "Paintable":
		{
			NGuiAction nGuiAction7 = new NGuiAction(Localization.Get("leveltoolsShowPaintable"), null, _isToggle: true);
			nGuiAction7.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowPaintables = !GameManager.bShowPaintables;
				setChunkPartVisible(goNamesPaintable, GameManager.bShowPaintables);
			});
			nGuiAction7.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowPaintables);
			return nGuiAction7;
		}
		case "Terrain":
		{
			NGuiAction nGuiAction16 = new NGuiAction(Localization.Get("leveltoolsShowTerrain"), null, _isToggle: true);
			nGuiAction16.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowTerrain = !GameManager.bShowTerrain;
				setChunkPartVisible(goNamesTerrain, GameManager.bShowTerrain);
			});
			nGuiAction16.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowTerrain);
			return nGuiAction16;
		}
		case "Decor":
		{
			NGuiAction nGuiAction18 = new NGuiAction(Localization.Get("leveltoolsShowDecor"), null, _isToggle: true);
			nGuiAction18.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowDecorBlocks = !GameManager.bShowDecorBlocks;
				foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					item.NeedsRegeneration = true;
				}
			});
			nGuiAction18.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowDecorBlocks);
			return nGuiAction18;
		}
		case "Loot":
		{
			NGuiAction nGuiAction23 = new NGuiAction(Localization.Get("leveltoolsShowLoot"), null, _isToggle: true);
			nGuiAction23.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowLootBlocks = !GameManager.bShowLootBlocks;
				foreach (Chunk item2 in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					item2.NeedsRegeneration = true;
				}
			});
			nGuiAction23.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowLootBlocks);
			return nGuiAction23;
		}
		case "QuestLoot":
		{
			NGuiAction nGuiAction5 = new NGuiAction(Localization.Get("leveltoolsShowQuestLoot"), null, _isToggle: true);
			nGuiAction5.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.HighlightQuestLoot = !PrefabEditModeManager.Instance.HighlightQuestLoot;
			});
			nGuiAction5.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.HighlightQuestLoot);
			return nGuiAction5;
		}
		case "BlockTriggers":
		{
			NGuiAction nGuiAction10 = new NGuiAction(Localization.Get("leveltoolsShowBlockTriggers"), null, _isToggle: true);
			nGuiAction10.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.HighlightBlockTriggers = !PrefabEditModeManager.Instance.HighlightBlockTriggers;
			});
			nGuiAction10.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.HighlightBlockTriggers);
			return nGuiAction10;
		}
		case "SleeperXRay":
		{
			NGuiAction nGuiAction3 = new NGuiAction(Localization.Get("leveltoolsSleeperXRay"), null, _isToggle: true);
			nGuiAction3.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				SleeperVolumeToolManager.SetXRay(!SleeperVolumeToolManager.GetXRay());
			});
			nGuiAction3.SetIsCheckedDelegate(SleeperVolumeToolManager.GetXRay);
			return nGuiAction3;
		}
		case "GroundGridToggle":
		{
			NGuiAction nGuiAction19 = new NGuiAction(Localization.Get("xuiShowGroundGrid"), null, _isToggle: true);
			nGuiAction19.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.ToggleGroundGrid();
			});
			nGuiAction19.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsGroundGrid());
			return nGuiAction19;
		}
		case "GroundGridMoveUp":
		{
			NGuiAction nGuiAction13 = new NGuiAction(Localization.Get("xuiShowMoveGroundGridUp"), null, _isToggle: false);
			nGuiAction13.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(1);
			});
			nGuiAction13.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsGroundGrid() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
			return nGuiAction13;
		}
		case "GroundGridMoveDown":
		{
			NGuiAction nGuiAction28 = new NGuiAction(Localization.Get("xuiShowMoveGroundGridDown"), null, _isToggle: false);
			nGuiAction28.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(-1);
			});
			nGuiAction28.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsGroundGrid() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
			return nGuiAction28;
		}
		case "PrefabMoveDown":
		{
			NGuiAction nGuiAction24 = new NGuiAction(Localization.Get("xuiMovePrefabDown"), null, _isToggle: false);
			nGuiAction24.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.MovePrefabUpOrDown(-1);
			});
			nGuiAction24.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction24;
		}
		case "PrefabMoveUp":
		{
			NGuiAction nGuiAction11 = new NGuiAction(Localization.Get("xuiMovePrefabUp"), null, _isToggle: false);
			nGuiAction11.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.MovePrefabUpOrDown(1);
			});
			nGuiAction11.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction11;
		}
		case "PrefabUpdateBounds":
		{
			NGuiAction nGuiAction30 = new NGuiAction(Localization.Get("xuiUpdateBounds"), null, _isToggle: false);
			nGuiAction30.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.UpdatePrefabBounds();
			});
			nGuiAction30.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction30;
		}
		case "PrefabFacingToggle":
		{
			NGuiAction nGuiAction20 = new NGuiAction(Localization.Get("xuiShowFacing"), null, _isToggle: true);
			nGuiAction20.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.TogglePrefabFacing(!PrefabEditModeManager.Instance.IsPrefabFacing());
			});
			nGuiAction20.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsPrefabFacing());
			return nGuiAction20;
		}
		case "PrefabFacingUpdate":
		{
			NGuiAction nGuiAction15 = new NGuiAction(Localization.Get("xuiUpdateFacing"), null, _isToggle: false);
			nGuiAction15.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.RotatePrefabFacing();
			});
			nGuiAction15.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.IsPrefabFacing());
			return nGuiAction15;
		}
		case "LightPerformance":
		{
			NGuiAction nGuiAction6 = new NGuiAction(Localization.Get("xuiDebugMenuShowLightPerf"), null, _isToggle: true);
			nGuiAction6.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				LightViewer.SetEnabled(!LightViewer.IsEnabled);
			});
			nGuiAction6.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => LightViewer.IsEnabled);
			return nGuiAction6;
		}
		case "CapturePrefabStats":
		{
			NGuiAction nGuiAction2 = new NGuiAction(Localization.Get("xuiCapturePrefabStats"), null, _isToggle: false);
			nGuiAction2.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				if (PrefabEditModeManager.Instance.VoxelPrefab == null)
				{
					GameManager.ShowTooltip(LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer, "[FF4444]" + Localization.Get("xuiPrefabStatsNoPrefabLoaded"));
				}
				else
				{
					XUiC_EditorStat.ManualStats = WorldStats.CaptureWorldStats();
				}
			});
			return nGuiAction2;
		}
		case "ImposterUpdate":
		{
			NGuiAction nGuiAction25 = new NGuiAction(Localization.Get("xuiUpdateImposter"), null, _isToggle: false);
			nGuiAction25.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				XUiC_SaveDirtyPrefab.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, updateImposter);
			});
			nGuiAction25.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None);
			return nGuiAction25;
		}
		case "ImposterToggle":
		{
			NGuiAction nGuiAction21 = new NGuiAction(Localization.Get("xuiShowImposter"), null, _isToggle: true);
			nGuiAction21.SetClickActionDelegate(imposterToggleShow);
			nGuiAction21.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.IsShowingImposterPrefab());
			nGuiAction21.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
			return nGuiAction21;
		}
		case "PrefabScreenshotToggleBounds":
		{
			NGuiAction nGuiAction12 = new NGuiAction(Localization.Get("xuiShowScreenshotBounds"), null, _isToggle: true);
			nGuiAction12.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				drawingScreenshotGuide = !drawingScreenshotGuide;
				if (drawingScreenshotGuide)
				{
					ThreadManager.StartCoroutine(drawScreenshotGuide());
				}
			});
			nGuiAction12.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => drawingScreenshotGuide);
			return nGuiAction12;
		}
		case "PrefabScreenshotTake":
		{
			NGuiAction nGuiAction8 = new NGuiAction(Localization.Get("xuiTakeScreenshot"), null, _isToggle: false);
			nGuiAction8.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				if (PrefabEditModeManager.Instance.VoxelPrefab == null)
				{
					GameManager.ShowTooltip(LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer, "[FF4444]" + Localization.Get("xuiScreenshotNoPrefabLoaded"));
				}
				else
				{
					ThreadManager.StartCoroutine(screenshotCo(PrefabEditModeManager.Instance.LoadedPrefab.FullPathNoExtension));
				}
			});
			nGuiAction8.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction8;
		}
		case "PrefabProperties":
		{
			NGuiAction nGuiAction32 = new NGuiAction(Localization.Get("xuiPrefabProperties"), null, _isToggle: false);
			nGuiAction32.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				XUiC_PrefabPropertiesEditor.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
			});
			nGuiAction32.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction32;
		}
		case "HighlightBlocksToggle":
		{
			NGuiAction nGuiAction27 = new NGuiAction(Localization.Get("xuiHighlightBlocks"), null, _isToggle: true);
			nGuiAction27.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.ToggleHighlightBlocks();
			});
			nGuiAction27.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.HighlightingBlocks);
			return nGuiAction27;
		}
		case "PaintTexturesToggle":
		{
			NGuiAction nGuiAction22 = new NGuiAction(Localization.Get("xuiShowPaintTextures"), null, _isToggle: true);
			nGuiAction22.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Chunk.IgnorePaintTextures = !Chunk.IgnorePaintTextures;
				foreach (Chunk item3 in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					item3.NeedsRegeneration = true;
				}
			});
			nGuiAction22.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => !Chunk.IgnorePaintTextures);
			return nGuiAction22;
		}
		case "TexturesStrip":
		{
			NGuiAction nGuiAction17 = new NGuiAction(Localization.Get("xuiStripTextures"), null, _isToggle: false);
			nGuiAction17.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.StripTextures();
			});
			nGuiAction17.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction17;
		}
		case "TexturesStripInternal":
		{
			NGuiAction nGuiAction14 = new NGuiAction(Localization.Get("xuiStripInternalTextures"), null, _isToggle: false);
			nGuiAction14.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.StripInternalTextures();
			});
			return nGuiAction14;
		}
		case "DensitiesSmoothLand":
		{
			NGuiAction nGuiAction9 = new NGuiAction(Localization.Get("xuiSmoothPrefabLand"), null, _isToggle: false);
			nGuiAction9.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabHelpers.SmoothPOI(1, _land: true);
			});
			nGuiAction9.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction9;
		}
		case "DensitiesSmoothAir":
		{
			NGuiAction nGuiAction4 = new NGuiAction(Localization.Get("xuiSmoothPrefabAir"), null, _isToggle: false);
			nGuiAction4.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabHelpers.SmoothPOI(1, _land: false);
			});
			nGuiAction4.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction4;
		}
		case "DensitiesClean":
		{
			NGuiAction nGuiAction = new NGuiAction(Localization.Get("xuiCleanDensity"), null, _isToggle: false);
			nGuiAction.SetClickActionDelegate(DensitiesClean);
			nGuiAction.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nGuiAction;
		}
		default:
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(string[] _matchedNames, bool _visible, List<ChunkGameObject> _cgos = null)
	{
		if (_cgos == null)
		{
			_cgos = GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects();
		}
		foreach (ChunkGameObject _cgo in _cgos)
		{
			setChunkPartVisible(_cgo.transform, _matchedNames, _visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(Transform _parent, string[] _matchedNames, bool _visible)
	{
		for (int i = 0; i < _parent.childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			string name = child.name;
			if (_matchedNames.ContainsCaseInsensitive(name))
			{
				child.gameObject.SetActive(_visible);
			}
			else if (child.childCount > 0)
			{
				setChunkPartVisible(child, _matchedNames, _visible);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		if (_action != XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.TempHUDDisable();
			wasShowingImposterBeforeUpdate = PrefabEditModeManager.Instance.IsShowingImposterPrefab();
			PrefabHelpers.convert(waitForUpdateImposter);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void waitForUpdateImposter()
	{
		PrefabHelpers.Cleanup();
		if (wasShowingImposterBeforeUpdate)
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		else
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.ReEnableHUD();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void imposterToggleShow()
	{
		if (PrefabEditModeManager.Instance.IsShowingImposterPrefab())
		{
			showPrefab();
		}
		else
		{
			XUiC_SaveDirtyPrefab.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, showImposter);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void showImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		if (_action != XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
			PrefabEditModeManager.Instance.ClearImposterPrefab();
			if (PrefabEditModeManager.Instance.HasPrefabImposter(loadedPrefab))
			{
				PrefabEditModeManager.Instance.LoadImposterPrefab(loadedPrefab);
			}
			else
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], "Prefab " + loadedPrefab.Name + " has no imposter yet");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void showPrefab()
	{
		if (PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
	}

	public static bool IsShowImposter()
	{
		return PrefabEditModeManager.Instance.IsShowingImposterPrefab();
	}

	public static void SetShowImposter()
	{
		imposterToggleShow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator drawScreenshotGuide()
	{
		while (drawingScreenshotGuide)
		{
			yield return new WaitForEndOfFrame();
			Rect screenshotRect = GameUtils.GetScreenshotRect(0.15f, _b4to3: true);
			screenshotRect = new Rect(screenshotRect.x - 2f, screenshotRect.y - 2f, screenshotRect.width + 4f, screenshotRect.height + 4f);
			GUIUtils.DrawRect(screenshotRect, Color.green);
			if (!GameManager.Instance.gameStateManager.IsGameStarted())
			{
				drawingScreenshotGuide = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator screenshotCo(string _filename)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.TempHUDDisable();
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		bool isSpectator = player.IsSpectator;
		player.IsSpectator = true;
		SkyManager.SetSkyEnabled(_enabled: false);
		yield return null;
		try
		{
			GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.File, _filename, 0.15f, _b4to3: true, 280, 210);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		yield return null;
		player.IsSpectator = isSpectator;
		SkyManager.SetSkyEnabled(_enabled: true);
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.ReEnableHUD();
	}

	public static void ReplaceBlockId(Block _srcBlockClass, Block _dstBlockClass)
	{
		int sourceBlockId = _srcBlockClass.blockID;
		int targetBlockId = _dstBlockClass.blockID;
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
			{
				if (_bv.type == sourceBlockId)
				{
					if (bUseSelection)
					{
						Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
						if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
						{
							return;
						}
					}
					if (_srcBlockClass.shape.IsTerrain() != _dstBlockClass.shape.IsTerrain())
					{
						sbyte b = curChunk.GetDensity(_x, _y, _z);
						if (_dstBlockClass.shape.IsTerrain())
						{
							b = MarchingCubes.DensityTerrain;
						}
						else if (b != 0)
						{
							b = MarchingCubes.DensityAir;
						}
						curChunk.SetDensity(_x, _y, _z, b);
					}
					BlockValue blockValue = new BlockValue((uint)targetBlockId)
					{
						rotation = _bv.rotation,
						meta = _bv.meta
					};
					curChunk.SetBlockRaw(_x, _y, _z, blockValue);
					changedChunks.Add(curChunk);
				}
			}, _bIncludeChilds: false, _bIncludeAirBlocks: true);
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReplacePaint(int _sourcePaintId, int _targetPaintId)
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				bool flag = false;
				long num = curChunk.GetTextureFull(_x, _y, _z);
				for (int j = 0; j < 6; j++)
				{
					if (((num >> j * 8) & 0xFF) == _sourcePaintId)
					{
						num &= ~(255L << j * 8);
						num |= (long)_targetPaintId << j * 8;
						flag = true;
					}
				}
				if (flag)
				{
					curChunk.SetTextureFull(_x, _y, _z, num);
					changedChunks.Add(curChunk);
				}
			});
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public static void ReplaceBlockShapeMaterials(string _oldMaterial, string _newMaterial)
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		Dictionary<int, int> blockReplaceCache = new Dictionary<int, int>();
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		int hits = 0;
		int misses = 0;
		int replaced = 0;
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				int type = _bv.type;
				if (!blockReplaceCache.TryGetValue(type, out var value))
				{
					int num = misses;
					misses = num + 1;
					Block block = _bv.Block;
					if (block.GetAutoShapeType() != EAutoShapeType.Shape)
					{
						blockReplaceCache[type] = -1;
						return;
					}
					if (!block.GetAutoShapeBlockName().Equals(_oldMaterial))
					{
						blockReplaceCache[type] = -1;
						return;
					}
					string autoShapeShapeName = block.GetAutoShapeShapeName();
					Block blockByName = Block.GetBlockByName(_newMaterial + ":" + autoShapeShapeName, _caseInsensitive: true);
					if (blockByName == null)
					{
						blockReplaceCache[type] = -1;
						return;
					}
					value = blockByName.blockID;
					blockReplaceCache[type] = value;
				}
				else
				{
					int num = hits;
					hits = num + 1;
				}
				if (value >= 0)
				{
					int num = replaced;
					replaced = num + 1;
					BlockValue blockValue = new BlockValue((uint)value)
					{
						rotation = _bv.rotation,
						meta = _bv.meta
					};
					curChunk.SetBlockRaw(_x, _y, _z, blockValue);
					changedChunks.Add(curChunk);
				}
			}, _bIncludeChilds: false, _bIncludeAirBlocks: true);
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
		Log.Out($"Replace material done in {microStopwatch.ElapsedMilliseconds} ms. Total checked blocks: {hits + misses}, replaced: {replaced}, cache hits: {hits}, misses: {misses}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DensitiesClean()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				Block block = _bv.Block;
				sbyte density = curChunk.GetDensity(_x, _y, _z);
				sbyte b = (block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
				if (b != density)
				{
					curChunk.SetDensity(_x, _y, _z, b);
					changedChunks.Add(curChunk);
				}
			}, _bIncludeChilds: false, _bIncludeAirBlocks: true);
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}
}
