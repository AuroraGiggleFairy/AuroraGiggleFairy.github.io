using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardComplex : BlockShapeBillboardAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[12];

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		v[0] = new Vector3(_drawPos.x - 0.2f, _drawPos.y - 0.2f, _drawPos.z - 0.2f);
		v[1] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y - 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		v[2] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y - 0.2f, _drawPos.z - 0.2f);
		v[3] = new Vector3(_drawPos.x - 0.2f, _drawPos.y - 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		v[4] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z - 0.2f);
		v[5] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z + 1f + 0.2f + 0.01f);
		v[6] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z - 0.2f + 0.01f);
		v[7] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 1f + 0.2f, _drawPos.z + 1f + 0.2f);
		v[8] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 0.5f, _drawPos.z - 0.2f);
		v[9] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 0.5f + 0.01f, _drawPos.z - 0.2f);
		v[10] = new Vector3(_drawPos.x + 1f + 0.2f, _drawPos.y + 0.5f + 0.01f, _drawPos.z + 1f + 0.2f);
		v[11] = new Vector3(_drawPos.x - 0.2f, _drawPos.y + 0.5f, _drawPos.z + 1f + 0.2f);
		Block block = _blockValue.Block;
		VoxelMesh obj = _meshes[block.MeshIndex];
		obj.AddBlockSide(v[0], v[1], v[5], v[4], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[1], v[0], v[4], v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[3], v[2], v[6], v[7], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[2], v[3], v[7], v[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[8], v[9], v[10], v[11], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[11], v[10], v[9], v[8], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
	}
}
