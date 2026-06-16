using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCategory
{
	public readonly string name;

	public readonly string LocalizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform transform;

	public readonly Color colActive;

	public readonly Color colInactive;

	public readonly Color colFaceSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool bCollider;

	public readonly string tag;

	public ISelectionBoxCallback BoxCallbacks;

	public ISelectionCategoryCallback CategoryCallbacks;

	public Action CheckKeysCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int layer;

	public readonly Dictionary<string, SelectionBox> boxes = new Dictionary<string, SelectionBox>();

	public SelectionCategory(string _name, Transform _transform, Color _colActive, Color _colInactive, Color _colFaceSelected, bool _bCollider, string _tag, ISelectionBoxCallback _boxCallbacks, int _layer = 0)
	{
		name = _name;
		LocalizedName = Localization.Get("selectionCategory" + name);
		transform = _transform;
		colActive = _colActive;
		colInactive = _colInactive;
		colFaceSelected = _colFaceSelected;
		bCollider = _bCollider;
		tag = _tag;
		BoxCallbacks = _boxCallbacks;
		layer = _layer;
	}

	public void SetCallback(ISelectionBoxCallback _callback)
	{
		BoxCallbacks = _callback;
	}

	public bool IsVisible()
	{
		return transform.gameObject.activeSelf;
	}

	public void SetVisible(bool _bVisible)
	{
		transform.gameObject.SetActive(_bVisible);
		CategoryCallbacks?.OnSelectionCategoryVisibilityChanged(this, _bVisible);
		if (!_bVisible && SelectionBoxManager.Instance.Selection?.Category == this)
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
			UnityEngine.Object.Destroy(box.Value.gameObject);
		}
		boxes.Clear();
		CategoryCallbacks?.OnSelectionCategoryCleared(this);
	}

	public SelectionBox AddBox(string _name, Vector3i _pos, Vector3i _size, bool _bDrawDirection = false, bool _bAlwaysDrawDirection = false)
	{
		if (boxes.TryGetValue(_name, out var _))
		{
			RemoveBox(_name);
		}
		Transform obj = new GameObject().transform;
		obj.parent = transform;
		SelectionBox selectionBox = obj.gameObject.AddComponent<SelectionBox>();
		selectionBox.name = _name;
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
		CategoryCallbacks?.OnSelectionCategoryBoxAdded(selectionBox);
		return selectionBox;
	}

	public SelectionBox GetBox(string _name)
	{
		boxes.TryGetValue(_name, out var value);
		return value;
	}

	public bool TryGetBox(string _name, out SelectionBox _box)
	{
		return boxes.TryGetValue(_name, out _box);
	}

	public void RenameBox(SelectionBox _box, string _newName)
	{
		if (!_box.name.Equals(_newName))
		{
			string key = _box.name;
			_box.name = _newName;
			boxes[_newName] = _box;
			boxes.Remove(key);
		}
	}

	public void RemoveBox(SelectionBox _box)
	{
		if (SelectionBoxManager.Instance.Selection == _box)
		{
			SelectionBoxManager.Instance.Deactivate();
		}
		CategoryCallbacks?.OnSelectionCategoryBoxRemoved(_box);
		UnityEngine.Object.Destroy(_box.gameObject);
		boxes.Remove(_box.name);
	}

	public void RemoveBox(string _name)
	{
		if (boxes.TryGetValue(_name, out var value))
		{
			RemoveBox(value);
		}
	}

	public override string ToString()
	{
		return "Cat " + name;
	}
}
