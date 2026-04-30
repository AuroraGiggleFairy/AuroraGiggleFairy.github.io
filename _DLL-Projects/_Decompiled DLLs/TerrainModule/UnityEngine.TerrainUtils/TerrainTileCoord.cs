namespace UnityEngine.TerrainUtils;

public readonly struct TerrainTileCoord(int tileX, int tileZ)
{
	public readonly int tileX = tileX;

	public readonly int tileZ = tileZ;
}
