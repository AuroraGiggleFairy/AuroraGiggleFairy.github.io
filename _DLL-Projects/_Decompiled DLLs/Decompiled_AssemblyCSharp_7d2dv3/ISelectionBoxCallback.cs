using UnityEngine;

public interface ISelectionBoxCallback
{
	bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated);

	void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector);

	void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest);

	void OnSelectionBoxMirrored(Vector3i _axis);

	bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly);

	bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria);

	void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager);

	void OnSelectionBoxRotated(SelectionBox _box);

	void OnSelectionBoxUserDataChanged(SelectionBox _box);
}
