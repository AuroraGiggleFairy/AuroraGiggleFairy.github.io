using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class BlockTriggerDowngrade : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public override void LateInit()
	{
		base.LateInit();
		if (!DowngradeBlock.isair && DowngradeBlock.Block is BlockHazard blockHazard)
		{
			DowngradeBlock = blockHazard.SetHazardState(DowngradeBlock, isOn: true);
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsEditor())
		{
			return "";
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "trigger")
		{
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _clrIdx, _blockPos, _showTriggers: false, _showTriggeredBy: true);
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		cmds[0].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		HandleDowngrade(_world, _cIdx, _blockPos, _blockValue, _blockChanges);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleDowngrade(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null || DowngradeBlock.isair)
		{
			return;
		}
		SpawnDowngradeFX(_world, _blockValue, _blockPos, _blockValue.Block.tintColor, -1);
		BlockValue downgradeBlock = DowngradeBlock;
		downgradeBlock = BlockPlaceholderMap.Instance.Replace(downgradeBlock, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
		downgradeBlock.rotation = _blockValue.rotation;
		if (!downgradeBlock.Block.shape.IsTerrain())
		{
			_blockChanges.Add(new BlockChangeInfo(_cIdx, _blockPos, downgradeBlock));
			if (chunkCluster.GetTextureFull(_blockPos) == 0L)
			{
				return;
			}
			if (RemovePaintOnDowngrade == null)
			{
				GameManager.Instance.SetBlockTextureServer(_blockPos, BlockFace.None, 0, -1);
				return;
			}
			for (int i = 0; i < RemovePaintOnDowngrade.Count; i++)
			{
				GameManager.Instance.SetBlockTextureServer(_blockPos, RemovePaintOnDowngrade[i], 0, -1);
			}
		}
		else
		{
			_blockChanges.Add(new BlockChangeInfo(_cIdx, _blockPos, downgradeBlock, downgradeBlock.Block.Density));
		}
	}
}
