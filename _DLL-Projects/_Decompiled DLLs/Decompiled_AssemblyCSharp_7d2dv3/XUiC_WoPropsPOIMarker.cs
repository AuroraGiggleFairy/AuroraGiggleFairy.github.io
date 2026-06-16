using System;
using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsPOIMarker : XUiController, ISelectionBoxCallback, ISelectionCategoryCallback
{
	public static string ID = "";

	public static XUiC_WoPropsPOIMarker Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput startX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput startY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput startZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput sizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput sizeY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput sizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Marker.MarkerSize> markerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Marker.MarkerTypes> markerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtPoiMarkerPartName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabMarkerList markerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat partSpawnChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreSelectionChange;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		Instance = this;
		markerList = GetChildById("markers") as XUiC_PrefabMarkerList;
		if (markerList != null)
		{
			markerList.SelectionChanged += MarkerList_SelectionChanged;
		}
		startX = GetChildById("txtStartX") as XUiC_TextInput;
		if (startX != null)
		{
			startX.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifyStartPos(_text, _changeFromCode, 0);
			};
		}
		startY = GetChildById("txtStartY") as XUiC_TextInput;
		if (startY != null)
		{
			startY.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifyStartPos(_text, _changeFromCode, 1);
			};
		}
		startZ = GetChildById("txtStartZ") as XUiC_TextInput;
		if (startZ != null)
		{
			startZ.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifyStartPos(_text, _changeFromCode, 2);
			};
		}
		sizeX = GetChildById("txtSizeX") as XUiC_TextInput;
		if (sizeX != null)
		{
			sizeX.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifySize(_text, _changeFromCode, 0);
			};
		}
		sizeY = GetChildById("txtSizeY") as XUiC_TextInput;
		if (sizeY != null)
		{
			sizeY.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifySize(_text, _changeFromCode, 1);
			};
		}
		sizeZ = GetChildById("txtSizeZ") as XUiC_TextInput;
		if (sizeZ != null)
		{
			sizeZ.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _changeFromCode) =>
			{
				modifySize(_text, _changeFromCode, 2);
			};
		}
		rotations = GetChildById("txtMarkerRotations") as XUiC_ComboBoxInt;
		if (rotations != null)
		{
			rotations.OnValueChanged += Rotations_OnValueChanged;
		}
		partSpawnChance = GetChildById("cbxPartSpawnChance") as XUiC_ComboBoxFloat;
		if (partSpawnChance != null)
		{
			partSpawnChance.OnValueChanged += PartSpawnChance_OnValueChanged;
		}
		markerSize = GetChildById("cbxPOIMarkerSize") as XUiC_ComboBoxEnum<Marker.MarkerSize>;
		if (markerSize != null)
		{
			markerSize.OnValueChanged += MarkerSize_OnValueChanged;
		}
		markerType = GetChildById("cbxPOIMarkerType") as XUiC_ComboBoxEnum<Marker.MarkerTypes>;
		if (markerType != null)
		{
			markerType.OnValueChanged += MarkerType_OnValueChanged;
		}
		groupName = GetChildById("txtGroup") as XUiC_TextInput;
		if (groupName != null)
		{
			groupName.OnChangeHandler += GroupName_OnChangeHandler;
		}
		tags = GetChildById("txtTags") as XUiC_TextInput;
		if (tags != null)
		{
			tags.OnChangeHandler += Tags_OnChangeHandler;
		}
		if (GetChildById("btnCreateMarker") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnCreateMarker_OnPress;
		}
		if (GetChildById("btnDeleteMarker") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnDeleteMarker_OnPress;
		}
		txtPoiMarkerPartName = GetChildById("txtPoiMarkerPartName") as XUiC_DropDown;
		if (txtPoiMarkerPartName != null)
		{
			foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, _ignoreDuplicateNames: true))
			{
				if (availablePaths.RelativePath.EqualsCaseInsensitive("parts"))
				{
					txtPoiMarkerPartName.AllEntries.Add(availablePaths.Name);
				}
			}
			txtPoiMarkerPartName.UpdateFilteredList();
			txtPoiMarkerPartName.OnChangeHandler += txtPoiMarkerPartName_OnValueChanged;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categoryPOIMarker = SelectionBoxManager.Instance.CategoryPOIMarker;
			categoryPOIMarker.BoxCallbacks = this;
			categoryPOIMarker.CategoryCallbacks = this;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCreateMarker_OnPress(XUiController _sender, int _mouseButton)
	{
		spawnNewMarker();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteMarker_OnPress(XUiController _sender, int _mouseButton)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _box, out var _, out var _, out var _))
		{
			PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Marker, _box, _checkCanDeleteOnly: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerList_SelectionChanged(XUiC_List<XUiC_PrefabMarkerList.PrefabMarkerEntry> _list, XUiC_PrefabMarkerList.PrefabMarkerEntry _previousEntry, XUiC_PrefabMarkerList.PrefabMarkerEntry _newEntry)
	{
		if (ignoreSelectionChange || !windowGroup.isShowing)
		{
			return;
		}
		SelectionBox selectionBox = _newEntry?.Box;
		if (selectionBox == null)
		{
			if (SelectionBoxManager.Instance.Selection?.Category == SelectionBoxManager.Instance.CategoryPOIMarker)
			{
				SelectionBoxManager.Instance.Deactivate();
			}
		}
		else if (!selectionBox.IsActive)
		{
			SelectionBoxManager.Instance.SetActive(selectionBox, _bActive: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modifyCurrentMarker(Action<Marker> _modifyAction)
	{
		if (!PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex))
		{
			return false;
		}
		Marker marker = _volume.CloneGeneric();
		_modifyAction(marker);
		PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeIndex, marker);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tags_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.Tags = FastTags<TagGroup.Poi>.Parse(_text);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PartSpawnChance_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.PartChanceToSpawn = (float)Mathf.RoundToInt((float)_newValue * 100f) / 100f;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rotations_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.Rotations = (byte)_newValue;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void txtPoiMarkerPartName_OnValueChanged(XUiController _sender, string _text, bool _validInput, bool _changeFromCode)
	{
		if (!_changeFromCode && _validInput)
		{
			modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
			{
				_volumeCopy.PartToSpawn = _text;
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.GroupName = _text;
		}))
		{
			POIMarkerToolManager.UpdateAllColors();
			markerList.RefreshBindingsSelfAndChildren();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerType_OnValueChanged(XUiController _sender, Marker.MarkerTypes _oldValue, Marker.MarkerTypes _newValue)
	{
		modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.MarkerType = _newValue;
			if (_volumeCopy.MarkerType == Marker.MarkerTypes.POISpawn)
			{
				Vector3i size = _volumeCopy.size;
				size.y = 0;
				_volumeCopy.size = size;
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerSize_OnValueChanged(XUiController _sender, Marker.MarkerSize _oldValue, Marker.MarkerSize _newValue)
	{
		if (_newValue != Marker.MarkerSize.Custom)
		{
			modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
			{
				_volumeCopy.size = Marker.MarkerSizes[(int)_newValue];
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StartSize_ModifyVector(string _text, bool _changeFromCode, int _dimension, Vector3i _currentValue, out Vector3i _newValue)
	{
		_newValue = _currentValue;
		if (_text.Length == 0 || _changeFromCode)
		{
			return false;
		}
		if (!StringParsers.TryParseSInt32(_text, out var _result))
		{
			return false;
		}
		switch (_dimension)
		{
		case 0:
			_newValue.x = _result;
			break;
		case 1:
			_newValue.y = _result;
			break;
		case 2:
			_newValue.z = _result;
			break;
		default:
			throw new ArgumentOutOfRangeException("_dimension", _dimension, "_dimension must be between 0 and 2");
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void modifyStartPos(string _text, bool _changeFromCode, int _dimension)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex) && StartSize_ModifyVector(_text, _changeFromCode, _dimension, _volume.startPos, out var _newValue))
		{
			Marker marker = _volume.CloneGeneric();
			marker.startPos = _newValue;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeIndex, marker);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void modifySize(string _text, bool _changeFromCode, int _dimension)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _, out var _volume, out var _prefabInstance, out var _volumeIndex) && StartSize_ModifyVector(_text, _changeFromCode, _dimension, _volume.size, out var _newValue))
		{
			if (_volume.MarkerType == Marker.MarkerTypes.POISpawn)
			{
				_newValue.y = 0;
				return;
			}
			Marker marker = _volume.CloneGeneric();
			marker.size = _newValue;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeIndex, marker);
		}
	}

	public override void Update(float _dt)
	{
		RefreshBindings();
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "iscustomsize":
			_value = (markerSize != null && markerSize.Value == Marker.MarkerSize.Custom).ToString();
			return true;
		case "markertype":
			_value = ((markerType != null) ? markerType.Value.ToStringCached() : "None");
			return true;
		case "markersize":
			_value = ((markerSize != null) ? markerSize.Value.ToStringCached() : "One");
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void OnOpen()
	{
		ignoreSelectionChange = true;
		base.OnOpen();
		ignoreSelectionChange = false;
		updateValues();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateValues()
	{
		PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _, out var _volume, out var _, out var _);
		startX.Text = _volume?.startPos.x.ToString() ?? "";
		startY.Text = _volume?.startPos.y.ToString() ?? "";
		startZ.Text = _volume?.startPos.z.ToString() ?? "";
		if (markerSize != null)
		{
			if (_volume != null && Marker.MarkerSizes.Contains(_volume.size))
			{
				markerSize.Value = (Marker.MarkerSize)Marker.MarkerSizes.IndexOf(_volume.size);
			}
			else
			{
				markerSize.Value = Marker.MarkerSize.Custom;
			}
		}
		if (sizeX != null)
		{
			sizeX.Text = _volume?.size.x.ToString() ?? "";
		}
		if (sizeY != null)
		{
			sizeY.Text = _volume?.size.y.ToString() ?? "";
		}
		if (sizeZ != null)
		{
			sizeZ.Text = _volume?.size.z.ToString() ?? "";
		}
		if (markerType != null)
		{
			markerType.Value = _volume?.MarkerType ?? Marker.MarkerTypes.None;
		}
		if (groupName != null)
		{
			groupName.Text = _volume?.GroupName ?? "";
		}
		if (tags != null)
		{
			tags.Text = _volume?.Tags.ToString() ?? "";
		}
		if (txtPoiMarkerPartName != null)
		{
			txtPoiMarkerPartName.Text = _volume?.PartToSpawn ?? "";
		}
		if (rotations != null)
		{
			rotations.Value = _volume?.Rotations ?? 0;
		}
		if (partSpawnChance != null)
		{
			partSpawnChance.Value = (float)Mathf.RoundToInt((_volume?.PartChanceToSpawn ?? 0f) * 100f) / 100f;
		}
		if (markerList != null && _volume != null)
		{
			markerList.SelectByMarker(_volume);
		}
	}

	public void CheckSpecialKeys(Event _ev, PlayerActionsLocal _playerActions)
	{
		if ((_ev.modifiers & EventModifiers.Control) != EventModifiers.None && (_ev.modifiers & EventModifiers.Shift) != EventModifiers.None && _ev.keyCode == KeyCode.Return)
		{
			spawnNewMarker();
			_ev.Use();
		}
		if ((_ev.modifiers & EventModifiers.Control) == 0 || (_ev.modifiers & EventModifiers.Shift) == 0 || _ev.keyCode != KeyCode.A)
		{
			return;
		}
		_ev.Use();
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return;
		}
		foreach (var (_, selectionBox2) in SelectionBoxManager.Instance.CategoryPOIMarker.boxes)
		{
			if (selectionBox2.UserData is Marker { MarkerType: Marker.MarkerTypes.POISpawn })
			{
				POIMarkerToolManager.DisplayPrefabPreviewForMarker(selectionBox2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spawnNewMarker()
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(1000f);
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i size = new Vector3i(1, 1, 1);
			Vector3i startPos = World.worldToBlockPos(raycastHitPoint) - new Vector3i(size.x / 2, 0, size.z / 2);
			PrefabVolumeManager.Instance.AddVolumeServer(PrefabVolumeAbs.EVolumeType.Marker, startPos, size);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categoryPOIMarker = SelectionBoxManager.Instance.CategoryPOIMarker;
			categoryPOIMarker.BoxCallbacks = null;
			categoryPOIMarker.CategoryCallbacks = null;
		}
		POIMarkerToolManager.CleanUp();
		Instance = null;
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		updateValues();
		POIMarkerToolManager.SelectionChanged(_bActivated ? _box : null);
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		PrefabVolumeManager.Instance.SelectionBoxMoved(PrefabVolumeAbs.EVolumeType.Marker, _box, _moveVector);
		POIMarkerToolManager.UpdateAllColors();
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (PrefabVolumeManager.TryGetSelectedVolume<Marker>(SelectionBoxManager.Instance.CategoryPOIMarker, out var _, out var _volume, out var _, out var _) && _volume.MarkerType != Marker.MarkerTypes.PartSpawn)
		{
			if (_volume.MarkerType == Marker.MarkerTypes.POISpawn)
			{
				_dTop = 0;
				_dBottom = 0;
			}
			PrefabVolumeManager.Instance.SelectionBoxSized(PrefabVolumeAbs.EVolumeType.Marker, _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
			POIMarkerToolManager.UpdateAllColors();
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
		POIMarkerToolManager.UpdateAllColors();
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		return PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Marker, _box, _checkCanDeleteOnly);
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
		modifyCurrentMarker([PublicizedFrom(EAccessModifier.Internal)] (Marker _volumeCopy) =>
		{
			_volumeCopy.Rotations = (byte)((_volumeCopy.Rotations + 1) % 4);
		});
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
		POIMarkerToolManager.RegisterPoiMarker(_box);
		POIMarkerToolManager.UpdateAllColors();
		ignoreSelectionChange = true;
		markerList.RebuildList();
		ignoreSelectionChange = false;
		updatePartSpawnerSize(_box);
		updateValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePartSpawnerSize(SelectionBox _selBox)
	{
		if (!(_selBox.UserData is Marker { MarkerType: Marker.MarkerTypes.PartSpawn } marker) || string.IsNullOrEmpty(marker.PartToSpawn))
		{
			return;
		}
		if (!POIMarkerToolManager.AllPrefabData.TryGetValue(marker.PartToSpawn, out var value))
		{
			Log.Error("Part '" + marker.PartToSpawn + "' not found!");
			return;
		}
		Vector3i vector3i = (((value.rotationToFaceNorth + marker.Rotations) % 2 != 1) ? value.size : new Vector3i(value.size.z, value.size.y, value.size.x));
		if (!(marker.size == vector3i) && PrefabVolumeManager.GetPrefabIdAndVolumeId(_selBox.name, out var _volumeId, out var _prefabInstance))
		{
			Marker marker2 = marker.CloneGeneric();
			marker2.size = vector3i;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeId, marker2);
		}
	}

	public void OnSelectionCategoryVisibilityChanged(SelectionCategory _category, bool _visible)
	{
		POIMarkerToolManager.UpdateAllColors();
	}

	public void OnSelectionCategoryBoxAdded(SelectionBox _box)
	{
		POIMarkerToolManager.RegisterPoiMarker(_box);
		ignoreSelectionChange = true;
		markerList.RebuildList();
		ignoreSelectionChange = false;
	}

	public void OnSelectionCategoryBoxRemoved(SelectionBox _box)
	{
		POIMarkerToolManager.UnRegisterPoiMarker(_box);
		ignoreSelectionChange = true;
		markerList.RebuildList();
		ignoreSelectionChange = false;
	}

	public void OnSelectionCategoryCleared(SelectionCategory _category)
	{
	}
}
