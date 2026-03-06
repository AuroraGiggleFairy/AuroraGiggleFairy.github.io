using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeBillboardPlant : BlockShapeBillboardRotatedAbstract
{
	public struct RenderData
	{
		public int count;

		public int count2;

		public float height;

		public float rotation;

		public float scale;

		public float sideShift;

		public float offsetY;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float xzAdd = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float yAdd = 0.15f;

	public BlockShapeBillboardPlant()
	{
		boundsArr = new Bounds[1] { BoundsUtils.BoundsForMinMax(0.3f, 0f, 0.3f, 0.7f, 1f, 0.7f) };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createVertices()
	{
		vertices = new Vector3[8]
		{
			new Vector3(-0.35f, -0.15f, -0.35f),
			new Vector3(-0.34f, 1.15f, -0.35f),
			new Vector3(1.36f, 1.15f, -0.35f),
			new Vector3(1.35f, -0.15f, -0.35f),
			new Vector3(-0.35f, -0.15f, 1.35f),
			new Vector3(-0.34f, 1.15f, 1.35f),
			new Vector3(1.36f, 1.15f, 1.35f),
			new Vector3(1.35f, -0.15f, 1.35f)
		};
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return Quaternion.AngleAxis(20 * _blockValue.rotation, Vector3.up);
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		Block block = _blockValue.Block;
		int meshIndex = block.MeshIndex;
		VoxelMesh voxelMesh = _meshes[meshIndex];
		if (meshIndex == 3)
		{
			Rect uVRectFromSideAndMetadata = block.getUVRectFromSideAndMetadata(meshIndex, BlockFace.Top, Vector3.zero, _blockValue);
			RenderData data = default(RenderData);
			data.count = 2 + MeshDescription.GrassQualityPlanes;
			data.count2 = 0;
			data.offsetY = -0.05f;
			data.scale = 1.25f;
			data.height = data.scale;
			data.sideShift = 0.04f;
			data.rotation = 20 * _blockValue.rotation;
			RenderSpinMesh(voxelMesh, _drawPos, _vertices, uVRectFromSideAndMetadata, sun, blocklight, data);
			AddCollider(voxelMesh, _drawPos, 0.85f);
		}
		else
		{
			Vector3[] array = rotateVertices(vertices, _drawPos, _blockValue);
			voxelMesh.AddBlockSide(array[7], array[0], array[1], array[6], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, meshIndex);
			voxelMesh.AddBlockSide(array[0], array[7], array[6], array[1], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, meshIndex);
			voxelMesh.AddBlockSide(array[3], array[4], array[5], array[2], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, meshIndex);
			voxelMesh.AddBlockSide(array[4], array[3], array[2], array[5], _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight, meshIndex);
			MemoryPools.poolVector3.Free(array);
		}
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		return boundsArr;
	}

	public override byte Rotate(bool _isLeft, int _rotation)
	{
		_rotation = (_rotation + ((!_isLeft) ? 1 : (-1))) & 3;
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		_blockValue.rotation = (byte)((_blockValue.rotation + _rotCount) & 3);
		return _blockValue;
	}

	public static void RenderSpinMesh(VoxelMesh _mesh, Vector3 _drawPos, Vector3[] _vertices, Rect uvTex, byte _sunlight, byte _blocklight, RenderData _data)
	{
		float num = _drawPos.y;
		if (_vertices != null)
		{
			num += _data.offsetY;
		}
		float num2 = num + _data.height;
		float num3 = _drawPos.x + 0.5f;
		float num4 = _drawPos.z + 0.5f;
		float num5 = 180f / (float)_data.count;
		num5 += 180f;
		float num6 = 0.5f * _data.scale;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 v = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		for (int i = 0; i < _data.count; i++)
		{
			float f = ((float)i * num5 + _data.rotation) * (MathF.PI / 180f);
			float num7 = Mathf.Sin(f);
			float num8 = Mathf.Cos(f);
			float num9 = 0f - num6;
			float sideShift = _data.sideShift;
			vector.x = num3 + num9 * num8 - sideShift * num7;
			vector.y = num;
			vector.z = num4 + num9 * num7 + sideShift * num8;
			num9 = num6;
			vector2.x = num3 + num9 * num8 - sideShift * num7;
			vector2.y = num;
			vector2.z = num4 + num9 * num7 + sideShift * num8;
			float num10 = num2;
			num10 += (float)(i % 3) * -0.15f;
			v.x = vector2.x;
			v.y = num10;
			v.z = vector2.z;
			vector3.x = vector.x;
			vector3.y = num10;
			vector3.z = vector.z;
			vector4.x = num7;
			vector4.y = 0f;
			vector4.z = 0f - num8;
			Vector4 tangent = (vector2 - vector).normalized;
			tangent.w = -1f;
			Vector3 vector5 = -vector4;
			_mesh.AddRectangle(vector, BlockShape.uvZero, vector2, BlockShape.uvRightBot, v, BlockShape.uvOne, vector3, BlockShape.uvLeftTop, vector4, vector4, tangent, uvTex, _sunlight, _blocklight);
			tangent.w = 1f;
			_mesh.AddRectangle(vector, BlockShape.uvZero, vector3, BlockShape.uvLeftTop, v, BlockShape.uvOne, vector2, BlockShape.uvRightBot, vector5, vector5, tangent, uvTex, _sunlight, _blocklight);
		}
	}

	public static void RenderGridMesh(VoxelMesh _mesh, Vector3 _drawPos, Vector3[] _vertices, Rect uvTex, byte _sunlight, byte _blocklight, RenderData _data)
	{
		float num = _drawPos.y;
		if (_vertices != null)
		{
			num += _data.offsetY;
		}
		float num2 = num + _data.height - 0.12f;
		float num3 = _drawPos.x + 0.5f;
		float num4 = _drawPos.z + 0.5f;
		float num5 = _data.rotation;
		float num6 = 165f / (float)_data.count2;
		float num7 = _data.sideShift * 2f / ((float)_data.count - 0.99f);
		float num8 = _data.sideShift * 1.85f;
		Vector3 normal = default(Vector3);
		normal.y = 0f;
		Vector3 normal2 = default(Vector3);
		normal2.y = 0f;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		Vector3 vector5 = default(Vector3);
		for (int i = 0; i < _data.count2; i++)
		{
			float num9 = 0f - _data.sideShift;
			float f = num5 * (MathF.PI / 180f);
			float num10 = Mathf.Sin(f);
			float num11 = Mathf.Cos(f);
			normal.x = 0f - num11;
			normal.z = 0f - num10;
			for (int j = 0; j < _data.count; j++)
			{
				float num12 = num3 - num8 * num10;
				float num13 = num4 + num8 * num11;
				vector.x = num12 + num9 * num11;
				vector.y = num;
				vector.z = num13 + num9 * num10;
				float num14 = num2;
				num14 += (float)(j % 3) * 0.06f;
				float num15 = num9 * 0.7f;
				vector2.x = num12 + num15 * num11;
				vector2.y = num14;
				vector2.z = num13 + num15 * num10;
				num12 = num3 + num8 * num10;
				num13 = num4 - num8 * num11;
				vector3.x = num12 + num9 * num11;
				vector3.y = num;
				vector3.z = num13 + num9 * num10;
				vector4.x = num12 + num15 * num11;
				vector4.y = num14;
				vector4.z = num13 + num15 * num10;
				vector5.x = normal.x;
				vector5.y = (num15 - num9) * 2.2f;
				vector5.z = normal.z;
				vector5 *= 1f / vector5.magnitude;
				normal2.x = 0f - normal.x;
				normal2.z = 0f - normal.z;
				Vector4 tangent = (vector3 - vector).normalized;
				tangent.w = -1f;
				if ((j & 1) == 0)
				{
					_mesh.AddRectangle(vector, BlockShape.uvZero, vector3, BlockShape.uvRightBot, vector4, BlockShape.uvOne, vector2, BlockShape.uvLeftTop, normal, vector5, tangent, uvTex, _sunlight, _blocklight);
					tangent.w = 1f;
					_mesh.AddRectangle(vector3, BlockShape.uvRightBot, vector, BlockShape.uvZero, vector2, BlockShape.uvLeftTop, vector4, BlockShape.uvOne, normal2, -vector5, tangent, uvTex, _sunlight, _blocklight);
				}
				else
				{
					_mesh.AddRectangle(vector, BlockShape.uvRightBot, vector3, BlockShape.uvZero, vector4, BlockShape.uvLeftTop, vector2, BlockShape.uvOne, normal, vector5, tangent, uvTex, _sunlight, _blocklight);
					tangent.w = 1f;
					_mesh.AddRectangle(vector3, BlockShape.uvZero, vector, BlockShape.uvRightBot, vector2, BlockShape.uvOne, vector4, BlockShape.uvLeftTop, normal2, -vector5, tangent, uvTex, _sunlight, _blocklight);
				}
				num9 += num7;
			}
			num5 += num6;
		}
	}

	public static void AddCollider(VoxelMesh _mesh, Vector3 _drawPos, float _height)
	{
		float num = _drawPos.x + 0.5f;
		float num2 = _drawPos.z + 0.5f;
		float y = _drawPos.y;
		float y2 = y + _height;
		Vector3 v = default(Vector3);
		v.x = num - 0.45f;
		v.y = y;
		v.z = num2;
		Vector3 v2 = default(Vector3);
		v2.x = num + 0.45f;
		v2.y = y;
		v2.z = num2;
		Vector3 v3 = default(Vector3);
		v3.x = v2.x;
		v3.y = y2;
		v3.z = num2;
		Vector3 v4 = default(Vector3);
		v4.x = v.x;
		v4.y = y2;
		v4.z = num2;
		_mesh.AddRectangleColliderPair(v, v2, v3, v4);
		v.x = num;
		v.z = num2 - 0.45f;
		v2.x = num;
		v2.z = num2 + 0.45f;
		v3.x = num;
		v3.z = v2.z;
		v4.x = num;
		v4.z = v.z;
		_mesh.AddRectangleColliderPair(v, v2, v3, v4);
	}
}
