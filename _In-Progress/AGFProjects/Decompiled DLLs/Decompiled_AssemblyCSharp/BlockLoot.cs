using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class BlockLoot : Block
{
	public struct AlternateLootEntry
	{
		public FastTags<TagGroup.Global> tag;

		public string lootEntry;
	}

	public static string PropLootList = "LootList";

	public static string PropAlternateLootList = "AlternateLootList";

	public static string PropLootStageMod = "LootStageMod";

	public static string PropLootStageBonus = "LootStageBonus";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lootList;

	public float LootStageMod;

	public float LootStageBonus;

	public List<AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("Search", "search", _enabled: true)
	};

	public BlockLoot()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(PropLootList))
		{
			throw new Exception("Block with name " + GetBlockName() + " doesnt have a loot list");
		}
		lootList = base.Properties.Values[PropLootList];
		base.Properties.ParseFloat(PropLootStageMod, ref LootStageMod);
		base.Properties.ParseFloat(PropLootStageBonus, ref LootStageBonus);
		for (int i = 1; i < 99; i++)
		{
			string text = PropAlternateLootList + i;
			if (!base.Properties.Values.ContainsKey(text))
			{
				break;
			}
			string text2 = "";
			if (base.Properties.Params1.ContainsKey(text))
			{
				text2 = base.Properties.Params1[text];
			}
			if (text2 != "")
			{
				FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse(text2);
				if (AlternateLootList == null)
				{
					AlternateLootList = new List<AlternateLootEntry>();
				}
				AlternateLootList.Add(new AlternateLootEntry
				{
					tag = tag,
					lootEntry = base.Properties.Values[text]
				});
			}
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityLootContainer tileEntityLootContainer && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityLootContainer.bPlayerStorage = true;
			tileEntityLootContainer.worldTimeTouched = _world.GetWorldTime();
			tileEntityLootContainer.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityLootContainer tileEntityLootContainer))
		{
			return string.Empty;
		}
		string arg = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (!tileEntityLootContainer.bTouched)
		{
			return string.Format(Localization.Get("lootTooltipNew"), arg2, arg);
		}
		if (tileEntityLootContainer.IsEmpty())
		{
			return string.Format(Localization.Get("lootTooltipEmpty"), arg2, arg);
		}
		return string.Format(Localization.Get("lootTooltipTouched"), arg2, arg);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild && LootContainer.GetLootContainer(lootList) != null)
		{
			addTileEntity(world, _chunk, _blockPos, _blockValue);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityLootContainer tileEntityLootContainer)
		{
			tileEntityLootContainer.OnDestroy();
		}
		removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityLootContainer tileEntityLootContainer = new TileEntityLootContainer(_chunk);
		tileEntityLootContainer.localChunkPos = World.toBlock(_blockPos);
		tileEntityLootContainer.lootListName = lootList;
		tileEntityLootContainer.SetContainerSize(LootContainer.GetLootContainer(lootList).size);
		_chunk.AddTileEntity(tileEntityLootContainer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityLootContainer>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityLootContainer tileEntityLootContainer)
		{
			tileEntityLootContainer.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_LootWindowGroup.CloseIfOpenAtPos(_blockPos);
		}
		return DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityLootContainer tileEntityLootContainer))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityLootContainer.ToWorldPos();
		tileEntityLootContainer.bWasTouched = tileEntityLootContainer.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityLootContainer.entityId, _player.entityId);
		return true;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "Search")
		{
			return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return cmds;
	}
}
