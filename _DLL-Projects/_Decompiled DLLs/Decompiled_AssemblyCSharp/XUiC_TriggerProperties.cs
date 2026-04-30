using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TriggerProperties : XUiController, ISelectionBoxCallback
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	public BlockTrigger blockTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.PrefabTriggerVolume triggerVolume;

	public bool ShowTriggers = true;

	public bool ShowTriggeredBy = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance selectedPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> prefabGroupsList = new List<string>();

	public Vector3i BlockPos
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return blockPos;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			blockPos = value;
			triggerVolume = null;
			SetupTrigger();
		}
	}

	public Prefab.PrefabTriggerVolume TriggerVolume
	{
		get
		{
			return triggerVolume;
		}
		set
		{
			triggerVolume = value;
			blockTrigger = null;
		}
	}

	public List<byte> TriggersIndices
	{
		get
		{
			if (blockTrigger != null)
			{
				return blockTrigger.TriggersIndices;
			}
			if (triggerVolume != null)
			{
				return triggerVolume.TriggersIndices;
			}
			return null;
		}
	}

	public List<byte> TriggeredByIndices
	{
		get
		{
			if (blockTrigger != null)
			{
				return blockTrigger.TriggeredByIndices;
			}
			return null;
		}
	}

	public Prefab Prefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return prefab;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != prefab)
			{
				prefab = value;
				triggersList.EditPrefab = value;
				triggersList.Owner = this;
				triggersList.IsTriggers = true;
				triggeredByList.EditPrefab = value;
				triggeredByList.Owner = this;
				triggeredByList.IsTriggers = false;
				if (prefab.TriggerLayers.Count == 0)
				{
					prefab.AddInitialTriggerLayers();
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		triggersList = GetChildById("triggers") as XUiC_PrefabTriggerEditorList;
		if (triggersList != null)
		{
			triggersList.SelectionChanged += TriggersList_SelectionChanged;
		}
		XUiController childById = GetChildById("addTriggersButton");
		if (childById != null)
		{
			childById.OnPress += HandleAddTriggersEntry;
		}
		triggeredByList = GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (triggeredByList != null)
		{
			triggeredByList.SelectionChanged += TriggeredByList_SelectionChanged;
		}
		XUiController childById2 = GetChildById("addTriggeredByButton");
		if (childById2 != null)
		{
			childById2.OnPress += HandleAddTriggeredByEntry;
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
			SelectionBoxManager.Instance.GetCategory("TriggerVolume").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerExclude_OnPressed(XUiController controller, int button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.ExcludeIcon = !blockTrigger.ExcludeIcon;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[clrIdx].GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
			setChunkModified(chunkModified);
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerOperation_OnPressed(XUiController controller, int button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.UseOrForMultipleTriggers = !blockTrigger.UseOrForMultipleTriggers;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[clrIdx].GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
			setChunkModified(chunkModified);
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerUnlock_OnPressed(XUiController controller, int button)
	{
		if (blockTrigger != null)
		{
			blockTrigger.Unlock = !blockTrigger.Unlock;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[clrIdx].GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
			setChunkModified(chunkModified);
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setChunkModified(Chunk _chunk)
	{
		PrefabEditModeManager.Instance.NeedsSaving = true;
		_chunk.isModified = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("TriggerVolume").SetCallback(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggersEntry(XUiController _sender, int _mouseButton)
	{
		TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggeredByEntry(XUiController _sender, int _mouseButton)
	{
		TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenInEditor_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		Process.Start(prefab.location.FullPathNoExtension + ".xml");
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed()
	{
		prefab.AddNewTriggerLayer();
		triggersList.RebuildList();
		triggeredByList.RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validTriggerName(byte val)
	{
		return !prefab.TriggerLayers.Contains(val);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggersList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		byte _result = 0;
		if (StringParsers.TryParseUInt8(_newEntry.GetEntry().name, out _result))
		{
			if (triggerVolume != null)
			{
				HandleTriggersSetting(_result, isTriggers: true, GameManager.Instance.World);
			}
			else
			{
				HandleTriggersSetting(_result, isTriggers: true, GameManager.Instance.World, clrIdx, blockPos);
			}
		}
		_newEntry.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		byte _result = 0;
		if (StringParsers.TryParseUInt8(_newEntry.GetEntry().name, out _result))
		{
			if (triggerVolume != null)
			{
				HandleTriggersSetting(_result, isTriggers: false, GameManager.Instance.World);
			}
			else
			{
				HandleTriggersSetting(_result, isTriggers: false, GameManager.Instance.World, clrIdx, blockPos);
			}
		}
		_newEntry.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		prefab = null;
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
		case "title":
			_value = Localization.Get("xuiPrefabProperties") + ": " + ((prefab != null) ? prefab.PrefabName : "-");
			return true;
		case "triggers_enabled":
			_value = ShowTriggers.ToString();
			return true;
		case "triggeredby_enabled":
			_value = ShowTriggeredBy.ToString();
			return true;
		case "window_height":
			_value = ((ShowTriggeredBy && ShowTriggers) ? "752" : "396");
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
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTrigger()
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.ChunkClusters[clrIdx].GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
		blockTrigger = chunk.GetBlockTrigger(World.toBlock(blockPos));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(byte triggerLayer, bool isTriggers, World _world, int _cIdx, Vector3i _blockPos)
	{
		if (!_world.IsEditor())
		{
			return;
		}
		Chunk chunk = (Chunk)_world.ChunkClusters[_cIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
		blockTrigger = chunk.GetBlockTrigger(World.toBlock(_blockPos));
		if (triggerLayer == 0)
		{
			if (blockTrigger != null)
			{
				chunk.RemoveBlockTrigger(blockTrigger);
				blockTrigger = null;
			}
		}
		else
		{
			if (blockTrigger == null)
			{
				blockTrigger = new BlockTrigger(chunk);
				if (isTriggers)
				{
					if (blockTrigger.HasTriggers(triggerLayer))
					{
						blockTrigger.RemoveTriggersFlag(triggerLayer);
					}
					else
					{
						blockTrigger.SetTriggersFlag(triggerLayer);
					}
				}
				else if (blockTrigger.HasTriggeredBy(triggerLayer))
				{
					blockTrigger.RemoveTriggeredByFlag(triggerLayer);
				}
				else
				{
					blockTrigger.SetTriggeredByFlag(triggerLayer);
				}
				blockTrigger.LocalChunkPos = World.toBlock(_blockPos);
				chunk.AddBlockTrigger(blockTrigger);
			}
			else if (isTriggers)
			{
				if (blockTrigger.HasTriggers(triggerLayer))
				{
					blockTrigger.RemoveTriggersFlag(triggerLayer);
				}
				else
				{
					blockTrigger.SetTriggersFlag(triggerLayer);
				}
			}
			else if (blockTrigger.HasTriggeredBy(triggerLayer))
			{
				blockTrigger.RemoveTriggeredByFlag(triggerLayer);
			}
			else
			{
				blockTrigger.SetTriggeredByFlag(triggerLayer);
			}
			if (!blockTrigger.HasAnyTriggers() && !blockTrigger.HasAnyTriggeredBy() && blockTrigger != null)
			{
				chunk.RemoveBlockTrigger(blockTrigger);
				blockTrigger = null;
			}
			setChunkModified(chunk);
		}
		if (blockTrigger != null)
		{
			blockTrigger.TriggerUpdated(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(byte triggerLayer, bool isTriggers, World _world)
	{
		if (!_world.IsEditor())
		{
			return;
		}
		if (isTriggers)
		{
			if (triggerVolume.HasTriggers(triggerLayer))
			{
				triggerVolume.RemoveTriggersFlag(triggerLayer);
			}
			else
			{
				triggerVolume.SetTriggersFlag(triggerLayer);
			}
		}
		if (blockTrigger != null)
		{
			blockTrigger.TriggerUpdated(null);
		}
	}

	public static void Show(XUi _xui, int _clrIdx, Vector3i _blockPos, bool _showTriggers, bool _showTriggeredBy)
	{
		XUiC_TriggerProperties childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_TriggerProperties>();
		childByType.Prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		childByType.clrIdx = _clrIdx;
		childByType.BlockPos = _blockPos;
		childByType.ShowTriggers = _showTriggers;
		childByType.ShowTriggeredBy = _showTriggeredBy;
		childByType.RefreshBindings();
		_xui.playerUI.windowManager.Open(ID, _bModal: true);
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
			{
				selIdx = _volumeId;
			}
		}
		else
		{
			selectedPrefabInstance = null;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefabIdAndVolumeId(string _name, out int _prefabInstanceId, out int _volumeId)
	{
		_prefabInstanceId = (_volumeId = 0);
		string[] array = _name.Split('.');
		if (array.Length > 1)
		{
			string[] array2 = array[1].Split('_');
			if (array2.Length > 1 && int.TryParse(array2[1], out _volumeId) && int.TryParse(array2[0], out _prefabInstanceId))
			{
				selectedPrefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				Prefab = selectedPrefabInstance.prefab;
				return true;
			}
		}
		return false;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (selectedPrefabInstance != null)
		{
			Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume(selectedPrefabInstance.prefab.TriggerVolumes[selIdx]);
			prefabTriggerVolume.startPos += new Vector3i(_moveVector);
			PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(selectedPrefabInstance.id, selIdx, prefabTriggerVolume);
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (selectedPrefabInstance != null)
		{
			Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume(selectedPrefabInstance.prefab.TriggerVolumes[selIdx]);
			prefabTriggerVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabTriggerVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size = prefabTriggerVolume.size;
			if (size.x < 2)
			{
				size = new Vector3i(1, size.y, size.z);
			}
			if (size.y < 2)
			{
				size = new Vector3i(size.x, 1, size.z);
			}
			if (size.z < 2)
			{
				size = new Vector3i(size.x, size.y, 1);
			}
			prefabTriggerVolume.size = size;
			PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(selectedPrefabInstance.id, selIdx, prefabTriggerVolume);
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		foreach (LocalPlayerUI playerUI in LocalPlayerUI.PlayerUIs)
		{
			if (playerUI.windowManager.IsModalWindowOpen())
			{
				SelectionBoxManager.Instance.SetActive(_category, _name, _bActive: true);
				return false;
			}
		}
		if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
		{
			Prefab.PrefabTriggerVolume volumeSettings = new Prefab.PrefabTriggerVolume(selectedPrefabInstance.prefab.TriggerVolumes[_volumeId]);
			PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(selectedPrefabInstance.id, _volumeId, volumeSettings, remove: true);
			return true;
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria != EnumSelectionBoxAvailabilities.CanShowProperties)
		{
			return _criteria == EnumSelectionBoxAvailabilities.CanResize;
		}
		return true;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		if (SelectionBoxManager.Instance.GetSelected(out var _selectedCategory, out var _selectedName) && _selectedCategory.Equals("TriggerVolume") && getPrefabIdAndVolumeId(_selectedName, out var _, out var _volumeId))
		{
			Prefab.PrefabTriggerVolume prefabTriggerVolume = selectedPrefabInstance.prefab.TriggerVolumes[_volumeId];
			ShowTriggers = true;
			ShowTriggeredBy = false;
			TriggerVolume = prefabTriggerVolume;
			_windowManager.SwitchVisible(ID);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}
}
