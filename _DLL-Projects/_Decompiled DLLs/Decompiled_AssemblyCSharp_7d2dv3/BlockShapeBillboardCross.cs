using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardCross : BlockShapeBillboardAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float h = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float s;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[8];

	public BlockShapeBillboardCross()
	{
	}

	public BlockShapeBillboardCross(float _scaleAdd)
	{
		s = _scaleAdd;
		h = 1f + _scaleAdd;
		yPosSubtract += _scaleAdd;
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		float num = _drawPos.y;
		float num2 = _drawPos.y;
		float num3 = _drawPos.y;
		float num4 = _drawPos.y;
		if (_vertices != null)
		{
			float num5 = _drawPos.y - _vertices[0].y;
			num = _vertices[0].y + (_vertices[3].y - _vertices[0].y) * 0.5f - yPosSubtract + num5;
			num2 = _vertices[3].y + (_vertices[7].y - _vertices[3].y) * 0.5f - yPosSubtract + num5;
			num3 = _vertices[0].y + (_vertices[4].y - _vertices[0].y) * 0.5f - yPosSubtract + num5;
			num4 = _vertices[4].y + (_vertices[7].y - _vertices[4].y) * 0.5f - yPosSubtract + num5;
		}
		v[0] = new Vector3(_drawPos.x - s, num, _drawPos.z + 0.5f);
		v[1] = new Vector3(_drawPos.x + 1f + s, num2, _drawPos.z + 0.5f);
		v[2] = new Vector3(_drawPos.x + 0.5f, num3, _drawPos.z - s);
		v[3] = new Vector3(_drawPos.x + 0.5f, num4, _drawPos.z + 1f + s);
		v[4] = new Vector3(_drawPos.x - s, num + h, _drawPos.z + 0.5f);
		v[5] = new Vector3(_drawPos.x + 1f + s, num2 + h, _drawPos.z + 0.5f);
		v[6] = new Vector3(_drawPos.x + 0.5f, num3 + h, _drawPos.z - s);
		v[7] = new Vector3(_drawPos.x + 0.5f, num4 + h, _drawPos.z + 1f + s);
		Block block = _blockValue.Block;
		VoxelMesh obj = _meshes[block.MeshIndex];
		obj.AddBlockSide(v[0], v[1], v[5], v[4], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[1], v[0], v[4], v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[3], v[2], v[6], v[7], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
		obj.AddBlockSide(v[2], v[3], v[7], v[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, block.MeshIndex);
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}
}
