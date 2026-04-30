using UnityEngine;

public abstract class BlockShape
{
	public enum MeshPurpose
	{
		World,
		Drop,
		Hold,
		Local,
		Preview
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvZero = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvOne = Vector2.one;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvRightBot = new Vector2(1f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvLeftTop = new Vector2(0f, 1f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvMiddle = new Vector2(0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvMidBot = new Vector2(0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvMidTop = new Vector2(0.5f, 1f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvLeftMid = new Vector2(0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector2 uvRightMid = new Vector2(1f, 0.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector4 tngRight = new Vector4(1f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Block block;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds bounds;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Bounds[] boundsArr;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 minBounds;

	public bool IsSolidCube;

	public bool IsRotatable;

	public bool IsSolidSpace;

	public bool IsOmitTerrainSnappingUp;

	public bool IsNotifyOnLoadUnload;

	public byte LightOpacity;

	public int SymmetryType = 1;

	public bool Has45DegreeRotations;

	public BlockShape()
	{
		bounds = BoundsUtils.BoundsForMinMax(0f, 0f, 0f, 1f, 1f, 1f);
		boundsArr = new Bounds[1] { bounds };
		IsSolidCube = true;
		IsSolidSpace = true;
		IsRotatable = false;
		IsOmitTerrainSnappingUp = false;
		LightOpacity = byte.MaxValue;
		minBounds = Vector3.zero;
	}

	public virtual void Init(Block _block)
	{
		block = _block;
	}

	public virtual void LateInit()
	{
	}

	public virtual Quaternion GetRotation(BlockValue _blockValue)
	{
		return Quaternion.identity;
	}

	public virtual Vector3 GetRotationOffset(BlockValue _blockValue)
	{
		return Vector3.zero;
	}

	public virtual int[][] GetRotationLookup(int _rotation)
	{
		return null;
	}

	public virtual byte Rotate(bool _bLeft, int _rotation)
	{
		return (byte)_rotation;
	}

	public virtual BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		_blockValue.rotation = (byte)((_blockValue.rotation + _rotCount) & 0xF);
		return _blockValue;
	}

	public virtual BlockValue MirrorY(bool _bAlongZ, BlockValue _blockValue)
	{
		_blockValue = RotateY(_bLeft: true, _blockValue, 1);
		return RotateY(_bLeft: true, _blockValue, 1);
	}

	public virtual int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 255;
	}

	public virtual bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return true;
	}

	public virtual void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
	}

	public virtual void renderFace(Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, Vector2 UVdata, MeshPurpose _purpose = MeshPurpose.World)
	{
	}

	public virtual void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
	}

	public virtual bool IsRenderDecoration()
	{
		return false;
	}

	public virtual void renderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		renderFull(_worldPos, _blockValue, _drawPos, _vertices, _lightingAround, _textureFullArray, _meshes);
	}

	public virtual int MapSideAndRotationToTextureIdx(BlockValue _blockValue, BlockFace _side)
	{
		return (int)_side;
	}

	public virtual Bounds[] GetBounds(BlockValue _blockValue)
	{
		return boundsArr;
	}

	public virtual Vector2 GetPathOffset(int _rotation)
	{
		return Vector2.zero;
	}

	public virtual Quaternion GetPreviewRotation()
	{
		return Quaternion.identity;
	}

	public virtual Vector3 GetPreviewPosition()
	{
		return Vector3.zero;
	}

	public virtual float GetStepHeight(BlockValue blockDef, BlockFace crossingFace)
	{
		return blockDef.Block.IsCollideMovement ? 1 : 0;
	}

	public virtual bool IsMovementBlocked(BlockValue blockDef, BlockFace crossingFace)
	{
		return GetStepHeight(blockDef, crossingFace) > 0.5f;
	}

	public void SetMinAABB(Vector3 _add)
	{
		minBounds = _add;
	}

	public virtual bool IsTerrain()
	{
		return false;
	}

	public virtual void OnBlockValueChanged(WorldBase _world, Vector3i _blockPos, int _clrIdx, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
	}

	public virtual void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
	}

	public virtual void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
	}

	public virtual void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual VoxelMesh GetBoundsMesh(BlockValue _blockValue)
	{
		return null;
	}

	public virtual BlockFace GetRotatedBlockFace(BlockValue _blockValue, BlockFace _face)
	{
		return _face;
	}

	public virtual void MirrorFace(EnumMirrorAlong _axis, int _sourceRot, int _targetRot, BlockFace _face, out BlockFace _sourceFace, out BlockFace _targetFace)
	{
		_sourceFace = _face;
		_targetFace = _face;
	}

	public virtual int GetVertexCount()
	{
		return 0;
	}

	public virtual int GetTriangleCount()
	{
		return 0;
	}

	public virtual string GetName()
	{
		return string.Empty;
	}

	public virtual bool UseRepairDamageState(BlockValue _blockValue)
	{
		return false;
	}
}
