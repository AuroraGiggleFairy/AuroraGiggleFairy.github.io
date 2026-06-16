using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SelectionBoxManager : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SelectionCategory> categories = new Dictionary<string, SelectionCategory>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float alphaMultiplier = 1f;

	public static SelectionBoxManager Instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMousedPressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 highlightedAxisScreenDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i highlightedAxis = Vector3i.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mouseMoveDir = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastSelOpMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWaitForRelease;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryDynamicPrefab
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryStartPoint
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategorySelection
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryTraderTeleport
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryInfoVolume
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryWallVolume
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryTriggerVolume
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategorySleeperVolume
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryPOIMarker
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory CategoryPrefabFacingVolume
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public SelectionBox Selection
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float AlphaMultiplier
	{
		get
		{
			return alphaMultiplier;
		}
		set
		{
			alphaMultiplier = Mathf.Clamp01(value);
			GamePrefs.Set(EnumGamePrefs.OptionsSelectionBoxAlphaMultiplier, alphaMultiplier);
			UpdateAllColors();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		Instance = this;
		Origin.Add(base.transform, 1);
		alphaMultiplier = GamePrefs.GetFloat(EnumGamePrefs.OptionsSelectionBoxAlphaMultiplier);
	}

	public void SetupCategories()
	{
		Color colSelected = new Color(0f, 0f, 1f, 0.5f);
		Color colUnselected = new Color(0f, 0f, 1f, 0.5f);
		Color colFaceSelected = new Color(1f, 1f, 0f, 0.4f);
		Color colUnselected2 = new Color(1f, 1f, 1f, 0.5f);
		Color color = new Color(1f, 1f, 0f, 0.8f);
		Color colUnselected3 = new Color(0f, 0.4f, 0f, 0.6f);
		Color colSelected2 = new Color(0.6f, 1f, 0f, 0.15f);
		Color colFaceSelected2 = new Color(0f, 1f, 0f, 0.6f);
		Color colUnselected4 = new Color(0.5f, 0f, 0.5f, 0.6f);
		Color color2 = new Color(1f, 0f, 1f, 0.3f);
		Color color3 = new Color(0.7f, 0.75f, 1f, 0.3f);
		Color colUnselected5 = new Color(0.25f, 0.25f, 0.5f, 0.6f);
		Color color4 = new Color(0f, 1f, 1f, 0.4f);
		Color colUnselected6 = new Color(0f, 0.6f, 0.6f, 0.6f);
		Color color5 = new Color(0.5f, 1f, 1f, 0.4f);
		Color colUnselected7 = new Color(0.5f, 0.6f, 0.6f, 0.6f);
		Color color6 = new Color(1f, 0f, 0f, 0.4f);
		Color colUnselected8 = new Color(0.6f, 0f, 0f, 0.6f);
		CategorySelection = CreateCategory("Selection", colSelected, colUnselected, colFaceSelected, _bCollider: false, null);
		CategoryStartPoint = CreateCategory("StartPoint", color, colUnselected2, color, _bCollider: true, "SB_StartPoint", 31);
		CategoryDynamicPrefab = CreateCategory("DynamicPrefabs", colSelected2, colUnselected3, colFaceSelected2, _bCollider: true, "SB_Prefabs", 31);
		CategoryTraderTeleport = CreateCategory("TraderTeleport", color2, colUnselected4, color2, _bCollider: true, "SB_TraderTeleport", 31);
		CategorySleeperVolume = CreateCategory("SleeperVolume", color3, colUnselected5, color3, _bCollider: true, "SB_SleeperVolume", 31);
		CategoryInfoVolume = CreateCategory("InfoVolume", color4, colUnselected6, color4, _bCollider: true, "SB_InfoVolume", 31);
		CategoryWallVolume = CreateCategory("WallVolume", color5, colUnselected7, color5, _bCollider: true, "SB_WallVolume", 31);
		CategoryTriggerVolume = CreateCategory("TriggerVolume", color6, colUnselected8, color6, _bCollider: true, "SB_TriggerVolume", 31);
		CategoryPOIMarker = CreateCategory("POIMarker", colSelected2, colUnselected3, colFaceSelected2, _bCollider: true, "SB_Prefabs", 31);
		CategoryPrefabFacingVolume = CreateCategory("PrefabFacing", color3, colUnselected5, color3, _bCollider: true, "SB_SleeperVolume", 31);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Origin.Remove(base.transform);
	}

	public SelectionCategory GetCategory(string _name)
	{
		categories.TryGetValue(_name, out var value);
		return value;
	}

	public bool TryGetSelectionBox(string _category, string _name, out SelectionBox _selectionBox)
	{
		_selectionBox = GetCategory(_category)?.GetBox(_name);
		return _selectionBox != null;
	}

	public SelectionCategory CreateCategory(string _name, Color _colSelected, Color _colUnselected, Color _colFaceSelected, bool _bCollider, string _tag, int _layer = 0)
	{
		Transform transform = new GameObject(_name).transform;
		transform.parent = base.transform;
		SelectionCategory selectionCategory = new SelectionCategory(_name, transform, _colSelected, _colUnselected, _colFaceSelected, _bCollider, _tag, null, _layer);
		selectionCategory.SetVisible(_bVisible: false);
		categories[_name] = selectionCategory;
		return selectionCategory;
	}

	public void SetActive(SelectionBox _box, bool _bActive)
	{
		if (!(_box == null))
		{
			if (!_box.Category.IsVisible())
			{
				_box.Category.SetVisible(_bVisible: true);
			}
			activate(_bActive ? _box : null);
		}
	}

	public void Deactivate()
	{
		activate(null);
	}

	public bool TryGetSelected(out SelectionBox _selectedBox)
	{
		_selectedBox = Selection;
		return _selectedBox != null;
	}

	public bool Select(WorldRayHitInfo _hitInfo)
	{
		if (_hitInfo.tag == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			category.Deconstruct(out var key, out var value);
			SelectionCategory selectionCategory = value;
			if (!_hitInfo.tag.Equals(selectionCategory.tag))
			{
				continue;
			}
			foreach (KeyValuePair<string, SelectionBox> box in selectionCategory.boxes)
			{
				box.Deconstruct(out key, out var value2);
				SelectionBox selectionBox = value2;
				if (!(selectionBox.GetBoxTransform() != _hitInfo.transform))
				{
					Manager.PlayButtonClick();
					return activate(selectionBox);
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activate(SelectionBox _box)
	{
		SelectionBox selection = Selection;
		bool result = true;
		if (selection == _box || _box == null)
		{
			Selection = null;
		}
		else
		{
			Selection = _box;
			_box.SetFrameActive(_active: true);
			_box.SetAllFacesColor(_box.Category.colActive);
		}
		if (selection != null)
		{
			selection.SetFrameActive(_active: false);
			selection.SetAllFacesColor(selection.Category.colInactive);
			selection.Category.BoxCallbacks?.OnSelectionBoxActivated(selection, _bActivated: false);
		}
		if (Selection != null && Selection.Category.BoxCallbacks != null)
		{
			result = Selection.Category.BoxCallbacks.OnSelectionBoxActivated(Selection, _bActivated: true);
		}
		return result;
	}

	public void UpdateAllColors()
	{
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			category.Deconstruct(out var key, out var value);
			SelectionCategory selectionCategory = value;
			foreach (KeyValuePair<string, SelectionBox> box in selectionCategory.boxes)
			{
				box.Deconstruct(out key, out var value2);
				SelectionBox selectionBox = value2;
				Color c = ((Selection == selectionBox) ? selectionCategory.colActive : selectionCategory.colInactive);
				selectionBox.SetAllFacesColor(c);
			}
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			category.Deconstruct(out var _, out var value);
			value.Clear();
		}
	}

	public void OpenPropertiesWindow(GUIWindowManager _windowManager)
	{
		if (!(Selection == null))
		{
			SelectionCategory category = Selection.Category;
			ISelectionBoxCallback boxCallbacks = category.BoxCallbacks;
			if (boxCallbacks != null && boxCallbacks.OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities.CanShowProperties))
			{
				Manager.PlayButtonClick();
				category.BoxCallbacks.OnSelectionBoxShowProperties(_bVisible: true, _windowManager);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i createBlockMoveVector(Vector3 _relPlayerAxis)
	{
		return (!(Math.Abs(_relPlayerAxis.x) > Math.Abs(_relPlayerAxis.z))) ? new Vector3i(0f, 0f, Mathf.Sign(_relPlayerAxis.z)) : new Vector3i(Mathf.Sign(_relPlayerAxis.x), 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void moveSelection(Vector3i _deltaVec)
	{
		if (Selection?.Category.BoxCallbacks != null)
		{
			Selection.Category.BoxCallbacks.OnSelectionBoxMoved(Selection, _deltaVec.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void incSelection(int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (Selection?.Category.BoxCallbacks != null && Selection.Category.BoxCallbacks.OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities.CanResize))
		{
			Selection.Category.BoxCallbacks.OnSelectionBoxSized(Selection, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void mirrorSelection(Vector3i _axis)
	{
		if (Selection?.Category.BoxCallbacks != null && Selection.Category.BoxCallbacks.OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities.CanMirror))
		{
			Selection.Category.BoxCallbacks.OnSelectionBoxMirrored(_axis);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showThroughWalls(SelectionCategory _category, bool _isThroughWalls, bool _isAll)
	{
		if (_isAll)
		{
			foreach (KeyValuePair<string, SelectionBox> box in _category.boxes)
			{
				box.Deconstruct(out var _, out var value);
				value.ShowThroughWalls(_isThroughWalls);
			}
			return;
		}
		Selection.ShowThroughWalls(_isThroughWalls);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteBox(SelectionBox _selBox)
	{
		SetActive(_selBox, _bActive: false);
		if (_selBox.Category.BoxCallbacks.OnSelectionBoxDelete(_selBox, _checkCanDeleteOnly: false))
		{
			Manager.PlayButtonClick();
			_selBox.Category.RemoveBox(_selBox);
			Selection = null;
		}
	}

	public bool ConsumeScrollWheel(float _scrollWheel, PlayerActionsLocal _playerActions)
	{
		if (Selection?.Category.BoxCallbacks == null)
		{
			return false;
		}
		if (Mathf.Abs(_scrollWheel) < 0.1f)
		{
			return false;
		}
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) != 2)
		{
			return false;
		}
		int num = Mathf.RoundToInt(_scrollWheel * 10f);
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		bool result = false;
		if (_playerActions.Jump.IsPressed)
		{
			incSelection(num, 0, 0, 0, 0, 0);
			result = true;
		}
		if (_playerActions.Crouch.IsPressed && !controlKeyPressed)
		{
			incSelection(0, num, 0, 0, 0, 0);
			result = true;
		}
		if (_playerActions.MoveLeft.IsPressed)
		{
			incSelection(0, 0, 0, 0, num, 0);
			result = true;
		}
		if (_playerActions.MoveRight.IsPressed)
		{
			incSelection(0, 0, 0, 0, 0, num);
			result = true;
		}
		if (_playerActions.MoveForward.IsPressed)
		{
			incSelection(0, 0, num, 0, 0, 0);
			result = true;
		}
		if (_playerActions.MoveBack.IsPressed)
		{
			incSelection(0, 0, 0, num, 0, 0);
			result = true;
		}
		return result;
	}

	public void CheckKeys(GameManager _gameManager, PlayerActionsLocal _playerActions, WorldRayHitInfo _hitInfo)
	{
		bool altKeyPressed = InputUtils.AltKeyPressed;
		GameManager.bVolumeBlocksEditing = !altKeyPressed;
		foreach (var (_, selectionCategory2) in categories)
		{
			if (selectionCategory2.IsVisible())
			{
				showThroughWalls(selectionCategory2, altKeyPressed, _isAll: true);
			}
		}
		if (_playerActions.SelectionRotate.WasPressed && !Input.GetKey(KeyCode.Tab))
		{
			if (Selection?.Category.BoxCallbacks == null)
			{
				BlockToolSelection.Instance.RotateFocusedBlock(_hitInfo, _playerActions);
			}
			else
			{
				SelectionBox selection = Selection;
				selection.Category.BoxCallbacks.OnSelectionBoxRotated(selection);
			}
		}
		if (Selection?.Category.BoxCallbacks == null)
		{
			return;
		}
		SelectionBox selBox = Selection;
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 1 && GamePrefs.GetInt(EnumGamePrefs.SelectionContextMode) == 1)
		{
			if (_playerActions.MoveBack.WasPressed)
			{
				moveSelection(-1 * createBlockMoveVector(_gameManager.World.GetPrimaryPlayer().transform.forward));
			}
			if (_playerActions.MoveForward.WasPressed)
			{
				moveSelection(createBlockMoveVector(_gameManager.World.GetPrimaryPlayer().transform.forward));
			}
			if (_playerActions.MoveLeft.WasPressed)
			{
				moveSelection(-1 * createBlockMoveVector(_gameManager.World.GetPrimaryPlayer().transform.right));
			}
			if (_playerActions.MoveRight.WasPressed)
			{
				moveSelection(createBlockMoveVector(_gameManager.World.GetPrimaryPlayer().transform.right));
			}
		}
		else if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 1 && GamePrefs.GetInt(EnumGamePrefs.SelectionContextMode) == 0)
		{
			if (_playerActions.MoveBack.WasPressed)
			{
				moveSelection(-1 * Vector3i.forward);
			}
			if (_playerActions.MoveForward.WasPressed)
			{
				moveSelection(Vector3i.forward);
			}
			if (_playerActions.MoveLeft.WasPressed)
			{
				moveSelection(-1 * Vector3i.right);
			}
			if (_playerActions.MoveRight.WasPressed)
			{
				moveSelection(Vector3i.right);
			}
		}
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 1)
		{
			if (_playerActions.Jump.WasPressed)
			{
				moveSelection(new Vector3i(0, 1, 0));
			}
			if (_playerActions.Crouch.WasPressed && !controlKeyPressed)
			{
				moveSelection(new Vector3i(0, -1, 0));
			}
		}
		if (_playerActions.SelectionMoveMode.WasPressed)
		{
			GamePrefs.Set(EnumGamePrefs.SelectionContextMode, (GamePrefs.GetInt(EnumGamePrefs.SelectionContextMode) + 1) % 2);
		}
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 2)
		{
			Color colFaceSelected = selBox.Category.colFaceSelected;
			if (_playerActions.Jump.WasPressed)
			{
				selBox.SetFaceColor(BlockFace.Top, colFaceSelected);
			}
			else if (_playerActions.Jump.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
			if (_playerActions.Crouch.WasPressed && !controlKeyPressed)
			{
				selBox.SetFaceColor(BlockFace.Bottom, colFaceSelected);
			}
			else if (_playerActions.Crouch.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
			if (_playerActions.MoveLeft.WasPressed)
			{
				selBox.SetFaceColor(BlockFace.East, colFaceSelected);
			}
			else if (_playerActions.MoveLeft.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
			if (_playerActions.MoveRight.WasPressed)
			{
				selBox.SetFaceColor(BlockFace.West, colFaceSelected);
			}
			else if (_playerActions.MoveRight.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
			if (_playerActions.MoveForward.WasPressed)
			{
				selBox.SetFaceColor(BlockFace.North, colFaceSelected);
			}
			else if (_playerActions.MoveForward.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
			if (_playerActions.MoveBack.WasPressed)
			{
				selBox.SetFaceColor(BlockFace.South, colFaceSelected);
			}
			else if (_playerActions.MoveBack.WasReleased)
			{
				selBox.ResetAllFacesColor();
			}
		}
		if (_playerActions.SelectionDelete.WasPressed)
		{
			if (GamePrefs.GetBool(EnumGamePrefs.OptionsPoiVolumesSkipDeleteConfirmation))
			{
				deleteBox(selBox);
			}
			else if (selBox.Category.BoxCallbacks.OnSelectionBoxDelete(selBox, _checkCanDeleteOnly: true))
			{
				string text2 = string.Format(Localization.Get("xuiConfirmDeleteVolumeText"), selBox.Category.LocalizedName, selBox.Size.ToString(), selBox.Position.ToString(), selBox.name);
				XUiC_MessageBoxWindowGroup.ShowOkCancel(GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui, Localization.Get("xuiConfirmDeleteVolumeTitle"), text2, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					deleteBox(selBox);
				}, null, _openMainMenuOnClose: false);
			}
			else
			{
				deleteBox(selBox);
			}
		}
		Selection?.Category.CheckKeysCallback?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		EntityPlayerLocal primaryPlayer;
		if (Selection == null || GameManager.Instance == null || GameManager.Instance.World == null || (primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer()) == null)
		{
			return;
		}
		Camera finalCamera = primaryPlayer.finalCamera;
		if (finalCamera == null)
		{
			return;
		}
		SelectionBox selection = Selection;
		if (selection.Axises.Count == 0)
		{
			return;
		}
		if (lastSelOpMode != GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode))
		{
			lastSelOpMode = GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode);
			mouseMoveDir = Vector3.zero;
		}
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 0f;
		Vector3 vector = finalCamera.WorldToScreenPoint(selection.AxisOrigin);
		vector.z = 0f;
		bool flag = true;
		if (!bMousedPressed)
		{
			for (int i = 0; i < selection.Axises.Count; i++)
			{
				Vector3 vector2 = finalCamera.WorldToScreenPoint(selection.Axises[i]);
				vector2.z = 0f;
				if (GetLineDistanceSq(vector, vector2, mousePosition) < 225f)
				{
					highlightedAxis = selection.AxisesI[i];
					highlightedAxisScreenDir = (vector - vector2).normalized;
					flag = false;
					break;
				}
			}
		}
		if (!bMousedPressed && flag)
		{
			highlightedAxis = Vector3i.zero;
		}
		selection.HighlightAxis(highlightedAxis);
		if (bWaitForRelease && Input.GetMouseButton(0))
		{
			return;
		}
		bWaitForRelease = false;
		if (!highlightedAxis.Equals(Vector3i.zero))
		{
			if (!bMousedPressed && Input.GetMouseButtonDown(0))
			{
				bMousedPressed = true;
			}
			float magnitude = (primaryPlayer.cameraTransform.position - selection.AxisOrigin).magnitude;
			float num = Math.Max(0.5f, magnitude / 35f);
			mouseMoveDir += new Vector3((0f - Input.GetAxis("Mouse X")) * 5f, (0f - Input.GetAxis("Mouse Y")) * 5f, 0f) * num;
			float magnitude2 = mouseMoveDir.magnitude;
			if (bMousedPressed && !Input.GetMouseButtonUp(0) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 3)
			{
				mirrorSelection(highlightedAxis);
				bWaitForRelease = true;
			}
			if (bMousedPressed && magnitude2 > 5f)
			{
				float num2 = ((!highlightedAxis.Equals(Vector3i.one)) ? Vector3.Dot(highlightedAxisScreenDir, mouseMoveDir) : (-1f * Mathf.Sign(mouseMoveDir.y)));
				num2 *= magnitude2 * 0.05f;
				if (Mathf.Abs(num2) > 1f)
				{
					mouseMoveDir = Vector3.zero;
					int num3 = (int)Mathf.Sign(num2);
					if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 1)
					{
						moveSelection(highlightedAxis * num3);
					}
					else if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 2)
					{
						incSelection((highlightedAxis.y > 0) ? (highlightedAxis.y * num3) : 0, (highlightedAxis.y < 0) ? (-1 * highlightedAxis.y * num3) : 0, (highlightedAxis.z > 0) ? (highlightedAxis.z * num3) : 0, (highlightedAxis.z < 0) ? (-1 * highlightedAxis.z * num3) : 0, (highlightedAxis.x > 0) ? (highlightedAxis.x * num3) : 0, (highlightedAxis.x < 0) ? (-1 * highlightedAxis.x * num3) : 0);
					}
				}
			}
		}
		if (!Input.GetMouseButton(0))
		{
			bMousedPressed = false;
			mouseMoveDir = Vector3.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetLineDistanceSq(Vector3 _lineStart, Vector3 _lineEnd, Vector3 _point)
	{
		Vector3 vector = _lineEnd - _lineStart;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < 1E-06f)
		{
			return (_point - _lineStart).sqrMagnitude;
		}
		float num = Mathf.Clamp01(Vector3.Dot(_point - _lineStart, vector) / sqrMagnitude);
		return (_lineStart + vector * num - _point).sqrMagnitude;
	}
}
