public struct TileAreaConfig
{
	public Vector2i tileStart;

	public Vector2i tileEnd;

	public int tileSizeInWorldUnits;

	public bool bWrapAroundX;

	public bool bWrapAroundZ;

	public void checkCoordinates(ref int _tileX, ref int _tileZ)
	{
		Vector2i vector2i = tileEnd - tileStart + new Vector2i(1, 1);
		if (_tileX < tileStart.x)
		{
			if (bWrapAroundX)
			{
				_tileX += vector2i.x;
			}
			else
			{
				_tileX = tileStart.x;
			}
		}
		else if (_tileX > tileEnd.x)
		{
			if (bWrapAroundX)
			{
				_tileX -= vector2i.x;
			}
			else
			{
				_tileX = tileEnd.x;
			}
		}
		if (_tileZ < tileStart.y)
		{
			if (bWrapAroundZ)
			{
				_tileZ += vector2i.y;
			}
			else
			{
				_tileZ = tileStart.y;
			}
		}
		else if (_tileZ > tileEnd.y)
		{
			if (bWrapAroundZ)
			{
				_tileZ -= vector2i.y;
			}
			else
			{
				_tileZ = tileEnd.y;
			}
		}
	}
}
