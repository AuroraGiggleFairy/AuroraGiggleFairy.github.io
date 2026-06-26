using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpeaker : BlockPowered
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string playSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float soundDelay = 1f;

	public BlockSpeaker()
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
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		byte b = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
			try
			{
				if (isOn)
				{
					Manager.BroadcastPlay(_blockPos.ToVector3(), playSound);
				}
				else
				{
					Manager.BroadcastStop(_blockPos.ToVector3(), playSound);
				}
			}
			catch (Exception)
			{
			}
		}
		return true;
	}

	public override void OnBlockUnloaded(WorldBase world, int clrIdx, Vector3i blockPos, BlockValue blockValue)
	{
		base.OnBlockUnloaded(world, clrIdx, blockPos, blockValue);
		Manager.Stop(blockPos.ToVector3(), playSound);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		Manager.Stop(_blockPos.ToVector3(), playSound);
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if ((TileEntityPoweredBlock)_world.GetTileEntity(_clrIdx, _blockPos) != null)
		{
			if ((_blockValue.meta & 2) != 0)
			{
				Manager.Play(_blockPos.ToVector3(), playSound);
			}
			else
			{
				Manager.Stop(_blockPos.ToVector3(), playSound);
			}
		}
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredBlock(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.Consumer
		};
	}
}
