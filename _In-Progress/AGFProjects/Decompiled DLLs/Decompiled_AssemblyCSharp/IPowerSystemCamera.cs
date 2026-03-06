using UnityEngine;

public interface IPowerSystemCamera
{
	void SetPitch(float pitch);

	void SetYaw(float yaw);

	float GetPitch();

	float GetYaw();

	Transform GetCameraTransform();

	void SetUserAccessing(bool userAccessing);

	bool HasCone();

	void SetConeColor(Color _color);

	Color GetOriginalConeColor();

	void SetConeActive(bool _active);

	bool GetConeActive();

	bool HasLaser();

	void SetLaserColor(Color _color);

	Color GetOriginalLaserColor();

	void SetLaserActive(bool _active);

	bool GetLaserActive();
}
