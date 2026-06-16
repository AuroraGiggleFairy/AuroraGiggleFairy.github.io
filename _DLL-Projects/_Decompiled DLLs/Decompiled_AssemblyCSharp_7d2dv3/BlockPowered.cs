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

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache != null)
		{
			Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
			if (chunk != null)
			{
				BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
				blockEntityData.bNeedsTemperature = true;
				chunk.AddEntityBlockStub(blockEntityData);
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		if (!_blockValue.ischild && _world.GetTileEntity(_blockPos) is TileEntityPowered tileEntityPowered)
		{
			tileEntityPowered.BlockTransform = _ebcd.transform;
			GameManager.Instance.StartCoroutine(drawWiresLater(tileEntityPowered));
			if (tileEntityPowered.GetParent().y != -9999 && _world.GetTileEntity(tileEntityPowered.GetParent()) is IPowered powered)
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
			if (tileEntityPowered.GetParent().y != -9999 && world.GetTileEntity(tileEntityPowered.GetParent()) is IPowered powered && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
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

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) || !(TakeDelay > 0f))
		{
			return "";
		}
		Block block = _blockValue.Block;
		return string.Format(Localization.Get("pickupPrompt"), block.GetLocalizedBlockName());
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		if (_commandName == "take")
		{
			takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
			return true;
		}
		return false;
	}
}
