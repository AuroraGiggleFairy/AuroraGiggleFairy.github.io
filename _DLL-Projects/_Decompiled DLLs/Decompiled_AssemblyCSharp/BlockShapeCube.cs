using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeCube : BlockShape
{
	public static readonly Vector3[] Cube = new Vector3[8]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] v = new Vector3[8];

	public BlockShapeCube()
	{
		IsSolidCube = true;
		IsSolidSpace = true;
		IsRotatable = true;
	}

	public override void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte meshIndex = _blockValue.Block.MeshIndex;
		Rect uVRectFromSideAndMetadata = block.getUVRectFromSideAndMetadata(meshIndex, _face, _vertices, _blockValue);
		_meshes[meshIndex].AddQuadWithCracks(_vertices[0], Color.white, _vertices[1], Color.white, _vertices[2], Color.white, _vertices[3], Color.white, uVRectFromSideAndMetadata, WorldConstants.MapDamageToUVRect(_blockValue), bSwitchUvHorizontal: false);
		if (_blockValue.hasdecal && _blockValue.decalface == _face)
		{
			UVRectTiling uVRectTiling = MeshDescription.meshes[4].textureAtlas.uvMapping[500 + _blockValue.decaltex];
			Utils.MoveInBlockFaceDirection(_vertices, _face, 0.02f);
			_meshes[4].AddQuadWithCracks(_vertices[0], Color.white, _vertices[1], Color.white, _vertices[2], Color.white, _vertices[3], Color.white, uVRectTiling.uv, WorldConstants.uvRectZero, bSwitchUvHorizontal: false);
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		byte meshIndex = _blockValue.Block.MeshIndex;
		VoxelMesh obj = _meshes[meshIndex];
		if (_vertices == null)
		{
			v[0] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z);
			v[1] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z);
			v[2] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z);
			v[3] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z);
			v[4] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z + 1f);
			v[5] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z + 1f);
			v[6] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z + 1f);
			v[7] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z + 1f);
		}
		else
		{
			v[0] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z) + _vertices[0];
			v[1] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z) + _vertices[1];
			v[2] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z) + _vertices[2];
			v[3] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z) + _vertices[3];
			v[4] = new Vector3(_drawPos.x, _drawPos.y, _drawPos.z + 1f) + _vertices[4];
			v[5] = new Vector3(_drawPos.x, _drawPos.y + 1f, _drawPos.z + 1f) + _vertices[5];
			v[6] = new Vector3(_drawPos.x + 1f, _drawPos.y + 1f, _drawPos.z + 1f) + _vertices[6];
			v[7] = new Vector3(_drawPos.x + 1f, _drawPos.y, _drawPos.z + 1f) + _vertices[7];
		}
		obj.AddBlockSide(v[0], v[3], v[2], v[1], _blockValue, VoxelMesh.COLOR_SOUTH, BlockFace.South, sun, blocklight, meshIndex);
		obj.AddBlockSide(v[7], v[4], v[5], v[6], _blockValue, VoxelMesh.COLOR_NORTH, BlockFace.North, sun, blocklight, meshIndex);
		obj.AddBlockSide(v[4], v[0], v[1], v[5], _blockValue, VoxelMesh.COLOR_WEST, BlockFace.West, sun, blocklight, meshIndex);
		obj.AddBlockSide(v[3], v[7], v[6], v[2], _blockValue, VoxelMesh.COLOR_EAST, BlockFace.East, sun, blocklight, meshIndex);
		obj.AddBlockSide(v[1], v[2], v[6], v[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, meshIndex);
		obj.AddBlockSide(v[4], v[7], v[3], v[0], _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, blocklight, meshIndex);
	}

	public override int MapSideAndRotationToTextureIdx(BlockValue _blockValue, BlockFace _side)
	{
		if (_side == BlockFace.Bottom || _side == BlockFace.Top)
		{
			return (int)_side;
		}
		return (int)(((_side - 2 + _blockValue.rotation) & BlockFace.West) + 2);
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += ((!_bLeft) ? 1 : (-1));
		if (_rotation > 3)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 3;
		}
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		for (int i = 0; i < _rotCount; i++)
		{
			byte b = _blockValue.rotation;
			if (b <= 3)
			{
				b = ((!_bLeft) ? ((byte)((b < 3) ? ((uint)(b + 1)) : 0u)) : ((byte)((b > 0) ? ((uint)(b - 1)) : 3u)));
			}
			else if (b <= 7)
			{
				b = ((!_bLeft) ? ((byte)((b < 7) ? ((uint)(b + 1)) : 4u)) : ((byte)((b > 4) ? ((uint)(b - 1)) : 7u)));
			}
			else if (b <= 11)
			{
				b = ((!_bLeft) ? ((byte)((b < 11) ? ((uint)(b + 1)) : 8u)) : ((byte)((b > 8) ? ((uint)(b - 1)) : 11u)));
			}
			_blockValue.rotation = b;
		}
		return _blockValue;
	}
}
