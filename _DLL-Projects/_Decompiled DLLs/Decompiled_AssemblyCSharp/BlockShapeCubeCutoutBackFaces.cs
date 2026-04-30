using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeCubeCutoutBackFaces : BlockShapeCubeCutout
{
	public BlockShapeCubeCutoutBackFaces()
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

	public override void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		base.renderFace(_worldPos, _blockValue, _drawPos, _face, _vertices, _lightingAround, _textureFullArray, _meshes, _purpose);
		byte meshIndex = _blockValue.Block.MeshIndex;
		Rect uVRectFromSideAndMetadata = block.getUVRectFromSideAndMetadata(meshIndex, _face, _vertices, _blockValue);
		_meshes[meshIndex].AddQuadWithCracks(_vertices[3], Color.white, _vertices[2], Color.white, _vertices[1], Color.white, _vertices[0], Color.white, uVRectFromSideAndMetadata, WorldConstants.MapDamageToUVRect(_blockValue), bSwitchUvHorizontal: false);
	}
}
