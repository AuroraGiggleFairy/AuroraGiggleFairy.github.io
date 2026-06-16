using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeWater : BlockShapeCube
{
	public BlockShapeWater()
	{
		IsSolidCube = false;
		IsSolidSpace = false;
		LightOpacity = 0;
	}

	public override void renderFace(Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, Vector2 UVdata, MeshPurpose _purpose = MeshPurpose.World)
	{
		_meshes[1].AddBasicQuad(_vertices, Color.white, UVdata, bForceNormalsUp: true);
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		return 0f;
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}
}
