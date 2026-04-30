using UnityEngine;

public interface IWireNode
{
	Vector3 GetStartPosition();

	Vector3 GetStartPositionOffset();

	void SetStartPosition(Vector3 pos);

	void SetStartPositionOffset(Vector3 pos);

	Vector3 GetEndPosition();

	Vector3 GetEndPositionOffset();

	void SetEndPosition(Vector3 pos);

	void SetEndPositionOffset(Vector3 pos);

	void SetWireDip(float _dist);

	float GetWireDip();

	void SetWireRadius(float _radius);

	void SetWireCanHide(bool _canHide);

	void SetVisible(bool _visible);

	void Reset();

	void BuildMesh();

	void SetPulseColor(Color color);

	void TogglePulse(bool isOn);

	GameObject GetGameObject();

	Bounds GetBounds();
}
