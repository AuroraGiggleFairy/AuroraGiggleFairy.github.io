using UnityEngine;

public interface ISelectionBoxCallback
{
	bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated);

	void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector);

	void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest);

	void OnSelectionBoxMirrored(Vector3i _axis);

	bool OnSelectionBoxDelete(string _category, string _name);

	bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria);

	void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager);

	void OnSelectionBoxRotated(string _category, string _name);
}
