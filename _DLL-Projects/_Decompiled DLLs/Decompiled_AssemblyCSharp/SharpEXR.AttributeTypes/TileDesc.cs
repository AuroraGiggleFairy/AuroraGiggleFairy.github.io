namespace SharpEXR.AttributeTypes;

public struct TileDesc
{
	public readonly uint XSize;

	public readonly uint YSize;

	public readonly LevelMode LevelMode;

	public readonly RoundingMode RoundingMode;

	public TileDesc(uint xSize, uint ySize, byte mode)
	{
		XSize = xSize;
		YSize = ySize;
		int roundingMode = (mode & 0xF0) >> 4;
		int levelMode = mode & 0xF;
		RoundingMode = (RoundingMode)roundingMode;
		LevelMode = (LevelMode)levelMode;
	}

	public override string ToString()
	{
		return $"{GetType().Name}: XSize={XSize}, YSize={YSize}";
	}
}
