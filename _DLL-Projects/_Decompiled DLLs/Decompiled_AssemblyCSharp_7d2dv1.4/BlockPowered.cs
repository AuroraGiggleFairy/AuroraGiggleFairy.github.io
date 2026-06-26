using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPowered : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int requiredPower = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string poweredType = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public int RequiredPower => requiredPower;

	public BlockPowered()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RequiredPower"))
		{
			requiredPower = int.Parse(base.Properties.Values["RequiredPower"]);
		}
		else
		{
			requiredPower = 5;
		}
		if (base.Properties.Values.ContainsKey("PoweredType"))
		{
			poweredType = base.Properties.Values["PoweredType"];
		}
		if (base.Properties.Values.ContainsKey("TakeDelay"))
		{
			TakeDelay = StringParsers.ParseFloat(base.Properties.Values["TakeDelay"]);
		}
		else
		{
			TakeDelay = 2f;
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
			if (chunk != null)
			{
				BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
				blockEntityData.bNeedsTemperature = true;
				chunk.AddEntityBlockStub(blockEntityData);
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (!_blockValue.ischild && _world.GetTileEntity(_cIdx, _blockPos) is TileEntityPowered tileEntityPowered)
		{
			tileEntityPowered.BlockTransform = _ebcd.transform;
			GameManager.Instance.StartCoroutine(drawWiresLater(tileEntityPowered));
			if (tileEntityPowered.GetParent().y != -9999 && _world.GetTileEntity(0, tileEntityPowered.GetParent()) is IPowered powered)
			{
				GameManager.Instance.StartCoroutine(drawWiresLater(powered));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawWiresLater(IPowered powered)
	{
		yield return new WaitForSeconds(0.5f);
		powered.DrawWires();
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (_chunk.GetTileEntity(World.toBlock(_blockPos)) is TileEntityPowered tileEntityPowered)
		{
			if (!GameManager.IsDedicatedServer)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionConnectPower)
				{
					(primaryPlayer.inventory.holdingItem.Actions[1] as ItemActionConnectPower).CheckForWireRemoveNeeded(primaryPlayer, _blockPos);
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerManager.Instance.RemovePowerNode(tileEntityPowered.GetPowerItem());
			}
			if (tileEntityPowered.GetParent().y != -9999 && world.GetTileEntity(0, tileEntityPowered.GetParent()) is IPowered powered && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				powered.SendWireData();
			}
			tileEntityPowered.RemoveWires();
		}
		_chunk.RemoveTileEntityAt<TileEntityPowered>((World)world, World.toBlock(_blockPos));
	}

	public virtual TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredBlock(chunk);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) || !(TakeDelay > 0f))
		{
			return "";
		}
		Block block = _blockValue.Block;
		return string.Format(Localization.Get("pickupPrompt"), block.GetLocalizedBlockName());
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		if (_commandName == "take")
		{
			TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
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
		if (world.GetTileEntity(clrIdx, vector3i) is TileEntityPowered tileEntityPowered && tileEntityPowered.IsUserAccessing())
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
}
