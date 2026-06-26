using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardAbstract : BlockShape
{
	public float yPosSubtract;

	public BlockShapeBillboardAbstract()
	{
		IsSolidCube = false;
		IsSolidSpace = false;
		LightOpacity = 0;
		IsOmitTerrainSnappingUp = true;
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}
}
