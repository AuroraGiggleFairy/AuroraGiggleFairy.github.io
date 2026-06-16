using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardDiagonal : BlockShapeBillboardAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[8];

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		float num = 1f;
		float num2 = num;
		if (_vertices == null)
		{
			float y = _drawPos.y;
			float y2 = _drawPos.y;
			float y3 = _drawPos.y;
			float y4 = _drawPos.y;
			v[0] = new Vector3(_drawPos.x, y, _drawPos.z);
			v[1] = new Vector3(_drawPos.x + num, y2, _drawPos.z + num);
			v[2] = new Vector3(_drawPos.x + num, y3, _drawPos.z);
			v[3] = new Vector3(_drawPos.x, y4, _drawPos.z + num);
			v[4] = new Vector3(_drawPos.x, y + num2, _drawPos.z);
			v[5] = new Vector3(_drawPos.x + num, y2 + num2, _drawPos.z + num);
			v[6] = new Vector3(_drawPos.x + num, y3 + num2, _drawPos.z);
			v[7] = new Vector3(_drawPos.x, y4 + num2, _drawPos.z + num);
		}
		else
		{
			float num3 = _drawPos.y - _vertices[0].y;
			v[0] = _vertices[0] + new Vector3(0f, num3, 0f);
			v[1] = _vertices[7] + new Vector3(0f, num3, 0f);
			v[2] = _vertices[3] + new Vector3(0f, num3, 0f);
			v[3] = _vertices[4] + new Vector3(0f, num3, 0f);
			v[4] = new Vector3(_vertices[0].x, _vertices[0].y + num2 + num3, _vertices[0].z);
			v[5] = new Vector3(_vertices[7].x, _vertices[7].y + num2 + num3, _vertices[7].z);
			v[6] = new Vector3(_vertices[3].x, _vertices[3].y + num2 + num3, _vertices[3].z);
			v[7] = new Vector3(_vertices[4].x, _vertices[4].y + num2 + num3, _vertices[4].z);
		}
		Block block = _blockValue.Block;
		VoxelMesh obj = _meshes[block.MeshIndex];
		obj.AddBlockSide(v[0], v[1], v[5], v[4], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[1], v[0], v[4], v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[3], v[2], v[6], v[7], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[2], v[3], v[7], v[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
	}
}
