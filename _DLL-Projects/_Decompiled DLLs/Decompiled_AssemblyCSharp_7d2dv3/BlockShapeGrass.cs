using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeGrass : BlockShapeBillboardAbstract
{
	public BlockShapeGrass()
	{
		LightOpacity = 0;
		IsOmitTerrainSnappingUp = true;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		_block.IsDecoration = true;
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		Vector3 drawPos = default(Vector3);
		drawPos.x = _drawPos.x + (float)((int)_drawPos.z & 1) * 0.2f - 0.1f;
		drawPos.y = _drawPos.y;
		drawPos.z = _drawPos.z + (float)((int)_drawPos.x & 1) * 0.2f - 0.1f;
		byte meta2and = _blockValue.meta2and1;
		BlockShapeBillboardPlant.RenderData data = default(BlockShapeBillboardPlant.RenderData);
		data.offsetY = -0.15f;
		data.scale = 0.93f + (float)(meta2and >> 6) * 0.07f;
		data.height = data.scale;
		switch ((meta2and >> 3) & 3)
		{
		case 0:
			data.count = 2 + MeshDescription.GrassQualityPlanes;
			data.scale *= 0.8f;
			data.sideShift = 0.045f;
			break;
		case 1:
			data.count = 2 + MeshDescription.GrassQualityPlanes;
			data.sideShift = 0.075f;
			break;
		case 2:
			data.count = 3 + MeshDescription.GrassQualityPlanes;
			data.sideShift = 0.09f;
			break;
		default:
			data.count = 3 + MeshDescription.GrassQualityPlanes * 2;
			data.sideShift = 0.2f;
			break;
		}
		data.count2 = 0;
		data.sideShift *= data.scale;
		data.rotation = 10f + (float)(_blockValue.rotation & 7) * 22.5f;
		Block block = _blockValue.Block;
		VoxelMesh mesh = _meshes[block.MeshIndex];
		int num = _blockValue.meta & 7;
		if (num >= 6)
		{
			num = 0;
		}
		BlockFace side = (BlockFace)num;
		BlockShapeBillboardPlant.RenderSpinMesh(uvTex: block.getUVRectFromSideAndMetadata(block.MeshIndex, side, Vector3.zero, _blockValue), _sunlight: _lightingAround[LightingAround.Pos.Middle].sun, _blocklight: _lightingAround[LightingAround.Pos.Middle].block, _mesh: mesh, _drawPos: drawPos, _vertices: _vertices, _data: data);
		BlockShapeBillboardPlant.AddCollider(mesh, _drawPos, 0.85f);
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		return 0f;
	}
}
