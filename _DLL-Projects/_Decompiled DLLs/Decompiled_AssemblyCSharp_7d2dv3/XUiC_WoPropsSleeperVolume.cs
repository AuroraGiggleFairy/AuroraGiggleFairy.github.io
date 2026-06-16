using System;
using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperVolume : XUiController, ISelectionBoxCallback, ISelectionCategoryCallback
{
	public struct VolumeStats(int _index, Vector3i _volumePos, Vector3i _worldPos, Vector3i _size, short _groupId, string _groupName, int _sleeperCount, int _spawnCountMin, int _spawnCountMax, bool _isPriority, bool _isQuestExclude, SleeperVolume.ETriggerType _triggerType, string _minScript)
	{
		public readonly int Index = _index;

		public readonly Vector3i VolumePos = _volumePos;

		public readonly Vector3i WorldPos = _worldPos;

		public readonly Vector3i Size = _size;

		public readonly short GroupId = _groupId;

		public readonly string GroupName = _groupName;

		public readonly int SleeperCount = _sleeperCount;

		public readonly int SpawnCountMin = _spawnCountMin;

		public readonly int SpawnCountMax = _spawnCountMax;

		public readonly bool IsPriority = _isPriority;

		public readonly bool IsQuestExclude = _isQuestExclude;

		public readonly SleeperVolume.ETriggerType TriggerType = _triggerType;

		public readonly string MinScript = _minScript;
	}

	public readonly struct CountPreset(short _min, short _max, string _name)
	{
		public readonly short Min = _min;

		public readonly short Max = _max;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name = _name;

		public override string ToString()
		{
			return name;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView triggersBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSleeperCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGroupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<CountPreset> cbxCountPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SleeperVolume.ETriggerType> cbxTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMinScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnersList spawnersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSleeperVolumeChanged;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_WoPropsSleeperVolume Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public List<byte> TriggeredByIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _volume, out var _, out var _))
			{
				return null;
			}
			return _volume.triggeredByIndices;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		Instance = this;
		labelIndex = GetChildById("labelIndex").ViewComponent as XUiV_Label;
		labelPosition = GetChildById("labelPosition").ViewComponent as XUiV_Label;
		labelSize = GetChildById("labelSize").ViewComponent as XUiV_Label;
		labelSleeperCount = GetChildById("labelSleeperCount").ViewComponent as XUiV_Label;
		labelGroup = GetChildById("labelGroup").ViewComponent as XUiV_Label;
		txtGroupId = (XUiC_TextInput)GetChildById("groupId");
		txtGroupId.OnChangeHandler += TxtGroupId_OnChangeHandler;
		cbxPriority = (XUiC_ComboBoxBool)GetChildById("cbxPriority");
		cbxPriority.OnValueChanged += CbxPriority_OnValueChanged;
		cbxQuestExclude = (XUiC_ComboBoxBool)GetChildById("cbxQuestExclude");
		cbxQuestExclude.OnValueChanged += CbxQuestExclude_OnValueChanged;
		cbxCountPreset = (XUiC_ComboBoxList<CountPreset>)GetChildById("cbxCountPreset");
		cbxCountPreset.OnValueChanged += CbxCountPreset_OnValueChanged;
		cbxCountPreset.Elements.Add(new CountPreset(-1, -1, "Custom"));
		cbxCountPreset.Elements.Add(new CountPreset(1, 2, "12"));
		cbxCountPreset.Elements.Add(new CountPreset(2, 3, "23"));
		cbxCountPreset.Elements.Add(new CountPreset(3, 4, "34"));
		cbxCountPreset.Elements.Add(new CountPreset(4, 5, "45"));
		cbxCountPreset.Elements.Add(new CountPreset(5, 6, "56"));
		cbxCountPreset.Elements.Add(new CountPreset(6, 7, "67"));
		cbxCountPreset.Elements.Add(new CountPreset(7, 8, "78"));
		cbxCountPreset.Elements.Add(new CountPreset(8, 9, "89"));
		cbxCountPreset.Elements.Add(new CountPreset(9, 10, "910"));
		cbxCountPreset.MinIndex = 1;
		txtSpawnMin = (XUiC_TextInput)GetChildById("spawnMin");
		txtSpawnMin.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _fromCode) =>
		{
			TxtSpawnMinMax_OnChangeHandler(_text, _fromCode, _isMax: false);
		};
		txtSpawnMax = (XUiC_TextInput)GetChildById("spawnMax");
		txtSpawnMax.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _fromCode) =>
		{
			TxtSpawnMinMax_OnChangeHandler(_text, _fromCode, _isMax: true);
		};
		cbxTrigger = (XUiC_ComboBoxEnum<SleeperVolume.ETriggerType>)GetChildById("cbxTrigger");
		cbxTrigger.OnValueChanged += CbxTrigger_OnValueChanged;
		txtMinScript = (XUiC_TextInput)GetChildById("script");
		txtMinScript.OnChangeHandler += TxtMinScript_OnChangeHandler;
		spawnersList = (XUiC_SpawnersList)GetChildById("spawners");
		spawnersList.SelectionChanged += SpawnersList_SelectionChanged;
		spawnersList.SelectableEntries = false;
		triggersBox = GetChildById("triggersBox").ViewComponent;
		triggeredByList = GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (triggeredByList != null)
		{
			triggeredByList.SelectionChanged += TriggeredByList_SelectionChanged;
			triggeredByList.GetCurrentTriggerIndicesList = [PublicizedFrom(EAccessModifier.Private)] () => TriggeredByIndices;
			triggeredByList.GetCurrentPrefabTriggerLayers = [PublicizedFrom(EAccessModifier.Internal)] () => (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _, out var _prefabInstance, out var _)) ? null : _prefabInstance.prefab.TriggerLayers;
		}
		XUiController childById = GetChildById("addTriggeredByButton");
		if (childById != null)
		{
			childById.OnPress += TriggerOnAddTriggersPressed;
		}
		if (GetChildById("btnCloneVolume") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnCloneVolume_OnPress;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
			categorySleeperVolume.BoxCallbacks = this;
			categorySleeperVolume.CategoryCallbacks = this;
			categorySleeperVolume.CheckKeysCallback = SleeperVolumeToolManager.CheckKeys;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCloneVolume_OnPress(XUiController _sender, int _mouseButton)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex))
		{
			PrefabVolumeManager.Instance.CloneVolumeServer(PrefabVolumeAbs.EVolumeType.Sleeper, _prefabInstance.id, _volumeIndex, new Vector3i(0, _volume.size.y + 1, 0));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modifyCurrentVolume(Action<PrefabSleeperVolume> _modifyAction)
	{
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex))
		{
			return false;
		}
		PrefabSleeperVolume prefabSleeperVolume = _volume.CloneGeneric();
		_modifyAction(prefabSleeperVolume);
		PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeIndex, prefabSleeperVolume);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGroupId_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0)
		{
			modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
			{
				_volumeCopy.groupId = StringParsers.ParseSInt16(_text);
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPriority_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			_volumeCopy.isPriority = _newValue;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxQuestExclude_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			_volumeCopy.isQuestExclude = _newValue;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findCountPresetIndex(int _min, int _max)
	{
		for (int i = 0; i < cbxCountPreset.Elements.Count; i++)
		{
			if (cbxCountPreset.Elements[i].Min == _min && cbxCountPreset.Elements[i].Max == _max)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCountPresetLabel()
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _volume, out var _, out var _))
		{
			int num = findCountPresetIndex(_volume.spawnCountMin, _volume.spawnCountMax);
			if (num < 0)
			{
				cbxCountPreset.MinIndex = 0;
				cbxCountPreset.SelectedIndex = 0;
			}
			else
			{
				cbxCountPreset.MinIndex = 1;
				cbxCountPreset.SelectedIndex = num;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxCountPreset_OnValueChanged(XUiController _sender, CountPreset _oldValue, CountPreset _newValue)
	{
		cbxCountPreset.MinIndex = 1;
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			_volumeCopy.spawnCountMin = _newValue.Min;
			_volumeCopy.spawnCountMax = _newValue.Max;
		});
		updateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMinMax_OnChangeHandler(string _text, bool _changeFromCode, bool _isMax)
	{
		if (_changeFromCode || _text.Length <= 0)
		{
			return;
		}
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			short num = StringParsers.ParseSInt16(_text);
			if (_isMax)
			{
				_volumeCopy.spawnCountMax = num;
			}
			else
			{
				_volumeCopy.spawnCountMin = num;
			}
		});
		updateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxTrigger_OnValueChanged(XUiController _sender, SleeperVolume.ETriggerType _oldValue, SleeperVolume.ETriggerType _newValue)
	{
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			_volumeCopy.SetTrigger(_newValue);
		});
		triggersBox.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtMinScript_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode)
		{
			modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
			{
				_volumeCopy.minScript = MinScript.ConvertFromUIText(_text);
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnersList_SelectionChanged(XUiC_List<XUiC_SpawnersList.SpawnerEntry> _list, XUiC_SpawnersList.SpawnerEntry _previousEntry, XUiC_SpawnersList.SpawnerEntry _newEntry)
	{
		string spawnerName = null;
		if (_newEntry != null)
		{
			spawnerName = _newEntry.name;
		}
		modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
		{
			_volumeCopy.groupName = spawnerName;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry != null)
		{
			modifyCurrentVolume([PublicizedFrom(EAccessModifier.Internal)] (PrefabSleeperVolume _volumeCopy) =>
			{
				_volumeCopy.ToggleTriggeredByFlag(_newEntry.TriggerLayer);
			});
			_newEntry.UiDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _, out var _prefabInstance, out var _))
		{
			_prefabInstance.prefab.AddNewTriggerLayer();
			triggeredByList.RebuildList();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _, out var _prefabInstance, out var _);
		if (_prefabInstance?.prefab != null && _prefabInstance.prefab.TriggerLayers.Count == 0)
		{
			_prefabInstance.prefab.AddInitialTriggerLayers();
		}
		updateCountPresetLabel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (getSelectedVolumeStats(out var _stats))
		{
			XUiC_TextInput xUiC_TextInput = txtGroupId;
			short groupId = _stats.GroupId;
			xUiC_TextInput.Text = groupId.ToString();
			cbxPriority.Value = _stats.IsPriority;
			cbxQuestExclude.Value = _stats.IsQuestExclude;
			XUiV_Label xUiV_Label = labelIndex;
			int index = _stats.Index;
			xUiV_Label.Text = index.ToString();
			labelPosition.Text = _stats.VolumePos.ToString();
			labelSize.Text = _stats.Size.ToString();
			XUiV_Label xUiV_Label2 = labelSleeperCount;
			index = _stats.SleeperCount;
			xUiV_Label2.Text = index.ToString();
			labelGroup.Text = _stats.GroupName;
			XUiC_TextInput xUiC_TextInput2 = txtSpawnMin;
			index = _stats.SpawnCountMin;
			xUiC_TextInput2.Text = index.ToString();
			XUiC_TextInput xUiC_TextInput3 = txtSpawnMax;
			index = _stats.SpawnCountMax;
			xUiC_TextInput3.Text = index.ToString();
			cbxTrigger.Value = _stats.TriggerType;
			txtMinScript.Text = MinScript.ConvertToUIText(_stats.MinScript);
		}
		else
		{
			SetDefaultBoxContents();
		}
		triggersBox.IsVisible = true;
		[PublicizedFrom(EAccessModifier.Private)]
		void SetDefaultBoxContents()
		{
			txtGroupId.Text = string.Empty;
			cbxPriority.Value = false;
			cbxQuestExclude.Value = false;
			labelIndex.Text = string.Empty;
			labelPosition.Text = string.Empty;
			labelSize.Text = string.Empty;
			labelSleeperCount.Text = string.Empty;
			labelGroup.Text = string.Empty;
			txtSpawnMin.Text = string.Empty;
			txtSpawnMax.Text = string.Empty;
			cbxTrigger.Value = SleeperVolume.ETriggerType.Active;
			txtMinScript.Text = string.Empty;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
			categorySleeperVolume.BoxCallbacks = null;
			categorySleeperVolume.CategoryCallbacks = null;
			categorySleeperVolume.CheckKeysCallback = null;
		}
		Instance = null;
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		if (_bActivated)
		{
			bSleeperVolumeChanged = true;
		}
		SleeperVolumeToolManager.SelectionChanged(_bActivated ? _box : null);
		return true;
	}

	public void SleeperVolumeChanged(int _prefabInstanceId, int _volumeId)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _, out var _prefabInstance, out var _volumeIndex) && _prefabInstance.id == _prefabInstanceId && _volumeIndex == _volumeId)
		{
			bSleeperVolumeChanged = true;
		}
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		PrefabVolumeManager.Instance.SelectionBoxMoved(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _moveVector);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		PrefabVolumeManager.Instance.SelectionBoxSized(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		if (!PrefabVolumeManager.GetPrefabIdAndVolumeId(_box.name, out var _, out var _))
		{
			return false;
		}
		return PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _checkCanDeleteOnly);
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
		_windowManager.SwitchVisible(windowGroup);
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
		SleeperVolumeToolManager.RegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryVisibilityChanged(SelectionCategory _category, bool _visible)
	{
		SleeperVolumeToolManager.SetVisible(_visible);
	}

	public void OnSelectionCategoryBoxAdded(SelectionBox _box)
	{
		SleeperVolumeToolManager.RegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryBoxRemoved(SelectionBox _box)
	{
		SleeperVolumeToolManager.UnRegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryCleared(SelectionCategory _category)
	{
		SleeperVolumeToolManager.ClearSleeperVolumes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleSleeperVolumeChanged()
	{
		if (bSleeperVolumeChanged && PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _, out var _prefabInstance, out var _volumeIndex))
		{
			bSleeperVolumeChanged = false;
			_prefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, _prefabInstance.boundingBoxPosition, _volumeIndex);
			updateCountPresetLabel();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getSelectedVolumeStats(out VolumeStats _stats)
	{
		_stats = default(VolumeStats);
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex))
		{
			return false;
		}
		handleSleeperVolumeChanged();
		_stats = new VolumeStats(_volumeIndex, _volume.startPos, _prefabInstance.boundingBoxPosition + _volume.startPos, _volume.size, _volume.groupId, GameStageGroup.MakeDisplayName(_volume.groupName), _prefabInstance.prefab.Transient_NumSleeperSpawns, _volume.spawnCountMin, _volume.spawnCountMax, _volume.isPriority, _volume.isQuestExclude, (SleeperVolume.ETriggerType)(_volume.flags & 7), _volume.minScript);
		return true;
	}

	public static bool GetSelectedVolumeStats(out VolumeStats _stats)
	{
		return Instance.getSelectedVolumeStats(out _stats);
	}
}
