using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsPOIMarker : XUiController, ISelectionBoxCallback
{
	public static string ID = "";

	public static XUiC_WoPropsPOIMarker Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput GroupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput Tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController grdCustSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblCustSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPOIMarker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblMarkerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartSpawnChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Prefab.Marker.MarkerSize> MarkerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Prefab.Marker.MarkerTypes> MarkerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> MarkerPartName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabMarkerList markerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt Rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat PartSpawnChance;

	public const float cPrefabYPosition = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsCustomSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsPartSpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastShowRotations;

	public Prefab.Marker CurrentMarker;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		Instance = this;
		markerList = GetChildById("markers") as XUiC_PrefabMarkerList;
		if (markerList != null)
		{
			markerList.SelectionChanged += MarkerList_SelectionChanged;
		}
		StartX = GetChildById("txtStartX") as XUiC_TextInput;
		if (StartX != null)
		{
			StartX.OnChangeHandler += StartX_OnChangeHandler;
		}
		StartY = GetChildById("txtStartY") as XUiC_TextInput;
		if (StartY != null)
		{
			StartY.OnChangeHandler += StartY_OnChangeHandler;
		}
		StartZ = GetChildById("txtStartZ") as XUiC_TextInput;
		if (StartZ != null)
		{
			StartZ.OnChangeHandler += StartZ_OnChangeHandler;
		}
		SizeX = GetChildById("txtSizeX") as XUiC_TextInput;
		if (SizeX != null)
		{
			SizeX.OnChangeHandler += SizeX_OnChangeHandler;
		}
		SizeY = GetChildById("txtSizeY") as XUiC_TextInput;
		if (SizeY != null)
		{
			SizeY.OnChangeHandler += SizeY_OnChangeHandler;
		}
		SizeZ = GetChildById("txtSizeZ") as XUiC_TextInput;
		if (SizeZ != null)
		{
			SizeZ.OnChangeHandler += SizeZ_OnChangeHandler;
		}
		Rotations = GetChildById("txtMarkerRotations") as XUiC_ComboBoxInt;
		if (Rotations != null)
		{
			Rotations.OnValueChanged += Rotations_OnValueChanged;
		}
		PartSpawnChance = GetChildById("cbxPartSpawnChance") as XUiC_ComboBoxFloat;
		if (PartSpawnChance != null)
		{
			PartSpawnChance.OnValueChanged += PartSpawnChance_OnValueChanged;
		}
		MarkerSize = GetChildById("cbxPOIMarkerSize") as XUiC_ComboBoxEnum<Prefab.Marker.MarkerSize>;
		if (MarkerSize != null)
		{
			MarkerSize.OnValueChanged += MarkerSize_OnValueChanged;
		}
		MarkerType = GetChildById("cbxPOIMarkerType") as XUiC_ComboBoxEnum<Prefab.Marker.MarkerTypes>;
		if (MarkerType != null)
		{
			MarkerType.OnValueChanged += MarkerType_OnValueChanged;
		}
		GroupName = GetChildById("txtGroup") as XUiC_TextInput;
		if (GroupName != null)
		{
			GroupName.OnChangeHandler += GroupName_OnChangeHandler;
		}
		Tags = GetChildById("txtTags") as XUiC_TextInput;
		if (Tags != null)
		{
			Tags.OnChangeHandler += Tags_OnChangeHandler;
		}
		btnPOIMarker = GetChildById("btnPOIMarker");
		if (btnPOIMarker != null)
		{
			btnPOIMarker.GetChildById("clickable").OnPress += BtnPOIMarker_Controller_OnPress;
		}
		MarkerPartName = GetChildById("cbxPOIMarkerPartName") as XUiC_ComboBoxList<string>;
		if (MarkerPartName != null)
		{
			foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, null, null, _ignoreDuplicateNames: true))
			{
				if (availablePaths.RelativePath.EqualsCaseInsensitive("parts"))
				{
					MarkerPartName.Elements.Add(availablePaths.Name);
				}
			}
			MarkerPartName.OnValueChanged += MarkerPartName_OnValueChanged;
		}
		lblPartSpawn = GetChildById("lblPartName");
		lblCustSize = GetChildById("lblCustSize");
		grdCustSize = GetChildById("grdCustSize");
		lblPartRotations = GetChildById("lblMarkerRotations");
		lblMarkerSize = GetChildById("lblMarkerSize");
		lblPartSpawnChance = GetChildById("lblPartSpawnChance");
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("POIMarker").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tags_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.Tags = FastTags<TagGroup.Poi>.Parse(_text);
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PartSpawnChance_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.PartChanceToSpawn = (float)Mathf.RoundToInt((float)_newValue * 100f) / 100f;
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rotations_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.Rotations = (byte)_newValue;
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerPartName_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.PartToSpawn = _newValue;
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "iscustomsize")
		{
			value = (MarkerSize != null && MarkerSize.Value == Prefab.Marker.MarkerSize.Custom).ToString();
			return true;
		}
		return false;
	}

	public override void OnClose()
	{
		saveDataToPrefab();
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPOIMarker_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(1000f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), vector3i);
			if (prefabInstance != null)
			{
				Vector3i size = new Vector3i(1, 1, 1);
				prefabInstance.prefab.AddNewPOIMarker(prefabInstance.name, prefabInstance.boundingBoxPosition, vector3i - prefabInstance.boundingBoxPosition - new Vector3i(size.x / 2, 0, size.z / 2), size, "new", FastTags<TagGroup.Poi>.none, Prefab.Marker.MarkerTypes.None);
			}
			updatePrefabDataAndVis();
			POIMarkerToolManager.UpdateAllColors();
			markerList.RebuildList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.GroupName = _text;
			updatePrefabDataAndVis();
			POIMarkerToolManager.UpdateAllColors();
			if (!_changeFromCode)
			{
				markerList.RebuildList();
			}
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerType_OnValueChanged(XUiController _sender, Prefab.Marker.MarkerTypes _oldValue, Prefab.Marker.MarkerTypes _newValue)
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.MarkerType = _newValue;
			if (CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
			{
				SizeY.Text = "0";
			}
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerSize_OnValueChanged(XUiController _sender, Prefab.Marker.MarkerSize _oldValue, Prefab.Marker.MarkerSize _newValue)
	{
		if (CurrentMarker != null && _newValue != Prefab.Marker.MarkerSize.Custom)
		{
			CurrentMarker.Size = Prefab.Marker.MarkerSizes[(int)_newValue];
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeZ_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!(_text.Length == 0 || _changeFromCode) && CurrentMarker != null && StringParsers.TryParseSInt32(_text, out var _result))
		{
			CurrentMarker.Size = new Vector3i(CurrentMarker.Size.x, CurrentMarker.Size.y, _result);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeY_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || CurrentMarker == null)
		{
			return;
		}
		int _result = 0;
		if (StringParsers.TryParseSInt32(_text, out _result))
		{
			if (CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
			{
				_result = 0;
				SizeY.Text = _result.ToString();
			}
			CurrentMarker.Size = new Vector3i(CurrentMarker.Size.x, _result, CurrentMarker.Size.z);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeX_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!(_text.Length == 0 || _changeFromCode) && CurrentMarker != null && StringParsers.TryParseSInt32(_text, out var _result))
		{
			CurrentMarker.Size = new Vector3i(_result, CurrentMarker.Size.y, CurrentMarker.Size.z);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartZ_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!(_text.Length == 0 || _changeFromCode) && CurrentMarker != null && StringParsers.TryParseSInt32(_text, out var _result))
		{
			CurrentMarker.Start = new Vector3i(CurrentMarker.Start.x, CurrentMarker.Start.y, _result);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartY_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!(_text.Length == 0 || _changeFromCode) && CurrentMarker != null && StringParsers.TryParseSInt32(_text, out var _result))
		{
			CurrentMarker.Start = new Vector3i(CurrentMarker.Start.x, _result, CurrentMarker.Start.z);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartX_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!(_text.Length == 0 || _changeFromCode) && CurrentMarker != null && StringParsers.TryParseSInt32(_text, out var _result))
		{
			CurrentMarker.Start = new Vector3i(_result, CurrentMarker.Start.y, CurrentMarker.Start.z);
			updatePrefabDataAndVis();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public void updatePrefabDataAndVis()
	{
		if (CurrentMarker == null)
		{
			return;
		}
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory("POIMarker");
		SelectionBox selectionBox = category?.GetBox(CurrentMarker.Name);
		Prefab prefab = null;
		if (selectionBox != null)
		{
			POIMarkerToolManager.UnRegisterPOIMarker(selectionBox);
			category.RemoveBox(CurrentMarker.Name);
			if (CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn && CurrentMarker.PartToSpawn != null && CurrentMarker.PartToSpawn.Length > 0)
			{
				prefab = new Prefab();
				prefab.Load(CurrentMarker.PartToSpawn, _applyMapping: false, _fixChildblocks: false, _allowMissingBlocks: true);
				if ((prefab.rotationToFaceNorth + CurrentMarker.Rotations) % 2 == 1)
				{
					CurrentMarker.Size = new Vector3i(prefab.size.z, prefab.size.y, prefab.size.x);
				}
				else
				{
					CurrentMarker.Size = prefab.size;
				}
			}
			selectionBox = category.AddBox(CurrentMarker.Name, CurrentMarker.Start - getBaseVisualOffset(), CurrentMarker.Size);
			selectionBox.UserData = CurrentMarker;
			selectionBox.bAlwaysDrawDirection = true;
			selectionBox.bDrawDirection = true;
			float facing = 0f;
			switch (CurrentMarker.Rotations)
			{
			case 1:
				facing = ((CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 90 : 270);
				break;
			case 2:
				facing = 180f;
				break;
			case 3:
				facing = ((CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 270 : 90);
				break;
			}
			SelectionBoxManager.Instance.SetFacingDirection("POIMarker", CurrentMarker.Name, facing);
			SelectionBoxManager.Instance.SetActive("POIMarker", CurrentMarker.Name, _bActive: true);
			POIMarkerToolManager.RegisterPOIMarker(selectionBox);
		}
		saveDataToPrefab();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveDataToPrefab()
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null || PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers == null)
		{
			return;
		}
		for (int i = 0; i < PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.Count; i++)
		{
			if (PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i].Name == CurrentMarker.Name)
			{
				PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i] = CurrentMarker;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUIElements()
	{
		if (MarkerType != null)
		{
			switch (MarkerType.Value)
			{
			case Prefab.Marker.MarkerTypes.PartSpawn:
			{
				XUiView xUiView16 = lblMarkerSize.ViewComponent;
				bool isVisible = (MarkerSize.ViewComponent.IsVisible = false);
				xUiView16.IsVisible = isVisible;
				XUiView xUiView17 = lblCustSize.ViewComponent;
				isVisible = (grdCustSize.ViewComponent.IsVisible = false);
				xUiView17.IsVisible = isVisible;
				XUiView xUiView18 = lblPartRotations.ViewComponent;
				isVisible = (Rotations.ViewComponent.IsVisible = true);
				xUiView18.IsVisible = isVisible;
				XUiView xUiView19 = lblPartSpawn.ViewComponent;
				isVisible = (MarkerPartName.ViewComponent.IsVisible = true);
				xUiView19.IsVisible = isVisible;
				XUiView xUiView20 = lblPartRotations.ViewComponent;
				isVisible = (Rotations.ViewComponent.IsVisible = true);
				xUiView20.IsVisible = isVisible;
				XUiView xUiView21 = lblPartSpawnChance.ViewComponent;
				isVisible = (PartSpawnChance.ViewComponent.IsVisible = true);
				xUiView21.IsVisible = isVisible;
				break;
			}
			case Prefab.Marker.MarkerTypes.POISpawn:
			{
				XUiView xUiView11 = lblMarkerSize.ViewComponent;
				bool isVisible = (MarkerSize.ViewComponent.IsVisible = true);
				xUiView11.IsVisible = isVisible;
				XUiView xUiView12 = lblPartSpawn.ViewComponent;
				isVisible = (MarkerPartName.ViewComponent.IsVisible = false);
				xUiView12.IsVisible = isVisible;
				XUiView xUiView13 = lblPartRotations.ViewComponent;
				isVisible = (Rotations.ViewComponent.IsVisible = true);
				xUiView13.IsVisible = isVisible;
				XUiView xUiView14 = lblPartSpawnChance.ViewComponent;
				isVisible = (PartSpawnChance.ViewComponent.IsVisible = false);
				xUiView14.IsVisible = isVisible;
				XUiView xUiView15 = lblCustSize.ViewComponent;
				isVisible = (grdCustSize.ViewComponent.IsVisible = MarkerSize.Value == Prefab.Marker.MarkerSize.Custom);
				xUiView15.IsVisible = isVisible;
				break;
			}
			case Prefab.Marker.MarkerTypes.RoadExit:
			{
				XUiView xUiView6 = lblMarkerSize.ViewComponent;
				bool isVisible = (MarkerSize.ViewComponent.IsVisible = true);
				xUiView6.IsVisible = isVisible;
				XUiView xUiView7 = lblPartSpawn.ViewComponent;
				isVisible = (MarkerPartName.ViewComponent.IsVisible = false);
				xUiView7.IsVisible = isVisible;
				XUiView xUiView8 = lblPartRotations.ViewComponent;
				isVisible = (Rotations.ViewComponent.IsVisible = true);
				xUiView8.IsVisible = isVisible;
				XUiView xUiView9 = lblPartSpawnChance.ViewComponent;
				isVisible = (PartSpawnChance.ViewComponent.IsVisible = false);
				xUiView9.IsVisible = isVisible;
				XUiView xUiView10 = lblCustSize.ViewComponent;
				isVisible = (grdCustSize.ViewComponent.IsVisible = MarkerSize.Value == Prefab.Marker.MarkerSize.Custom);
				xUiView10.IsVisible = isVisible;
				break;
			}
			case Prefab.Marker.MarkerTypes.None:
			{
				XUiView xUiView = lblMarkerSize.ViewComponent;
				bool isVisible = (MarkerSize.ViewComponent.IsVisible = false);
				xUiView.IsVisible = isVisible;
				XUiView xUiView2 = lblPartSpawn.ViewComponent;
				isVisible = (MarkerPartName.ViewComponent.IsVisible = false);
				xUiView2.IsVisible = isVisible;
				XUiView xUiView3 = lblPartRotations.ViewComponent;
				isVisible = (Rotations.ViewComponent.IsVisible = false);
				xUiView3.IsVisible = isVisible;
				XUiView xUiView4 = lblPartSpawnChance.ViewComponent;
				isVisible = (PartSpawnChance.ViewComponent.IsVisible = false);
				xUiView4.IsVisible = isVisible;
				XUiView xUiView5 = lblCustSize.ViewComponent;
				isVisible = (grdCustSize.ViewComponent.IsVisible = false);
				xUiView5.IsVisible = isVisible;
				break;
			}
			}
		}
	}

	public override void Update(float _dt)
	{
		updateUIElements();
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateValues();
		updateUIElements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateValues()
	{
		if (CurrentMarker == null)
		{
			return;
		}
		StartX.Text = CurrentMarker.Start.x.ToString();
		StartY.Text = CurrentMarker.Start.y.ToString();
		StartZ.Text = CurrentMarker.Start.z.ToString();
		if (MarkerSize != null)
		{
			if (Prefab.Marker.MarkerSizes.Contains(CurrentMarker.Size))
			{
				MarkerSize.Value = (Prefab.Marker.MarkerSize)Prefab.Marker.MarkerSizes.IndexOf(CurrentMarker.Size);
			}
			else
			{
				MarkerSize.Value = Prefab.Marker.MarkerSize.Custom;
			}
		}
		if (SizeX != null)
		{
			SizeX.Text = CurrentMarker.Size.x.ToString();
		}
		if (SizeY != null)
		{
			SizeY.Text = CurrentMarker.Size.y.ToString();
		}
		if (SizeZ != null)
		{
			SizeZ.Text = CurrentMarker.Size.z.ToString();
		}
		if (MarkerType != null)
		{
			MarkerType.Value = CurrentMarker.MarkerType;
		}
		if (GroupName != null)
		{
			GroupName.Text = CurrentMarker.GroupName;
		}
		if (Tags != null)
		{
			Tags.Text = CurrentMarker.Tags.ToString();
		}
		if (MarkerPartName != null)
		{
			MarkerPartName.Value = CurrentMarker.PartToSpawn;
		}
		if (Rotations != null)
		{
			Rotations.Value = CurrentMarker.Rotations;
		}
		if (PartSpawnChance != null)
		{
			PartSpawnChance.Value = (float)Mathf.RoundToInt(CurrentMarker.PartChanceToSpawn * 100f) / 100f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabMarkerList.PrefabMarkerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabMarkerList.PrefabMarkerEntry> _newEntry)
	{
		if (_newEntry != null && _newEntry.GetEntry() != null)
		{
			CurrentMarker = _newEntry.GetEntry().marker;
			SelectionBoxManager.Instance.SetActive("POIMarker", CurrentMarker.Name, _bActive: true);
			_newEntry.Selected = true;
			_newEntry.IsDirty = true;
			updateValues();
			IsDirty = true;
		}
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
		if ((ev.modifiers & EventModifiers.Control) != EventModifiers.None && (ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Return)
		{
			SpawnNewMarker();
			if (POIMarkerToolManager.currentSelectionBox != null && POIMarkerToolManager.currentSelectionBox.UserData is Prefab.Marker)
			{
				CurrentMarker = POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker;
			}
			updatePrefabDataAndVis();
			POIMarkerToolManager.UpdateAllColors();
			markerList.RebuildList();
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Return)
		{
			if (POIMarkerToolManager.currentSelectionBox != null && POIMarkerToolManager.currentSelectionBox.UserData is Prefab.Marker)
			{
				CurrentMarker = POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker;
			}
			base.xui.playerUI.windowManager.Open(ID, _bModal: true);
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Control) != EventModifiers.None && (ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Z)
		{
			if (CurrentMarker != null)
			{
				CurrentMarker.Rotations = (byte)((CurrentMarker.Rotations + 1) % 4);
				updatePrefabDataAndVis();
			}
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Control) == 0 || (ev.modifiers & EventModifiers.Shift) == 0 || ev.keyCode != KeyCode.A)
		{
			return;
		}
		ev.Use();
		if (PrefabEditModeManager.Instance.VoxelPrefab == null || PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers == null)
		{
			return;
		}
		for (int i = 0; i < PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.Count; i++)
		{
			if (PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i].MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
			{
				SelectionBox _selectionBox = null;
				if (SelectionBoxManager.Instance.TryGetSelectionBox("POIMarker", "POIMarker_" + i, out _selectionBox))
				{
					POIMarkerToolManager.DisplayPrefabPreviewForMarker(_selectionBox);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnNewMarker()
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(1000f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), vector3i);
			if (prefabInstance != null)
			{
				Vector3i size = new Vector3i(1, 1, 1);
				prefabInstance.prefab.AddNewPOIMarker(prefabInstance.name, prefabInstance.boundingBoxPosition, vector3i - prefabInstance.boundingBoxPosition - new Vector3i(size.x / 2, 0, size.z / 2), size, "new", FastTags<TagGroup.Poi>.none, Prefab.Marker.MarkerTypes.None, isSelected: true);
			}
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("POIMarker")?.SetCallback(null);
		}
		POIMarkerToolManager.CleanUp();
		Instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tryGetSelectedMarker(out Prefab.Marker _marker)
	{
		if (POIMarkerToolManager.currentSelectionBox == null || POIMarkerToolManager.currentSelectionBox.UserData == null)
		{
			_marker = null;
			return false;
		}
		_marker = (Prefab.Marker)POIMarkerToolManager.currentSelectionBox.UserData;
		return true;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		return true;
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return false;
		}
		if ((bool)POIMarkerToolManager.currentSelectionBox)
		{
			POIMarkerToolManager.UnRegisterPOIMarker(POIMarkerToolManager.currentSelectionBox);
		}
		SelectionBoxManager.Instance.GetCategory(_category)?.RemoveBox(_name);
		return PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker x) => x.Name == _name) > 0;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria != EnumSelectionBoxAvailabilities.CanShowProperties)
		{
			return _criteria == EnumSelectionBoxAvailabilities.CanResize;
		}
		return true;
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		if (SelectionBoxManager.Instance.GetSelected(out var _selectedCategory, out var _) && _selectedCategory.Equals("POIMarker"))
		{
			_windowManager.SwitchVisible(ID);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (tryGetSelectedMarker(out var _marker))
		{
			_marker.Start += new Vector3i(_moveVector.x, _moveVector.y, _moveVector.z);
			SelectionBoxManager.Instance.GetCategory("POIMarker")?.GetBox(_name)?.SetPositionAndSize(_marker.Start - getBaseVisualOffset(), _marker.Size);
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (!tryGetSelectedMarker(out var _marker))
		{
			_marker.Start += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			_marker.Size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			SelectionBoxManager.Instance.GetCategory("POIMarker")?.GetBox(_name)?.SetPositionAndSize(_marker.Start - getBaseVisualOffset(), _marker.Size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i getBaseVisualOffset()
	{
		Vector3i result = Vector3i.zero;
		if (PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			result = PrefabEditModeManager.Instance.VoxelPrefab.size * 0.5f;
			result.y = -1;
		}
		return result;
	}
}
