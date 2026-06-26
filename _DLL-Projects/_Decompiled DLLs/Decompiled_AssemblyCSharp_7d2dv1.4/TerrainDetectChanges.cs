using UnityEngine;

[ExecuteInEditMode]
public class TerrainDetectChanges : MonoBehaviour
{
	public bool bChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTerrainChanged(TerrainChangedFlags flags)
	{
		_ = flags & TerrainChangedFlags.Heightmap;
		if ((flags & TerrainChangedFlags.DelayedHeightmapUpdate) != 0)
		{
			bChanged = true;
		}
		_ = flags & TerrainChangedFlags.TreeInstances;
	}
}
