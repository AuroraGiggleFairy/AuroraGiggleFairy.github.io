using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockElectricWire : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public float brokenPercentage;

	public BlockElectricWire()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("BrokenPercentage"))
		{
			brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(base.Properties.Values["BrokenPercentage"]));
		}
		else
		{
			brokenPercentage = 0.25f;
		}
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredMeleeTrap(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.ElectricWireRelay
		};
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPoweredMeleeTrap.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!(_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredMeleeTrap))
		{
			TileEntityPowered tileEntityPowered = CreateTileEntity(_chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowered);
		}
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if ((_blockValue.meta & 2) != 0 && 1f - (float)_blockValue.damage / (float)_blockValue.Block.MaxDamage > brokenPercentage)
		{
			if (buffActions == null && base.Properties.Values.ContainsKey("Buff"))
			{
				string[] array = base.Properties.Values["Buff"].Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					buffActions.Add(array[i]);
				}
			}
			if (buffActions != null && _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPoweredMeleeTrap { IsPowered: not false } tileEntityPoweredMeleeTrap && _world.GetEntity(_entityIdThatDamaged) is EntityAlive entityAlive)
			{
				ItemAction itemAction = entityAlive.inventory.holdingItemData.item.Actions[0];
				if ((object)entityAlive != null && (!(itemAction is ItemActionRanged) || (itemAction is ItemActionRanged itemActionRanged && (itemActionRanged.Hitmask & 0x80) != 0)))
				{
					for (int j = 0; j < buffActions.Count; j++)
					{
						entityAlive.Buffs.AddBuff(buffActions[j], tileEntityPoweredMeleeTrap.OwnerEntityID);
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}
}
