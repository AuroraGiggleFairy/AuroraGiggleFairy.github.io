using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeTerrain : BlockShapeCube
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new readonly Vector3[] v = new Vector3[6]
	{
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(1f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 1f),
		new Vector3(0f, 0.5f, 0.5f),
		new Vector3(0.5f, 1f, 0.5f)
	};

	public BlockShapeTerrain()
	{
		IsOmitTerrainSnappingUp = true;
	}

	public override void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		float num = _drawPos.y - _vertices[0].y + 1f + 0.0001f;
		if (num > -0.01f)
		{
			num = 0.0001f;
		}
		UVRectTiling uVRectTiling = MeshDescription.meshes[4].textureAtlas.uvMapping[500 + _blockValue.decaltex];
		Utils.MoveInBlockFaceDirection(_vertices, _face, num);
		_meshes[4].AddQuadNoCollision(_vertices[0], _vertices[1], _vertices[2], _vertices[3], Color.white, uVRectTiling.uv);
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		if (_blockValue.hasdecal)
		{
			return _blockValue.decalface == _face;
		}
		return false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool IsTerrain()
	{
		return true;
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte blocklight = _lightingAround[LightingAround.Pos.Middle].block;
		VoxelMeshTerrain voxelMeshTerrain = (VoxelMeshTerrain)_meshes[5];
		Block obj = _blockValue.Block;
		byte meshIndex = obj.MeshIndex;
		voxelMeshTerrain.AddBlockSideTri(v[0] + _drawPos, v[2] + _drawPos, v[1] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[0] + _drawPos, v[1] + _drawPos, v[4] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[2] + _drawPos, v[5] + _drawPos, v[1] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[1] + _drawPos, v[5] + _drawPos, v[4] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[3] + _drawPos, v[5] + _drawPos, v[2] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[0] + _drawPos, v[3] + _drawPos, v[2] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_BOTTOM, BlockFace.Bottom, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[0] + _drawPos, v[4] + _drawPos, v[3] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight);
		voxelMeshTerrain.AddBlockSideTri(v[4] + _drawPos, v[5] + _drawPos, v[3] + _drawPos, meshIndex, _blockValue, VoxelMesh.COLOR_TOP, BlockFace.Top, sun, blocklight);
		int sideTextureId = obj.GetSideTextureId(_blockValue, BlockFace.Top, 0);
		int sideTextureId2 = obj.GetSideTextureId(_blockValue, BlockFace.South, 0);
		int submesh = voxelMeshTerrain.FindOrCreateSubMesh((sideTextureId << 16) | sideTextureId2, -1, -1);
		for (int i = 0; i < voxelMeshTerrain.Indices.Count; i += 3)
		{
			voxelMeshTerrain.AddIndices(voxelMeshTerrain.Indices[i], voxelMeshTerrain.Indices[i + 1], voxelMeshTerrain.Indices[i + 2], submesh);
		}
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(55f, Vector3.up) * Quaternion.AngleAxis(10f, Vector3.forward);
	}
}
