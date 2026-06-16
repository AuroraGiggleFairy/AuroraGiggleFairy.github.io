using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TriggerProperties : XUiController, ISelectionBoxCallback
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EOpenedFor
	{
		Block,
		TriggerVolume
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public EOpenedFor openedFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTrigger blockTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggers = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggeredBy = true;

	public Prefab Prefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			prefab = value;
			if (prefab != null && prefab.TriggerLayers.Count == 0)
			{
				prefab.AddInitialTriggerLayers();
			}
		}
	}

	public Vector3i BlockPos
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			blockPos = value;
			blockTrigger = BlockChunk.GetBlockTrigger(World.toBlock(blockPos));
		}
	}

	public Chunk BlockChunk
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (Chunk)GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(blockPos);
		}
	}

	public List<byte> TriggersIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox _box;
			PrefabTriggerVolume _volume;
			PrefabInstance _prefabInstance;
			int _volumeIndex;
			return openedFor switch
			{
				EOpenedFor.Block => blockTrigger?.TriggersIndices, 
				EOpenedFor.TriggerVolume => PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out _box, out _volume, out _prefabInstance, out _volumeIndex) ? _volume.TriggersIndices : null, 
				_ => null, 
			};
		}
	}

	public List<byte> TriggeredByIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return blockTrigger?.TriggeredByIndices;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		triggersList = GetChildById("triggers") as XUiC_PrefabTriggerEditorList;
		if (triggersList != null)
		{
			triggersList.SelectionChanged += TriggersList_SelectionChanged;
			triggersList.GetCurrentTriggerIndicesList = [PublicizedFrom(EAccessModifier.Private)] () => TriggersIndices;
			triggersList.GetCurrentPrefabTriggerLayers = [PublicizedFrom(EAccessModifier.Private)] () => prefab?.TriggerLayers;
		}
		XUiController childById = GetChildById("addTriggersButton");
		if (childById != null)
		{
			childById.OnPress += TriggerOnAddTriggersPressed;
		}
		triggeredByList = GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (triggeredByList != null)
		{
			triggeredByList.SelectionChanged += TriggeredByList_SelectionChanged;
			triggeredByList.GetCurrentTriggerIndicesList = [PublicizedFrom(EAccessModifier.Private)] () => TriggeredByIndices;
			triggeredByList.GetCurrentPrefabTriggerLayers = [PublicizedFrom(EAccessModifier.Private)] () => prefab?.TriggerLayers;
		}
		XUiController childById2 = GetChildById("addTriggeredByButton");
		if (childById2 != null)
		{
			childById2.OnPress += TriggerOnAddTriggersPressed;
		}
		XUiController childById3 = GetChildById("exclude");
		if (childById3 != null)
		{
			childById3.OnPress += triggerExclude_OnPressed;
		}
		childById3 = GetChildById("operation");
		if (childById3 != null)
		{
			childById3.OnPress += triggerOperation_OnPressed;
		}
		childById3 = GetChildById("unlock");
		if (childById3 != null)
		{
			childById3.OnPress += triggerUnlock_OnPressed;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.CategoryTriggerVolume.SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerExclude_OnPressed(XUiController _controller, int _button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.ExcludeIcon = !blockTrigger.ExcludeIcon;
			setChunkModified();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerOperation_OnPressed(XUiController _controller, int _button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.UseOrForMultipleTriggers = !blockTrigger.UseOrForMultipleTriggers;
			setChunkModified();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerUnlock_OnPressed(XUiController _controller, int _button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.Unlock = !blockTrigger.Unlock;
			setChunkModified();
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setChunkModified()
	{
		PrefabEditModeManager.Instance.NeedsSaving = true;
		BlockChunk.isModified = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.CategoryTriggerVolume.SetCallback(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed(XUiController _sender, int _mouseButton)
	{
		prefab.AddNewTriggerLayer();
		triggersList.RebuildList();
		triggeredByList.RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggersList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry != null)
		{
			byte triggerLayer = _newEntry.TriggerLayer;
			if (openedFor == EOpenedFor.TriggerVolume)
			{
				handleTriggerVolumeSetting(triggerLayer, _isTriggers: true);
			}
			else
			{
				handleTriggersSetting(triggerLayer, _isTriggers: true, GameManager.Instance.World, blockPos);
			}
			_newEntry.UiDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry != null)
		{
			byte triggerLayer = _newEntry.TriggerLayer;
			if (openedFor == EOpenedFor.TriggerVolume)
			{
				handleTriggerVolumeSetting(triggerLayer, _isTriggers: false);
			}
			else
			{
				handleTriggersSetting(triggerLayer, _isTriggers: false, GameManager.Instance.World, blockPos);
			}
			_newEntry.UiDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		Prefab = null;
		blockTrigger = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "triggers_enabled":
			_value = showTriggers.ToString();
			return true;
		case "triggeredby_enabled":
			_value = showTriggeredBy.ToString();
			return true;
		case "excludeTickmarkSelected":
			_value = ((blockTrigger != null) ? blockTrigger.ExcludeIcon.ToString() : "false");
			return true;
		case "operationTickmarkSelected":
			_value = ((blockTrigger != null) ? blockTrigger.UseOrForMultipleTriggers.ToString() : "false");
			return true;
		case "unlockTickmarkSelected":
			_value = ((blockTrigger != null) ? blockTrigger.Unlock.ToString() : "false");
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleTriggersSetting(byte _triggerLayer, bool _isTriggers, World _world, Vector3i _blockPos)
	{
		if (!_world.IsEditor())
		{
			return;
		}
		Chunk blockChunk = BlockChunk;
		blockTrigger = blockChunk.GetBlockTrigger(World.toBlock(_blockPos));
		if (_triggerLayer == 0)
		{
			if (blockTrigger != null)
			{
				blockChunk.RemoveBlockTrigger(blockTrigger);
				blockTrigger = null;
			}
		}
		else
		{
			if (blockTrigger == null)
			{
				blockTrigger = new BlockTrigger(blockChunk)
				{
					LocalChunkPos = World.toBlock(_blockPos)
				};
				blockChunk.AddBlockTrigger(blockTrigger);
			}
			if (_isTriggers)
			{
				blockTrigger.ToggleTriggersFlag(_triggerLayer);
			}
			else
			{
				blockTrigger.ToggleTriggeredByFlag(_triggerLayer);
			}
			if (!blockTrigger.HasAnyTriggers() && !blockTrigger.HasAnyTriggeredBy() && blockTrigger != null)
			{
				blockChunk.RemoveBlockTrigger(blockTrigger);
				blockTrigger = null;
			}
		}
		setChunkModified();
		blockTrigger?.TriggerUpdated(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleTriggerVolumeSetting(byte _triggerLayer, bool _isTriggers)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex))
		{
			if (_isTriggers)
			{
				PrefabTriggerVolume prefabTriggerVolume = _volume.CloneGeneric();
				prefabTriggerVolume.ToggleTriggersFlag(_triggerLayer);
				PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeIndex, prefabTriggerVolume);
			}
			blockTrigger?.TriggerUpdated(null);
		}
	}

	public static void Show(XUi _xui, Vector3i _blockPos, bool _showTriggers, bool _showTriggeredBy)
	{
		XUiC_TriggerProperties childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_TriggerProperties>();
		childByType.openedFor = EOpenedFor.Block;
		childByType.Prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		childByType.BlockPos = _blockPos;
		childByType.showTriggers = _showTriggers;
		childByType.showTriggeredBy = _showTriggeredBy;
		childByType.RefreshBindings();
		_xui.playerUI.windowManager.Open(ID, _bModal: true);
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		PrefabVolumeManager.Instance.SelectionBoxMoved(PrefabVolumeAbs.EVolumeType.Trigger, _box, _moveVector);
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		PrefabVolumeManager.Instance.SelectionBoxSized(PrefabVolumeAbs.EVolumeType.Trigger, _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		if (!PrefabVolumeManager.GetPrefabIdAndVolumeId(_box.name, out var _, out var _))
		{
			return false;
		}
		return PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Trigger, _box, _checkCanDeleteOnly);
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria != EnumSelectionBoxAvailabilities.CanShowProperties)
		{
			return _criteria == EnumSelectionBoxAvailabilities.CanResize;
		}
		return true;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out var _, out var _, out var _prefabInstance, out var _))
		{
			openedFor = EOpenedFor.TriggerVolume;
			Prefab = _prefabInstance.prefab;
			showTriggers = true;
			showTriggeredBy = false;
			_windowManager.Open(windowGroup, _bModal: true);
		}
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
	}
}
