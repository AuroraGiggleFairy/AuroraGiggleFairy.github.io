using UnityEngine.Scripting;

[Preserve]
public class BlockShapeCubeCutout : BlockShapeCube
{
	public BlockShapeCubeCutout()
	{
		IsSolidCube = false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return _blockValue.type != _adjBlockValue.type;
	}
}
