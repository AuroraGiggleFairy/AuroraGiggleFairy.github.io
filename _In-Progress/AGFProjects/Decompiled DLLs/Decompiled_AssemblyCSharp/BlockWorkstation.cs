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
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("open", "campfire", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
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
		base.Properties.ParseString("Workstation.ToolNames", ref optionalValue);
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

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		if (tileEntityWorkstation == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityWorkstation.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityWorkstation.entityId, _player.entityId);
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		checkParticles(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		bool flag = GameManager.Instance.HasBlockParticleEffect(_blockPos);
		if (_blockValue.meta != 0 && !flag)
		{
			addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
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

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useWorkstation");
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "open"))
		{
			if (_commandName == "take")
			{
				TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
				return true;
			}
			return false;
		}
		return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		bool flag2 = false;
		if (tileEntityWorkstation != null)
		{
			flag2 = tileEntityWorkstation.IsPlayerPlaced;
		}
		cmds[1].enabled = flag && flag2 && TakeDelay > 0f;
		return cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (!(GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityWorkstation).IsEmpty)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", _bModal: true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[4] { _cIdx, _blockValue, _blockPos, _player };
		timerEventData.Event += EventData_Event;
		childByType.SetTimer(TakeDelay, timerEventData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = obj[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), string.Empty, "ui_denied");
			return;
		}
		if ((world.GetTileEntity(vector3i) as TileEntityWorkstation).IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uIForPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
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
