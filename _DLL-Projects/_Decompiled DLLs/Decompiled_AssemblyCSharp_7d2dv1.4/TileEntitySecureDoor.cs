public class TileEntitySecureDoor : TileEntitySecure
{
	public TileEntitySecureDoor(Chunk _chunk)
		: base(_chunk)
	{
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.SecureDoor;
	}
}
