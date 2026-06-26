using System.Collections.Generic;
using UnityEngine;

public class SelectionCategory
{
	public readonly string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform transform;

	public readonly Color colActive;

	public readonly Color colInactive;

	public readonly Color colFaceSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool bCollider;

	public readonly string tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int layer;

	public readonly Dictionary<string, SelectionBox> boxes = new Dictionary<string, SelectionBox>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ISelectionBoxCallback callback
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public SelectionCategory(string _name, Transform _transform, Color _colActive, Color _colInactive, Color _colFaceSelected, bool _bCollider, string _tag, ISelectionBoxCallback _callback, int _layer = 0)
	{
		name = _name;
		transform = _transform;
		colActive = _colActive;
		colInactive = _colInactive;
		colFaceSelected = _colFaceSelected;
		bCollider = _bCollider;
		tag = _tag;
		callback = _callback;
		layer = _layer;
	}

	public void SetCallback(ISelectionBoxCallback _callback)
	{
		callback = _callback;
	}

	public bool IsVisible()
	{
		return transform.gameObject.activeSelf;
	}

	public void SetVisible(bool _bVisible)
	{
		transform.gameObject.SetActive(_bVisible);
		string text = name;
		if (!(text == "SleeperVolume"))
		{
			if (text == "POIMarker")
			{
				POIMarkerToolManager.UpdateAllColors();
				POIMarkerToolManager.ShowPOIMarkers(_bVisible && SelectionBoxManager.Instance.Selection?.category != null);
			}
		}
		else
		{
			SleeperVolumeToolManager.SetVisible(_bVisible);
		}
		if (!_bVisible && SelectionBoxManager.Instance.Selection?.category == this)
		{
			SelectionBoxManager.Instance.Deactivate();
		}
	}

	public void SetCaptionVisibility(bool _visible)
	{
		foreach (KeyValuePair<string, SelectionBox> box in boxes)
		{
			box.Value.SetCaptionVisibility(_visible);
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, SelectionBox> box in boxes)
		{
			Object.Destroy(box.Value.gameObject);
		}
		boxes.Clear();
		if (name == "SleeperVolume")
		{
			SleeperVolumeToolManager.ClearSleeperVolumes();
		}
	}

	public SelectionBox AddBox(string _name, Vector3 _pos, Vector3i _size, bool _bDrawDirection = false, bool _bAlwaysDrawDirection = false)
	{
		if (boxes.TryGetValue(_name, out var _))
		{
			RemoveBox(_name);
		}
		Transform obj = new GameObject(_name).transform;
		obj.parent = transform;
		SelectionBox selectionBox = obj.gameObject.AddComponent<SelectionBox>();
		selectionBox.SetOwner(this);
		selectionBox.SetAllFacesColor(colInactive);
		selectionBox.bDrawDirection = _bDrawDirection;
		selectionBox.bAlwaysDrawDirection = _bAlwaysDrawDirection;
		selectionBox.SetPositionAndSize(_pos, _size);
		if (bCollider)
		{
			selectionBox.EnableCollider(tag, layer);
		}
		boxes[_name] = selectionBox;
		if (name == "SleeperVolume")
		{
			SleeperVolumeToolManager.RegisterSleeperVolume(selectionBox);
		}
		return selectionBox;
	}

	public SelectionBox GetBox(string _name)
	{
		boxes.TryGetValue(_name, out var value);
		return value;
	}

	public void RenameBox(string _name, string _newName)
	{
		if (!_name.Equals(_newName) && boxes.TryGetValue(_name, out var value))
		{
			value.name = _newName;
			boxes[_newName] = value;
			boxes.Remove(_name);
		}
	}

	public void RemoveBox(string _name)
	{
		if (boxes.TryGetValue(_name, out var value))
		{
			if (SelectionBoxManager.Instance.Selection?.box == value)
			{
				SelectionBoxManager.Instance.Deactivate();
			}
			if (name == "SleeperVolume")
			{
				SleeperVolumeToolManager.UnRegisterSleeperVolume(value);
			}
			Object.Destroy(value.gameObject);
			boxes.Remove(_name);
		}
	}
}
