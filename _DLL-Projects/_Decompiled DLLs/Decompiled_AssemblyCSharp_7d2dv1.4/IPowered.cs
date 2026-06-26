using System.Collections.Generic;
using UnityEngine;

public interface IPowered
{
	Vector3i GetParent();

	PowerItem GetPowerItem();

	void DrawWires();

	void RemoveWires();

	void MarkWireDirty();

	void MarkChanged();

	void AddWireData(Vector3i child);

	Vector3 GetWireOffset();

	int GetRequiredPower();

	bool CanHaveParent(IPowered newParent);

	void SetParentWithWireTool(IPowered parent, int entityID);

	void RemoveParentWithWiringTool(int wiringEntityID);

	void SetWireData(List<Vector3i> wireChildren);

	void SendWireData();

	void CreateWireDataFromPowerItem();
}
