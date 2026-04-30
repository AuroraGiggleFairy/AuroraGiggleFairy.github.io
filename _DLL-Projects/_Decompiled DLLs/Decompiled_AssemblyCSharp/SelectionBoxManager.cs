using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SelectionBoxManager : MonoBehaviour
{
	public const string CategoryDynamicPrefab = "DynamicPrefabs";

	public const string CategoryStartPoint = "StartPoint";

	public const string CategorySelection = "Selection";

	public const string CategoryTraderTeleport = "TraderTeleport";

	public const string CategoryInfoVolume = "InfoVolume";

	public const string CategoryWallVolume = "WallVolume";

	public const string CategoryTriggerVolume = "TriggerVolume";

	public const string CategorySleeperVolume = "SleeperVolume";

	public const string CategoryPOIMarker = "POIMarker";

	public const string CategoryPrefabFacingVolume = "PrefabFacing";

	public static Color ColDynamicPrefabInactive = new Color(0f, 0.4f, 0f, 0.6f);

	public static Color ColDynamicPrefabActive = new Color(0.6f, 1f, 0f, 0.15f);

	public static Color ColDynamicPrefabFaceSel = new Color(0f, 1f, 0f, 0.6f);

	public static Color ColEntitySpawnerInactive = new Color(0.6f, 0f, 0f, 0.6f);

	public static Color ColEntitySpawnerActive = new Color(1f, 0f, 0f, 0.4f);

	public static Color ColEntitySpawnerFaceSel = new Color(1f, 1f, 0f, 0.3f);

	public static Color ColEntitySpawnerTrigger = new Color(1f, 1f, 0f, 0.3f);

	public static Color ColStartPointInactive = new Color(1f, 1f, 1f, 0.5f);

	public static Color ColStartPointActive = new Color(1f, 1f, 0f, 0.8f);

	public static Color ColSelectionActive = new Color(0f, 0f, 1f, 0.5f);

	public static Color ColSelectionInactive = new Color(0f, 0f, 1f, 0.5f);

	public static Color ColSelectionFaceSel = new Color(1f, 1f, 0f, 0.4f);

	public static Color ColTraderTeleportInactive = new Color(0.5f, 0f, 0.5f, 0.6f);

	public static Color ColTraderTeleport = new Color(1f, 0f, 1f, 0.3f);

	public static Color ColSleeperVolume = new Color(0.7f, 0.75f, 1f, 0.3f);

	public static Color ColSleeperVolumeInactive = new Color(0.25f, 0.25f, 0.5f, 0.6f);

	public static Color ColTriggerVolume = new Color(1f, 0f, 0f, 0.4f);

	public static Color ColTriggerVolumeInactive = new Color(0.6f, 0f, 0f, 0.6f);

	public static Color ColInfoVolume = new Color(0f, 1f, 1f, 0.4f);

	public static Color ColInfoVolumeInactive = new Color(0f, 0.6f, 0.6f, 0.6f);

	public static Color ColWallVolume = new Color(0.5f, 1f, 1f, 0.4f);

	public static Color ColWallVolumeInactive = new Color(0.5f, 0.6f, 0.6f, 0.6f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SelectionCategory> categories = new Dictionary<string, SelectionCategory>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public (SelectionCategory category, SelectionBox box)? selection;

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

	public (SelectionCategory category, SelectionBox box)? Selection
	{
		get
		{
			if (!selection.HasValue)
			{
				return null;
			}
			if (selection.Value.box == null)
			{
				return null;
			}
			return selection.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			selection = value;
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Origin.Remove(base.transform);
	}

	public Dictionary<string, SelectionCategory> GetCategories()
	{
		return categories;
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

	public void CreateCategory(string _name, Color _colSelected, Color _colUnselected, Color _colFaceSelected, bool _bCollider, string _tag, int _layer = 0)
	{
		Transform transform = new GameObject(_name).transform;
		transform.parent = base.transform;
		SelectionCategory selectionCategory = new SelectionCategory(_name, transform, _colSelected, _colUnselected, _colFaceSelected, _bCollider, _tag, null, _layer);
		selectionCategory.SetVisible(_bVisible: false);
		categories[_name] = selectionCategory;
	}

	public void SetUserData(string _category, string _name, object _data)
	{
		if (TryGetSelectionBox(_category, _name, out var _selectionBox))
		{
			_selectionBox.UserData = _data;
		}
		UpdateSleepersAndMarkers();
	}

	public bool IsActive(string _category, string _name)
	{
		if (!TryGetSelectionBox(_category, _name, out var _selectionBox))
		{
			return false;
		}
		return Selection?.box == _selectionBox;
	}

	public void SetActive(string _category, string _name, bool _bActive)
	{
		SelectionCategory category = GetCategory(_category);
		if (!category.IsVisible())
		{
			category.SetVisible(_bVisible: true);
		}
		if (TryGetSelectionBox(_category, _name, out var _selectionBox))
		{
			activate(categories[_category], _bActive ? _selectionBox : null);
		}
	}

	public void SetFacingDirection(string _category, string _name, float _facing)
	{
		SelectionCategory category = GetCategory(_category);
		if (!category.IsVisible())
		{
			category.SetVisible(_bVisible: true);
		}
		if (TryGetSelectionBox(_category, _name, out var _selectionBox))
		{
			_selectionBox.facingDirection = _facing;
		}
	}

	public void Deactivate()
	{
		activate(null, null);
	}

	public bool GetSelected(out string _selectedCategory, out string _selectedName)
	{
		if (Selection.HasValue)
		{
			_selectedCategory = Selection.Value.category.name;
			_selectedName = Selection.Value.box.name;
			return true;
		}
		_selectedCategory = null;
		_selectedName = null;
		return false;
	}

	public void Unselect()
	{
		Selection = null;
		UpdateSleepersAndMarkers();
	}

	public bool Select(WorldRayHitInfo _hitInfo)
	{
		if (_hitInfo.tag == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			if (!_hitInfo.tag.Equals(category.Value.tag))
			{
				continue;
			}
			foreach (KeyValuePair<string, SelectionBox> box in category.Value.boxes)
			{
				if (box.Value.GetBoxTransform() == _hitInfo.transform)
				{
					if (category.Value.name != "SleeperVolume")
					{
						SleeperVolumeToolManager.ShowSleepers(bShow: false);
					}
					Manager.PlayButtonClick();
					return activate(category.Value, box.Value);
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activate(SelectionCategory _cat, SelectionBox _box)
	{
		(SelectionCategory, SelectionBox)? tuple = Selection;
		bool result = true;
		if (Selection?.box == _box || _box == null)
		{
			Selection = null;
		}
		else
		{
			Selection = (_cat, _box);
			_box.SetFrameActive(_active: true);
			_box.SetAllFacesColor(_cat.colActive);
		}
		if (tuple.HasValue)
		{
			tuple.Value.Item2.SetFrameActive(_active: false);
			tuple.Value.Item2.SetAllFacesColor(tuple.Value.Item1.colInactive);
			tuple.Value.Item1.callback?.OnSelectionBoxActivated(tuple.Value.Item1.name, tuple.Value.Item2.name, _bActivated: false);
		}
		if (Selection.HasValue && Selection.Value.category.callback != null)
		{
			result = Selection.Value.category.callback.OnSelectionBoxActivated(Selection.Value.category.name, Selection.Value.box.name, _bActivated: true);
		}
		UpdateSleepersAndMarkers();
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSleepersAndMarkers()
	{
		if (!Selection.HasValue)
		{
			SleeperVolumeToolManager.SelectionChanged(null);
			POIMarkerToolManager.SelectionChanged(null);
			return;
		}
		var (selectionCategory, selBox) = Selection.Value;
		if (selectionCategory.name.Equals("SleeperVolume"))
		{
			SleeperVolumeToolManager.SelectionChanged(selBox);
		}
		else if (selectionCategory.name.Equals("POIMarker"))
		{
			POIMarkerToolManager.SelectionChanged(selBox);
		}
	}

	public void UpdateAllColors()
	{
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			foreach (KeyValuePair<string, SelectionBox> box in category.Value.boxes)
			{
				Color c = ((Selection?.box == box.Value) ? category.Value.colActive : category.Value.colInactive);
				box.Value.SetAllFacesColor(c);
			}
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SelectionCategory> category in categories)
		{
			category.Value.Clear();
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
		if (Selection?.category.callback != null)
		{
			Selection.Value.category.callback.OnSelectionBoxMoved(Selection.Value.category.name, Selection.Value.box.name, _deltaVec.ToVector3());
			UpdateSleepersAndMarkers();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void incSelection(int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (Selection?.category.callback != null && Selection.Value.category.callback.OnSelectionBoxIsAvailable(Selection.Value.category.name, EnumSelectionBoxAvailabilities.CanResize))
		{
			Selection.Value.category.callback.OnSelectionBoxSized(Selection.Value.category.name, Selection.Value.box.name, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
			UpdateSleepersAndMarkers();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void mirrorSelection(Vector3i _axis)
	{
		if (Selection?.category.callback != null && Selection.Value.category.callback.OnSelectionBoxIsAvailable(Selection.Value.category.name, EnumSelectionBoxAvailabilities.CanMirror))
		{
			Selection.Value.category.callback.OnSelectionBoxMirrored(_axis);
			UpdateSleepersAndMarkers();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowThroughWalls(string _categoryName, bool _isThroughWalls, bool _isAll)
	{
		if (_isAll)
		{
			foreach (KeyValuePair<string, SelectionBox> box in categories[_categoryName].boxes)
			{
				box.Value.ShowThroughWalls(_isThroughWalls);
			}
			return;
		}
		Selection?.box.ShowThroughWalls(_isThroughWalls);
	}

	public bool ConsumeScrollWheel(float _scrollWheel, PlayerActionsLocal _playerActions)
	{
		if (Selection?.category.callback == null)
		{
			return false;
		}
		if (Mathf.Abs(_scrollWheel) < 0.1f)
		{
			return false;
		}
		int num = Mathf.RoundToInt(_scrollWheel * 10f);
		bool result = false;
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 2)
		{
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
		}
		return result;
	}

	public void CheckKeys(GameManager _gameManager, PlayerActionsLocal _playerActions, WorldRayHitInfo _hitInfo)
	{
		bool altKeyPressed = InputUtils.AltKeyPressed;
		GameManager.bVolumeBlocksEditing = !altKeyPressed;
		foreach (var (categoryName, selectionCategory2) in categories)
		{
			if (selectionCategory2.IsVisible())
			{
				ShowThroughWalls(categoryName, altKeyPressed, _isAll: true);
			}
		}
		if (_playerActions.SelectionRotate.WasPressed && !Input.GetKey(KeyCode.Tab))
		{
			if (Selection?.category.callback == null)
			{
				BlockToolSelection.Instance.RotateFocusedBlock(_hitInfo, _playerActions);
			}
			else
			{
				var (selectionCategory3, selectionBox) = Selection.Value;
				selectionCategory3.callback.OnSelectionBoxRotated(selectionCategory3.name, selectionBox.name);
			}
		}
		if (Selection?.category.callback == null)
		{
			return;
		}
		(SelectionCategory category, SelectionBox box) value = Selection.Value;
		SelectionCategory item = value.category;
		SelectionBox item2 = value.box;
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
			Color colFaceSelected = item.colFaceSelected;
			if (_playerActions.Jump.WasPressed)
			{
				item2.SetFaceColor(BlockFace.Top, colFaceSelected);
			}
			else if (_playerActions.Jump.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
			if (_playerActions.Crouch.WasPressed && !controlKeyPressed)
			{
				item2.SetFaceColor(BlockFace.Bottom, colFaceSelected);
			}
			else if (_playerActions.Crouch.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
			if (_playerActions.MoveLeft.WasPressed)
			{
				item2.SetFaceColor(BlockFace.East, colFaceSelected);
			}
			else if (_playerActions.MoveLeft.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
			if (_playerActions.MoveRight.WasPressed)
			{
				item2.SetFaceColor(BlockFace.West, colFaceSelected);
			}
			else if (_playerActions.MoveRight.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
			if (_playerActions.MoveForward.WasPressed)
			{
				item2.SetFaceColor(BlockFace.North, colFaceSelected);
			}
			else if (_playerActions.MoveForward.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
			if (_playerActions.MoveBack.WasPressed)
			{
				item2.SetFaceColor(BlockFace.South, colFaceSelected);
			}
			else if (_playerActions.MoveBack.WasReleased)
			{
				item2.ResetAllFacesColor();
			}
		}
		if (_playerActions.SelectionDelete.WasPressed)
		{
			SetActive(item.name, item2.name, _bActive: false);
			if (item.callback.OnSelectionBoxDelete(item.name, item2.name))
			{
				Manager.PlayButtonClick();
				item.RemoveBox(item2.name);
				Selection = null;
			}
		}
		(SelectionCategory, SelectionBox)? tuple2 = Selection;
		if (tuple2.HasValue && tuple2.GetValueOrDefault().Item1.name.Equals("SleeperVolume"))
		{
			SleeperVolumeToolManager.CheckKeys();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		EntityPlayerLocal entityPlayerLocal = null;
		if (!Selection.HasValue || GameManager.Instance == null || GameManager.Instance.World == null || (entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer()) == null)
		{
			return;
		}
		Camera finalCamera = entityPlayerLocal.finalCamera;
		if (finalCamera == null)
		{
			return;
		}
		SelectionBox item = Selection.Value.box;
		if (item.Axises.Count == 0)
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
		Vector3 vector = finalCamera.WorldToScreenPoint(item.AxisOrigin);
		vector.z = 0f;
		bool flag = true;
		if (!bMousedPressed)
		{
			for (int i = 0; i < item.Axises.Count; i++)
			{
				Vector3 vector2 = finalCamera.WorldToScreenPoint(item.Axises[i]);
				vector2.z = 0f;
				if (GetLineDistanceSq(vector, vector2, mousePosition) < 225f)
				{
					highlightedAxis = item.AxisesI[i];
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
		item.HighlightAxis(highlightedAxis);
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
			float magnitude = (entityPlayerLocal.cameraTransform.position - item.AxisOrigin).magnitude;
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
	public float GetLineDistanceSq(Vector3 _lineStart, Vector3 _lineEnd, Vector3 _point)
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
