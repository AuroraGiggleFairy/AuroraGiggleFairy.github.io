namespace WorldGenerationEngineFinal;

public struct PathTile
{
	public enum PathTileStates : byte
	{
		Free,
		Blocked,
		Highway,
		Country
	}

	public PathTileStates TileState;
}
