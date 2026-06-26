namespace WorldGenerationEngineFinal;

public class PathTile
{
	public enum PathTileStates : byte
	{
		Free,
		Blocked,
		Highway,
		Country
	}

	public PathTileStates TileState;

	public byte PathRadius;

	public Path Path;
}
