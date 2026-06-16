using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockWorkstation : BlockParticle
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay;

	public WorkstationData WorkstationData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] toolTransformNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float CraftingParticleLightIntensity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[3]
	{
		new BlockActivationCommand("open", "campfire", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false),
		new BlockActivationCommand("extract", "store_all_up", _enabled: false)
	};

	public BlockWorkstation()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		TakeDelay = 2f;
		base.Properties.ParseFloat("TakeDelay", ref TakeDelay);
		string optionalValue = "1,2,3";
		base.Properties.ParseString("Workstation", "ToolNames", ref optionalValue);
		toolTransformNames = optionalValue.Split(',');
		WorkstationData = new WorkstationData(GetBlockName(), base.Properties);
		CraftingManager.AddWorkstationData(WorkstationData);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			TileEntityWorkstation tileEntityWorkstation = new TileEntityWorkstation(_chunk);
			tileEntityWorkstation.localChunkPos = World.toBlock(_blockPos);
			_chunk.AddTileEntity(tileEntityWorkstation);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntityWorkstation>((World)world, World.toBlock(_blockPos));
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_result.blockPos);
		if (tileEntityWorkstation != null)
		{
			tileEntityWorkstation.IsPlayerPlaced = true;
		}
	}

	public override bool OnBlockActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		if (tileEntityWorkstation == null)
		{
			return false;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(tileEntityWorkstation, null, 0);
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		checkParticles(_world, _blockPos, _newBlockValue);
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		bool flag = GameManager.Instance.HasBlockParticleEffect(_blockPos);
		if (_blockValue.meta != 0 && !flag)
		{
			addParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
			if (CraftingParticleLightIntensity > 0f)
			{
				UpdateVisible(_world, _blockPos);
			}
		}
		else if (_blockValue.meta == 0 && flag)
		{
			removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
		}
	}

	public static bool IsLit(BlockValue _blockValue)
	{
		return _blockValue.meta != 0;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _blockValue.Block.GetLocalizedBlockName() + "\n" + Localization.Get("useWorkstation");
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		switch (_commandName)
		{
		case "open":
			return OnBlockActivated(_world, _blockPos, _blockValue, _player);
		case "take":
			takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
			return true;
		case "extract":
		{
			TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
			for (int i = 0; i < tileEntityWorkstation.Input.Length; i++)
			{
				if (!tileEntityWorkstation.Input[i].IsEmpty())
				{
					ItemStack itemStack = tileEntityWorkstation.Input[i];
					ItemClass itemClass = itemStack.itemValue.ItemClass;
					ItemStack itemStack2 = ((!(itemClass.ReplaceResourceUnit != "")) ? itemStack.Clone() : new ItemStack(ItemClass.GetItem(itemClass.ReplaceResourceUnit).Clone(), itemStack.count));
					itemStack.count = 0;
					if (!itemStack2.IsEmpty())
					{
						_player.PlayerUI.xui.PlayerInventory.AddItem(itemStack2);
					}
					if (!itemStack2.IsEmpty())
					{
						_player.PlayerUI.xui.PlayerInventory.DropItem(itemStack2);
					}
				}
			}
			return true;
		}
		default:
			return false;
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		bool flag2 = false;
		if (tileEntityWorkstation != null)
		{
			flag2 = tileEntityWorkstation.IsPlayerPlaced;
		}
		cmds[1].enabled = flag && flag2 && TakeDelay > 0f;
		cmds[2].enabled = tileEntityWorkstation.InputSlotCount > 0 && !tileEntityWorkstation.InputIsEmpty() && XUiM_Recipes.DisableSmelter;
		return cmds;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool takeItemWithTimerCanTake(Vector3i _blockPos, EntityAlive _player)
	{
		if ((GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityWorkstation).IsEmpty)
		{
			return true;
		}
		GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty"), string.Empty, "ui_denied");
		return false;
	}

	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
		UpdateVisible(_world, _blockPos);
	}

	public void UpdateVisible(WorldBase _world, Vector3i _blockPos)
	{
		if (_world.GetTileEntity(_blockPos) is TileEntityWorkstation te)
		{
			UpdateVisible(te);
		}
	}

	public void UpdateVisible(TileEntityWorkstation _te)
	{
		BlockEntityData blockEntity = _te.GetChunk().GetBlockEntity(_te.ToWorldPos());
		if (blockEntity == null)
		{
			return;
		}
		Transform transform = blockEntity.transform;
		if (!transform)
		{
			return;
		}
		ItemStack[] tools = _te.Tools;
		int num = Utils.FastMin(tools.Length, toolTransformNames.Length);
		for (int i = 0; i < num; i++)
		{
			Transform transform2 = transform.Find(toolTransformNames[i]);
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(!tools[i].IsEmpty());
			}
		}
		Transform transform3 = transform.Find("craft");
		if (!transform3)
		{
			return;
		}
		bool isCrafting = _te.IsCrafting;
		transform3.gameObject.SetActive(isCrafting);
		if (!(CraftingParticleLightIntensity > 0f))
		{
			return;
		}
		Transform blockParticleEffect = GameManager.Instance.GetBlockParticleEffect(_te.ToWorldPos());
		if ((bool)blockParticleEffect)
		{
			Light componentInChildren = blockParticleEffect.GetComponentInChildren<Light>();
			if ((bool)componentInChildren)
			{
				componentInChildren.intensity = (isCrafting ? CraftingParticleLightIntensity : 1f);
			}
		}
		else if (isCrafting)
		{
			_te.SetVisibleChanged();
		}
	}
}
