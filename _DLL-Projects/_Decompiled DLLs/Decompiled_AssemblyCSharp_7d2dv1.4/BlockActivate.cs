using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockActivate : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string activateSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockChangeTo = BlockValue.Air;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useChangeTo;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: true),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockChangeTo = "BlockChangeTo";

	public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("ActivateSound"))
		{
			activateSound = base.Properties.Values["ActivateSound"];
		}
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(PropBlockChangeTo))
		{
			blockChangeTo = Block.GetBlockValue(base.Properties.Values[PropBlockChangeTo]);
			useChangeTo = true;
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _clrIdx, _blockPos, _showTriggers: true, _showTriggeredBy: false);
			}
		}
		else if (!_world.IsEditor())
		{
			HandleTrigger(_player, (World)_world, _clrIdx, _blockPos, _blockValue);
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, activateSound);
			if (useChangeTo)
			{
				blockChangeTo.rotation = _blockValue.rotation;
			}
			return true;
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
		cmds[0].enabled = true;
		cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}
}
