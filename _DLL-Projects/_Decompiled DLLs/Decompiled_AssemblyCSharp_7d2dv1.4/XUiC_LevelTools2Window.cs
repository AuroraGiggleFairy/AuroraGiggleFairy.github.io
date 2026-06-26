using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools2Window : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float screenshotBorderPercentage = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool screenshot4To3 = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Table layoutTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleGroundGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMoveGroundGridUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMoveGroundGridDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMovePrefabUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMovePrefabDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtOldId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtNewId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnReplaceBlockIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleHighlightBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtHighlightBlockName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTakeScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPrefabProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStripTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCleanDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool blockListsInitDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool drawingScreenshotGuide;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		layoutTable = GetChildById("layoutTable").ViewComponent as XUiV_Table;
		toggleGroundGrid = GetChildById("toggleGroundGrid")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleGroundGrid != null)
		{
			toggleGroundGrid.OnValueChanged += ToggleGroundGrid_OnValueChanged;
		}
		btnMoveGroundGridUp = GetChildById("btnMoveGroundGridUp") as XUiC_SimpleButton;
		if (btnMoveGroundGridUp != null)
		{
			btnMoveGroundGridUp.OnPressed += BtnMoveGroundGridUp_OnPressed;
		}
		btnMoveGroundGridDown = GetChildById("btnMoveGroundGridDown") as XUiC_SimpleButton;
		if (btnMoveGroundGridDown != null)
		{
			btnMoveGroundGridDown.OnPressed += BtnMoveGroundGridDown_OnPressed;
		}
		btnMovePrefabUp = GetChildById("btnMovePrefabUp") as XUiC_SimpleButton;
		if (btnMovePrefabUp != null)
		{
			btnMovePrefabUp.OnPressed += BtnMovePrefabUp_OnPressed;
		}
		btnMovePrefabDown = GetChildById("btnMovePrefabDown") as XUiC_SimpleButton;
		if (btnMovePrefabDown != null)
		{
			btnMovePrefabDown.OnPressed += BtnMovePrefabDown_OnPressed;
		}
		btnUpdateBounds = GetChildById("btnUpdateBounds") as XUiC_SimpleButton;
		if (btnUpdateBounds != null)
		{
			btnUpdateBounds.OnPressed += BtnUpdateBoundsOnOnPressed;
		}
		toggleShowFacing = GetChildById("toggleShowFacing")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleShowFacing != null)
		{
			toggleShowFacing.OnValueChanged += ToggleShowFacing_OnValueChanged;
		}
		btnUpdateFacing = GetChildById("btnUpdateFacing") as XUiC_SimpleButton;
		if (btnUpdateFacing != null)
		{
			btnUpdateFacing.OnPressed += BtnUpdateFacingOnOnPressed;
		}
		txtOldId = GetChildById("txtOldId") as XUiC_DropDown;
		txtNewId = GetChildById("txtNewId") as XUiC_DropDown;
		if (txtOldId != null)
		{
			txtOldId.OnChangeHandler += ReplaceBlockIds_OnChangeHandler;
			txtOldId.OnSubmitHandler += ReplaceBlockIds_OnSubmitHandler;
			txtOldId.TextInput.SelectOnTab = txtNewId?.TextInput;
		}
		if (txtNewId != null)
		{
			txtNewId.OnChangeHandler += ReplaceBlockIds_OnChangeHandler;
			txtNewId.OnSubmitHandler += ReplaceBlockIds_OnSubmitHandler;
			txtNewId.TextInput.SelectOnTab = txtOldId?.TextInput;
		}
		btnReplaceBlockIds = GetChildById("btnReplaceBlockIds") as XUiC_SimpleButton;
		if (btnReplaceBlockIds != null)
		{
			btnReplaceBlockIds.OnPressed += BtnReplaceBlockIds_OnPressed;
		}
		toggleHighlightBlocks = GetChildById("toggleHighlightBlocks") as XUiC_ToggleButton;
		if (toggleHighlightBlocks != null)
		{
			toggleHighlightBlocks.OnValueChanged += ToggleHighlightBlocks_OnValueChanged;
		}
		txtHighlightBlockName = GetChildById("txtHighlightBlockName") as XUiC_DropDown;
		if (txtHighlightBlockName != null)
		{
			txtHighlightBlockName.OnChangeHandler += HighlightBlock_OnChangeHandler;
			txtHighlightBlockName.OnSubmitHandler += HighlightBlock_OnSubmitHandler;
		}
		XUiC_ToggleButton xUiC_ToggleButton = GetChildById("toggleScreenshotBounds")?.GetChildByType<XUiC_ToggleButton>();
		if (xUiC_ToggleButton != null)
		{
			xUiC_ToggleButton.OnValueChanged += ToggleScreenshotBounds_OnValueChanged;
		}
		btnTakeScreenshot = GetChildById("btnTakeScreenshot") as XUiC_SimpleButton;
		if (btnTakeScreenshot != null)
		{
			btnTakeScreenshot.OnPressed += BtnTakeScreenshot_OnPressed;
		}
		btnUpdateImposter = GetChildById("btnUpdateImposter") as XUiC_SimpleButton;
		if (btnUpdateImposter != null)
		{
			btnUpdateImposter.OnPressed += BtnUpdateImposterOnOnPressed;
		}
		toggleShowImposter = GetChildById("toggleShowImposter")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleShowImposter != null)
		{
			toggleShowImposter.OnValueChanged += ToggleShowImposterOnOnValueChanged;
		}
		btnPrefabProperties = GetChildById("btnPrefabProperties") as XUiC_SimpleButton;
		if (btnPrefabProperties != null)
		{
			btnPrefabProperties.OnPressed += BtnPrefabPropertiesOnOnPressed;
		}
		btnStripTextures = GetChildById("btnStripTextures") as XUiC_SimpleButton;
		if (btnStripTextures != null)
		{
			btnStripTextures.OnPressed += BtnStripTexturesOnPressed;
		}
		if (GetChildById("btnStripInternalTextures") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnStripInternalTexturesOnPressed;
		}
		btnCleanDensity = GetChildById("btnCleanDensity") as XUiC_SimpleButton;
		if (btnCleanDensity != null)
		{
			btnCleanDensity.OnPressed += BtnCleanDensityOnPressed;
		}
		if (GetChildById("btnCapturePrefabStats") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnCapturePrefabStatsOnPressed;
		}
		if (GetChildById("btnPOIMarkers") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += BtnPOIMarkers_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPOIMarkers_OnPressed(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleGroundGrid_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		PrefabEditModeManager.Instance.ToggleGroundGrid();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveGroundGridUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveGroundGridDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(-1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMovePrefabUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MovePrefabUpOrDown(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMovePrefabDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MovePrefabUpOrDown(-1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateBoundsOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.UpdatePrefabBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowFacing_OnValueChanged(XUiC_ToggleButton _sender, bool _newvalue)
	{
		PrefabEditModeManager.Instance.TogglePrefabFacing(toggleShowFacing.Value);
		btnUpdateFacing.Enabled = toggleShowFacing.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateFacingOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.RotatePrefabFacing();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnChangeHandler(XUiController _sender, string _text, bool _changefromcode)
	{
		bool flag = Block.GetBlockByName(txtOldId.Text, _caseInsensitive: true) != null;
		bool flag2 = Block.GetBlockByName(txtNewId.Text, _caseInsensitive: true) != null;
		txtOldId.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
		txtNewId.TextInput.ActiveTextColor = (flag2 ? Color.white : Color.red);
		btnReplaceBlockIds.Enabled = flag && flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnSubmitHandler(XUiController _sender, string _text)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReplaceBlockIds_OnPressed(XUiController _sender, int _mouseButton)
	{
		replaceBlockId();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleHighlightBlocks_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			HighlightBlock_OnSubmitHandler(_sender, txtHighlightBlockName.Text);
		}
		else
		{
			PrefabEditModeManager.Instance.HighlightBlocks(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		bool flag = Block.GetBlockByName(txtHighlightBlockName.Text, _caseInsensitive: true) != null;
		txtHighlightBlockName.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnSubmitHandler(XUiController _sender, string _text)
	{
		Block blockByName = Block.GetBlockByName(txtHighlightBlockName.Text, _caseInsensitive: true);
		if (toggleHighlightBlocks != null)
		{
			toggleHighlightBlocks.Value = true;
		}
		if (blockByName != null)
		{
			PrefabEditModeManager.Instance.HighlightBlocks(blockByName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replaceBlockId()
	{
		if (!btnReplaceBlockIds.Enabled)
		{
			return;
		}
		Block srcBlockClass = Block.GetBlockByName(txtOldId.Text, _caseInsensitive: true);
		Block dstBlockClass = Block.GetBlockByName(txtNewId.Text, _caseInsensitive: true);
		if (srcBlockClass == null || dstBlockClass == null)
		{
			return;
		}
		int sourceBlockId = srcBlockClass.blockID;
		int targetBlockId = dstBlockClass.blockID;
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
					if (srcBlockClass.shape.IsTerrain() != dstBlockClass.shape.IsTerrain())
					{
						sbyte b = curChunk.GetDensity(_x, _y, _z);
						if (dstBlockClass.shape.IsTerrain())
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaint()
	{
		if (!int.TryParse(txtOldId.Text, out var sourcePaintId) || !int.TryParse(txtNewId.Text, out var targetPaintId))
		{
			return;
		}
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
					if (((num >> j * 8) & 0xFF) == sourcePaintId)
					{
						num &= ~(255L << j * 8);
						num |= (long)targetPaintId << j * 8;
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleScreenshotBounds_OnValueChanged(XUiC_ToggleButton _sender, bool _newvalue)
	{
		drawingScreenshotGuide = _newvalue;
		if (_newvalue)
		{
			ThreadManager.StartCoroutine(drawScreenshotGuide());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawScreenshotGuide()
	{
		while (drawingScreenshotGuide)
		{
			yield return new WaitForEndOfFrame();
			Rect screenshotRect = GameUtils.GetScreenshotRect(0.15f, _b4to3: true);
			screenshotRect = new Rect(screenshotRect.x - 2f, screenshotRect.y - 2f, screenshotRect.width + 4f, screenshotRect.height + 4f);
			GUIUtils.DrawRect(screenshotRect, Color.green);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTakeScreenshot_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "[FF4444]" + Localization.Get("xuiScreenshotNoPrefabLoaded"));
			return;
		}
		string fullPathNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FullPathNoExtension;
		ThreadManager.StartCoroutine(screenshotCo(fullPathNoExtension));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowImposterOnOnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, showImposter);
		}
		else
		{
			showPrefab();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateImposterOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, updateImposter);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPrefabPropertiesOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStripTexturesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.StripTextures();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStripInternalTexturesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.StripInternalTextures();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCleanDensityOnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnCleanDensity.Enabled)
		{
			return;
		}
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCapturePrefabStatsOnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "[FF4444]" + Localization.Get("xuiPrefabStatsNoPrefabLoaded"));
		}
		else
		{
			XUiC_EditorStat.ManualStats = WorldStats.CaptureWorldStats();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator screenshotCo(string _filename)
	{
		base.xui.playerUI.windowManager.TempHUDDisable();
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
		base.xui.playerUI.windowManager.ReEnableHUD();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		if (_action != XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			base.xui.playerUI.windowManager.TempHUDDisable();
			PrefabHelpers.convert(waitForUpdateImposter);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waitForUpdateImposter()
	{
		PrefabHelpers.Cleanup();
		if (toggleShowImposter.Value)
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		else
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		base.xui.playerUI.windowManager.ReEnableHUD();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		if (_action != XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
			PrefabEditModeManager.Instance.ClearImposterPrefab();
			if (PrefabEditModeManager.Instance.HasPrefabImposter(loadedPrefab))
			{
				PrefabEditModeManager.Instance.LoadImposterPrefab(loadedPrefab);
				return;
			}
			GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], "Prefab " + loadedPrefab.Name + " has no imposter yet");
			toggleShowImposter.Value = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showPrefab()
	{
		if (PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ReplaceBlockIds_OnChangeHandler(this, null, _changefromcode: true);
		toggleShowFacing.Value = PrefabEditModeManager.Instance.IsPrefabFacing();
		btnUpdateFacing.Enabled = toggleShowFacing.Value;
		if (blockListsInitDone)
		{
			return;
		}
		List<string> list = new List<string>();
		Block[] list2 = Block.list;
		foreach (Block block in list2)
		{
			if (block != null)
			{
				list.Add(block.GetBlockName());
			}
		}
		txtOldId.AllEntries.AddRange(list);
		txtOldId.UpdateFilteredList();
		txtNewId.AllEntries.AddRange(list);
		txtNewId.UpdateFilteredList();
		txtHighlightBlockName.AllEntries.AddRange(list);
		txtHighlightBlockName.UpdateFilteredList();
		blockListsInitDone = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = PrefabEditModeManager.Instance.VoxelPrefab != null;
		bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		bool flag2 = PrefabEditModeManager.Instance.IsGroundGrid();
		btnMoveGroundGridDown.Enabled = flag2 && isServer;
		btnMoveGroundGridUp.Enabled = flag2 && isServer;
		toggleGroundGrid.Value = flag2;
		btnMovePrefabDown.Enabled = flag && isServer;
		btnMovePrefabUp.Enabled = flag && isServer;
		toggleShowImposter.Enabled = isServer;
		btnUpdateBounds.Enabled = flag && isServer;
		btnTakeScreenshot.Enabled = flag && isServer;
		btnUpdateImposter.Enabled = PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None && isServer;
		btnPrefabProperties.Enabled = isServer && flag;
		btnStripTextures.Enabled = isServer && flag;
		btnCleanDensity.Enabled = isServer && flag;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		drawingScreenshotGuide = false;
	}

	public static bool IsShowImposter(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_LevelTools2Window>().toggleShowImposter.Value;
	}

	public static void SetShowImposter(XUi _xui, bool _showImposter)
	{
		((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_LevelTools2Window>().toggleShowImposter.Value = _showImposter;
	}
}
