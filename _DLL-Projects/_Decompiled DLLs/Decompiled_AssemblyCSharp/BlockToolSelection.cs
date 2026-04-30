using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;

public class BlockToolSelection : ISelectionBoxCallback, IBlockTool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBlockUndoRedoCount = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colActive = new Color(1f, 0f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colInactive = new Color(0f, 0f, 1f, 0.5f);

	public static BlockToolSelection Instance;

	public Prefab clipboard = new Prefab();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_selectionStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_SelectionEndPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iSelectionLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> undoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> redoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBuildTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SelectionBoxName = "SingleInstance";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, NGuiAction> actions;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGOParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot1;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot2;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool copyPasteAirBlocks = true;

	public PlayerActionsLocal playerInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 selectionRotCenter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo hitInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWaitForRelease;

	public int SelectionClrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> undoChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int undoClrIdx;

	public SelectionBox SelectionBox
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox box = SelectionBoxManager.Instance.GetCategory("Selection").GetBox("SingleInstance");
			if (box != null)
			{
				return box;
			}
			box = SelectionBoxManager.Instance.GetCategory("Selection").AddBox("SingleInstance", Vector3i.zero, Vector3i.one);
			box.SetVisible(_visible: false);
			box.SetSizeVisibility(_visible: true);
			return box;
		}
	}

	public bool SelectionActive
	{
		get
		{
			return SelectionBoxManager.Instance.IsActive("Selection", "SingleInstance");
		}
		set
		{
			if (SelectionActive != value)
			{
				_ = SelectionBox;
				SelectionBoxManager.Instance.SetActive("Selection", "SingleInstance", value);
			}
		}
	}

	public int SelectionLockMode
	{
		get
		{
			return m_iSelectionLockMode;
		}
		set
		{
			if (m_iSelectionLockMode != value)
			{
				m_iSelectionLockMode = value;
				SelectionBox.SetVisible(SelectionActive);
				Color c = colInactive;
				if (m_iSelectionLockMode == 1)
				{
					c = new Color(0.5f, 0f, 1f, 0.5f);
					removeBlockPreview();
				}
				else if (m_iSelectionLockMode == 2)
				{
					c = colActive;
				}
				else
				{
					removeBlockPreview();
				}
				SelectionBox.SetAllFacesColor(c);
			}
		}
	}

	public Vector3i SelectionMin => new Vector3i(Utils.FastMin(SelectionStart.x, SelectionEnd.x), Utils.FastMin(SelectionStart.y, SelectionEnd.y), Utils.FastMin(SelectionStart.z, SelectionEnd.z));

	public Vector3i SelectionStart
	{
		get
		{
			return m_selectionStartPoint;
		}
		set
		{
			if (!m_selectionStartPoint.Equals(value))
			{
				m_selectionStartPoint = value;
				updateSelection();
			}
		}
	}

	public Vector3i SelectionEnd
	{
		get
		{
			return m_SelectionEndPoint;
		}
		set
		{
			if (!m_SelectionEndPoint.Equals(value))
			{
				m_SelectionEndPoint = value;
				updateSelection();
			}
		}
	}

	public Vector3i SelectionSize => new Vector3i(Mathf.Abs(m_selectionStartPoint.x - m_SelectionEndPoint.x) + 1, Mathf.Abs(m_selectionStartPoint.y - m_SelectionEndPoint.y) + 1, Mathf.Abs(m_selectionStartPoint.z - m_SelectionEndPoint.z) + 1);

	public BlockToolSelection()
	{
		Instance = this;
		SelectionBoxManager.Instance.GetCategory("Selection").SetCallback(this);
		PlayerActionsLocal primaryPlayer = PlatformManager.NativePlatform.Input.PrimaryPlayer;
		NGuiAction nGuiAction = new NGuiAction(Localization.Get("selectionToolsEditBlocksVolume"), null, _isToggle: true);
		nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			GameManager.bVolumeBlocksEditing = !GameManager.bVolumeBlocksEditing;
		});
		nGuiAction.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bVolumeBlocksEditing);
		nGuiAction.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		NGuiAction nGuiAction2 = new NGuiAction(Localization.Get("selectionToolsCopySleeperVolume"), null);
		nGuiAction2.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (XUiC_WoPropsSleeperVolume.selectedPrefabInstance != null && XUiC_WoPropsSleeperVolume.selectedVolumeIndex >= 0)
			{
				int selectedVolumeIndex = XUiC_WoPropsSleeperVolume.selectedVolumeIndex;
				PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
				XUiC_WoPropsSleeperVolume.selectedPrefabInstance.prefab.CloneSleeperVolume(selectedPrefabInstance.name, selectedPrefabInstance.boundingBoxPosition, selectedVolumeIndex);
			}
		});
		nGuiAction2.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction2.SetTooltip("selectionToolsCopySleeperVolumeTip");
		NGuiAction nGuiAction3 = new NGuiAction(Localization.Get("selectionToolsCopyAirBlocks"), null, _isToggle: true);
		nGuiAction3.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			copyPasteAirBlocks = !copyPasteAirBlocks;
		});
		nGuiAction3.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Private)] () => copyPasteAirBlocks);
		nGuiAction3.SetIsVisibleDelegate(GameManager.Instance.IsEditMode);
		NGuiAction nGuiAction4 = new NGuiAction(Localization.Get("selectionToolsClearSelection"), primaryPlayer.SelectionClear);
		nGuiAction4.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			if (SelectionLockMode == 2)
			{
				SelectionLockMode = 0;
				SelectionActive = false;
			}
			else
			{
				BeginUndo(0);
				BlockTools.CubeRPC(GameManager.Instance, SelectionClrIdx, SelectionStart, SelectionEnd, BlockValue.Air, MarchingCubes.DensityAir, 0, TextureFullArray.Default);
				BlockTools.CubeWaterRPC(GameManager.Instance, SelectionStart, SelectionEnd, WaterValue.Empty);
				EndUndo(0);
			}
		});
		nGuiAction4.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Private)] () => GameManager.Instance.IsEditMode() && SelectionActive);
		nGuiAction4.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction4.SetTooltip("selectionToolsClearSelectionTip");
		NGuiAction nGuiAction5 = new NGuiAction(Localization.Get("selectionToolsFillSelection"), primaryPlayer.SelectionFill);
		nGuiAction5.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			BeginUndo(0);
			EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
			ItemValue holdingItemItemValue = primaryPlayer2.inventory.holdingItemItemValue;
			BlockValue blockValue = holdingItemItemValue.ToBlockValue();
			if (!blockValue.isair)
			{
				Block block = blockValue.Block;
				BlockPlacement.Result _bpResult = new BlockPlacement.Result(0, Vector3.zero, Vector3i.zero, blockValue);
				block.OnBlockPlaceBefore(GameManager.Instance.World, ref _bpResult, primaryPlayer2, GameManager.Instance.World.GetGameRandom());
				blockValue = _bpResult.blockValue;
				blockValue.rotation = ((primaryPlayer2.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData) ? ((ItemClassBlock.ItemBlockInventoryData)primaryPlayer2.inventory.holdingItemData).rotation : blockValue.rotation);
				BlockTools.CubeRPC(GameManager.Instance, SelectionClrIdx, m_selectionStartPoint, m_SelectionEndPoint, blockValue, blockValue.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, holdingItemItemValue.TextureFullArray);
				EndUndo(0);
			}
		});
		nGuiAction5.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Private)] () => GameManager.Instance.IsEditMode() && SelectionActive);
		nGuiAction5.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction5.SetTooltip("selectionToolsFillSelectionTip");
		NGuiAction nGuiAction6 = new NGuiAction(Localization.Get("selectionToolsRandomFillSelection"), null);
		nGuiAction6.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			BeginUndo(0);
			BlockTools.CubeRandomRPC(GameManager.Instance, SelectionClrIdx, m_selectionStartPoint, m_SelectionEndPoint, GameManager.Instance.World.GetPrimaryPlayer().inventory.holdingItemItemValue.ToBlockValue(), 0.1f, EBlockRotationClasses.Basic90);
			EndUndo(0);
		});
		nGuiAction6.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Private)] () => SelectionActive);
		nGuiAction6.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction6.SetTooltip("selectionToolsRandomFillSelectionTip");
		NGuiAction nGuiAction7 = new NGuiAction(Localization.Get("selectionToolsUndo"), null);
		nGuiAction7.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			blockUndoRedo(_redo: false);
		});
		nGuiAction7.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Private)] () => undoQueue.Count > 0);
		nGuiAction7.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction7.SetTooltip("selectionToolsUndoTip");
		NGuiAction nGuiAction8 = new NGuiAction(Localization.Get("selectionToolsRedo"), null);
		nGuiAction8.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			blockUndoRedo(_redo: true);
		});
		nGuiAction8.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Private)] () => redoQueue.Count > 0);
		nGuiAction8.SetIsVisibleDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.Instance.IsEditMode());
		nGuiAction8.SetTooltip("selectionToolsRedoTip");
		actions = new Dictionary<string, NGuiAction>
		{
			{ "volumeBlocksEditing", nGuiAction },
			{ "copySleeperVolume", nGuiAction2 },
			{ "copyAirBlocks", nGuiAction3 },
			{
				"sep1",
				NGuiAction.Separator
			},
			{ "clearSelection", nGuiAction4 },
			{ "fillSelection", nGuiAction5 },
			{ "randomFillSelection", nGuiAction6 },
			{
				"sep2",
				NGuiAction.Separator
			},
			{ "undo", nGuiAction7 },
			{ "redo", nGuiAction8 }
		};
		foreach (var (_, action) in actions)
		{
			LocalPlayerUI.primaryUI.windowManager.AddGlobalAction(action);
		}
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
		if (hitInfo == null)
		{
			return;
		}
		Vector3i vector3i = ((GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed) ? hitInfo.hit.blockPos : hitInfo.lastBlockPos);
		bool flag = (InputUtils.IsMac ? ((ev.modifiers & EventModifiers.Command) != 0) : ((ev.modifiers & EventModifiers.Control) != 0));
		bool flag2 = (ev.modifiers & EventModifiers.Shift) != 0;
		switch (ev.keyCode)
		{
		case KeyCode.C:
			if (flag)
			{
				if (!SelectionActive)
				{
					SelectionStart = vector3i;
					SelectionEnd = vector3i;
				}
				blockCopy(clipboard);
			}
			break;
		case KeyCode.V:
			if (!flag)
			{
				break;
			}
			if (!flag2 && SelectionLockMode != 2)
			{
				if (SelectionActive && clipboard.size.Equals(Vector3i.one) && !SelectionSize.Equals(clipboard.size))
				{
					BeginUndo(0);
					BlockValue block = clipboard.GetBlock(0, 0, 0);
					WaterValue water = clipboard.GetWater(0, 0, 0);
					TextureFullArray texture = clipboard.GetTexture(0, 0, 0);
					BlockTools.CubeRPC(GameManager.Instance, SelectionClrIdx, m_selectionStartPoint, m_SelectionEndPoint, block, block.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, texture);
					BlockTools.CubeWaterRPC(GameManager.Instance, m_selectionStartPoint, m_SelectionEndPoint, water);
					EndUndo(0);
				}
				else if (SelectionActive && !SelectionSize.Equals(clipboard.size))
				{
					SelectionEnd = SelectionStart + clipboard.size - Vector3i.one;
				}
				else if (!SelectionActive)
				{
					SelectionStart = vector3i;
					SelectionEnd = SelectionStart + clipboard.size - Vector3i.one;
					SelectionActive = true;
				}
				else if (SelectionActive && SelectionSize.Equals(clipboard.size))
				{
					blockPaste(0, SelectionMin, clipboard);
				}
			}
			else if (SelectionLockMode != 2)
			{
				if (SelectionSize != clipboard.size)
				{
					SelectionEnd = SelectionStart + clipboard.size - Vector3i.one;
				}
				SelectionActive = true;
				SelectionLockMode = 2;
				createBlockPreviewFrom(clipboard);
			}
			else
			{
				SelectionLockMode = 0;
				blockPaste(0, SelectionMin, clipboard);
			}
			break;
		case KeyCode.Z:
			if (flag)
			{
				blockUndoRedo(_redo: false);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotatePreviewAroundY()
	{
		if (!(previewGORot2 == null))
		{
			previewGORot2.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * previewGORot2.transform.localRotation;
			clipboard.RotateY(_bLeft: false, 1);
			Vector3 vector = previewGORot2.transform.localRotation * offsetToMin;
			Vector3 vector2 = selectionRotCenter + vector;
			Vector3 vector3 = previewGORot2.transform.localRotation * offsetToMax;
			Vector3 vector4 = selectionRotCenter + vector3;
			Vector3i vector3i = (SelectionStart = new Vector3i(Utils.Fastfloor(Utils.FastMin(vector2.x, vector4.x)), Utils.Fastfloor(Utils.FastMin(vector2.y, vector4.y)), Utils.Fastfloor(Utils.FastMin(vector2.z, vector4.z))));
			SelectionEnd = vector3i + clipboard.size - Vector3i.one;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBlockPreview()
	{
		previewGORot3.transform.DestroyChildren();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createBlockPreviewFrom(Prefab _prefab)
	{
		if (previewGOParent == null)
		{
			previewGOParent = new GameObject("Preview");
			previewGOParent.transform.parent = null;
			previewGOParent.transform.localPosition = Vector3.zero;
			previewGORot1 = new GameObject("Rot1");
			previewGORot1.transform.parent = previewGOParent.transform;
			previewGORot2 = new GameObject("Rot2");
			previewGORot2.transform.parent = previewGORot1.transform;
			previewGORot3 = new GameObject("Rot3");
			previewGORot3.transform.parent = previewGORot2.transform;
		}
		else
		{
			removeBlockPreview();
		}
		ThreadManager.RunCoroutineSync(_prefab.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, previewGORot3.transform, "PrefabImposter", Vector3.zero, DynamicPrefabDecorator.PrefabPreviewLimit));
		Transform transform = previewGORot3.transform.Find("PrefabImposter");
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;
		Vector3 vector = new Vector3(_prefab.size.x / 2, 0f, _prefab.size.z / 2);
		previewGORot1.transform.position = SelectionMin.ToVector3() - Origin.position;
		previewGORot1.transform.rotation = Quaternion.identity;
		previewGORot2.transform.localPosition = vector;
		previewGORot2.transform.localRotation = Quaternion.identity;
		previewGORot3.transform.localPosition = -vector;
		previewGORot3.transform.localRotation = Quaternion.identity;
		vector = -vector;
		vector.y = -_prefab.size.y / 2;
		offsetToMax = vector + (_prefab.size - Vector3i.one).ToVector3() + Vector3.one * 0.5f;
		offsetToMin = vector + Vector3.one * 0.5f;
		selectionRotCenter = SelectionMin.ToVector3() - vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _newOrigin)
	{
		if (!(previewGORot1 == null))
		{
			previewGORot1.transform.position = SelectionMin.ToVector3() - Origin.position;
		}
	}

	public void RotateFocusedBlock(WorldRayHitInfo _hitInfo, PlayerActionsLocal _playerActions)
	{
		if (_hitInfo.bHitValid)
		{
			Vector3i vector3i = ((GameManager.Instance.World.IsEditor() && _playerActions.Run.IsPressed) ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos);
			BlockValue block = GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx].GetBlock(vector3i);
			if (block.Block.shape.IsRotatable)
			{
				block.rotation = block.Block.shape.Rotate(_bLeft: false, block.rotation);
				setBlock(_hitInfo.hit.clrIdx, vector3i, block);
			}
		}
	}

	public void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo, PlayerActionsLocal playerActions)
	{
		if (LocalPlayerUI.primaryUI.windowManager.IsInputActive())
		{
			return;
		}
		hitInfo = _hitInfo;
		Vector3i vector3i = ((_data.world.IsEditor() && playerActions.Run.IsPressed) ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos);
		if (_data is ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData)
		{
			BlockValue bv = itemBlockInventoryData.itemValue.ToBlockValue();
			bv.rotation = itemBlockInventoryData.rotation;
			itemBlockInventoryData.rotation = bv.Block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, bv, hitInfo.hit, itemBlockInventoryData.holdingEntity.position).blockValue.rotation;
		}
		if (!GameManager.Instance.IsEditMode() && !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		if (playerActions.SelectionSet.IsPressed)
		{
			if (GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx] == null || InputUtils.ControlKeyPressed)
			{
				return;
			}
			SelectionLockMode = 0;
			SelectionClrIdx = _hitInfo.hit.clrIdx;
			Vector3i vector3i2 = vector3i;
			if (!SelectionActive)
			{
				Vector3i selectionSize = SelectionSize;
				SelectionStart = vector3i2;
				if (SelectionLockMode == 1)
				{
					SelectionEnd = SelectionStart + selectionSize - Vector3i.one;
				}
				else
				{
					SelectionEnd = SelectionStart;
				}
				SelectionActive = true;
			}
			else
			{
				SelectionEnd = vector3i2;
			}
		}
		if (!GameManager.Instance.IsEditMode())
		{
			return;
		}
		if (playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed || playerActions.DensityM10.WasPressed || playerActions.DensityP10.WasPressed)
		{
			int num = ((playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed) ? 1 : 10);
			if (playerActions.DensityM1.WasPressed || playerActions.DensityM10.WasPressed)
			{
				num = -num;
			}
			if (InputUtils.ControlKeyPressed)
			{
				num *= 50;
			}
			BlockValue block = GameManager.Instance.World.GetBlock(_hitInfo.hit.clrIdx, vector3i);
			Block block2 = block.Block;
			if (block2.BlockTag == BlockTags.Door)
			{
				if (num > 0)
				{
					num = ((block.damage + num >= block2.MaxDamagePlusDowngrades) ? (block2.MaxDamagePlusDowngrades - block.damage - 1) : num);
				}
				block2.DamageBlock(GameManager.Instance.World, _hitInfo.hit.clrIdx, vector3i, block, num, -1);
			}
			else
			{
				int num2 = (SelectionActive ? GameManager.Instance.World.GetDensity(0, m_selectionStartPoint) : GameManager.Instance.World.GetDensity(_hitInfo.hit.clrIdx, vector3i));
				num2 += num;
				num2 = Utils.FastClamp(num2, MarchingCubes.DensityTerrain, MarchingCubes.DensityAir);
				if (!SelectionActive)
				{
					GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
					{
						new BlockChangeInfo(_hitInfo.hit.clrIdx, vector3i, (sbyte)num2)
					});
				}
				else
				{
					BlockTools.CubeDensityRPC(GameManager.Instance, m_selectionStartPoint, m_SelectionEndPoint, (sbyte)num2);
				}
			}
		}
		if ((!playerActions.FocusCopyBlock.WasPressed && (!playerActions.Secondary.WasPressed || !InputUtils.ControlKeyPressed)) || !GameManager.Instance.IsEditMode() || !_hitInfo.bHitValid || _hitInfo.hit.blockValue.isair)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		BlockValue blockValue = _hitInfo.hit.blockValue;
		if (blockValue.ischild)
		{
			Vector3i parentPos = blockValue.Block.multiBlockPos.GetParentPos(_hitInfo.hit.blockPos, blockValue);
			blockValue = GameManager.Instance.World.GetBlock(parentPos);
		}
		ItemStack itemStack = new ItemStack(blockValue.ToItemValue(), 99);
		if (blockValue.Block.GetAutoShapeType() != EAutoShapeType.Helper)
		{
			itemStack.itemValue.TextureFullArray = GameManager.Instance.World.ChunkCache.GetTextureFullArray(_hitInfo.hit.blockPos);
		}
		if (primaryPlayer.inventory.GetItemCount(itemStack.itemValue, _bConsiderTexture: true) == 0 && primaryPlayer.inventory.CanTakeItem(itemStack))
		{
			if (primaryPlayer.inventory.AddItem(itemStack, out var _slot) && primaryPlayer.inventory.GetItemDataInSlot(_slot) is ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData2)
			{
				itemBlockInventoryData2.damage = blockValue.damage;
			}
		}
		else if (_data is ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData3 && hasSameShape(blockValue.type, primaryPlayer.inventory.holdingItemItemValue.type))
		{
			itemBlockInventoryData3.rotation = blockValue.rotation;
			itemBlockInventoryData3.damage = blockValue.damage;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSameShape(int _blockId1, int _blockId2)
	{
		Block block = Block.list[_blockId1];
		Block block2 = Block.list[_blockId2];
		if (block.shape.GetType() != block2.shape.GetType())
		{
			return false;
		}
		if (block.shape is BlockShapeNew)
		{
			return block.Properties.Values["Model"] == block2.Properties.Values["Model"];
		}
		return true;
	}

	public bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		if ((_playerInput.Reload.IsPressed || _playerInput.PermanentActions.Reload.IsPressed) && _data is ItemClassBlock.ItemBlockInventoryData && Mathf.Abs(_scrollWheelInput) >= 0.001f)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.rotation = itemBlockInventoryData.itemValue.ToBlockValue().Block.BlockPlacementHelper.LimitRotation(itemBlockInventoryData.mode, ref itemBlockInventoryData.localRot, _data.hitInfo.hit, _scrollWheelInput > 0f, itemBlockInventoryData.itemValue.ToBlockValue(), itemBlockInventoryData.rotation);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i createBlockMoveVector(Vector3 _relPlayerAxis)
	{
		Vector3i zero = Vector3i.zero;
		return (!(Math.Abs(_relPlayerAxis.x) > Math.Abs(_relPlayerAxis.z))) ? new Vector3i(0f, 0f, Mathf.Sign(_relPlayerAxis.z)) : new Vector3i(Mathf.Sign(_relPlayerAxis.x), 0f, 0f);
	}

	public bool ExecuteUseAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!(_data is ItemClassBlock.ItemBlockInventoryData))
		{
			return false;
		}
		bool flag = GameManager.Instance.IsEditMode() || GameStats.GetInt(EnumGameStats.GameModeId) == 2;
		if (flag && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (_bReleased)
		{
			return false;
		}
		if (Time.time - lastBuildTime < Constants.cBuildIntervall)
		{
			return true;
		}
		lastBuildTime = Time.time;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
		EntityAlive holdingEntity = itemBlockInventoryData.holdingEntity;
		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;
		if (itemBlockInventoryData.item is ItemClassBlock itemClassBlock)
		{
			tags = itemClassBlock.GetBlock().Tags;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, tags) > 0f)
		{
			lastBuildTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return false;
		}
		HitInfoDetails hitInfoDetails = itemBlockInventoryData.hitInfo.hit.Clone();
		if (!itemBlockInventoryData.hitInfo.bHitValid)
		{
			return false;
		}
		hitInfoDetails.blockPos = ((flag && playerActions.Run.IsPressed) ? itemBlockInventoryData.hitInfo.hit.blockPos : itemBlockInventoryData.hitInfo.lastBlockPos);
		BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue();
		Block block = blockValue.Block;
		blockValue.damage = itemBlockInventoryData.damage;
		blockValue.rotation = itemBlockInventoryData.rotation;
		World world = GameManager.Instance.World;
		if (!GameManager.Instance.IsEditMode())
		{
			int placementDistanceSq = block.GetPlacementDistanceSq();
			if (hitInfoDetails.distanceSq > (float)placementDistanceSq)
			{
				return true;
			}
			Vector3i freePlacementPosition = block.GetFreePlacementPosition(world, itemBlockInventoryData.hitInfo.hit.clrIdx, hitInfoDetails.blockPos, blockValue, holdingEntity);
			if (!holdingEntity.IsGodMode.Value && GameUtils.IsColliderWithinBlock(freePlacementPosition, blockValue))
			{
				return true;
			}
			if (hitInfoDetails.blockPos == Vector3i.zero)
			{
				return true;
			}
		}
		_data.holdingEntity.RightArmAnimationUse = true;
		BlockPlacement.Result _bpResult = block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, blockValue, hitInfoDetails, itemBlockInventoryData.holdingEntity.position);
		block.OnBlockPlaceBefore(itemBlockInventoryData.world, ref _bpResult, itemBlockInventoryData.holdingEntity, itemBlockInventoryData.world.GetGameRandom());
		blockValue = _bpResult.blockValue;
		block = blockValue.Block;
		if (blockValue.damage == 0)
		{
			blockValue.damage = block.StartDamage;
			_bpResult.blockValue.damage = block.StartDamage;
		}
		if (!playerActions.Run.IsPressed)
		{
			_bpResult.blockPos = block.GetFreePlacementPosition(itemBlockInventoryData.holdingEntity.world, 0, _bpResult.blockPos, blockValue, itemBlockInventoryData.holdingEntity);
		}
		if (!block.CanPlaceBlockAt(itemBlockInventoryData.world, _bpResult.clrIdx, _bpResult.blockPos, blockValue))
		{
			itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning");
			return true;
		}
		if (!BlockLimitTracker.instance.CanAddBlock(blockValue, _bpResult.blockPos, out var _response))
		{
			switch (_response)
			{
			case eSetBlockResponse.PowerBlockLimitExceeded:
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddpowerblock");
				break;
			case eSetBlockResponse.StorageBlockLimitExceeded:
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddstorageblock");
				break;
			}
			return true;
		}
		if (!GameManager.Instance.IsEditMode())
		{
			if (block.IndexName == "lpblock")
			{
				if (!itemBlockInventoryData.world.CanPlaceLandProtectionBlockAt(itemBlockInventoryData.hitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer()))
				{
					itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning");
					return true;
				}
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_placed");
			}
			else if (!itemBlockInventoryData.world.CanPlaceBlockAt(itemBlockInventoryData.hitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer()))
			{
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning");
				return true;
			}
		}
		BiomeDefinition biome = itemBlockInventoryData.world.GetBiome(_bpResult.blockPos.x, _bpResult.blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(_bpResult.blockValue.type))
		{
			_bpResult.blockValue.type = biome.Replacements[_bpResult.blockValue.type];
		}
		addToUndo(_data.hitInfo.hit.clrIdx, _bpResult.blockPos, GameManager.Instance.World.GetBlock(_bpResult.blockPos));
		if (Block.list[itemBlockInventoryData.itemValue.type].SelectAlternates)
		{
			if (itemBlockInventoryData.itemValue.TextureFullArray.IsDefault)
			{
				block.PlaceBlock(itemBlockInventoryData.world, _bpResult, itemBlockInventoryData.holdingEntity);
			}
			else
			{
				BlockChangeInfo blockChangeInfo = new BlockChangeInfo(0, _bpResult.blockPos, blockValue, itemBlockInventoryData.holdingEntity.entityId);
				blockChangeInfo.textureFull = itemBlockInventoryData.itemValue.TextureFullArray;
				blockChangeInfo.bChangeTexture = true;
				GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { blockChangeInfo });
			}
		}
		else if (itemBlockInventoryData.itemValue.TextureFullArray.IsDefault)
		{
			block.PlaceBlock(itemBlockInventoryData.world, _bpResult, itemBlockInventoryData.holdingEntity);
		}
		else
		{
			BlockChangeInfo blockChangeInfo2 = new BlockChangeInfo(0, _bpResult.blockPos, blockValue, itemBlockInventoryData.holdingEntity.entityId);
			blockChangeInfo2.textureFull = itemBlockInventoryData.itemValue.TextureFullArray;
			blockChangeInfo2.bChangeTexture = true;
			GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { blockChangeInfo2 });
		}
		QuestEventManager.Current.BlockPlaced(block.GetBlockName(), _bpResult.blockPos);
		itemBlockInventoryData.holdingEntity.RightArmAnimationUse = true;
		itemBlockInventoryData.lastBuildTime = Time.time;
		itemBlockInventoryData.holdingEntity.MinEventContext.ItemActionData = itemBlockInventoryData.actionData[0];
		itemBlockInventoryData.holdingEntity.MinEventContext.BlockValue = _bpResult.blockValue;
		itemBlockInventoryData.holdingEntity.MinEventContext.Position = _bpResult.pos;
		itemBlockInventoryData.holdingEntity.FireEvent(MinEventTypes.onSelfPlaceBlock);
		GameManager.Instance.StartCoroutine(decInventoryLater(itemBlockInventoryData, itemBlockInventoryData.holdingEntity.inventory.holdingItemIdx));
		if (!block.shape.IsOmitTerrainSnappingUp && !block.IsTerrainDecoration)
		{
			itemBlockInventoryData.world.ChunkCache.SnapTerrainToPositionAroundRPC(itemBlockInventoryData.world, itemBlockInventoryData.hitInfo.lastBlockPos - Vector3i.up);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator decInventoryLater(ItemInventoryData data, int index)
	{
		data.holdingEntity.inventory.WaitForSecondaryRelease = data.holdingEntity.inventory.holdingItemStack.count == 1;
		yield return new WaitForSeconds(0.1f);
		if (!GameManager.Instance.IsEditMode())
		{
			ItemStack itemStack = data.holdingEntity.inventory.GetItem(index).Clone();
			if (itemStack.count > 0)
			{
				itemStack.count--;
			}
			data.holdingEntity.inventory.SetItem(index, itemStack);
		}
		BlockValue blockValue = data.itemValue.ToBlockValue();
		string clipName = "placeblock";
		Block block = blockValue.Block;
		if (block.CustomPlaceSound != null)
		{
			clipName = block.CustomPlaceSound;
		}
		data.holdingEntity.PlayOneShot(clipName);
	}

	public bool ExecuteAttackAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!_bReleased)
		{
			return false;
		}
		bool flag = false;
		if (GameManager.Instance.IsEditMode() && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (!playerActions.SelectionSet.IsPressed && SelectionActive)
		{
			if (!playerActions.Drop.IsPressed)
			{
				flag = flag || SelectionActive;
				if (SelectionLockMode == 1)
				{
					Vector3i selectionSize = SelectionSize;
					SelectionStart = _data.hitInfo.hit.blockPos;
					SelectionEnd = SelectionStart + selectionSize - Vector3i.one;
				}
				else if (SelectionLockMode == 0)
				{
					SelectionActive = false;
				}
			}
		}
		else if (GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed && _data.hitInfo.bHitValid)
		{
			Vector3i blockPos = (playerActions.Run.IsPressed ? _data.hitInfo.hit.blockPos : _data.hitInfo.lastBlockPos);
			setBlock(_data.hitInfo.hit.clrIdx, blockPos, BlockValue.Air);
			flag = true;
		}
		else if (_data is ItemClassBlock.ItemBlockInventoryData)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.itemValue.ToBlockValue().Block.RotateHoldingBlock(itemBlockInventoryData, _increaseRotation: true);
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateSelectionAroundY()
	{
		Vector3i vector3i = new Vector3i(Mathf.Abs(m_selectionStartPoint.x - m_SelectionEndPoint.x), Mathf.Abs(m_selectionStartPoint.y - m_SelectionEndPoint.y), Mathf.Abs(m_selectionStartPoint.z - m_SelectionEndPoint.z));
		Vector3i vector3i2 = new Vector3i(Mathf.Min(m_selectionStartPoint.x, m_SelectionEndPoint.x), Mathf.Min(m_selectionStartPoint.y, m_SelectionEndPoint.y), Mathf.Min(m_selectionStartPoint.z, m_SelectionEndPoint.z));
		Prefab prefab = BlockTools.CopyIntoStorage(GameManager.Instance, vector3i2, vector3i2 + vector3i);
		BeginUndo(0);
		Prefab prefab2 = new Prefab(prefab.size);
		prefab2.bCopyAirBlocks = true;
		prefab2.CopyIntoRPC(GameManager.Instance, vector3i2);
		prefab.RotateY(_bLeft: false, 1);
		prefab.CopyIntoRPC(GameManager.Instance, vector3i2, copyPasteAirBlocks);
		SelectionStart = vector3i2;
		SelectionEnd = vector3i2 + prefab.size - Vector3i.one;
		EndUndo(0);
	}

	public void SelectionSizeSet(Vector3i _size)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSelection()
	{
		Vector3 v = SelectionSize.ToVector3();
		Vector3 v2 = new Vector3(Mathf.Min(m_selectionStartPoint.x, m_SelectionEndPoint.x), Mathf.Min(m_selectionStartPoint.y, m_SelectionEndPoint.y), Mathf.Min(m_selectionStartPoint.z, m_SelectionEndPoint.z));
		SelectionBox.SetPositionAndSize(new Vector3i(v2), new Vector3i(v));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setBlock(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = GameManager.Instance.World.GetBlock(_clrIdx, _blockPos);
		if (block.rawData == _blockValue.rawData)
		{
			return false;
		}
		TextureFullArray textureFullArray = GameManager.Instance.World.GetTextureFullArray(_blockPos.x, _blockPos.y, _blockPos.z);
		undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, block, MarchingCubes.DensityAir, textureFullArray)
		});
		if (undoQueue.Count > 100)
		{
			undoQueue.RemoveAt(0);
		}
		if (_blockValue.Block.shape.IsTerrain())
		{
			GameManager.Instance.World.SetBlockRPC(_clrIdx, _blockPos, _blockValue, MarchingCubes.DensityTerrain);
		}
		else
		{
			GameManager.Instance.World.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToUndo(int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue)
	{
		TextureFullArray textureFullArray = GameManager.Instance.World.GetTextureFullArray(_blockPos.x, _blockPos.y, _blockPos.z);
		undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _oldBlockValue, MarchingCubes.DensityAir, textureFullArray)
		});
		if (undoQueue.Count > 100)
		{
			undoQueue.RemoveAt(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockUndoRedo(bool _redo)
	{
		List<List<BlockChangeInfo>> list = (_redo ? redoQueue : undoQueue);
		if (list.Count != 0)
		{
			List<BlockChangeInfo> list2 = list[list.Count - 1];
			BeginUndo(list2[0].clrIdx);
			GameManager.Instance.SetBlocksRPC(list2);
			list.RemoveAt(list.Count - 1);
			EndUndo(list2[0].clrIdx, !_redo);
		}
	}

	public void BeginUndo(int _clrIdx)
	{
		undoChanges = new List<BlockChangeInfo>();
		undoClrIdx = _clrIdx;
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			chunkCluster.OnBlockChangedDelegates += undoBlockChangeDelegate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void undoBlockChangeDelegate(Vector3i pos, BlockValue bvOld, sbyte oldDens, TextureFullArray oldTex, BlockValue bvNew)
	{
		if (undoChanges != null && !bvOld.ischild)
		{
			undoChanges.Add(new BlockChangeInfo(undoClrIdx, pos, bvOld, oldDens, oldTex));
		}
	}

	public void EndUndo(int _clrIdx, bool _bRedo = false)
	{
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			chunkCluster.OnBlockChangedDelegates -= undoBlockChangeDelegate;
		}
		if (undoChanges.Count <= 0)
		{
			undoChanges = null;
			return;
		}
		undoChanges.Reverse();
		List<List<BlockChangeInfo>> list = (_bRedo ? redoQueue : undoQueue);
		list.Add(undoChanges);
		if (list.Count > 100)
		{
			list.RemoveAt(0);
		}
		undoChanges = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockCopy(Prefab _storage)
	{
		return _storage.CopyFromWorldWithEntities(GameManager.Instance.World, SelectionStart, SelectionEnd, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockPaste(int _clrIdx, Vector3i _destPos, Prefab _storage)
	{
		BeginUndo(0);
		_storage.CopyIntoRPC(GameManager.Instance, _destPos, copyPasteAirBlocks);
		SelectionActive = true;
		SelectionStart = _destPos;
		SelectionEnd = _destPos + _storage.size - Vector3i.one;
		EndUndo(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i sizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void union(Vector3i _pos1Start, Vector3i _pos1End, Vector3i _pos2Start, Vector3i _pos2End, out Vector3i _unionStart, out Vector3i _unionEnd)
	{
		_unionStart = new Vector3i(Utils.FastMin(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMin(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMin(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
		_unionEnd = new Vector3i(Utils.FastMax(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMax(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMax(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPrefabActive()
	{
		if (GameManager.Instance.GetDynamicPrefabDecorator() != null)
		{
			return GameManager.Instance.GetDynamicPrefabDecorator().ActivePrefab != null;
		}
		return false;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		SelectionBox.SetVisible(_bActivated);
		if (!_bActivated)
		{
			SelectionLockMode = 0;
		}
		return true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		Vector3i vector3i = new Vector3i(_moveVector);
		_ = Vector3i.zero;
		_ = SelectionLockMode;
		_ = 2;
		SelectionStart += vector3i;
		SelectionEnd += vector3i;
		selectionRotCenter += vector3i.ToVector3();
		if (SelectionLockMode == 2)
		{
			previewGORot1.transform.position += _moveVector;
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (SelectionLockMode == 2)
		{
			SelectionLockMode = 0;
			return;
		}
		if (_dEast != 0 && (_dEast >= 0 || SelectionSize.x > 1))
		{
			if (SelectionEnd.x > SelectionStart.x)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x + _dEast, SelectionEnd.y, SelectionEnd.z);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x + _dEast, SelectionStart.y, SelectionStart.z);
			}
		}
		if (_dWest != 0 && (_dWest >= 0 || SelectionSize.x > 1))
		{
			if (SelectionEnd.x <= SelectionStart.x)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x - _dWest, SelectionEnd.y, SelectionEnd.z);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x - _dWest, SelectionStart.y, SelectionStart.z);
			}
		}
		if (_dTop != 0 && (_dTop >= 0 || SelectionSize.y > 1))
		{
			if (SelectionEnd.y > SelectionStart.y)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x, SelectionEnd.y + _dTop, SelectionEnd.z);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x, SelectionStart.y + _dTop, SelectionStart.z);
			}
		}
		if (_dBottom != 0 && (_dBottom >= 0 || SelectionSize.y > 1))
		{
			if (SelectionEnd.y <= SelectionStart.y)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x, SelectionEnd.y - _dBottom, SelectionEnd.z);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x, SelectionStart.y - _dBottom, SelectionStart.z);
			}
		}
		if (_dNorth != 0 && (_dNorth >= 0 || SelectionSize.z > 1))
		{
			if (SelectionEnd.z > SelectionStart.z)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x, SelectionEnd.y, SelectionEnd.z + _dNorth);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x, SelectionStart.y, SelectionStart.z + _dNorth);
			}
		}
		if (_dSouth != 0 && (_dSouth >= 0 || SelectionSize.z > 1))
		{
			if (SelectionEnd.z <= SelectionStart.z)
			{
				SelectionEnd = new Vector3i(SelectionEnd.x, SelectionEnd.y, SelectionEnd.z - _dSouth);
			}
			else
			{
				SelectionStart = new Vector3i(SelectionStart.x, SelectionStart.y, SelectionStart.z - _dSouth);
			}
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _selAxis)
	{
		EnumMirrorAlong axis = EnumMirrorAlong.XAxis;
		if (_selAxis.y != 0)
		{
			axis = EnumMirrorAlong.YAxis;
		}
		else if (_selAxis.z != 0)
		{
			axis = EnumMirrorAlong.ZAxis;
		}
		if (previewGORot3 != null && previewGORot3.transform.childCount > 0)
		{
			clipboard.Mirror(axis);
			removeBlockPreview();
			createBlockPreviewFrom(clipboard);
		}
		else
		{
			Prefab prefab = new Prefab();
			prefab.CopyFromWorldWithEntities(GameManager.Instance.World, SelectionStart, SelectionEnd, null);
			prefab.Mirror(axis);
			prefab.CopyIntoRPC(GameManager.Instance, SelectionMin, copyPasteAirBlocks);
		}
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria != EnumSelectionBoxAvailabilities.CanResize)
		{
			return _criteria == EnumSelectionBoxAvailabilities.CanMirror;
		}
		return true;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
		if (SelectionLockMode == 2)
		{
			rotatePreviewAroundY();
		}
		else
		{
			rotateSelectionAroundY();
		}
	}

	public string GetDebugOutput()
	{
		if (SelectionActive)
		{
			return $"Selection pos/size: {SelectionStart.ToString()}/{SelectionSize.ToString()}";
		}
		return "-";
	}

	public Dictionary<string, NGuiAction> GetActions()
	{
		return actions;
	}

	public void LoadPrefabIntoClipboard(Prefab _prefab)
	{
		clipboard = _prefab;
		SelectionLockMode = 2;
		if (SelectionSize != clipboard.size)
		{
			SelectionEnd = SelectionStart + clipboard.size - Vector3i.one;
		}
		SelectionActive = true;
		createBlockPreviewFrom(clipboard);
	}
}
