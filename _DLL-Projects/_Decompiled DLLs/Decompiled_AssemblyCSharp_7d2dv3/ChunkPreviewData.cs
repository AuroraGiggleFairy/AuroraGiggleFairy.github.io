public class ChunkPreviewData
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3i WorldPosition { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Prefab PrefabData { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance PrefabInstance { get; set; }
}
