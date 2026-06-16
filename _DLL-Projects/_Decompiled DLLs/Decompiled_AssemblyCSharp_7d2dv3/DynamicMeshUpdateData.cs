public class DynamicMeshUpdateData
{
	public Vector3i ChunkPosition;

	public long Key;

	public float MaxTime;

	public float UpdateTime;

	public bool IsUrgent;

	public bool AddToThread;

	public string ToDebugLocation()
	{
		return ChunkPosition.x + "," + ChunkPosition.z;
	}
}
