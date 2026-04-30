using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeExt3dModel : BlockShapeRotatedAbstract
{
	public string ext3dModelName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public VoxelMeshExt3dModel modelMesh;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] normals;

	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMesh[] cachedRotatedBoundsMeshes = new VoxelMesh[32];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 modelOffset;

	public BlockShapeExt3dModel()
	{
		IsSolidCube = false;
		IsSolidSpace = false;
		LightOpacity = 0;
	}

	public override void Init(Block _block)
	{
		ext3dModelName = _block.Properties.Values["Model"];
		if (ext3dModelName == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		modelOffset = Vector3.zero;
		if (_block.Properties.Values.ContainsKey("ModelOffset"))
		{
			modelOffset = StringParsers.ParseVector3(_block.Properties.Values["ModelOffset"]);
		}
		base.Init(_block);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createBoundingBoxes()
	{
		if (modelMesh != null && modelMesh.boundingBoxMesh.Vertices.Count <= 0)
		{
			base.createBoundingBoxes();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createVertices()
	{
		TextureAtlasExternalModels textureAtlasExternalModels = ((MeshDescription.meshes != null) ? ((TextureAtlasExternalModels)MeshDescription.meshes[0].textureAtlas) : null);
		if (textureAtlasExternalModels == null)
		{
			return;
		}
		if (!textureAtlasExternalModels.Meshes.ContainsKey(ext3dModelName))
		{
			throw new Exception("External 3D model with name '" + ext3dModelName + "' not found! Maybe you need to create the atlas first?");
		}
		modelMesh = ((TextureAtlasExternalModels)MeshDescription.meshes[0].textureAtlas).Meshes[ext3dModelName];
		vertices = modelMesh.Vertices.ToArray();
		normals = modelMesh.Normals.ToArray();
		if (modelMesh != null && modelMesh.aabb != null && modelMesh.aabb.Length != 0)
		{
			boundsArr = new Bounds[modelMesh.aabb.Length];
			for (int i = 0; i < modelMesh.aabb.Length; i++)
			{
				boundsArr[i] = new Bounds(modelMesh.aabb[i].center, modelMesh.aabb[i].size);
			}
		}
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return BlockShapeNew.GetRotationStatic(_blockValue.rotation);
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		Vector3[] array = rotateVertices(vertices, _drawPos + modelOffset, _blockValue);
		Vector3[] array2 = rotateNormals(normals, _blockValue);
		Block block = _blockValue.Block;
		byte meshIndex = block.MeshIndex;
		_meshes[meshIndex].CheckVertexLimit(array.Length);
		_meshes[meshIndex].AddMesh(_drawPos, vertices.Length, array, array2, modelMesh.Indices, modelMesh.Uvs, sun, blocklight, GetBoundsMesh(_blockValue), (int)(10f * (float)_blockValue.damage) / block.MaxDamage);
		MemoryPools.poolVector3.Free(array);
		MemoryPools.poolVector3.Free(array2);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] rotateNormals(Vector3[] _normals, BlockValue _blockValue)
	{
		Quaternion rotation = GetRotation(_blockValue);
		Vector3[] array = MemoryPools.poolVector3.Alloc(_normals.Length);
		for (int i = 0; i < _normals.Length; i++)
		{
			array[i] = rotation * _normals[i];
		}
		return array;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		if (_blockValue.Block.HasTag(BlockTags.Door) || _blockValue.Block.HasTag(BlockTags.Window))
		{
			return 0f;
		}
		return base.GetStepHeight(_blockValue, crossingFace);
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		if (_blockValue.Block.HasTag(BlockTags.Door) || _blockValue.Block.HasTag(BlockTags.Window))
		{
			return true;
		}
		return base.IsMovementBlocked(_blockValue, crossingFace);
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += ((!_bLeft) ? 1 : (-1));
		if (_rotation > 9)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 9;
		}
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_bLeft)
		{
			_rotCount = -_rotCount;
		}
		int rotation = _blockValue.rotation;
		if (rotation >= 24)
		{
			_blockValue.rotation = (byte)(((rotation - 24 + _rotCount) & 3) + 24);
		}
		else
		{
			int num = 90 * _rotCount;
			_blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree(rotation, Quaternion.AngleAxis(num, Vector3.up));
		}
		return _blockValue;
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(180f, Vector3.up);
	}

	public override VoxelMesh GetBoundsMesh(BlockValue _blockValue)
	{
		VoxelMesh voxelMesh = cachedRotatedBoundsMeshes[_blockValue.rotation];
		if (voxelMesh == null)
		{
			Vector3[] array = modelMesh.boundingBoxMesh.Vertices.ToArray();
			Vector3[] array2 = rotateVertices(array, Vector3.zero, _blockValue);
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] += modelOffset;
			}
			voxelMesh = new VoxelMesh(-1, 0);
			voxelMesh.Vertices.AddRange(array2, 0, array2.Length);
			voxelMesh.Indices = modelMesh.boundingBoxMesh.Indices;
			MemoryPools.poolVector3.Free(array2);
			cachedRotatedBoundsMeshes[_blockValue.rotation] = voxelMesh;
		}
		return voxelMesh;
	}
}
