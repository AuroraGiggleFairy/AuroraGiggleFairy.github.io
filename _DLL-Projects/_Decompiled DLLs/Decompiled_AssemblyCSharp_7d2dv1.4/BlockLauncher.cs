using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockLauncher : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string playSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string startSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string endSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float soundDelay = 1f;

	public string AmmoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("options", "tool", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockLauncher()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("PlaySound"))
		{
			playSound = base.Properties.Values["PlaySound"];
		}
		if (base.Properties.Values.ContainsKey("StartSound"))
		{
			startSound = base.Properties.Values["StartSound"];
		}
		if (base.Properties.Values.ContainsKey("EndSound"))
		{
			endSound = base.Properties.Values["EndSound"];
		}
		if (base.Properties.Values.ContainsKey("AmmoItem"))
		{
			AmmoItemName = base.Properties.Values["AmmoItem"];
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = enabled;
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return "";
		}
		TileEntityPoweredRangedTrap obj = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityPoweredRangedTrap;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (obj != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate"), arg, arg2);
		}
		return "";
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap))
		{
			return false;
		}
		if (!(_commandName == "options"))
		{
			if (_commandName == "take")
			{
				TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
				return true;
			}
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityPoweredRangedTrap.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityPoweredRangedTrap.entityId, _player.entityId);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) != 0;
		TileEntityPoweredBlock tileEntityPoweredBlock = ((TileEntityPowered)_world.GetTileEntity(_cIdx, _blockPos)) as TileEntityPoweredBlock;
		if (flag)
		{
			if (tileEntityPoweredBlock != null && !PowerManager.Instance.ClientUpdateList.Contains(tileEntityPoweredBlock))
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredBlock);
			}
		}
		else if (tileEntityPoweredBlock != null && PowerManager.Instance.ClientUpdateList.Contains(tileEntityPoweredBlock))
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredBlock);
		}
		Transform transform = blockEntity.transform.Find("Activated");
		if (transform != null)
		{
			transform.gameObject.SetActive(flag);
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPowered == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPowered = CreateTileEntity(chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
		}
		tileEntityPowered.BlockTransform = _ebcd.transform;
		tileEntityPowered.MarkWireDirty();
		if (tileEntityPowered.GetParent().y != -9999 && _world.GetTileEntity(_cIdx, tileEntityPowered.GetParent()) is IPowered powered)
		{
			powered.DrawWires();
		}
		updateState(_world, _cIdx, _blockPos, _blockValue);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		updateState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		byte b = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		return true;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredRangedTrap))
			{
				TileEntityPowered tileEntityPowered = CreateTileEntity(_chunk);
				tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
				tileEntityPowered.InitializePowerData();
				_chunk.AddTileEntity(tileEntityPowered);
			}
			BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
			blockEntityData.bNeedsTemperature = true;
			_chunk.AddEntityBlockStub(blockEntityData);
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPoweredRangedTrap.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		bool num = (_blockValue.meta & 2) != 0;
		TileEntityPoweredBlock tileEntityPoweredBlock = ((TileEntityPowered)_world.GetTileEntity(_clrIdx, _blockPos)) as TileEntityPoweredBlock;
		if (num)
		{
			if (tileEntityPoweredBlock != null && !PowerManager.Instance.ClientUpdateList.Contains(tileEntityPoweredBlock))
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredBlock);
			}
		}
		else if (tileEntityPoweredBlock != null && PowerManager.Instance.ClientUpdateList.Contains(tileEntityPoweredBlock))
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredBlock);
		}
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredRangedTrap(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.RangedTrap
		};
	}

	public bool InstantiateProjectile(WorldBase _world, int _cIdx, Vector3i _blockPos)
	{
		if (_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap)
		{
			if (!tileEntityPoweredRangedTrap.IsLocked)
			{
				return false;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !tileEntityPoweredRangedTrap.DecrementAmmo())
			{
				tileEntityPoweredRangedTrap.IsLocked = false;
				tileEntityPoweredRangedTrap.SetModified();
				return false;
			}
			ItemClass ammoItem = tileEntityPoweredRangedTrap.AmmoItem;
			ItemClass.GetItem(ammoItem.GetItemName());
			if (ammoItem == null)
			{
				return false;
			}
			ItemValue itemValue = new ItemValue(ammoItem.Id, 2, 2);
			Transform transform = ammoItem.CloneModel((World)_world, itemValue, Vector3.zero, null, BlockShape.MeshPurpose.World, 0L);
			Transform transform2 = null;
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return false;
			}
			IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunkSync == null)
			{
				return false;
			}
			BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
			if (blockEntity == null || !blockEntity.bHasTransform)
			{
				return false;
			}
			transform2 = blockEntity.transform;
			if (transform2 != null)
			{
				transform.parent = transform2;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
			else
			{
				transform.parent = null;
			}
			Utils.SetLayerRecursively(transform.gameObject, (transform2 != null) ? transform2.gameObject.layer : 0);
			BlockProjectileMoveScript blockProjectileMoveScript = transform.gameObject.AddComponent<BlockProjectileMoveScript>();
			blockProjectileMoveScript.itemProjectile = ammoItem;
			blockProjectileMoveScript.itemValueProjectile = itemValue;
			blockProjectileMoveScript.itemValueLauncher = ItemValue.None.Clone();
			blockProjectileMoveScript.itemActionProjectile = (ItemActionProjectile)((ammoItem.Actions[0] is ItemActionProjectile) ? ammoItem.Actions[0] : ammoItem.Actions[1]);
			blockProjectileMoveScript.ProjectileOwnerID = tileEntityPoweredRangedTrap.OwnerEntityID;
			blockProjectileMoveScript.Fire(_blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f), transform2.forward, null);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Manager.BroadcastPlay(_blockPos.ToVector3(), playSound);
			}
			return true;
		}
		return false;
	}
}
