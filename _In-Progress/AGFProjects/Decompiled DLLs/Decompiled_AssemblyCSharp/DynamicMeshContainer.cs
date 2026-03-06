using UnityEngine;

public abstract class DynamicMeshContainer
{
	public Vector3i WorldPosition;

	public long Key;

	public string ToDebugLocation()
	{
		return WorldPosition.x + " " + WorldPosition.z;
	}

	public abstract GameObject GetGameObject();

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicMeshContainer()
	{
	}
}
