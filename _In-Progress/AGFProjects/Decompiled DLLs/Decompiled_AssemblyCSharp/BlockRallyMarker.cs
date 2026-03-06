using UnityEngine.Scripting;

[Preserve]
public class BlockRallyMarker : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: false)
	};

	public BlockRallyMarker()
	{
		StabilityIgnore = true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		Quest quest = ((EntityPlayerLocal)_entityFocusing).QuestJournal.HasQuestAtRallyPosition(_blockPos.ToVector3());
		if (quest != null && !quest.RallyMarkerActivated && ((EntityPlayerLocal)_entityFocusing).QuestJournal.ActiveQuest == null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			_blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("questRallyActivate"), arg, quest.QuestClass.Name);
		}
		return string.Empty;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "activate")
		{
			Quest quest = _player.QuestJournal.HasQuestAtRallyPosition(_blockPos.ToVector3());
			if (quest != null && !quest.RallyMarkerActivated && _player.QuestJournal.ActiveQuest == null)
			{
				QuestEventManager.Current.HandleRallyMarkerActivate(_player, _blockPos, _blockValue);
				return true;
			}
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		cmds[0].enabled = true;
		return cmds;
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
		_world.IsEditor();
	}
}
