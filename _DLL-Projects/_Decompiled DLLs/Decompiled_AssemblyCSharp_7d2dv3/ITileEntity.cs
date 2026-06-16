using System.Collections.Generic;
using UnityEngine;

public interface ITileEntity : ILockTarget
{
	List<ITileEntityChangedListener> listeners { get; }

	BlockValue blockValue { get; }

	bool IsRemoving { get; set; }

	event XUiEvent_TileEntityDestroyed Destroyed;

	void SetUserAccessing(bool _bUserAccessing);

	bool IsUserAccessing();

	void SetModified();

	Chunk GetChunk();

	Vector3i ToWorldPos();

	Vector3 ToWorldCenterPos();
}
