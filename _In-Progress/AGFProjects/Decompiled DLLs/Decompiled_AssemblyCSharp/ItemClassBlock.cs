using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassBlock : ItemClass
{
	public class ItemBlockInventoryData : ItemInventoryData
	{
		public float lastBuildTime;

		public byte rotation;

		public BlockPlacement.EnumRotationMode mode = BlockPlacement.EnumRotationMode.Simple;

		public int localRot;

		public int damage;

		public ItemBlockInventoryData(ItemClass _item, ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
			: base(_item, _itemStack, _gameManager, _holdingEntity, _slotIdx)
		{
			lastBuildTime = 0f;
			rotation = 128;
			Block obj = Block.list[_item.Id];
			if (obj.HandleFace != BlockFace.None)
			{
				mode = BlockPlacement.EnumRotationMode.ToFace;
			}
			if (obj.BlockPlacementHelper != BlockPlacement.None)
			{
				mode = BlockPlacement.EnumRotationMode.Auto;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cNoRotation = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 renderOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	public ItemClassBlock()
	{
		HoldType = new DataItem<int>(7);
		AnimationDelayData.AnimationDelay[HoldType.Value] = new AnimationDelayData.AnimationDelays(0f, 0f, 0.31f, 0.31f, _twoHanded: true);
		Stacknumber = new DataItem<int>(500);
	}

	public override void Init()
	{
		base.Init();
		Block block = Block.list[base.Id];
		DescriptionKey = block.DescriptionKey;
		MadeOfMaterial = block.blockMaterial;
		if (block.CustomIcon != null)
		{
			CustomIcon = new DataItem<string>(block.CustomIcon);
		}
		NoScrapping = block.NoScrapping;
		CustomIconTint = block.CustomIconTint;
		SortOrder = block.SortOrder;
		CreativeMode = block.CreativeMode;
		TraderStageTemplate = block.TraderStageTemplate;
		SoundPickup = block.SoundPickup;
		SoundPlace = block.SoundPlace;
	}

	public override bool IsActionRunning(ItemInventoryData _data)
	{
		return Time.time - (_data as ItemBlockInventoryData).lastBuildTime < Constants.cBuildIntervall;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ItemInventoryData createItemInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
	{
		return new ItemBlockInventoryData(this, _itemStack, _gameManager, _holdingEntity, _slotIdx);
	}

	public override void StopHolding(ItemInventoryData _data, Transform _modelTransform)
	{
	}

	public override bool IsBlock()
	{
		return true;
	}

	public override Block GetBlock()
	{
		return Block.list[base.Id];
	}

	public override string GetItemName()
	{
		return GetBlock().GetBlockName();
	}

	public override string GetLocalizedItemName()
	{
		return GetBlock().GetLocalizedBlockName();
	}

	public override bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return GetBlock().Tags.Test_AnySet(_tags);
	}

	public override bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return GetBlock().Tags.Test_AllSet(_tags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue GetBlockValueFromItemValue(ItemValue _itemValue)
	{
		Block block = GetBlock();
		if (block.SelectAlternates)
		{
			return block.GetAltBlockValue(_itemValue.Meta);
		}
		return _itemValue.ToBlockValue();
	}

	public override Transform CloneModel(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		return CreateMesh(_go, _world, _blockValue, _vertices, _position, _parent, _purpose, _textureFullArray);
	}

	public override Transform CloneModel(World _world, ItemValue _itemValue, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		return CreateMesh(null, _world, GetBlockValueFromItemValue(_itemValue), null, _position, _parent, _purpose, _textureFullArray);
	}

	public static Transform CreateMesh(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray = default(TextureFullArray))
	{
		if (_purpose == BlockShape.MeshPurpose.Drop)
		{
			GameObject gameObject = Object.Instantiate(LoadManager.LoadAsset<GameObject>("@:Other/Items/Misc/sack_droppedPrefab.prefab", null, null, false, true).Asset);
			Transform transform = gameObject.transform;
			transform.SetParent(_parent, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			return gameObject.transform;
		}
		Block block = _blockValue.Block;
		if (block.shape is BlockShapeModelEntity blockShapeModelEntity)
		{
			if (_go == null)
			{
				_go = new GameObject();
				_go.transform.SetParent(_parent, worldPositionStays: false);
			}
			blockShapeModelEntity.CloneModel(_blockValue, _go.transform);
			return _go.transform;
		}
		int meshIndex = block.MeshIndex;
		Transform transform2 = CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFullArray, meshIndex);
		if (meshIndex == 0 && (_purpose == BlockShape.MeshPurpose.Preview || _purpose == BlockShape.MeshPurpose.Local))
		{
			Transform transform3 = CreateMeshOfType(_go, _world, _blockValue, _vertices, _worldPos, _parent, _purpose, _textureFullArray, 2);
			if ((bool)transform3)
			{
				if ((bool)transform2)
				{
					transform3.SetParent(transform2, worldPositionStays: false);
				}
				else
				{
					transform2 = transform3;
				}
			}
		}
		return transform2;
	}

	public static Transform CreateMeshOfType(GameObject _go, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _worldPos, Transform _parent, BlockShape.MeshPurpose _purpose, TextureFullArray _textureFullArray, int _meshIndex)
	{
		Vector3i vector3i = World.worldToBlockPos(_worldPos);
		_world.GetSunAndBlockColors(vector3i, out var sunLight, out var blockLight);
		VoxelMesh voxelMesh = VoxelMesh.Create(_meshIndex, MeshDescription.meshes[_meshIndex].meshType, 1);
		VoxelMesh[] array = new VoxelMesh[MeshDescription.meshes.Length];
		array[_meshIndex] = voxelMesh;
		_blockValue.Block.shape.renderFull(vector3i, _blockValue, renderOffsetV, _vertices, new LightingAround(sunLight, blockLight, 0), _textureFullArray, array, _purpose);
		if (voxelMesh.m_Vertices.Count == 0)
		{
			return null;
		}
		if (_go == null)
		{
			_go = new GameObject();
			_go.transform.SetParent(_parent, worldPositionStays: false);
			_go.AddComponent<UpdateLightOnChunkMesh>();
		}
		_go.name = "Block_" + _blockValue.type;
		VoxelMesh.CreateMeshFilter(_meshIndex, 0, _go, "Item", false, out MeshFilter[] _mf, out MeshRenderer[] _mr);
		if (_mf[0] != null)
		{
			voxelMesh.CopyToMesh(_mf, _mr, 0);
		}
		return _go.transform;
	}

	public override EnumCrosshairType GetCrosshairType(ItemInventoryData _holdingData)
	{
		return EnumCrosshairType.Plus;
	}

	public override RenderCubeType GetFocusType(ItemInventoryData _data)
	{
		return RenderCubeType.FullBlockBothSides;
	}

	public override float GetFocusRange()
	{
		return Constants.cDigAndBuildDistance;
	}

	public override void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
	{
		if (_data.holdingEntity is EntityPlayerLocal { bFirstPersonView: false } entityPlayerLocal)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
		}
		if (_actionIdx == 0)
		{
			GameManager.Instance.GetActiveBlockTool().ExecuteAttackAction(_data, _bReleased, _playerActions);
		}
		else
		{
			GameManager.Instance.GetActiveBlockTool().ExecuteUseAction(_data, _bReleased, _playerActions);
		}
	}

	public override bool IsFocusBlockInside()
	{
		return false;
	}

	public override Vector3 GetDroppedCorrectionRotation()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionRotation()
	{
		return new Vector3(90f, 0f, 0f);
	}

	public override Vector3 GetCorrectionPosition()
	{
		return Vector3.zero;
	}

	public override Vector3 GetCorrectionScale()
	{
		return new Vector3(0.1f, 0.1f, 0.1f);
	}

	public override bool CanHold()
	{
		return false;
	}
}
